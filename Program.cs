using System;
using PasswordManager.UI;

namespace PasswordManager
{
    class Program
    {
        static void Main(string[] args)
        {
            // hey this is where everything starts! :)
            Console.WriteLine("Welcome to My Password Manager!");
            
            var ui = new ConsoleUI();
            ui.Start();  // starting the main menu loop
        }
    }
}
