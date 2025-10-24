# Test runner script with coverage
# Run from the tests directory: tests/Application.Tests/

Write-Host "🧪 Running Application Tests with Coverage..." -ForegroundColor Green

# Restore packages
Write-Host "📦 Restoring packages..." -ForegroundColor Yellow
dotnet restore

# Run tests with coverage
Write-Host "🔍 Running tests with coverage collection..." -ForegroundColor Yellow
dotnet test --collect:"XPlat Code Coverage" --results-directory:"./TestResults" --logger:"console;verbosity=detailed"

# Generate coverage report
Write-Host "📊 Generating coverage report..." -ForegroundColor Yellow
$latestCoverageFile = Get-ChildItem -Path "./TestResults" -Recurse -Filter "coverage.cobertura.xml" | Sort-Object LastWriteTime -Descending | Select-Object -First 1

if ($latestCoverageFile) {
    Write-Host "📈 Coverage file found: $($latestCoverageFile.FullName)" -ForegroundColor Cyan
    
    # Install reportgenerator tool if not exists
    $toolList = dotnet tool list --global
    if ($toolList -notmatch "dotnet-reportgenerator-globaltool") {
        Write-Host "🔧 Installing ReportGenerator tool..." -ForegroundColor Yellow
        dotnet tool install -g dotnet-reportgenerator-globaltool
    }
    
    # Generate HTML report
    $reportPath = "./CoverageReport"
    Write-Host "📋 Generating HTML coverage report to: $reportPath" -ForegroundColor Yellow
    reportgenerator -reports:$($latestCoverageFile.FullName) -targetdir:$reportPath -reporttypes:Html
    
    Write-Host "✅ Coverage report generated successfully!" -ForegroundColor Green
    Write-Host "🌐 Open the report: $reportPath/index.html" -ForegroundColor Cyan
} else {
    Write-Host "❌ Coverage file not found in TestResults directory" -ForegroundColor Red
}

Write-Host "🎉 Test execution completed!" -ForegroundColor Green