# 📋 Plan de Implementación: FamiliaSync Grocery (Versión Final)

Este documento es la hoja de ruta completa para el sistema de listas de compras. Incluye la gestión de familias, sincronización offline y optimización para gama baja.

---

## 🏗️ FASE 1: Base de Datos y Estructura de Identidad (PostgreSQL + EF Core)

- [x] **Esquema de Familias:**
    - [x] Tabla `Families`: `Id` (Guid), `Name` (String), `InviteCode` (String, único, 6-8 caracteres), `CreatedAt` (DateTime).
- [x] **Esquema de Usuarios:**
    - [x] Tabla `Users`: `Id`, `Username`, `PasswordHash`, `FamilyId` (Guid, Nullable al inicio).
    - [x] Lógica de `CurrentDeviceId` (Guid) para control de sesión única.
- [x] **Esquema de Listas:**
    - [x] Tabla `ShoppingLists`: `Id`, `Name`, `CreatedAt`, `IsActive` (bool), `FamilyId` (Relación 1:N con Families).
- [x] **Esquema de Productos:**
    - [x] Tabla `Products`: `Id`, `Name`, `Quantity`, `Status` (Enum: Pending, InCart, Paid).
    - [x] Relaciones: `ListId`, `UpdatedAt` (Timestamp), `LastModifiedByUserId`.
- [x] **Esquema de Dispositivos:**
    - [x] Tabla `UserDevices`: Mapeo de `UserId` con `FCM_Token`.

---

## ⚙️ FASE 2: Back-end - Gestión de Familias y Auth (C# .NET 8)

- [ ] **Autenticación Base:**
    - [ ] Endpoint de Registro/Login de usuario individual.
    - [ ] Middleware de validación de JWT con validación de `DeviceId`.
- [ ] **Gestión de Onboarding Familiar:**
    - [ ] Generador de códigos de invitación (Lógica para crear strings aleatorios únicos de 6 caracteres).
    - [ ] Endpoint `POST /families/create`: Crea la familia y asigna automáticamente al creador.
    - [ ] Endpoint `POST /families/join`: Valida el código de invitación y vincula al usuario con la familia.
- [ ] **API de Listas y Productos:**
    - [ ] Endpoint `GET /products/suggestions`: Sugerencias basadas en `FamilyId`.
    - [ ] Endpoint `POST /sync`: Conciliación de cambios offline (validando siempre que el `ListId` pertenezca al `FamilyId` del usuario).
- [ ] **Comunicación e Infraestructura:**
    - [ ] `ShoppingListHub` (SignalR): Unión de usuarios a grupos nombrados como `family-{FamilyId}`.
    - [ ] Integración `FirebaseAdmin` para Notificaciones Push.

---

## 📱 FASE 3: App Móvil - Persistencia y Lógica Local (Flutter + Drift)

- [ ] **Base de Datos Local (Drift):**
    - [ ] Definir tablas espejo: `LocalFamilies`, `LocalProducts`, `LocalLists`.
    - [ ] Tabla `SyncOutbox`: Para encolar cambios realizados sin conexión.
- [ ] **Lógica de Onboarding en Flutter:**
    - [ ] Pantalla de Selección: "¿Eres nuevo o ya tienes un código?".
    - [ ] Formulario de "Crear Familia" o "Ingresar Código".
    - [ ] Guardado persistente del `FamilyId` en `SecureStorage`.
- [ ] **Motor de Sincronización:**
    - [ ] `ConnectivityWatcher` para detectar recuperación de internet.
    - [ ] Procesador de `SyncOutbox` para vaciar cola de cambios hacia el API.
- [ ] **Cliente SignalR:**
    - [ ] Conexión persistente con `withAutomaticReconnect`.
    - [ ] Listener de eventos para actualizar Drift local ante cambios externos.

---

## 🎨 FASE 4: UI/UX y Feedback "Antifallos" para Familiares

- [ ] **Navegación y Estados:**
    - [ ] Layout de 3 pestañas: **Pendientes** (Amarillo), **En el Carrito** (Verde), **Pagados** (Azul/Gris).
    - [ ] Botón de "Nueva Lista" (solo si el usuario ya pertenece a una familia).
- [ ] **Entrada de Datos Simplificada:**
    - [ ] Widget `Autocomplete` para nombres (fuente: Drift local).
    - [ ] Gestos: Tap simple para avanzar de estado, Long Press para opciones extra.
- [ ] **Feedback Crítico (Cabos Sueltos):**
    - [ ] **Indicador de Red:** Icono de nube (Verde: Online / Rojo: Modo Local).
    - [ ] **Botón Undo (Deshacer):** SnackBar temporal tras mover un producto.
    - [ ] **Validación de Conflictos:** Si SignalR reporta un cambio en un ítem mientras el usuario lo ve, actualizar con animación suave.
    - [ ] **Código de Invitación:** Pantalla en "Ajustes" que muestre el código de la familia actual para compartirlo fácilmente.

---

## 🚀 FASE 5: Despliegue y DevOps (Docker + Cloud)

- [ ] **Dockerización:**
    - [ ] `Dockerfile` para .NET 8.
    - [ ] `docker-compose.yml` (API + Postgres + Nginx).
- [ ] **Servidor:**
    - [ ] Despliegue en VM (Oracle/AWS).
    - [ ] Proxy Inverso Nginx con soporte para WebSockets (Upgrade header).
    - [ ] Certificado SSL (Let's Encrypt) para seguridad de datos familiares.