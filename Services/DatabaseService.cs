using System;
using System.Collections.Generic;
using PasswordManager.Models;

namespace PasswordManager.Services
{
    public class DatabaseService
    {
        // TODO: Add real database connection tomorrow!
        private readonly string _connectionString;

        public DatabaseService()
        {
            // will add actual connection string when implementing DB stuff
            _connectionString = "Server=localhost...IDK";
        }

        // just placeholder methods for now...
        public bool SavePassword(Password password)
        {
            // will implement this with real DB code tomorrow!
            Console.WriteLine("Pretending to save password... :P");
            return true;
        }

        public List<Password> GetAllPasswords()
        {
            return new List<Password>();
        }

        public Password? GetPasswordById(int id)  
        {

            return null; 
        }

        private void TestConnection()
        {
            // adding this to use _connectionString and remove the warning
            Console.WriteLine($"Will test connection with: {_connectionString}");
        }
    }
}