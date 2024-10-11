To create the L15_GoodFriends

NOTE: If you have already created and migrated to the Azure database, skip to step 7
NOTE: If you have already created an Azure Key Vault and moved your user-secrets to it, skip to step 9

1. Create a SQL database in Azure as described in the pdf Lesson13
2. Make sure the connection strings
        "SQLServer-goodfriendsefc-docker-sysadmin"
        "SQLServer-goodfriendsefc-docker-gstusr": 
        "SQLServer-goodfriendsefc-docker-usr": 
        "SQLServer-goodfriendsefc-docker-supusr":
   are all set to the same database connection string given by Azure     

3. Set "DbSetActiveIdx": 1 in appsettings.json DbContext project AND AppGoodFriendsWebApi 

4. From Azure Data Studio connect to the database
   Use connection string from user secrets:
   connection string corresponding to key
   "SQLServer-goodfriendsefc-docker-sysadmin"

5. With Terminal in folder .scripts 
   macOs run: ./database-rebuild-all.sh
   Windows run: .\database-rebuild-all.ps1
   Ensure no errors from build, migration or database update

6. Use Azure Data Studio to execute SQL script DbContext/SqlScripts/initDatabase.sql

-----------------

7. Create a Azure Key Vault as described in the pdf Lesson14

8. Open a terminal in folder .scripts and run ./azkv-update.sh
   Your key vault is now updated to the latest .NET user-secrets

-----------------
9. Prepare to publish buy open a terminal in folder .scripts and run ./prep-publish.sh

9. Run AppGoodFriendsWebApi without debugger
   open url: http://localhost:5000/swagger

10. Use endpoint Guest/LoginUser to login with below credentials
{
  "userNameOrEmail": "superuser1",
  "password": "superuser1"
}

11. The response from endpoint Guest/LoginUser includes an encryptedToken.
   Copy and paste the token (the string without "" - corresponding to <The token> below)
   "encryptedToken": "<The token>"
   into Swagger Authorize.  

10. Use endpoint Admin/Info to verify appEnvironment, secretSource, dbConnection. The response should read
{
  "appEnvironment": "Production",
  "dbConnection": "SQLServer-goodfriendsefc-azure-sysadmin",
  "secretSource": "Azure Keyvault secret: goodfriends"
}

11. Create an App Service in Azure as described in the pdf Lesson15

12. In Visual Studio Code Azure Extension, open App Services
    right click on AppGoodFriendsWebApiNet8 and select Deploy to Web App…. 

13. Browse to the website https://appgoodfriendswebapinet8.azurewebsites.net/swagger/index.html
    Will give an HTTP Error 500.30 first time

    Set up Environment Variables on the Azure App Service AppGoodFriendsWebApiNet8 as descriped in pdf Lesson15

14. Now browse to the website https://appgoodfriendswebapinet8.azurewebsites.net/swagger/index.html
    Your application is deployed.

Enjoy!