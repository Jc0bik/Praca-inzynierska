using InzV3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace InzV3.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DevCategoriesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public DevCategoriesController()
        {
        }

        // GET: DevCategories
        public ActionResult Index()
        {
            var categories = db.DevCategories.ToList();
            return View(categories);
        }
        public ActionResult Create()
        {
            return View("CreateCategory");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DevCategory category)
        {
            if(db.DevCategories.Any(c => c.category_name.ToLower() == category.category_name.ToLower()))
            {
                ModelState.AddModelError("category_name", "Kategoria o takiej nazwie już istnieje!");
            }
            if (ModelState.IsValid)
            {
                db.DevCategories.Add(category);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View("CreateCategory", category);
        }
        public ActionResult Delete(int? id)
        {
            if(id==null)
            {
                TempData["ErrorMessage"] = "Nie można znaleźć tej kategorii.";
                return RedirectToAction("Index");
            }
            DevCategory category = db.DevCategories.Find(id);
            if(category==null)
            {
                TempData["ErrorMessage"] = "Nie można znaleźć tej kategorii.";
                return RedirectToAction("Index");
            }
            return View("DeleteCategory",category);

        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            bool isCategoryInUse = db.DevCharacteristics.Any(c => c.id_category == id);
            if (isCategoryInUse)
            {
                TempData["ErrorMessage"] = "Nie można usunąć tej kategorii, ponieważ jest używana przez  urządzenia.";
                return RedirectToAction("Index");
            }
            DevCategory category = db.DevCategories.Find(id);
            if (category != null)
            {
                db.DevCategories.Remove(category);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Kategoria została usunięta.";
            }
            return RedirectToAction("Index");

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