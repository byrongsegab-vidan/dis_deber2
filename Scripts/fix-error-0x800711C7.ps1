# Ejecutar como ADMINISTRADOR (clic derecho > Ejecutar con PowerShell)
# Corrige el error 0x800711C7: Smart App Control bloquea dis_deber2.dll

$ErrorActionPreference = "Stop"
$projectRoot = Split-Path $PSScriptRoot -Parent
if (-not (Test-Path (Join-Path $projectRoot "dis_deber2\dis_deber2.csproj"))) {
    $projectRoot = "C:\ITSCO DIS\dis_deber2"
}
$webProject = Join-Path $projectRoot "dis_deber2"

Write-Host "=== Fix error 0x800711C7 (Smart App Control) ===" -ForegroundColor Cyan
Write-Host "Proyecto: $webProject"

# 1. Detener IIS Express
Get-Process iisexpress, iisexpresstray -ErrorAction SilentlyContinue | Stop-Process -Force
Write-Host "[OK] IIS Express detenido" -ForegroundColor Green

# 2. Limpiar caché ASP.NET
$tempPaths = @(
    "$env:LOCALAPPDATA\Temp\Temporary ASP.NET Files",
    "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\Temporary ASP.NET Files",
    "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\Temporary ASP.NET Files"
)
foreach ($p in $tempPaths) {
    if (Test-Path $p) {
        Remove-Item "$p\*" -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "[OK] Limpiado: $p" -ForegroundColor Green
    }
}

# 3. Desbloquear archivos del proyecto (Mark of the Web)
if (Test-Path $webProject) {
    Get-ChildItem $webProject -Recurse -Include *.dll,*.exe,*.pdb -ErrorAction SilentlyContinue |
        Unblock-File -ErrorAction SilentlyContinue
    Write-Host "[OK] Unblock-File aplicado" -ForegroundColor Green
}

# 4. Exclusiones en Windows Defender
$exclusions = @(
    $webProject,
    "$env:LOCALAPPDATA\Temp\Temporary ASP.NET Files",
    "${env:ProgramFiles(x86)}\IIS Express",
    "$env:ProgramFiles\IIS Express"
)
foreach ($path in $exclusions) {
    if (Test-Path $path) {
        try {
            Add-MpPreference -ExclusionPath $path -ErrorAction Stop
            Write-Host "[OK] Exclusión Defender: $path" -ForegroundColor Green
        }
        catch {
            if ($_.Exception.Message -match "already exists|ya existe") {
                Write-Host "[--] Ya excluido: $path" -ForegroundColor Yellow
            }
            else {
                Write-Host "[!!] No se pudo excluir (¿ejecutó como admin?): $path" -ForegroundColor Red
            }
        }
    }
}

# 5. Desactivar Smart App Control (requiere reinicio)
$ciPolicy = "HKLM:\SYSTEM\CurrentControlSet\Control\CI\Policy"
$current = (Get-ItemProperty -Path $ciPolicy -Name VerifiedAndReputablePolicyState -ErrorAction SilentlyContinue).VerifiedAndReputablePolicyState
Write-Host ""
Write-Host "Smart App Control (VerifiedAndReputablePolicyState): $current (0=Off, 1=On, 2=Evaluación)" -ForegroundColor Yellow

if ($current -ne 0) {
    try {
        Set-ItemProperty -Path $ciPolicy -Name VerifiedAndReputablePolicyState -Value 0 -Type DWord
        Write-Host "[OK] Smart App Control desactivado en registro. REINICIE Windows." -ForegroundColor Green
    }
    catch {
        Write-Host "[!!] No se pudo modificar el registro. Ejecute este script como ADMINISTRADOR." -ForegroundColor Red
        Write-Host "    O desactívelo manualmente:" -ForegroundColor Red
        Write-Host "    Configuración > Privacidad y seguridad > Seguridad de Windows >" -ForegroundColor Red
        Write-Host "    Control de aplicaciones > Configuración de Smart App Control > Desactivado" -ForegroundColor Red
    }
}
else {
    Write-Host "[OK] Smart App Control ya está desactivado en registro." -ForegroundColor Green
}

Write-Host ""
Write-Host "=== Pasos finales ===" -ForegroundColor Cyan
Write-Host "1. REINICIE el equipo (obligatorio si cambió el registro)"
Write-Host "2. Abra Visual Studio como administrador"
Write-Host "3. Compilar > Limpiar solución > Recompilar solución"
Write-Host "4. Ejecutar con F5"
