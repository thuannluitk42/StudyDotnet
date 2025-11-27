# Stress Test Script for Microservices
param(
    [int]$TotalRequests = 1000,
    [int]$ConcurrentJobs = 50,
    [string]$TargetService = "both" # "bookapi", "orderapi", or "both"
)

Write-Host "🚀 Starting Stress Test..." -ForegroundColor Green
Write-Host "Total Requests: $TotalRequests" -ForegroundColor Cyan
Write-Host "Concurrent Jobs: $ConcurrentJobs" -ForegroundColor Cyan
Write-Host "Target Service: $TargetService" -ForegroundColor Cyan
Write-Host ""

# Test BookApi
function Test-BookApi {
    param([int]$RequestCount, [int]$Concurrent)
    
    Write-Host "📚 Testing BookApi (GET /api/books)..." -ForegroundColor Yellow
    
    $results = @{
        Success = 0
        Failed = 0
        TotalTime = 0
        MinTime = [double]::MaxValue
        MaxTime = 0
    }
    
    $jobs = @()
    $batchSize = [Math]::Ceiling($RequestCount / $Concurrent)
    
    for ($i = 0; $i -lt $Concurrent; $i++) {
        $jobs += Start-Job -ScriptBlock {
            param($count, $batchNum)
            
            $batchResults = @()
            for ($j = 0; $j -lt $count; $j++) {
                $sw = [System.Diagnostics.Stopwatch]::StartNew()
                try {
                    $response = Invoke-RestMethod -Uri "http://localhost:5238/api/books" -Method Get -TimeoutSec 30
                    $sw.Stop()
                    $batchResults += @{
                        Success = $true
                        Time = $sw.ElapsedMilliseconds
                    }
                }
                catch {
                    $sw.Stop()
                    $batchResults += @{
                        Success = $false
                        Time = $sw.ElapsedMilliseconds
                    }
                }
            }
            return $batchResults
        } -ArgumentList $batchSize, $i
    }
    
    Write-Host "⏳ Waiting for $Concurrent concurrent jobs to complete..." -ForegroundColor Gray
    
    $allResults = $jobs | Wait-Job | Receive-Job
    $jobs | Remove-Job
    
    foreach ($result in $allResults) {
        if ($result.Success) {
            $results.Success++
        } else {
            $results.Failed++
        }
        $results.TotalTime += $result.Time
        if ($result.Time -lt $results.MinTime) { $results.MinTime = $result.Time }
        if ($result.Time -gt $results.MaxTime) { $results.MaxTime = $result.Time }
    }
    
    $avgTime = if ($allResults.Count -gt 0) { $results.TotalTime / $allResults.Count } else { 0 }
    
    Write-Host ""
    Write-Host "📊 BookApi Results:" -ForegroundColor Green
    Write-Host "  ✅ Successful: $($results.Success)" -ForegroundColor Green
    Write-Host "  ❌ Failed: $($results.Failed)" -ForegroundColor Red
    Write-Host "  ⚡ Avg Response Time: $([Math]::Round($avgTime, 2))ms" -ForegroundColor Cyan
    Write-Host "  🏃 Min Response Time: $($results.MinTime)ms" -ForegroundColor Cyan
    Write-Host "  🐌 Max Response Time: $($results.MaxTime)ms" -ForegroundColor Cyan
    Write-Host ""
    
    return $results
}

# Test OrderApi
function Test-OrderApi {
    param([int]$RequestCount, [int]$Concurrent)
    
    Write-Host "📦 Testing OrderApi (POST /api/orders)..." -ForegroundColor Yellow
    
    $results = @{
        Success = 0
        Failed = 0
        TotalTime = 0
        MinTime = [double]::MaxValue
        MaxTime = 0
    }
    
    $jobs = @()
    $batchSize = [Math]::Ceiling($RequestCount / $Concurrent)
    
    for ($i = 0; $i -lt $Concurrent; $i++) {
        $jobs += Start-Job -ScriptBlock {
            param($count, $batchNum)
            
            $batchResults = @()
            for ($j = 0; $j -lt $count; $j++) {
                $orderBody = @{
                    bookId = Get-Random -Minimum 1 -Maximum 100
                    bookTitle = "Load Test Book $batchNum-$j"
                    bookAuthor = "Test Author"
                    quantity = Get-Random -Minimum 1 -Maximum 10
                    unitPrice = [Math]::Round((Get-Random -Minimum 10 -Maximum 100), 2)
                    customerEmail = "loadtest$batchNum-$j@test.com"
                } | ConvertTo-Json
                
                $sw = [System.Diagnostics.Stopwatch]::StartNew()
                try {
                    $response = Invoke-RestMethod -Uri "http://localhost:5013/api/orders" -Method Post -Body $orderBody -ContentType "application/json" -TimeoutSec 30
                    $sw.Stop()
                    $batchResults += @{
                        Success = $true
                        Time = $sw.ElapsedMilliseconds
                    }
                }
                catch {
                    $sw.Stop()
                    $batchResults += @{
                        Success = $false
                        Time = $sw.ElapsedMilliseconds
                    }
                }
            }
            return $batchResults
        } -ArgumentList $batchSize, $i
    }
    
    Write-Host "⏳ Waiting for $Concurrent concurrent jobs to complete..." -ForegroundColor Gray
    
    $allResults = $jobs | Wait-Job | Receive-Job
    $jobs | Remove-Job
    
    foreach ($result in $allResults) {
        if ($result.Success) {
            $results.Success++
        } else {
            $results.Failed++
        }
        $results.TotalTime += $result.Time
        if ($result.Time -lt $results.MinTime) { $results.MinTime = $result.Time }
        if ($result.Time -gt $results.MaxTime) { $results.MaxTime = $result.Time }
    }
    
    $avgTime = if ($allResults.Count -gt 0) { $results.TotalTime / $allResults.Count } else { 0 }
    
    Write-Host ""
    Write-Host "📊 OrderApi Results:" -ForegroundColor Green
    Write-Host "  ✅ Successful: $($results.Success)" -ForegroundColor Green
    Write-Host "  ❌ Failed: $($results.Failed)" -ForegroundColor Red
    Write-Host "  ⚡ Avg Response Time: $([Math]::Round($avgTime, 2))ms" -ForegroundColor Cyan
    Write-Host "  🏃 Min Response Time: $($results.MinTime)ms" -ForegroundColor Cyan
    Write-Host "  🐌 Max Response Time: $($results.MaxTime)ms" -ForegroundColor Cyan
    Write-Host ""
    
    return $results
}

# Run tests
$startTime = Get-Date

if ($TargetService -eq "bookapi" -or $TargetService -eq "both") {
    $bookResults = Test-BookApi -RequestCount $TotalRequests -Concurrent $ConcurrentJobs
}

if ($TargetService -eq "orderapi" -or $TargetService -eq "both") {
    $orderResults = Test-OrderApi -RequestCount $TotalRequests -Concurrent $ConcurrentJobs
}

$endTime = Get-Date
$totalDuration = ($endTime - $startTime).TotalSeconds

Write-Host "=" * 60 -ForegroundColor Green
Write-Host "🎯 STRESS TEST COMPLETED" -ForegroundColor Green
Write-Host "=" * 60 -ForegroundColor Green
Write-Host "Total Duration: $([Math]::Round($totalDuration, 2)) seconds" -ForegroundColor Cyan
Write-Host "Requests/Second: $([Math]::Round(($TotalRequests * (if ($TargetService -eq 'both') { 2 } else { 1 })) / $totalDuration, 2))" -ForegroundColor Cyan
Write-Host ""