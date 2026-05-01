using InzV3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
namespace InzV3.Controllers
{
    [Authorize(Roles = "Admin, Pracownik Serwisu")]
    public class SoftwareController : Controller
    {
        private ApplicationDbContext db=new ApplicationDbContext();
        private List<string> GetCategories()
        {
            return new List<string> { "System operacyjny", "Oprogramowanie", "Subskrybcja" };
        }
        // GET: Software
        public ActionResult Index(string searchName, string searchSerial, string categoryFilter, string statusFilter, string sortOrder)
        {
            var software = db.Software.Include(s => s.id_device).AsQueryable();

            // Filtry wyszukiwania
            if (!string.IsNullOrEmpty(searchName))
            {
                software = software.Where(s => s.software_name.ToLower().Contains(searchName.ToLower()));
            }
            if (!string.IsNullOrEmpty(searchSerial) && searchSerial.Length >= 4)
            {
                software = software.Where(s => s.software_serial_number.ToLower().Contains(searchSerial.ToLower()));
            }
            if (!string.IsNullOrEmpty(categoryFilter))
            {
                software = software.Where(s => s.software_category == categoryFilter);
            }
            if (statusFilter == "Nieprzypisane")
            {
                software = software.Where(s => s.assigned_device == null);
            }
            else if (statusFilter == "Przypisane")
            {
                software = software.Where(s => s.assigned_device != null);
            }
            switch (sortOrder)
            {
                case "date_ascending":
                    software = software.OrderBy(s => s.license_end_date);
                    break;
                case "date_descending":
                    software = software.OrderByDescending(s => s.license_end_date);
                    break;
                default:
                    software = software.OrderBy(s => s.software_name);
                    break;
            }
            ViewBag.Categories = new SelectList(GetCategories());
            ViewBag.CurrentSort = sortOrder;
            return View(software.ToList());
        }
        [HttpGet]
        public JsonResult GetDeviceInfo(string serial)
        {
            var device = db.Devices.FirstOrDefault(d => d.serial_num == serial);
            if (device != null)
            {
                return Json(new { success = true, id = device.id_device, brand = device.brand, model = device.model }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create()
        {
            ViewBag.Categories = new SelectList(GetCategories());
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SoftwareModel software)
        {
            if(ModelState.IsValid)
            {
                db.Software.Add(software);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Categories = new SelectList(GetCategories(), software.software_category);
            return View(software);
        }
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            SoftwareModel software = db.Software.Include(s=>s.id_device).FirstOrDefault(s => s.id_software == id);
            if (software == null) return HttpNotFound();
            ViewBag.Categories = new SelectList(GetCategories(), software.software_category);
            ViewBag.CurrentDeviceSerial=software.id_device != null ? software.id_device.serial_num : "";
            return View(software);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SoftwareModel software)
        {
            if (ModelState.IsValid)
            {
                db.Entry(software).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Categories = new SelectList(GetCategories(), software.software_category);
            return View(software);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            SoftwareModel software=db.Software.Find(id);
            if(software!=null && software.assigned_device==null)
            {
                db.Software.Remove(software);
                db.SaveChanges();
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