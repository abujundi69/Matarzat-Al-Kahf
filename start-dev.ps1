[CmdletBinding()]
param(
    [switch]$Seed
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
$root = $PSScriptRoot
$backend = Join-Path $root 'Backend'
$apiProject = Join-Path $backend 'MatarzatAlKahf.Api'
$frontend = Join-Path $backend 'Frontend'
$logs = Join-Path $root '.runtime-logs'
$apiUrl = 'http://127.0.0.1:5212'
$frontendUrl = 'http://127.0.0.1:5173'
$healthUrl = "$apiUrl/api/v1/health"

New-Item -ItemType Directory -Path $logs -Force | Out-Null

function Get-PortOwner([int]$Port) {
    $listener = Get-NetTCPConnection -LocalPort $Port -State Listen `
        -ErrorAction SilentlyContinue | Select-Object -First 1
    if (-not $listener) {
        return $null
    }

    Get-CimInstance Win32_Process -Filter "ProcessId = $($listener.OwningProcess)"
}

function Clear-StaleProjectListener([int]$Port, [string]$ProjectMarker) {
    $owner = Get-PortOwner $Port
    if (-not $owner) {
        return
    }

    if ([string]$owner.CommandLine -notlike "*$ProjectMarker*") {
        throw "Port $Port is used by unrelated process $($owner.ProcessId) ($($owner.Name))."
    }

    Write-Host "Stopping stale project process $($owner.ProcessId) on port $Port..."
    Stop-Process -Id $owner.ProcessId
    $ownerProcess = Get-Process -Id $owner.ProcessId -ErrorAction SilentlyContinue
    if ($ownerProcess) {
        $ownerProcess.WaitForExit(5000)
    }
}

Clear-StaleProjectListener 5212 'MatarzatAlKahf.Api'
Clear-StaleProjectListener 5173 'vite'

Push-Location $backend
try {
    & dotnet ef database update --project MatarzatAlKahf.Infrastructure `
        --startup-project MatarzatAlKahf.Api
    if ($LASTEXITCODE -ne 0) {
        throw 'Database migration failed.'
    }

    if ($Seed) {
        $env:ASPNETCORE_ENVIRONMENT = 'Development'
        & dotnet run --project MatarzatAlKahf.Api --no-launch-profile `
            -- --seed-development
        if ($LASTEXITCODE -ne 0) {
            throw 'Development seed failed.'
        }
    }
}
finally {
    Pop-Location
}

$apiOut = Join-Path $logs 'backend.out.log'
$apiErr = Join-Path $logs 'backend.err.log'
$apiProcess = Start-Process dotnet -ArgumentList @(
    'run', '--project', $apiProject, '--launch-profile', 'http'
) -WorkingDirectory $backend -RedirectStandardOutput $apiOut `
    -RedirectStandardError $apiErr -WindowStyle Hidden -PassThru

$healthy = $false
for ($attempt = 1; $attempt -le 60; $attempt++) {
    if ($apiProcess.HasExited) {
        Get-Content -LiteralPath $apiOut -Tail 80 -ErrorAction SilentlyContinue
        Get-Content -LiteralPath $apiErr -Tail 80 -ErrorAction SilentlyContinue
        throw "Backend exited with code $($apiProcess.ExitCode)."
    }
    try {
        $response = Invoke-WebRequest $healthUrl -UseBasicParsing -TimeoutSec 2
        if ($response.StatusCode -eq 200) {
            $healthy = $true
            break
        }
    }
    catch {
        Start-Sleep -Milliseconds 500
    }
}
if (-not $healthy) {
    throw "Backend did not become healthy at $healthUrl."
}

$frontendOut = Join-Path $logs 'frontend.out.log'
$frontendErr = Join-Path $logs 'frontend.err.log'
$frontendProcess = Start-Process npm.cmd -ArgumentList @('run', 'dev') `
    -WorkingDirectory $frontend -RedirectStandardOutput $frontendOut `
    -RedirectStandardError $frontendErr -WindowStyle Hidden -PassThru

$frontendReady = $false
for ($attempt = 1; $attempt -le 40; $attempt++) {
    if ($frontendProcess.HasExited) {
        Get-Content -LiteralPath $frontendOut -Tail 80 -ErrorAction SilentlyContinue
        Get-Content -LiteralPath $frontendErr -Tail 80 -ErrorAction SilentlyContinue
        throw "Frontend exited with code $($frontendProcess.ExitCode)."
    }
    try {
        $response = Invoke-WebRequest $frontendUrl -UseBasicParsing -TimeoutSec 2
        if ($response.StatusCode -eq 200) {
            $frontendReady = $true
            break
        }
    }
    catch {
        Start-Sleep -Milliseconds 500
    }
}
if (-not $frontendReady) {
    throw "Frontend did not become available at $frontendUrl."
}

$proxiedHealthUrl = "$frontendUrl/api/v1/health"
try {
    $proxiedHealth = Invoke-WebRequest $proxiedHealthUrl -UseBasicParsing `
        -TimeoutSec 5
}
catch {
    throw "Vite proxy health check failed at $proxiedHealthUrl`: $($_.Exception.Message)"
}
if ($proxiedHealth.StatusCode -ne 200) {
    throw "Vite proxy health check returned HTTP $($proxiedHealth.StatusCode)."
}

Write-Host ''
Write-Host "Backend:  $apiUrl (PID $($apiProcess.Id))"
Write-Host "Health:   $healthUrl"
Write-Host "Proxy:    $proxiedHealthUrl"
Write-Host "Swagger:  $apiUrl/swagger/index.html"
Write-Host "Frontend: $frontendUrl (PID $($frontendProcess.Id))"
Write-Host "Logs:     $logs"
