# 📋 Plan de Implementación: FamiliaSync Grocery

Este documento contiene la hoja de ruta detallada para el desarrollo del sistema de listas de compras compartidas, optimizado para dispositivos de gama baja y con resiliencia offline.

---

## 🏗️ FASE 1: Base de Datos y Persistencia (PostgreSQL + EF Core)

- [ ] **Esquema de Usuarios:** - [ ] Tabla `Users`: `Id`, `Username`, `PasswordHash`, `FamilyId` (Guid).
    - [ ] Lógica de `CurrentDeviceId` (Guid) para control de sesión única.
- [ ] **Esquema de Listas:** - [ ] Tabla `ShoppingLists`: `Id`, `Name`, `CreatedAt`, `IsActive` (bool), `FamilyId`.
- [ ] **Esquema de Productos:** - [ ] Tabla `Products`: `Id`, `Name` (String), `Quantity` (String), `Status` (Enum: Pending, InCart, Paid).
    - [ ] Relación `ListId`, Auditoría `UpdatedAt` (Timestamp) y `LastModifiedByUserId`.
- [ ] **Esquema de Dispositivos:** - [ ] Tabla `UserDevices`: Mapeo de `UserId` con `FCM_Token` para notificaciones push.
- [ ] **Mantenimiento:** - [ ] Configurar borrado lógico o físico de listas con antigüedad > 30 días.

---

## ⚙️ FASE 2: Back-end y Lógica de Negocio (C# .NET 8)

- [ ] **Autenticación y Seguridad:**
    - [ ] Endpoint de Login: Generación de JWT + Nuevo `DeviceId`.
    - [ ] Middleware de Validación: Comparar `DeviceId` del JWT vs Base de Datos (Error 401 si hay discrepancia).
- [ ] **API de Productos y Sincronización:**
    - [ ] Endpoint `GET /products/suggestions`: Nombres únicos basados en el historial familiar.
    - [ ] Endpoint `POST /sync`: Recibir paquetes de cambios offline y aplicar lógica de reconciliación por Timestamp.
- [ ] **Comunicación Real-time (SignalR):**
    - [ ] Implementar `ShoppingListHub`.
    - [ ] Manejo de Grupos: `Groups.AddToGroupAsync` usando el `FamilyId`.
    - [ ] Difusión selectiva de eventos: `ProductUpdated`, `ProductAdded`, `ProductDeleted`.
- [ ] **Integración con FCM:**
    - [ ] Configurar `FirebaseAdmin` SDK.
    - [ ] Servicio de notificaciones: Enviar push al cambiar estados a "Pagado" (excluyendo al autor).

---

## 📱 FASE 3: App Móvil - Datos y Sync (Flutter + Drift)

- [ ] **Base de Datos Local (Drift/SQLite):**
    - [ ] Definir tablas espejo: `LocalProducts`, `LocalLists`.
    - [ ] Implementar tabla `SyncOutbox` para encolar cambios realizados sin internet.
- [ ] **Motor de Sincronización:**
    - [ ] `ConnectivityWatcher`: Detector de cambio de estado de red.
    - [ ] Procesador de `SyncOutbox`: Envío secuencial de cambios pendientes al API al recuperar conexión.
- [ ] **Cliente SignalR:**
    - [ ] Configuración de `HubConnectionBuilder` con reconexión automática.
    - [ ] Suscripción a eventos del Hub para actualizar la base de datos local Drift en tiempo real.
- [ ] **Persistencia de Sesión:**
    - [ ] Guardado seguro del JWT y `FamilyId` en `flutter_secure_storage`.

---

## 🎨 FASE 4: Interfaz de Usuario (UX) y Feedback Familiar

- [ ] **Navegación por Estados:**
    - [ ] Layout de 3 pestañas: **Pendientes** (Amarillo), **En el Carrito** (Verde), **Pagados** (Gris/Azul).
- [ ] **Entrada de Datos:**
    - [ ] Widget `Autocomplete` para nombres de productos (consumiendo Drift local).
    - [ ] Interacción rápida: Tap simple para avanzar de estado, Long Press para editar.
- [ ] **Sistemas de Feedback (Cabos Sueltos):**
    - [ ] **Indicador de Red:** Icono de nube en el AppBar (Verde = Online, Rojo = Offline).
    - [ ] **Sistema de Deshacer (Undo):** SnackBar con opción de revertir el cambio de estado.
    - [ ] **Bloqueo de Conflictos:** Deshabilitar edición si SignalR notifica que otro usuario ya interactuó con el ítem.
    - [ ] **Notificación de Cierre:** Alerta visual de "¡Lista completada!" cuando no queden pendientes.

---

## 🚀 FASE 5: Infraestructura y Despliegue (DevOps)

- [ ] **Contenedores (Docker):**
    - [ ] `Dockerfile` para la API .NET (Build multi-stage).
    - [ ] `docker-compose.yml`: API + PostgreSQL + Nginx.
- [ ] **Servidor y Red:**
    - [ ] Configurar VM en la nube (Oracle/AWS/GCP).
    - [ ] Configurar Nginx como Reverse Proxy con soporte para WebSockets.
    - [ ] SSL con Certbot (Let's Encrypt) para HTTPS y WSS seguro.
