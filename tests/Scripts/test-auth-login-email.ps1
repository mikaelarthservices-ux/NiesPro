# Test de connexion Auth.API avec email
Write-Host "=== TEST DE CONNEXION AUTH.API (avec email) ===" -ForegroundColor Green

$loginData = @{
    "email" = "test@niespro.com"      # Champ correct selon LoginRequest
    "password" = "TestPassword123!"
    "deviceKey" = "test-device-key-12345"
    "deviceName" = "Test Device"
    "rememberMe" = $true
}

$body = $loginData | ConvertTo-Json -Depth 3
$endpoint = "http://localhost:5001/api/v1/auth/login"

Write-Host "Endpoint: $endpoint" -ForegroundColor Yellow
Write-Host "Body: $body" -ForegroundColor Cyan

try {
    $response = Invoke-RestMethod -Uri $endpoint -Method POST -Body $body -ContentType "application/json"
    
    Write-Host "[SUCCESS] Connexion réussie !" -ForegroundColor Green
    Write-Host "Response:" -ForegroundColor Yellow
    $response | ConvertTo-Json -Depth 3 | Write-Host
    
} catch {
    Write-Host "[ERROR] Échec de la connexion" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        try {
            $reader = [System.IO.StreamReader]::new($_.Exception.Response.GetResponseStream())
            $errorContent = $reader.ReadToEnd()
            Write-Host "Server Response: $errorContent" -ForegroundColor Yellow
        } catch {
            Write-Host "Could not read error response" -ForegroundColor Yellow
        }
    }
}