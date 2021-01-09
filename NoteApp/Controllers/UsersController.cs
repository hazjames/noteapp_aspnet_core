using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoteApp.Models;

namespace NoteApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // GET: /users/
        public async Task<IActionResult> Index()
        {
            return View(await _userManager.Users.ToListAsync());
        }

        // POST: /users/
        [HttpPost]
        public async Task<IActionResult> Index(string id, bool DisableLockout)
        {
            if (!string.IsNullOrEmpty(id) && DisableLockout)
            {
                var user = _userManager.FindByIdAsync(id).Result;
                await _userManager.SetLockoutEndDateAsync(user, DateTime.Now - TimeSpan.FromMinutes(1));
            }

            return View(await _userManager.Users.ToListAsync());
        }
    }
}
