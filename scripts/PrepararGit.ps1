# Solo retira archivos generados del índice. No borra archivos ni hace commit o push.
$ErrorActionPreference = 'Stop'
$raizProyecto = Split-Path -Parent $PSScriptRoot
if (-not (Test-Path -LiteralPath (Join-Path $raizProyecto '.git'))) { throw 'No se encontró .git en el proyecto.' }
Push-Location -LiteralPath $raizProyecto
try {
    $archivosVersionados = @(git ls-files)
    if ($LASTEXITCODE -ne 0) { throw 'No se pudo leer el índice.' }
    # Detecta las carpetas también cuando sus nombres en el índice son anteriores al renombrado.
    $carpetasGeneradas = @($archivosVersionados | ForEach-Object {
        if ($_ -match '^(.+?/(?:bin|obj))/') { $Matches[1] }
    } | Sort-Object -Unique)
    if ($carpetasGeneradas.Count -gt 0) {
        git rm -r --cached --quiet --ignore-unmatch -- @carpetasGeneradas
        if ($LASTEXITCODE -ne 0) { throw 'Git no pudo actualizar el índice. No se hizo commit ni push.' }
    }
    git diff --cached --shortstat
    Write-Output 'Índice preparado para revisar. No se borró ningún archivo del disco.'
} finally { Pop-Location }
