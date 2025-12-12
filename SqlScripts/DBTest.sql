SET NOCOUNT ON;
USE EventManagementDB;

-- PRINT HEADER
PRINT 'Starting Data Population (Target: ~10-20 rows per table)';

-----------------------------------------
-- BLOCK 1: Roles (Fixed: 5 rows)
-----------------------------------------
PRINT 'Block 1: Insert Roles';
-- These are static reference data, so we keep them fixed
INSERT INTO ems.Role (RoleName, Description)
VALUES ('President','Top-level admin'),
       ('Vice President','Second level admin'),
       ('General Member','Normal registered member'),
       ('Treasurer','Handles budgets'),
       ('Event Coordinator','Manages event operations');

-----------------------------------------
-- BLOCK 2: Members (Target: 15 rows)
-----------------------------------------
PRINT 'Block 2: Insert Members';

;WITH N AS (
    SELECT TOP (15) 
        ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS seq
    FROM sys.all_objects a
    CROSS JOIN sys.all_objects b
)
INSERT INTO ems.Member (FullName, Email, PhoneNumber, RoleID, PasswordHash)
SELECT
    CONCAT('Member ', seq),
    CONCAT('member', seq, '@example.com'),
    -- Generates a random-looking phone number
    CONCAT('+92', RIGHT('000000000' + CONVERT(varchar(9), ABS(CHECKSUM(NEWID())) % 1000000000), 9)),
    -- Assigns roles 1-5 cyclically to ensure we have Admins, Treasurers, etc.
    ((seq - 1) % 5) + 1,
    LEFT(CONVERT(varchar(100), NEWID()), 40)
FROM N;

DECLARE @MemberCount INT = (SELECT COUNT(*) FROM ems.Member);
PRINT CONCAT('MemberCount = ', @MemberCount);

-----------------------------------------
-- BLOCK 3: Budgets (Target: 10 rows)
-----------------------------------------
PRINT 'Block 3: Insert Budgets';

;WITH N AS (
    SELECT TOP (10) 
        ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS seq
    FROM sys.all_objects a CROSS JOIN sys.all_objects b
)
INSERT INTO ems.Budget (BudgetName, TotalFunds)
SELECT
    CONCAT('Budget ', seq),
    -- Random fund between 5,000 and 55,000
    CAST(5000 + (ABS(CHECKSUM(NEWID())) % 50000) AS DECIMAL(14,2))
FROM N;

DECLARE @BudgetCount INT = (SELECT COUNT(*) FROM ems.Budget);

-----------------------------------------
-- BLOCK 4: Events (Target: 15 rows - Mixed Past & Future)
-----------------------------------------
PRINT 'Block 4: Insert Events';

;WITH N AS (
    SELECT TOP (15) 
        ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS seq
    FROM sys.all_objects a CROSS JOIN sys.all_objects b
)
INSERT INTO ems.[Event] (Title, Description, EventDate, Venue, OrganizedBy, BudgetAllocated, Capacity)
SELECT
    CONCAT('Event ', seq),
    CONCAT('Description for event ', seq),
    -- [CRITICAL UPDATE]: Generates dates +/- 100 days from TODAY. 
    -- This guarantees you have both 'Upcoming' and 'Past' events for your dashboard.
    DATEADD(day, (ABS(CHECKSUM(NEWID())) % 200) - 100, GETDATE()),
    CONCAT('Venue ', ((seq - 1) % 5) + 1), 
    -- Picks a random valid MemberID
    ((ABS(CHECKSUM(NEWID())) % @MemberCount) + 1),
    CAST(100 + (ABS(CHECKSUM(NEWID())) % 5000) AS DECIMAL(14,2)),
    (ABS(CHECKSUM(NEWID())) % 100) + 10
FROM N;

DECLARE @EventCount INT = (SELECT COUNT(*) FROM ems.[Event]);

-----------------------------------------
-- BLOCK 5: EventBudget (Linked to Events)
-----------------------------------------
PRINT 'Block 5: Insert EventBudget';

-- Links events to budgets sequentially
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
-- BLOCK 6: Expenses (Target: 20 rows)
-----------------------------------------
PRINT 'Block 6: Insert Expenses';

;WITH N AS (
    SELECT TOP (20) 
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
-- BLOCK 7: Participation (Target: 20 rows)
-----------------------------------------
PRINT 'Block 7: Insert Participation';

DECLARE @TotalParticipation INT = 20;

;WITH Numbers AS (
    SELECT TOP (@TotalParticipation)
        ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS seq
    FROM sys.all_objects a CROSS JOIN sys.all_objects b
)
INSERT INTO ems.Participation (EventID, MemberID, Status)
SELECT
    -- Distributes events cyclically
    ((seq - 1) % @EventCount) + 1 AS EventID,
    
    -- Distributes members carefully to avoid Unique Key violations
    ((seq - 1) / @EventCount) + 1 AS MemberID,

    CASE (seq % 3)
        WHEN 0 THEN 'Cancelled'
        WHEN 1 THEN 'Attended'
        ELSE 'Registered'
    END AS Status
FROM Numbers;

-----------------------------------------
-- BLOCK 8: Attendance (Subset of Participation)
-----------------------------------------
PRINT 'Block 8: Insert Attendance';

DECLARE @AvailAtt INT = (SELECT COUNT(*) FROM ems.Participation WHERE Status = 'Attended');
DECLARE @ToInsert INT = CASE WHEN @AvailAtt > 15 THEN 15 ELSE @AvailAtt END; 

INSERT INTO ems.Attendance (ParticipationID, CheckInTime)
SELECT TOP (@ToInsert)
    ParticipationID,
    DATEADD(minute, ABS(CHECKSUM(NEWID())) % 60, SYSUTCDATETIME())
FROM ems.Participation
WHERE Status = 'Attended'
ORDER BY NEWID();

-----------------------------------------
-- BLOCK 9: Announcements (Target: 10 rows)
-----------------------------------------
PRINT 'Block 9: Insert Announcements';

;WITH N AS (
    SELECT TOP (10) 
        ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS seq
    FROM sys.all_objects a CROSS JOIN sys.all_objects b
)
INSERT INTO ems.Announcement (Title, Message, CreatedBy)
SELECT
    CONCAT('Announcement ', seq),
    CONCAT('This is the message body for announcement number ', seq),
    ((ABS(CHECKSUM(NEWID())) % @MemberCount) + 1)
FROM N;

-----------------------------------------
-- BLOCK 10: Notifications (Target: 10 rows)
-----------------------------------------
PRINT 'Block 10: Insert Notifications';

;WITH N AS (
    SELECT TOP (10) 
        ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS seq
    FROM sys.all_objects a CROSS JOIN sys.all_objects b
)
INSERT INTO ems.Notification (MemberID, Message)
SELECT
    ((ABS(CHECKSUM(NEWID())) % @MemberCount) + 1),
    CONCAT('You have a new notification ID ', seq)
FROM N;

-----------------------------------------
-- BLOCK 11: Pictures (Target: 10 rows)
-----------------------------------------
PRINT 'Block 11: Insert Pictures';

;WITH N AS (
    SELECT TOP (10) 
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
PRINT '---------------------------------';
PRINT 'Block 12: Summary Row Counts';

SELECT 
    s.name AS SchemaName,
    t.name AS TableName,
    SUM(p.rows) AS TotalRows
FROM sys.tables t
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
INNER JOIN sys.partitions p ON t.object_id = p.object_id
WHERE s.name = 'ems'
  AND p.index_id IN (0,1)
GROUP BY s.name, t.name
ORDER BY SUM(p.rows) DESC;

PRINT 'Data generation completed successfully.';