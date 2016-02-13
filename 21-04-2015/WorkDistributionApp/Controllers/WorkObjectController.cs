using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using WorkDistributionApp.Models;
using WorkDistributionApp.DAL;

namespace WorkDistributionApp.Controllers
{  
    [Authorize(Roles="Admin, CanEdit")]
    public class WorkObjectController : Controller
    {
       
        private WorkObjectContext db = new WorkObjectContext();


        // GET: /WorkObject/Index
        public ActionResult Index(string sortOrder, string nameSearch, string policySearch,
                                    string teamQueueSearch) 
        {
            // subscription check
            string user = User.Identity.Name;
            if (hasActiveStandardSubscription(user) || hasActiveUltimateSubscription(user))
            {
                // check priority
                checkPriority();

                ViewBag.PrioritySortParm = String.IsNullOrEmpty(sortOrder) ? "Priority" : "";
                ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
                ViewBag.StatusSortParm = sortOrder == "Status" ? "status_desc" : "Status";
                ViewBag.PolicyNoSortParm = sortOrder == "PolicyNo" ? "policyNo_desc" : "PolicyNo";
                ViewBag.TeamQueueSortParm = sortOrder == "TeamQueue" ? "teamQueue_desc" : "TeamQueue";
                ViewBag.NameSortParm = sortOrder == "Name" ? "name_desc" : "Name";

                var workObjects = from s in db.WorkObjects
                                  where s.Status != StatusType.Processed
                                  where s.Status != StatusType.End
                                  select s;

                // assign work objects to appriopriate queue
                if (User.IsInRole("servicing") && User.IsInRole("new_business") && User.IsInRole("claims"))
                {
                    workObjects = from s in db.WorkObjects
                                  where s.Status != StatusType.Processed
                                  where s.Status != StatusType.End
                                  select s;
                }
                else if (User.IsInRole("claims"))
                {
                    workObjects = from s in db.WorkObjects
                                  where s.TeamQueue == TeamQueueType.Claims
                                  where s.Status != StatusType.Processed
                                  where s.Status != StatusType.End
                                  select s;
                }
                else if (User.IsInRole("servicing"))
                {
                    workObjects = from s in db.WorkObjects
                                  where s.TeamQueue == TeamQueueType.Servicing
                                  where s.Status != StatusType.Processed
                                  where s.Status != StatusType.End
                                  select s;
                }
                else if (User.IsInRole("new_business"))
                {
                    workObjects = from s in db.WorkObjects
                                  where s.TeamQueue == TeamQueueType.New_Business
                                  where s.Status != StatusType.Processed
                                  where s.Status != StatusType.End
                                  select s;
                }


                if (!String.IsNullOrEmpty(nameSearch))
                {
                    workObjects = workObjects.Where(s => s.ClientSurname.Contains(nameSearch)
                                                    || s.ClientName.Contains(nameSearch));
                }
                if (!String.IsNullOrEmpty(policySearch))
                {
                    workObjects = workObjects.Where(s => s.PolicyNo.Contains(policySearch));
                }

                switch (sortOrder)
                {
                    case "Priority":
                        workObjects = workObjects.OrderBy(s => s.Priority);
                        break;
                    case "Date":
                        workObjects = workObjects.OrderBy(s => s.StartDate);
                        break;
                    case "date_desc":
                        workObjects = workObjects.OrderByDescending(s => s.StartDate);
                        break;
                    case "Status":
                        workObjects = workObjects.OrderBy(s => s.Status);
                        break;
                    case "status_desc":
                        workObjects = workObjects.OrderByDescending(s => s.Status);
                        break;
                    case "PolicyNo":
                        workObjects = workObjects.OrderBy(s => s.PolicyNo);
                        break;
                    case "policyNo_desc":
                        workObjects = workObjects.OrderByDescending(s => s.PolicyNo);
                        break;
                    case "TeamQueue":
                        workObjects = workObjects.OrderBy(s => s.TeamQueue);
                        break;
                    case "teamQueue_desc":
                        workObjects = workObjects.OrderByDescending(s => s.TeamQueue);
                        break;
                    case "Name":
                        workObjects = workObjects.OrderBy(s => s.ClientSurname);
                        break;
                    case "name_desc":
                        workObjects = workObjects.OrderByDescending(s => s.ClientSurname);
                        break;
                    default: // highest priority is at the top of the table
                        workObjects = workObjects.OrderByDescending(s => s.Priority);
                        break;
                }
                return View(workObjects.ToList());
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: /WorkObject/History
        public ActionResult History(string sortOrder, string nameSearch, string policySearch,
                                    string teamQueueSearch)
        {
            string user = User.Identity.Name;
            if (hasActiveStandardSubscription(user) || hasActiveUltimateSubscription(user))
            {
                ViewBag.PrioritySortParm = String.IsNullOrEmpty(sortOrder) ? "Priority" : "";
                ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
                ViewBag.StatusSortParm = sortOrder == "Status" ? "status_desc" : "Status";
                ViewBag.PolicyNoSortParm = sortOrder == "PolicyNo" ? "policyNo_desc" : "PolicyNo";
                ViewBag.TeamQueueSortParm = sortOrder == "TeamQueue" ? "teamQueue_desc" : "TeamQueue";
                ViewBag.NameSortParm = sortOrder == "Name" ? "name_desc" : "Name";

                var workObjects = from s in db.WorkObjects
                                  where (s.Status == StatusType.Processed || s.Status == StatusType.End)
                                  select s;

                // assigning work object to specific team queue
                if (User.IsInRole("servicing") && User.IsInRole("new_business") && User.IsInRole("claims"))
                {
                    workObjects = from s in db.WorkObjects
                                  where (s.Status == StatusType.Processed || s.Status == StatusType.End)
                                  select s;
                }
                else if (User.IsInRole("claims"))
                {
                    workObjects = from s in db.WorkObjects
                                  where s.TeamQueue == TeamQueueType.Claims
                                  where (s.Status == StatusType.Processed || s.Status == StatusType.End)
                                  select s;
                }
                else if (User.IsInRole("servicing"))
                {
                    workObjects = from s in db.WorkObjects
                                  where s.TeamQueue == TeamQueueType.Servicing
                                  where (s.Status == StatusType.Processed || s.Status == StatusType.End)
                                  select s;
                }
                else if (User.IsInRole("new_business"))
                {
                    workObjects = from s in db.WorkObjects
                                  where s.TeamQueue == TeamQueueType.New_Business
                                  where (s.Status == StatusType.Processed || s.Status == StatusType.End)
                                  select s;
                }


                if (!String.IsNullOrEmpty(nameSearch))
                {
                    workObjects = workObjects.Where(s => s.ClientSurname.Contains(nameSearch)
                                                    || s.ClientName.Contains(nameSearch));
                }
                if (!String.IsNullOrEmpty(policySearch))
                {
                    workObjects = workObjects.Where(s => s.PolicyNo.Contains(policySearch));
                }

                switch (sortOrder)
                {
                    case "Priority":
                        workObjects = workObjects.OrderBy(s => s.Priority);
                        break;
                    case "Date":
                        workObjects = workObjects.OrderBy(s => s.CloseDate);
                        break;
                    case "date_desc":
                        workObjects = workObjects.OrderByDescending(s => s.CloseDate);
                        break;
                    case "Status":
                        workObjects = workObjects.OrderBy(s => s.Status);
                        break;
                    case "status_desc":
                        workObjects = workObjects.OrderByDescending(s => s.Status);
                        break;
                    case "PolicyNo":
                        workObjects = workObjects.OrderBy(s => s.PolicyNo);
                        break;
                    case "policyNo_desc":
                        workObjects = workObjects.OrderByDescending(s => s.PolicyNo);
                        break;
                    case "TeamQueue":
                        workObjects = workObjects.OrderBy(s => s.TeamQueue);
                        break;
                    case "teamQueue_desc":
                        workObjects = workObjects.OrderByDescending(s => s.TeamQueue);
                        break;
                    case "Name":
                        workObjects = workObjects.OrderBy(s => s.ClientSurname);
                        break;
                    case "name_desc":
                        workObjects = workObjects.OrderByDescending(s => s.ClientSurname);
                        break;
                    default: // when no query string, highest priority is at the top the table
                        workObjects = workObjects.OrderByDescending(s => s.Priority);
                        break;
                }
                return View(workObjects.ToList());
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            
        }

        // GET: /WorkObject/Details/5
        public ActionResult Details(int? id)
        {
            string user = User.Identity.Name;
            if (hasActiveStandardSubscription(user) || hasActiveUltimateSubscription(user))
            {

                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                WorkObject workobject = db.WorkObjects.Find(id);

                if (workobject == null)
                {
                    return HttpNotFound();
                }
                // improves mvc security
                if ( (workobject.TeamQueue == TeamQueueType.Claims && User.IsInRole("claims")) ||
                     (workobject.TeamQueue == TeamQueueType.Servicing && User.IsInRole("servicing")) ||
                     (workobject.TeamQueue == TeamQueueType.New_Business && User.IsInRole("new_business")) )
                {             
                    return View(workobject);
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        // GET: /WorkObject/Create
        public ActionResult Create()
        {
            string user = User.Identity.Name;
            if (hasActiveStandardSubscription(user) || hasActiveUltimateSubscription(user))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        // POST: /WorkObject/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="PolicyNo,ClientName,ClientSurname,ClientDOB,TeamQueue,Status,Priority")] WorkObject workobject)
        {
            string user = User.Identity.Name;
            if (hasActiveStandardSubscription(user) || hasActiveUltimateSubscription(user))
            {
                try 
                {
                    if (ModelState.IsValid)
                    {
                        // converts time to Greenwich Mean Time
                        DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

                        workobject.CreatedBy = User.Identity.Name;
                        workobject.StartDate = currentTime;
                        db.WorkObjects.Add(workobject);
                        db.SaveChanges();
                        return RedirectToAction("Details", new { id = workobject.WorkObjectID });
                    } 
                }
                catch (DataException /* dex */)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
                return View(workobject);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        // GET: /WorkObject/Edit/5
        public ActionResult Edit(int? id)
        {
            string user = User.Identity.Name;
            if (hasActiveStandardSubscription(user) || hasActiveUltimateSubscription(user))
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                WorkObject workobject = db.WorkObjects.Find(id);
   
                if (workobject == null)
                {
                    return HttpNotFound();
                }
                else
                {
                    // locking feature             
                    if (workobject.isLocked == true && workobject.LockedBy.Equals(User.Identity.Name))
                    {
                        // WO already locked by the user, access allowed
                        return View(workobject);
                    }
                    else if (workobject.isLocked == true && !workobject.LockedBy.Equals(User.Identity.Name))
                    {
                        // access not allowed
                        return RedirectToAction("Details", new { id = id });
                    }

                    // improve mvc security holes
                    if ( (workobject.TeamQueue == TeamQueueType.Claims && User.IsInRole("claims")) ||
                         (workobject.TeamQueue == TeamQueueType.Servicing && User.IsInRole("servicing")) ||
                         (workobject.TeamQueue == TeamQueueType.New_Business && User.IsInRole("new_business")) )
                    {
                        // once the status was changed to 'end' or 'processed' it cannot be edited
                        if (workobject.Status == StatusType.End || workobject.Status == StatusType.Processed)
                        {
                            return RedirectToAction("Details", new { id = id });
                        }
                        else 
                        {
                            workobject.isLocked = true;
                            workobject.LockedBy = User.Identity.Name;
                            workobject.LockedTime = DateTime.Now;
                            db.SaveChanges();
                            return View(workobject);                        
                        }
                    }
                    else 
                    {
                        return RedirectToAction("Index");
                    } 
                 }
            }
            else
            {
                return RedirectToAction("Index");
            } 
        }

        // POST: /WorkObject/Edit/5
        // security prevention for overposting attack 
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(int? id)
        {
            string user = User.Identity.Name;
            if (hasActiveStandardSubscription(user) || hasActiveUltimateSubscription(user))
            {

                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var workObjectToUpdate = db.WorkObjects.Find(id);
            
                // status state before change
                StatusType before = workObjectToUpdate.Status;
                // priority state before change
                PriorityType beforeP = workObjectToUpdate.Priority;

                if(TryUpdateModel(workObjectToUpdate, "",
                    new string[] { "PolicyNo","ClientName","ClientSurname","ClientDOB","StartDate","TeamQueue","Status","Priority","CreatedBy", "isLocked", "LockedBy", "LockedTime" }))
                {
                    try
                    {
                        workObjectToUpdate.LockedBy = "";
                        workObjectToUpdate.LockedTime = null;
                        workObjectToUpdate.isLocked = false;

                        // save EndDate if status is either 'end' or 'processed'
                        if (workObjectToUpdate.Status == StatusType.End || workObjectToUpdate.Status == StatusType.Processed)
                        {
                            // converts time to Greenwich Mean Time
                            DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

                            workObjectToUpdate.CloseDate = currentTime;
                            workObjectToUpdate.CloseBy = User.Identity.Name;
                        }

                        // record change of status state
                        changeStatus(workObjectToUpdate, before);

                        // record change of priority status state
                        changePriority(workObjectToUpdate, beforeP);

                        db.Entry(workObjectToUpdate).State = EntityState.Modified;
                        db.SaveChanges();
                        checkPriority(); // check priority level
                        return RedirectToAction("Details", new { id = workObjectToUpdate.WorkObjectID });
                    }
                    catch (DataException /* dex */)
                    {
                        //Log the error (uncomment dex variable name and add a line here to write a log.
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                    }
                }
                return View(workObjectToUpdate);
            }
            else
            {
                return RedirectToAction("Index");
            } 
        }

        [Authorize(Roles="Admin")]
        public ActionResult Unlock(int? id)
        {
            if(hasActiveUltimateSubscription(User.Identity.Name) == true)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                WorkObject workobject = db.WorkObjects.Find(id);

                if (workobject == null)
                {
                    return HttpNotFound();
                }
                else
                {
                    workobject.isLocked = false;
                    workobject.LockedBy = null;
                    workobject.LockedTime = null;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            else
            {
                return RedirectToAction("Index");
            }            
        }

        public ActionResult Lock(int? id)
        {
            string user = User.Identity.Name;
            if (hasActiveStandardSubscription(user) || hasActiveUltimateSubscription(user))
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                WorkObject workobject = db.WorkObjects.Find(id);

                if (workobject == null)
                {
                    return HttpNotFound();
                }
                else
                {
                    if ((workobject.TeamQueue == TeamQueueType.Claims && User.IsInRole("claims")) ||
                        (workobject.TeamQueue == TeamQueueType.Servicing && User.IsInRole("servicing")) ||
                        (workobject.TeamQueue == TeamQueueType.New_Business && User.IsInRole("new_business")))
                    {
                        if (workobject.Status == StatusType.End || workobject.Status == StatusType.Processed)
                        {
                            return RedirectToAction("Details", new { id = id });
                        }
                        else
                        {
                            // converts time to Greenwich Mean Time
                            DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

                            workobject.isLocked = true;
                            workobject.LockedBy = User.Identity.Name;
                            workobject.LockedTime = currentTime;
                            db.SaveChanges();
                            return RedirectToAction("Details", new { id = id });
                        }                   
                    }
                    else
                    {
                        return RedirectToAction("Index");
                    }
                }
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        public ActionResult Unlock2(int? id)
        {
            string user = User.Identity.Name;
            if (hasActiveStandardSubscription(user) || hasActiveUltimateSubscription(user))
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                WorkObject workobject = db.WorkObjects.Find(id);

                if (workobject == null)
                {
                    return HttpNotFound();
                }
                else
                {
                    if ((workobject.TeamQueue == TeamQueueType.Claims && User.IsInRole("claims")) ||
                        (workobject.TeamQueue == TeamQueueType.Servicing && User.IsInRole("servicing")) ||
                        (workobject.TeamQueue == TeamQueueType.New_Business && User.IsInRole("new_business")))
                    {
                        if (User.Identity.Name == workobject.LockedBy || (User.IsInRole("Admin") && hasActiveUltimateSubscription(user)))
                        {
                            workobject.isLocked = false;
                            workobject.LockedBy = null;
                            workobject.LockedTime = null;
                            db.SaveChanges();
                            return RedirectToAction("Details", new { id = id });
                        }
                        else
                        {
                            return RedirectToAction("Details", new { id = id });
                        }
                    }
                    else
                    {
                        return RedirectToAction("Index");
                    }
                }
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        // check and change the priority level based on time elapsed - triggered on index and details views
        void checkPriority()
        {
            var workObjects = from s in db.WorkObjects select s;
            try
            {
                foreach (var w in workObjects)
                {
                    // converts time to Greenwich Mean Time
                    DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

                    DateTime dateToCheck = currentTime;

                     // 4 up days range
                    if (w.StartDate.AddDays(4) <= dateToCheck && w.Priority <= PriorityType.High)
                    {
                        w.Priority = PriorityType.Urgent;
                        Comment newComment = new Comment { WorkObjectID = w.WorkObjectID, Content = "*** Priority changed to Urgent (+4 days) ***", Timestamp = currentTime, User = "WDS@Autoupdate.com" };
                        db.Comments.Add(newComment);
                    }
                    // 3 to 4 days range
                    else if (w.StartDate.AddDays(3) <= dateToCheck && dateToCheck < w.StartDate.AddDays(4) && w.Priority <= PriorityType.Normal)
                    {
                        w.Priority = PriorityType.High;
                        Comment newComment = new Comment { WorkObjectID = w.WorkObjectID, Content = "*** Priority changed to High (+3 days) ***", Timestamp = currentTime, User = "WDS@Autoupdate.com" };
                        db.Comments.Add(newComment);
                    }
                     // 2 to 3 days range
                    else if (w.StartDate.AddDays(2) <= dateToCheck && dateToCheck < w.StartDate.AddDays(3) && w.Priority <= PriorityType.Low)
                    {
                        w.Priority = PriorityType.Normal;
                        Comment newComment = new Comment { WorkObjectID = w.WorkObjectID, Content = "*** Priority changed to Normal (+2 days) ***", Timestamp = currentTime, User = "WDS@Autoupdate.com" };
                        db.Comments.Add(newComment);
                    }
                    // 1 to 2 days range
                    else if (w.StartDate.AddDays(1) <= dateToCheck && dateToCheck < w.StartDate.AddDays(2) && w.Priority == PriorityType.Lowest)
                    {
                        w.Priority = PriorityType.Low;
                        Comment newComment = new Comment { WorkObjectID = w.WorkObjectID, Content = "*** Priority changed to Low (+1 day) ***", Timestamp = currentTime, User = "WDS@Autoupdate.com" };
                        db.Comments.Add(newComment);
                    }                                          
                }
                db.SaveChanges();
            }
            catch (DataException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }
        }

        // change status and record it as a new comment
        void changeStatus(WorkObject workObjectToUpdate, StatusType before)
        {
            if (workObjectToUpdate.Status != before)
            {
                StatusType after = workObjectToUpdate.Status;
                int id_ = workObjectToUpdate.WorkObjectID;

                if (before == StatusType.Open && after == StatusType.Process) // open -> process
                {
                    // converts time to Greenwich Mean Time
                    DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

                    Comment newComment = new Comment { WorkObjectID = id_, Content = "*** Status changed: Open to Process ***", Timestamp = currentTime, User = User.Identity.Name };
                    db.Comments.Add(newComment);
                }
                else if (before == StatusType.Open && after == StatusType.Delay) // open -> delay
                {
                    // converts time to Greenwich Mean Time
                    DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

                    Comment newComment = new Comment { WorkObjectID = id_, Content = "*** Status changed: Open to Delay ***", Timestamp = currentTime, User = User.Identity.Name };
                    db.Comments.Add(newComment);
                }
                else if (before == StatusType.Open && after == StatusType.End) // open -> end
                {
                    // converts time to Greenwich Mean Time
                    DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

                    Comment newComment = new Comment { WorkObjectID = id_, Content = "*** Status changed: Open to End ***", Timestamp = currentTime, User = User.Identity.Name };
                    db.Comments.Add(newComment);
                }
                else if (before == StatusType.Open && after == StatusType.Processed) // open -> processed
                {
                    // converts time to Greenwich Mean Time
                    DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

                    Comment newComment = new Comment { WorkObjectID = id_, Content = "*** Status changed: Open to Processed ***", Timestamp = currentTime, User = User.Identity.Name };
                    db.Comments.Add(newComment);
                }
                else if (before == StatusType.Process && after == StatusType.Open) // process -> open
                {
                    // converts time to Greenwich Mean Time
                    DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

                    Comment newComment = new Comment { WorkObjectID = id_, Content = "*** Status changed: Process to Open ***", Timestamp = currentTime, User = User.Identity.Name };
                    db.Comments.Add(newComment);
                }
                else if (before == StatusType.Process && after == StatusType.Processed) // process -> processed
                {
                    // converts time to Greenwich Mean Time
                    DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

                    Comment newComment = new Comment { WorkObjectID = id_, Content = "*** Status changed: Process to Processed ***", Timestamp = currentTime, User = User.Identity.Name };
                    db.Comments.Add(newComment);
                }                              
                else if (before == StatusType.Process && after == StatusType.Delay) // process -> delay
                {
                    // converts time to Greenwich Mean Time
                    DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

                    Comment newComment = new Comment { WorkObjectID = id_, Content = "*** Status changed: Process to Delay ***", Timestamp = currentTime, User = User.Identity.Name };
                    db.Comments.Add(newComment);
                }
                else if (before == StatusType.Process && after == StatusType.End) // process -> end
                {
                    // converts time to Greenwich Mean Time
                    DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

                    Comment newComment = new Comment { WorkObjectID = id_, Content = "*** Status changed: Process to Delay ***", Timestamp = currentTime, User = User.Identity.Name };
                    db.Comments.Add(newComment);
                }
                else if (before == StatusType.Process && after == StatusType.End) // process -> end
                {
                    // converts time to Greenwich Mean Time
                    DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

                    Comment newComment = new Comment { WorkObjectID = id_, Content = "*** Status changed: Process to Delay ***", Timestamp = currentTime, User = User.Identity.Name };
                    db.Comments.Add(newComment);
                }
                else if (before == StatusType.Delay && after == StatusType.Processed) // delay -> processed
                {
                    // converts time to Greenwich Mean Time
                    DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

                    Comment newComment = new Comment { WorkObjectID = id_, Content = "*** Status changed: Delay to Processed ***", Timestamp = currentTime, User = User.Identity.Name };
                    db.Comments.Add(newComment);
                }
                else if (before == StatusType.Delay && after == StatusType.Open) // delay -> open
                {
                    // converts time to Greenwich Mean Time
                    DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

                    Comment newComment = new Comment { WorkObjectID = id_, Content = "*** Status changed: Delay to Open ***", Timestamp = currentTime, User = User.Identity.Name };
                    db.Comments.Add(newComment);
                }
                else if (before == StatusType.Delay && after == StatusType.End) // delay -> end
                {
                    // converts time to Greenwich Mean Time
                    DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

                    Comment newComment = new Comment { WorkObjectID = id_, Content = "*** Status changed: Delay to End ***", Timestamp = currentTime, User = User.Identity.Name };
                    db.Comments.Add(newComment);
                }
                else if (before == StatusType.Delay && after == StatusType.Process) // delay -> process
                {
                    // converts time to Greenwich Mean Time
                    DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

                    Comment newComment = new Comment { WorkObjectID = id_, Content = "*** Status changed: Delay to Process ***", Timestamp = currentTime, User = User.Identity.Name };
                    db.Comments.Add(newComment);
                } // processed and end status mean a workobject is in history queue and cannot be edited
            }
        }

        void changePriority(WorkObject workObjectToUpdate, PriorityType before)
        {
            if (workObjectToUpdate.Priority != before)
            {
                PriorityType after = workObjectToUpdate.Priority;
                int id_ = workObjectToUpdate.WorkObjectID;

                if (before == PriorityType.Lowest && after == PriorityType.Low) // lowest -> low
                {
                    // converts time to Greenwich Mean Time
                    DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

                    Comment newComment = new Comment { WorkObjectID = id_, Content = "*** Priority changed: Lowest to Low ***", Timestamp = currentTime, User = User.Identity.Name };
                    db.Comments.Add(newComment);
                }
                else if (before == PriorityType.Lowest && after == PriorityType.Normal) // lowest -> normal
                {
                    // converts time to Greenwich Mean Time
                    DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

                    Comment newComment = new Comment { WorkObjectID = id_, Content = "*** Priority changed: Lowest to Normal ***", Timestamp = currentTime, User = User.Identity.Name };
                    db.Comments.Add(newComment);
                }
                else if (before == PriorityType.Lowest && after == PriorityType.High) // lowest -> high
                {
                    // converts time to Greenwich Mean Time
                    DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

                    Comment newComment = new Comment { WorkObjectID = id_, Content = "*** Priority changed: Lowest to High ***", Timestamp = currentTime, User = User.Identity.Name };
                    db.Comments.Add(newComment);
                }
                else if (before == PriorityType.Lowest && after == PriorityType.High) // lowest -> urgent
                {
                    // converts time to Greenwich Mean Time
                    DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

                    Comment newComment = new Comment { WorkObjectID = id_, Content = "*** Priority changed: Lowest to Urgent ***", Timestamp = currentTime, User = User.Identity.Name };
                    db.Comments.Add(newComment);
                }
                else if (before == PriorityType.Low && after == PriorityType.Lowest) // low -> lowest
                {
                    // converts time to Greenwich Mean Time
                    DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

                    Comment newComment = new Comment { WorkObjectID = id_, Content = "*** Priority changed: Low to Lowest ***", Timestamp = currentTime, User = User.Identity.Name };
                    db.Comments.Add(newComment);
                }
                else if (before == PriorityType.Low && after == PriorityType.Normal) // low -> normal
                {
                    // converts time to Greenwich Mean Time
                    DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

                    Comment newComment = new Comment { WorkObjectID = id_, Content = "*** Priority changed: Low to Normal ***", Timestamp = currentTime, User = User.Identity.Name };
                    db.Comments.Add(newComment);
                }
                else if (before == PriorityType.Low && after == PriorityType.High) // low -> high
                {
                    // converts time to Greenwich Mean Time
                    DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

                    Comment newComment = new Comment { WorkObjectID = id_, Content = "*** Priority changed: Low to High ***", Timestamp = currentTime, User = User.Identity.Name };
                    db.Comments.Add(newComment);
                }
                else if (before == PriorityType.Low && after == PriorityType.High) // low -> urgent
                {
                    // converts time to Greenwich Mean Time
                    DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

                    Comment newComment = new Comment { WorkObjectID = id_, Content = "*** Priority changed: Low to Urgent ***", Timestamp = currentTime, User = User.Identity.Name };
                    db.Comments.Add(newComment);
                }
                else if (before == PriorityType.Normal && after == PriorityType.Lowest) // normal -> lowest
                {
                    // converts time to Greenwich Mean Time
                    DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

                    Comment newComment = new Comment { WorkObjectID = id_, Content = "*** Priority changed: Normal to Lowest ***", Timestamp = currentTime, User = User.Identity.Name };
                    db.Comments.Add(newComment);
                }
                else if (before == PriorityType.Normal && after == PriorityType.Low) // normal -> low
                {
                    // converts time to Greenwich Mean Time
                    DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

                    Comment newComment = new Comment { WorkObjectID = id_, Content = "*** Priority changed: Normal to Low ***", Timestamp = currentTime, User = User.Identity.Name };
                    db.Comments.Add(newComment);
                }
                else if (before == PriorityType.Normal && after == PriorityType.High) // normal -> high
                {
                    // converts time to Greenwich Mean Time
                    DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

                    Comment newComment = new Comment { WorkObjectID = id_, Content = "*** Priority changed: Normal to High ***", Timestamp = currentTime, User = User.Identity.Name };
                    db.Comments.Add(newComment);
                }
                else if (before == PriorityType.Normal && after == PriorityType.High) // normal -> urgent
                {
                    // converts time to Greenwich Mean Time
                    DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

                    Comment newComment = new Comment { WorkObjectID = id_, Content = "*** Priority changed: Normal to Urgent ***", Timestamp = currentTime, User = User.Identity.Name };
                    db.Comments.Add(newComment);
                }
                else if (before == PriorityType.High && after == PriorityType.Lowest) // high -> lowest
                {
                    // converts time to Greenwich Mean Time
                    DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

                    Comment newComment = new Comment { WorkObjectID = id_, Content = "*** Priority changed: High to Lowest ***", Timestamp = currentTime, User = User.Identity.Name };
                    db.Comments.Add(newComment);
                }
                else if (before == PriorityType.High && after == PriorityType.Low) // high -> low
                {
                    // converts time to Greenwich Mean Time
                    DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

                    Comment newComment = new Comment { WorkObjectID = id_, Content = "*** Priority changed: High to Low ***", Timestamp = currentTime, User = User.Identity.Name };
                    db.Comments.Add(newComment);
                }
                else if (before == PriorityType.High && after == PriorityType.Low) // high -> normal
                {
                    // converts time to Greenwich Mean Time
                    DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

                    Comment newComment = new Comment { WorkObjectID = id_, Content = "*** Priority changed: High to Normal ***", Timestamp = currentTime, User = User.Identity.Name };
                    db.Comments.Add(newComment);
                }
                else if (before == PriorityType.High && after == PriorityType.Low) // high -> urgent
                {
                    // converts time to Greenwich Mean Time
                    DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

                    Comment newComment = new Comment { WorkObjectID = id_, Content = "*** Priority changed: High to Urgent ***", Timestamp = currentTime, User = User.Identity.Name };
                    db.Comments.Add(newComment);
                }
                else if (before == PriorityType.Urgent && after == PriorityType.Lowest) // urgent -> lowest
                {
                    // converts time to Greenwich Mean Time
                    DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

                    Comment newComment = new Comment { WorkObjectID = id_, Content = "*** Priority changed: Urgent to Lowest ***", Timestamp = currentTime, User = User.Identity.Name };
                    db.Comments.Add(newComment);
                }
                else if (before == PriorityType.Urgent && after == PriorityType.Low) // urgent -> low
                {
                    // converts time to Greenwich Mean Time
                    DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

                    Comment newComment = new Comment { WorkObjectID = id_, Content = "*** Priority changed: Urgent to Low ***", Timestamp = currentTime, User = User.Identity.Name };
                    db.Comments.Add(newComment);
                }
                else if (before == PriorityType.Urgent && after == PriorityType.Normal) // urgent -> normal
                {
                    // converts time to Greenwich Mean Time
                    DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

                    Comment newComment = new Comment { WorkObjectID = id_, Content = "*** Priority changed: Urgent to Normal ***", Timestamp = currentTime, User = User.Identity.Name };
                    db.Comments.Add(newComment);
                }
                else if (before == PriorityType.Urgent && after == PriorityType.High) // urgent -> high
                {
                    // converts time to Greenwich Mean Time
                    DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

                    Comment newComment = new Comment { WorkObjectID = id_, Content = "*** Priority changed: Urgent to High ***", Timestamp = currentTime, User = User.Identity.Name };
                    db.Comments.Add(newComment);
                }
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
