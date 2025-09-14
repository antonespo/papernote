# Papernote App

PaperNote è un'applicazione web per creare, gestire e condividere note testuali con altri utenti, con funzioni di ricerca e organizzazione tramite tag.

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

## Sviluppo Locale

### Prerequisiti

- .NET 8 SDK (https://dotnet.microsoft.com/it-it/download/dotnet/8.0)
- Node.js v20.19.5 (https://nodejs.org/en/download)
- Docker
- PostgreSQL 16 (opzionale, fornito via Docker)
- Redis 7 (opzionale, fornito via Docker)

### Installazione e Setup

#### 1. Clone del Repository

```bash
git clone https://github.com/antonespo/papernote.git
cd papernote
```

#### Opzione 1: Docker Compose (Raccomandato)

Avvia tutto lo stack con un comando:

```bash
cd docker
docker-compose -f compose.dev.yml up --build
```

#### Opzione 2: Esecuzione Manuale

##### 1. Avvia Database e Cache

```bash
cd docker
docker-compose -f compose.dev.yml up postgres redis -d
```

##### 2. Avvia Backend Services

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

##### 3. Avvia Frontend

```bash
cd frontend
npm start
```
