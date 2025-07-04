﻿// using PasswordManager.UI;
//
// namespace PasswordManager
// {
//     class Program
//     {
//         static void Main(string[] args)
//         {
//             try
//             {
//                 // starting up
//                 Console.WriteLine("Starting Password Manager...\n");
//
//                 // Load env vars (should work, if not idk)
//                 try
//                 {
//                     DotNetEnv.Env.Load();
//                     Console.WriteLine("Loaded env vars!"); 
//                 }
//                 catch (Exception ex)
//                 {
//                     Console.WriteLine($"couldn't load .env file: {ex.Message}"); // TODO: check this later
//                 }
//
//                 var ui = new ConsoleUI();
//                 ui.Start();
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine("\n=== Uh oh, something broke! ===");
//                 Console.WriteLine($"err: {ex.Message}");
//                 Console.WriteLine("\npress any key to quit...");
//                 Console.ReadKey();
//             }
//         }
//     }
// }


using DotNetEnv;
using PasswordManager.UI;
namespace PasswordManager;


///<summary>
/// This Below is For WindowsUI
///</summary>
static class Program
{
    
    // The New Main
    [STAThread]
    static void Main()
    {
        
        // Load Environment Vars
        Env.Load();

        ApplicationConfiguration.Initialize();
        Application.Run((new WindowsUI()));
    }
}

