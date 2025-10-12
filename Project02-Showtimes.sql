USE db_movie
GO

DECLARE @MovieIDs TABLE (Movie_ID BIGINT);
INSERT INTO @MovieIDs
SELECT Movie_ID FROM dbo.Movies WHERE Movie_Status = 'Publish';

DECLARE @HallIDs TABLE (Hall_ID BIGINT);
INSERT INTO @HallIDs
SELECT Hall_ID FROM dbo.Halls;

DECLARE @StartTime DATETIME2 = '2025-10-01 09:00:00';

DECLARE @mId BIGINT;
DECLARE MovieCursor CURSOR FOR SELECT Movie_ID FROM @MovieIDs;
OPEN MovieCursor;
FETCH NEXT FROM MovieCursor INTO @mId;

WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @count INT = 1;
    DECLARE @hallIndex INT = 1;
    WHILE @count <= 5
    BEGIN
        DECLARE @hId BIGINT = (SELECT Hall_ID FROM (
            SELECT ROW_NUMBER() OVER (ORDER BY Hall_ID) AS rn, Hall_ID FROM @HallIDs
        ) AS halls WHERE rn = @hallIndex);

        INSERT INTO dbo.Showtimes (Movie_ID, Hall_ID, Start_Time, End_Time, [Language], [Format])
        VALUES (@mId, @hId, DATEADD(HOUR, (@count - 1) * 3, @StartTime),
                DATEADD(HOUR, (@count - 1) * 3 + 2, @StartTime),
                N'Vietnamese', N'2D');

        SET @count = @count + 1;
        SET @hallIndex = CASE WHEN @hallIndex < (SELECT COUNT(*) FROM @HallIDs) THEN @hallIndex + 1 ELSE 1 END;
    END

    FETCH NEXT FROM MovieCursor INTO @mId;
END

CLOSE MovieCursor;
DEALLOCATE MovieCursor;
