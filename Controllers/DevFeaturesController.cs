using InzV3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InzV3.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DevFeaturesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: DevCategories
        public ActionResult Index()
        {
            var features = db.DevFeatures.ToList();
            return View(features);
        }
        public ActionResult Create()
        {
            return View("CreateDevFeature");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DevFeature feature)
        {
            if(db.DevFeatures.Any(f => f.dev_feature_name.ToLower() == feature.dev_feature_name.ToLower()))
                {
                    ModelState.AddModelError("dev_feature_name", "Cecha o takiej nazwie już istnieje!");
            }
            if (ModelState.IsValid)
            {
                db.DevFeatures.Add(feature);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View("CreateDevFeature", feature);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            bool isFeatureInUse = db.DevCharacteristics.Any(c => c.id_dev_feature == id);
            if (isFeatureInUse)
            {
                
                TempData["ErrorMessage"] = "Ta cecha jest w użyciu!!!";
                return RedirectToAction("Index");
            }
            var features = db.DevFeatures.Find(id);
            if (features!=null)
            {
                db.DevFeatures.Remove(features);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Cecha została usunięta.";
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