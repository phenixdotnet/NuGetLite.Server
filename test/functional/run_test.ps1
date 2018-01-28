[CmdletBinding()]
Param(
	[switch]$LaunchServer = $True,
	[switch]$CleanServerDirectories = $True
)
$global:nugetServerLiteProcess = $null

Function Start-NuGetLiteServer {

	$nugetLiteServerBasePath = "..\..\src\NuGetLite.Server"

	If ($CleanServerDirectories) {

		rmdir "$nugetLiteServerBasePath\packages" -Recurse
		rmdir "$nugetLiteServerBasePath\metadata" -Recurse
	}

	Start-Process -WorkingDirectory $nugetLiteServerBasePath -FilePath "dotnet" -ArgumentList "restore" -Wait
	Start-Process -WorkingDirectory $nugetLiteServerBasePath -FilePath "dotnet" -ArgumentList "build -c Release" -Wait
	$global:nugetServerLiteProcess = Start-Process -WorkingDirectory $nugetLiteServerBasePath -FilePath "dotnet" -ArgumentList "run -c Release" -PassThru
}

Function PushPackage {
	Param (
        [Parameter(Mandatory=$True)]
        [string]$packageName,

        [Parameter(Mandatory=$True)]
        [string]$packageVersion
    )


	Write-Host "Pushing package $packageVersion to source"
	.\nuget.exe push .\TestPackage\nupkg\$packageName.$packageVersion.nupkg -Source debug -ConfigFile .\NuGet.Config -Verbosity detailed

	Write-Host "Looking for PackageBaseAddress resource for $packageName package"
	Invoke-WebRequest -UseBasicParsing "http://localhost:55983/v3-flatcontainer/$packageName/index.json" 

	Write-Host "Downloading nuspec for $packageName $packageVersion"
	Invoke-WebRequest -UseBasicParsing "http://localhost:55983/v3-flatcontainer/$packageName/$packageVersion/$packageName.nuspec" -OutFile "$packageName.$packageVersion.nuspec"

	Write-Host "Downloading package $packageName $packageVersion"
	Invoke-WebRequest -UseBasicParsing "http://localhost:55983/v3-flatcontainer/$packageName/$packageVersion/$packageName.$packageVersion.nupkg" -OutFile "$packageName.$packageVersion.nupkg"
}

If ($LaunchServer) {
	Start-NuGetLiteServer
}

Write-Host "Creating test package v1 and v2"

rmdir TestPackage -Recurse
mkdir TestPackage
Push-Location TestPackage

dotnet new classlib

Copy-Item ..\v1_TestPackage.csproj.txt .\TestPackage.csproj
dotnet pack -o .\nupkg -c Release

Copy-Item ..\v2_TestPackage.csproj.txt .\TestPackage.csproj
dotnet pack -o .\nupkg -c Release

Pop-Location

PushPackage -packageName "TestPackage" -packageVersion "1.0.0"
PushPackage -packageName "TestPackage" -packageVersion "2.0.0"

rmdir TestApp -Recurse
mkdir TestApp

Push-Location "TestApp"
dotnet new console --force

..\nuget.exe install TestPackage -Source debug -ConfigFile ..\NuGet.Config -Verbosity detailed

Pop-Location

Write-Host "Searching for package q=''"
Invoke-WebRequest "http://localhost:55983/query?q=&prerelease=false" -UseBasicParsing


$global:nugetServerLiteProcess.Kill()