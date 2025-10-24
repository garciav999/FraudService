# 🧪 Application Tests

Tests unitarios para la capa de aplicación del servicio anti-fraude usando xUnit, Moq y FluentAssertions.

## 📋 Cobertura de tests

### Componentes testeados:

- ✅ **FraudAnalysisService** - Motor principal de análisis de fraude
- ✅ **TransactionDayCommands** - Comandos para gestión de totales diarios  
- ✅ **ResponseBuilder** - Constructor de respuestas estandarizadas
- ✅ **DTOs** - Objetos de transferencia de datos

### Casos de prueba incluidos:

#### FraudAnalysisService
- 🔍 Validación de límite individual ($2,500)
- 📊 Validación de límite diario ($20,500) 
- ✅ Transacciones aprobadas (dentro de límites)
- ❌ Transacciones rechazadas (exceden límites)
- 🛡️ Manejo de errores de base de datos
- 📅 Conversión correcta de fechas a UTC medianoche
- 🎯 Escenarios variados con Theory/InlineData

#### TransactionDayCommands
- 📝 Paso correcto de parámetros al repositorio
- 🔢 Manejo de montos cero y negativos
- ⚠️ Propagación de excepciones

#### ResponseBuilder  
- ✅ Respuestas exitosas con y sin datos
- ❌ Respuestas de error con mensajes personalizados
- 🕐 Timestamps UTC correctos
- 📦 Objetos complejos como datos

#### DTOs
- 🏗️ Inicialización correcta de propiedades
- 📊 Manejo de valores por defecto
- 🔄 Diferentes tipos de datos

## 🚀 Ejecución de tests

### Opción 1: PowerShell (Windows)
```powershell
cd tests/Application.Tests
./run-tests-with-coverage.ps1
```

### Opción 2: Bash (Linux/Mac)
```bash
cd tests/Application.Tests
chmod +x run-tests-with-coverage.sh
./run-tests-with-coverage.sh
```

### Opción 3: Comando directo
```bash
# Tests básicos
dotnet test

# Tests con coverage
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings

# Tests con reporte detallado
dotnet test --logger:"console;verbosity=detailed"
```

## 📊 Reportes de cobertura

Los tests generan reportes de cobertura automáticamente:

### Archivos generados:
- `TestResults/` - Archivos XML de cobertura
- `CoverageReport/` - Reporte HTML navegable
- `CoverageReport/index.html` - Punto de entrada del reporte

### Métricas de cobertura objetivo:
- **Line Coverage**: > 90%
- **Branch Coverage**: > 85% 
- **Method Coverage**: > 95%

### Visualización:
```bash
# Abrir reporte en navegador
start CoverageReport/index.html  # Windows
open CoverageReport/index.html   # Mac
xdg-open CoverageReport/index.html # Linux
```

## 🛠️ Herramientas utilizadas

### Frameworks de testing:
- **xUnit** - Framework principal de tests
- **Moq** - Mocking de dependencias
- **FluentAssertions** - Assertions más legibles

### Coverage:
- **Coverlet** - Colección de cobertura multiplataforma
- **ReportGenerator** - Generación de reportes HTML

### Paquetes NuGet:
```xml
<PackageReference Include="xunit" Version="2.6.1" />
<PackageReference Include="Moq" Version="4.20.69" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="coverlet.collector" Version="6.0.0" />
<PackageReference Include="coverlet.msbuild" Version="6.0.0" />
```

## 📝 Estructura de tests

```
Application.Tests/
├── Services/
│   └── FraudAnalysisServiceTests.cs     # Tests del motor de fraude
├── Commands/
│   └── TransactionDayCommandsTests.cs   # Tests de comandos
├── Common/
│   └── ResponseBuilderTests.cs          # Tests del response builder
├── DTOs/
│   └── DTOTests.cs                       # Tests de DTOs
├── coverlet.runsettings                  # Configuración de coverage
├── run-tests-with-coverage.ps1          # Script Windows
├── run-tests-with-coverage.sh           # Script Linux/Mac
└── Application.Tests.csproj             # Proyecto de tests
```

## 📋 Patrones de testing utilizados

### Arrange-Act-Assert (AAA)
```csharp
[Fact]
public async Task AnalyzeTransaction_ShouldReject_WhenExceedsLimit()
{
    // Arrange
    var transaction = CreateTransactionEvent(value: 3000m);
    
    // Act  
    var result = await _service.AnalyzeTransactionAsync(transaction);
    
    // Assert
    result.IsApproved.Should().BeFalse();
}
```

### Theory con InlineData
```csharp
[Theory]
[InlineData(2499.99, true)]  // Dentro del límite
[InlineData(2500.01, false)] // Excede el límite
public async Task AnalyzeTransaction_VariousAmounts(decimal amount, bool expected)
{
    // Test parametrizado
}
```

### Mocking con Moq
```csharp
_repositoryMock.Setup(x => x.GetDailyTotalAsync(It.IsAny<Guid>(), It.IsAny<DateTime>()))
    .ReturnsAsync(1000m);

_repositoryMock.Verify(x => x.UpsertTransactionDayAsync(It.IsAny<DateTime>(), It.IsAny<Guid>(), It.IsAny<decimal>()), 
    Times.Once);
```

## 🎯 Casos de test importantes

### Límites críticos:
- Transacción exactamente en $2,500 (límite individual)
- Total diario exactamente en $20,500 (límite diario)
- Valores edge case (0.01, máximos, negativos)

### Manejo de errores:
- Excepciones de base de datos
- Datos nulos o inválidos
- Timeouts y fallos de conexión

### Logging y auditoría:
- Verificación de logs de error
- Timestamps correctos
- Propagación de excepciones

## 🚀 Integración continua

Los tests están configurados para ejecutarse en CI/CD:

```yaml
# Ejemplo GitHub Actions
- name: Run tests with coverage
  run: |
    cd tests/Application.Tests
    dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings
    
- name: Upload coverage reports
  uses: codecov/codecov-action@v3
  with:
    files: ./tests/Application.Tests/TestResults/**/coverage.cobertura.xml
```

## 📈 Métricas de calidad

### Objetivos de cobertura por componente:
- **FraudAnalysisService**: 100% (crítico para negocio)
- **TransactionDayCommands**: 95%
- **ResponseBuilder**: 100% (utilidad común)
- **DTOs**: 90% (objetos simples)

### KPIs de testing:
- Tests ejecutados: Todos los tests pasan ✅
- Tiempo de ejecución: < 30 segundos
- Cobertura total: > 90%
- Mutación testing: Opcional para componentes críticos

---

💡 **Tip**: Ejecuta `dotnet watch test` para testing continuo durante desarrollo.