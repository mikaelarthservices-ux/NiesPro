@echo off
:: ===============================================
:: Lancement Rapide des Tests - NiesPro
:: ===============================================

echo.
echo ===============================================
echo 🧪 ESPACE DE TESTS - NIESPRO
echo ===============================================
echo.

:MENU
echo Choisissez une option:
echo.
echo [1] Test de compilation simple
echo [2] Test de compilation détaillé  
echo [3] Démarrer tous les services
echo [4] Tester la santé des services
echo [5] Ouvrir la documentation des tests
echo [6] Quitter
echo.
set /p choice="Votre choix (1-6): "

if "%choice%"=="1" goto TEST_SIMPLE
if "%choice%"=="2" goto TEST_DETAILED
if "%choice%"=="3" goto START_SERVICES
if "%choice%"=="4" goto TEST_HEALTH
if "%choice%"=="5" goto OPEN_DOCS
if "%choice%"=="6" goto EXIT

echo Choix invalide. Veuillez réessayer.
goto MENU

:TEST_SIMPLE
echo.
echo 🔧 Lancement du test de compilation simple...
powershell -ExecutionPolicy Bypass -File "%~dp0Scripts\test-simple.ps1"
pause
goto MENU

:TEST_DETAILED
echo.
echo 🔍 Lancement du test de compilation détaillé...
powershell -ExecutionPolicy Bypass -File "%~dp0Scripts\test-compilation.ps1"
pause
goto MENU

:START_SERVICES
echo.
echo 🚀 Démarrage des services...
echo Note: Cette option nécessite que les services soient compilés
powershell -ExecutionPolicy Bypass -File "%~dp0Scripts\test-services.ps1" -StartServices
pause
goto MENU

:TEST_HEALTH
echo.
echo 🩺 Test de santé des services...
powershell -ExecutionPolicy Bypass -File "%~dp0Scripts\test-services.ps1" -CheckHealth
pause
goto MENU

:OPEN_DOCS
echo.
echo 📖 Ouverture de la documentation...
start "%~dp0README.md"
start "%~dp0Api\README.md"
goto MENU

:EXIT
echo.
echo Au revoir! 👋
echo.
exit /b 0