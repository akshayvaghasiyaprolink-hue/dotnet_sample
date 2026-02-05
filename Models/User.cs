using System.ComponentModel.DataAnnotations;

namespace noteapp.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        // Role: admin, staff, user (default = user)
        [Required]
        public string Role { get; set; } = "user";

        public List<Note> Notes { get; set; } = new List<Note>();
    }
}
