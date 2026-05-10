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
            var usersQuery = context.Users.Include(u=>u.SubRole).AsQueryable();
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
                SubRoleId = u.SubRoleId,
                SubRoleName=u.SubRole!= null ? u.SubRole.Name : "Brak"
            }).ToList();
            if (!string.IsNullOrEmpty(roleFilter))
            {
                userList = userList.Where(u => u.Role == roleFilter).ToList();
            }
            ViewBag.Roles = new SelectList(userRoles.Select(r => r.Name).ToList());
            return View(userList);
        }
        protected override void Dispose(bool disposing)
        {
            if(disposing) context.Dispose();
            base.Dispose(disposing);
        }
    }
}