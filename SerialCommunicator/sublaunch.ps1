$url = "http://localhost:5000/"

function Is-Application-Running {
    try {
        $response = Invoke-WebRequest -Uri $url -UseBasicParsing
        if ($response.StatusCode -eq 200) {
            return $true
        } else {
            return $false
        }
    } catch {
        return $false
    }
}

if (-not (Is-Application-Running)) {

    Write-Host "Application is down, starting SerialCommunicator..."
    Start-Process $url
    Invoke-Expression -Command ".\SerialCommunicator.exe"
}
else {

    Start-Process $url
}
