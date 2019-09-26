dotnet clean
dotnet build ./src/Fluxup.Core/Fluxup.Core.csproj -c Release
dotnet pack ./src/Fluxup.Core/Fluxup.Core.csproj -c Release
dotnet build ./src/Fluxup.Updater/Fluxup.Updater.csproj -c Release
dotnet pack ./src/Fluxup.Updater/Fluxup.Updater.csproj -c Release
Invoke-WebRequest -OutFile nuget.exe https://dist.nuget.org/win-x86-commandline/latest/nuget.exe
cd ./src/Fluxup.Core/bin/Release/
.\"../../../../nuget.exe" push *.nupkg -Source "GitHub"
cd ../../../../
cd ./src/Fluxup.Updater/bin/Release/
.\"../../../../nuget.exe" push *.nupkg -Source "GitHub"
cd ../../../../