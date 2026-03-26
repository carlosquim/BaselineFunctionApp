#!/bin/bash
set -e

RG="rg_aksmig"
COSMOS_NAME="cqmaksevent"
SA_NAME="testcqmaksmig"
FUNC_NAME="funccqmaksmig"

echo "Enabling System-Assigned Managed Identity for Function App..."
PRINCIPAL_ID=$(az functionapp identity assign -g $RG -n $FUNC_NAME --query "principalId" -o tsv)

echo "Setting Function App AppSettings (Identity-based)..."
az functionapp config appsettings set --name $FUNC_NAME --resource-group $RG --settings \
  "CosmosDbConnectionString__accountEndpoint=https://${COSMOS_NAME}.documents.azure.com:443/" \
  "AzureWebJobsStorage__blobServiceUri=https://${SA_NAME}.blob.core.windows.net/" \
  "AzureWebJobsStorage__queueServiceUri=https://${SA_NAME}.queue.core.windows.net/" \
  "AzureWebJobsStorage__tableServiceUri=https://${SA_NAME}.table.core.windows.net/" > /dev/null



echo "Assigning RBAC Role for Cosmos DB (Cosmos DB Built-in Data Contributor)..."
COSMOS_ID=$(az cosmosdb show -n $COSMOS_NAME -g $RG --query "id" -o tsv)
az cosmosdb sql role assignment create -a $COSMOS_NAME -g $RG \
  --scope "$COSMOS_ID" \
  --principal-id $PRINCIPAL_ID \
  --role-definition-id 00000000-0000-0000-0000-000000000002 > /dev/null 2>&1 || echo "Cosmos DB Role already assigned or failed."

echo "Creating Storage container..."
az storage container create --name samples-workitems --account-name $SA_NAME --auth-mode login > /dev/null 2>&1 || true

echo "Assigning RBAC Roles for Storage Account..."
SA_ID=$(az storage account show -n $SA_NAME -g $RG --query "id" -o tsv)
az role assignment create --role "Storage Blob Data Owner" --assignee $PRINCIPAL_ID --scope $SA_ID > /dev/null 2>&1 || true
az role assignment create --role "Storage Queue Data Contributor" --assignee $PRINCIPAL_ID --scope $SA_ID > /dev/null 2>&1 || true
az role assignment create --role "Storage Account Contributor" --assignee $PRINCIPAL_ID --scope $SA_ID > /dev/null 2>&1 || true

echo "Building and Zipping project..."
dotnet publish -c Release -o ./publish > /dev/null
cd ./publish
zip -r ../app.zip . > /dev/null

echo "Deploying to Azure Functions..."
cd ..
az functionapp deployment source config-zip -g $RG -n $FUNC_NAME --src app.zip

echo "Deployment finished."
