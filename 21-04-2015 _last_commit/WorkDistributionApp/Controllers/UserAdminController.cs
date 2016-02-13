using WorkDistributionApp.Models;
using Microsoft.AspNet.Identity.Owin;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WorkDistributionApp.DAL;

namespace WorkDistributionApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersAdminController : Controller
    {
        private WorkObjectContext db = new WorkObjectContext();

        public UsersAdminController()
        {
        }

        public UsersAdminController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
        {
            UserManager = userManager;
            RoleManager = roleManager;
        }

        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        private ApplicationRoleManager _roleManager;
        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }

        //
        // GET: /Users/
        public async Task<ActionResult> Index()
        {
            if (hasActiveUltimateSubscription(User.Identity.Name))
            {
                return View(await UserManager.Users.ToListAsync());
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        //
        // GET: /Users/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (hasActiveUltimateSubscription(User.Identity.Name))
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var user = await UserManager.FindByIdAsync(id);

                ViewBag.RoleNames = await UserManager.GetRolesAsync(user.Id);

                return View(user);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            } 
        }

        //
        // GET: /Users/Create
        public async Task<ActionResult> Create()
        {
            if (hasActiveUltimateSubscription(User.Identity.Name))
            {
                //Get the list of Roles
                ViewBag.RoleId = new SelectList(await RoleManager.Roles.ToListAsync(), "Name", "Name");
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            } 
        }

        //
        // POST: /Users/Create
        [HttpPost]
        public async Task<ActionResult> Create(RegisterViewModel userViewModel, params string[] selectedRoles)
        {
            if (hasActiveUltimateSubscription(User.Identity.Name))
            {
                if (ModelState.IsValid)
                {
                    var user = new ApplicationUser { UserName = userViewModel.Email, Email = userViewModel.Email };
                    var adminresult = await UserManager.CreateAsync(user, userViewModel.Password);

                    //Add User to the selected Roles 
                    if (adminresult.Succeeded)
                    {
                        if (selectedRoles != null)
                        {
                            var result = await UserManager.AddToRolesAsync(user.Id, selectedRoles);
                            if (!result.Succeeded)
                            {
                                ModelState.AddModelError("", result.Errors.First());
                                ViewBag.RoleId = new SelectList(await RoleManager.Roles.ToListAsync(), "Name", "Name");
                                return View();
                            }
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", adminresult.Errors.First());
                        ViewBag.RoleId = new SelectList(RoleManager.Roles, "Name", "Name");
                        return View();

                    }
                    return RedirectToAction("Index");
                }
                ViewBag.RoleId = new SelectList(RoleManager.Roles, "Name", "Name");
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            } 
        }

        //
        // GET: /Users/Edit/1
        public async Task<ActionResult> Edit(string id)
        {
            if (hasActiveUltimateSubscription(User.Identity.Name))
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var user = await UserManager.FindByIdAsync(id);
                if (user == null)
                {
                    return HttpNotFound();
                }

                var userRoles = await UserManager.GetRolesAsync(user.Id);

                return View(new EditUserViewModel()
                {
                    Id = user.Id,
                    Email = user.Email,
                    RolesList = RoleManager.Roles.ToList().Select(x => new SelectListItem()
                    {
                        Selected = userRoles.Contains(x.Name),
                        Text = x.Name,
                        Value = x.Name
                    })
                });
            }
            else
            {
                return RedirectToAction("Index", "Home");
            } 
        }

        //
        // POST: /Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Email,Id")] EditUserViewModel editUser, params string[] selectedRole)
        {
            if (hasActiveUltimateSubscription(User.Identity.Name))
            {
                if (ModelState.IsValid)
                {
                    var user = await UserManager.FindByIdAsync(editUser.Id);
                    if (user == null)
                    {
                        return HttpNotFound();
                    }

                    user.UserName = editUser.Email;
                    user.Email = editUser.Email;

                    var userRoles = await UserManager.GetRolesAsync(user.Id);

                    selectedRole = selectedRole ?? new string[] { };

                    var result = await UserManager.AddToRolesAsync(user.Id, selectedRole.Except(userRoles).ToArray<string>());

                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError("", result.Errors.First());
                        return View();
                    }
                    result = await UserManager.RemoveFromRolesAsync(user.Id, userRoles.Except(selectedRole).ToArray<string>());

                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError("", result.Errors.First());
                        return View();
                    }
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("", "Something failed.");
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            } 
        }

        //
        // GET: /Users/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (hasActiveUltimateSubscription(User.Identity.Name))
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var user = await UserManager.FindByIdAsync(id);
                if (user == null)
                {
                    return HttpNotFound();
                }
                if (user.UserName == "admin@example.com") // prevent from deleting admin
                {
                    return RedirectToAction("Index");
                }
                return View(user);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            } 
        }

        //
        // POST: /Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            if (hasActiveUltimateSubscription(User.Identity.Name))
            {
                if (ModelState.IsValid)
                {
                    if (id == null)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }

                    var user = await UserManager.FindByIdAsync(id);
                    if (user == null)
                    {
                        return HttpNotFound();
                    }
                    if (user.UserName == "admin@example.com") // prevent from deleting admin account
                    {
                        return RedirectToAction("Index");
                    }
                    var result = await UserManager.DeleteAsync(user);
                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError("", result.Errors.First());
                        return View();
                    }
                    return RedirectToAction("Index");
                }
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            } 
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
    }
}
