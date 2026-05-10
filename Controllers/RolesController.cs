using InzV3.Models;
using System.Data.Entity;
using System.Linq;
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
            var subRoles = db.SubRoles.Include(s => s.Role).OrderBy(s => s.Role.Name).ThenBy(s => s.Name).ToList();
            return View(subRoles);
        }
        public ActionResult Create()
        {
            ViewBag.ParentRoles = new SelectList(db.Roles, "Id", "Name");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SubRoleModel model)
        {
            if (ModelState.IsValid)
            {
                if (db.SubRoles.Any(s => s.Name.ToLower() == model.Name.ToLower() && s.RoleId == model.RoleId))
                {
                    ModelState.AddModelError("", "Taka rola już istnieje!");
                    ViewBag.ParentRoles = new SelectList(db.Roles.ToList(), "Id", "Name", model.RoleId);
                    return View(model);
                }
                db.SubRoles.Add(model);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ParentRoles = new SelectList(db.Roles.ToList(), "Id", "Name", model.RoleId);
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
                if (db.Users.Any(u => u.SubRoleId == id))
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