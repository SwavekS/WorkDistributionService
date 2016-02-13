using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using WorkDistributionApp.Models;
using WorkDistributionApp.DAL;

namespace WorkDistributionApp.Controllers
{
    [Authorize(Roles="Admin, CanEdit")]
    public class CommentController : Controller
    {
        private WorkObjectContext db = new WorkObjectContext();

        // GET: /Comment/Create/4
        public ActionResult Create(int? id)
        {
            string user = User.Identity.Name;
            if (hasActiveStandardSubscription(user) || hasActiveUltimateSubscription(user))
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                Comment comment = new Comment 
                {
                    WorkObjectID = id.GetValueOrDefault()
                };

                WorkObject workobject = db.WorkObjects.Find(id);
                // improves mvc security
                if ((workobject.TeamQueue == TeamQueueType.Claims && User.IsInRole("claims")) ||
                     (workobject.TeamQueue == TeamQueueType.Servicing && User.IsInRole("servicing")) ||
                     (workobject.TeamQueue == TeamQueueType.New_Business && User.IsInRole("new_business")))
                {
                    if ((workobject.isLocked == true && workobject.LockedBy == User.Identity.Name) || workobject.isLocked == false)
                    {
                        return View(comment);
                    }
                    else
                    {
                        return RedirectToAction("Details", "WorkObject", new { id = comment.WorkObjectID });
                    }
                }
                else
                {
                    return RedirectToAction("Index", "WorkObject");
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

        }

        // POST: /Comment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "WorkObjectID,Content")] Comment comment)
        {
            // converts time to Greenwich Mean Time
            DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

            string user = User.Identity.Name;
            if (hasActiveStandardSubscription(user) || hasActiveUltimateSubscription(user))
            {
                if (ModelState.IsValid)
                {
                    comment.Timestamp = currentTime;
                    comment.User = User.Identity.Name;
                    db.Comments.Add(comment);
                    db.SaveChanges();
                    return RedirectToAction("Details", "WorkObject", new { id = comment.WorkObjectID });
                }
                return View(comment);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        bool hasActiveStandardSubscription(string name)
        {
            // check if user has an active subscription
            var stripeCustomers = (from s in db.StripeCustomers
                                   where (s.CustomerName == User.Identity.Name)
                                   select s).SingleOrDefault();
            if (stripeCustomers != null)
            {
                if (stripeCustomers.HasSubscription == true && stripeCustomers.SubscriptionType == "standard")
                {
                    return true;
                }
            }
            return false;
        }

        bool hasActiveUltimateSubscription(string name)
        {
            // check if user has an active subscription
            var stripeCustomers = (from s in db.StripeCustomers
                                   where (s.CustomerName == User.Identity.Name)
                                   select s).SingleOrDefault();
            if (stripeCustomers != null)
            {
                if (stripeCustomers.HasSubscription == true && stripeCustomers.SubscriptionType == "ultimate")
                {
                    return true;
                }
            }
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
