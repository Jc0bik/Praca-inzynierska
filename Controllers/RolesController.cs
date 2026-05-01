using InzV3.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InzV3.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        // GET: Roles
        private ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index()
        {
            var roles = db.Roles.ToList();
            return View(roles);
        }
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                ModelState.AddModelError("", "Nazwa roli nie może być pusta.");
                return View();
            }
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            if (roleManager.RoleExists(roleName))
            {
                ModelState.AddModelError("", "Ta rola już istnieje");
                return View();
            }
            roleManager.Create(new IdentityRole(roleName));
            return RedirectToAction("Index");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string id)
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            var role = roleManager.FindById(id);
            if (role != null)
            {
                //zabezpieczenie przed usuwaniem roli do której są przypisani użytkownicy
                if (role.Users.Count > 0)
                {
                    TempData["Error"] = "Nie można usunąć roli, ponieważ są do niej przypisani użytkownicy.";
                    return RedirectToAction("Index");
                }
                if (role.Name == "Admin")
                {
                    TempData["Error"] = "Nie można usunąć roli Admin.";
                    return RedirectToAction("Index");
                }
                roleManager.Delete(role);
            }
            return RedirectToAction("Index");
        }
    }
}