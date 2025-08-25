-- =========================================
--  Database & Schema (PBKDF2-ready)
-- =========================================
IF DB_ID(N'project2') IS NULL
BEGIN
    CREATE DATABASE project2;
END
GO

USE project2;
GO

-- Drop tables if needed (SAFE ORDER) - optional in dev
-- DROP TABLE IF EXISTS Comments, Favorites, FilmGenres, Genres, Film, Admin, Person, Account;

-- =========================================
--  Account (PBKDF2)
-- =========================================
IF OBJECT_ID(N'dbo.Account', N'U') IS NOT NULL DROP TABLE dbo.Account;
GO
CREATE TABLE dbo.Account
(
    Account_ID           BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserName             NVARCHAR(255)  NOT NULL,
    Password_Hash        VARBINARY(512) NOT NULL,   -- PBKDF2 hash bytes
    Password_Salt        VARBINARY(128) NOT NULL,   -- PBKDF2 salt bytes (bắt buộc)
    Password_Algo        NVARCHAR(20)   NOT NULL CONSTRAINT DF_Account_Algo DEFAULT(N'PBKDF2'),
    Password_Iterations  INT            NOT NULL CONSTRAINT DF_Account_Iters DEFAULT(100000),
    Role                 NVARCHAR(10)   NOT NULL,
    Status               BIT            NOT NULL CONSTRAINT DF_Account_Status DEFAULT(1),
    Create_At            DATETIME2(7)   NOT NULL CONSTRAINT DF_Account_CreateAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT UQ_Account_UserName UNIQUE (UserName),
    CONSTRAINT CK_Account_Role CHECK (Role IN (N'Admin', N'User')),
    CONSTRAINT CK_Account_Iters CHECK (Password_Iterations >= 10000)
);
GO

-- =========================================
--  Person
-- =========================================
IF OBJECT_ID(N'dbo.Person', N'U') IS NOT NULL DROP TABLE dbo.Person;
GO
CREATE TABLE dbo.Person
(
    Person_ID       BIGINT IDENTITY(1,1) PRIMARY KEY,
    Person_FullName NVARCHAR(255) NOT NULL,
    Person_Email    NVARCHAR(255) NOT NULL,
    Person_Phone    VARCHAR(15)   NOT NULL,
    Account_ID      BIGINT        NULL,
    CONSTRAINT UQ_Person_FullName UNIQUE (Person_FullName),
    CONSTRAINT UQ_Person_Email    UNIQUE (Person_Email),
    CONSTRAINT FK_Person_Account  FOREIGN KEY(Account_ID) REFERENCES dbo.Account(Account_ID)
        ON UPDATE NO ACTION ON DELETE SET NULL
);
GO

-- =========================================
--  Admin
-- =========================================
IF OBJECT_ID(N'dbo.Admin', N'U') IS NOT NULL DROP TABLE dbo.Admin;
GO
CREATE TABLE dbo.Admin
(
    Admin_ID       BIGINT IDENTITY(1,1) PRIMARY KEY,
    Admin_FullName NVARCHAR(255) NOT NULL,
    Admin_Email    NVARCHAR(255) NOT NULL,
    Admin_Phone    VARCHAR(15)   NOT NULL,
    Account_ID     BIGINT        NULL,
    CONSTRAINT UQ_Admin_FullName UNIQUE (Admin_FullName),
    CONSTRAINT UQ_Admin_Email    UNIQUE (Admin_Email),
    CONSTRAINT FK_Admin_Account  FOREIGN KEY(Account_ID) REFERENCES dbo.Account(Account_ID)
        ON UPDATE NO ACTION ON DELETE SET NULL
);
GO

-- =========================================
--  Film
-- =========================================
IF OBJECT_ID(N'dbo.Film', N'U') IS NOT NULL DROP TABLE dbo.Film;
GO
CREATE TABLE dbo.Film
(
    Film_ID           BIGINT IDENTITY(1,1) PRIMARY KEY,
    Film_Slug         NVARCHAR(255) NOT NULL,
    Film_Name         NVARCHAR(255) NOT NULL,
    Film_Type         NVARCHAR(100) NOT NULL,
    Film_Description  NVARCHAR(MAX) NOT NULL,
    Film_Duration     SMALLINT      NOT NULL,
    Film_Status       NVARCHAR(30)  NOT NULL CONSTRAINT DF_Film_Status DEFAULT (N'Publish'),
    Film_Created_At   DATETIME2(7)  NOT NULL CONSTRAINT DF_Film_Created_At DEFAULT (SYSUTCDATETIME()),
    Film_Update_At    DATETIME2(7)  NOT NULL CONSTRAINT DF_Film_Updated_At DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT UQ_Film_Name UNIQUE (Film_Name),
    CONSTRAINT UQ_Film_Slug UNIQUE (Film_Slug)
);
GO

-- =========================================
--  Genres
-- =========================================
IF OBJECT_ID(N'dbo.Genres', N'U') IS NOT NULL DROP TABLE dbo.Genres;
GO
CREATE TABLE dbo.Genres
(
    Genres_ID   BIGINT IDENTITY(1,1) PRIMARY KEY,
    Genres_Name NVARCHAR(100) NOT NULL,
    Genres_Slug NVARCHAR(100) NOT NULL,
    CONSTRAINT UQ_Genres_Slug UNIQUE (Genres_Slug)
);
GO

-- =========================================
--  FilmGenres (many-to-many)
-- =========================================
IF OBJECT_ID(N'dbo.FilmGenres', N'U') IS NOT NULL DROP TABLE dbo.FilmGenres;
GO
CREATE TABLE dbo.FilmGenres
(
    Film_ID   BIGINT NOT NULL,
    Genres_ID BIGINT NOT NULL,
    CONSTRAINT PK_FilmGenres PRIMARY KEY (Film_ID, Genres_ID),
    CONSTRAINT FK_FilmGenres_Film   FOREIGN KEY (Film_ID)   REFERENCES dbo.Film(Film_ID)
        ON UPDATE NO ACTION ON DELETE CASCADE,
    CONSTRAINT FK_FilmGenres_Genres FOREIGN KEY (Genres_ID) REFERENCES dbo.Genres(Genres_ID)
        ON UPDATE NO ACTION ON DELETE CASCADE
);
GO

-- =========================================
--  Favorites
-- =========================================
IF OBJECT_ID(N'dbo.Favorites', N'U') IS NOT NULL DROP TABLE dbo.Favorites;
GO
CREATE TABLE dbo.Favorites
(
    Person_ID  BIGINT       NOT NULL,
    Film_ID    BIGINT       NOT NULL,
    Created_At DATETIME2(7) NOT NULL CONSTRAINT DF_Favorites_Created_At DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_Favorites PRIMARY KEY (Person_ID, Film_ID),
    CONSTRAINT FK_Favorites_Person FOREIGN KEY (Person_ID) REFERENCES dbo.Person(Person_ID)
        ON UPDATE NO ACTION ON DELETE CASCADE,
    CONSTRAINT FK_Favorites_Film   FOREIGN KEY (Film_ID)   REFERENCES dbo.Film(Film_ID)
        ON UPDATE NO ACTION ON DELETE CASCADE
);
GO

-- =========================================
--  Comments
-- =========================================
IF OBJECT_ID(N'dbo.Comments', N'U') IS NOT NULL DROP TABLE dbo.Comments;
GO
CREATE TABLE dbo.Comments
(
    Comment_ID BIGINT IDENTITY(1,1) PRIMARY KEY,
    Person_ID  BIGINT       NOT NULL,
    Film_ID    BIGINT       NOT NULL,
    Content    NVARCHAR(MAX) NOT NULL,
    Created_At DATETIME2(7) NOT NULL CONSTRAINT DF_Comments_Created_At DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_Comments_Person FOREIGN KEY (Person_ID) REFERENCES dbo.Person(Person_ID)
        ON UPDATE NO ACTION ON DELETE CASCADE,
    CONSTRAINT FK_Comments_Film   FOREIGN KEY (Film_ID)   REFERENCES dbo.Film(Film_ID)
        ON UPDATE NO ACTION ON DELETE CASCADE
);
GO

-- =========================================
--  SP: Tạo account Admin bằng PBKDF2 (app sẽ gửi sẵn hash/salt/iters)
-- =========================================
IF OBJECT_ID(N'dbo.usp_CreateAdminAccountPBKDF2', N'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_CreateAdminAccountPBKDF2;
GO
CREATE PROCEDURE dbo.usp_CreateAdminAccountPBKDF2
(
    @UserName            NVARCHAR(255),
    @Password_Hash       VARBINARY(512),
    @Password_Salt       VARBINARY(128),
    @Password_Iterations INT
)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.Account WHERE UserName = @UserName)
    BEGIN
        RAISERROR(N'UserName đã tồn tại', 16, 1);
        RETURN;
    END

    INSERT INTO dbo.Account
    (
        UserName, Password_Hash, Password_Salt,
        Password_Algo, Password_Iterations,
        Role, Status, Create_At
    )
    VALUES
    (
        @UserName, @Password_Hash, @Password_Salt,
        N'PBKDF2', @Password_Iterations,
        N'Admin', 1, SYSUTCDATETIME()
    );
END
GO

-- =========================================
--  (Tuỳ chọn) Index gợi ý thêm
-- =========================================
CREATE INDEX IX_Film_Slug ON dbo.Film(Film_Slug);
CREATE INDEX IX_Genres_Slug ON dbo.Genres(Genres_Slug);