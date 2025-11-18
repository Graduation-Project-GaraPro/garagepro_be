# Script test SignalR endpoints
$baseUrl = "http://localhost:5117"

Write-Host "=== Testing SignalR EmergencyRequest Endpoints ===" -ForegroundColor Cyan
Write-Host ""

# Test 1: Test EmergencyRequestCreated
Write-Host "1. Testing EmergencyRequestCreated..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/TestSignalR/test-created" -Method POST -ContentType "application/json"
    Write-Host "   ✓ Success!" -ForegroundColor Green
    Write-Host "   Response: $($response.Message)" -ForegroundColor Gray
    Write-Host "   EmergencyRequestId: $($response.Data.EmergencyRequestId)" -ForegroundColor Gray
    Write-Host ""
} catch {
    Write-Host "   ✗ Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
}

# Test 2: Test EmergencyRequestApproved
Write-Host "2. Testing EmergencyRequestApproved..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/TestSignalR/test-approved" -Method POST -ContentType "application/json"
    Write-Host "   ✓ Success!" -ForegroundColor Green
    Write-Host "   Response: $($response.Message)" -ForegroundColor Gray
    Write-Host "   EmergencyRequestId: $($response.Data.EmergencyRequestId)" -ForegroundColor Gray
    Write-Host ""
} catch {
    Write-Host "   ✗ Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
}

# Test 3: Test EmergencyRequestRejected
Write-Host "3. Testing EmergencyRequestRejected..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/TestSignalR/test-rejected" -Method POST -ContentType "application/json"
    Write-Host "   ✓ Success!" -ForegroundColor Green
    Write-Host "   Response: $($response.Message)" -ForegroundColor Gray
    Write-Host "   EmergencyRequestId: $($response.Data.EmergencyRequestId)" -ForegroundColor Gray
    Write-Host ""
} catch {
    Write-Host "   ✗ Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
}

Write-Host "=== Test Complete ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "To test real-time notifications:" -ForegroundColor Yellow
Write-Host "1. Open browser and go to: http://localhost:5117/test-signalr.html" -ForegroundColor White
Write-Host "2. Click 'Connect' button" -ForegroundColor White
Write-Host "3. Click test buttons to send notifications" -ForegroundColor White
Write-Host "4. Watch the logs update in real-time!" -ForegroundColor White

