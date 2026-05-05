using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using InzV3.Models;
using System.Runtime.Remoting.Contexts;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity;
namespace InzV3.Controllers
{
    [Authorize]
    public class UserDeviceController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: UserDevice
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Home");
            }
            string curentUserId = User.Identity.GetUserId();
            var device = db.Devices.Include(d => d.Charachteristics.Select(c => c.DevFeature))
                .SingleOrDefault(d => d.id_device == id && d.id_user == curentUserId);
            if (device == null)
            {
                return HttpNotFound("Nie masz uprawnień do przeglądania szczegółów tego urządzenia");
            }
            var user=db .Users.Find(curentUserId);
            var subRole=db.SubRoles.FirstOrDefault(s=>s.Name==user.SubRole);
            int interval = 30;
            if(subRole!= null) 
            {
                interval = subRole.RatingIntervalDays;
            }

            DateTime nextRatingDate=device.last_rated_at.HasValue
                ? device.last_rated_at.Value.AddDays(interval)
                : DateTime.MinValue; // to dla sytuacji kiedy sprzęt nie był nigdy oceniany

            // Wymuszanie oceny, kiedy minie odpowiedni czas od ostatniej ocen
            bool justRated = TempData["JustRated"] != null && (bool)TempData["JustRated"];
            if (justRated)
            {
                ViewBag.ForceRating = false;
            }
            else
            {
                ViewBag.ForceRating = DateTime.Now >= nextRatingDate;
            }    

            ViewBag.NextRatingDate = device.last_rated_at.HasValue ? device.last_rated_at.Value.AddDays(interval): (DateTime?)null;
            ViewBag.LastRatingDate = device.last_rated_at;

            ViewBag.Software = db.Software.Where(s => s.assigned_device == id).ToList();
            ViewBag.Ratings = db.DeviceRating.Where(r => r.id_device == id).ToList();
            return View(device);
        }   

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveRatings(FormCollection form)
        {
            int id_device = int.Parse(form["id_device"]);
            string currentUserId = User.Identity.GetUserId();
            var device = db.Devices.FirstOrDefault(d=>d.id_device==id_device && d.id_user==currentUserId);
            if (device == null)
            {
                return new HttpUnauthorizedResult();

            }
            var ratings = db.DeviceRating.Where(r => r.id_device == id_device).ToList();

            // Logika automatycznego zgłoszenia przy niskiej ocenie
            bool isLowRating = false;
            List<string>lowRatingDetails=new List<string>();
            foreach (var rating in ratings)
            {
                string val = form["rating_" + rating.id_rating];
                if (!string.IsNullOrEmpty(val))
                {
                    rating.rating_value = int.Parse(val);
                    if (rating.rating_value <= 1)
                    {
                        isLowRating = true;
                        lowRatingDetails.Add($"{rating.category_name}: {rating.rating_value}/5");
                    }
                }
            }

            device.last_rated_at = DateTime.Now;
            db.SaveChanges();
            TempData["JustRated"] = true;

            if (isLowRating)
            {
                bool hasOpenTicket = db.Tickets.Any(t => t.id_device == id_device && t.status != TicketStatuses.Zamkniete);
                if (!hasOpenTicket)
                {
                    var ticket = new TicketModel
                    {
                        id_device = id_device,
                        description = "Automatyczne zgłoszenie: niska ocena urządzenia\n" + string.Join("\n", lowRatingDetails),
                        status = TicketStatuses.Wyslane,
                        createdAt = DateTime.Now,
                        updatedAt = DateTime.Now,
                        id_user = currentUserId
                    };
                    db.Tickets.Add(ticket);
                    db.SaveChanges();
                    TempData["AutoTicketCreated"] = true;
                }
            }

            return RedirectToAction("Details", new { id = id_device });
        }
    }
}