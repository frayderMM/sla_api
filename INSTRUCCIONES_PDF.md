# 📄 MÓDULO DE REPORTES PDF - INSTRUCCIONES

## ✅ FASE 6 – GENERACIÓN DE REPORTE PDF PROFESIONAL (TCS + ESAN)

### 📦 Paquetes Instalados
- **iText7** v9.4.0 (librería profesional para PDF en .NET)

### 📁 Archivos Creados

#### 1. **Dtos/Sla/FiltroReporteDto.cs**
DTO opcional para filtrar reportes (por rol o mes).

#### 2. **Services/PdfService.cs**
Servicio principal que genera el PDF completo con:
- **Logos institucionales** (ESAN + TCS)
- **Título corporativo** del reporte
- **KPIs resumidos por rol**:
  - Total solicitudes
  - Cumplen SLA
  - No cumplen SLA
  - % de cumplimiento
- **Tabla detallada** de todas las solicitudes SLA con:
  - ID
  - Rol
  - Tipo SLA
  - Fecha solicitud
  - Fecha ingreso
  - Resultado (cumple/no cumple)
- **Pie de página institucional**

#### 3. **Controllers/ReportesController.cs**
Endpoint REST para descargar el PDF:
```
GET /api/reportes/pdf
```
- **Autorización**: Requiere JWT (roles `general` o `analista`)
- **Respuesta**: Archivo PDF descargable (`Reporte_SLA.pdf`)

#### 4. **wwwroot/logos/**
Carpeta creada para almacenar los logos:
- `esan.png` (logo Universidad ESAN)
- `tcs.png` (logo TCS)

⚠️ **IMPORTANTE**: Debes copiar manualmente los archivos de imagen PNG a esta carpeta.

### 🔧 Configuración Realizada

El servicio `PdfService` fue registrado en `Program.cs`:
```csharp
builder.Services.AddScoped<PdfService>();
```

### 🧪 Cómo Probar desde Swagger

1. **Iniciar la aplicación**:
   ```bash
   dotnet run
   ```

2. **Autenticarse**:
   - Ir a `POST /api/auth/login`
   - Usar credenciales (ejemplo):
     ```json
     {
       "email": "analista@tcs.com",
       "password": "123456"
     }
     ```
   - Copiar el `token` de la respuesta

3. **Autorizar en Swagger**:
   - Click en el botón **"Authorize"** (🔓)
   - Ingresar: `Bearer {token}`
   - Click en **"Authorize"** y luego **"Close"**

4. **Descargar el PDF**:
   - Ir a `GET /api/reportes/pdf`
   - Click en **"Try it out"** → **"Execute"**
   - Swagger descargará el archivo `Reporte_SLA.pdf`

### 📱 Integración con Android

El endpoint está listo para ser consumido desde tu app móvil Flutter:

```dart
// Ejemplo de integración
Future<void> descargarReportePDF() async {
  final response = await http.get(
    Uri.parse('https://tu-api.com/api/reportes/pdf'),
    headers: {
      'Authorization': 'Bearer $token',
    },
  );
  
  if (response.statusCode == 200) {
    // Guardar o mostrar el PDF
    final bytes = response.bodyBytes;
    // Usar paquete como flutter_pdfview o pdf_viewer
  }
}
```

### 🎨 Características del PDF Generado

✅ **Encabezado profesional**:
   - Logo ESAN (izquierda)
   - Logo TCS (derecha)
   - Título institucional centrado

✅ **Sección de KPIs**:
   - Tabla con 5 columnas
   - Resumen agrupado por rol
   - Porcentajes de cumplimiento

✅ **Tabla de detalle**:
   - Todas las solicitudes SLA
   - 6 columnas de información
   - Formato corporativo

✅ **Footer**:
   - Texto institucional
   - Generado automáticamente

### 🔐 Seguridad

- **JWT requerido**: Solo usuarios autenticados pueden generar reportes
- **Roles permitidos**: `general` y `analista`
- **Datos dinámicos**: Se consultan en tiempo real desde Aurora PostgreSQL

### ⚠️ Pendientes

1. **Copiar logos**:
   - Coloca `esan.png` y `tcs.png` en `wwwroot/logos/`
   - Si no tienes las imágenes, puedes:
     - Descargarlas de internet
     - Crear placeholders temporales
     - Solicitar los logos oficiales

2. **Configuración de base de datos**:
   - Actualizar password en `appsettings.json`
   - Ejecutar script SQL en Aurora (`Data/setup-database.sql`)
   - Crear migraciones EF Core:
     ```bash
     dotnet ef migrations add InitialCreate
     dotnet ef database update
     ```

### 🚀 Próximas Mejoras Opcionales

- Agregar filtros por fecha o rol
- Incluir gráficos de cumplimiento
- Exportar a Excel además de PDF
- Agregar firma digital al PDF
- Personalizar colores según KPIs (semáforo)

---

## 📊 Estado del Proyecto

**COMPLETADO**:
✅ Fase 1: Estructura del proyecto
✅ Fase 2: Autenticación JWT + Login/Registro
✅ Fase 3: Módulo SLA (cálculos, indicadores, KPIs)
✅ Fase 4: CRUD de Solicitudes
✅ Fase 5: Carga masiva desde Excel
✅ **Fase 6: Generación de reportes PDF profesionales**

**PENDIENTES**:
- RolesController (CRUD de roles)
- EmailService (notificaciones)
- Módulo de predicción/regresión

---

**Generado por**: DAMSLApi - Sistema de gestión SLA TCS–ESAN
**Fecha**: 2025
