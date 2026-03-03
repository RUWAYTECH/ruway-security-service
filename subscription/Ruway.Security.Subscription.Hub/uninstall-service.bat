@echo off
REM Script de desinstalación para Ruway Security Subscription Hub
REM Ejecutar como Administrador

set SERVICE_NAME="Ruway Security Subscription Hub"

echo.
echo ========================================
echo Desinstalando Ruway Security Subscription Hub
echo ========================================
echo.

REM Verificar si se está ejecutando como administrador
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: Este script debe ejecutarse como Administrador
    echo Haga clic derecho en el archivo y seleccione "Ejecutar como administrador"
    pause
    exit /b 1
)

REM Verificar si el servicio existe
echo Verificando si el servicio existe...
sc query %SERVICE_NAME% >nul 2>&1
if %errorLevel% neq 0 (
    echo El servicio %SERVICE_NAME% no está instalado.
    pause
    exit /b 0
)

REM Detener el servicio
echo Deteniendo el servicio...
sc stop %SERVICE_NAME%
if %errorLevel% equ 0 (
    echo Servicio detenido correctamente.
    timeout /t 5 /nobreak >nul
) else (
    echo El servicio ya estaba detenido o no se pudo detener.
)

REM Eliminar el servicio
echo Eliminando el servicio...
sc delete %SERVICE_NAME%

if %errorLevel% equ 0 (
    echo.
    echo ========================================
    echo Desinstalación completada exitosamente
    echo ========================================
    echo.
    echo El servicio %SERVICE_NAME% ha sido eliminado.
    echo.
    echo NOTA: Los archivos de la aplicación y logs no han sido eliminados.
    echo Si desea eliminarlos completamente, hágalo manualmente.
    echo.
) else (
    echo.
    echo ========================================
    echo Error en la desinstalación
    echo ========================================
    echo.
    echo No se pudo eliminar el servicio %SERVICE_NAME%.
    echo.
)

pause