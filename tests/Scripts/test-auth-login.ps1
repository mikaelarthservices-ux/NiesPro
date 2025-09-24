# Test de connexion Auth.API
Write-Host "=== TEST DE CONNEXION AUTH.API ===" -ForegroundColor Green

$loginData = @{
    "username" = "testuser"
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
    
    # Sauvegarder le token pour les tests suivants
    if ($response.data -and $response.data.accessToken) {
        $response.data.accessToken | Out-File -FilePath "C:\Users\HP\Documents\projets\NiesPro\tests\Scripts\access-token.txt" -Encoding UTF8
        Write-Host "[INFO] Token sauvegardé dans access-token.txt" -ForegroundColor Blue
    }
    
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