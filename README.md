# 🛡️ Anti-Fraud Service

Servicio de análisis anti-fraude en tiempo real para transacciones financieras, desarrollado con .NET 8 y AWS Lambda.

## 📋 Características

- ✅ **Análisis en tiempo real** de transacciones
- ✅ **Reglas de detección de fraude** configurables
- ✅ **Integración con Kafka** para eventos
- ✅ **Base de datos PostgreSQL** con Entity Framework Core
- ✅ **API REST** completa con documentación OpenAPI
- ✅ **Background services** para procesamiento asíncrono
- ✅ **Logging estructurado** para auditoría
- ✅ **Patrón ResponseBuilder** para respuestas consistentes

## 🏗️ Arquitectura

El servicio sigue una arquitectura limpia (Clean Architecture) con las siguientes capas:

```
├── 📁 Domain/           # Entidades y reglas de negocio
├── 📁 Application/      # Casos de uso y DTOs
├── 📁 Infrastructure/   # Persistencia y servicios externos
└── 📁 Lambda/          # Punto de entrada (AWS Lambda + HTTP Server)
```

### Componentes principales:

- **FraudAnalysisService**: Motor de análisis de fraude
- **KafkaConsumerService**: Consumidor de eventos de transacciones
- **KafkaService**: Productor de eventos de estado
- **TransactionDayRepository**: Gestión de totales diarios

## 🚀 Configuración rápida

### Prerrequisitos

- .NET 8 SDK
- PostgreSQL
- Kafka (Confluent Cloud o local)
- Docker (opcional)

### Variables de entorno

```bash
# Base de datos
ConnectionStrings__DefaultConnection="Host=localhost;Database=frauddb;Username=postgres;Password=yourpassword"

# Kafka
Kafka__BootstrapServers="localhost:9092"
Kafka__TransactionEventsTopic="transaction-events"
Kafka__TransactionStatusEventsTopic="transaction-status-events"
Kafka__GroupId="fraud-service-group"

# Logging
Logging__LogLevel__Default="Information"
Logging__LogLevel__Microsoft="Warning"
```

### Instalación y ejecución

1. **Clonar el repositorio**
   ```bash
   git clone https://github.com/garciav999/FraudService.git
   cd FraudService
   ```

2. **Restaurar dependencias**
   ```bash
   dotnet restore
   ```

3. **Configurar base de datos**
   ```bash
   cd app/src/Infrastructure
   dotnet ef database update
   ```

4. **Ejecutar el servicio**
   ```bash
   cd app/src/Lambda
   dotnet run
   ```

5. **Acceder a la aplicación**
   - HTTP Server: `http://localhost:5051`
   - Swagger UI: `http://localhost:5051/swagger` (si está configurado)

### Con Docker

```bash
cd app
docker-compose up -d
```

## 📚 API Endpoints

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `POST` | `/analyze-fraud` | Analizar transacción para detección de fraude |
| `POST` | `/upsert-transaction-day` | Crear/actualizar total diario |
| `POST` | `/start-kafka-consumer` | Iniciar consumidor Kafka manualmente |
| `GET`  | `/health` | Estado de salud del servicio |

### Ejemplo de análisis de fraude

```bash
curl -X POST http://localhost:5051/analyze-fraud \
  -H "Content-Type: application/json" \
  -d '{
    "transactionExternalId": "550e8400-e29b-41d4-a716-446655440001",
    "sourceAccountId": "11111111-1111-1111-1111-111111111111",
    "targetAccountId": "22222222-2222-2222-2222-222222222222",
    "transferTypeId": 1,
    "value": 1500.00,
    "status": "Pending",
    "id": "550e8400-e29b-41d4-a716-446655440002",
    "occurredAt": "2025-10-24T14:30:00Z",
    "eventType": "transaction.created"
  }'
```

**Respuesta de transacción aprobada:**
```json
{
  "success": true,
  "data": {
    "isApproved": true,
    "reason": "Transaction approved",
    "riskScore": 0,
    "riskFactors": []
  },
  "message": "Fraud analysis completed successfully",
  "error": null,
  "timestamp": "2025-10-24T14:30:00Z"
}
```

## 🔍 Reglas de detección de fraude

### Reglas actuales:

1. **Límite individual**: Transacciones > $2,500 son rechazadas
2. **Límite diario**: Total diario por cuenta > $20,500 es rechazado

### Configuración de reglas:

Las reglas están definidas en `FraudAnalysisService.cs` y pueden ser configuradas mediante:

```csharp
private const decimal INDIVIDUAL_LIMIT = 2500m;
private const decimal DAILY_LIMIT = 20500m;
```

## 🔄 Flujo de eventos Kafka

### Topics utilizados:

- **`transaction-events`** (entrada): Eventos de transacciones creadas
- **`transaction-status-events`** (salida): Resultados del análisis

### Estructura de eventos:

**Evento de entrada (TransactionCreatedEvent):**
```json
{
  "TransactionExternalId": "uuid",
  "SourceAccountId": "uuid",
  "TargetAccountId": "uuid", 
  "TransferTypeId": 1,
  "Value": 1500.00,
  "Status": "Pending",
  "Id": "uuid",
  "OccurredAt": "2025-10-24T14:30:00Z",
  "EventType": "transaction.created"
}
```

**Evento de salida (TransactionStatusEvent):**
```json
{
  "TransactionExternalId": "uuid",
  "Status": "Approved",
  "Reason": "Transaction approved",
  "ProcessedAt": "2025-10-24T14:30:01Z"
}
```

## 🗄️ Base de datos

### Tabla TransactionDay

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | GUID | Identificador único |
| `SourceAccountId` | GUID | ID de la cuenta |
| `TransactionDate` | DateTime | Fecha de la transacción |
| `TotalAmount` | Decimal | Total acumulado del día |
| `CreatedAt` | DateTime | Fecha de creación |
| `UpdatedAt` | DateTime | Fecha de actualización |

### Migraciones

```bash
# Crear migración
dotnet ef migrations add MigrationName

# Aplicar migraciones
dotnet ef database update
```

## 🧪 Testing

### Ejecutar tests unitarios

```bash
cd tests
dotnet test
```

### Testing manual

El servicio incluye un servidor HTTP para testing manual de endpoints.

## 📊 Monitoreo y Logging

### Logs estructurados

El servicio utiliza logging estructurado para facilitar el monitoreo:

```csharp
_logger.LogInformation("Fraud analysis completed for transaction {TransactionId} with result {IsApproved}", 
    transactionId, result.IsApproved);
```

### Métricas importantes

- Transacciones procesadas por minuto
- Porcentaje de transacciones rechazadas
- Tiempo promedio de análisis
- Estado del consumidor Kafka

## 🔧 Desarrollo

### Estructura del proyecto

```
FraudService/
├── app/
│   ├── docker-compose.yml
│   ├── FraudService.sln
│   └── src/
│       ├── Domain/              # Entidades de dominio
│       ├── Application/         # Casos de uso y DTOs
│       ├── Infrastructure/      # Persistencia y servicios
│       └── Lambda/             # Punto de entrada
├── tests/                      # Tests unitarios
├── docs/
│   ├── openapi.yml            # Documentación API
│   └── sequence-diagram.md    # Diagrama de secuencia
└── README.md
```

### Convenciones de código

- Utilizar `PascalCase` para clases y métodos públicos
- Utilizar `camelCase` para variables locales
- Incluir logging para operaciones importantes
- Manejar excepciones apropiadamente
- Seguir principios SOLID

### Agregar nuevas reglas de fraude

1. Extender `IFraudAnalysisService`
2. Implementar lógica en `FraudAnalysisService`
3. Agregar tests unitarios
4. Actualizar documentación

## 🚀 Deployment

### AWS Lambda

El servicio está configurado para ejecutarse como AWS Lambda:

```bash
# Publicar a AWS
dotnet lambda deploy-function
```

### Configuración de producción

- Configurar variables de entorno en AWS
- Establecer políticas IAM apropiadas
- Configurar VPC para acceso a base de datos
- Configurar CloudWatch para logs

## 🤝 Contribución

1. Fork el proyecto
2. Crear una rama para tu feature (`git checkout -b feature/nueva-regla`)
3. Commit tus cambios (`git commit -am 'Agregar nueva regla de fraude'`)
4. Push a la rama (`git push origin feature/nueva-regla`)
5. Crear un Pull Request

## 📝 Licencia

Este proyecto está bajo la Licencia MIT. Ver `LICENSE` para más detalles.

## 🆘 Soporte

Para soporte y preguntas:

- 📧 Email: antifraud@company.com
- 🐛 Issues: [GitHub Issues](https://github.com/garciav999/FraudService/issues)
- 📚 Documentación: Ver `/docs/openapi.yml`

---

⭐ **¡Dale una estrella si este proyecto te fue útil!**
