USE db_movie
GO


DECLARE @HallCursor CURSOR;
DECLARE @HallId BIGINT;
SET @HallCursor = CURSOR FOR SELECT Hall_ID FROM dbo.Halls;
OPEN @HallCursor;
FETCH NEXT FROM @HallCursor INTO @HallId;

WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @Rows NVARCHAR(12) = N'ABCDEFGHIJKL';
    DECLARE @RowIdx INT = 1;

    WHILE @RowIdx <= 12
    BEGIN
        DECLARE @SeatType NVARCHAR(10) = CASE WHEN @RowIdx <= 4 THEN N'Normal' ELSE N'VIP' END;
        DECLARE @SeatPrice DECIMAL(19,0) = CASE WHEN @SeatType = N'Normal' THEN 60000 ELSE 65000 END;

        DECLARE @SeatNum INT = 1;
        WHILE @SeatNum <= 12
        BEGIN
            INSERT INTO dbo.Seats (Hall_ID, RowNumber, SeatNumber, SeatType, Description, SeatStatus, SeatPrice)
            VALUES(@HallId,
                SUBSTRING(@Rows, @RowIdx, 1),
                RIGHT('00' + CAST(@SeatNum AS NVARCHAR(2)), 2),
                @SeatType,
                NULL,
                N'Available',
                @SeatPrice);

            SET @SeatNum = @SeatNum + 1;
        END

        SET @RowIdx = @RowIdx + 1;
    END

    FETCH NEXT FROM @HallCursor INTO @HallId;
END

CLOSE @HallCursor;
DEALLOCATE @HallCursor;
