SET NOCOUNT ON;
SET STATISTICS IO ON;
SET STATISTICS TIME ON;

PRINT '=== 1A. Filtered page (Status = InProgress, newest first, page 3) - with indexes ===';
EXEC sp_executesql N'
SELECT r.Id, r.Title, r.OrganizationName, r.Status, r.Priority, r.AssignedTo, r.CreatedAt, r.UpdatedAt, r.RowVersion
FROM Requests AS r
WHERE r.Status = @status
ORDER BY r.CreatedAt DESC, r.Id DESC
OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY',
N'@status tinyint, @offset int, @pageSize int', @status = 2, @offset = 40, @pageSize = 20;

PRINT '=== 1B. Filtered page - forced clustered scan (no secondary index) ===';
EXEC sp_executesql N'
SELECT r.Id, r.Title, r.OrganizationName, r.Status, r.Priority, r.AssignedTo, r.CreatedAt, r.UpdatedAt, r.RowVersion
FROM Requests AS r WITH (INDEX(0))
WHERE r.Status = @status
ORDER BY r.CreatedAt DESC, r.Id DESC
OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY',
N'@status tinyint, @offset int, @pageSize int', @status = 2, @offset = 40, @pageSize = 20;

PRINT '=== 2A. Filtered count (Status = InProgress) - with indexes ===';
EXEC sp_executesql N'SELECT COUNT(*) FROM Requests AS r WHERE r.Status = @status',
N'@status tinyint', @status = 2;

PRINT '=== 2B. Filtered count - forced clustered scan ===';
EXEC sp_executesql N'SELECT COUNT(*) FROM Requests AS r WITH (INDEX(0)) WHERE r.Status = @status',
N'@status tinyint', @status = 2;

PRINT '=== 3A. Summary aggregation (GROUP BY Status) - with indexes ===';
SELECT r.Status, COUNT(*) AS [Count] FROM Requests AS r GROUP BY r.Status;

PRINT '=== 3B. Summary aggregation - forced clustered scan ===';
SELECT r.Status, COUNT(*) AS [Count] FROM Requests AS r WITH (INDEX(0)) GROUP BY r.Status;

PRINT '=== 4. Text search (LIKE %term%) with Priority filter - bottleneck candidate ===';
EXEC sp_executesql N'
SELECT r.Id, r.Title, r.OrganizationName, r.Status, r.Priority, r.AssignedTo, r.CreatedAt, r.UpdatedAt, r.RowVersion
FROM Requests AS r
WHERE r.Priority = @priority AND (r.Title LIKE @search ESCAPE N''\'' OR r.OrganizationName LIKE @search ESCAPE N''\'')
ORDER BY r.CreatedAt DESC, r.Id DESC
OFFSET 0 ROWS FETCH NEXT 20 ROWS ONLY',
N'@priority tinyint, @search nvarchar(200)', @priority = 3, @search = N'%permit%';

PRINT '=== 5. Deep page (OFFSET 50000) - bottleneck candidate ===';
EXEC sp_executesql N'
SELECT r.Id, r.Title, r.OrganizationName, r.Status, r.Priority, r.AssignedTo, r.CreatedAt, r.UpdatedAt, r.RowVersion
FROM Requests AS r
ORDER BY r.CreatedAt DESC, r.Id DESC
OFFSET @offset ROWS FETCH NEXT 20 ROWS ONLY',
N'@offset int', @offset = 50000;

SET STATISTICS IO OFF;
SET STATISTICS TIME OFF;
