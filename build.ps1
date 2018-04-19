$NuGetVersion = 'v4.6.1'
$VisualStudioVersion = '15.0'
$VSWhereVersion = '2.4.1'

#####

$NuGetUrl = "https://dist.nuget.org/win-x86-commandline/$NuGetVersion/NuGet.exe"

$ScriptDir = Split-Path $script:MyInvocation.MyCommand.Path

Push-Location $ScriptDir

# determine cache dir
$NuGetCacheDir = "$env:LocalAppData\.nuget\$NuGetVersion"
$CachedNuGetExecutable = Join-Path $NuGetCacheDir NuGet.exe
$NuGetExecutable = Join-Path tools NuGet.exe

# download nuget to cache dir
if ( ! ( Test-Path $CachedNuGetExecutable ) ) {
    if ( ! ( Test-Path $NuGetCacheDir ) ) {
        New-Item -Type directory $NuGetCacheDir > $nul
    }

    Write-Output "Downloading '$NuGetUrl' to '$CachedNuGetExecutable'..."
    $ProgressPreference = 'SilentlyContinue'
    Invoke-WebRequest $NuGetUrl -OutFile $CachedNuGetExecutable
}

Write-Output "Copying NuGet.exe to $NuGetExecutable"
Copy-Item $CachedNuGetExecutable $NuGetExecutable

# restore packages
Write-Output "Restoring NuGet packages for build script"
& $NuGetExecutable restore .\tools\packages.config -PackagesDirectory .\tools\packages -Verbosity quiet

$VSDir = & ".\tools\packages\vswhere.$VSWhereVersion\tools\vswhere.exe" -version $VisualStudioVersion -products * -requires Microsoft.Component.MSBuild -property installationPath
if ($VSDir) {
    $CSIPath = join-path $VSDir "MSBuild\$VisualStudioVersion\Bin\Roslyn\csi.exe"
    if (test-path $CSIPath) {
        & $CSIPath .\tools\build.csx $args
    }
}

Pop-Location