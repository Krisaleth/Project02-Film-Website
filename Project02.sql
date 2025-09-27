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
);

IF OBJECT_ID(N'dbo.Halls', N'U') IS NOT NULL DROP TABLE dbo.Halls;
GO
CREATE TABLE dbo.Halls
(
    Hall_ID BIGINT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    Cinema_ID BIGINT NOT NULL,
    Capacity INT NOT NULL,
    CONSTRAINT FK_Halls_Cinema_ID FOREIGN KEY (Cinema_ID) REFERENCES dbo.Cinemas(Cinema_ID),
);

IF OBJECT_ID(N'dbo.Showtimes', N'U') IS NOT NULL DROP TABLE dbo.Showtimes;
GO
CREATE TABLE dbo.Showtimes
(
    Showtime_ID BIGINT IDENTITY(1, 1) PRIMARY KEY,
    Movie_ID BIGINT NOT NULL,
    Hall_ID BIGINT NOT NULL,
    Start_Time DATETIME2 NOT NULL,
    End_Time DATETIME2 NOT NULL,
    [Language] NVARCHAR(20) NOT NULL,
    [Format] NVARCHAR(20) NOT NULL,
    CONSTRAINT FK_Showtimes_Movie_ID FOREIGN KEY (Movie_ID) REFERENCES dbo.Movies(Movie_ID),
    CONSTRAINT FK_Showtimes_Hall_ID FOREIGN KEY (Hall_ID) REFERENCES dbo.Halls(Hall_ID),
    CONSTRAINT CK_Showtimes_Format CHECK ([Format] IN (N'3D', N'2D')),
);

IF OBJECT_ID(N'dbo.Seats', N'U') IS NOT NULL DROP TABLE dbo.Seats;
GO
CREATE TABLE dbo.Seats
(
    Seat_ID BIGINT IDENTITY(1, 1) PRIMARY KEY NOT NULL,
    Hall_ID BIGINT NOT NULL,
    RowNumber NVARCHAR(1) NOT NULL,
    SeatNumber NVARCHAR(2) NOT NULL,
    SeatPrice DECIMAL(19,0) NOT NULL,
    SeatType NVARCHAR(10) NOT NULL,
    [Description] NVARCHAR(255),
    SeatStatus NVARCHAR(20) NOT NULL,
    CONSTRAINT FK_Seats_Hall_ID FOREIGN KEY (Hall_ID) REFERENCES dbo.Halls(Hall_ID),
    CONSTRAINT CK_Seats_Status CHECK (SeatStatus IN (N'Available', N'Booked', N'Hold', N'Broken')),
    CONSTRAINT CK_Seats_SeatType CHECK (SeatType IN (N'VIP', N'Normal', N'Couple')),
    CONSTRAINT CK_Seats_SeatPrice CHECK (SeatPrice IN (60000, 95000, 65000)),
);

IF OBJECT_ID(N'dbo.Orders', N'U') IS NOT NULL DROP TABLE dbo.Orders;
GO
CREATE TABLE dbo.Orders 
(
    Order_ID BIGINT IDENTITY(1, 1) PRIMARY KEY NOT NULL,
    [User_ID] BIGINT NOT NULL,
    OrderDate DATETIME NOT NULL,
    TotalAmount DECIMAL(19, 0) NOT NULL,
    [Status] VARCHAR(20) NOT NULL DEFAULT 'Pending'
);

CREATE TABLE OrderSeats (
    OrderSeat_ID BIGINT IDENTITY(1, 1) PRIMARY KEY,
    Order_ID BIGINT NOT NULL,
    Seat_ID BIGINT NOT NULL,
    Price DECIMAL(19,0) NOT NULL,
    Status VARCHAR(20) NOT NULL DEFAULT 'Booked',
    CONSTRAINT FK_OrderSeats_Order FOREIGN KEY (Order_ID) REFERENCES Orders(Order_ID),
    CONSTRAINT FK_OrderSeats_Seat FOREIGN KEY (Seat_ID) REFERENCES Seats(Seat_ID)
);

IF OBJECT_ID(N'dbo.Tickets', N'U') IS NOT NULL DROP TABLE dbo.Tickets;
GO
CREATE TABLE dbo.Tickets
(
    Ticket_ID BIGINT IDENTITY(1, 1) PRIMARY KEY NOT NULL,
    OrderSeat_ID BIGINT NOT NULL,
    Showtime_ID BIGINT NOT NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT N'Available',
    BookingTime DATETIME2 NOT NULL,
    CONSTRAINT FK_Tickets_OrderSeat FOREIGN KEY (OrderSeat_ID) REFERENCES dbo.OrderSeats(OrderSeat_ID),
    CONSTRAINT CK_Tickets_Status CHECK ([Status] IN (N'Available', N'Used', N'Expired')),
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

INSERT INTO dbo.Cinemas (Cinema_Name, [Location], Contact_Info) VALUES
(N'Galaxy Cinema', N'123 Le Loi, District 1, HCM City', N'0909123456'),
(N'CineStar', N'456 Nguyen Trai, District 5, HCM City', N'0833123456'),
(N'BHD Star', N'789 Tran Hung Dao, District 3, HCM City', N'0899123456');

INSERT INTO dbo.Halls (Cinema_ID, Capacity) VALUES
(1, 150),
(1, 100),
(2, 120);

INSERT INTO dbo.Seats (Hall_ID, RowNumber, SeatNumber, SeatType, Description, SeatStatus, SeatPrice)
VALUES
-- Row A
(1, N'A', N'01', N'Normal', NULL, N'Available', 60000),
(1, N'A', N'02', N'Normal', NULL, N'Available', 60000),
(1, N'A', N'03', N'Normal', NULL, N'Available', 60000),
(1, N'A', N'04', N'Normal', NULL, N'Available', 60000),
(1, N'A', N'05', N'Normal', NULL, N'Available', 60000),
(1, N'A', N'06', N'Normal', NULL, N'Available', 60000),
(1, N'A', N'07', N'Normal', NULL, N'Available', 60000),
(1, N'A', N'08', N'Normal', NULL, N'Available', 60000),
(1, N'A', N'09', N'Normal', NULL, N'Available', 60000),
(1, N'A', N'10', N'Normal', NULL, N'Available', 60000),
(1, N'A', N'11', N'Normal', NULL, N'Available', 60000),
(1, N'A', N'12', N'Normal', NULL, N'Available', 60000),

-- Row B
(1, N'B', N'01', N'Normal', NULL, N'Available', 60000),
(1, N'B', N'02', N'Normal', NULL, N'Available', 60000),
(1, N'B', N'03', N'Normal', NULL, N'Available', 60000),
(1, N'B', N'04', N'Normal', NULL, N'Available', 60000),
(1, N'B', N'05', N'Normal', NULL, N'Available', 60000),
(1, N'B', N'06', N'Normal', NULL, N'Available', 60000),
(1, N'B', N'07', N'Normal', NULL, N'Available', 60000),
(1, N'B', N'08', N'Normal', NULL, N'Available', 60000),
(1, N'B', N'09', N'Normal', NULL, N'Available', 60000),
(1, N'B', N'10', N'Normal', NULL, N'Available', 60000),
(1, N'B', N'11', N'Normal', NULL, N'Available', 60000),
(1, N'B', N'12', N'Normal', NULL, N'Available', 60000),

-- Row C
(1, N'C', N'01', N'Normal', NULL, N'Available', 60000),
(1, N'C', N'02', N'Normal', NULL, N'Available', 60000),
(1, N'C', N'03', N'Normal', NULL, N'Available', 60000),
(1, N'C', N'04', N'Normal', NULL, N'Available', 60000),
(1, N'C', N'05', N'Normal', NULL, N'Available', 60000),
(1, N'C', N'06', N'Normal', NULL, N'Available', 60000),
(1, N'C', N'07', N'Normal', NULL, N'Available', 60000),
(1, N'C', N'08', N'Normal', NULL, N'Available', 60000),
(1, N'C', N'09', N'Normal', NULL, N'Available', 60000),
(1, N'C', N'10', N'Normal', NULL, N'Available', 60000),
(1, N'C', N'11', N'Normal', NULL, N'Available', 60000),
(1, N'C', N'12', N'Normal', NULL, N'Available', 60000),

-- Row D
(1, N'D', N'01', N'Normal', NULL, N'Available', 60000),
(1, N'D', N'02', N'Normal', NULL, N'Available', 60000),
(1, N'D', N'03', N'Normal', NULL, N'Available', 60000),
(1, N'D', N'04', N'Normal', NULL, N'Available', 60000),
(1, N'D', N'05', N'Normal', NULL, N'Available', 60000),
(1, N'D', N'06', N'Normal', NULL, N'Available', 60000),
(1, N'D', N'07', N'Normal', NULL, N'Available', 60000),
(1, N'D', N'08', N'Normal', NULL, N'Available', 60000),
(1, N'D', N'09', N'Normal', NULL, N'Available', 60000),
(1, N'D', N'10', N'Normal', NULL, N'Available', 60000),
(1, N'D', N'11', N'Normal', NULL, N'Available', 60000),
(1, N'D', N'12', N'Normal', NULL, N'Available', 60000),

-- Row E
(1, N'E', N'01', N'Normal', NULL, N'Available', 60000),
(1, N'E', N'02', N'Normal', NULL, N'Available', 60000),
(1, N'E', N'03', N'Normal', NULL, N'Available', 60000),
(1, N'E', N'04', N'Normal', NULL, N'Available', 60000),
(1, N'E', N'05', N'Normal', NULL, N'Available', 60000),
(1, N'E', N'06', N'Normal', NULL, N'Available', 60000),
(1, N'E', N'07', N'Normal', NULL, N'Available', 60000),
(1, N'E', N'08', N'Normal', NULL, N'Available', 60000),
(1, N'E', N'09', N'Normal', NULL, N'Available', 60000),
(1, N'E', N'10', N'Normal', NULL, N'Available', 60000),
(1, N'E', N'11', N'Normal', NULL, N'Available', 60000),
(1, N'E', N'12', N'Normal', NULL, N'Available', 60000),

-- Row F
(1, N'F', N'01', N'Normal', NULL, N'Available', 60000),
(1, N'F', N'02', N'Normal', NULL, N'Available', 60000),
(1, N'F', N'03', N'Normal', NULL, N'Available', 60000),
(1, N'F', N'04', N'Normal', NULL, N'Available', 60000),
(1, N'F', N'05', N'Normal', NULL, N'Available', 60000),
(1, N'F', N'06', N'Normal', NULL, N'Available', 60000),
(1, N'F', N'07', N'Normal', NULL, N'Available', 60000),
(1, N'F', N'08', N'Normal', NULL, N'Available', 60000),
(1, N'F', N'09', N'Normal', NULL, N'Available', 60000),
(1, N'F', N'10', N'Normal', NULL, N'Available', 60000),
(1, N'F', N'11', N'Normal', NULL, N'Available', 60000),
(1, N'F', N'12', N'Normal', NULL, N'Available', 60000),

-- Row G
(1, N'G', N'01', N'Normal', NULL, N'Available', 60000),
(1, N'G', N'02', N'Normal', NULL, N'Available', 60000),
(1, N'G', N'03', N'Normal', NULL, N'Available', 60000),
(1, N'G', N'04', N'Normal', NULL, N'Available', 60000),
(1, N'G', N'05', N'Normal', NULL, N'Available', 60000),
(1, N'G', N'06', N'Normal', NULL, N'Available', 60000),
(1, N'G', N'07', N'Normal', NULL, N'Available', 60000),
(1, N'G', N'08', N'Normal', NULL, N'Available', 60000),
(1, N'G', N'09', N'Normal', NULL, N'Available', 60000),
(1, N'G', N'10', N'Normal', NULL, N'Available', 60000),
(1, N'G', N'11', N'Normal', NULL, N'Available', 60000),
(1, N'G', N'12', N'Normal', NULL, N'Available', 60000),

-- Row H
(1, N'H', N'01', N'Normal', NULL, N'Available', 60000),
(1, N'H', N'02', N'Normal', NULL, N'Available', 60000),
(1, N'H', N'03', N'Normal', NULL, N'Available', 60000),
(1, N'H', N'04', N'Normal', NULL, N'Available', 60000),
(1, N'H', N'05', N'Normal', NULL, N'Available', 60000),
(1, N'H', N'06', N'Normal', NULL, N'Available', 60000),
(1, N'H', N'07', N'Normal', NULL, N'Available', 60000),
(1, N'H', N'08', N'Normal', NULL, N'Available', 60000),
(1, N'H', N'09', N'Normal', NULL, N'Available', 60000),
(1, N'H', N'10', N'Normal', NULL, N'Available', 60000),
(1, N'H', N'11', N'Normal', NULL, N'Available', 60000),
(1, N'H', N'12', N'Normal', NULL, N'Available', 60000),

-- Row I
(1, N'I', N'01', N'Normal', NULL, N'Available', 60000),
(1, N'I', N'02', N'Normal', NULL, N'Available', 60000),
(1, N'I', N'03', N'Normal', NULL, N'Available', 60000),
(1, N'I', N'04', N'Normal', NULL, N'Available', 60000),
(1, N'I', N'05', N'Normal', NULL, N'Available', 60000),
(1, N'I', N'06', N'Normal', NULL, N'Available', 60000),
(1, N'I', N'07', N'Normal', NULL, N'Available', 60000),
(1, N'I', N'08', N'Normal', NULL, N'Available', 60000),
(1, N'I', N'09', N'Normal', NULL, N'Available', 60000),
(1, N'I', N'10', N'Normal', NULL, N'Available', 60000),
(1, N'I', N'11', N'Normal', NULL, N'Available', 60000),
(1, N'I', N'12', N'Normal', NULL, N'Available', 60000),

-- Row J
(1, N'J', N'01', N'Normal', NULL, N'Available', 60000),
(1, N'J', N'02', N'Normal', NULL, N'Available', 60000),
(1, N'J', N'03', N'Normal', NULL, N'Available', 60000),
(1, N'J', N'04', N'Normal', NULL, N'Available', 60000),
(1, N'J', N'05', N'Normal', NULL, N'Available', 60000),
(1, N'J', N'06', N'Normal', NULL, N'Available', 60000),
(1, N'J', N'07', N'Normal', NULL, N'Available', 60000),
(1, N'J', N'08', N'Normal', NULL, N'Available', 60000),
(1, N'J', N'09', N'Normal', NULL, N'Available', 60000),
(1, N'J', N'10', N'Normal', NULL, N'Available', 60000),
(1, N'J', N'11', N'Normal', NULL, N'Available', 60000),
(1, N'J', N'12', N'Normal', NULL, N'Available', 60000),

-- Row K
(1, N'K', N'01', N'Normal', NULL, N'Available', 60000),
(1, N'K', N'02', N'Normal', NULL, N'Available', 60000),
(1, N'K', N'03', N'Normal', NULL, N'Available', 60000),
(1, N'K', N'04', N'Normal', NULL, N'Available', 60000),
(1, N'K', N'05', N'Normal', NULL, N'Available', 60000),
(1, N'K', N'06', N'Normal', NULL, N'Available', 60000),
(1, N'K', N'07', N'Normal', NULL, N'Available', 60000),
(1, N'K', N'08', N'Normal', NULL, N'Available', 60000),
(1, N'K', N'09', N'Normal', NULL, N'Available', 60000),
(1, N'K', N'10', N'Normal', NULL, N'Available', 60000),
(1, N'K', N'11', N'Normal', NULL, N'Available', 60000),
(1, N'K', N'12', N'Normal', NULL, N'Available', 60000),

-- Row L
(1, N'L', N'01', N'Normal', NULL, N'Available', 60000),
(1, N'L', N'02', N'Normal', NULL, N'Available', 60000),
(1, N'L', N'03', N'Normal', NULL, N'Available', 60000),
(1, N'L', N'04', N'Normal', NULL, N'Available', 60000),
(1, N'L', N'05', N'Normal', NULL, N'Available', 60000),
(1, N'L', N'06', N'Normal', NULL, N'Available', 60000),
(1, N'L', N'07', N'Normal', NULL, N'Available', 60000),
(1, N'L', N'08', N'Normal', NULL, N'Available', 60000),
(1, N'L', N'09', N'Normal', NULL, N'Available', 60000),
(1, N'L', N'10', N'Normal', NULL, N'Available', 60000),
(1, N'L', N'11', N'Normal', NULL, N'Available', 60000),
(1, N'L', N'12', N'Normal', NULL, N'Available', 60000);


INSERT INTO dbo.Showtimes (Movie_ID, Hall_ID, Start_Time, End_Time, [Language], [Format]) VALUES
(1, 1, '2025-09-22 14:00:00', '2025-09-22 16:00:00', N'Vietnamese', N'2D'),
(2, 2, '2025-09-22 16:30:00', '2025-09-22 18:30:00', N'English', N'3D'),
(3, 3, '2025-09-22 19:00:00', '2025-09-22 21:00:00', N'French', N'2D');