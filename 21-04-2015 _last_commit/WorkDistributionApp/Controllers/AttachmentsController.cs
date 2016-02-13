using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WorkDistributionApp.DAL;
using WorkDistributionApp.Models;

namespace WorkDistributionApp.Controllers
{
    [Authorize(Roles = "Admin, CanEdit")]
    public class AttachmentsController : Controller
    {
        private WorkObjectContext db = new WorkObjectContext();

        // GET: Attachments/Create/4
        public ActionResult Create(int? id)
        {
            string user = User.Identity.Name;
            if (hasActiveStandardSubscription(user) || hasActiveUltimateSubscription(user))
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                Attachment attachment = new Attachment 
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
                        return View(attachment);
                    }
                    else
                    {
                        return RedirectToAction("Details", "WorkObject", new { id = attachment.WorkObjectID });
                    }
                }
                else
                {
                    return RedirectToAction("Index", "WorkObject" );
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: Attachments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "WorkObjectID,FileType,File, FileName")] Attachment attachment)
        {
            string user = User.Identity.Name;
            if (hasActiveStandardSubscription(user) || hasActiveUltimateSubscription(user))
            {
                if (ModelState.IsValid)
                {
                    if (Request.Files != null && Request.Files.Count == 1)
                    {
                        HttpPostedFileBase myFile = Request.Files[0];
                        if (myFile != null && myFile.ContentLength > 0)
                        {
                            Byte[] content = new byte[myFile.ContentLength];
                            myFile.InputStream.Read(content, 0, myFile.ContentLength);

                            // file name attribute validation
                            if(myFile.FileName.Length > 30)
                            {
                                ModelState.AddModelError("myFile", "The name of the file should not exceed 30 characters");
                                return View(attachment);
                            }

                            // file size attribute validation
                            if (myFile.ContentLength > 10 * 1024 * 1024)
                            {
                                ModelState.AddModelError("myFile", "The size of the file should not exceed 10MB");
                                return View(attachment);
                            }

                            // file type attribute validation
                            if (attachment.FileType == FileType.Audio)
                            {
                                var supportedTypes = new[] { "mp3" };
                                var fileExt = Path.GetExtension(myFile.FileName).Substring(1).ToLower();                          

                                if(!supportedTypes.Contains(fileExt))
                                {
                                    ModelState.AddModelError("myFile", "Invalid type. Only the following (mp3) is supported");
                                    return View(attachment);
                                }
                            }
                            else 
                            { 
                                var supportedTypes = new[] { "jpeg", "jpg", "png", "bmp", "txt", "docx", "doc", "pdf" };

                                var fileExt = Path.GetExtension(myFile.FileName).Substring(1).ToLower();

                                if (!supportedTypes.Contains(fileExt))
                                {
                                    ModelState.AddModelError("myFile", "Invalid type. Only the following (jpg, jpeg, bmp, png, txt, doc, docx, pdf) are supported");
                                    return View(attachment);
                                }
                            }
                            // converts time to Greenwich Mean Time
                            DateTime currentTime = TimeConverter.ConvertToLocalTime(DateTime.Now, "GMT Standard Time");

                            attachment.File = content; // converted file
                            attachment.FileName = myFile.FileName;
                            attachment.Timestamp = currentTime;
                            attachment.User = User.Identity.Name;
                            db.Attachments.Add(attachment);
                            db.SaveChanges();
                            return RedirectToAction("Details", "WorkObject", new { id = attachment.WorkObjectID });
                        }
                    }
                }
                return View(attachment);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize(Roles="Admin")]
        // GET: Attachments/Delete/5
        public ActionResult Delete(int? id)
        {
            Attachment attachment = db.Attachments.Find(id);

            if (hasActiveUltimateSubscription(User.Identity.Name))
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                
                if (attachment == null)
                {
                    return HttpNotFound();
                }
                return View(attachment);
            }
            else
            {
                return RedirectToAction("Details", "WorkObject", new { id = attachment.WorkObjectID });
            }
        }

        // POST: Attachments/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Attachment attachment = db.Attachments.Find(id);

            if (hasActiveUltimateSubscription(User.Identity.Name))
            {
                db.Attachments.Remove(attachment);
                db.SaveChanges();
                return RedirectToAction("Details", "WorkObject", new { id = attachment.WorkObjectID });
            }
            else
            {
                return RedirectToAction("Details", "WorkObject", new { id = attachment.WorkObjectID });
            }
        }

        // GET: Attachments/RenderFile/5
        public ActionResult RenderFile(int? id)
        {
            string user = User.Identity.Name;
            if (hasActiveStandardSubscription(user) || hasActiveUltimateSubscription(user))
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Attachment attachment = db.Attachments.Find(id);
                byte[] file = attachment.File;
                WorkObject workobject = db.WorkObjects.Find(attachment.WorkObjectID);

                if ((workobject.TeamQueue == TeamQueueType.Claims && User.IsInRole("claims")) ||
                     (workobject.TeamQueue == TeamQueueType.Servicing && User.IsInRole("servicing")) ||
                     (workobject.TeamQueue == TeamQueueType.New_Business && User.IsInRole("new_business")))
                {
                    if ((workobject.isLocked == true && workobject.LockedBy == User.Identity.Name) || workobject.isLocked == false)
                    {
                        // file extension
                        var fileExt = Path.GetExtension(attachment.FileName).Substring(1).ToLower();

                        // render different file types
                        if (fileExt == "pdf")
                        {
                            return File(file, "application/pdf");
                        }
                        else if (fileExt == "docx")
                        {  // open word document in read only mode
                            return File(file, "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                        }
                        else if (fileExt == "doc")
                        {   // open word document in read only mode
                            return File(file, "application/msword");
                        }
                        else
                        {
                            return File(file, "application/octet-stream");
                        }
                    }
                    else
                    {
                        return RedirectToAction("Index", "WorkObject");
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

        // GET: Attachments/DownloadFile/4
        public ActionResult DownloadFile(int? id)
        {
            string user = User.Identity.Name;
            if (hasActiveStandardSubscription(user) || hasActiveUltimateSubscription(user))
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Attachment attachment = db.Attachments.Find(id);
                byte[] file = attachment.File;
            return File(file, "application/octet-stream", attachment.FileName);
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
