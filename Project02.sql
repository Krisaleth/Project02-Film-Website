IF DB_ID(N'db_movie') IS NULL
BEGIN
    CREATE DATABASE db_movie;
END
GO

USE db_movie;
GO

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
    CONSTRAINT CK_Account_Status CHECK (Status IN (N'Active', N'Inactive', N'Banned')),
    CONSTRAINT CK_Account_Iters CHECK (Password_Iterations >= 10000)
);
GO  


IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL DROP TABLE dbo.Users;
GO
CREATE TABLE dbo.Users
(
    [User_ID]     BIGINT IDENTITY(1,1) PRIMARY KEY,
    [User_Name]   NVARCHAR(255) NOT NULL,
    User_Email    NVARCHAR(255) NOT NULL,
    User_Phone    VARCHAR(15)   NOT NULL,
    RowsVersion    ROWVERSION    NOT NULL,
    Account_ID      BIGINT        NULL,
    CONSTRAINT UQ_Users_FullName UNIQUE ([User_Name]),
    CONSTRAINT UQ_Users_Email    UNIQUE (User_Email),
    CONSTRAINT FK_Users_Account  FOREIGN KEY(Account_ID) REFERENCES dbo.Account(Account_ID)
        ON UPDATE NO ACTION ON DELETE SET NULL
);
GO


IF OBJECT_ID(N'dbo.Movie', N'U') IS NOT NULL DROP TABLE dbo.Movie;
GO
CREATE TABLE dbo.Movies
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


IF OBJECT_ID(N'dbo.MovieGenres', N'U') IS NOT NULL DROP TABLE dbo.MovieGenres;
GO
CREATE TABLE dbo.MovieGenres
(
    Movie_ID BIGINT NOT NULL,
    Genre_ID BIGINT NOT NULL,
    CONSTRAINT PK_MovieGenre PRIMARY KEY (Movie_ID, Genre_ID),
    CONSTRAINT FK_MovieGenre_Movie   FOREIGN KEY (Movie_ID)   REFERENCES dbo.Movies(Movie_ID)
        ON UPDATE NO ACTION ON DELETE CASCADE,
    CONSTRAINT FK_MovieGenre_Genre FOREIGN KEY (Genre_ID) REFERENCES dbo.Genres(Genre_ID)
        ON UPDATE NO ACTION ON DELETE CASCADE
);
GO

IF OBJECT_ID(N'dbo.Cinemas', N'U') IS NOT NULL DROP TABLE dbo.Cinemas;
GO
CREATE TABLE dbo.Cinemas
(
    Cinema_ID BIGINT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    Cinema_Name NVARCHAR(50) UNIQUE NOT NULL,
    [Location] NVARCHAR(255) NOT NULL,
    Contact_Info NVARCHAR(200) NOT NULL, 
)

IF OBJECT_ID(N'dbo.Halls', N'U') IS NOT NULL DROP TABLE dbo.Halls;
GO
CREATE TABLE dbo.Halls
(
    Hall_ID BIGINT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    Cinema_ID BIGINT NOT NULL,
    Capacity INT NOT NULL,
    CONSTRAINT FK_Halls_Cinema_ID FOREIGN KEY (Cinema_ID) REFERENCES dbo.Cinemas(Cinema_ID),
)

IF OBJECT_ID(N'dbo.Showtimes', N'U') IS NOT NULL DROP TABLE dbo.Showtimes;
GO
CREATE TABLE dbo.Showtimes
(
    Showtime_ID BIGINT IDENTITY(1, 1) PRIMARY KEY,
    Movie_ID BIGINT NOT NULL,
    Cinema_ID BIGINT NOT NULL,
    Hall_ID BIGINT NOT NULL,
    Start_Time DATETIME2 NOT NULL,
    End_Time DATETIME2 NOT NULL,
    [Language] NVARCHAR(20) NOT NULL,
    [Format] NVARCHAR(20) NOT NULL,
    Price DECIMAL(19,0) NOT NULL,
    CONSTRAINT FK_Showtimes_Movie_ID FOREIGN KEY (Movie_ID) REFERENCES dbo.Movies(Movie_ID),
    CONSTRAINT FK_Showtimes_Cinema_ID FOREIGN KEY (Cinema_ID) REFERENCES dbo.Cinemas(Cinema_ID),
    CONSTRAINT FK_Showtimes_Hall_ID FOREIGN KEY (Hall_ID) REFERENCES dbo.Halls(Hall_ID),
    CONSTRAINT CK_Showtimes_Format CHECK ([Format] IN (N'3D', N'2D')),
);

IF OBJECT_ID(N'dbo.Seats', N'U') IS NOT NULL DROP TABLE dbo.Seats;
GO
CREATE TABLE dbo.Seats
(
    Seat_ID BIGINT IDENTITY(1, 1) PRIMARY KEY NOT NULL,
    Hall_ID BIGINT NOT NULL ,
    RowNumber NVARCHAR(1) NOT NULL,
    SeatNumber NVARCHAR(2) NOT NULL,
    SeatType NVARCHAR(10) NOT NULL,
    CONSTRAINT FK_Seats_Hall_ID FOREIGN KEY (Hall_ID) REFERENCES dbo.Halls(Hall_ID),
    CONSTRAINT CK_Seats_SeatType CHECK (SeatType IN (N'VIP', N'Normal', N'Couple')),
);

IF OBJECT_ID(N'dbo.Tickets', N'U') IS NOT NULL DROP TABLE dbo.Tickets;
GO
CREATE TABLE dbo.Tickets
(
    Ticket_ID BIGINT IDENTITY(1, 1) PRIMARY KEY NOT NULL,
    Showtime_ID BIGINT NOT NULL,
    Seat_ID BIGINT NOT NULL,
    [User_ID] BIGINT NOT NULL,
    Price DECIMAL(19, 0) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    BookingTime DATETIME2 NOT NULL,
    CONSTRAINT FK_Tickets_Showtimes_ID FOREIGN KEY (Showtime_ID) REFERENCES dbo.Showtimes(Showtime_ID),
    CONSTRAINT FK_Tickets_Seat_ID FOREIGN KEY (Seat_ID) REFERENCES dbo.Seats(Seat_ID),
    CONSTRAINT FK_Tickets_User_ID FOREIGN KEY ([User_ID]) REFERENCES dbo.Users([User_ID]),
    CONSTRAINT CK_Tickets_Status CHECK (Status IN (N'Available', N'Reserved', N'Paid', N'Cancelled', N'Used', N'Expired')),
);

IF OBJECT_ID(N'dbo.Payments', N'U') IS NOT NULL DROP TABLE dbo.Payments;
GO
CREATE TABLE dbo.Payments 
(
    Payment_ID BIGINT IDENTITY(1, 1) PRIMARY KEY NOT NULL,
    [User_ID] BIGINT NOT NULL,
    Ticket_ID BIGINT NOT NULL,
    Amount INT NOT NULL,
    PaymentMethod NVARCHAR(50) NOT NULL,
    PaymentStatus NVARCHAR(50) NOT NULL,
    PaymentTime DATETIME2 NOT NULL,
    CONSTRAINT FK_Payments_User_ID FOREIGN KEY ([User_ID]) REFERENCES dbo.Users([User_ID]),
    CONSTRAINT FK_Payments_Ticket_ID FOREIGN KEY (Ticket_ID) REFERENCES dbo.Tickets(Ticket_ID),
    CONSTRAINT CK_Payments_PaymentMethod CHECK (PaymentMethod IN (N'CreditCard', N'DebitCard', N'BankTransfer', N'EWallet', N'Cash', N'Paypal', N'Other')),
    CONSTRAINT CK_Payments_PaymentStatus CHECK (PaymentStatus IN (N'Pending', N'Completed', N'Failed', N'Cancelled', N'Refunded')),
);


CREATE INDEX IX_Movie_Slug ON dbo.Movies(Movie_Slug);
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


INSERT INTO dbo.Movies
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
