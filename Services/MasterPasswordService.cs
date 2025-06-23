namespace PasswordManager.Services
{
    public class MasterPasswordService
    {
        private readonly DatabaseService _database;

        public MasterPasswordService(DatabaseService database)
        {
            _database = database;

        }

        public bool SetMasterPassword(string password)
        {
            try
            {
                // Generate a salt and hash the password
                // salt huh i have a stratigic advantage here (if u don't get it read World History)
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12)); // secure enough ?? idk 
                
                // Save the hashed password to database
                return _database.SaveMasterPasswordHash(hashedPassword);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"couldn't set master password: {ex.Message}"); // TODO: fix if this happens (P.S. nope )
                return false;
            }
        }

        public bool VerifyMasterPassword(string password)
        {
            try
            {
                // Get the stored hash
                string? storedHash = _database.GetMasterPasswordHash();
                
                if (string.IsNullOrEmpty(storedHash))
                {
                    Console.WriteLine("no master password set yet!"); // should set one
                    return false;
                }

                // Verify the password against the stored hash
                return BCrypt.Net.BCrypt.Verify(password, storedHash);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"couldn't verify master password: {ex.Message}");
                return false;
            }
        }

        public bool IsMasterPasswordSet()
        {
            string? hash = _database.GetMasterPasswordHash();
            // just check if hash exists
            return !string.IsNullOrEmpty(hash);
        }
    }
}