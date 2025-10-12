# Project02 - Film Website

A beginner-level ASP.NET Core MVC project developed as a big school assignment.
---

## Table of Contents

- [Requirements](#requirements)  
- [Setup & Commands](#setup--commands)  
- [Restoring the Project After Cloning](#restoring-the-project-after-cloning)  
- [Usage Recommendations](#usage-recommendations)  
- [Default Admin Account](#default-admin-account)  
- [Frameworks and Technologies Used](#frameworks-and-technologies-used)  
- [Development Environment](#development-environment)  
- [About](#about)

---

## Requirements

This project requires the following NuGet packages:

- [EntityFrameworkCore.SqlServer 9.0.8](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.SqlServer/9.0.8?_src=template)
- [EntityFrameworkCore.Design 9.0.8](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Design/)

- You also need to install the EF Core CLI tool:
```bash
dotnet tool install --global dotnet-ef
```

- Verify installation via:
```bash
dotnet ef --version
```
---

## Setup & Commands

- To scaffold the database context and models based on the existing SQL Server database, use:

```bash
dotnet ef dbcontext scaffold "Server=.\SQLEXPRESS;Database=db_movie;Trusted_Connection=True;TrustServerCertificate=True" Microsoft.EntityFrameworkCore.SqlServer --context AppDbContext --context-dir Data -o Models --data-annotations --use-database-names --no-onconfiguring --force
```

## Restoring the Project After Cloning

1. Clone the repository:
```bash
git clone https://github.com/Krisaleth/Project02-Film-Website
```

2. Open SQL Server Management Studio and run the provided `Project02.sql`, `Project02-Showtimes.sql` and `Project02-Seats.sql` file to create and seed the database.

3. Navigate to the project folder and restore dependencies:
```bash
dotnet restore Project02.sln
```


4. Build and run the project (run automatically after restore on some environments):
```bash
dotnet build
dotnet run
```
---

## Default Admin Account

- **Username:** admin  
- **Password:** 123456  

Use this account to log in as administrator.

---

## Frameworks and Technologies Used

- **ASP.NET Core MVC**: The main framework for building the web application using the Model-View-Controller architectural pattern.  
- **Entity Framework Core (EF Core)**: ORM used for data access and management with SQL Server.  
- **SQL Server**: The primary database system for storing movies, tickets, users, and related data.  
- **Bootstrap CSS**: Utilized for responsive and modern UI design.  
- **jQuery and JavaScript**: Used for client-side interactivity, such as seat selection and dynamic UI updates.  
- **Font Awesome**: Icon library integrated for UI elements and decorations.  
- **Razor Pages / Razor Views**: Server-side view engine for rendering dynamic HTML pages in ASP.NET Core MVC.  
- **.NET 9**: The .NET SDK and runtime version used, along with EF Core packages version 9.0.8.  
- **Visual Studio 2022**: The primary integrated development environment (IDE) used for development and debugging.

---

## Development Environment

- Visual Studio 2022 Community 17.14.13 (August 2025)  
- SQL Server Management Studio 21 21.4.12  
- Microsoft SQL Server Express (64-bit) 16.0.1000.6  

---

## About

This is a beginner ASP.NET Core MVC project developed as part of a school assignment. The project demonstrates the use of MVC architecture, Entity Framework Core for database operations, and building a cinema ticket booking website.

---

If you need any help or contributions, feel free to open an issue or pull request.