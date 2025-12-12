SET NOCOUNT ON;
USE EventManagementDB;

-----------------------------------------
-- BLOCK 1: Roles
-----------------------------------------
PRINT 'Block 1: Insert Roles';
INSERT INTO ems.Role (RoleName, Description)
VALUES ('President','Top-level admin'),
       ('Vice President','Second level admin'),
       ('General Member','Normal registered member'),
       ('Treasurer','Handles budgets'),
       ('Event Coordinator','Manages event operations');

-----------------------------------------
-- BLOCK 2: Members (50k)
-----------------------------------------
PRINT 'Block 2: Insert Members';

;WITH N AS (
    SELECT TOP (50000)
        ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS seq
    FROM sys.all_objects a
    CROSS JOIN sys.all_objects b
)
INSERT INTO ems.Member (FullName, Email, PhoneNumber, RoleID, PasswordHash)
SELECT
    CONCAT('Member ', seq),
    CONCAT('member', seq, '@example.com'),
    CONCAT('+92', RIGHT('000000000' + CONVERT(varchar(9), ABS(CHECKSUM(NEWID())) % 1000000000), 9)),
    ((seq - 1) % (SELECT COUNT(*) FROM ems.Role)) + 1,
    LEFT(CONVERT(varchar(100), NEWID()), 40)
FROM N;

DECLARE @MemberCount INT = (SELECT COUNT(*) FROM ems.Member);
PRINT CONCAT('MemberCount = ', @MemberCount);

-----------------------------------------
-- BLOCK 3: Budgets (2k)
-----------------------------------------
PRINT 'Block 3: Insert Budgets';

;WITH N AS (
    SELECT TOP (2000)
        ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS seq
    FROM sys.all_objects a CROSS JOIN sys.all_objects b
)
INSERT INTO ems.Budget (BudgetName, TotalFunds)
SELECT
    CONCAT('Budget ', seq),
    CAST(5000 + (ABS(CHECKSUM(NEWID())) % 50000) AS DECIMAL(14,2))
FROM N;

DECLARE @BudgetCount INT = (SELECT COUNT(*) FROM ems.Budget);
PRINT CONCAT('BudgetCount = ', @BudgetCount);

-----------------------------------------
-- BLOCK 4: Events (30k)
-----------------------------------------
PRINT 'Block 4: Insert Events';

;WITH N AS (
    SELECT TOP (30000)
        ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS seq
    FROM sys.all_objects a CROSS JOIN sys.all_objects b
)
INSERT INTO ems.[Event] (Title, Description, EventDate, Venue, OrganizedBy, BudgetAllocated, Capacity)
SELECT
    CONCAT('Event ', seq),
    CONCAT('Description for event ', seq),
    DATEADD(day, ABS(CHECKSUM(NEWID())) % 1100, '2023-01-01'),
    CONCAT('Venue ', ((seq - 1) % 200) + 1),
    ((ABS(CHECKSUM(NEWID())) % @MemberCount) + 1),
    CAST(100 + (ABS(CHECKSUM(NEWID())) % 20000) AS DECIMAL(14,2)),
    (ABS(CHECKSUM(NEWID())) % 500) + 10
FROM N;

DECLARE @EventCount INT = (SELECT COUNT(*) FROM ems.[Event]);
PRINT CONCAT('EventCount = ', @EventCount);

-----------------------------------------
-- BLOCK 5: EventBudget
-----------------------------------------
PRINT 'Block 5: Insert EventBudget';

;WITH EventBudgetCTE AS (
    SELECT 
        E.EventID,
        B.BudgetID,
        E.BudgetAllocated AS AmountAllocated,
        ROW_NUMBER() OVER (ORDER BY E.EventID) AS rn
    FROM ems.[Event] E
    JOIN ems.Budget B ON B.BudgetID <= @BudgetCount
)
INSERT INTO ems.EventBudget (EventID, BudgetID, AmountAllocated)
SELECT EventID, BudgetID, AmountAllocated
FROM EventBudgetCTE
WHERE rn <= @BudgetCount;

-----------------------------------------
-- BLOCK 6: Expenses (500k)
-----------------------------------------
PRINT 'Block 6: Insert Expenses';

;WITH N AS (
    SELECT TOP (500000)
        ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS seq
    FROM sys.all_objects a CROSS JOIN sys.all_objects b
)
INSERT INTO ems.Expense (BudgetID, Title, Amount, CreatedBy)
SELECT
    ((ABS(CHECKSUM(NEWID())) % @BudgetCount) + 1),
    CONCAT('Expense ', seq),
    CAST((ABS(CHECKSUM(NEWID())) % 500) + 50 AS DECIMAL(14,2)),
    ((ABS(CHECKSUM(NEWID())) % @MemberCount) + 1)
FROM N;

-----------------------------------------
-- BLOCK 7: Participation (300k)
-----------------------------------------
PRINT 'Block 7: Insert Participation (Safe Deterministic)';

DECLARE @TotalParticipation INT = 300000;

;WITH Numbers AS (
    SELECT TOP (@TotalParticipation)
        ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS seq
    FROM sys.all_objects a CROSS JOIN sys.all_objects b
)
INSERT INTO ems.Participation (EventID, MemberID, Status)
SELECT
    ((seq - 1) % @EventCount) + 1 AS EventID,

    ((seq - 1) / @EventCount) + 1 AS MemberID,

    CASE (seq % 3)
        WHEN 0 THEN 'Cancelled'
        WHEN 1 THEN 'Attended'
        WHEN 2 THEN 'Attended'
        ELSE 'Registered'
    END AS Status
FROM Numbers;


-----------------------------------------
-- BLOCK 8: Attendance (200k)
-----------------------------------------
PRINT 'Block 8: Insert Attendance';

DECLARE @AvailAtt INT =
    (SELECT COUNT(*) FROM ems.Participation WHERE Status = 'Attended');

DECLARE @ToInsert INT =
    CASE WHEN @AvailAtt > 200000 THEN 200000 ELSE @AvailAtt END;

INSERT INTO ems.Attendance (ParticipationID, CheckInTime)
SELECT TOP (@ToInsert)
    ParticipationID,
    DATEADD(minute, ABS(CHECKSUM(NEWID())) % 525600, SYSUTCDATETIME())
FROM ems.Participation
WHERE Status = 'Attended'
ORDER BY NEWID();

-----------------------------------------
-- BLOCK 9: Announcements (10k)
-----------------------------------------
PRINT 'Block 9: Insert Announcements';

;WITH N AS (
    SELECT TOP (10000)
        ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS seq
    FROM sys.all_objects a CROSS JOIN sys.all_objects b
)
INSERT INTO ems.Announcement (Title, Message, CreatedBy)
SELECT
    CONCAT('Announcement ', seq),
    CONCAT('Message body for announcement ', seq),
    ((ABS(CHECKSUM(NEWID())) % @MemberCount) + 1)
FROM N;

-----------------------------------------
-- BLOCK 10: Notifications (150k)
-----------------------------------------
PRINT 'Block 10: Insert Notifications';

;WITH N AS (
    SELECT TOP (150000)
        ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS seq
    FROM sys.all_objects a CROSS JOIN sys.all_objects b
)
INSERT INTO ems.Notification (MemberID, Message)
SELECT
    ((ABS(CHECKSUM(NEWID())) % @MemberCount) + 1),
    CONCAT('Notification message ', seq)
FROM N;

-----------------------------------------
-- BLOCK 11: Pictures (20k)
-----------------------------------------
PRINT 'Block 11: Insert Pictures';

;WITH N AS (
    SELECT TOP (20000)
        ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS seq
    FROM sys.all_objects a CROSS JOIN sys.all_objects b
)
INSERT INTO ems.Pictures (EventID, FilePath)
SELECT
    ((ABS(CHECKSUM(NEWID())) % @EventCount) + 1),
    CONCAT('images/event_', seq, '.jpg')
FROM N;

-----------------------------------------
-- BLOCK 12: Summary
-----------------------------------------

-- BLOCK 12: Summary
PRINT 'Block 12: Summary Row Counts';

-- Query table counts
SELECT 
    s.name AS SchemaName,
    t.name AS TableName,
    SUM(p.rows) AS TotalRows
FROM sys.tables t
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
INNER JOIN sys.partitions p ON t.object_id = p.object_id
WHERE s.name = 'ems'
  AND p.index_id IN (0,1)  -- 0 = heap, 1 = clustered
GROUP BY s.name, t.name
ORDER BY SUM(p.rows) DESC;

PRINT 'Data generation completed.';
