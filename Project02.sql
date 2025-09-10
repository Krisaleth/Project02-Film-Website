-- =========================================
--  Database & Schema (PBKDF2-ready)
-- =========================================
IF DB_ID(N'db_movie') IS NULL
BEGIN
    CREATE DATABASE db_movie;
END
GO

USE db_movie;
GO

-- Drop tables if needed (SAFE ORDER) - optional in dev
-- DROP TABLE IF EXISTS Comments, Favorites, MovieGenres, Genres, Movie, Admin, Users, Account;

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
    Status               NVARCHAR(10)   NOT NULL CONSTRAINT DF_Account_Status DEFAULT(N'Active'),
    Create_At            DATETIME2(7)   NOT NULL CONSTRAINT DF_Account_CreateAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT UQ_Account_UserName UNIQUE (UserName),
    CONSTRAINT CK_Account_Role CHECK (Role IN (N'Admin', N'User')),
    CONSTRAINT CK_Account_Status CHECK (Status IN (N'Active', N'Banned')),
    CONSTRAINT CK_Account_Iters CHECK (Password_Iterations >= 10000)
);
GO

-- =========================================
--  Users
-- =========================================
IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL DROP TABLE dbo.Users;
GO
CREATE TABLE dbo.Users
(
    Users_ID       BIGINT IDENTITY(1,1) PRIMARY KEY,
    Users_FullName NVARCHAR(255) NOT NULL,
    Users_Email    NVARCHAR(255) NOT NULL,
    Users_Phone    VARCHAR(15)   NOT NULL,
    RowsVersion    ROWVERSION    NOT NULL,
    Account_ID      BIGINT        NULL,
    CONSTRAINT UQ_Users_FullName UNIQUE (Users_FullName),
    CONSTRAINT UQ_Users_Email    UNIQUE (Users_Email),
    CONSTRAINT FK_Users_Account  FOREIGN KEY(Account_ID) REFERENCES dbo.Account(Account_ID)
        ON UPDATE NO ACTION ON DELETE SET NULL
);
GO

-- =========================================
--  Movie
-- =========================================
IF OBJECT_ID(N'dbo.Movie', N'U') IS NOT NULL DROP TABLE dbo.Movie;
GO
CREATE TABLE dbo.Movie
(
    Movie_ID           BIGINT IDENTITY(1,1) PRIMARY KEY,
    Movie_Slug         NVARCHAR(255) NOT NULL,
    Movie_Name         NVARCHAR(255) NOT NULL,
    Movie_Producer     NVARCHAR(255) NOT NULL DEFAULT N'Unknown',
    Movie_Poster       NVARCHAR(500) NOT NULL,
    Movie_Description  NVARCHAR(MAX) NOT NULL,
    Movie_Duration     SMALLINT      NOT NULL,
    Movie_Year         INT           NOT NULL DEFAULT 2000,
    RowsVersion        ROWVERSION    NOT NULL,
    Movie_Status       NVARCHAR(30)  NOT NULL CONSTRAINT DF_Movie_Status DEFAULT (N'Publish'),
    Movie_Created_At   DATETIME2(7)  NOT NULL CONSTRAINT DF_Movie_Created_At DEFAULT (SYSUTCDATETIME()),
    Movie_Update_At    DATETIME2(7)  NOT NULL CONSTRAINT DF_Movie_Updated_At DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT UQ_Movie_Name UNIQUE (Movie_Name),
    CONSTRAINT UQ_Movie_Slug UNIQUE (Movie_Slug),
    CONSTRAINT CK_Movie_Status CHECK (Movie_Status IN (N'Publish', N'Unpublish'))
);
GO

-- =========================================
--  Genres
-- =========================================
IF OBJECT_ID(N'dbo.Genres', N'U') IS NOT NULL DROP TABLE dbo.Genres;
GO
CREATE TABLE dbo.Genres
(
    Genre_ID   BIGINT IDENTITY(1,1) PRIMARY KEY,
    Genre_Name NVARCHAR(100) NOT NULL,
    Genre_Slug NVARCHAR(100) NOT NULL,
    CONSTRAINT UQ_Genre_Slug UNIQUE (Genre_Slug)
);
GO

-- =========================================
--  MovieGenres (many-to-many)
-- =========================================
IF OBJECT_ID(N'dbo.MovieGenres', N'U') IS NOT NULL DROP TABLE dbo.MovieGenres;
GO
CREATE TABLE dbo.MovieGenres
(
    Movie_ID   BIGINT NOT NULL,
    Genre_ID BIGINT NOT NULL,
    CONSTRAINT PK_MovieGenre PRIMARY KEY (Movie_ID, Genre_ID),
    CONSTRAINT FK_MovieGenre_Movie   FOREIGN KEY (Movie_ID)   REFERENCES dbo.Movie(Movie_ID)
        ON UPDATE NO ACTION ON DELETE CASCADE,
    CONSTRAINT FK_MovieGenre_Genre FOREIGN KEY (Genre_ID) REFERENCES dbo.Genres(Genre_ID)
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
    Users_ID  BIGINT       NOT NULL,
    Movie_ID    BIGINT       NOT NULL,
    Created_At DATETIME2(7) NOT NULL CONSTRAINT DF_Favorites_Created_At DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_Favorites PRIMARY KEY (Users_ID, Movie_ID),
    CONSTRAINT FK_Favorites_Users FOREIGN KEY (Users_ID) REFERENCES dbo.Users(Users_ID)
        ON UPDATE NO ACTION ON DELETE CASCADE,
    CONSTRAINT FK_Favorites_Movie   FOREIGN KEY (Movie_ID)   REFERENCES dbo.Movie(Movie_ID)
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
    Users_ID  BIGINT       NOT NULL,
    Movie_ID    BIGINT       NOT NULL,
    Content    NVARCHAR(MAX) NOT NULL,
    Created_At DATETIME2(7) NOT NULL CONSTRAINT DF_Comments_Created_At DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_Comments_Users FOREIGN KEY (Users_ID) REFERENCES dbo.Users(Users_ID)
        ON UPDATE NO ACTION ON DELETE CASCADE,
    CONSTRAINT FK_Comments_Movie   FOREIGN KEY (Movie_ID)   REFERENCES dbo.Movie(Movie_ID)
        ON UPDATE NO ACTION ON DELETE CASCADE
);
GO

CREATE INDEX IX_Movie_Slug ON dbo.Movie(Movie_Slug);
CREATE INDEX IX_Genres_Slug ON dbo.Genres(Genre_Slug);

INSERT INTO dbo.Genres (Genre_Name, Genre_Slug)
VALUES
(N'Action', 'action'),
(N'Comedy', 'comedy'),
(N'Drama', 'drama'),
(N'Horror', 'horror'),
(N'Sci-Fi', 'sci-fi'),
(N'Animation', 'animation'),
(N'Romance','romance'),
(N'Thriller', 'thriller'),
(N'Fantasy','fantasy'),
(N'Adventure','adventure'),
(N'Crime','crime');


INSERT INTO dbo.Movie
(Movie_Slug, Movie_Name, Movie_Description, Movie_Duration, Movie_Status, Movie_Poster, Movie_Producer, Movie_Year)
VALUES
-- 1
('interstellar-2014', N'Interstellar',
 N'A team of explorers travel through a wormhole in space to ensure humanity''s survival.',
 169, N'Publish', N'/uploads/posters/interstellar-2014.jpg', 
 N'Syncopy / Warner Bros.', 2014),

-- 2
('inception-2010', N'Inception',
 N'A skilled thief leads a team into the subconscious world of dreams.',
 148, N'Publish', N'/uploads/posters/inception-2010.jpg', 
 N'Syncopy / Warner Bros.', 2010),

-- 3
('tenet-2020', N'Tenet',
 N'A secret agent manipulates time to prevent World War III.',
 150, N'Unpublish', N'/uploads/posters/tenet-2020.jpg', 
 N'Syncopy / Warner Bros.', 2020),

-- 4
('avengers-endgame-2019', N'Avengers: Endgame',
 N'The Avengers assemble once more to reverse the damage caused by Thanos.',
 181, N'Publish', N'/uploads/posters/avengers-endgame-2019.jpg', 
 N'Marvel Studios', 2019),

-- 5
('joker-2019', N'Joker',
 N'An origin story of the infamous DC villain Arthur Fleck.',
 122, N'Unpublish', N'/uploads/posters/joker-2019.jpg', 
 N'Warner Bros. Pictures', 2019);




-- Interstellar: Sci-Fi, Adventure, Drama
INSERT INTO dbo.MovieGenres (Movie_ID, Genre_ID) VALUES
(1, 1), (1, 8), (1, 5);

-- Inception: Sci-Fi, Action, Thriller
INSERT INTO dbo.MovieGenres (Movie_ID, Genre_ID) VALUES
(2, 1), (2, 2), (2, 6);

-- Tenet: Sci-Fi, Action, Thriller
INSERT INTO dbo.MovieGenres (Movie_ID, Genre_ID) VALUES
(3, 1), (3, 2), (3, 6);

-- Avengers Endgame: Action, Adventure, Sci-Fi
INSERT INTO dbo.MovieGenres (Movie_ID, Genre_ID) VALUES
(4, 2), (4, 8), (4, 1);

-- Joker: Drama, Crime, Thriller
INSERT INTO dbo.MovieGenres (Movie_ID, Genre_ID) VALUES
(5, 5), (5, 9), (5, 6);
