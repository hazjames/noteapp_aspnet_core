using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace NoteApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<Note> Notes { get; set; }
    }
}
