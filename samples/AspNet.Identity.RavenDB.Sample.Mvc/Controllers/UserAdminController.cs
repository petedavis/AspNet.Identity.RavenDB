using System;
using System.Collections.Generic;
using System.Security.Claims;
using AspNet.Identity.RavenDB.Entities;
using AspNet.Identity.RavenDB.Sample.Mvc;
using AspNet.Identity.RavenDB.Sample.Mvc.Infrastructure.AutoMapper;
using AspNet.Identity.RavenDB.Sample.Mvc.Infrastructure.AutoMapper.Profiles;
using AspNet.Identity.RavenDB.Sample.Mvc.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Raven.Client;

namespace IdentitySample.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersAdminController : Controller
    {
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
            return ListView(await UserManager.Users.ToListAsync());
        }

        //
        // GET: /Users/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);

            ViewBag.RoleNames = user.Claims.Where(x => x.ClaimType == ClaimTypes.Role).Select(x => x.ClaimValue).ToList();

            return View(user);
        }

        //
        // GET: /Users/Create
        public async Task<ActionResult> Create()
        {
            //Get the list of Roles
            ViewBag.RoleId = new SelectList(await RoleManager.Roles.ToListAsync(), "Name", "Name");
            return View();
        }

        //
        // POST: /Users/Create
        [HttpPost]
        public async Task<ActionResult> Create(RegisterViewModel userViewModel, params string[] selectedRoles)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser (userViewModel.Email, userViewModel.Email );


                //Add User to the selected Roles 
                foreach (var claim in selectedRoles.Select(x => new IdentityUserClaim(ClaimTypes.Role, x)))
                {
                    user.Claims.Add(claim);
                }

                var adminresult = await UserManager.CreateAsync(user, userViewModel.Password);

                if (!adminresult.Succeeded)
                {
                    ModelState.AddModelError("", adminresult.Errors.First());
                    ViewBag.RoleId = new SelectList(await RoleManager.Roles.ToListAsync(), "Name", "Name");
                    return View();

                }
                return RedirectToAction("Index");
            }
            ViewBag.RoleId = new SelectList(await RoleManager.Roles.ToListAsync(), "Name", "Name");
            return View();
        }

        //
        // GET: /Users/Edit/1
        public async Task<ActionResult> Edit(string id)
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

            var ravenRoles = await RoleManager.Roles.ToListAsync();
            return View(new EditUserViewModel()
            {
                Id = user.Id,
                Email = user.Email,
                RolesList = ravenRoles.Select(x => new SelectListItem()
                {
                    Selected = user.Claims.Any(c => c.ClaimType == ClaimTypes.Role && c.ClaimValue.Equals(x.Name, StringComparison.OrdinalIgnoreCase)),
                    Text = x.Name,
                    Value = x.Name
                })
            });
        }

        //
        // POST: /Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Email,Id")] EditUserViewModel editUser, params string[] selectedRole)
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

                var userRoles = user.Claims.Where(c => c.ClaimType == ClaimTypes.Role).Select(c => c.ClaimValue).ToList();

                selectedRole = selectedRole ?? new string[] { };

                foreach (var claim in selectedRole.Except(userRoles).Select(x => new IdentityUserClaim(ClaimTypes.Role, x)))
                {
                    user.Claims.Add(claim);
                }

                foreach (var claim in userRoles.Except(selectedRole).Select(x => new IdentityUserClaim(ClaimTypes.Role, x)))
                {
                    user.Claims.Remove(claim);
                }

                var result = await UserManager.UpdateAsync(user);

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

        //
        // GET: /Users/Delete/5
        public async Task<ActionResult> Delete(string id)
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
            return View(user);
        }

        //
        // POST: /Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
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

        private ActionResult ListView(IList<ApplicationUser> users)
        {
            var summaries = users.MapTo<UsersViewModel.UserSummary>();

            return View("Index", new UsersViewModel
            {
                Users = summaries
            });
        }
    }
}
