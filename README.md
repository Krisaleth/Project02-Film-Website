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

- Cài dotnet-ef:
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

## Hướng dẫn restore dotnet sau khi clone project về

- Đầu tiên, chúng ta cần clone project về máy trước:
```bash
git clone https://github.com/Krisaleth/Project02-Film-Website
```

- Sau khi clone về, hãy mở SQL Server Management Studio và chạy file Project02.sql để có cho mình DB

- Vào folder chứa project sau khi chạy file SQL, mở Command Prompt ở đây và chạy lệnh sau
```bash
dotnet restore Project02.sln
```

- Nếu build và run tự động chạy thì bạn có thể dừng, nếu không thì chạy thêm 2 lệnh sau:
```bash
dotnet build
dotnet run
```

## Advise

- Hãy viết ra branch khác khi làm thêm chức năng.
- Viết xong lúc nào post luôn lúc đấy, tránh bỏ dở không post xong người khác post lên là bị conflict, kiểm code rất mệt.
- Default admin account: 

- username: admin

- password: 123456


## Code Editor and Database

- Visual Studio 2022 Community 17.14.13 (August 2025)
- SQL Server Management Studio 21 21.4.12
- Microsoft SQL Server Express (64-bit) 16.0.1000.6