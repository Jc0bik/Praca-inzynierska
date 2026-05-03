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
            var subRoles = db.SubRoles.OrderBy(s => s.ParentRoleName).ThenBy(s => s.Name).ToList();
            return View(subRoles);
        }
        public ActionResult Create()
        {
            ViewBag.ParentRoles = new SelectList(new[] { "Admin", "Pracownik Serwisu", "Użytkownik" });
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SubRoleModel model)
        {
            if (ModelState.IsValid)
            {
                if (db.SubRoles.Any(s => s.Name.ToLower() == model.Name.ToLower() && s.ParentRoleName==model.ParentRoleName))
                {
                    ModelState.AddModelError("", "Taka rola już istnieje!");
                    ViewBag.ParentRoles = new SelectList(new[] {"Admin", "Pracownik Serwisu", "Użytkownik"}, model.ParentRoleName);
                    return View(model);
                }
                db.SubRoles.Add(model);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ParentRoles = new SelectList(new[] { "Admin", "Pracownik Serwisu", "Użytkownik" }, model.ParentRoleName);
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var subRole = db.SubRoles.Find(id);
            if (subRole != null)
            {
                //zabezpieczenie przed usuwaniem roli do której są przypisani użytkownicy
                if (db.Users.Any(u => u.SubRole == subRole.Name))
                {
                    TempData["Error"] = "Nie można usunąć podroli, ponieważ są do niej przypisani użytkownicy.";
                    return RedirectToAction("Index");
                }
                db.SubRoles.Remove(subRole);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}