using System;
using System.ComponentModel.DataAnnotations;

namespace NoteApp.Models
{
    public enum Priority
    {
        High, Normal, Low
    }

    public class Note
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        public DateTime CreatedDate { get; set; }
        [Required]
        public Priority Priority { get; set; }
        public string Comments { get; set; }

        public ApplicationUser User { get; set; }
    }
}
