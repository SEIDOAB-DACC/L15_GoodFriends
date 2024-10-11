use goodfriendsefc;
GO

--testing access to the spLogin
--Make sure to get Password and UserNameOrEmail from dbo.Users table

--Impersonate a Guest
EXECUTE AS USER = 'gstusrUser'; 

--Will work as gstusr is allowed to execute a stored procedure (and sp is accessing dbo schema)
DECLARE @UserId UNIQUEIDENTIFIER;
DECLARE @UserName  NVARCHAR(100);
DECLARE @Role NVARCHAR(100);

BEGIN TRY
    EXEC  gstusr.spLogin 'user30', '8IbDMcvK0H175Z1ob9c27V9oZt7fCDlVfUXELAMY0sYG10dzlq1oHkhPvnvx/JwbUBqAIB8JxkHj/9+pZvRsGQ==',
    @UserId OUTPUT, @UserName OUTPUT, @Role OUTPUT;

    PRINT 'Login successful';
    PRINT @UserId;
    PRINT @UserName;
    PRINT @Role;
END TRY

BEGIN CATCH
    PRINT ERROR_MESSAGE()
END CATCH

--gstusr could execute a store procedure but this will not work
--gstusr is not allowed to do anything in dbo schema
SELECT * FROM dbo.Users;

REVERT;





/*
DROP PROCEDURE IF EXISTS gstusr.spLogin
*/