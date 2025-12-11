USE master;
IF DB_ID('EventManagementDB') IS NOT NULL
BEGIN
    ALTER DATABASE EventManagementDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE EventManagementDB;
END;
GO

CREATE DATABASE EventManagementDB;
GO
USE EventManagementDB;
GO

CREATE SCHEMA ems;
GO

--partition scheme and fucntion
CREATE PARTITION FUNCTION pfDateRange (DATETIME2)
AS RANGE RIGHT FOR VALUES (
    ('2023-01-01'),
    ('2024-01-01'),
    ('2025-01-01'),
    ('2026-01-01')
);
GO

CREATE PARTITION SCHEME psDateRange
AS PARTITION pfDateRange
ALL TO ([PRIMARY]);
GO

--table definitions
CREATE TABLE ems.Role (
    RoleID INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(255)
);
GO


CREATE TABLE ems.Member (
    MemberID INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(150) NOT NULL,
    Email NVARCHAR(150) NOT NULL UNIQUE,
    PhoneNumber NVARCHAR(20),
    RoleID INT NOT NULL,
    JoinDate DATE NOT NULL DEFAULT (CONVERT(DATE, GETDATE())),
    PasswordHash NVARCHAR(255) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Member_Role FOREIGN KEY (RoleID)
        REFERENCES ems.Role(RoleID) ON UPDATE CASCADE
);
GO


CREATE TABLE ems.Budget (
    BudgetID INT IDENTITY(1,1) PRIMARY KEY,
    BudgetName NVARCHAR(150) NOT NULL,
    TotalFunds DECIMAL(14,2) NOT NULL CHECK (TotalFunds >= 0),
    FundsUsed DECIMAL(14,2) NOT NULL DEFAULT 0 CHECK (FundsUsed >= 0),
    RemainingFunds AS (TotalFunds - FundsUsed) PERSISTED,
    LastUpdated DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO


CREATE TABLE ems.[Event] (
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(150) NOT NULL,
    Description NVARCHAR(MAX),
    EventDate DATETIME2 NOT NULL,
    Venue NVARCHAR(150) NOT NULL,
    OrganizedBy INT NOT NULL,
    BudgetAllocated DECIMAL(14,2) NOT NULL CHECK (BudgetAllocated >= 0),
    Capacity INT NOT NULL CHECK (Capacity > 0),
    CONSTRAINT FK_Event_Organizer FOREIGN KEY (OrganizedBy)
        REFERENCES ems.Member(MemberID)
);
GO


CREATE TABLE ems.EventBudget (
    EventBudgetID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL,
    BudgetID INT NOT NULL,
    AmountAllocated DECIMAL(14,2) NOT NULL CHECK (AmountAllocated >= 0),
    CONSTRAINT UQ_EventBudget_Budget UNIQUE (BudgetID),
    CONSTRAINT FK_EventBudget_Event FOREIGN KEY (EventID)
        REFERENCES ems.[Event](EventID) ON DELETE CASCADE,
    CONSTRAINT FK_EventBudget_Budget FOREIGN KEY (BudgetID)
        REFERENCES ems.Budget(BudgetID)
);
GO


CREATE TABLE ems.Expense (
    ExpenseID INT IDENTITY(1,1) NOT NULL,
    BudgetID INT NOT NULL,
    Title NVARCHAR(150) NOT NULL,
    Amount DECIMAL(14,2) NOT NULL CHECK (Amount >= 0),
    ExpenseDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy INT NOT NULL,
    CONSTRAINT PK_Expense PRIMARY KEY CLUSTERED (ExpenseID, ExpenseDate),
    CONSTRAINT FK_Expense_Budget FOREIGN KEY (BudgetID)
        REFERENCES ems.Budget(BudgetID),
    CONSTRAINT FK_Expense_Member FOREIGN KEY (CreatedBy)
        REFERENCES ems.Member(MemberID)
) ON psDateRange(ExpenseDate);
GO


CREATE TABLE ems.Participation (
    ParticipationID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL,
    MemberID INT NOT NULL,
    Status NVARCHAR(50) NOT NULL CHECK (Status IN ('Registered','Attended','Cancelled')),
    RegistrationDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Part_Event FOREIGN KEY (EventID)
        REFERENCES ems.[Event](EventID) ON DELETE CASCADE,
    CONSTRAINT FK_Part_Member FOREIGN KEY (MemberID)
        REFERENCES ems.Member(MemberID) ON DELETE CASCADE,
    CONSTRAINT UQ_Participation UNIQUE (EventID, MemberID)
);
GO


CREATE TABLE ems.Attendance (
    AttendanceID INT IDENTITY(1,1) PRIMARY KEY,
    ParticipationID INT NOT NULL,
    CheckInTime DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Attendance_Participation FOREIGN KEY (ParticipationID)
        REFERENCES ems.Participation(ParticipationID) ON DELETE CASCADE
);
GO


CREATE TABLE ems.Announcement (
    AnnouncementID INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(150) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    EventID INT NULL,
    CreatedBy INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Announcement_Event FOREIGN KEY (EventID)
        REFERENCES ems.[Event](EventID) ON DELETE CASCADE,
    CONSTRAINT FK_Announcement_Member FOREIGN KEY (CreatedBy)
        REFERENCES ems.Member(MemberID)
);
GO


CREATE TABLE ems.Notification (
    NotificationID INT IDENTITY(1,1) PRIMARY KEY,
    MemberID INT NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    IsRead BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Notification_Member FOREIGN KEY (MemberID)
        REFERENCES ems.Member(MemberID) ON DELETE CASCADE
);
GO


CREATE TABLE ems.Pictures (
    PictureID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL,
    FilePath NVARCHAR(255) NOT NULL,
    UploadedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Pictures_Event FOREIGN KEY (EventID)
        REFERENCES ems.[Event](EventID) ON DELETE CASCADE
);
GO


CREATE TABLE ems.AuditLog (
    LogID INT IDENTITY(1,1) NOT NULL,
    TableName NVARCHAR(150) NOT NULL,
    Action NVARCHAR(50) NOT NULL,
    RecordID INT NOT NULL,
    PerformedBy INT NULL,
    TimeStamp DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_AuditLog PRIMARY KEY CLUSTERED (LogID, TimeStamp)
) ON psDateRange(TimeStamp);
GO

--index definitions
CREATE NONCLUSTERED INDEX IX_Participation_Event_Status
ON ems.Participation (EventID, Status)
INCLUDE (MemberID, RegistrationDate);
GO

CREATE NONCLUSTERED INDEX IX_Notification_Unread
ON ems.Notification (MemberID)
WHERE IsRead = 0;
GO

CREATE NONCLUSTERED INDEX IX_Expense_Budget_Date
ON ems.Expense (BudgetID, ExpenseDate)
INCLUDE (Amount, Title, CreatedBy);
GO


--function definitions
CREATE FUNCTION ems.fn_GetEventTotalExpenses (@EventID INT)
RETURNS DECIMAL(14,2)
AS
BEGIN
    DECLARE @Total DECIMAL(14,2);
    SELECT @Total = SUM(E.Amount)
    FROM ems.Expense E
    INNER JOIN ems.EventBudget EB ON EB.BudgetID = E.BudgetID
    WHERE EB.EventID = @EventID;
    RETURN ISNULL(@Total, 0);
END;
GO

CREATE FUNCTION ems.fn_GetMemberAttendanceRate (@MemberID INT)
RETURNS DECIMAL(5,2)
AS
BEGIN
    DECLARE @Rate DECIMAL(5,2);
    DECLARE @Registered INT;
    DECLARE @Attended INT;

    SELECT @Registered = COUNT(*)
    FROM ems.Participation
    WHERE MemberID = @MemberID;

    SELECT @Attended = COUNT(*)
    FROM ems.Participation P
    INNER JOIN ems.Attendance A ON A.ParticipationID = P.ParticipationID
    WHERE P.MemberID = @MemberID;

    IF @Registered = 0 SET @Rate = 0;
    ELSE SET @Rate = (@Attended * 100.0) / @Registered;
    RETURN @Rate;
END;
GO

CREATE FUNCTION ems.fn_GetEventParticipationCount 
(
    @EventID INT,
    @Status NVARCHAR(50)
)
RETURNS INT
AS
BEGIN
    DECLARE @Count INT;

    SELECT @Count = COUNT(*)
    FROM ems.Participation
    WHERE EventID = @EventID
      AND Status = @Status;
    RETURN ISNULL(@Count, 0);
END;
GO

--view definitions
CREATE VIEW ems.v_EventFinancialSummary
AS
SELECT 
    E.EventID,
    E.Title AS EventTitle,
    E.EventDate,
    M.FullName AS OrganizedBy,
    SUM(EB.AmountAllocated) AS TotalAllocated,
    ems.fn_GetEventTotalExpenses(E.EventID) AS TotalExpenses,
    (SUM(EB.AmountAllocated) - ems.fn_GetEventTotalExpenses(E.EventID)) AS RemainingAmount
FROM ems.[Event] E
INNER JOIN ems.Member M ON M.MemberID = E.OrganizedBy
INNER JOIN ems.EventBudget EB ON EB.EventID = E.EventID
GROUP BY 
    E.EventID, E.Title, E.EventDate, M.FullName;
GO

CREATE VIEW ems.v_MemberParticipationSummary
AS
SELECT 
    M.MemberID,
    M.FullName,
    R.RoleName,
    ems.fn_GetMemberAttendanceRate(M.MemberID) AS AttendanceRate,
    (SELECT COUNT(*) 
     FROM ems.Participation P 
     WHERE P.MemberID = M.MemberID AND P.Status = 'Registered') AS TotalRegistered,
    (SELECT COUNT(*) 
     FROM ems.Participation P 
     WHERE P.MemberID = M.MemberID AND P.Status = 'Attended') AS TotalAttended,
    (SELECT COUNT(*) 
     FROM ems.Participation P 
     WHERE P.MemberID = M.MemberID AND P.Status = 'Cancelled') AS TotalCancelled
FROM ems.Member M
INNER JOIN ems.Role R ON R.RoleID = M.RoleID;
GO

CREATE VIEW ems.v_UpcomingEvents
AS
SELECT 
    E.EventID,
    E.Title,
    E.EventDate,
    E.Venue,
    M.FullName AS Organizer,
    (SELECT COUNT(*)
     FROM ems.Participation P
     WHERE P.EventID = E.EventID
       AND P.Status = 'Registered') AS RegisteredCount
FROM ems.[Event] E
INNER JOIN ems.Member M ON M.MemberID = E.OrganizedBy
WHERE E.EventDate > SYSUTCDATETIME();
GO


--Stored Procedures
CREATE PROCEDURE ems.sp_RegisterMemberForEvent
    @EventID  INT,
    @MemberID INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Capacity INT;
    DECLARE @CurrentCount INT;
    SELECT @Capacity = Capacity
    FROM ems.[Event]
    WHERE EventID = @EventID;

    IF @Capacity IS NULL
    BEGIN
        RAISERROR ('Event does not exist', 16, 1);
        RETURN;
    END

    IF EXISTS (
        SELECT 1 
        FROM ems.Participation
        WHERE EventID = @EventID
          AND MemberID = @MemberID
    )
    BEGIN
        RAISERROR ('Member is already registered for this event', 16, 1);
        RETURN;
    END

    SELECT @CurrentCount = COUNT(*)
    FROM ems.Participation
    WHERE EventID = @EventID
      AND Status IN ('Registered', 'Attended');

    IF @CurrentCount >= @Capacity
    BEGIN
        RAISERROR ('Event capacity has been reached.', 16, 1);
        RETURN;
    END

    INSERT INTO ems.Participation (EventID, MemberID, Status)
    VALUES (@EventID, @MemberID, 'Registered');
    INSERT INTO ems.Notification (MemberID, Message)
    VALUES (@MemberID, CONCAT('You have been registered for event ID ', @EventID));
END;
GO

CREATE PROCEDURE ems.sp_AddExpenseWithBudgetCheck
    @BudgetID  INT,
    @Title     NVARCHAR(150),
    @Amount    DECIMAL(14,2),
    @CreatedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @TotalFunds DECIMAL(14,2);
    DECLARE @FundsUsed  DECIMAL(14,2);

    SELECT 
        @TotalFunds = TotalFunds,
        @FundsUsed  = FundsUsed
    FROM ems.Budget
    WHERE BudgetID = @BudgetID;

    IF @TotalFunds IS NULL
    BEGIN
        RAISERROR ('Budget does not exist.', 16, 1);
        RETURN;
    END

    IF (@TotalFunds - @FundsUsed) < @Amount
    BEGIN
        RAISERROR ('Insufficient remaining funds for this budget.', 16, 1);
        RETURN;
    END

    INSERT INTO ems.Expense (BudgetID, Title, Amount, CreatedBy)
    VALUES (@BudgetID, @Title, @Amount, @CreatedBy);
END;
GO

CREATE PROCEDURE ems.sp_CreateEventWithBudget
    @Title           NVARCHAR(150),
    @Description     NVARCHAR(MAX),
    @EventDate       DATETIME2,
    @Venue           NVARCHAR(150),
    @OrganizedBy     INT,
    @Capacity        INT,
    @BudgetID        INT,
    @AmountAllocated DECIMAL(14,2)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @EventID INT;
    DECLARE @TotalFunds DECIMAL(14,2);
    DECLARE @FundsUsed  DECIMAL(14,2);

    SELECT 
        @TotalFunds = TotalFunds,
        @FundsUsed  = FundsUsed
    FROM ems.Budget
    WHERE BudgetID = @BudgetID;

    IF @TotalFunds IS NULL
    BEGIN
        RAISERROR ('Budget does not exist.', 16, 1);
        RETURN;
    END

    IF (@TotalFunds - @FundsUsed) < @AmountAllocated
    BEGIN
        RAISERROR ('Insufficient remaining funds to allocate to this event.', 16, 1);
        RETURN;
    END

    BEGIN TRAN;
    INSERT INTO ems.[Event] 
        (Title, Description, EventDate, Venue, OrganizedBy, BudgetAllocated, Capacity)
    VALUES
        (@Title, @Description, @EventDate, @Venue, @OrganizedBy, @AmountAllocated, @Capacity);

    SET @EventID = SCOPE_IDENTITY();
    INSERT INTO ems.EventBudget (EventID, BudgetID, AmountAllocated)
    VALUES (@EventID, @BudgetID, @AmountAllocated);
    COMMIT TRAN;
END;
GO

CREATE PROCEDURE ems.sp_CreateAnnouncementAndNotify
    @Title     NVARCHAR(150),
    @Message   NVARCHAR(MAX),
    @EventID   INT = NULL,
    @CreatedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @AnnouncementID INT;

    BEGIN TRAN;
    INSERT INTO ems.Announcement (Title, Message, EventID, CreatedBy)
    VALUES (@Title, @Message, @EventID, @CreatedBy);
    SET @AnnouncementID = SCOPE_IDENTITY();
    INSERT INTO ems.Notification (MemberID, Message)
    SELECT 
        M.MemberID,
        CONCAT('Announcement: ', @Title, ' - ', @Message)
    FROM ems.Member M
    WHERE M.IsActive = 1;
    COMMIT TRAN;
END;
GO

--trigger definitions
CREATE TRIGGER ems.tr_Expense_AfterInsert
ON ems.Expense
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    WITH BudgetDelta AS (
        SELECT BudgetID, SUM(Amount) AS TotalAmount
        FROM inserted
        GROUP BY BudgetID
    )
    UPDATE B
    SET 
        B.FundsUsed = B.FundsUsed + D.TotalAmount,
        B.LastUpdated = SYSUTCDATETIME()
    FROM ems.Budget B
    INNER JOIN BudgetDelta D ON D.BudgetID = B.BudgetID;

    INSERT INTO ems.AuditLog (TableName, Action, RecordID, PerformedBy)
    SELECT 
        'Expense',
        'INSERT',
        I.ExpenseID,
        I.CreatedBy
    FROM inserted I;
END;
GO

CREATE TRIGGER ems.tr_Expense_AfterUpdate
ON ems.Expense
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    WITH OldAmounts AS (
        SELECT BudgetID, SUM(Amount) AS TotalAmount
        FROM deleted
        GROUP BY BudgetID
    ),
    NewAmounts AS (
        SELECT BudgetID, SUM(Amount) AS TotalAmount
        FROM inserted
        GROUP BY BudgetID
    )
    UPDATE B
    SET 
        B.FundsUsed = B.FundsUsed - ISNULL(O.TotalAmount, 0) + ISNULL(N.TotalAmount, 0),
        B.LastUpdated = SYSUTCDATETIME()
    FROM ems.Budget B
    LEFT JOIN OldAmounts O ON O.BudgetID = B.BudgetID
    LEFT JOIN NewAmounts N ON N.BudgetID = B.BudgetID
    WHERE O.BudgetID IS NOT NULL OR N.BudgetID IS NOT NULL;

    INSERT INTO ems.AuditLog (TableName, Action, RecordID, PerformedBy)
    SELECT 
        'Expense',
        'UPDATE',
        I.ExpenseID,
        I.CreatedBy
    FROM inserted I;
END;
GO

CREATE TRIGGER ems.tr_Event_InsteadOfDelete
ON ems.[Event]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO ems.AuditLog (TableName, Action, RecordID, PerformedBy)
    SELECT
        'Event',
        'DELETE',
        D.EventID,
        NULL
    FROM deleted D;

    DELETE E
    FROM ems.[Event] E
    INNER JOIN deleted D ON D.EventID = E.EventID;
END;
GO

--Common Table Expressions
CREATE PROCEDURE ems.sp_GetAttendanceTrend
AS
BEGIN
    SET NOCOUNT ON;

    WITH AttendanceByEventMonth AS (
        SELECT 
            E.EventID,
            E.Title AS EventTitle,
            YEAR(A.CheckInTime)  AS [Year],
            MONTH(A.CheckInTime) AS [Month],
            COUNT(*) AS TotalAttendance
        FROM ems.Attendance A
        INNER JOIN ems.Participation P ON P.ParticipationID = A.ParticipationID
        INNER JOIN ems.[Event] E       ON E.EventID = P.EventID
        GROUP BY 
            E.EventID,
            E.Title,
            YEAR(A.CheckInTime),
            MONTH(A.CheckInTime)
    )
    SELECT 
        EventID,
        EventTitle,
        [Year],
        [Month],
        TotalAttendance
    FROM AttendanceByEventMonth
    ORDER BY EventTitle, [Year], [Month];
END;
GO

CREATE PROCEDURE ems.sp_GetBudgetUtilizationSummary
AS
BEGIN
    SET NOCOUNT ON;

    WITH BudgetUsage AS (
        SELECT 
            B.BudgetID,
            B.BudgetName,
            B.TotalFunds,
            B.FundsUsed,
            B.RemainingFunds,
            CASE 
                WHEN B.TotalFunds = 0 THEN 0
                ELSE (B.FundsUsed * 100.0 / B.TotalFunds)
            END AS UsagePercent
        FROM ems.Budget B
    )
    SELECT
        BudgetID,
        BudgetName,
        TotalFunds,
        FundsUsed,
        RemainingFunds,
        UsagePercent,
        CASE 
            WHEN UsagePercent >= 100 THEN 'Overspent'
            WHEN UsagePercent >= 80  THEN 'High'
            WHEN UsagePercent >= 50  THEN 'Medium'
            ELSE 'Low'
        END AS RiskLevel
    FROM BudgetUsage
    ORDER BY UsagePercent DESC, BudgetName;
END;
GO
