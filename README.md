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

Scaffold command:

```bash
dotnet ef dbcontext scaffold "Server=localhost\SQLEXPRESS;Database=project2;Trusted_Connection=True;TrustServerCertificate=True" Microsoft.EntityFrameworkCore.SqlServer --context AppDbContext --context-dir Data -o Models --data-annotations --use-database-names --no-onconfiguring --force
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