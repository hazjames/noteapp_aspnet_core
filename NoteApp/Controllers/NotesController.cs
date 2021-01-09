using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoteApp.Data;
using NoteApp.Models;

namespace NoteApp.Controllers
{
    [Authorize]
    public class NotesController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly UserManager<ApplicationUser> _userManager;

        public NotesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Notes
        public async Task<IActionResult> Index(string viewAll)
        {
            ViewData["ViewAll"] = viewAll;
            var notes = _context.Notes
                .Include(n => n.User);

            IOrderedQueryable<Note> orderedNotes;
            if (viewAll == "all" && User.IsInRole("Admin"))
            {
                orderedNotes = notes.OrderBy(n => n.User.UserName);
            }
            else
            {
                var user = await _userManager.GetUserAsync(User);
                orderedNotes = notes.Where(n => n.User.Equals(user))
                    .OrderBy(n => n.Priority)
                    .ThenBy(n => EF.Property<object>(n, "CreatedDate"));
            }
            return View(await orderedNotes.ToListAsync());
        }

        // POST: Notes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([Bind("Title")] Note note)
        {
            if (!String.IsNullOrEmpty(note.Title))
            {
                note.CreatedDate = DateTime.Now;
                note.Priority = Priority.Normal;
                note.Comments = "";
                note.User = await _userManager.GetUserAsync(User);
                _context.Add(note);
                await _context.SaveChangesAsync();
            }

            return View(await _context.Notes.ToListAsync());
        }

        // GET: Notes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Notes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Priority,Comments")] Note note)
        {
            if (ModelState.IsValid)
            {
                note.CreatedDate = DateTime.Now;
                note.User = await _userManager.GetUserAsync(User);
                _context.Add(note);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(note);
        }

        // GET: Notes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var note = await _context.Notes.FindAsync(id);
            if (note == null)
            {
                return NotFound();
            }

            if (await _userManager.GetUserAsync(User) != note.User && !User.IsInRole("Admin"))
            {
                return RedirectToAction(nameof(Index));
            }

            return View(note);
        }

        // POST: Notes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var noteToUpdate = await _context.Notes.FirstOrDefaultAsync(n => n.Id == id);

            string adminView = "";

            if (await _userManager.GetUserAsync(User) != noteToUpdate.User)
            {
                if (User.IsInRole("Admin"))
                {
                    adminView = "all";
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            if (await TryUpdateModelAsync<Note>(
                noteToUpdate,
                "",
                n => n.Title, n => n.Priority, n => n.Comments))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index), new { viewAll = adminView });
                }
                catch (DbUpdateException /* ex */)
                {
                    // ** Log the Error Here (Uncomment ex, etc.) **
                    ModelState.AddModelError("", "Unable to save changes. " +
                        "Please try again.");
                }
            }
            return View(noteToUpdate);
        }

        // GET: Notes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var note = await _context.Notes.FindAsync(id);

            string adminView = "";

            if (await _userManager.GetUserAsync(User) != note.User)
            {
                if (User.IsInRole("Admin"))
                {
                    adminView = "all";
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { viewAll = adminView });
        }

        private bool NoteExists(int id)
        {
            return _context.Notes.Any(e => e.Id == id);
        }
    }
}
