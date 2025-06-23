using PasswordManager.Services;
using System.Text;

namespace PasswordManager.UI
{
    public class ConsoleUI
    {
        private readonly DatabaseService _database;
        private readonly MasterPasswordService _masterPassword;
        bool _isLoggedIn = false;
        private readonly EncryptionService _encryption;

        public ConsoleUI()
        {
            _database = new DatabaseService();
            _masterPassword = new MasterPasswordService(_database);

            var config = new ConfigurationService();
            var key = config.GetEncryptionKey();
            var iv = config.GetEncryptionIV();
            _encryption = new EncryptionService(key, iv);
        }

        public void Start()
        {
            if (!_database.InitializeDatabase())
            {
                Console.WriteLine("db didn't start, check ur Database!");
                return;
            }

            if (!HandleMasterPassword())
            {
                return;
            }

            RunMainMenu();
        }

        bool HandleMasterPassword()
        {
            if (!_masterPassword.IsMasterPasswordSet())
            {
                Console.Clear();
                Console.WriteLine("=== Welcome to Password Manager! ===");
                Console.WriteLine("looks like ur new here!");
                Console.WriteLine("lets set ur master password.");

                while (true)
                {
                    Console.Write("\nEnter your master password: ");
                    string password = ReadPassword();

                    if (password.Length < 8)
                    {
                        Console.WriteLine("password too short, try again");
                        continue;
                    }

                    Console.Write("Confirm your password: ");
                    string confirm = ReadPassword();

                    if (password != confirm)
                    {
                        Console.WriteLine("not the same, try again");
                        continue;
                    }

                    if (_masterPassword.SetMasterPassword(password))
                    {
                        Console.WriteLine("\nMaster password set successfully!");
                        _isLoggedIn = true;
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("couldn't set password, try again?");
                    }
                }
            }
            else
            {
                int attempts = 3;
                while (attempts > 0)
                {
                    Console.Clear();
                    Console.WriteLine("=== Password Manager Login ===");
                    Console.Write("\nEnter master password: ");
                    string password = ReadPassword();

                    if (_masterPassword.VerifyMasterPassword(password))
                    {
                        _isLoggedIn = true;
                        return true;
                    }

                    attempts--;
                    if (attempts > 0)
                    {
                        Console.WriteLine($"\nwrong password! {attempts} left.");
                        Console.WriteLine("press any key to try again...");
                        Console.ReadKey();
                    }
                }

                Console.WriteLine("\nToo many failed attempts. Try again later!");
                return false;
            }
        }

        void RunMainMenu()
        {
            bool running = true;
            while (running)
            {
                ShowMainMenu();
                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        ViewAllPasswords();
                        break;
                    case "2":
                        AddNewPassword();
                        break;
                    case "3":
                        SearchPasswords();
                        break;
                    case "4":
                        GeneratePassword();
                        break;
                    case "5":
                        UpdateOrDeletePassword();
                        break;
                    case "6":
                        HandleChangeMasterPassword();
                        break;
                    case "7":
                        CheckPasswordStrength();
                        break;
                    case "8":
                        BackupPasswords();
                        break;
                    case "9":
                        running = false;
                        break;
                    default:
                        Console.WriteLine("\nuhh, not a choice. try again!");
                        PressAnyKey();
                        break;
                }
            }

            Console.WriteLine("\nthanks for using my password manager! bye!"); 
        }

        void ShowMainMenu()
        {
            Console.Clear();
            Console.WriteLine("=== My Password Manager ===");
            Console.WriteLine("1. View All Passwords");
            Console.WriteLine("2. Add New Password");
            Console.WriteLine("3. Search Passwords");
            Console.WriteLine("4. Generate Password");
            Console.WriteLine("5. Update/Delete Password");
            Console.WriteLine("6. Change Master Password");
            Console.WriteLine("7. Check Password Strength");
            Console.WriteLine("8. Backup Passwords");
            Console.WriteLine("9. Exit");
            Console.Write("\nChoose an option (1-9): ");
        }

        void PressAnyKey()
        {
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        static string ReadPassword()
        {
            StringBuilder password = new StringBuilder();
            ConsoleKeyInfo key;

            while (true)
            {
                key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (password.Length > 0)
                    {
                        password.Length--;
                        Console.Write("\b \b");
                    }
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    password.Append(key.KeyChar);
                    Console.Write("*");
                }
            }
            return password.ToString();
        }

        void ViewAllPasswords()
        {
            Console.Clear();
            Console.WriteLine("=== All Stored Passwords ===\n");
            try
            {
                var passwords = _database.GetAllPasswords();
                if (passwords.Count == 0)
                {
                    Console.WriteLine("No passwords stored yet.");
                }
                else
                {
                    foreach (var p in passwords)
                    {
                        Console.WriteLine($"Website: {p.Website}");
                        Console.WriteLine($"userName: {p.userName}");
                        Console.WriteLine($"Password: {_encryption.Decrypt(p.EncryptedPwd)}");
                        if (!string.IsNullOrWhiteSpace(p.Notes))
                            Console.WriteLine($"Notes: {p.Notes}");
                        Console.WriteLine($"Last Modified: {p.LastModified}");
                        Console.WriteLine(new string('-', 30));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving passwords: {ex.Message}");
            }
            PressAnyKey();
        }

        void AddNewPassword()
        {
            Console.Clear();
            Console.WriteLine("=== Add New Password ===\n");
            try
            {
                // hope this works
                Console.Write("Website: ");
                string site = Console.ReadLine();
                Console.Write("userName: ");
                string uname = Console.ReadLine();
                Console.Write("Password: ");
                string pwd = ReadPassword();
                Console.Write("Notes (optional): ");
                string notes = Console.ReadLine();

                var pw = new Models.Password
                {
                    Website = site,
                    userName = uname,
                    EncryptedPwd = _encryption.Encrypt(pwd),
                    Notes = string.IsNullOrWhiteSpace(notes) ? null : notes
                };

                if (_database.AddPassword(pw))
                {
                    Console.WriteLine("\nsaved!"); 
                }
                else
                {
                    Console.WriteLine("\ncouldn't save password :(");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving password: {ex.Message}");
            }
            PressAnyKey();
        }

        void SearchPasswords()
        {
            Console.Clear();
            Console.WriteLine("=== Search Passwords ===\n");
            Console.Write("Enter website or username to search: ");
            string query = Console.ReadLine()?.Trim().ToLower();

            try
            {
                var passwords = _database.GetAllPasswords();
                var results = passwords.FindAll(p =>
                    (!string.IsNullOrEmpty(p.Website) && p.Website.ToLower().Contains(query)) ||
                    (!string.IsNullOrEmpty(p.userName) && p.userName.ToLower().Contains(query))
                );

                if (results.Count == 0)
                {
                    Console.WriteLine("no matches, sorry.");
                }
                else
                {
                    foreach (var p in results)
                    {
                        Console.WriteLine($"Website: {p.Website}");
                        Console.WriteLine($"userName: {p.userName}");
                        Console.WriteLine($"Password: {_encryption.Decrypt(p.EncryptedPwd)}");
                        if (!string.IsNullOrWhiteSpace(p.Notes))
                            Console.WriteLine($"Notes: {p.Notes}");
                        Console.WriteLine($"Last Modified: {p.LastModified}");
                        Console.WriteLine(new string('-', 30));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching passwords: {ex.Message}");
            }
            PressAnyKey();
        }

        void UpdateOrDeletePassword()
        {
            Console.Clear();
            Console.WriteLine("=== Update/Delete Password ===\n");
            try
            {
                var passwords = _database.GetAllPasswords();
                if (passwords.Count == 0)
                {
                    Console.WriteLine("No passwords stored.");
                    PressAnyKey();
                    return;
                }

                for (int i = 0; i < passwords.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {passwords[i].Website} ({passwords[i].userName})");
                }
                Console.Write("\nSelect a password by number: ");
                if (!int.TryParse(Console.ReadLine(), out int idx) || idx < 1 || idx > passwords.Count)
                {
                    Console.WriteLine("Invalid selection.");
                    PressAnyKey();
                    return;
                }
                var selected = passwords[idx - 1];

                Console.WriteLine("\n1. Update\n2. Delete\n3. Cancel");
                Console.Write("Choose an action: ");
                var action = Console.ReadLine();
                if (action == "1")
                {
                    Console.Write("New Website (leave blank to keep): ");
                    string website = Console.ReadLine();
                    Console.Write("New userName (leave blank to keep): ");
                    string username = Console.ReadLine();
                    Console.Write("New Password (leave blank to keep): ");
                    string password = ReadPassword();
                    Console.Write("New Notes (leave blank to keep): ");
                    string notes = Console.ReadLine();

                    if (!string.IsNullOrWhiteSpace(website)) selected.Website = website;
                    if (!string.IsNullOrWhiteSpace(username)) selected.userName = username;
                    if (!string.IsNullOrWhiteSpace(password)) selected.EncryptedPwd = _encryption.Encrypt(password);
                    if (!string.IsNullOrWhiteSpace(notes)) selected.Notes = notes;

                    if (_database.UpdatePassword(selected))
                    {
                        Console.WriteLine("updated!");
                    }
                    else
                    {
                        Console.WriteLine("couldn't update password");
                    }
                }
                else if (action == "2")
                {
                    if (_database.DeletePassword(selected.Id))
                    {
                        Console.WriteLine("deleted!");
                    }
                    else
                    {
                        Console.WriteLine("couldn't delete password");
                    }
                }
                else
                {
                    Console.WriteLine("cancelled, i guess.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating/deleting password: {ex.Message}");
            }
            PressAnyKey();
        }

        void CheckPasswordStrength()
        {
            Console.Clear();
            Console.WriteLine("=== Password Strength Checker ===\n");
            Console.Write("Enter a password to check: ");
            string password = ReadPassword();

            int score = 0;
            if (password.Length >= 8) score++;
            if (System.Text.RegularExpressions.Regex.IsMatch(password, "[A-Z]")) score++;
            if (System.Text.RegularExpressions.Regex.IsMatch(password, "[a-z]")) score++;
            if (System.Text.RegularExpressions.Regex.IsMatch(password, "[0-9]")) score++;
            if (System.Text.RegularExpressions.Regex.IsMatch(password, "[^a-zA-Z0-9]")) score++;

            string strength = score switch
            {
                5 => "Very Strong",
                4 => "Strong",
                3 => "Medium",
                2 => "Weak",
                _ => "Very Weak"
            };

            Console.WriteLine($"\nPassword Strength: {strength}");
            PressAnyKey();
        }

        void BackupPasswords()
        {
            Console.Clear();
            Console.WriteLine("=== Backup Passwords ===\n");
            try
            {
                var passwords = _database.GetAllPasswords();
                if (passwords.Count == 0)
                {
                    Console.WriteLine("nothing to backup.");
                }
                else
                {
                    string fileName = $"passwords_backup_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    using (var writer = new System.IO.StreamWriter(fileName))
                    {
                        writer.WriteLine("Website,userName,Password,Notes,LastModified,Created_At");
                        foreach (var p in passwords)
                        {
                            string line = $"\"{p.Website}\",\"{p.userName}\",\"{p.EncryptedPwd}\",\"{p.Notes}\",\"{p.LastModified}\",\"{p.Created_At}\"";
                            writer.WriteLine(line);
                        }
                    }
                    Console.WriteLine($"backup done! file: {fileName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during backup: {ex.Message}");
            }
            PressAnyKey();
        }

        void GeneratePassword()
        {
            Console.Clear();
            Console.WriteLine("=== Password Generator ===\n");
            Console.Write("Enter desired password length (8-32): ");
            if (!int.TryParse(Console.ReadLine(), out int length) || length < 8 || length > 32)
            {
                Console.WriteLine("bad length, using 12.");
                length = 12;
            }

            string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()";
            var rnd = new Random();
            var sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                sb.Append(chars[rnd.Next(chars.Length)]);
            }
            string generated = sb.ToString();
            Console.WriteLine($"\ngenerated: {generated}");
            PressAnyKey();
        }

        void HandleChangeMasterPassword()
        {
            Console.Clear();
            Console.WriteLine("=== Change Master Password ===\n");

            Console.Write("Enter current master password: ");
            string currentPassword = ReadPassword();

            if (!_masterPassword.VerifyMasterPassword(currentPassword))
            {
                Console.WriteLine("Incorrect password!");
                PressAnyKey();
                return;
            }

            while (true)
            {
                Console.Write("\nEnter new master password: ");
                string newPassword = ReadPassword();

                if (newPassword.Length < 8)
                {
                    Console.WriteLine("Password must be at least 8 characters long!");
                    continue;
                }

                Console.Write("Confirm new password: ");
                string confirm = ReadPassword();

                if (newPassword != confirm)
                {
                    Console.WriteLine("Passwords don't match! Try again.");
                    continue;
                }

                if (_masterPassword.SetMasterPassword(newPassword))
                {
                    Console.WriteLine("\nMaster password changed successfully!");
                    PressAnyKey();
                    return;
                }
                else
                {
                    Console.WriteLine("Failed to change master password. Try again!");
                    PressAnyKey();
                    return;
                }
            }
        }
    }
}