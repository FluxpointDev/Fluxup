Invoke-WebRequest -OutFile nuget.exe https://dist.nuget.org/win-x86-commandline/latest/nuget.exe
cd ./src/Fluxup.Core/bin/Release/
.\"../../../../nuget.exe" push *.nupkg -Source "GitHub"
cd ../../../../
cd ./src/Fluxup.Updater/bin/Release/
.\"../../../../nuget.exe" push *.nupkg -Source "GitHub"
cd ../../../../