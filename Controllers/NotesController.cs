using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using noteapp.Data;
using noteapp.DTOs;
using noteapp.Models;
using System.Security.Claims;

namespace noteapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotesController(AppDbContext context)
        {
            _context = context;
        }

        // Token se UserId nikalna
        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }

        // ✅ Create Note (token wale user ke liye)
        [HttpPost]
        public IActionResult CreateNote(NoteCreateDto dto)
        {
            int userId = GetUserId();

            var note = new Note
            {
                Title = dto.Title,
                Content = dto.Content,
                UserId = userId
            };

            _context.Notes.Add(note);
            _context.SaveChanges();

            return Ok(new NoteDto
            {
                Id = note.Id,
                Title = note.Title,
                Content = note.Content
            });
        }

        // ✅ Get all notes of logged-in user
        [HttpGet]
        public IActionResult GetMyNotes()
        {
            int userId = GetUserId();

            var notes = _context.Notes
                .Where(n => n.UserId == userId)
                .Select(n => new NoteDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Content = n.Content
                })
                .ToList();

            return Ok(notes);
        }

        // ✅ Update note (only if belongs to logged in user)
[HttpPut("{id}")]
public IActionResult UpdateNote(int id, NoteCreateDto dto)
{
    int userId = GetUserId();

    var note = _context.Notes.FirstOrDefault(n => n.Id == id && n.UserId == userId);

    if (note == null)
        return NotFound("Note not found or you are not owner");

    note.Title = dto.Title;
    note.Content = dto.Content;

    _context.SaveChanges();

    return Ok(new NoteDto
    {
        Id = note.Id,
        Title = note.Title,
        Content = note.Content
    });
}

        // ✅ Delete note (only if belongs to logged in user)
        [HttpDelete("{id}")]
        public IActionResult DeleteNote(int id)
        {
            int userId = GetUserId();

            var note = _context.Notes.FirstOrDefault(n => n.Id == id && n.UserId == userId);

            if (note == null)
                return NotFound("Note not found or you are not owner");

            _context.Notes.Remove(note);
            _context.SaveChanges();

            return Ok("Note deleted");
        }
    }
}
