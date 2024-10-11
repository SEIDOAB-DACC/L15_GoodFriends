USE goodfriendsefc;
GO

--Programatically determine if a user is a mamber of a role
EXECUTE AS USER = 'gstusrUser'; 
--EXECUTE AS USER = 'usrUser'; 
--EXECUTE AS USER = 'supusrUser'; 

IF (SELECT IS_MEMBER('goodfriendsefcGstUsr')) = 1
       SELECT 'Member of the goodfriendsefcGstUsr';
IF (SELECT IS_MEMBER('goodfriendsefcUsr')) = 1
       SELECT 'Member of the goodfriendsefcUsr';
IF (SELECT IS_MEMBER('goodfriendsefcSupUsr')) = 1
       SELECT 'Member of the goodfriendsefcSupUsr';

--note, this query only returns rows for tables where the user has SOME rights
SELECT  TABLE_SCHEMA + '.' + TABLE_NAME AS tableName,
        HAS_PERMS_BY_NAME(TABLE_SCHEMA + '.' + TABLE_NAME, 'OBJECT', 'SELECT') AS AllowSelect,
        HAS_PERMS_BY_NAME(TABLE_SCHEMA + '.' + TABLE_NAME, 'OBJECT', 'INSERT') AS AllowInsert,
        HAS_PERMS_BY_NAME(TABLE_SCHEMA + '.' + TABLE_NAME, 'OBJECT', 'DELETE') AS AllowDelete,
        HAS_PERMS_BY_NAME(TABLE_SCHEMA + '.' + TABLE_NAME, 'OBJECT', 'UPDATE') AS AllowUpdate
FROM    INFORMATION_SCHEMA.TABLES;

REVERT;