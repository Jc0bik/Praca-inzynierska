using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using InzV3.Models;
using System.Globalization;

namespace InzV3.Controllers
{
    [Authorize(Roles = "Admin, Pracownik Serwisu")]
    public class ServiceController : Controller
    {
        private ApplicationDbContext context= new ApplicationDbContext();
        // GET: Service
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Users(string searchString, string roleFilter)
        {
            var usersQuery = context.Users.AsQueryable();
            var userRoles = context.Roles.ToList();

            //wyszukiwanie użytkowników z listy
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                usersQuery = usersQuery.Where(u =>
                    u.FirstName.ToLower().Contains(searchString) ||
                    u.LastName.ToLower().Contains(searchString) ||
                    u.Email.ToLower().Contains(searchString));
            }
            var userList = usersQuery.ToList().Select(u => new UserListViewModel
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Role = userRoles.FirstOrDefault(r => r.Id == u.Roles?.FirstOrDefault()?.RoleId)?.Name ?? "Brak",
                SubRole = u.SubRole
            }).ToList();
            if (!string.IsNullOrEmpty(roleFilter))
            {
                userList = userList.Where(u => u.Role == roleFilter).ToList();
            }
            ViewBag.Roles = new SelectList(userRoles.Select(r => r.Name).ToList());
            return View(userList);
        }
        public class UserViewModel
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string Role { get; set; }
            public string SubRole { get;set; }
        }
        protected override void Dispose(bool disposing)
        {
            if(disposing) context.Dispose();
            base.Dispose(disposing);
        }
    }
}