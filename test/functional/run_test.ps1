
Write-Host "Pushing package to source"
.\nuget.exe push .\newtonsoft.json.10.0.3.nupkg -Source debug -ConfigFile .\NuGet.Config -Verbosity detailed

Write-Host "Searching for package q=''"
Invoke-WebRequest "http://localhost:55983/query?q=&prerelease=false" -UseBasicParsing