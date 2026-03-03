@echo off
REM Script de instalación para Ruway Security Subscription Hub
REM Ejecutar como Administrador

set SERVICE_NAME="Ruway Security Subscription Hub"
set SERVICE_DISPLAY_NAME="Ruway Security Subscription Hub"
set SERVICE_DESCRIPTION="Servicio que se suscribe a eventos de empleados y beneficiarios para el sistema de seguridad"
set EXECUTABLE_PATH=%~dp0Ruway.Security.Subscription.Hub.exe

echo.
echo ========================================
echo Instalando Ruway Security Subscription Hub
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

REM Detener el servicio si ya está ejecutándose
echo Verificando si el servicio ya existe...
sc query %SERVICE_NAME% >nul 2>&1
if %errorLevel% equ 0 (
    echo Deteniendo servicio existente...
    sc stop %SERVICE_NAME%
    timeout /t 5 /nobreak >nul
    
    echo Eliminando servicio existente...
    sc delete %SERVICE_NAME%
    timeout /t 2 /nobreak >nul
)

REM Verificar que el ejecutable existe
if not exist "%EXECUTABLE_PATH%" (
    echo ERROR: No se encontró el archivo ejecutable: %EXECUTABLE_PATH%
    echo Asegúrese de que el proyecto esté compilado y publicado.
    pause
    exit /b 1
)

REM Crear el servicio
echo Creando el servicio...
sc create %SERVICE_NAME% binpath= "\"%EXECUTABLE_PATH%\"" DisplayName= %SERVICE_DISPLAY_NAME%

if %errorLevel% neq 0 (
    echo ERROR: No se pudo crear el servicio
    pause
    exit /b 1
)

REM Configurar el servicio
echo Configurando el servicio...
sc config %SERVICE_NAME% start= auto
sc description %SERVICE_NAME% %SERVICE_DESCRIPTION%

REM Configurar acciones de recuperación ante fallos
echo Configurando recuperación ante fallos...
sc failure %SERVICE_NAME% reset= 86400 actions= restart/60000/restart/60000/restart/60000

REM Iniciar el servicio
echo Iniciando el servicio...
sc start %SERVICE_NAME%

if %errorLevel% equ 0 (
    echo.
    echo ========================================
    echo Instalación completada exitosamente
    echo ========================================
    echo.
    echo El servicio %SERVICE_NAME% ha sido instalado y iniciado.
    echo.
    echo Para verificar el estado:
    echo   sc query %SERVICE_NAME%
    echo.
    echo Para ver los logs:
    echo   Revise la carpeta logs\ en el directorio de instalación
    echo.
) else (
    echo.
    echo ========================================
    echo Error en la instalación
    echo ========================================
    echo.
    echo El servicio se creó pero no se pudo iniciar.
    echo Verifique:
    echo - Configuración de RabbitMQ en appsettings.json
    echo - Conectividad de red
    echo - Permisos del usuario del servicio
    echo.
)

pause