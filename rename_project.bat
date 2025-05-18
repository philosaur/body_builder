@echo off
setlocal enabledelayedexpansion

if "%~1"=="" (
    echo Usage: rename_project.bat "NewProjectName"
    exit /b 1
)

set "new_name=%~1"
set "old_name=PROJECT_NAME"

echo Current directory: %CD%
echo Renaming project from %old_name% to %new_name%

:: Check if we're in the right directory
if not exist "project.godot" (
    echo Error: project.godot not found in current directory
    echo Please run this script from the project root directory
    exit /b 1
)

:: Update project.godot
echo.
echo Updating project.godot...
if exist "project.godot" (
    powershell -Command "(Get-Content 'project.godot') -replace '%old_name%', '%new_name%' | Set-Content 'project.godot.tmp'"
    move /y "project.godot.tmp" "project.godot" >nul
    echo Updated project.godot
)

:: Update PROJECT_NAME.csproj
echo.
echo Updating PROJECT_NAME.csproj...
if exist "PROJECT_NAME.csproj" (
    powershell -Command "(Get-Content 'PROJECT_NAME.csproj') -replace '%old_name%', '%new_name%' | Set-Content 'PROJECT_NAME.csproj.tmp'"
    move /y "PROJECT_NAME.csproj.tmp" "PROJECT_NAME.csproj" >nul
    echo Updated PROJECT_NAME.csproj
)

:: Update README.md
echo.
echo Updating README.md...
if exist "README.md" (
    powershell -Command "(Get-Content 'README.md') -replace '%old_name%', '%new_name%' | Set-Content 'README.md.tmp'"
    move /y "README.md.tmp" "README.md" >nul
    echo Updated README.md
)

:: Rename the .csproj file
echo.
echo Renaming project files...
if exist "PROJECT_NAME.csproj" (
    echo Found PROJECT_NAME.csproj, renaming to %new_name%.csproj
    ren "PROJECT_NAME.csproj" "%new_name%.csproj"
    if errorlevel 1 (
        echo Error renaming PROJECT_NAME.csproj
        exit /b 1
    )
    echo Successfully renamed PROJECT_NAME.csproj to %new_name%.csproj
) else (
    echo Warning: PROJECT_NAME.csproj not found
)

:: Create solution file
echo.
echo Creating solution file...
(
echo Microsoft Visual Studio Solution File, Format Version 12.00
echo # Visual Studio Version 17
echo VisualStudioVersion = 17.0.31903.59
echo MinimumVisualStudioVersion = 10.0.40219.1
echo Project^("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"^) = "%new_name%", "%new_name%.csproj", "{GUID}"
echo EndProject
echo Global
echo     GlobalSection^(SolutionConfigurationPlatforms^) = preSolution
echo         Debug^|Any CPU = Debug^|Any CPU
echo         Release^|Any CPU = Release^|Any CPU
echo     EndGlobalSection
echo     GlobalSection^(ProjectConfigurationPlatforms^) = postSolution
echo         {GUID}.Debug^|Any CPU.ActiveCfg = Debug^|Any CPU
echo         {GUID}.Debug^|Any CPU.Build.0 = Debug^|Any CPU
echo         {GUID}.Release^|Any CPU.ActiveCfg = Release^|Any CPU
echo         {GUID}.Release^|Any CPU.Build.0 = Release^|Any CPU
echo     EndGlobalSection
echo     GlobalSection^(SolutionProperties^) = preSolution
echo         HideSolutionNode = FALSE
echo     EndGlobalSection
echo EndGlobal
) > "%new_name%.sln"

echo Created %new_name%.sln
echo.
echo Project rename complete! 