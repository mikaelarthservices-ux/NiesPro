# Script rapide de verification des services
$ports = @(5000, 5001, 5002, 5003, 5004)
$services = @("Gateway", "Auth", "Order", "Catalog", "Payment")

Write-Host "Status des services NiesPro:" -ForegroundColor Cyan

for ($i = 0; $i -lt $ports.Count; $i++) {
    $port = $ports[$i]
    $service = $services[$i]
    
    try {
        $tcpClient = New-Object System.Net.Sockets.TcpClient
        $connection = $tcpClient.BeginConnect("localhost", $port, $null, $null)
        $wait = $connection.AsyncWaitHandle.WaitOne(500, $false)
        
        if ($wait) {
            $tcpClient.EndConnect($connection)
            $tcpClient.Close()
            Write-Host "  [$port] $service API: " -NoNewline -ForegroundColor Yellow
            Write-Host "ONLINE" -ForegroundColor Green
        } else {
            $tcpClient.Close()
            Write-Host "  [$port] $service API: " -NoNewline -ForegroundColor Yellow
            Write-Host "OFFLINE" -ForegroundColor Red
        }
    } catch {
        Write-Host "  [$port] $service API: " -NoNewline -ForegroundColor Yellow
        Write-Host "OFFLINE" -ForegroundColor Red
    }
}