﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PurrfectMatch.Data;
using PurrfectMatch.Models;
using Microsoft.AspNetCore.Identity;

namespace PurrfectMatch.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly CatDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationController(CatDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();  
            }

            var notifications = await _context.Notifications
                .Where(n => n.UserId == user.Id)  // Sprawdzanie po ID użytkownika
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(notifications);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == id && n.UserId == user.Id);

                if (notification == null)
                {
                    return NotFound();
                }

                notification.IsRead = true;
                _context.Notifications.Update(notification);
                await _context.SaveChangesAsync();

                return Ok(); // Upewnij się, że zwrócisz status OK
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Wystąpił błąd podczas oznaczania powiadomienia.");
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var user = await _userManager.GetUserAsync(User);  // Pobranie użytkownika
            if (user == null)
            {
                return Json(new { message = "Brak zalogowanego użytkownika." });
            }

            var notifications = await _context.Notifications
                .Where(n => n.UserId == user.Id && !n.IsRead)  // Pobieranie powiadomień tylko dla użytkownika
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new { n.Id, n.Message, n.CreatedAt })
                .ToListAsync();

            if (notifications == null || notifications.Count == 0)
            {
                return Json(new { message = "Brak nowych powiadomień." });
            }

            return Json(notifications);
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadNotificationsCount()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                var unreadCount = await _context.Notifications
                    .Where(n => n.UserId == user.Id && !n.IsRead)
                    .CountAsync();

                return Json(new { count = unreadCount });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Wystąpił błąd podczas pobierania liczby nieprzeczytanych powiadomień.");
            }
        }
    }
}
