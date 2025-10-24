#!/bin/bash

# Test runner script with coverage (Linux/Mac)
# Run from the tests directory: tests/Application.Tests/

echo "🧪 Running Application Tests with Coverage..."

# Restore packages
echo "📦 Restoring packages..."
dotnet restore

# Run tests with coverage
echo "🔍 Running tests with coverage collection..."
dotnet test --collect:"XPlat Code Coverage" --results-directory:"./TestResults" --logger:"console;verbosity=detailed"

# Generate coverage report
echo "📊 Generating coverage report..."
LATEST_COVERAGE_FILE=$(find ./TestResults -name "coverage.cobertura.xml" -type f -exec ls -t {} + | head -n1)

if [ -n "$LATEST_COVERAGE_FILE" ]; then
    echo "📈 Coverage file found: $LATEST_COVERAGE_FILE"
    
    # Install reportgenerator tool if not exists
    if ! dotnet tool list --global | grep -q "dotnet-reportgenerator-globaltool"; then
        echo "🔧 Installing ReportGenerator tool..."
        dotnet tool install -g dotnet-reportgenerator-globaltool
    fi
    
    # Generate HTML report
    REPORT_PATH="./CoverageReport"
    echo "📋 Generating HTML coverage report to: $REPORT_PATH"
    reportgenerator -reports:"$LATEST_COVERAGE_FILE" -targetdir:"$REPORT_PATH" -reporttypes:Html
    
    echo "✅ Coverage report generated successfully!"
    echo "🌐 Open the report: $REPORT_PATH/index.html"
else
    echo "❌ Coverage file not found in TestResults directory"
fi

echo "🎉 Test execution completed!"