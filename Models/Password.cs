using System;

namespace PasswordManager.Models
{
    public class Password
    {
        public int Id { get; set; }
        public string? Website { get; set; }
        public string? userName { get; set; }  
        public string? EncryptedPwd { get; set; }
        public string? Notes { get; set; }
        public DateTime LastModified { get; set; }
        public DateTime Created_At { get; set; }


        public Password()
        {
            // just set these to now, should be fine "famous last words"
            LastModified = DateTime.Now;
            Created_At = DateTime.Now;
        }
    }
}