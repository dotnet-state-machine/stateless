echo "build: Build started"

Push-Location $PSScriptRoot

if(Test-Path .\artifacts) {
	echo "build: Cleaning .\artifacts"
	Remove-Item .\artifacts -Force -Recurse
}

& dotnet restore .\Stateless\project.json --no-cache
& dotnet restore .\Stateless.Tests\project.json --no-cache

$branch = @{ $true = $env:APPVEYOR_REPO_BRANCH; $false = $(git symbolic-ref --short -q HEAD) }[$env:APPVEYOR_REPO_BRANCH -ne $NULL];
$revision = @{ $true = "{0:00000}" -f [convert]::ToInt32("0" + $env:APPVEYOR_BUILD_NUMBER, 10); $false = "local" }[$env:APPVEYOR_BUILD_NUMBER -ne $NULL];
$suffix = @{ $true = ""; $false = "$($branch.Substring(0, [math]::Min(10,$branch.Length)))-$revision"}[$branch -eq "master" -and $revision -ne "local"]


echo "build: Version suffix is $suffix"


& dotnet pack .\Stateless\project.json -c Release -o .\artifacts --version-suffix=$suffix
    if($LASTEXITCODE -ne 0) { exit 1 }    

& dotnet test .\Stateless.Tests\project.json 
    if($LASTEXITCODE -ne 0) { exit 2 }    

Pop-Location