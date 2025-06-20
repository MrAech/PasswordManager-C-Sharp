using System;

namespace PasswordManager.Models
{
    public class Password
    {
        public int Id { get; set; }
        public string? Website { get; set; } 
        public string? Username { get; set; }  
        public string? EncryptedPassword { get; set; }  
        public string? Notes { get; set; }  
        public DateTime LastModified { get; set; }
        public DateTime CreatedAt { get; set; }


        public Password()
        {
            LastModified = DateTime.Now;
            CreatedAt = DateTime.Now;
        }
    }
}