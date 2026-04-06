# Shared Groceries
[![app-ci](https://github.com/AngelLizardo-Waggamer/SharedGroceries/actions/workflows/app-ci.yml/badge.svg)](https://github.com/AngelLizardo-Waggamer/SharedGroceries/actions/workflows/app-ci.yml)

[![backend-ci](https://github.com/AngelLizardo-Waggamer/SharedGroceries/actions/workflows/backend-ci.yml/badge.svg)](https://github.com/AngelLizardo-Waggamer/SharedGroceries/actions/workflows/backend-ci.yml)

[![backend-cd](https://github.com/AngelLizardo-Waggamer/SharedGroceries/actions/workflows/backend-cd.yml/badge.svg)](https://github.com/AngelLizardo-Waggamer/SharedGroceries/actions/workflows/backend-cd.yml)

## English
The reason for this project to be born was the non existing sync between the groceries lists made by my family.
This monorepo contains the back-end used for the project and the Flutter front-end. 

**Note:** My family does not speak english as a main language, neither do I, so the UI will be in Spanish and maybe some of the comments will contain spanish fragments, which I intend to minimize. The main code of the *back-end* will be written in english.

### Backend CI/CD (Azure AKS + Terraform + Helm)

Continuous deployment applies only to the backend (`BackSharedGroceries`).

#### Added Structure
- `Infra/`: Terraform configuration to create Resource Group, ACR, and AKS.
- `BackSharedGroceries/deploy/helm/backsharedgroceries/`: Backend Helm chart.
- `.github/workflows/terraform-bootstrap-state.yml`: Bootstrap for remote state storage.
- `.github/workflows/terraform-apply.yml`: Infrastructure creation/update.
- `.github/workflows/terraform-destroy.yml`: Manual infrastructure destruction.
- `.github/workflows/backend-cd.yml`: Build, test, image push, and deployment with Helm.

#### GitHub Requirements (Secrets)
Configure the following secrets in the repository:
- `AZURE_CLIENT_ID`
- `AZURE_TENANT_ID`
- `AZURE_SUBSCRIPTION_ID`
- `POSTGRES_ADMIN_PASSWORD`
- `JWT_KEY`
- `JWT_ISSUER`
- `JWT_AUDIENCE`

#### GitHub Requirements (Repository Variables)
Configure the following variables:
- `TF_STATE_RESOURCE_GROUP`
- `TF_STATE_STORAGE_ACCOUNT`
- `TF_STATE_CONTAINER`

#### Recommended Usage Order
1. Manually run `terraform-bootstrap-state` (only once per project).
2. Manually run `terraform-apply` (environment: `dev`).
3. Push changes to `master` (backend-related) or manually run `backend-cd`.
4. After finishing the project, manually run `terraform-destroy` with confirmation `DESTROY`.

#### Zero Downtime Deployment
The Helm chart uses `RollingUpdate` with:
- `maxUnavailable: 0`
- `maxSurge: 1`

Additionally, health probes (`/health`) are used, and deployment runs with `helm upgrade --install --atomic --wait`, ensuring the previous version is only removed once the new one is ready.

#### Public Ingress from First Deployment
The `backend-cd` workflow installs/updates `ingress-nginx` in the cluster and deploys an `Ingress` enabled by default for the backend. This allows the application to receive public internet traffic from the very first deployment via the public IP of the `ingress-nginx-controller` service.

#### PostgreSQL Managed by Terraform
The infrastructure also provisions an `Azure Database for PostgreSQL Flexible Server` along with a database for the application.

The app’s connection string is generated as a sensitive Terraform output (`db_connection_string`), and the CD workflow automatically injects it into the Helm chart during each deployment.

## Spanish
La razón de existir de este proyecto es la nula sincronización entre las listas de compras que mi familia hace cuando toca ir al super.
Este monorepositorio contiene el *back-end* del proyecto y el *front-end* en Flutter. 

**Nota:** Mi familia no habla inglés como su primer idioma, y yo tampoco, por lo que la interfaz de usuario va a contener el texto en español y quizá algunos comentarios en el código también tengan partes en español, aunque pretendo minimizarlas. El código principal del *back-end* va a estar en inglés.

### Backend CI/CD (Azure AKS + Terraform + Helm)

El despliegue continuo aplica solo para el backend (`BackSharedGroceries`).

#### Estructura agregada
- `Infra/`: Terraform para crear Resource Group, ACR y AKS.
- `BackSharedGroceries/deploy/helm/backsharedgroceries/`: Helm chart del backend.
- `.github/workflows/terraform-bootstrap-state.yml`: bootstrap de storage para estado remoto.
- `.github/workflows/terraform-apply.yml`: creación/actualización de infraestructura.
- `.github/workflows/terraform-destroy.yml`: destrucción manual de infraestructura.
- `.github/workflows/backend-cd.yml`: build, test, push de imagen y deploy con Helm.

#### Requisitos de GitHub (Secrets)
Configurar estos secretos en el repositorio:
- `AZURE_CLIENT_ID`
- `AZURE_TENANT_ID`
- `AZURE_SUBSCRIPTION_ID`
- `POSTGRES_ADMIN_PASSWORD`
- `JWT_KEY`
- `JWT_ISSUER`
- `JWT_AUDIENCE`

#### Requisitos de GitHub (Repository Variables)
Configurar estas variables:
- `TF_STATE_RESOURCE_GROUP`
- `TF_STATE_STORAGE_ACCOUNT`
- `TF_STATE_CONTAINER`

#### Orden recomendado de uso
1. Ejecutar manualmente `terraform-bootstrap-state` (una sola vez por proyecto).
2. Ejecutar manualmente `terraform-apply` (ambiente `dev`).
3. Hacer push a `master` con cambios de backend o ejecutar manualmente `backend-cd`.
4. Al terminar prácticas, ejecutar manualmente `terraform-destroy` con confirmación `DESTROY`.

#### Zero downtime deployment
El chart de Helm usa `RollingUpdate` con:
- `maxUnavailable: 0`
- `maxSurge: 1`

Adicionalmente se usan probes de salud (`/health`) y el deploy se ejecuta con `helm upgrade --install --atomic --wait`, para que la versión anterior solo se retire cuando la nueva esté lista.

#### Ingress público desde el primer despliegue
El workflow `backend-cd` instala/actualiza `ingress-nginx` en el clúster y despliega un `Ingress` habilitado por defecto para el backend. Esto permite recibir tráfico público de Internet desde el primer despliegue mediante la IP pública del servicio `ingress-nginx-controller`.

#### PostgreSQL gestionado por Terraform
La infraestructura crea también un `Azure Database for PostgreSQL Flexible Server` y una base de datos para la app.

El `connection string` de la app se genera como output sensible de Terraform (`db_connection_string`) y el workflow de CD lo inyecta automáticamente al chart de Helm en cada despliegue.
