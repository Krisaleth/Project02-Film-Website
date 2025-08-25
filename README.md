# Film Project
- A beginner project by using ASP.NET Core MVC for big assignment.

## Table of contents

- Requirements
- Command
- Advice
- Code Editor and Database


## Requirements

This project requires the following packages:

- [EntityFrameworkCore.SqlServer 9.0.8](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.SqlServer/9.0.8?_src=template)
- [EntityFrameworkCore.Design 9.0.8](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Design/)

## Command

- Thêm path này vào enviroment:
```bash
%USERPROFILE%\.dotnet\tools
```
- Cài dotnet-ef
```bash
dotnet tool install --global dotnet-ef
```

- Check dotnet-ef
```bash
dotnet ef --version
```

- Scaffold command:

```bash
dotnet ef dbcontext scaffold "Server=localhost\SQLEXPRESS;Database=project2;Trusted_Connection=True;TrustServerCertificate=True" Microsoft.EntityFrameworkCore.SqlServer --context AppDbContext --context-dir Data -o Models --data-annotations --use-database-names --no-onconfiguring --force
```

- Thêm dòng này sau build trong Program.cs
```bash
    using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            if (!db.Accounts.Any(a => a.UserName == "admin"))
            {
                var (hash, salt) = PasswordHasher.HashPassword("123456");
                db.Accounts.Add(new Account {
                    UserName = "admin",
                    Password_Hash = hash,
                    Password_Salt = salt,
                    Password_Algo = "PBKDF2",
                    Password_Iterations = 100000,
                    Role = "Admin",
                    Status = true,
                    Create_At = DateTime.UtcNow
                });
                db.SaveChanges();
            }
        }
```

## Advise

- Làm ơn viết ra branch khác giúp tôi r để tôi check lại
- Default admin account: 

username: admin

password: 123456


## Code Editor and Database

- Visual Studio 2022 Community 17.14.13 (August 2025)
- SQL Server Management Studio 21 21.4.12
- Microsoft SQL Server Express (64-bit) 16.0.1000.6