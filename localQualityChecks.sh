toolName="jetBrains.resharper.globaltools"
toolFound=$(dotnet tool list -g | grep $toolName | wc -l)

echo "Checking for installation of $toolName..."
if test $toolFound -eq 0
then
  echo "Installing $toolName..."
  $(dotnet tool install -v q -g $toolName --version 2021.3.4)
else
  echo "$toolName already installed."
fi

pattern="*.sln"
files=( $pattern )
solutionPath="${files[0]}"
echo "Using solution file: $solutionPath"

outputPath="localQualityCheckResult.xml"
echo "Using outputPath of $outputPath"

set -e
jb inspectcode -o=$outputPath -a $solutionPath --build --severity=warning

reportExists=$(ls -al | grep $outputPath | wc -l)
if test $reportExists -eq 0
then
  echo "No issues file found named $outputPath."
else
  echo "Writing out report file $outputPath."
  cat $outputPath
fi

