#!/bin/bash
#To make the .sh file executable
#sudo chmod +x ./azkv-update.sh

#NOTE:
#update and run latest azure cli
#docker pull mcr.microsoft.com/azure-cli
#docker run -it mcr.microsoft.com/azure-cli

#USER INPUT
#check the container name created by above script 
AzureCliContainer=kind_kapitsa
#END USER INPUT

#start the container $AzureCliContainer in Docker Desktop
docker ps -a | grep "$AzureCliContainer" | awk '{print $1}' |  xargs docker start

#Unique to application, read from applications vault-access.json
UserSecretPath=$(./azkv-vap.sh path)
AzureKeyVaultSecret=$(./azkv-vap.sh kvsecret)
AzureHomeTenantId=$(./azkv-vap.sh tenantid)
AzureKeyVaultName=$(./azkv-vap.sh kvname)

if [ -z "$AzureKeyVaultSecret" ] || [ -z "$UserSecretPath" ] || [ -z "$AzureHomeTenantId" ] || [ -z "$AzureKeyVaultName" ]; then
    echo "Key vault access parameter error"
else
    echo -e "\nKey vault access parameters"
    echo "UserSecretPath=$UserSecretPath"
    echo AzureKeyVaultSecret=$AzureKeyVaultSecret
    echo AzureHomeTenantId=$AzureHomeTenantId
    echo AzureKeyVaultName=$AzureKeyVaultName
    echo -e "\n\n"
fi

#login to azure, if not already
#test if logged in
LoggedInTenant=$(docker exec -it $AzureCliContainer az account show | grep -o '"homeTenantId": "[^"]*' | grep -o '[^"]*$')
if [[ $LoggedInTenant != $AzureHomeTenantId ]]; then

    echo "Not logged in: logging in to azure"
    docker exec -it $AzureCliContainer az logout
    docker exec -it $AzureCliContainer az login
else 

    echo "Logged in as "$LoggedInTenant
fi

#Create location for user secret in Azure cli container
docker exec -it $AzureCliContainer rm -rf /$AzureKeyVaultSecret 
docker exec -it $AzureCliContainer mkdir /$AzureKeyVaultSecret 

#copy user secrets into Azure cli container at right location
docker cp $UserSecretPath/secrets.json $AzureCliContainer:/$AzureKeyVaultSecret/

#create and copy the secret to azkv
docker exec -it $AzureCliContainer az keyvault secret set --name "$AzureKeyVaultSecret" --vault-name "$AzureKeyVaultName" --file "/$AzureKeyVaultSecret/secrets.json"

#Cleanup
#rm location for user secret in Azure cli container
docker exec -it $AzureCliContainer rm -rf /$AzureKeyVaultSecret 
