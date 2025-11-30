# 🚀 Deploy DamslaApi a Render

## ✅ Cambios Aplicados

Tu API está **lista para producción** con:

- ✔ **CORS AllowAll** configurado
- ✔ **Variable de entorno CONN_STR** para PostgreSQL
- ✔ **Dockerfile optimizado** para .NET 9
- ✔ **Swagger habilitado** en producción
- ✔ **RoleClaimType** configurado para JWT

---

## 📋 PASO 1 — Subir a GitHub

```bash
git add .
git commit -m "Ready for Render deployment"
git push origin main
```

---

## 🌐 PASO 2 — Crear Web Service en Render

1. Ve a https://dashboard.render.com
2. Clic en **New → Web Service**
3. Conecta tu repositorio **frayderMM/sla_api**
4. Configura:

| Opción | Valor |
|--------|-------|
| **Name** | `dam-sla-api` |
| **Environment** | `Docker` |
| **Region** | `Oregon (US West)` |
| **Branch** | `main` |
| **Instance Type** | `Free` |

---

## 🔐 PASO 3 — Variables de Entorno

En Render → **Environment Variables**, agrega:

```
CONN_STR=Host=xxxx.amazonaws.com;Port=5432;Database=TCS-XSA;Username=postgres;Password=xxxx;SSL Mode=Require;

JWT__Key=tu_clave_secreta_super_segura

ASPNETCORE_ENVIRONMENT=Production
```

⚠️ **IMPORTANTE:** Usa tu cadena de conexión AWS Aurora exacta.

---

## 🎯 PASO 4 — Deploy

1. Clic en **Create Web Service**
2. Render construirá tu Docker image automáticamente
3. Espera 3-5 minutos mientras despliega

---

## 🧪 PASO 5 — Probar la API

Tu API estará disponible en:

```
https://dam-sla-api.onrender.com
```

### Swagger UI
```
https://dam-sla-api.onrender.com/swagger
```

### Test con PowerShell
```powershell
$login = @{
    email = "fray@gmail.com"
    password = "fray"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "https://dam-sla-api.onrender.com/api/Auth/login" -Method POST -ContentType "application/json" -Body $login

Write-Host "Token: $($response.token)"
```

---

## 📱 PASO 6 — Conectar Android

En tu código Retrofit:

```kotlin
private const val BASE_URL = "https://dam-sla-api.onrender.com/"
```

---

## 🔄 Actualizaciones Futuras

Cada vez que hagas `git push origin main`, Render re-desplegará automáticamente.

---

## ⚠️ Notas Importantes

- **Free tier:** La API se dormirá después de 15 min de inactividad
- **Primera llamada:** Puede tardar 30-60 segundos en despertar
- **SSL:** Render proporciona HTTPS automáticamente
- **Logs:** Visibles en el dashboard de Render

---

## 📞 Endpoints Principales

```
POST   /api/Auth/login
GET    /api/solicitudes
POST   /api/solicitudes (requiere rol: analista)
PUT    /api/solicitudes/{id} (requiere rol: analista)
DELETE /api/solicitudes/{id} (requiere rol: analista)
GET    /api/sla/metrics_glob
GET    /api/sla/metricsxsla
GET    /api/tiposSla
```

---

🎉 **¡Listo! Tu API está en producción.**
