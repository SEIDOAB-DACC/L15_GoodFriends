USE goodfriendsefc;
GO

--create roles
CREATE ROLE goodfriendsefcGstUsr;
CREATE ROLE goodfriendsefcUsr;
CREATE ROLE goodfriendsefcSupUsr;

--assign securables creadentials to the roles
GRANT SELECT, EXECUTE ON SCHEMA::gstusr to goodfriendsefcGstUsr;
GRANT SELECT, UPDATE, INSERT ON SCHEMA::supusr to goodfriendsefcUsr;
GRANT SELECT, UPDATE, INSERT, DELETE, EXECUTE ON SCHEMA::supusr to goodfriendsefcSupUsr;

--finally, add the users to the roles
ALTER ROLE goodfriendsefcGstUsr ADD MEMBER gstusrUser;

ALTER ROLE goodfriendsefcGstUsr ADD MEMBER usrUser;
ALTER ROLE goodfriendsefcUsr ADD MEMBER usrUser;

ALTER ROLE goodfriendsefcGstUsr ADD MEMBER supusrUser;
ALTER ROLE goodfriendsefcUsr ADD MEMBER supusrUser;
ALTER ROLE goodfriendsefcSupUsr ADD MEMBER supusrUser;

/*
--To verify: List database roles and their members
--Show all roles and their members
SELECT DP1.name AS DatabaseRoleName,  ISNULL (DP2.name, 'No members') AS DatabaseUserName   
FROM sys.database_role_members AS DRM  
    RIGHT OUTER JOIN sys.database_principals AS DP1 ON DRM.role_principal_id = DP1.principal_id  
    LEFT OUTER JOIN sys.database_principals AS DP2 ON DRM.member_principal_id = DP2.principal_id  
WHERE DP1.type = 'R'
ORDER BY DP1.name;  
*/

/*
--Cleanup
ALTER ROLE goodfriendsefcGstUsr DROP MEMBER gstusrUser;
ALTER ROLE goodfriendsefcGstUsr DROP MEMBER usrUser;
ALTER ROLE goodfriendsefcGstUsr DROP MEMBER supusrUser;

ALTER ROLE goodfriendsefcUsr DROP MEMBER usrUser;
ALTER ROLE goodfriendsefcUsr DROP MEMBER supusrUser;

ALTER ROLE goodfriendsefcSupUsr DROP MEMBER supusrUser;

DROP ROLE goodfriendsefcGstUsr;
DROP ROLE goodfriendsefcUsr;
DROP ROLE goodfriendsefcSupUsr;
*/