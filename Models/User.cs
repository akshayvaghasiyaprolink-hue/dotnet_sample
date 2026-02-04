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

        public List<Note> Notes { get; set; } = new List<Note>();
    }
}
