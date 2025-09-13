param(
    [string]$RootDir = (Split-Path $PSScriptRoot -Parent),
    [string]$AuthProject = "backend\Papernote\Papernote.Auth.API",
    [string]$NotesProject = "backend\Papernote\Papernote.Notes.API",
    [string]$FrontendApiDir = "frontend\src\app\api",
    [string]$ApiSpecsDir = "api-specs"
)

Set-Location $RootDir

if (!(Test-Path $ApiSpecsDir)) { 
    New-Item -ItemType Directory -Path $ApiSpecsDir | Out-Null 
}

Write-Host "Building Auth API project..."
dotnet build "$AuthProject\Papernote.Auth.API.csproj" --configuration Release

Write-Host "Building Notes API project..."
dotnet build "$NotesProject\Papernote.Notes.API.csproj" --configuration Release

Write-Host "Starting Auth API to generate OpenAPI spec..."
$authProcess = Start-Process -FilePath "dotnet" -ArgumentList "run --project $AuthProject --no-build --configuration Release" -PassThru -WindowStyle Hidden
Start-Sleep -Seconds 10

Write-Host "Downloading Auth OpenAPI spec..."
Invoke-WebRequest -Uri "http://localhost:5001/swagger/v1/swagger.json" -OutFile "$ApiSpecsDir\auth-api.json"

Write-Host "Stopping Auth API..."
Stop-Process -Id $authProcess.Id -Force

Write-Host "Starting Notes API to generate OpenAPI spec..."
$notesProcess = Start-Process -FilePath "dotnet" -ArgumentList "run --project $NotesProject --no-build --configuration Release" -PassThru -WindowStyle Hidden
Start-Sleep -Seconds 10

Write-Host "Downloading Notes OpenAPI spec..."
Invoke-WebRequest -Uri "http://localhost:5002/swagger/v1/swagger.json" -OutFile "$ApiSpecsDir\notes-api.json"

Write-Host "Stopping Notes API..."
Stop-Process -Id $notesProcess.Id -Force

Write-Host "Generating Auth TypeScript client..."
if (Test-Path "$FrontendApiDir\auth") { Remove-Item -Recurse -Force "$FrontendApiDir\auth" }
npx @openapitools/openapi-generator-cli generate -i "$ApiSpecsDir\auth-api.json" -g typescript-angular -o "$FrontendApiDir\auth" --additional-properties="providedInRoot=true,ngVersion=18.0.0,modelPropertyNaming=camelCase,apiNameSuffix=Api"

Write-Host "Generating Notes TypeScript client..."
if (Test-Path "$FrontendApiDir\notes") { Remove-Item -Recurse -Force "$FrontendApiDir\notes" }
npx @openapitools/openapi-generator-cli generate -i "$ApiSpecsDir\notes-api.json" -g typescript-angular -o "$FrontendApiDir\notes" --additional-properties="providedInRoot=true,ngVersion=18.0.0,modelPropertyNaming=camelCase,apiNameSuffix=Api"

Write-Host "TypeScript Angular clients generated successfully."