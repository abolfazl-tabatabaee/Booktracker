# BookTracker

BookTracker is a web application for managing books and user reviews, built with **ASP.NET Core**, **Entity Framework Core**, and **ASP.NET Core Identity**.
The project focuses on clean architecture, security best practices, and maintainable backend design.

It includes user authentication, role-based authorization, an admin panel, and secure file uploads.

---

## ✨ Features

- User authentication and authorization using ASP.NET Core Identity
- Role-based access control (Admin, User)
- Admin panel for managing books, users, and reviews
- Book management (create, edit, delete, list)
- User reviews and ratings for books
- User profile management and password change
- Secure local file upload for book cover images
- Server-side validation for all user inputs
- CSRF protection for state-changing requests
- Security headers and Content Security Policy (CSP)

---

## 🧱 Tech Stack

- ASP.NET Core
- Entity Framework Core
- SQL Server
- ASP.NET Core Identity
- MVC pattern and Web API
- C#

---

## ⚙️ Setup (Development)

### Requirements
- .NET SDK (matching the project version)
- SQL Server (LocalDB or full SQL Server)
- Visual Studio or Rider (optional)

### Steps

1. Clone the repository
```
git clone <https://github.com/abolfazl-tabatabaee/booktracker.git>
cd bookTracker
```

2. Configure the database connection  
Edit `appsettings.json` and update the connection string:
```
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=BookTackerDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

3. Configure development secrets (Admin seed)  
Run the following commands in the directory containing the `.csproj` file:
```
dotnet user-secrets init
dotnet user-secrets set "SeedAdmin:Email" "admin@booktracker.local"
dotnet user-secrets set "SeedAdmin:Password" "Your_Strong_Admin_Password"
```

4. Apply database migrations
```
dotnet ef database update
```

5. Run the application
```
dotnet run
```

Open the application in your browser:
```
https://localhost:<port>
```

---

## 🔑 Admin Account (Development Only)

An admin account is automatically created **only in Development mode** if admin seed credentials are configured.

- Email: value of `SeedAdmin:Email`
- Password: value of `SeedAdmin:Password`

If an admin user already exists, the seed process will not overwrite the existing password.

---

## 🔐 Security Notes

- Sensitive information such as passwords and connection strings is not stored in the repository
- Development secrets are handled via .NET User Secrets or environment variables
- Authentication cookies are configured as secure and HTTP-only
- CSRF protection is enabled for state-changing requests
- Uploaded files are validated by size, type, and file signature
- Uploaded files and local secrets are excluded from version control via `.gitignore`

---

## 📁 File Uploads

Book cover images are stored locally in:
```
wwwroot/uploads/covers/
```

This directory is excluded from version control and intended for development or demo purposes only.

---

## 📄 License

This project is licensed under the MIT License.
See the LICENSE file for details.