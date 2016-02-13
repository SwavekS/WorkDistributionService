using System;
using System.Linq;
using System.Web.Mvc;
using WorkDistributionApp.DAL;
using WorkDistributionApp.Models;

namespace WorkDistributionApp.Controllers
{
    public class HomeController : Controller
    {     
        private WorkObjectContext db = new WorkObjectContext();
        public ActionResult Index() // no subscription
        {
            ViewData["ContactAdmin"] = false;

            //subscription information
            var stripeCustomer = getCustomer();

            if (stripeCustomer != null)
            {
                if (stripeCustomer.HasSubscription == true && stripeCustomer.SubscriptionType == "standard")
                { // standard-active subscription
                    return RedirectToAction("Index3");
                }
                else if (stripeCustomer.HasSubscription == true && stripeCustomer.SubscriptionType == "ultimate")
                { // ultimate subscription
                    return RedirectToAction("Index2");
                }
                else if (stripeCustomer.HasSubscription == false && stripeCustomer.SubscriptionType == "standard")
                { // standard-not active subscription
                    return RedirectToAction("Index4");
                }
            }

            ViewData["ContactAdmin"] = isAdminAssistRequired();
            return View();        
        }

        public ActionResult Index2() // ultimate subscription
        {
            ViewData["DowngradeButton"] = false;
            ViewData["ContactAdmin"] = false;

            var stripeCustomer = getCustomer();
            if (stripeCustomer.HasSubscription == true && stripeCustomer.SubscriptionType == "ultimate")
            {
                ViewBag.SubscriptionStatus = "Active";
                ViewBag.SubscriptionType = stripeCustomer.SubscriptionType;
                ViewBag.CustomerId = stripeCustomer.StripeCustomerID;
                ViewBag.SubscriptionId = stripeCustomer.StripeSubscriptionID;
                ViewBag.BillingInterval = stripeCustomer.Interval;
                ViewBag.StartDate = stripeCustomer.StartDate;

                ViewData["DowngradeButton"] = true;
                ViewData["ContactAdmin"] = isAdminAssistRequired();
                return View();
            }
            else
            {
                return RedirectToAction("Index");
            }            
        }
        public ActionResult Index3() // standard-active subscription
        {
            // converts time to Greenwich Mean Time
            DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

            ViewData["UpgradeButton"] = false;
            ViewData["ContactAdmin"] = false;
            
            var stripeCustomer = getCustomer();
            if(stripeCustomer.HasSubscription == true && stripeCustomer.SubscriptionType == "standard")
            {
                ViewBag.SubscriptionStatus = "Active";
                ViewBag.SubscriptionType = stripeCustomer.SubscriptionType;
                ViewBag.CustomerId = stripeCustomer.StripeCustomerID;
                ViewBag.BillingInterval = stripeCustomer.Interval;
                ViewBag.StartDate = stripeCustomer.StartDate;

                // calculate free time left
                DateTime endDate = stripeCustomer.TrialValidUntil.GetValueOrDefault();
                DateTime startDate = currentTime;
                TimeSpan timeLeft = endDate.Subtract(startDate);

                // check if plan is downgraded
                if(endDate <= startDate)
                {
                    ViewBag.FreeDaysLeft = "Plan Downgraded";
                }
                else
                {
                    ViewBag.FreeDaysLeft = Helper.ToReadableString(timeLeft);
                }

                ViewData["UpgradeButton"] = true; // display 'upgrade' insted of 'sign up'
                ViewData["ContactAdmin"] = isAdminAssistRequired(); 
                return View();
            }
            else
            {
                return RedirectToAction("Index");
            }          
        }

        public ActionResult Index4() // standard-not active subscription
        {
            // converts time to Greenwich Mean Time
            DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

            ViewData["ContactAdmin"] = false;
            ViewData["LockedButton"] = false;

            var stripeCustomer = getCustomer();
            if (stripeCustomer.HasSubscription == false && stripeCustomer.SubscriptionType == "standard")
            {
                ViewBag.SubscriptionStatus = "Unsubscribed";
                ViewBag.SubscriptionType = stripeCustomer.SubscriptionType;
                ViewBag.CustomerId = stripeCustomer.StripeCustomerID;

                // add 3 months to the time when a new free subscription will be allowe
                // prevent users from using only free days
                // logic is in StripeController isFreeTrailAllowed() method

                DateTime endDate = stripeCustomer.TrialValidUntil.GetValueOrDefault();
                DateTime startDate = currentTime;
                endDate = endDate.AddMonths(3);
                TimeSpan timeLeft = endDate.Subtract(startDate);
                ViewBag.NewFreeSubscriptionAllowed = Helper.ToReadableStringRough(timeLeft);

                ViewData["ContactAdmin"] = isAdminAssistRequired();
                ViewData["LockedButton"] = isLocked();

                return View();
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        private bool isLocked()
        {
            var customer = getCustomer();
            if(customer != null && customer.HasSubscription == false && customer.SubscriptionType == "standard")
            {
                return true;
            }
            return false;         
        }
        

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        private StripeCustomers getCustomer()
        {
            var stripeCustomer = (from s in db.StripeCustomers
                                  where (s.CustomerName == User.Identity.Name)
                                  select s).FirstOrDefault();
            return stripeCustomer;
        }

        private bool isAdminAssistRequired()
        {
            if((User.IsInRole("Admin") == false && User.IsInRole("CanEdit") == false))
            {
                return true;
            }
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }

    // helper class to display time remaining to the end of a trial period
    public static class Helper
    {
        public static string ToReadableString(this TimeSpan span)
        {
            string formatted = string.Format("{0}{1}{2}{3}",
                span.Duration().Days > 0 ? string.Format("{0:0} day{1}, ", span.Days, span.Days == 1 ? String.Empty : "s") : string.Empty,
                span.Duration().Hours > 0 ? string.Format("{0:0} hour{1}, ", span.Hours, span.Hours == 1 ? String.Empty : "s") : string.Empty,
                span.Duration().Minutes > 0 ? string.Format("{0:0} minute{1}, ", span.Minutes, span.Minutes == 1 ? String.Empty : "s") : string.Empty,
                span.Duration().Seconds > 0 ? string.Format("{0:0} second{1}", span.Seconds, span.Seconds == 1 ? String.Empty : "s") : string.Empty);

            if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

            if (string.IsNullOrEmpty(formatted)) formatted = "0 days";

            return formatted;
        }

        public static string ToReadableStringRough(this TimeSpan span)
        {
            string formatted = string.Format("{0}{1}",
                span.Duration().Days > 0 ? string.Format("{0:0} day{1}, ", span.Days, span.Days == 1 ? String.Empty : "s") : string.Empty,
                span.Duration().Hours > 0 ? string.Format("{0:0} hour{1} ", span.Hours, span.Hours == 1 ? String.Empty : "s") : string.Empty);
            return formatted;
        }
    }


}
