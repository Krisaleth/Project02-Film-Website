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
    Status               BIT            NOT NULL CONSTRAINT DF_Account_Status DEFAULT(1),
    Create_At            DATETIME2(7)   NOT NULL CONSTRAINT DF_Account_CreateAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT UQ_Account_UserName UNIQUE (UserName),
    CONSTRAINT CK_Account_Role CHECK (Role IN (N'Admin', N'User')),
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
    Movie_Description  NVARCHAR(MAX) NOT NULL,
    Movie_Duration     SMALLINT      NOT NULL,
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
    Genres_ID   BIGINT IDENTITY(1,1) PRIMARY KEY,
    Genres_Name NVARCHAR(100) NOT NULL,
    Genres_Slug NVARCHAR(100) NOT NULL,
    CONSTRAINT UQ_Genres_Slug UNIQUE (Genres_Slug)
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
    Genres_ID BIGINT NOT NULL,
    CONSTRAINT PK_MovieGenres PRIMARY KEY (Movie_ID, Genres_ID),
    CONSTRAINT FK_MovieGenres_Movie   FOREIGN KEY (Movie_ID)   REFERENCES dbo.Movie(Movie_ID)
        ON UPDATE NO ACTION ON DELETE CASCADE,
    CONSTRAINT FK_MovieGenres_Genres FOREIGN KEY (Genres_ID) REFERENCES dbo.Genres(Genres_ID)
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
CREATE INDEX IX_Genres_Slug ON dbo.Genres(Genres_Slug);

INSERT INTO dbo.Genres (Genres_Name, Genres_Slug)
VALUES
(N'Action', 'action'),
(N'Comedy', 'comedy'),
(N'Drama', 'drama'),
(N'Horror', 'horror'),
(N'Sci-Fi', 'sci-fi');

INSERT INTO dbo.Movie (Movie_Slug, Movie_Name, Movie_Description, Movie_Duration, Movie_Status)
VALUES
('inception-2010', N'Inception', N'Dream within a dream thriller.', 148, 'Publish'),
('interstellar-2014', N'Interstellar', N'Space travel through wormholes.', 169, 'Publish'),
('matrix-1999', N'The Matrix', N'Virtual reality and human freedom.', 136, 'Publish'),
('avengers-2012', N'The Avengers', N'Marvel heroes unite.', 143, 'Publish'),
('darkknight-2008', N'The Dark Knight', N'Batman faces Joker in Gotham.', 152, 'Publish'),
('parasite-2019', N'Parasite', N'South Korean social thriller.', 132, 'Publish'),
('joker-2019', N'Joker', N'The rise of Arthur Fleck.', 122, 'Publish'),
('titanic-1997', N'Titanic', N'Romance on doomed ship.', 195, 'Publish'),
('avatar-2009', N'Avatar', N'Human vs Na''vi in Pandora.', 162, 'Publish'),
('terminator2-1991', N'Terminator 2', N'Robot fights for future.', 137, 'Publish'),
('godfather-1972', N'The Godfather', N'Mafia family crime drama.', 175, 'Publish'),
('shawshank-1994', N'The Shawshank Redemption', N'Hope inside prison.', 142, 'Publish'),
('lotr-fellowship-2001', N'LOTR: The Fellowship of the Ring', N'Journey to destroy the One Ring.', 178, 'Publish'),
('lotr-return-2003', N'LOTR: The Return of the King', N'Epic finale to the Ring war.', 201, 'Publish'),
('harrypotter-2001', N'Harry Potter and the Sorcerer''s Stone', N'Boy wizard begins adventure.', 152, 'Publish'),
('harrypotter-2004', N'Harry Potter and the Prisoner of Azkaban', N'Harry discovers Sirius Black.', 142, 'Publish'),
('spiderman-2002', N'Spider-Man', N'Peter Parker becomes Spider-Man.', 121, 'Publish'),
('blackpanther-2018', N'Black Panther', N'T''Challa defends Wakanda.', 134, 'Publish'),
('frozen-2013', N'Frozen', N'Sisters Elsa and Anna.', 102, 'Publish'),
('toy-story-1995', N'Toy Story', N'Pixar toys come alive.', 81, 'Publish');


-- 1:Action, 2:Comedy, 3:Drama, 4:Horror, 5:Sci-Fi
INSERT INTO dbo.MovieGenres (Movie_ID, Genres_ID) VALUES
(1,5),(2,5),(3,5),
(4,1),(5,1),(6,3),(7,3),(8,3),
(9,5),(10,1),
(11,3),(12,3),
(13,5),(14,5),
(15,5),(16,5),
(17,1),(18,1),
(19,2),(20,2);
