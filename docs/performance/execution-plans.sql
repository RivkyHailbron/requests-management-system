SET NOCOUNT ON;
GO
SET SHOWPLAN_TEXT ON;
GO
SELECT r.Id, r.Title, r.OrganizationName, r.Status, r.Priority, r.AssignedTo, r.CreatedAt, r.UpdatedAt, r.RowVersion FROM Requests AS r WHERE r.Status = 2 ORDER BY r.CreatedAt DESC, r.Id DESC OFFSET 40 ROWS FETCH NEXT 20 ROWS ONLY;
GO
SELECT r.Status, COUNT(*) AS [Count] FROM Requests AS r GROUP BY r.Status;
GO
SELECT r.Id, r.Title FROM Requests AS r WHERE r.Priority = 3 AND (r.Title LIKE N'%xyz-not-found%' OR r.OrganizationName LIKE N'%xyz-not-found%') ORDER BY r.CreatedAt DESC, r.Id DESC OFFSET 0 ROWS FETCH NEXT 20 ROWS ONLY;
GO
SELECT r.Id FROM Requests AS r ORDER BY r.CreatedAt DESC, r.Id DESC OFFSET 50000 ROWS FETCH NEXT 20 ROWS ONLY;
GO
SET SHOWPLAN_TEXT OFF;
GO
SET STATISTICS IO ON;
PRINT '=== rare search term (no matches) ===';
SELECT r.Id, r.Title FROM Requests AS r WHERE r.Priority = 3 AND (r.Title LIKE N'%xyz-not-found%' OR r.OrganizationName LIKE N'%xyz-not-found%') ORDER BY r.CreatedAt DESC, r.Id DESC OFFSET 0 ROWS FETCH NEXT 20 ROWS ONLY;
