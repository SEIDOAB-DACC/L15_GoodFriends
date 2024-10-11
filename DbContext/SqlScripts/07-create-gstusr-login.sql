use goodfriendsefc;
GO

CREATE OR ALTER PROC gstusr.spLogin
    @UserNameOrEmail NVARCHAR(100),
    @Password NVARCHAR(200),

    @UserId UNIQUEIDENTIFIER OUTPUT,
    @UserName NVARCHAR(100) OUTPUT,
    @Role NVARCHAR(100) OUTPUT
    
    AS

    SET NOCOUNT ON;
    
    SET @UserId = NULL;
    SET @UserName = NULL;
    SET @Role = NULL;
    
    SELECT Top 1 @UserId = UserId, @UserName = UserName, @Role = [Role] FROM dbo.Users 
    WHERE ((UserName = @UserNameOrEmail) OR
           (Email IS NOT NULL AND (Email = @UserNameOrEmail))) AND ([Password] = @Password);
    
    IF (@UserId IS NULL)
    BEGIN
        ;THROW 999999, 'Login error: wrong user or password', 1
    END

GO



/*
DROP PROCEDURE IF EXISTS gstusr.spLogin
*/