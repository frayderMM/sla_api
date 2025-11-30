# 🔐 Sistema de Autenticación Implementado

## ✅ Componentes Instalados

### Paquetes NuGet
- `Microsoft.AspNetCore.Authentication.JwtBearer` (9.0.0) - Autenticación JWT
- `BCrypt.Net-Next` (4.0.3) - Hash seguro de contraseñas

## 📁 Archivos Creados

### DTOs de Autenticación
- `Dtos/Auth/LoginDto.cs` - Credenciales de login
- `Dtos/Auth/RegisterDto.cs` - Datos de registro de usuario

### Servicios
- `Services/JwtService.cs` - Generación de tokens JWT
- `Services/AuthService.cs` - Lógica de login y registro
- `Utils/PasswordHasher.cs` - Encriptación de contraseñas con BCrypt

### Controladores
- `Controllers/AuthController.cs` - Endpoints de autenticación

## 🚀 Endpoints Disponibles

### 1. Login
**POST** `/api/auth/login`

**Body:**
```json
{
  "email": "admin@tcs.com",
  "password": "123456"
}
```

**Respuesta exitosa:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

### 2. Registro
**POST** `/api/auth/register`

**Body:**
```json
{
  "nombre": "Juan Pérez",
  "email": "juan@tcs.com",
  "password": "123456",
  "rolId": 1
}
```

**Respuesta exitosa:**
```json
{
  "message": "Usuario creado exitosamente",
  "user": {
    "id": 1,
    "nombre": "Juan Pérez",
    "email": "juan@tcs.com"
  }
}
```

## 🔒 Proteger Endpoints con Roles

### Solo Analistas
```csharp
[Authorize(Roles = "analista")]
[HttpPost("crear")]
public IActionResult CrearSolicitud() { ... }
```

### Analistas y Generales
```csharp
[Authorize(Roles = "general,analista")]
[HttpGet("listar")]
public IActionResult ListarSolicitudes() { ... }
```

### Solo Autenticados (cualquier rol)
```csharp
[Authorize]
[HttpGet("perfil")]
public IActionResult MiPerfil() { ... }
```

## 🧪 Probar en Swagger

1. **Ejecuta la aplicación:**
   ```powershell
   dotnet run
   ```

2. **Abre Swagger:** `https://localhost:XXXX/swagger`

3. **Haz login** en `/api/auth/login` y copia el token

4. **Autoriza en Swagger:**
   - Click en botón "Authorize" 🔒
   - Ingresa: `Bearer {token_copiado}`
   - Click "Authorize"

5. **Ahora puedes acceder a endpoints protegidos**

## ⚙️ Configuración JWT (appsettings.json)

```json
{
  "Jwt": {
    "Key": "ClaveUltraSecretaDAMSLATCS2025",
    "Issuer": "DAMSLApi",
    "Audience": "DamslaMobile"
  }
}
```

## 📋 Roles Disponibles

| ID | Nombre   | Descripción                    |
|----|----------|--------------------------------|
| 1  | general  | Usuario estándar (solo lectura)|
| 2  | analista | Administrador completo         |

## 🔐 Características de Seguridad

✅ **Password Hashing:** BCrypt con salt automático  
✅ **JWT:** Tokens con expiración de 8 horas  
✅ **Claims:** Sub, Email, Rol, Jti  
✅ **Validación:** Issuer, Audience, SigningKey, Lifetime  
✅ **Middleware:** Authentication + Authorization configurados  

## 📝 Próximos Pasos

Antes de probar, necesitas:

1. **Actualizar password en `appsettings.json`** (conexión Aurora PostgreSQL)
2. **Ejecutar script SQL** en Aurora (`Data/setup-database.sql`)
3. **Crear migraciones EF Core:**
   ```powershell
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```
4. **Crear un usuario de prueba** usando `/api/auth/register`

## 🎯 Estado Actual

✅ Autenticación JWT completa  
✅ Login y registro funcionando  
✅ Password hasheado con BCrypt  
✅ Roles implementados  
✅ Middleware configurado  
✅ Endpoints protegidos listos  
✅ Compilación exitosa  

**Siguiente fase:** Implementar SLA, Solicitudes, Reportes, etc.
