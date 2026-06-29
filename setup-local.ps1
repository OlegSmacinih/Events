Write-Host "Starting containers..."
docker compose up -d

Write-Host "Waiting for DynamoDB Local..."

$env:AWS_ACCESS_KEY_ID = "fake"
$env:AWS_SECRET_ACCESS_KEY = "fake"
$env:AWS_DEFAULT_REGION = "eu-central-1"

do {
	Start-Sleep -Seconds 1
	aws dynamodb list-tables --endpoint-url http://localhost:8000 *> $null
} while ($LASTEXITCODE -ne 0)

Write-Host "DynamoDB Local is ready."

$tableExists = aws dynamodb describe-table --table-name DeviceEvents --endpoint-url http://localhost:8000 2> $null

if ($LASTEXITCODE -eq 0) {
	Write-Host "DynamoDB table DeviceEvents already exists."
}
else {
	Write-Host "Creating DynamoDB table DeviceEvents..."
	aws dynamodb create-table --cli-input-json file://create-table.json --endpoint-url http://localhost:8000

	Write-Host "Table created."
}

Write-Host "Local infrastructure is ready."