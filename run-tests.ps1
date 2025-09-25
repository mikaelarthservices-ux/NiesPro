#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Execute all tests for Catalog service (Unit + Integration)
.DESCRIPTION 
    This script runs both unit and integration tests for the Catalog service
    and provides comprehensive reporting of the results.
#>

param(
    [switch]$UnitOnly,
    [switch]$IntegrationOnly,
    [switch]$Verbose
)

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "       CATALOG SERVICE TEST RUNNER         " -ForegroundColor Cyan  
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Continue"
$totalTests = 0
$passedTests = 0
$failedTests = 0
$skippedTests = 0

# Test project paths
$unitTestPath = "tests\Catalog\Unit\Catalog.Tests.Unit.csproj"
$integrationTestPath = "tests\Catalog\Integration\Catalog.Tests.Integration.csproj"

function Run-Tests {
    param(
        [string]$ProjectPath,
        [string]$TestType
    )
    
    Write-Host "Running $TestType Tests..." -ForegroundColor Yellow
    Write-Host "Project: $ProjectPath" -ForegroundColor Gray
    Write-Host ""
    
    if (!(Test-Path $ProjectPath)) {
        Write-Host "⚠️  Project not found: $ProjectPath" -ForegroundColor Red
        return @{ Total = 0; Passed = 0; Failed = 0; Skipped = 0 }
    }
    
    try {
        # Build the test project first
        Write-Host "Building test project..." -ForegroundColor Gray
        $buildResult = dotnet build $ProjectPath --no-restore --verbosity minimal 2>&1
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Build failed for $TestType tests" -ForegroundColor Red
            if ($Verbose) { Write-Host $buildResult -ForegroundColor Red }
            return @{ Total = 0; Passed = 0; Failed = 1; Skipped = 0 }
        }
        
        # Run tests with detailed output
        $testArgs = @(
            "test", $ProjectPath,
            "--no-build",
            "--verbosity", "normal",
            "--logger", "console;verbosity=detailed"
        )
        
        if ($Verbose) {
            $testArgs += "--logger"
            $testArgs += "trx"
        }
        
        Write-Host "Executing tests..." -ForegroundColor Gray
        $testOutput = dotnet @testArgs 2>&1
        
        # Parse test results
        $results = @{ Total = 0; Passed = 0; Failed = 0; Skipped = 0 }
        
        $testOutput | ForEach-Object {
            $line = $_.ToString()
            
            if ($line -match "Total tests: (\d+)") {
                $results.Total = [int]$matches[1]
            }
            elseif ($line -match "Passed: (\d+)") {
                $results.Passed = [int]$matches[1]
            }
            elseif ($line -match "Failed: (\d+)") {
                $results.Failed = [int]$matches[1]
            }
            elseif ($line -match "Skipped: (\d+)") {
                $results.Skipped = [int]$matches[1]
            }
            
            # Show test progress
            if ($line -match "^\s*(Passed|Failed|Skipped)") {
                if ($line -match "Passed") {
                    Write-Host "  ✅ $line" -ForegroundColor Green
                } elseif ($line -match "Failed") {
                    Write-Host "  ❌ $line" -ForegroundColor Red
                } else {
                    Write-Host "  ⏭️  $line" -ForegroundColor Yellow
                }
            }
        }
        
        # If we couldn't parse results from output, check exit code
        if ($results.Total -eq 0) {
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✅ $TestType tests completed successfully" -ForegroundColor Green
                $results = @{ Total = 1; Passed = 1; Failed = 0; Skipped = 0 }
            } else {
                Write-Host "❌ $TestType tests failed" -ForegroundColor Red
                $results = @{ Total = 1; Passed = 0; Failed = 1; Skipped = 0 }
                if ($Verbose) { Write-Host $testOutput -ForegroundColor Red }
            }
        }
        
        return $results
        
    } catch {
        Write-Host "❌ Error running $TestType tests: $($_.Exception.Message)" -ForegroundColor Red
        return @{ Total = 1; Passed = 0; Failed = 1; Skipped = 0 }
    }
}

function Show-Summary {
    param($Results)
    
    Write-Host ""
    Write-Host "============================================" -ForegroundColor Cyan
    Write-Host "              TEST SUMMARY                  " -ForegroundColor Cyan
    Write-Host "============================================" -ForegroundColor Cyan
    
    $Results | ForEach-Object {
        $testType = $_.TestType
        $total = $_.Total
        $passed = $_.Passed
        $failed = $_.Failed
        $skipped = $_.Skipped
        
        Write-Host ""
        Write-Host "$testType Tests:" -ForegroundColor White
        Write-Host "  Total:   $total" -ForegroundColor Gray
        Write-Host "  Passed:  $passed" -ForegroundColor Green
        Write-Host "  Failed:  $failed" -ForegroundColor $(if($failed -gt 0) { 'Red' } else { 'Gray' })
        Write-Host "  Skipped: $skipped" -ForegroundColor Yellow
        
        if ($failed -gt 0) {
            Write-Host "  Status:  ❌ FAILED" -ForegroundColor Red
        } elseif ($passed -gt 0) {
            Write-Host "  Status:  ✅ PASSED" -ForegroundColor Green
        } else {
            Write-Host "  Status:  ⏭️  NO TESTS" -ForegroundColor Yellow
        }
    }
    
    # Overall summary
    $overallTotal = ($Results | Measure-Object -Property Total -Sum).Sum
    $overallPassed = ($Results | Measure-Object -Property Passed -Sum).Sum
    $overallFailed = ($Results | Measure-Object -Property Failed -Sum).Sum
    $overallSkipped = ($Results | Measure-Object -Property Skipped -Sum).Sum
    
    Write-Host ""
    Write-Host "============================================" -ForegroundColor Cyan
    Write-Host "             OVERALL RESULTS                " -ForegroundColor Cyan
    Write-Host "============================================" -ForegroundColor Cyan
    Write-Host "Total Tests:   $overallTotal" -ForegroundColor White
    Write-Host "Passed:        $overallPassed" -ForegroundColor Green
    Write-Host "Failed:        $overallFailed" -ForegroundColor $(if($overallFailed -gt 0) { 'Red' } else { 'Green' })
    Write-Host "Skipped:       $overallSkipped" -ForegroundColor Yellow
    
    $successRate = if ($overallTotal -gt 0) { [math]::Round(($overallPassed / $overallTotal) * 100, 2) } else { 0 }
    Write-Host "Success Rate:  $successRate%" -ForegroundColor $(if($successRate -eq 100) { 'Green' } else { 'Yellow' })
    
    if ($overallFailed -eq 0 -and $overallTotal -gt 0) {
        Write-Host ""
        Write-Host "🎉 ALL TESTS PASSED! 🎉" -ForegroundColor Green
        Write-Host "The test suite is ready for production." -ForegroundColor Green
    } elseif ($overallFailed -gt 0) {
        Write-Host ""
        Write-Host "⚠️  SOME TESTS FAILED" -ForegroundColor Red
        Write-Host "Please review and fix failing tests before deployment." -ForegroundColor Red
    }
    
    Write-Host ""
}

# Restore packages first
Write-Host "Restoring NuGet packages..." -ForegroundColor Gray
dotnet restore --verbosity minimal

$results = @()

# Run Unit Tests
if (!$IntegrationOnly) {
    $unitResults = Run-Tests $unitTestPath "Unit"
    $results += @{
        TestType = "Unit"
        Total = $unitResults.Total
        Passed = $unitResults.Passed
        Failed = $unitResults.Failed
        Skipped = $unitResults.Skipped
    }
}

# Run Integration Tests
if (!$UnitOnly) {
    Write-Host ""
    $integrationResults = Run-Tests $integrationTestPath "Integration"
    $results += @{
        TestType = "Integration"
        Total = $integrationResults.Total
        Passed = $integrationResults.Passed
        Failed = $integrationResults.Failed
        Skipped = $integrationResults.Skipped
    }
}

# Show final summary
Show-Summary $results

# Exit with error code if any tests failed
$totalFailed = ($results | Measure-Object -Property Failed -Sum).Sum
if ($totalFailed -gt 0) {
    exit 1
} else {
    exit 0
}