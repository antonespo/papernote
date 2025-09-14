# Papernote App

PaperNote è un'applicazione web per creare, gestire e condividere note testuali con altri utenti, con funzioni di ricerca e organizzazione tramite tag.

## Esecuzione in Locale

### Prerequisiti

#### Per esecuzione con Docker (Raccomandato)

- **Docker** e **Docker Compose**

#### Per esecuzione manuale (senza Docker)

- .NET 8 SDK (https://dotnet.microsoft.com/it-it/download/dotnet/8.0)
- Entity Framework Core CLI (per db migrations): `dotnet tool install --global dotnet-ef`
- Node.js v20.19.5 (https://nodejs.org/en/download)
- Docker
- PostgreSQL 16 (tramite Docker)
- Redis 7 (tramite Docker)

### Installazione e Setup

#### 1. Clone del Repository

```bash
git clone https://github.com/antonespo/papernote.git
cd papernote
```

#### Opzione 1: Docker Compose (Raccomandato)

Avvia tutto lo stack con un comando (include database migration):

```bash
cd docker
docker-compose -f compose.dev.yml up --build
```

#### Opzione 2: Esecuzione Manuale

##### 1. Avvia PostgreSQL e Redis con Docker

```bash
cd docker
docker-compose -f compose.dev.yml up postgres redis -d
```

##### 2. Applica Migrations Database (Ricorda di installare Entity Framework come descritto nei prerequisiti)

```bash
cd backend/Papernote

# Migrations per Auth Service (database papernote_auth)
dotnet ef database update --project Papernote.Auth.Infrastructure --startup-project Papernote.Auth.API

# Migrations per Notes Service (database papernote)
dotnet ef database update --project Papernote.Notes.Infrastructure --startup-project Papernote.Notes.API
```

##### 3. Avvia Backend Services

Terminal 1 - Auth Service:

```bash
cd backend/Papernote
dotnet run --project Papernote.Auth.API --urls "https://localhost:7001"
```

Terminal 2 - Notes Service:

```bash
cd backend/Papernote
dotnet run --project Papernote.Notes.API --urls "https://localhost:7002"
```

Terminal 3 - Gateway:

```bash
cd backend/Papernote
dotnet run --project Papernote.Gateway --urls "https://localhost:7000"
```

##### 4. Avvia Frontend

```bash
cd frontend
npm install
npm start
```

## Endpoints e Servizi

### URL Servizi

| Servizio          | Locale (HTTPS)         | Docker (HTTP)         |
| ----------------- | ---------------------- | --------------------- |
| **Gateway**       | https://localhost:7000 | http://localhost:5005 |
| **Auth Service**  | https://localhost:7001 | http://localhost:5003 |
| **Notes Service** | https://localhost:7002 | http://localhost:5004 |
| **Frontend**      | http://localhost:4200  | http://localhost:4200 |

### Database e Cache

| Servizio       | URL            |
| -------------- | -------------- |
| **PostgreSQL** | localhost:5432 |
| **Redis**      | localhost:6379 |

### Health Check Endpoints

| Servizio    | Locale                        | Docker                       |
| ----------- | ----------------------------- | ---------------------------- |
| **Gateway** | https://localhost:7000/health | http://localhost:5005/health |
| **Auth**    | https://localhost:7001/health | http://localhost:5003/health |
| **Notes**   | https://localhost:7002/health | http://localhost:5004/health |

### API Endpoints

#### Auth Service

**Base URL**: https://localhost:7001/api/v1/auth (locale) | http://localhost:5003/api/v1/auth (Docker)

| Endpoint    | Method | Autenticazione | Descrizione                |
| ----------- | ------ | -------------- | -------------------------- |
| `/register` | POST   | Nessuna        | Registrazione nuovo utente |
| `/login`    | POST   | Nessuna        | Autenticazione utente      |
| `/refresh`  | POST   | JWT Bearer     | Refresh token JWT          |
| `/logout`   | POST   | JWT Bearer     | Logout utente              |

**Internal APIs** (comunicazione tra microservizi):

| Endpoint                                      | Method | Autenticazione           | Descrizione                    |
| --------------------------------------------- | ------ | ------------------------ | ------------------------------ |
| `/api/internal/users/resolve/batch/usernames` | POST   | X-Internal-ApiKey header | Risoluzione username => UserID |
| `/api/internal/users/resolve/batch/userids`   | POST   | X-Internal-ApiKey header | Risoluzione UserID => username |

#### Notes Service

**Base URL**: https://localhost:7002/api/v1/notes (locale) | http://localhost:5004/api/v1/notes (Docker)

| Endpoint | Method | Autenticazione | Descrizione                                               |
| -------- | ------ | -------------- | --------------------------------------------------------- |
| `/`      | GET    | JWT Bearer     | Lista note con filtri (owned/shared, ricerca testo, tags) |
| `/`      | POST   | JWT Bearer     | Creazione nuova nota                                      |
| `/{id}`  | GET    | JWT Bearer     | Dettaglio nota specifica                                  |
| `/{id}`  | PUT    | JWT Bearer     | Modifica nota esistente                                   |
| `/{id}`  | DELETE | JWT Bearer     | Eliminazione nota                                         |

#### Autenticazione

- **JWT Bearer**: `Authorization: Bearer <jwt_token>`
- **Internal Service**: `X-Internal-ApiKey: <service_key>`

### Documentazione API Completa con Swagger

| Servizio      | Swagger UI Locale       | Swagger UI Docker      |
| ------------- | ----------------------- | ---------------------- |
| **Auth API**  | https://localhost:7001/ | http://localhost:5003/ |
| **Notes API** | https://localhost:7002/ | http://localhost:5004/ |

**Specifiche OpenAPI**: Disponibili in `/api-specs/auth-api.json` e `/api-specs/notes-api.json`

## Testing con Postman

Nella cartella `/postman` sono disponibili:

- **`PaperNote.API.postman_collection.json`**: Collezione completa con tutti gli endpoint e scenario di test automatizzato (3 utenti, 13 note con condivisioni)
- **`PaperNote.Docker.postman_environment.json`**: Environment per setup Docker (HTTP, porte 5003-5005)
- **`PaperNote.Local.postman_environment.json`**: Environment per esecuzione locale (HTTPS, porte 7000-7002)

Per utilizzare: importa la collezione e l'environment appropriato in Postman, quindi esegui lo scenario "Setup - Scenario Completo" per popolare automaticamente il sistema con dati di test.

## Approccio DevOps e CI/CD

Il sistema utilizza una struttura DevOps con pipeline separate per diversi ambienti e scopi.

### Pipeline Disponibili (GitHub Actions)

#### 0. Fullstack CI (fullstack-ci.yml)

- **Scopo**: Validazione di base e build completo del sistema
- **Trigger**: Ogni push o pull_request verso main nei percorsi specificati (backend, frontend ..)
- **Fasi**: Build di tutte le componenti

#### 1. CI Pipeline (ci.yml)

- **Scopo**: Validazione completa del codice
- **Trigger**: Manuale (workflow_dispatch) TODO: qualsiasi push o ogni pull_request verso main nei percorsi specificati

- **Fasi**:
  - Lint backend (.NET) e frontend (Angular)
  - Security scanning (CodeQL SAST, Trivy filesystem)
  - Dependency vulnerability check (.NET packages, npm audit)
  - Build backend e frontend
  - Unit tests con coverage
  - Integration tests con Docker Compose
  - Quality gate validation
  - Publish artifacts su GitHub Container Registry

#### 2. Test Deployment (deploy-test.yml)

- **Scopo**: Deploy automatico su ambiente di test (CD)
- **Trigger**: Manuale (workflow_dispatch)
- **Ambiente**: Test persistente con volumi Docker
- **Validazione**: Health checks post-deployment

#### 3. Production Deployment (deploy-prod.yml)

- **Scopo**: Deploy produzione con approvazione manuale
- **Trigger**: Manuale con parametri di input
- **Strategia**: Blue-green deployment
- **Funzionalità**: Backup automatico, rollback, digest pinning

### Ambienti Docker

- **Development**: docker/compose.dev.yml (attualmente in uso)
- **CI**: docker/compose.ci.yml
- **Test**: docker/compose.test.yml
- **Production**: docker/compose.prod.yml
