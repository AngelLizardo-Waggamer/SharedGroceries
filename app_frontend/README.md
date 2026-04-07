# SharedGroceries Mobile Frontend

## English

### 1. Project Overview
This repository contains the mobile frontend for SharedGroceries, built with Flutter.

The app is designed to support shared grocery management workflows for families or groups, including:
- Authentication and session handling.
- Family and profile features.
- Shopping list and product management.
- Real-time updates through SignalR.
- Local persistence for selected data.

This README is intentionally written at a stable, high level so it remains useful as the codebase evolves.

### 2. Tech Stack (High Level)
- Framework: Flutter (Dart)
- State management / dependency injection: Provider
- Networking: Dio
- Real-time communication: SignalR client
- Secure storage: flutter_secure_storage
- Local database: Drift (SQLite)

### 3. High-Level Architecture
The codebase follows a layered structure under `lib/`.

- `API/`: HTTP and real-time communication clients.
- `Auth/`: session handling, token lifecycle, and recovery flows.
- `Repositories/`: business-facing data access layer.
- `Database/`: local database schema and data access objects.
- `Views/`: UI screens grouped by feature.
- `Routes/`: centralized app routing.
- `Theme/`: app theming and visual styles.
- `main.dart`: app composition and dependency wiring.

At startup, the app initializes core services (session manager, API client, real-time client, repositories, database), provides them globally, and selects the initial route based on authentication state.

### 4. Prerequisites
Before running the project, make sure you have:
- Flutter SDK installed.
- A compatible Dart SDK (managed through Flutter).
- Android Studio and/or VS Code with Flutter tooling.
- At least one emulator/device configured.

To verify your environment:

```bash
flutter doctor
```

### 5. Getting Started
From the project root:

```bash
flutter pub get
flutter run
```

Useful development commands:

```bash
flutter analyze
flutter test
flutter clean
```

### 6. Configuration Notes
- API configuration is currently defined in `lib/config.dart`.
- Environment-specific values (local/dev/staging/prod) can be introduced gradually as the project grows.
- Avoid hardcoding secrets in source code; use secure approaches for sensitive data.

### 7. Generated Files and Codegen
This project uses Drift and build_runner tooling. When schema or annotated sources change, regenerate files as needed:

```bash
dart run build_runner build --delete-conflicting-outputs
```

### 8. Project Structure Reference
Current key directories (simplified):

```text
lib/
	API/
	Auth/
	Database/
	Repositories/
	Routes/
	Theme/
	Views/
	config.dart
	main.dart
assets/
	images/
android/
```

### 9. Development Guidelines (Base)
- Keep feature logic in repositories/services, not directly in views.
- Prefer small, testable units and clear interfaces.
- Keep route names and navigation flows centralized.
- Maintain consistency with existing lint rules and project conventions.
- Document non-obvious architectural decisions in code comments or ADR-style notes.

### 10. Testing and Quality
Recommended baseline quality checks:
- Static analysis with `flutter analyze`.
- Unit/widget tests for critical flows.
- Manual validation on at least one real device before release.

### 11. Build and Release (General)
For Android APK build:

```bash
flutter build apk
```

For Android App Bundle:

```bash
flutter build appbundle
```

For iOS (on macOS environment):

```bash
flutter build ios
```
---

## Español

### 1. Resumen del Proyecto
Este repositorio contiene el frontend móvil de SharedGroceries, desarrollado con Flutter.

La aplicación está pensada para soportar flujos de gestión de compras compartidas para familias o grupos, incluyendo:
- Autenticación y manejo de sesión.
- Funcionalidades de familia y perfil.
- Gestión de listas de compras y productos.
- Actualizaciones en tiempo real mediante SignalR.
- Persistencia local para datos seleccionados.

Este README está escrito intencionalmente en un nivel alto y estable para que siga siendo útil conforme evolucione el código.

### 2. Stack Tecnológico (Nivel General)
- Framework: Flutter (Dart)
- Gestión de estado / inyección de dependencias: Provider
- Networking: Dio
- Comunicación en tiempo real: cliente SignalR
- Almacenamiento seguro: flutter_secure_storage
- Base de datos local: Drift (SQLite)

### 3. Arquitectura General
El codigo sigue una estructura por capas bajo `lib/`.

- `API/`: clientes HTTP y de comunicacion en tiempo real.
- `Auth/`: manejo de sesion, ciclo de vida de tokens y flujos de recuperacion.
- `Repositories/`: capa de acceso a datos orientada al dominio.
- `Database/`: esquema de base de datos local y objetos de acceso a datos.
- `Views/`: pantallas UI agrupadas por funcionalidad.
- `Routes/`: enrutamiento centralizado de la aplicacion.
- `Theme/`: tema y estilos visuales.
- `main.dart`: composicion de la app e inyeccion de dependencias.

Durante el inicio, la app inicializa los servicios principales (session manager, API client, cliente en tiempo real, repositorios y base de datos), los expone de forma global y selecciona la ruta inicial según el estado de autenticacion.

### 4. Prerrequisitos
Antes de ejecutar el proyecto, asegúrate de tener:
- Flutter SDK instalado.
- Un Dart SDK compatible (gestionado por Flutter).
- Android Studio y/o VS Code con herramientas de Flutter.
- Al menos un emulador/dispositivo configurado.

Para validar el entorno:

```bash
flutter doctor
```

### 5. Inicio Rapido
Desde la raiz del proyecto:

```bash
flutter pub get
flutter run
```

Comandos utiles de desarrollo:

```bash
flutter analyze
flutter test
flutter clean
```

### 6. Notas de Configuracion
- La configuracion de API se define actualmente en `lib/config.dart`.
- Se pueden introducir gradualmente valores por ambiente (local/dev/staging/prod) conforme crezca el proyecto.
- Evita hardcodear secretos en el codigo fuente; utiliza estrategias seguras para datos sensibles.

### 7. Archivos Generados y Codegen
Este proyecto usa Drift y build_runner. Cuando cambie el esquema o fuentes anotadas, regenera archivos cuando sea necesario:

```bash
dart run build_runner build --delete-conflicting-outputs
```

### 8. Referencia de Estructura
Directorios principales actuales (simplificado):

```text
lib/
	API/
	Auth/
	Database/
	Repositories/
	Routes/
	Theme/
	Views/
	config.dart
	main.dart
assets/
	images/
android/
```

### 9. Lineamientos de Desarrollo (Base)
- Mantener la logica de funcionalidades en repositorios/servicios, no directamente en vistas.
- Preferir unidades pequenas, testeables y con interfaces claras.
- Centralizar nombres de rutas y flujos de navegacion.
- Mantener consistencia con reglas de lint y convenciones del proyecto.
- Documentar decisiones de arquitectura no obvias con comentarios o notas tipo ADR.

### 10. Pruebas y Calidad
Verificaciones recomendadas de calidad base:
- Analisis estatico con `flutter analyze`.
- Pruebas unitarias/widget para flujos criticos.
- Validacion manual en al menos un dispositivo real antes de publicar.

### 11. Build y Release (General)
Para generar APK Android:

```bash
flutter build apk
```

Para generar Android App Bundle:

```bash
flutter build appbundle
```

Para iOS (en entorno macOS):

```bash
flutter build ios
```
