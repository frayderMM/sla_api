# 📊 MÓDULO DASHBOARD + PREDICCIÓN SLA - INSTRUCCIONES

## ✅ FASE 7 — Dashboard Mensual + Predicción SLA (Regresión Lineal Simple)

### 📁 Archivos Creados

#### 1. **Dtos/Dashboard/DashboardMensualDto.cs**
DTO que representa los datos mensuales de cumplimiento SLA agrupados por rol:
- `Mes`: Formato "MM-YYYY" (ejemplo: "01-2024")
- `Rol`: Nombre del rol
- `Total`: Total de solicitudes del mes
- `Cumplen`: Cantidad que cumplen SLA
- `NoCumplen`: Cantidad que no cumplen SLA
- `Porcentaje`: % de cumplimiento
- `Color`: Indicador semáforo (`green`, `red`, `gray`)

#### 2. **Dtos/Dashboard/PrediccionSlaDto.cs**
DTO con los resultados de la predicción de regresión lineal:
- `Rol`: Rol analizado
- `PromedioMeses`: Promedio histórico de cumplimiento
- `Pendiente`: Coeficiente "m" de la regresión (y = mx + b)
- `Intercepto`: Coeficiente "b" de la regresión
- `PrediccionProximoMes`: % de cumplimiento predicho
- `EstadoEsperado`: Evaluación de riesgo basada en la predicción

#### 3. **Services/DashboardService.cs**
Servicio que genera el dashboard mensual:
- **GetDashboardMensual(year)**: Retorna lista de KPIs mensuales por rol
  - Filtra solicitudes por año
  - Agrupa por mes y rol
  - Calcula totales, cumplimientos y porcentajes
  - Asigna color según cumplimiento:
    - `green`: 100% cumplimiento
    - `gray`: 0% cumplimiento
    - `red`: Cumplimiento parcial

#### 4. **Services/PrediccionService.cs**
Servicio de predicción con regresión lineal simple:
- **Predecir(historico)**: Implementa regresión lineal desde cero
  - **Modelo matemático**: y = m*x + b
    - x = número de mes secuencial (1, 2, 3...)
    - y = porcentaje de cumplimiento
  - **Cálculos**:
    - Pendiente (m): Tasa de cambio del cumplimiento
    - Intercepto (b): Valor base
    - Predicción: Aplica modelo al mes siguiente
  - **Evaluación de riesgo**:
    - ≥80%: "Alta probabilidad de cumplimiento"
    - 60-79%: "Posible riesgo"
    - <60%: "Riesgo alto"
  - **Manejo de datos insuficientes**: Si hay menos de 2 meses, retorna promedio

#### 5. **Controllers/DashboardController.cs**
Controlador REST con dos endpoints:

**GET /api/dashboard/mensual/{year}**
- Retorna dashboard mensual completo del año especificado
- Agrupado por mes y rol
- Requiere autenticación JWT

**GET /api/dashboard/prediccion/{year}/{rol}**
- Retorna predicción SLA para un rol específico
- Usa datos del año especificado
- Aplica regresión lineal simple
- Requiere autenticación JWT

### 🔧 Configuración Realizada

Los servicios fueron registrados en `Program.cs`:
```csharp
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<PrediccionService>();
```

### 🧪 Cómo Probar desde Swagger

#### 1. **Dashboard Mensual**

**Endpoint**: `GET /api/dashboard/mensual/2024`

**Pasos**:
1. Autenticarse en `POST /api/auth/login`
2. Copiar token y autorizar en Swagger
3. Ejecutar endpoint con el año deseado (ejemplo: 2024)

**Ejemplo de Respuesta**:
```json
[
  {
    "mes": "01-2024",
    "rol": "Analista",
    "total": 10,
    "cumplen": 8,
    "noCumplen": 2,
    "porcentaje": 80.0,
    "color": "red"
  },
  {
    "mes": "02-2024",
    "rol": "Analista",
    "total": 15,
    "cumplen": 15,
    "noCumplen": 0,
    "porcentaje": 100.0,
    "color": "green"
  },
  {
    "mes": "03-2024",
    "rol": "Desarrollador",
    "total": 8,
    "cumplen": 6,
    "noCumplen": 2,
    "porcentaje": 75.0,
    "color": "red"
  }
]
```

#### 2. **Predicción SLA**

**Endpoint**: `GET /api/dashboard/prediccion/2024/Analista`

**Pasos**:
1. Autenticarse en `POST /api/auth/login`
2. Copiar token y autorizar en Swagger
3. Ejecutar endpoint con año y nombre del rol (usar `%20` para espacios)

**Ejemplo de Respuesta**:
```json
{
  "rol": "Analista",
  "promedioMeses": 85.5,
  "pendiente": 1.42,
  "intercepto": 75.3,
  "prediccionProximoMes": 87.8,
  "estadoEsperado": "Alta probabilidad de cumplimiento"
}
```

**Interpretación**:
- **Pendiente positiva (1.42)**: El cumplimiento está mejorando mes a mes
- **Predicción 87.8%**: Se espera 87.8% de cumplimiento el próximo mes
- **Estado**: Evaluación automática del riesgo

### 📱 Integración con Android/Flutter

#### Ejemplo: Dashboard Mensual
```dart
Future<List<DashboardMensual>> getDashboard(int year) async {
  final response = await http.get(
    Uri.parse('https://tu-api.com/api/dashboard/mensual/$year'),
    headers: {
      'Authorization': 'Bearer $token',
    },
  );
  
  if (response.statusCode == 200) {
    List jsonData = json.decode(response.body);
    return jsonData.map((json) => DashboardMensual.fromJson(json)).toList();
  }
  throw Exception('Error al cargar dashboard');
}
```

#### Ejemplo: Predicción SLA
```dart
Future<PrediccionSla> getPrediccion(int year, String rol) async {
  final response = await http.get(
    Uri.parse('https://tu-api.com/api/dashboard/prediccion/$year/${Uri.encodeComponent(rol)}'),
    headers: {
      'Authorization': 'Bearer $token',
    },
  );
  
  if (response.statusCode == 200) {
    return PrediccionSla.fromJson(json.decode(response.body));
  }
  throw Exception('Error al cargar predicción');
}
```

### 📊 Casos de Uso en la App Móvil

#### 1. **Gráfico de Líneas**
Usa el dashboard mensual para mostrar evolución del cumplimiento:
```dart
// Convertir datos del dashboard a puntos para gráfica
List<FlSpot> spots = dashboard.asMap().entries.map((entry) {
  return FlSpot(entry.key.toDouble(), entry.value.porcentaje);
}).toList();
```

#### 2. **Tarjetas KPI**
Mostrar resumen del último mes:
```dart
final ultimoMes = dashboard.last;
Card(
  child: Column(
    children: [
      Text('${ultimoMes.mes}'),
      Text('${ultimoMes.porcentaje}%'),
      Icon(
        Icons.circle,
        color: ultimoMes.color == 'green' ? Colors.green : Colors.red,
      ),
    ],
  ),
)
```

#### 3. **Indicador de Predicción**
Mostrar pronóstico con evaluación de riesgo:
```dart
Container(
  color: prediccion.prediccionProximoMes >= 80 
    ? Colors.green.shade100 
    : Colors.orange.shade100,
  child: Column(
    children: [
      Text('Predicción: ${prediccion.prediccionProximoMes}%'),
      Text(prediccion.estadoEsperado),
      Text('Tendencia: ${prediccion.pendiente > 0 ? "↗" : "↘"}'),
    ],
  ),
)
```

### 🎯 Características del Módulo

✅ **Dashboard Mensual**:
- Agrupación automática por mes y rol
- KPIs calculados dinámicamente
- Indicador de color (semáforo)
- Filtro por año
- Ordenamiento cronológico

✅ **Predicción SLA**:
- Regresión lineal implementada desde cero (sin librerías externas)
- Análisis de tendencia (pendiente positiva/negativa)
- Evaluación de riesgo automática
- Manejo de casos con datos insuficientes
- Predicción del próximo mes

✅ **Producción Ready**:
- Integrado con Aurora PostgreSQL
- Autenticación JWT obligatoria
- Endpoints RESTful estándar
- Respuestas JSON optimizadas
- Sin dependencias externas de ML

### 🔐 Seguridad

- **Autenticación**: JWT requerido en todos los endpoints
- **Autorización**: Acceso permitido a roles `general` y `analista`
- **Validación**: Verifica existencia de datos antes de calcular
- **Manejo de errores**: Retorna 404 si no hay datos para el rol

### 📈 Modelo de Regresión Lineal

**Fórmula matemática**:
```
y = m*x + b

Donde:
m = (n*Σ(xy) - Σx*Σy) / (n*Σ(x²) - (Σx)²)
b = (Σy - m*Σx) / n
```

**Variables**:
- `x`: Número de mes secuencial (1, 2, 3, 4...)
- `y`: Porcentaje de cumplimiento SLA del mes
- `m`: Pendiente (tasa de cambio mensual)
- `b`: Intercepto (valor base)
- `n`: Cantidad de meses con datos

**Interpretación**:
- **Pendiente > 0**: Cumplimiento mejorando
- **Pendiente < 0**: Cumplimiento empeorando
- **Pendiente ≈ 0**: Cumplimiento estable

### ⚠️ Consideraciones

1. **Datos Mínimos**: Se requieren al menos 2 meses de datos para generar predicción
2. **Calidad de Datos**: La precisión depende de la cantidad de datos históricos
3. **Modelo Simple**: Es regresión lineal básica, no considera estacionalidad ni factores externos
4. **Uso Recomendado**: Para tendencias generales y alertas tempranas
5. **Mejoras Futuras**: Considerar modelos más complejos (ARIMA, redes neuronales) si se requiere mayor precisión

### 🚀 Extensiones Opcionales

- Agregar filtro por tipo de SLA (SLA1/SLA2)
- Predicción a múltiples meses
- Análisis de estacionalidad
- Comparativa entre roles
- Alertas automáticas por email cuando la predicción es "Riesgo alto"
- Gráficos de dispersión con línea de tendencia
- Exportar predicciones a PDF

---

## 📊 Estado del Proyecto

**COMPLETADO**:
✅ Fase 1: Estructura del proyecto
✅ Fase 2: Autenticación JWT + Login/Registro
✅ Fase 3: Módulo SLA (cálculos, indicadores, KPIs)
✅ Fase 4: CRUD de Solicitudes
✅ Fase 5: Carga masiva desde Excel
✅ Fase 6: Generación de reportes PDF profesionales
✅ **Fase 7: Dashboard Mensual + Predicción SLA (Regresión Lineal)**

**PENDIENTES**:
- RolesController (CRUD de roles)
- EmailService (notificaciones)
- Gráficos avanzados
- Panel web administrativo

---

**Generado por**: DAMSLApi - Sistema de gestión SLA TCS–ESAN
**Fecha**: Noviembre 2025
**Versión**: 1.0
