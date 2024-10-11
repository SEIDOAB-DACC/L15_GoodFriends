--https://techcommunity.microsoft.com/t5/azure-database-support-blog/create-sql-login-and-sql-user-on-your-azure-sql-db/ba-p/368813
USE goodfriendsefc;
GO

--create 3 users we will late set credentials for these
DROP USER IF EXISTS  gstusrUser;
DROP USER IF EXISTS usrUser;
DROP USER IF EXISTS supusrUser;

CREATE USER gstusrUser WITH PASSWORD = N'pa$$Word1'; 
CREATE USER usrUser WITH PASSWORD = N'pa$$Word1'; 
CREATE USER supusrUser WITH PASSWORD = N'pa$$Word1'; 

ALTER ROLE db_datareader ADD MEMBER gstusrUser; 
ALTER ROLE db_datareader ADD MEMBER usrUser; 
ALTER ROLE db_datareader ADD MEMBER supusrUser; 

--Verify: See logins and users
SELECT * FROM sys.database_principals
WHERE type_desc = 'SQL_USER'

/*
--Cleanup
DROP USER gstusrUser;
DROP USER usrUser;
DROP USER supusrUser;
*/