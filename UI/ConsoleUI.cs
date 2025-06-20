using System;

namespace PasswordManager.UI
{
    public class ConsoleUI
    {
        public void Start()
        {
            bool running = true;
            while (running)
            {
                ShowMainMenu();
                var choice = Console.ReadLine();
                
                // super basic menu for now, will add more stuff later!
                switch (choice)
                {
                    case "1":
                        Console.WriteLine("\nView All Passwords (coming soon!)");
                        PressAnyKey();
                        break;
                    case "2":
                        Console.WriteLine("\nAdd New Password (coming soon!)");
                        PressAnyKey();
                        break;
                    case "3":
                        Console.WriteLine("\nSearch Passwords (coming soon!)");
                        PressAnyKey();
                        break;
                    case "4":
                        Console.WriteLine("\nGenerate Password (coming soon!)");
                        PressAnyKey();
                        break;
                    case "5":
                        Console.WriteLine("\nChange Master Password (coming soon!)");
                        PressAnyKey();
                        break;
                    case "6":
                        running = false;
                        break;
                    default:
                        Console.WriteLine("\nOops! Invalid choice, try again!");
                        PressAnyKey();
                        break;
                }
            }
            
            Console.WriteLine("\nThanks for using my password manager! Bye!");
        }

        private void ShowMainMenu()
        {
            Console.Clear();
            Console.WriteLine("=== My Password Manager ===");
            Console.WriteLine("1. View All Passwords");
            Console.WriteLine("2. Add New Password");
            Console.WriteLine("3. Search Passwords");
            Console.WriteLine("4. Generate Password");
            Console.WriteLine("5. Change Master Password");
            Console.WriteLine("6. Exit");
            Console.Write("\nChoose an option (1-6): ");
        }

        private void PressAnyKey()
        {
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
    }
}