# Ruway Security Subscription Hub

Servicio de Windows que maneja las suscripciones a eventos de empleados y beneficiarios para el sistema de seguridad de Ruway.

## Descripción

Este servicio se ejecuta como un servicio de Windows y está diseñado para:

- Suscribirse a eventos de empleados (creación y actualización)
- Suscribirse a eventos de beneficiarios (creación y actualización)
- Procesar estos eventos para mantener sincronizado el sistema de seguridad

## Eventos Soportados

### Eventos de Empleados
- `EmployeeCreatedEvent`: Se activa cuando se crea un nuevo empleado
- `EmployeeUpdatedEvent`: Se activa cuando se actualiza un empleado existente

### Eventos de Beneficiarios
- `BeneficiaryCreatedEvent`: Se activa cuando se crea un nuevo beneficiario
- `BeneficiaryUpdatedEvent`: Se activa cuando se actualiza un beneficiario existente

## Configuración

La configuración del servicio se maneja a través de los archivos `appsettings.json`:

### Configuración de RabbitMQ
```json
{
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "ExchangeName": "ruway_events_exchange",
    "ExchangeType": "topic",
    "QueueName": "ruway_security_subscription_queue",
    "RoutingKey": "*.events.*"
  }
}
```

## Instalación como Servicio de Windows

### 1. Compilar el proyecto
```bash
dotnet publish -c Release -r win-x64 --self-contained false
```

### 2. Instalar como servicio
```bash
sc create "Ruway Security Subscription Hub" binpath="C:\ruta\a\Ruway.Security.Subscription.Hub.exe"
```

### 3. Configurar el servicio
```bash
sc config "Ruway Security Subscription Hub" start=auto
sc description "Ruway Security Subscription Hub" "Servicio que se suscribe a eventos de empleados y beneficiarios"
```

### 4. Iniciar el servicio
```bash
sc start "Ruway Security Subscription Hub"
```

## Desarrollo Local

### Prerrequisitos
- .NET 9.0 SDK
- RabbitMQ Server ejecutándose localmente
- Acceso a las librerías de eventos compartidas

### Ejecutar en modo desarrollo
```bash
dotnet run --environment Development
```

### Ejecutar como consola (no como servicio)
```bash
dotnet run --console
```

## Logging

El servicio utiliza Serilog para el logging. Los logs se escriben:
- En la consola (durante desarrollo)
- En archivos rotatorios en la carpeta `logs/`

## Estructura del Proyecto

```
Ruway.Security.Subscription.Hub/
├── Extensions/
│   └── ServiceCollectionExtensions.cs
├── Services/
│   └── EventSubscriptionService.cs
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
├── appsettings.Production.json
└── README.md
```

## Lógica de Negocio

Los métodos de procesamiento de eventos están preparados para implementar la lógica de negocio específica:

- `ProcessEmployeeCreatedBusinessLogic`: Lógica para procesar empleados creados
- `ProcessEmployeeUpdatedBusinessLogic`: Lógica para procesar empleados actualizados  
- `ProcessBeneficiaryCreatedBusinessLogic`: Lógica para procesar beneficiarios creados
- `ProcessBeneficiaryUpdatedBusinessLogic`: Lógica para procesar beneficiarios actualizados

## Monitoreo

El servicio registra información importante sobre:
- Inicio y parada del servicio
- Estado de las suscripciones a eventos
- Procesamiento exitoso de eventos
- Errores durante el procesamiento

## Comandos Útiles

### Verificar estado del servicio
```bash
sc query "Ruway Security Subscription Hub"
```

### Detener el servicio
```bash
sc stop "Ruway Security Subscription Hub"
```

### Desinstalar el servicio
```bash
sc delete "Ruway Security Subscription Hub"
```

## Troubleshooting

### 1. El servicio no arranca
- Verificar que RabbitMQ esté ejecutándose
- Revisar la configuración en appsettings.json
- Consultar los logs del sistema de Windows

### 2. No se reciben eventos
- Verificar la conectividad con RabbitMQ
- Confirmar que el exchange y queue estén configurados correctamente
- Revisar que los routing keys coincidan con los eventos

### 3. Errores de permisos
- Asegurar que el usuario del servicio tenga permisos de escritura en la carpeta de logs
- Verificar permisos de red para acceder a RabbitMQ