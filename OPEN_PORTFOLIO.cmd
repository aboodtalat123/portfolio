@echo off
setlocal
cd /d "%~dp0"

start "" powershell.exe -NoProfile -WindowStyle Hidden -Command "$url='http://127.0.0.1:5180/'; for($i=0;$i -lt 45;$i++){ try { Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 2 | Out-Null; Start-Process $url; exit 0 } catch { Start-Sleep -Seconds 1 } }; Start-Process $url"

set ASPNETCORE_ENVIRONMENT=Development
dotnet run --project "src\Portfolio.Web\Portfolio.Web.csproj" --no-launch-profile --urls "http://127.0.0.1:5180"

endlocal
