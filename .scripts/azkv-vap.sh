#!/bin/bash
#To make the .sh file executable
#sudo chmod +x ./azkv-vap.sh

#Gets the Azure Key Vault Access parameters

#USER INPUT
#Location of Key Vault Access parameters
UserSecretPath="/Users/Martin/.microsoft/usersecrets/"
VaultAccessFileName="vault-access.json"
UserSecretsProjFile="../Configuration/Configuration.csproj"
#END USER INPUT


#use sed to extract User Secret GUID from cs.proj
UserSecretsId=$(sed -n 's:.*<UserSecretsId>\(.*\)</UserSecretsId>.*:\1:p' "$UserSecretsProjFile")

#grep the Vault Access Parameters from the json structure and set the environment variables
VaultAccessFile="$UserSecretPath$UserSecretsId/$VaultAccessFileName"

if [[ $1 == "kvuri" ]]; then
    cat $VaultAccessFile| grep -o '"AZURE_KeyVaultUri": "[^"]*' | grep -o '[^"]*$'

elif [[ $1 == "kvname" ]]; then
    cat $VaultAccessFile| grep -o '"AZURE_KeyVaultName": "[^"]*' | grep -o '[^"]*$'

elif [[ $1 == "kvsecret" ]]; then
    cat $VaultAccessFile| grep -o '"AZURE_KeyVaultSecret": "[^"]*' | grep -o '[^"]*$'

elif [[ $1 == "tenantid" ]]; then
    cat $VaultAccessFile| grep -o '"AZURE_TENANT_ID": "[^"]*' | grep -o '[^"]*$'

elif [[ $1 == "clientid" ]]; then
    cat $VaultAccessFile| grep -o '"AZURE_CLIENT_ID": "[^"]*' | grep -o '[^"]*$'

elif [[ $1 == "clientsecret" ]]; then
    cat $VaultAccessFile| grep -o '"AZURE_CLIENT_SECRET": "[^"]*' | grep -o '[^"]*$'

elif [[ $1 == "path" ]]; then
    echo $UserSecretPath$UserSecretsId
fi

