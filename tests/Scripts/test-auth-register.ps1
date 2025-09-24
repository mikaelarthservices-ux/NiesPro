# Test d'enregistrement Auth.API avec JSON complet

$body = @{
    username = "testuser"
    email = "test@niespro.com"
    password = "TestPassword123!"
    confirmPassword = "TestPassword123!"
    firstName = "Test"
    lastName = "User"
    phoneNumber = "+33123456789"
    deviceKey = "test-device-key-12345"
    deviceName = "Test Device"
} | ConvertTo-Json

try {
    Write-Host "=== TEST D'ENREGISTREMENT AUTH.API ===" -ForegroundColor Cyan
    Write-Host "Endpoint: http://localhost:5001/api/v1/auth/register" -ForegroundColor White
    Write-Host "Body: $body" -ForegroundColor Yellow
    
    $response = Invoke-RestMethod -Uri "http://localhost:5001/api/v1/auth/register" -Method POST -ContentType "application/json" -Body $body
    
    Write-Host ""
    Write-Host "[SUCCESS] Utilisateur enregistré !" -ForegroundColor Green
    Write-Host "Response:" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 3
    
} catch {
    Write-Host ""
    Write-Host "[ERROR] Échec de l'enregistrement" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    
    if ($_.ErrorDetails) {
        Write-Host "Details:" -ForegroundColor Yellow
        $_.ErrorDetails.Message
    }
}