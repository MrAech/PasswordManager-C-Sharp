**Name:** Happy  
**UID:** 23BCS10596
# 🔐 C# Password Manager

A simple but secure password manager I made for my C# project! (hope it works lol)

## 💡 How it Works

- Stores passwords securely using AES encryption
- Lets you generate strong passwords
- Lets you search, add, update, and delete passwords
- Uses a master password to protect your data
- All data is stored in a MySQL database
- Console-based interface (run in terminal)

## 🛠️ Tech Stack

- C# (.NET Core)
- MySQL
- BCrypt for password hashing
- DotNetEnv for environment variables

## 🚦 How to Run

1. Clone the repository
2. Copy `.env.example` to `.env` and fill in your database and encryption info
3. Make sure you have [.NET SDK](https://dotnet.microsoft.com/download) installed
4. Run this in the project folder to restore packages:
   ```
   dotnet restore
   ```
   If you get errors about missing packages, try running `dotnet restore` again or check your internet connection.
5. Start the app:
   ```
   dotnet run
   ```
6. Follow the prompts in the terminal

> Note: If you see errors about missing packages after cloning, it's probably because the `obj` or `bin` folders aren't included in the repo. Just run `dotnet restore` as above.
>
> If that doesn't work, you can add the packages manually:
> ```
> dotnet add package BCrypt.Net-Next
> dotnet add package DotNetEnv
> dotnet add package MySql.Data
> ```

## ❗ Troubleshooting

- If you get errors about missing environment variables, check your `.env` file.
- If the database doesn't connect, make sure MySQL is running and your credentials are correct.
- If something else breaks, try running it again or ask for help.

<!-- TODO: add screenshots or more instructions if needed -->