using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using InzV3.Models;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using System.Runtime.Remoting.Messaging;
using Microsoft.Ajax.Utilities;

namespace InzV3.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // Tu zaczyna się sekcja kodu dotycząca zwykłego użytkownika
        public ActionResult MyTickets()
        {
            var userId = User.Identity.GetUserId();
            var tickets = db.Tickets.Include(t => t.Device).Include(t => t.Technician).Where(t => t.id_user == userId).OrderByDescending(t => t.createdAt).ToList();
            return View(tickets);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int id_device, string description)
        {
            var userId = User.Identity.GetUserId();

            if (!db.Devices.Any(d => d.id_device == id_device && d.id_user == userId))
            {
                return new HttpUnauthorizedResult();
            }

            var ticket = new TicketModel
            {
                id_device = id_device,
                description = description,
                status = TicketStatuses.Wyslane,
                createdAt = DateTime.Now,
                updatedAt = DateTime.Now,
                id_user = userId
            };
            db.Tickets.Add(ticket);
            db.SaveChanges();
            TempData["SuccessMessage"] = "Zgłoszenie zostało pomyślnie wysłane";
            return RedirectToAction("MyTickets");
        }

        // Elementy wspólne dla obu ról

        public ActionResult Details(int id)
        {
            var ticket = db.Tickets
                .Include(t => t.Technician)
                .Include(t => t.User)
                .Include(t => t.Device)
                .Include(t => t.Messages.Select(m => m.Sender)).FirstOrDefault(t => t.id_ticket == id);

            if (ticket == null) return HttpNotFound();
            var UserId = User.Identity.GetUserId();
            bool isTechOrAdmin = User.IsInRole("Pracownik Serwisu") || User.IsInRole("Admin");
            if (ticket.id_user != UserId && !isTechOrAdmin)
            {
                return new HttpUnauthorizedResult();
            }
            ViewBag.IsTechOrAdmin = isTechOrAdmin;
            ViewBag.CurrentUserId = UserId;
            ticket.Messages = ticket.Messages.OrderBy(m => m.createdAt).ToList();

            ViewBag.StatusList = new SelectList(new List<string>
            {
                TicketStatuses.Przyjete,
                TicketStatuses.OczekujeUser,
                TicketStatuses.OczekujeSerwis,
                TicketStatuses.IdzDoSerwisu,
                TicketStatuses.WRealizacji,
                TicketStatuses.DoOdbioru,
                TicketStatuses.Zamkniete
            }, ticket.status);
            return View(ticket);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendMessage(int id_ticket, string messageContent)
        {
            if (string.IsNullOrWhiteSpace(messageContent))
            {
                return RedirectToAction("Details", new { id = id_ticket });
            }
            var ticket = db.Tickets.Find(id_ticket);
            if (ticket == null || ticket.status == TicketStatuses.Zamkniete)
            {
                return HttpNotFound();
            }
            var userId = User.Identity.GetUserId();
            bool isTechOrAdmin = User.IsInRole("Pracownik Serwisu") || User.IsInRole("Admin");
            if (ticket.id_user != userId && !isTechOrAdmin)
            {
                return new HttpUnauthorizedResult();
            }
            var msg = new TicketMessageModel
            {
                id_ticket = id_ticket,
                messageContent = messageContent,
                createdAt = DateTime.Now,
                id_sender = userId
            };
            db.TicketMessages.Add(msg);

            /* jeżeli wiadomość została przez pracownika serwisu wysłana, to status sam zmieni się
             * na "Oczekuje na odpowiedź użytkownika"*/
            if (isTechOrAdmin)
            {
                ticket.status = TicketStatuses.OczekujeUser;
                if (ticket.id_technician != userId)
                {
                    ticket.id_technician = userId;
                }

            }

            // Jeśli to użytkownik odpisze, to status zmieni się na "Oczekuje na odpowiedź serwisu"
            else
            {
                ticket.status = TicketStatuses.OczekujeSerwis;
            }
            ticket.updatedAt = DateTime.Now;
            db.SaveChanges();
            return RedirectToAction("Details", new { id = id_ticket });
        }
        // Tu zaczyna się sekcja dotycząca tylko serwisanta

        [Authorize(Roles = "Pracownik Serwisu, Admin")]
        public ActionResult ManageTickets(string assignFilter, string sortOrder)
        {
            var userId = User.Identity.GetUserId();
            var tickets = db.Tickets.Include(t => t.User).Include(t => t.Device).Include(t => t.Technician).AsQueryable();

            if (assignFilter == "Moje")
            {
                tickets = tickets.Where(t => t.id_technician == userId);
            }
            else if (assignFilter == "Nieprzypisane")
            {
                tickets = tickets.Where(t => t.id_technician == null);
            }
            else if (assignFilter == "Przypisane do innych")
            {
                tickets = tickets.Where(t => t.id_technician != null && t.id_technician != userId);
            }
            else if (assignFilter == "Wszystkie")
            {
                tickets = tickets.Where(t => t.id_technician != null);
            }

            if (sortOrder == "Oldest")
            {
                tickets = tickets.OrderBy(t => t.createdAt);
            }
            else
            {
                tickets = tickets.OrderByDescending(t => t.createdAt);
            }

            ViewBag.CurrentFilter = assignFilter;
            ViewBag.CurrentSort = sortOrder;
            return View(tickets.ToList());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Pracownik Serwisu, Admin")]
        public ActionResult AssignToMe(int id_ticket)
        {
            var ticket = db.Tickets.Find(id_ticket);
            if (ticket != null && ticket.status != TicketStatuses.Zamkniete)
            {
                ticket.id_technician = User.Identity.GetUserId();
                ticket.status = TicketStatuses.Przyjete;
                ticket.updatedAt = DateTime.Now;
                db.SaveChanges();
            }


            return RedirectToAction("Details", new { id = id_ticket });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Pracownik Serwisu, Admin")]
        public ActionResult ChangeStatus(int id_ticket, string newStatus)
        {
            var ticket = db.Tickets.Find(id_ticket);
            if (ticket != null)
            {
                ticket.status = newStatus;
                ticket.updatedAt = DateTime.Now;
                db.SaveChanges();
            }
            return RedirectToAction("Details", new { id = id_ticket });
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