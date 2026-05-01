using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using InzV3.Models;
using Microsoft.AspNet.Identity;
namespace InzV3.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var user_id = User.Identity.GetUserId();
            var user = db.Users.FirstOrDefault(u => u.Id == user_id);
            var userDevices = db.Devices.Where(d => d.id_user == user_id)
                .Include(d=>d.Charachteristics.Select(c=>c.DevCategory)).ToList();
            ViewBag.UserDevices = userDevices;
            return View(user);
        }
        public ActionResult MyDevices()
        {
            var user_id = User.Identity.GetUserId();
            var userDevices = db.Devices.Where(d => d.id_user == user_id)
                .Include(d => d.Charachteristics.Select(c => c.DevCategory)).ToList();
                
                
                return View(userDevices);
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