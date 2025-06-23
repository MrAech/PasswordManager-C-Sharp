using DotNetEnv;

namespace PasswordManager.Services
{
    public class ConfigurationService
    {
        public ConfigurationService()
        {
            // config thingy, nothing to do here lol
        }

        public string GetConnectionString()
        {
            try
            {
                var server = Env.GetString("DB_SERVER");
                var port = Env.GetString("DB_PORT");
                var database = Env.GetString("DB_NAME");
                var user = Env.GetString("DB_USER");
                var password = Env.GetString("DB_PASSWORD");

                // build the connection string from env vars 
                return $"Server={server};" +
                       $"Port={port};" +
                       $"Database={database};" +
                       $"User={user};" +
                       $"Password={password};";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"couldn't get db config: {ex.Message}"); // TODO: Apply fixes for the Errors if it happens
                throw; // can't do anything without db config
            }
        }

        public string GetEncryptionKey()
        {
            var key = Env.GetString("ENCRYPTION_KEY");
            if (string.IsNullOrEmpty(key) || key.Length != 16)
                throw new Exception("ENCRYPTION_KEY must be 16 chars, check ur .env");
            return key;
        }

        public string GetEncryptionIV()
        {
            var iv = Env.GetString("ENCRYPTION_IV");
            if (string.IsNullOrEmpty(iv) || iv.Length != 16)
                throw new Exception("ENCRYPTION_IV must be 16 chars, check ur .env");
            return iv;// _Trust me im a doctor_ (just imagine it being in italic)
        }
    }
}