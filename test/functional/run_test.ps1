[CmdletBinding()]
Param(
	[boolean]$LaunchServer = $True,
	[boolean]$CleanServerDirectories = $True
)

Function Start-NuGetLiteServer {

	$nugetLiteServerBasePath = Join-Path $PSScriptRoot "..\..\src\NuGetLite.Server"
	$binDirPath = Join-Path $nugetLiteServerBasePath "bin" "Release" "netcoreapp2.0"
	$binPath = Join-Path $binDirPath "NuGetLite.Server.dll"

	If ($CleanServerDirectories) {

		$packagePath = Join-Path $nugetLiteServerBasePath "packages"
		$metadataPath = Join-Path $nugetLiteServerBasePath "metadata"
		Remove-Item $packagePath -Recurse
		Remove-Item $metadataPath -Recurse

		Remove-Item $(Join-Path $binDirPath "packages") -Recurse
		Remove-Item $(Join-Path $binDirPath "metadata") -Recurse
	}

	Start-Process -WorkingDirectory $nugetLiteServerBasePath -FilePath "dotnet" -ArgumentList "restore" -Wait
	Start-Process -WorkingDirectory $nugetLiteServerBasePath -FilePath "dotnet" -ArgumentList "build -c Release" -Wait

	$ENV:ASPNETCORE_ENVIRONMENT="Development"
	$ENV:ASPNETCORE_URLS="http://+:55983"
	$ENV:PublicBaseUrl="http://localhost:55983"
	$ENV:PackageIndexType="File"

	$process = Start-Process -WorkingDirectory $binDirPath -FilePath "dotnet" -ArgumentList "exec $binPath" -PassThru
	$script:nugetServerLiteProcess = $process

	Start-Sleep 5
}

Function PushPackage {
	Param (
        [Parameter(Mandatory=$True)]
        [string]$packageName,

        [Parameter(Mandatory=$True)]
        [string]$packageVersion
    )


	Write-Host "Pushing package $packageVersion to source"
	If ($PSVersionTable.Platform -eq "Unix") {
		Start-Process -WorkingDirectory $PSScriptRoot -FilePath "mono" -ArgumentList "./nuget.exe push ./TestPackage/nupkg/$packageName.$packageVersion.nupkg -Source debug -ConfigFile ./NuGet.Config -Verbosity detailed" -Wait
	}
	else {
		Start-Process -WorkingDirectory $PSScriptRoot -FilePath ".\nuget.exe" -ArgumentList "push .\TestPackage\nupkg\$packageName.$packageVersion.nupkg -Source debug -ConfigFile .\NuGet.Config -Verbosity detailed" -Wait
	}

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

$testPackagePath = Join-Path $PSScriptRoot "TestPackage"
Remove-Item $testPackagePath -Recurse
New-Item -ItemType Directory -Force -Path $testPackagePath
Push-Location $testPackagePath

dotnet new classlib --force

Copy-Item ..\v1_TestPackage.csproj.txt .\TestPackage.csproj
dotnet pack -o .\nupkg -c Release

Copy-Item ..\v2_TestPackage.csproj.txt .\TestPackage.csproj
dotnet pack -o .\nupkg -c Release

Pop-Location

PushPackage -packageName "TestPackage" -packageVersion "1.0.0"
PushPackage -packageName "TestPackage" -packageVersion "2.0.0"

$testAppPath = Join-Path $PSScriptRoot "TestApp"
Remove-Item $testAppPath -Recurse
New-Item -ItemType Directory -Force -Path $testAppPath

Push-Location $testAppPath
dotnet new console --force

If ($PSVersionTable.Platform -eq "Unix") {
	Start-Process -WorkingDirectory $testAppPath -FilePath "mono" -ArgumentList "../nuget.exe install TestPackage -Source debug -ConfigFile ../NuGet.Config -Verbosity detailed" -Wait
}
else {
	Start-Process -WorkingDirectory $testAppPath -FilePath "..\nuget.exe" -ArgumentList "install TestPackage -Source debug -ConfigFile ..\NuGet.Config -Verbosity detailed" -Wait
}

Pop-Location

Write-Host "Searching for package q=''"
Invoke-WebRequest "http://localhost:55983/query?q=&prerelease=false" -UseBasicParsing

If ($script:nugetServerLiteProcess -ne $null) {
	
	Write-Host "Killing server (process id: $($script:nugetServerLiteProcess.Id))"
	$script:nugetServerLiteProcess.Kill()
	$script:nugetServerLiteProcess.Close()
}