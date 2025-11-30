# 📋 Guía de Roles y Permisos - API DAMSLA

## 🎯 Estructura de Roles

La API ahora usa **solamente 2 roles** para simplificar la gestión de permisos:

### 1. **Analista** (Acceso Total)
- **RolId:** `1`
- **Nombre:** `analista`
- **Permisos:** Acceso completo a todos los endpoints de la API
- **Funciones:**
  - ✅ Crear, editar y eliminar solicitudes
  - ✅ Generar reportes Excel y PDF
  - ✅ Ver dashboard y estadísticas
  - ✅ Gestionar configuraciones del sistema
  - ✅ Ver logs y alertas
  - ✅ Acceder a todos los endpoints protegidos

### 2. **General** (Solo Lectura)
- **RolId:** `2`
- **Nombre:** `general`
- **Permisos:** Solo lectura de datos básicos
- **Funciones:**
  - ✅ Ver solicitudes (sin editar/eliminar)
  - ✅ Ver dashboard básico
  - ❌ No puede crear/editar/eliminar solicitudes
  - ❌ No puede generar reportes
  - ❌ No puede acceder a configuraciones

---

## 👥 Usuarios Predefinidos

Al iniciar la API por primera vez, se crean automáticamente estos usuarios:

### Analista TCS
```json
{
  "email": "analista@tcs.com",
  "password": "Analista123!",
  "rolId": 1
}
```

### Usuario General
```json
{
  "email": "general@tcs.com",
  "password": "General123!",
  "rolId": 2
}
```

---

## 🔐 Cómo Registrar Usuarios

### Endpoint de Registro
```http
POST http://localhost:5149/api/Auth/register
Content-Type: application/json

{
  "nombre": "Nombre del Usuario",
  "email": "usuario@example.com",
  "password": "Password123!",
  "rolId": 1  // 1 = analista, 2 = general
}
```

### Ejemplos de Registro

#### Registrar un Analista
```json
{
  "nombre": "Juan Pérez",
  "email": "juan.perez@tcs.com",
  "password": "Password123!",
  "rolId": 1
}
```

#### Registrar un Usuario General
```json
{
  "nombre": "María García",
  "email": "maria.garcia@tcs.com",
  "password": "Password123!",
  "rolId": 2
}
```

### ⚠️ Validaciones

- ✅ **RolId válidos:** Solo `1` (analista) o `2` (general)
- ❌ **RolId = 0, 3, 4, etc.:** Error 400 - "El correo ya está registrado o el rol no existe"
- ❌ **Email duplicado:** Error 400 - "El correo ya está registrado o el rol no existe"

---

## 🔑 Cómo Iniciar Sesión

### Endpoint de Login
```http
POST http://localhost:5149/api/Auth/login
Content-Type: application/json

{
  "email": "analista@tcs.com",
  "password": "Analista123!"
}
```

### Respuesta Exitosa
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "usuario": {
    "id": 1,
    "nombre": "Analista TCS",
    "email": "analista@tcs.com",
    "rol": {
      "id": 1,
      "nombre": "analista"
    }
  }
}
```

---

## 🛡️ Usar el Token JWT

Después de iniciar sesión, usa el token en las peticiones protegidas:

```http
GET http://localhost:5149/api/Solicitudes
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### En Swagger UI:
1. Haz clic en el botón **Authorize** (🔓)
2. Ingresa: `Bearer TU_TOKEN_AQUI`
3. Haz clic en **Authorize**
4. Ahora todas las peticiones incluirán el token automáticamente

---

## 📊 Endpoints por Rol

### Acceso Público (No requiere autenticación)
- `POST /api/Auth/register` - Registrar usuario
- `POST /api/Auth/login` - Iniciar sesión

### Acceso General (rolId = 2)
- `GET /api/Solicitudes` - Ver solicitudes
- `GET /api/Dashboard` - Ver dashboard básico

### Acceso Analista (rolId = 1)
Todos los endpoints anteriores **PLUS:**
- `POST /api/Solicitudes` - Crear solicitud
- `PUT /api/Solicitudes/{id}` - Editar solicitud
- `DELETE /api/Solicitudes/{id}` - Eliminar solicitud
- `GET /api/Export/excel/{year}` - Exportar Excel
- `GET /api/Pdf/{year}` - Exportar PDF
- `GET /api/Alertas` - Ver alertas
- Todos los demás endpoints protegidos

---

## 🧪 Probar en Swagger

### Paso 1: Iniciar la API
```bash
cd DamslaApi
dotnet run
```

### Paso 2: Abrir Swagger
Navega a: http://localhost:5149/swagger

### Paso 3: Login como Analista
1. Expande `POST /api/Auth/login`
2. Clic en **Try it out**
3. Pega:
   ```json
   {
     "email": "analista@tcs.com",
     "password": "Analista123!"
   }
   ```
4. Clic en **Execute**
5. **Copia el token** de la respuesta

### Paso 4: Autorizar en Swagger
1. Clic en el botón **Authorize** arriba a la derecha
2. Ingresa: `Bearer TU_TOKEN_COPIADO`
3. Clic en **Authorize**

### Paso 5: Probar Endpoints
Ahora puedes probar cualquier endpoint protegido

---

## ❓ Preguntas Frecuentes

### ¿Por qué solo 2 roles?
Para simplificar la administración. El rol **analista** tiene acceso completo y el rol **general** es para usuarios de solo lectura.

### ¿Puedo crear más roles?
Actualmente no. El sistema está diseñado para usar solo `analista` y `general`.

### ¿Qué pasa si uso rolId = 0?
Recibirás un error 400: "El correo ya está registrado o el rol no existe."

### ¿Cómo cambio el rol de un usuario existente?
Actualmente no hay endpoint para esto. Deberías actualizar directamente en la base de datos o implementar un endpoint de administración.

### ¿Los usuarios predefinidos se crean siempre?
No, solo se crean la **primera vez** que inicias la API si la base de datos está vacía.

---

## 📝 Notas Importantes

1. **Los IDs de roles son fijos:**
   - `1` = analista
   - `2` = general

2. **No existen los roles:**
   - ~~admin~~ (eliminado)
   - ~~supervisor~~ (eliminado)

3. **Si intentas registrar con rolId inválido:**
   - Recibirás error 400
   - La validación se hace en `AuthService.Register()`

4. **Base de datos limpia:**
   - La base de datos fue recreada para tener solo 2 roles
   - Los usuarios antiguos (admin, supervisor) fueron eliminados

---

## 🚀 ¡Listo!

Tu API ahora funciona con **solo 2 roles**:
- ✅ **Analista** (acceso total)
- ✅ **General** (solo lectura)

Para cualquier duda, revisa los logs de la API o usa Swagger para explorar los endpoints disponibles.
