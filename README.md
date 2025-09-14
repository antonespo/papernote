# Papernote App

PaperNote è un'applicazione web per creare, gestire e condividere note testuali con altri utenti, con funzioni di ricerca e organizzazione tramite tag.

## Architettura e Componenti Principali

PaperNote implementa un'architettura a microservizi per garantire scalabilità e manutenibilità.

### Schema Architetturale

```
    [Utente Browser]
           |
    [Frontend Angular SPA]
           | HTTPS
    [API Gateway YARP]
         /         \
   [Auth Service]  [Notes Service]
        |                   |
[PostgreSQL Auth]  [PostgreSQL Notes]
        |                   |
        └─ [Redis Cache] ───┘
```

### Panoramica del Sistema

Il sistema funziona con questo flusso:

1. **Frontend Angular** - Interfaccia utente single-page application
2. **Gateway YARP** - Entry point che inoltra le richieste
3. **Auth Service** - Gestisce registrazione, login e JWT tokens
4. **Notes Service** - Gestisce CRUD note, condivisione e ricerca FTS
5. **Database separati** - Ogni servizio ha il proprio PostgreSQL database
6. **Redis** - Cache condivisa per performance e sessioni

### Microservizi

#### **API Gateway (YARP)**

- **Ruolo**: Entry point unico per sicurezza e routing
- **Tecnologie**: .NET 8, YARP Reverse Proxy
- **Funzionalità**: HTTPS termination, CORS, security headers

#### **Auth Service**

- **Ruolo**: Gestione utenti e autenticazione
- **Database**: PostgreSQL (`papernote_auth`)
- **Funzionalità**: Register/login, JWT tokens, password hashing Argon2id
- **API**: Pubbliche (auth) + interne (risoluzione username per Notes)

#### **Notes Service**

- **Ruolo**: Gestione note collaborative
- **Database**: PostgreSQL (`papernote_notes`)
- **Funzionalità**: CRUD note, condivisione, tag, full-text search
- **Sicurezza**: Tutti endpoint protetti da JWT Bearer

### Database e Cache

#### **SQL Database: PostgreSQL (2 database separati)**

- **`papernote_auth`**: Utenti, credenziali, refresh tokens
- **`papernote_notes`**: Note, condivisioni, tag
- **Full-Text Search**: PostgresSQL tsvector e indici GIN per ricerche testuali veloci
- **Vantaggi**: Isolamento dati, scaling indipendente, autonomia servizi

#### **NoSQL Database: Redis Cache**

- **Note Cache**: Cache singole note e liste per performance (anche dei risultati di ricerca)
- **Rate Limiting**: Protezione tentativi login (non per IP ma per username)
- **Token Blacklist**: JWT revocati per logout immediato (jti blacklist)
- **User Resolution**: Cache username <=> UserID per comunicazione inter-service

### Clean Architecture

Ogni microservizio usa Clean Architecture con 3 layer:

```
┌─────────────────┐
│   API Layer     │  Controllers, Middleware
├─────────────────┤
│Infrastructure   │  Database, Cache, HTTP
├─────────────────┤
│  Core/Domain    │  Business Logic, Entities
└─────────────────┘
```

**Benefici**: Testabilità, manutenibilità, indipendenza da framework esterni.

### Frontend (Angular 18)

- **Architettura**: SPA con lazy loading modules
- **API Client**: Autogenerato da specifiche OpenAPI backend
- **Sicurezza**: JWT interceptor, route guards, type safety TypeScript
- **Struttura**: Core (guards), Features (auth/notes), Shared (components)

### Sicurezza

#### Autenticazione

- **JWT**: HMAC-SHA256 tokens con refresh rotation
- **Password**: Hashing Argon2id con salt
- **Sessions**: Blacklist JTI per logout immediato

#### Gateway Security

- **HTTPS**: Enforcement con security headers
- **CORS**: Configurazione restrictive per origins
- **Headers**: CSP, X-Frame-Options, HSTS

## Eseguire App in Locale

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

## Miglioramenti Futuri

Roadmap di evoluzione per trasformare PaperNote in una soluzione enterprise-grade.

### Robustezza API e UX

- **Input Validation**: Schema validation rigorosa con FluentValidation
- **Hyperlink Validation**: Validazione esistenza e sicurezza link esterni
- **Optimistic Locking**: Gestione conflitti edit simultaneo note con ETag/versioning
- **Real-time Updates**: SignalR per notifiche modifiche in tempo reale
- **Paginazione Infinita**: Infinite scroll per migliorare UX su liste lunghe
- **Cache Resilienza**: Redis è comune ad auth e notes, graceful degradation se Redis non disponibile

### Testing e Quality Engineering

- **Unit Tests**: Coverage >90% per business logic
- **Integration Tests**: API testing con database reale
- **End-to-End Tests**: Playwright per user journey completi
- **Performance Tests**: Load testing con K6 o NBomber
- **Static Analysis**: SonarQube per code quality metrics
- **Dependency Scanning**: Automated vulnerability assessment

### Monitoring e Osservabilità

- **Distributed Tracing**: OpenTelemetry completo con Jaeger
- **Metrics Collection**: Prometheus + Grafana dashboards
- **Error Tracking**: Sentry per exception monitoring
- **Performance KPIs**: Response time, throughput, error rates

### Sicurezza Enterprise

- **OAuth2/OIDC**: Integration con identity providers (Azure AD, Google)
- **Certificate Management**: Automated cert rotation
- **Secrets Management**: Azure Key Vault per gestione credenziali
- **Role-Based Access Control**: Ruoli e permessi granulari
- **Audit Logging**: Compliance tracking per azioni sensibili
- **API Rate Limiting**: Advanced throttling per endpoint specifici
- **Network Security**: VNet isolation, private endpoints

### Deployment Cloud-Native (possibile architettura)

- **Container Apps**: Serverless containers con auto-scaling
- **Azure Database**: PostgreSQL Flexible Server con backup automatici
- **Redis Cache**: Azure Cache for Redis con clustering
- **API Management**: Centralized API gateway con policies
- **Infrastructure as Code**: Terraform per automation e versioning
- **Feature Flags**: Deployment graduale nuove funzionalità
- **Blue-Green Deployment**: Zero-downtime deployments
- **Canary Releases**: Gradual rollout con monitoring automatico
- **Disaster Recovery**: Cross-region backup e failover
