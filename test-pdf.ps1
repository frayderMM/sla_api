# Script para probar la generacion de PDF
Write-Host "Autenticando..." -ForegroundColor Cyan

$loginBody = @{
    email = "analista@tcs.com"
    password = "Analista123!"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "http://localhost:5149/api/Auth/login" `
        -Method POST `
        -Body $loginBody `
        -ContentType "application/json"
    
    $token = $loginResponse.token
    Write-Host "Autenticado exitosamente" -ForegroundColor Green
    
    Write-Host "Generando PDF..." -ForegroundColor Cyan
    
    $headers = @{
        Authorization = "Bearer $token"
    }
    
    $pdfPath = Join-Path $PSScriptRoot "Reporte_SLA_Generado.pdf"
    
    Invoke-RestMethod -Uri "http://localhost:5149/api/Reportes/pdf" `
        -Method GET `
        -Headers $headers `
        -OutFile $pdfPath
    
    if (Test-Path $pdfPath) {
        $fileInfo = Get-Item $pdfPath
        Write-Host ""
        Write-Host "PDF GENERADO EXITOSAMENTE!" -ForegroundColor Green
        Write-Host "   Ubicacion: $($fileInfo.FullName)" -ForegroundColor Yellow
        Write-Host "   Tamano: $($fileInfo.Length) bytes" -ForegroundColor Yellow
        Write-Host "   Generado: $($fileInfo.LastWriteTime)" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Abriendo PDF..." -ForegroundColor Cyan
        Start-Process $pdfPath
    } else {
        Write-Host "El archivo PDF no se creo" -ForegroundColor Red
    }
    
} catch {
    Write-Host ""
    Write-Host "ERROR:" -ForegroundColor Red
    Write-Host "   $($_.Exception.Message)" -ForegroundColor Yellow
}
