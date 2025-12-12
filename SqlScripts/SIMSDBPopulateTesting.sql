SET NOCOUNT ON;
USE EventManagementDB;

-----------------------------------------
-- BLOCK 1: Roles (5)
-----------------------------------------
PRINT 'Block 1: Insert Roles';
INSERT INTO ems.Role (RoleName, Description)
VALUES ('President','Top-level admin'),
       ('Vice President','Second level admin'),
       ('General Member','Normal registered member'),
       ('Treasurer','Handles budgets'),
       ('Event Coordinator','Manages event operations');

-----------------------------------------
-- BLOCK 2: Members (3)
-----------------------------------------
PRINT 'Block 2: Insert Members';

INSERT INTO ems.Member (FullName, Email, PhoneNumber, RoleID, PasswordHash)
VALUES ('Alice Smith','alice@example.com','+921234567890',1,LEFT(CONVERT(varchar(100), NEWID()),40)),
       ('Bob Johnson','bob@example.com','+921234567891',3,LEFT(CONVERT(varchar(100), NEWID()),40)),
       ('Charlie Lee','charlie@example.com','+921234567892',2,LEFT(CONVERT(varchar(100), NEWID()),40));

DECLARE @MemberCount INT = (SELECT COUNT(*) FROM ems.Member);

-----------------------------------------
-- BLOCK 3: Events (5)
-----------------------------------------
PRINT 'Block 3: Insert Events';

INSERT INTO ems.[Event] (Title, Description, EventDate, Venue, OrganizedBy, BudgetAllocated, Capacity)
VALUES ('Event 1','Desc 1','2025-01-10','Venue 1',1,1000,50),
       ('Event 2','Desc 2','2025-01-12','Venue 2',2,1500,60),
       ('Event 3','Desc 3','2025-01-15','Venue 3',3,2000,70),
       ('Event 4','Desc 4','2025-01-18','Venue 4',1,1200,40),
       ('Event 5','Desc 5','2025-01-20','Venue 5',2,1800,80);

DECLARE @EventCount INT = (SELECT COUNT(*) FROM ems.[Event]);

-----------------------------------------
-- BLOCK 4: Participation (5)
-----------------------------------------
PRINT 'Block 4: Insert Participation';

INSERT INTO ems.Participation (EventID, MemberID, Status)
VALUES (1,1,'Attended'),
       (1,2,'Registered'),
       (2,2,'Attended'),
       (3,3,'Cancelled'),
       (4,1,'Attended');

-----------------------------------------
-- BLOCK 5: Notifications (3)
-----------------------------------------
PRINT 'Block 5: Insert Notifications';

INSERT INTO ems.Notification (MemberID, Message)
VALUES (1,'Notification 1 for Alice'),
       (2,'Notification 2 for Bob'),
       (3,'Notification 3 for Charlie');

-----------------------------------------
-- BLOCK 6: Summary
-----------------------------------------
PRINT 'Summary Row Counts';

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

PRINT 'Test data generation completed.';
