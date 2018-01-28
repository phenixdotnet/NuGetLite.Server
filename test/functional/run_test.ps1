
Write-Host "Pushing package to source"
.\nuget.exe push .\newtonsoft.json.10.0.3.nupkg -Source debug -ConfigFile .\NuGet.Config -Verbosity detailed

Write-Host "Searching for package q=''"
Invoke-WebRequest "http://localhost:55983/query?q=&prerelease=false" -UseBasicParsing

Write-Host "Looking for PackageBaseAddress resource for Newtonsoft.json package"
Invoke-WebRequest -UseBasicParsing "http://localhost:55983/v3-flatcontainer/newtonsoft.json/index.json" 

Write-Host "Downloading nuspec for Newtonsoft.json 10.0.3"
Invoke-WebRequest -UseBasicParsing "http://localhost:55983/v3-flatcontainer/newtonsoft.json/10.0.3/newtonsoft.json.nuspec" -OutFile "Newtonsoft.Json.10.0.3.nuspec"

Write-Host "Downloading package Newtonsoft.json 10.0.3"
Invoke-WebRequest -UseBasicParsing "http://localhost:55983/v3-flatcontainer/newtonsoft.json/10.0.3/newtonsoft.json.10.0.3.nupkg" -OutFile "Newtonsoft.Json.10.0.3.nupkg"


mkdir TestApp
Push-Location "TestApp"
dotnet new console --force

..\nuget.exe install Newtonsoft.Json -Source debug -ConfigFile ..\NuGet.Config -Verbosity detailed

Pop-Location