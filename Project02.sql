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
    Showtime_ID BIGINT NOT NULL,
    OrderDate DATETIME NOT NULL,
    TotalAmount DECIMAL(19, 0) NOT NULL,
    [Status] VARCHAR(20) NOT NULL DEFAULT 'Pending'
    CONSTRAINT FK_Orders_User FOREIGN KEY ([User_ID]) REFERENCES dbo.Users([User_ID]),
    CONSTRAINT FK_Orders_Showtime FOREIGN KEY (Showtime_ID) REFERENCES dbo.Showtimes(Showtime_ID),
    CONSTRAINT CK_Orders_Status CHECK ([Status] IN (N'Pending', N'Completed', N'Cancelled'))
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
 N'Warner Bros. Pictures', 2019),

 -- 6
('the-dark-knight-2008', N'The Dark Knight',
 N'Batman battles the Joker to save Gotham City.',
 152, N'Publish', N'/uploads/posters/the-dark-knight-2008.jpg', N'Warner Bros.', 2008),

-- 7
('the-lord-of-the-rings-the-return-of-the-king-2003', N'The Lord of the Rings: The Return of the King',
 N'Gondor is overrun as Frodo and Sam approach Mount Doom to destroy the One Ring.',
 201, N'Publish', N'/uploads/posters/the-lord-of-the-rings-the-return-of-the-king-2003.jpg', N'New Line Cinema', 2003),

-- 8
('the-lord-of-the-rings-the-fellowship-of-the-ring-2001', N'The Lord of the Rings: The Fellowship of the Ring',
 N'A hobbit and companions set out to destroy the One Ring.',
 178, N'Publish', N'/uploads/posters/the-lord-of-the-rings-the-fellowship-of-the-ring-2001.jpg', N'New Line Cinema', 2001),

-- 9
('the-lord-of-the-rings-the-two-towers-2002', N'The Lord of the Rings: The Two Towers',
 N'The Fellowship is divided but fights Sauron in different ways.',
 179, N'Publish', N'/uploads/posters/the-lord-of-the-rings-the-two-towers-2002.jpg', N'New Line Cinema', 2002),

-- 10
('harry-potter-and-the-sorcerers-stone-2001', N'Harry Potter and the Sorcerer''s Stone',
 N'A boy discovers he''s a wizard and attends Hogwarts.',
 152, N'Publish', N'/uploads/posters/harry-potter-and-the-sorcerers-stone-2001.jpg', N'Warner Bros.', 2001),

-- 11
('harry-potter-and-the-chamber-of-secrets-2002', N'Harry Potter and the Chamber of Secrets',
 N'Harry and friends face new danger at Hogwarts.',
 161, N'Publish', N'/uploads/posters/harry-potter-and-the-chamber-of-secrets-2002.jpg', N'Warner Bros.', 2002),

-- 12
('harry-potter-and-the-prisoner-of-azkaban-2004', N'Harry Potter and the Prisoner of Azkaban',
 N'Harry learns about Sirius Black and time-turners.',
 142, N'Publish', N'/uploads/posters/harry-potter-and-the-prisoner-of-azkaban-2004.jpg', N'Warner Bros.', 2004),

-- 13
('avatar-2009', N'Avatar',
 N'A marine on an alien planet switches sides in a conflict.',
 162, N'Publish', N'/uploads/posters/avatar-2009.jpg', N'20th Century Fox', 2009),

-- 14
('star-wars-the-force-awakens-2015', N'Star Wars: The Force Awakens',
 N'A new threat rises and Rey discovers her destiny.',
 138, N'Publish', N'/uploads/posters/star-wars-the-force-awakens-2015.jpg', N'Lucasfilm', 2015),

-- 15
('mad-max-fury-road-2015', N'Mad Max: Fury Road',
 N'Max and Furiosa flee from a warlord across the wasteland.',
 120, N'Publish', N'/uploads/posters/mad-max-fury-road-2015.jpg', N'Village Roadshow', 2015),

-- 16
('black-panther-2018', N'Black Panther',
 N'T''Challa becomes king of Wakanda but faces enemies.',
 134, N'Publish', N'/uploads/posters/black-panther-2018.jpg', N'Marvel Studios', 2018),

-- 17
('la-la-land-2016', N'La La Land',
 N'Aspiring actress and jazz musician fall in love in Los Angeles.',
 128, N'Publish', N'/uploads/posters/la-la-land-2016.jpg', N'Summit Entertainment', 2016),

-- 18
('parasite-2019', N'Parasite',
 N'A poor family infiltrates a rich household in Korea.',
 132, N'Publish', N'/uploads/posters/parasite-2019.jpg', N'CJ Entertainment', 2019),

-- 19
('1917-2019', N'1917',
 N'Soldiers race against time to deliver a critical message during World War I.',
 119, N'Publish', N'/uploads/posters/1917-2019.jpg', N'Universal Pictures', 2019),

-- 20
('ford-v-ferrari-2019', N'Ford v Ferrari',
 N'Ford challenges Ferrari at Le Mans in the 1960s.',
 152, N'Publish', N'/uploads/posters/ford-v-ferrari-2019.jpg', N'20th Century Fox', 2019),

-- 21
('dune-2021', N'Dune',
 N'A young noblemen leads his people on a desert planet.',
 155, N'Publish', N'/uploads/posters/dune-2021.jpg', N'Legendary Pictures', 2021),

-- 22
('no-time-to-die-2021', N'No Time To Die',
 N'James Bond comes out of retirement for one last mission.',
 163, N'Publish', N'/uploads/posters/no-time-to-die-2021.jpg', N'Eon Productions', 2021),

-- 23
('spider-man-no-way-home-2021', N'Spider-Man: No Way Home',
 N'Spider-Man''s life is turned upside down after his identity is revealed.',
 148, N'Publish', N'/uploads/posters/spider-man-no-way-home-2021.jpg', N'Marvel Studios', 2021),

-- 24
('spotlight-2015', N'Spotlight',
 N'Reporters investigate a decades-long cover-up of sexual abuse inside the Catholic Church.',
 129, N'Publish', N'/uploads/posters/spotlight-2015.jpg', N'Participant Media', 2015),

-- 25
('whiplash-2014', N'Whiplash',
 N'A young drummer faces abusive mentorship in pursuit of greatness.',
 107, N'Publish', N'/uploads/posters/whiplash-2014.jpg', N'Bold Films', 2014),

-- 26
('inside-out-2015', N'Inside Out',
 N'Teenage girl''s emotions struggle to guide her through life.',
 95, N'Publish', N'/uploads/posters/inside-out-2015.jpg', N'Pixar Animation Studios', 2015),

-- 27
('coco-2017', N'Coco',
 N'A boy journeys to the Land of the Dead on Día de los Muertos.',
 105, N'Publish', N'/uploads/posters/coco-2017.jpg', N'Pixar Animation Studios', 2017),

-- 28
('frozen-2013', N'Frozen',
 N'Anna sets out to find her sister Elsa, whose powers have trapped their kingdom in eternal winter.',
 102, N'Publish', N'/uploads/posters/frozen-2013.jpg', N'Walt Disney Pictures', 2013),

-- 29
('the-social-network-2010', N'The Social Network',
 N'Story about founding Facebook and its legal battles.',
 120, N'Publish', N'/uploads/posters/the-social-network-2010.jpg', N'Columbia Pictures', 2010),

-- 30
('the-shape-of-water-2017', N'The Shape of Water',
 N'A mute woman falls in love with a mysterious aquatic creature.',
 123, N'Publish', N'/uploads/posters/the-shape-of-water-2017.jpg', N'Fox Searchlight', 2017),

-- 31
('moonlight-2016', N'Moonlight',
 N'Chronicles the life of a young black man grappling with his identity.',
 111, N'Publish', N'/uploads/posters/moonlight-2016.jpg', N'A24', 2016),

-- 32
('arrival-2016', N'Arrival',
 N'A linguist attempts to communicate with mysterious extraterrestrials.',
 116, N'Publish', N'/uploads/posters/arrival-2016.jpg', N'Paramount Pictures', 2016),

-- 33
('knives-out-2019', N'Knives Out',
 N'A detective investigates the death of a wealthy patriarch.',
 130, N'Publish', N'/uploads/posters/knives-out-2019.jpg', N'Lionsgate', 2019),

-- 34
('get-out-2017', N'Get Out',
 N'A black man uncovers disturbing secrets when visiting his girlfriend''s family.',
 104, N'Publish', N'/uploads/posters/get-out-2017.jpg', N'Blumhouse Productions', 2017),

-- 35
('capernaum-2018', N'Capernaum',
 N'A Lebanese boy sues his parents for the crime of giving him life.',
 126, N'Publish', N'/uploads/posters/capernaum-2018.jpg', N'Roadside Attractions', 2018),

-- 36
('shoplifters-2018', N'Shoplifters',
 N'A non-biological family survives by shoplifting in Tokyo.',
 121, N'Publish', N'/uploads/posters/shoplifters-2018.jpg', N'GAGA', 2018),

-- 37
('soul-2020', N'Soul',
 N'A jazz musician gets a second chance at life.',
 100, N'Publish', N'/uploads/posters/soul-2020.jpg', N'Pixar Animation Studios', 2020),

-- 38
('the-gran-budapest-hotel-2014', N'The Grand Budapest Hotel',
 N'Mischief and murder unfold at a lavish hotel between world wars.',
 99, N'Publish', N'/uploads/posters/the-gran-budapest-hotel-2014.jpg', N'Fox Searchlight', 2014),

-- 39
('her-2013', N'Her',
 N'A lonely man falls in love with an AI assistant.',
 126, N'Publish', N'/uploads/posters/her-2013.jpg', N'Annapurna Pictures', 2013),

-- 40
('gravity-2013', N'Gravity',
 N'Two astronauts attempt to survive after their shuttle is destroyed.',
 91, N'Publish', N'/uploads/posters/gravity-2013.jpg', N'Warner Bros.', 2013),

-- 41
('up-2009', N'Up',
 N'An old man ties thousands of balloons to his house and flies to South America.',
 96, N'Publish', N'/uploads/posters/up-2009.jpg', N'Pixar Animation Studios', 2009),

-- 42
('the-avengers-2012', N'The Avengers',
 N'Earth''s mightiest heroes must unite to stop an alien invasion.',
 143, N'Publish', N'/uploads/posters/the-avengers-2012.jpg', N'Marvel Studios', 2012),

-- 43
('skyfall-2012', N'Skyfall',
 N'James Bond investigates an attack on MI6.',
 143, N'Publish', N'/uploads/posters/skyfall-2012.jpg', N'Eon Productions', 2012),

-- 44
('the-wolf-of-wall-street-2013', N'The Wolf of Wall Street',
 N'Based on Jordan Belfort''s rise and fall as a stockbroker.',
 180, N'Publish', N'/uploads/posters/the-wolf-of-wall-street-2013.jpg', N'Paramount Pictures', 2013),

-- 45
('the-martian-2015', N'The Martian',
 N'An astronaut is left behind and must survive on Mars.',
 144, N'Publish', N'/uploads/posters/the-martian-2015.jpg', N'20th Century Fox', 2015),

-- 46
('the-revenant-2015', N'The Revenant',
 N'A frontiersman fights for survival after being mauled by a bear.',
 156, N'Publish', N'/uploads/posters/the-revenant-2015.jpg', N'Regency Enterprises', 2015),

-- 47
('logan-2017', N'Logan',
 N'An aging Wolverine and a young mutant must survive together.',
 137, N'Publish', N'/uploads/posters/logan-2017.jpg', N'20th Century Fox', 2017),

-- 48
('guardians-of-the-galaxy-2014', N'Guardians of the Galaxy',
 N'A group of intergalactic criminals must save the universe.',
 121, N'Publish', N'/uploads/posters/guardians-of-the-galaxy-2014.jpg', N'Marvel Studios', 2014),

-- 49
('doctor-strange-2016', N'Doctor Strange',
 N'A surgeon discovers the world of mystic arts after an accident.',
 115, N'Publish', N'/uploads/posters/doctor-strange-2016.jpg', N'Marvel Studios', 2016),

-- 50
('wonder-woman-2017', N'Wonder Woman',
 N'Diana, an Amazonian princess, discovers her powers and destiny.',
 141, N'Publish', N'/uploads/posters/wonder-woman-2017.jpg', N'DC Films', 2017);

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