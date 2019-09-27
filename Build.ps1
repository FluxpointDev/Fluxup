dotnet clean
dotnet build ./src/Fluxup.Core/Fluxup.Core.csproj -c Release
dotnet pack ./src/Fluxup.Core/Fluxup.Core.csproj -c Release
dotnet build ./src/Fluxup.Updater/Fluxup.Updater.csproj -c Release
dotnet pack ./src/Fluxup.Updater/Fluxup.Updater.csproj -c Release