using InzV3.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace InzV3.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Users(string searchString, string roleFilter)
        {
            var usersQuery = db.Users.Include(u => u.SubRole).AsQueryable();
            

            //wyszukiwanie użytkowników z listy
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                usersQuery = usersQuery.Where(u =>
                    u.FirstName.ToLower().Contains(searchString) ||
                    u.LastName.ToLower().Contains(searchString) ||
                    u.Email.ToLower().Contains(searchString));
            }
            var filteredUsers = usersQuery.ToList();
            // AsnoTracking dla zwiekszenia wydajości, czyli samo odczytanie
            var userRoles = db.Roles.AsNoTracking().ToList();
            var userList = filteredUsers.Select(u => new UserListViewModel
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Role = userRoles.FirstOrDefault(r => r.Id == u.Roles?.FirstOrDefault()?.RoleId)?.Name ?? "Brak",
                SubRoleId = u.SubRoleId,
                SubRoleName = u.SubRole != null ? u.SubRole.Name : "Brak"
            }).ToList();

            if (!string.IsNullOrEmpty(roleFilter))
            {
                userList = userList.Where(u => u.Role == roleFilter).ToList();
            }
            ViewBag.Users = userList;
            ViewBag.Roles = new SelectList(userRoles.Select(r => r.Name).ToList());
            return View();
        }

        //GET: Admin/   
        public ActionResult EditUser(string id)
        {
            var user = db.Users.Find(id);
            if (user == null) return RedirectToAction("Users");
            var userRoles=user.Roles.FirstOrDefault();
            var roleName= userRoles != null ? db.Roles.Find(userRoles.RoleId)?.Name : "Brak";
            var model = new EditUserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = roleName,
                SubRoleId = user.SubRoleId
            };
            ViewBag.AllRoles= new SelectList(db.Roles.Select(r => r.Name).ToList());
            return View(model);
        }
        // POST: Admin/EditUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUser(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.Find(model.Id);
                if (user != null)
                {
                    var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
                    var currentRoles = userManager.GetRoles(user.Id);
                    var currentRoleName = currentRoles.FirstOrDefault();
                    if (currentRoleName != model.Role)
                    {
                        userManager.RemoveFromRoles(user.Id, currentRoles.ToArray());
                        if (!string.IsNullOrEmpty(model.Role) && model.Role != "Brak")
                        {
                            userManager.AddToRole(user.Id, model.Role);
                        }
                        user.SubRoleId = model.SubRoleId;
                    }
                    else
                    {
                        user.SubRoleId = model.SubRoleId;
                    }   
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Zmieniono dane użytkownika";
                    return RedirectToAction("Users");
                }
            }
            ViewBag.AllRoles = new SelectList(db.Roles.Select(r => r.Name).ToList());
            return View(model);
        }
        // POST: Admin/DeleteUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteUser(string id)
        {
            var user = db.Users.Find(id);
            if (user != null)
            {
                // Warunek blokujący usuwanie jeśli użytkownik ma do siebie przypisane urządzenia
                bool hasDevices= db.Devices.Any(d => d.id_user == id);
                bool hasTickets = db.Tickets.Any(t => t.id_user == id || t.id_technician == id);
                if (hasDevices)
                {
                    TempData["ErrorMessage"] = "Nie można usunąć użytkownika, ponieważ ma przypisane urządzenia lub otwarte zgłoszenie";
                    return RedirectToAction("Users");
                }
                db.Users.Remove(user);
                db.SaveChanges();
                TempData["SuccessMessage"]="Usunięto użytkownika";
            }
            return RedirectToAction("Users");
        }
        [HttpGet]
        public JsonResult GetSubRolesByRole(string roleName)
        {
            var subRoles = db.SubRoles
                .Where(s => s.Role.Name == roleName)
                .Select(s => new { value = s.Id, text = s.Name })
                .ToList();
            return Json(subRoles, JsonRequestBehavior.AllowGet);
        }
    }
    public class UserListViewModel
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public int? SubRoleId { get; set; }
        public string SubRoleName { get; set; }
    }
    public class EditUserViewModel
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public int? SubRoleId { get; set; }
    }
}

