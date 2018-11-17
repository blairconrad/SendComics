@pushd %~dp0
@dotnet run --project ".\tools\SendComics.Build\SendComics.Build.csproj" -- %*
@popd
