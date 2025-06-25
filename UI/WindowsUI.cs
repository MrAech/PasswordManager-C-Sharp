

using PasswordManager.Services;

namespace PasswordManager.UI
{
    public class WindowsUI : Form
    {
        private readonly DatabaseService _database;
        private readonly MasterPasswordService _masterPassword;
        private readonly EncryptionService? _encryption;

        public WindowsUI()
        {
            // Initialize services
            _database = new DatabaseService();
            
            // Ensure It initialize as it seems to be not doing it 
            if (!_database.InitializeDatabase())
            {
                MessageBox.Show("DB didn't start run InitializeDatabase manually", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            _masterPassword = new MasterPasswordService(_database);
            
            var key = Environment.GetEnvironmentVariable("ENCRYPTION_KEY");
            var iv = Environment.GetEnvironmentVariable("ENCRYPTION_IV");

            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(iv))
            {
                MessageBox.Show("Enc IV and Key not there make sure you set it up corretly and is 16 char long",
                    "Config Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            _encryption = new EncryptionService(key, iv);
            
            // Master Password Stuff
            if (!HandleMasterPassword())
            {
                Close();
                return;
            }
            
            // Setup UI 
            InitializeComponents();
        }

        private bool HandleMasterPassword()
        {
            if (!_masterPassword.IsMasterPasswordSet())
            {
                // Set new Master Password
                while (true)
                {
                    var pass = PromptForPassword("Set Master Password", "Enter new master password:");
                    if (pass == null) return false;
                    if (pass.Length < 8)
                    {
                        MessageBox.Show("Password too short, must be 8 char min.", "Error", MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        continue;
                    }
                    var conf = PromptForPassword("Set Master Password", "Confirm master password:");
                    if(conf == null) return false;
                    if (pass != conf)
                    {
                        MessageBox.Show("Enter Same password not different", "Error", MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        continue;
                    }

                    if (_masterPassword.SetMasterPassword(pass))
                    {
                        MessageBox.Show("MasterPassword Set Succesfully", "success", MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("ohh umm sorry something didnt work could not set master password", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                // Verify Existing Password
                int attempts = 3;
                while (attempts > 0)
                {
                    var pass = PromptForPassword("Master Password Required", "Enter master password:");
                    if (pass == null) return false;
                    if (_masterPassword.VerifyMasterPassword(pass))
                    {
                        return true;
                    }
                    attempts--; // succeed did they not hmmmmmm
                    if (attempts > 0)
                    {
                        MessageBox.Show($"Incorrect You Shall Not Pass, {attempts} attempt(s) left", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        
                    }
                }

                MessageBox.Show("YOU SHALL NOT PASSSS.... see yah", "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        private string? PromptForPassword(string title, string text)
        {
            using(Form form = new Form())
            using (Label label = new Label())
            using (TextBox textbox = new TextBox())
            using (Button buttonOk = new Button())
            using (Button buttonCancel = new Button())
            {
                form.Text = title;
                label.Text = text;
                textbox.PasswordChar = '*';
                textbox.Width = 250;
                
                buttonOk.Text = "OK";
                buttonCancel.Text = "Cancel";
                buttonOk.DialogResult = DialogResult.OK;
                buttonCancel.DialogResult = DialogResult.Cancel;

                label.SetBounds(9, 20, 372, 13);
                textbox.SetBounds(12, 50, 372, 20);
                buttonOk.SetBounds(228, 80, 75, 23);
                buttonCancel.SetBounds(309, 80, 75, 23);
                
                label.AutoSize = true;
                form.ClientSize = new System.Drawing.Size(396, 120);
                form.Controls.AddRange(new Control[] { label, textbox, buttonOk, buttonCancel });
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.AcceptButton = buttonOk;
                form.CancelButton = buttonCancel;
                
                var dialogResult = form.ShowDialog();
                if (dialogResult == DialogResult.OK)
                {
                    return textbox.Text;
                }
                else
                {
                    return null;
                }
            }
            
        }

        private void InitializeComponents()
        {
            Text = "Aech's Password Manager :)";
            Width = 800;
            Height = 600;

            var mainMenuLabel = new Label
            {
                Text = "Main Menu",
                Font = FontLoader.GetPacificoFont(14),
                Location = new System.Drawing.Point(350, 20),
                AutoSize = true
            };
            Controls.Add(mainMenuLabel);

            int startY = 80;
            int spacing = 45;
            int btnWidth = 250;
            int btnX = 275;
            int idx = 0;
            
            var buttons = new[]
            {
                new { Text = "View All Passwords", Handler = (EventHandler)((s,e)=>ViewAllPasswords()) },
                new { Text = "Add New Password", Handler = (EventHandler)((s,e)=>AddNewPassword()) },
                new { Text = "Search Passwords", Handler = (EventHandler)((s,e)=>SearchPasswords()) },
                new { Text = "Generate Password", Handler = (EventHandler)((s,e)=>GeneratePassword()) },
                new { Text = "Update/Delete Password", Handler = (EventHandler)((s,e)=>UpdateOrDeletePassword()) },
                new { Text = "Change Master Password", Handler = (EventHandler)((s,e)=>ChangeMasterPassword()) },
                new { Text = "Check Password Strength", Handler = (EventHandler)((s,e)=>CheckPasswordStrength()) },
                new { Text = "Backup Passwords", Handler = (EventHandler)((s,e)=>BackupPasswords()) },
                new { Text = "Exit", Handler = (EventHandler)((s,e)=>this.Close()) }
            };

            foreach (var btn in buttons)
            {
                var button = new Button
                {
                    Text = btn.Text,
                    Font = FontLoader.GetPacificoFont(12),
                    Location = new System.Drawing.Point(btnX, startY + idx * spacing),
                    Width = btnWidth,
                    Height = 40
                };
                button.Click += btn.Handler;
                Controls.Add(button);
                idx++;
            }
        }
        
        
        // Button Handlers

        private void ViewAllPasswords()
        {
            try
            {
                var passwords = _database.GetAllPasswords();
                if (passwords.Count == 0)
                {
                    MessageBox.Show("First add some Passwords", "info", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                // Create New form with DataGrid if there are Passwords

                Form gridForm = new Form();
                gridForm.Text = "Stored Passwords";
                gridForm.Width = 1200;
                gridForm.Height = 500;

                DataGridView grid = new DataGridView();
                grid.Dock = DockStyle.Fill;
                grid.ReadOnly = true;
                grid.AllowUserToAddRows = false;
                grid.AllowUserToDeleteRows = false;
                grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                var colWebsite = new DataGridViewTextBoxColumn
                {
                    Name = "Website", HeaderText = "Website",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells, MaxInputLength = 100, Width = 150
                };
                var colUsername = new DataGridViewTextBoxColumn
                {
                    Name = "Username", HeaderText = "Username",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells, MaxInputLength = 100, Width = 120
                };
                var colPassword = new DataGridViewTextBoxColumn
                {
                    Name = "Password", HeaderText = "Password",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells, MaxInputLength = 100, Width = 150
                };
                var colNotes = new DataGridViewTextBoxColumn
                {
                    Name = "Notes", HeaderText = "Notes", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                    MaxInputLength = 200, MinimumWidth = 120
                };
                var colLastModified = new DataGridViewTextBoxColumn
                {
                    Name = "LastModified", HeaderText = "Last Modified",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells, MaxInputLength = 100, Width = 150
                };

                grid.Columns.AddRange(new DataGridViewColumn[]
                    { colWebsite, colUsername, colPassword, colNotes, colLastModified });

                // Adding copy and show 

                var btnShowPwd = new DataGridViewButtonColumn
                {
                    HeaderText = "Show", Text = "Show", UseColumnTextForButtonValue = true, Name = "ShowBtn", Width = 70
                };
                var btnCopyPwd = new DataGridViewButtonColumn
                {
                    HeaderText = "Copy Password", Text = "Copy Password", UseColumnTextForButtonValue = true,
                    Name = "CopyPwdBtn", Width = 110
                };
                var btnCopyUser = new DataGridViewButtonColumn
                {
                    HeaderText = "Copy Username", Text = "Copy Username", UseColumnTextForButtonValue = true,
                    Name = "CopyUserBtn", Width = 110
                };
                grid.Columns.Add(btnCopyPwd);
                grid.Columns.Add(btnShowPwd);
                grid.Columns.Add(btnCopyUser);

                var decryptPasswords = passwords
                    .Select(p => _encryption != null ? _encryption.Decrypt(p.EncryptedPwd) : "N/A").ToList();

                for (int i = 0; i < passwords.Count; i++)
                {
                    var p = passwords[i];
                    grid.Rows.Add(
                        p.Website,
                        p.userName,
                        new string('*', Math.Max(decryptPasswords[i].Length, 6)),
                        p.Notes,
                        p.LastModified
                    );
                }

                grid.CellContentClick += (s, e) =>
                {
                    if (e.RowIndex < 0) return;
                    int idx = e.RowIndex;
                    if (e.ColumnIndex == grid.Columns["ShowBtn"].Index)
                    {
                        // Toggle show/hide password
                        var cell = grid.Rows[idx].Cells["Password"];
                        if (cell.Value is string val && val.All(c => c == '*'))
                        {
                            cell.Value = decryptPasswords[idx];
                        }
                        else
                        {
                            cell.Value = new string('*', Math.Max(decryptPasswords[idx].Length, 6));
                        }
                    }
                    else if (e.ColumnIndex == grid.Columns["CopyPwdBtn"].Index)
                    {
                        Clipboard.SetText(decryptPasswords[idx]);
                        MessageBox.Show("Password copied to clipboard.", "Copied", MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    else if (e.ColumnIndex == grid.Columns["CopyUserBtn"].Index)
                    {
                        Clipboard.SetText(passwords[idx].userName ?? "");
                        MessageBox.Show("Username copied to clipboard.", "Copied", MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                };

                // Set max form Size
                gridForm.MaximumSize = new System.Drawing.Size(1200, 700);

                gridForm.Controls.Add(grid);
                gridForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting passwords: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddNewPassword()
        {
            var addPasswordForm = new Form
            {
                Text = "Add New Password",
                Width = 400,
                Height = 300,
            };

            var siteLabel = new Label
                { Text = "Website:", Location = new System.Drawing.Point(20, 20), AutoSize = true };
            var siteTextBox = new TextBox 
                { Location = new System.Drawing.Point(100, 20), Width = 250 };

            var userLabel = new Label
                { Text = "Username:", Location = new System.Drawing.Point(20, 60), AutoSize = true };
            var userTextBox = new TextBox 
                { Location = new System.Drawing.Point(100, 60), Width = 250 };

            var passwordLabel = new Label
                { Text = "Password:", Location = new System.Drawing.Point(20, 100), AutoSize = true };
            var passwordTextBox = new TextBox
                { Location = new System.Drawing.Point(100, 100), Width = 250, PasswordChar = '*' };

            var notesLabel = new Label
                { Text = "Notes:", Location = new System.Drawing.Point(20, 140), AutoSize = true };
            var notesTextBox = new TextBox 
                { Location = new System.Drawing.Point(100, 140), Width = 250 };

            var saveButton = new Button 
                { Text = "Save", Location = new System.Drawing.Point(150, 200), Width = 150 };

            saveButton.Click += (s, e) =>
            {
                try
                {
                    var pw = new Models.Password
                    {
                        Website = siteTextBox.Text,
                        userName = userTextBox.Text,
                        EncryptedPwd = _encryption != null ? _encryption.Encrypt(passwordTextBox.Text) : string.Empty,
                        Notes = string.IsNullOrWhiteSpace(notesTextBox.Text) ? null : notesTextBox.Text
                    };

                    if (_database.AddPassword(pw))
                    {
                        MessageBox.Show("Password Saved Succesfully", "Saved", MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("ohh it failed : ) try again ", "Error", MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding password: {ex.Message}", "Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            };
            
            addPasswordForm.Controls.Add(siteLabel);
            addPasswordForm.Controls.Add(siteTextBox);
            addPasswordForm.Controls.Add(userLabel);
            addPasswordForm.Controls.Add(userTextBox);
            addPasswordForm.Controls.Add(passwordLabel);
            addPasswordForm.Controls.Add(passwordTextBox);
            addPasswordForm.Controls.Add(notesLabel);
            addPasswordForm.Controls.Add(notesTextBox);
            addPasswordForm.Controls.Add(saveButton);
            
            addPasswordForm.ShowDialog();
        }
        
        private void ShowLargeText(string title, string text)
        {
            Form form = new Form();
            form.Text = title;
            form.Width = 600;
            form.Height = 500;
            TextBox textBox = new TextBox();
            textBox.Multiline = true;
            textBox.ReadOnly = true;
            textBox.ScrollBars = ScrollBars.Vertical;
            textBox.Dock = DockStyle.Fill;
            textBox.Text = text;
            form.Controls.Add(textBox);
            form.ShowDialog();
        }

        private void SearchPasswords()
        {
            var input = Microsoft.VisualBasic.Interaction.InputBox("Enter website or username to search:",
                "search Password", "");
            if(string.IsNullOrWhiteSpace(input)) return;
            try
            {
                var passwords = _database.GetAllPasswords();
                var results = passwords.FindAll(p =>
                    (!string.IsNullOrEmpty(p.Website) && p.Website.ToLower().Contains(input.ToLower())) ||
                    (!string.IsNullOrEmpty(p.userName) && p.userName.ToLower().Contains(input.ToLower()))
                );
                if (results.Count == 0)
                {
                    MessageBox.Show("No matches found.", "Search Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    string result = string.Join("\n\n", results.Select(p =>
                        $"Website: {p.Website}\nUsername: {p.userName}\nPassword: {(_encryption != null ? _encryption.Decrypt(p.EncryptedPwd) : "N/A")}\nNotes: {p.Notes}\nLast Modified: {p.LastModified}"));
                    ShowLargeText("Search Results", result);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching passwords: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GeneratePassword()
        {
            var input = Microsoft.VisualBasic.Interaction.InputBox("Enter desired password length (8-32):", "Generate Password", "12");
            if (!int.TryParse(input, out int length) || length < 8 || length > 32)
            {
                MessageBox.Show("Invalid length. Using 12.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                length = 12;
            }
            string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()";
            var rnd = new Random();
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < length; i++)
                sb.Append(chars[rnd.Next(chars.Length)]);
            string generated = sb.ToString();
            Clipboard.SetText(generated);
            MessageBox.Show($"Generated: {generated}\n(Copied to clipboard)", "Password Generator", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void UpdateOrDeletePassword()
        {
            var passwords = _database.GetAllPasswords();
            if (passwords.Count == 0)
            {
                MessageBox.Show("No passwords here turn back.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var items = passwords.Select((p, i) => $"{i + 1}. {p.Website} ({p.userName})").ToArray();
            var input = Microsoft.VisualBasic.Interaction.InputBox("Select a password by number:\n" + string.Join("\n", items), "Update/Delete Password", "1");
            if (!int.TryParse(input, out int idx) || idx < 1 || idx > passwords.Count)
            {
                MessageBox.Show("Invalid selection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var selected = passwords[idx - 1];
            var action = MessageBox.Show("Update (Yes) or Delete (No)?", "Update/Delete", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            
            if (action == DialogResult.Yes)
            {
                // Update
                var website = Microsoft.VisualBasic.Interaction.InputBox("New Website (leave blank to keep):", "Update", selected.Website);
                var username = Microsoft.VisualBasic.Interaction.InputBox("New Username (leave blank to keep):", "Update", selected.userName);
                var password = Microsoft.VisualBasic.Interaction.InputBox("New Password (leave blank to keep):", "Update", "");
                var notes = Microsoft.VisualBasic.Interaction.InputBox("New Notes (leave blank to keep):", "Update", selected.Notes);
                if (!string.IsNullOrWhiteSpace(website)) selected.Website = website;
                if (!string.IsNullOrWhiteSpace(username)) selected.userName = username;
                if (!string.IsNullOrWhiteSpace(password) && _encryption != null) selected.EncryptedPwd = _encryption.Encrypt(password);
                if (!string.IsNullOrWhiteSpace(notes)) selected.Notes = notes;
                if (_database.UpdatePassword(selected))
                    MessageBox.Show("Updated!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show("Couldn't update password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (action == DialogResult.No)
            {
                // Delete
                if (_database.DeletePassword(selected.Id))
                    MessageBox.Show("Deleted!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show("Couldn't delete password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ChangeMasterPassword()
        {
            var current = PromptForPassword("Change Master Password, Please tell me you didnt forget", "Enter current master password:");
            if (current == null) return;
            if (!_masterPassword.VerifyMasterPassword(current))
            {
                MessageBox.Show("Incorrect password!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            while (true)
            {
                var newPass = PromptForPassword("Change Master Password", "Enter new master password:");
                if (newPass == null) return;
                if (newPass.Length < 8)
                {
                    MessageBox.Show("Password must be at least 8 characters long!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    continue;
                }
                var confirm = PromptForPassword("Change Master Password", "Confirm new password:");
                if (confirm == null) return;
                if (newPass != confirm)
                {
                    MessageBox.Show("Passwords don't match! Try again, this time enter the same please.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    continue;
                }
                if (_masterPassword.SetMasterPassword(newPass))
                {
                    MessageBox.Show("Master password changed successfully dont forget to change again in 30 Days", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else
                {
                    MessageBox.Show("Failed to change master password. Try again!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }

        private void CheckPasswordStrength()
        {
            var password = PromptForPassword("Check Password Strength", "Enter a password to check:");
            if (password == null) return;
            int score = 0;
            if (password.Length >= 8) score++;
            if (System.Text.RegularExpressions.Regex.IsMatch(password, "[A-Z]")) score++;
            if (System.Text.RegularExpressions.Regex.IsMatch(password, "[a-z]")) score++;
            if (System.Text.RegularExpressions.Regex.IsMatch(password, "[0-9]")) score++;
            if (System.Text.RegularExpressions.Regex.IsMatch(password, "[^a-zA-Z0-9]")) score++;
            string strength = score switch
            {
                5 => "Very Strong yeahh ;)",
                4 => "Strong :)",
                3 => "Medium :|",
                2 => "Weak -_-",
                _ => "Very Weak, :/ "
            };
            MessageBox.Show($"Password Strength: {strength}", "Strength Checker", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BackupPasswords()
        {
            var passwords = _database.GetAllPasswords();
            if (passwords.Count == 0)
            {
                MessageBox.Show("Nothing to backup. Enjoy the app first", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string fileName = $"passwords_backup_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            try
            {
                using (var writer = new System.IO.StreamWriter(fileName))
                {
                    writer.WriteLine("Website,userName,Password,Notes,LastModified,Created_At");
                    foreach (var p in passwords)
                    {
                        string line = $"\"{p.Website}\",\"{p.userName}\",\"{p.EncryptedPwd}\",\"{p.Notes}\",\"{p.LastModified}\",\"{p.Created_At}\"";
                        writer.WriteLine(line);
                    }
                }
                MessageBox.Show($"Backup done! File: {fileName}", "Backup", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during backup: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        
    }
}