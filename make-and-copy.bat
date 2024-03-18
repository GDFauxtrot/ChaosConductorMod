@echo off

Rem devEnvAssetsPath: Path to the Assets folder in a decompiled version of the game running in the Unity editor
SET devEnvAssetsPath="D:\Unity\SonicMurderMod-Unity\Assets"
Rem bepInExPluginsPath: Path to the BepInEx\plugins folder located at the base of the game's install
SET bepInExPluginsPath="D:\Steam\steamapps\common\Themurderofsonicthehedgehog\The Murder of Sonic The Hedgehog\BepInEx\plugins"

echo //////////////////////////////////////////////////////////// & echo: & echo Building "ChaosConductor.Shared.dll"... & echo: & echo:

Rem Make editor DLL (mod depends on it)
dotnet build ChaosConductor.Shared.csproj -verbosity:m

echo Building done. Copying to lib, Unity project, and game... & echo:

Rem Copy editor DLL into multiple places - main mod library, Unity editor, game's plugin folder
copy "bin\Debug\net46\ChaosConductor.Shared.dll" "lib\"
if defined bepInExPluginsPath copy "bin\Debug\net46\ChaosConductor.Shared.dll" %bepInExPluginsPath%
if defined devEnvAssetsPath copy "bin\Debug\net46\ChaosConductor.Shared.dll" %devEnvAssetsPath%

echo: & echo Copy done. & echo:

echo: & echo //////////////////////////////////////////////////////////// & echo: & echo Building "ChaosConductor.dll"... & echo: & echo:

dotnet build ChaosConductor.csproj -verbosity:m

echo Building done. Copying to game... & echo:

Rem Copy main DLL into game's plugins folder
if defined bepInExPluginsPath copy "bin\Debug\net46\ChaosConductor.dll" %bepInExPluginsPath%

echo: & echo Copy done. & echo:

timeout 3