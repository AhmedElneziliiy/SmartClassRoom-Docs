# Stop any running SmartClassRoom processes
Write-Host "Stopping any running SmartClassRoom processes..."
Get-Process | Where-Object {$_.ProcessName -like '*SmartClassRoom*'} | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2

# Navigate to the project directory and build
Write-Host "Building the application..."
cd "C:\Users\Kareem Usama\Desktop\SMARTCLASSROOMDOCS\SmartClassRoom-LetUNO\SmartClassRoom.Web"
dotnet build

Write-Host "`nApplication built successfully. Please run it manually using 'dotnet run' or from Visual Studio."
