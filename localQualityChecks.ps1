$toolName = "JetBrains.ReSharper.GlobalTools"
$toolFound = dotnet tool list -g | Select-String $toolName

if([string]::IsNullOrEmpty($toolFound)){
  dotnet tool install -v q -g $toolName --version 2021.3.4  | out-null
}
else {
  Write-Output "$toolName already installed."
}

$solutionPathInfo = Get-ChildItem *.sln | Select-Object -first 1 
$solutionPath = $solutionPathInfo.Name
$outputPath = "localQualityCheckResult.xml"
Write-Output "Using solution: $solutionPath"
jb inspectcode -o="$outputPath" -a $solutionPath --build --severity=warning
jb cleanupcode $solutionPath --settings="$solutionPath.DotSettings"

if (Test-Path -Path $outputPath) {
  Write-Output "Witing ouput of report:"
  Get-Content $outputPath  
}
else {
  Write-Output "No issues file found"
}
