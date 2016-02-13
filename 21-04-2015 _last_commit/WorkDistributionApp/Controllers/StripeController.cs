using Stripe;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using WorkDistributionApp.DAL;
using WorkDistributionApp.Models;
using System.Linq;


namespace WorkDistributionApp.Controllers
{
    public class StripeController : Controller
    {
        // converts time to Greenwich Mean Time
        DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

        private WorkObjectContext db = new WorkObjectContext();

        // GET: Stripe
        public ActionResult Index()
        {
            return View();
        }

        //GET: Stripe/GetToken  :sing up to standard or ultimate
        public ActionResult GetToken()
        {
            // allow to subscribe only registered and logged in users
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            // check if a user exists in a database
            var stripeCustomer = getCustomer();
            if (stripeCustomer != null)
            {
                if (stripeCustomer.HasSubscription == true && stripeCustomer.SubscriptionType == "ultimate")
                { // downgrade plan 
                    return View("Downgrade");
                }
                if (stripeCustomer.HasSubscription == true)
                {
                    return RedirectToAction("Index", "Home");
                }
                else if (stripeCustomer.HasSubscription == false && stripeCustomer.SubscriptionType == "standard")
                {
                    if (isFreeTrailAllowed()) // check if standard plan is allowed
                    {
                        return View(new StripeModel());
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    return View(new StripeModel());
                }
            }
            else
            {
                return View(new StripeModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> GetToken(StripeModel model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var stripeCustomer = getCustomer();
            if(stripeCustomer == null)
            {
                await CreateCustomer(model);
                return RedirectToAction("Index", "Home");
            }
            else if(stripeCustomer.HasSubscription == false && stripeCustomer.SubscriptionType=="standard")
            {
                await CreateCustomer(model);
                return RedirectToAction("Index", "Home");
            }
       
            return RedirectToAction("Index", "Home");
        }

        // GET: //Stripe/GetToken2      :sing up to ultimate plan 
        public ActionResult GetToken2()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            // check if a user exist in database
            var stripeCustomer = getCustomer();
            if (stripeCustomer != null)
            {
                if (stripeCustomer.HasSubscription == true && stripeCustomer.SubscriptionType == "ultimate")
                { //exit
                    return RedirectToAction("Index", "Home");
                }
                else if (stripeCustomer.HasSubscription == false && stripeCustomer.SubscriptionType == "standard")
                {
                    return View(new StripeModel());
                }                
                else // upgrade plan to ultimate
                {
                    return View("Upgrade");
                }
            }
            else
            {
                return View(new StripeModel());
            }
        }

        // GET: /Stripe/Upgrade
        public ActionResult Upgrade()
        {
            return View();
        }

        // GET: /Stripe/Downgrade
        public ActionResult Downgrade()
        {
            return View();
        }

        private async Task CreateCustomer(StripeModel model)
        {
            await Task.Run(() =>
            {
                // create customer based on validated token from stripe.js 
                var myCustomer = new StripeCustomerCreateOptions();

                // assign token to a credit card option for a user
                myCustomer.Card = new StripeCreditCardOptions()
                {
                    TokenId = model.Token
                };

                myCustomer.Email = User.Identity.Name;
                myCustomer.Description = User.Identity.Name;
                myCustomer.PlanId = model.SubscriptionType.ToString();
                myCustomer.Quantity = 1;
                
                // create customer in stripe service
                var customerService = new StripeCustomerService();
                StripeCustomer stripeCustomer = customerService.Create(myCustomer);

                // get subscription Id from created user
                var subscriptionID = stripeCustomer.StripeSubscriptionList.StripeSubscriptions.FirstOrDefault().Id;              
                       
             // save credit card optional details 
                StripeCustomerService customerServic = new StripeCustomerService();
                stripeCustomer = customerService.Get(stripeCustomer.Id);
                var cardId = stripeCustomer.StripeDefaultCardId; // get card id
                var myCard = new StripeCardUpdateOptions();

                myCard.Name = model.CardHolderName;
                myCard.AddressLine1 = model.AddressLine1;
                myCard.AddressLine2 = model.AddressLine2;
                myCard.AddressCity = model.AddressCity;
                myCard.AddressZip = model.AddressPostcode;
                myCard.AddressCountry = model.AddressCountry;

                var cardService = new StripeCardService();
                StripeCard stripeCard = cardService.Update(stripeCustomer.Id, cardId, myCard);
            //........................
                
                // record customer in database
                var cust = getCustomer();
                if(cust == null) // new users
                {
                    // get values to create a new record in StripeCustomer table
                    StripeCustomers customer = new StripeCustomers();
                
                    customer.CustomerName = User.Identity.Name;
                    customer.StripeCustomerID = stripeCustomer.Id;
                    customer.StripeSubscriptionID = subscriptionID;
                    customer.SubscriptionType = model.SubscriptionType.ToString();
                    customer.HasSubscription = true;
                    customer.Interval = "Monthly";
                    customer.StartDate = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");
                    if (model.SubscriptionType.ToString() == "standard")
                    {
                        customer.TrialValidUntil = currentTime.AddDays(30);
                    }
                    db.StripeCustomers.Add(customer);
                    }
                    else // user with db records
                    {
                        StripeCustomers newRecord = new StripeCustomers();

                        // take the data from current user
                        newRecord.StripeCustomerID = stripeCustomer.Id;
                        newRecord.CustomerName = User.Identity.Name;
                        newRecord.StripeSubscriptionID = subscriptionID;
                        newRecord.SubscriptionType = model.SubscriptionType.ToString();
                        newRecord.HasSubscription = true;
                        newRecord.Interval = "Monthly";
                        newRecord.StartDate = currentTime;
                        if (model.SubscriptionType.ToString() == "standard")
                        {
                            newRecord.TrialValidUntil = currentTime.AddDays(30);
                        }
                        db.StripeCustomers.Add(newRecord);

                        // delete customer's old record in database
                        db.StripeCustomers.Remove(cust);
                    }
                db.SaveChanges();
            });
        }

        public async Task<ActionResult> UpgradePlan()
        {
            await Task.Run(() =>
            {
                var cust = getCustomer();

                if(cust != null) // upgrade
                {
                    // retrieve customer
                    var customerService = new StripeCustomerService();
                    StripeCustomer currentCustomer = customerService.Get(cust.StripeCustomerID);

                    // change plan
                    StripeSubscriptionService subService = new StripeSubscriptionService();
                    StripeSubscriptionUpdateOptions updateOptions = new StripeSubscriptionUpdateOptions();
                    updateOptions.PlanId = "ultimate";

                    var myUpdateSuscription = subService.Update(currentCustomer.Id, cust.StripeSubscriptionID, updateOptions);

                    // record changes in db
                    StripeCustomers newRecord = new StripeCustomers();

                    newRecord.StripeCustomerID = myUpdateSuscription.CustomerId;
                    newRecord.CustomerName = User.Identity.Name;
                    newRecord.StripeSubscriptionID = cust.StripeSubscriptionID;
                    newRecord.SubscriptionType = "ultimate";
                    newRecord.HasSubscription = true;
                    newRecord.StartDate = currentTime;
                    newRecord.Interval = "Monthly";

                    db.StripeCustomers.Add(newRecord);                                     
                    db.StripeCustomers.Remove(cust);
                    db.SaveChanges();                   
                }   
            });
            return RedirectToAction("Index", "Home");
        }

        public async Task<ActionResult> DowngradePlan()
        {
            await Task.Run(() =>
            {
                var cust = getCustomer();

                if (cust != null) // downgrade
                {
                    // retrieve customer
                    var customerService = new StripeCustomerService();
                    StripeCustomer currentCustomer = customerService.Get(cust.StripeCustomerID);

                    // change plan
                    StripeSubscriptionService subService = new StripeSubscriptionService();
                    StripeSubscriptionUpdateOptions updateOptions = new StripeSubscriptionUpdateOptions();
                    updateOptions.PlanId = "standard";
                    updateOptions.TrialEnd = currentTime; // no free days allowed when downgrading

                    var myUpdateSuscription = subService.Update(currentCustomer.Id, cust.StripeSubscriptionID, updateOptions);

                    // record changes in db
                    StripeCustomers newRecord = new StripeCustomers();

                    newRecord.StripeCustomerID = myUpdateSuscription.CustomerId;
                    newRecord.CustomerName = User.Identity.Name;
                    newRecord.StripeSubscriptionID = cust.StripeSubscriptionID;
                    newRecord.SubscriptionType = "standard";
                    newRecord.TrialValidUntil = currentTime;
                    newRecord.HasSubscription = true;
                    newRecord.StartDate = currentTime;
                    newRecord.Interval = "Monthly";

                    db.StripeCustomers.Add(newRecord);
                    db.StripeCustomers.Remove(cust);
                    db.SaveChanges();
                }
            });
            return RedirectToAction("Index", "Home");
        }

        public async Task<ActionResult> CancelSubscription()
        {          
            await Task.Run(() =>
            {
                var hasSubscription = getCustomer().HasSubscription;
                if(hasSubscription == true)
                {
                    var uStripeCustomer = getCustomer();
                    var stripeCustomerID = uStripeCustomer.StripeCustomerID;
                    var subscriptionID = uStripeCustomer.StripeSubscriptionID;                    
                  
                    if (uStripeCustomer.SubscriptionType == "ultimate")
                    {
                        // cancel subscription in stripe.com
                        var subscriptionService = new StripeSubscriptionService();
                        subscriptionService.Cancel(stripeCustomerID, subscriptionID);

                        // delete stripe customer as well
                        var customerService = new StripeCustomerService();
                        StripeCustomer customerToDelete = customerService.Get(stripeCustomerID);
                        customerService.Delete(customerToDelete.Id);

                        db.StripeCustomers.Remove(uStripeCustomer); //db
                    }
                    else if (uStripeCustomer.SubscriptionType == "standard")
                    {
                        // cancel subscription in stripe.com
                        var subscriptionService = new StripeSubscriptionService();
                        subscriptionService.Cancel(stripeCustomerID, subscriptionID);

                        // delete stripe customer as well
                        var customerService = new StripeCustomerService();
                        StripeCustomer customerToDelete = customerService.Get(stripeCustomerID);
                        customerService.Delete(customerToDelete.Id);

                        uStripeCustomer.HasSubscription = false; //db
                    }
                    db.SaveChanges();                  
                }
            });
         
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Delete()
        {
            if(hasSubscription() == true)
                ViewData["hasSubscription"] = true;
            else
                ViewData["hasSubscription"] = false;

           return View();
        }

        private bool hasSubscription()
        {
            return (from s in db.StripeCustomers
                    where (s.CustomerName == User.Identity.Name)
                    select s.HasSubscription).FirstOrDefault();
        }

        // check if 3 months expired since the last free trail
        private bool isFreeTrailAllowed()
        {
            var stripeCustomer = (from s in db.StripeCustomers
                                   where (s.CustomerName == User.Identity.Name &&
                                          s.SubscriptionType == "standard")
                                   select s).SingleOrDefault();
            if(stripeCustomer != null)
            {               
                DateTime dateToCheck = currentTime;
                if (stripeCustomer.TrialValidUntil <= dateToCheck.AddMonths(3))
                {
                    return true;
                }
            }
            return false;
        }

        StripeCustomers getCustomer()
        {
            var sd = (from s in db.StripeCustomers
                      where (s.CustomerName == User.Identity.Name)
                      select s).FirstOrDefault();
            return sd;
        }

    }
}