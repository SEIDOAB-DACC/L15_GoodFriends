#!/bin/bash
#To make the .sh file executable
#sudo chmod +x ./publish.sh

#Step1: Set the Azure Keyvault access parameters as operating system environment variables.
echo -e "\nSet the azure key vault access as environent variables"
export AZURE_KeyVaultUri=$(./azkv-vap.sh kvuri)
export AZURE_KeyVaultSecret=$(./azkv-vap.sh kvsecret)
export AZURE_TENANT_ID=$(./azkv-vap.sh tenantid)
export AZURE_CLIENT_ID=$(./azkv-vap.sh clientid)
export AZURE_CLIENT_SECRET=$(./azkv-vap.sh clientsecret)

#verify environment variables
echo "AZURE_KeyVaultUri=" $AZURE_KeyVaultUri
# echo "AZURE_TENANT_ID=" $AZURE_TENANT_ID
# echo "AZURE_CLIENT_ID=" $AZURE_CLIENT_ID
# echo "AZURE_CLIENT_SECRET=" $AZURE_CLIENT_SECRET
# echo "AZURE_APP_USERSECRET=" $AZURE_APP_USERSECRET

#Step2: Generate the release files
echo -e "\nPublish the webapi"

#remove any previous publish
rm -rf ../AppGoodFriendsWebApi/publish

cd ../AppGoodFriendsWebApi
dotnet publish --configuration Release --output ./publish


#Step3: Run the application from the folder containing the release files.
echo -e "\nRun the webapi from the published directory"
cd ./publish
./AppGoodFriendsWebApi
