# Papernote App

PaperNote è un'applicazione web per creare, gestire e condividere note testuali con altri utenti, con funzioni di ricerca e organizzazione tramite tag.

## Obiettivi Realizzati

- **Gestione completa del ciclo di vita delle note (CRUD)** - create, Read, Update, Delete con soft delete
- **Sistema di condivisione tra utenti** - in fase di creazione o modifica della nota tramite username
- **Sistema di tagging delle note** - organizzazione tramite tag personalizzati
- **Ricerca full-text con PostgreSQL** - ricerca avanzata su titolo e contenuto con tsvector
- **Funzionalità avanzate di caching** - Redis per rate limiting, token blacklist, input validation
- **Health-check per ogni servizio** - per monitoraggio stato applicazione e dipendenze
- **Implementazione UI completa** - con gestione errori e validazioni client-side
- **Supporto ad hyperlink con sanitizzazione (lato UI)**
- **Autenticazione JWT con refresh token**
- **Architettura microservizi scalabile**
- **Pipeline CI/CD automatizzate** - GitHub Actions
- **Documentazione API tramite Swagger** - OpenAPI specs disponibili nel repository e Swagger endpoints
- **Tool di supporto per testing** - Postman collection con scenario automatico e possibilità di testare tutte le funzionalità
- **Uso di Git** - Feature branch e PR verso main con check enforcement

## Architettura e Componenti Principali

PaperNote implementa un'architettura a microservizi per garantire scalabilità e manutenibilità.

### Schema Architetturale (brutto da vedere ma efficace)

```
    [Utente Browser]
           |
    [Frontend Angular SPA]
           | HTTPS
    [API Gateway YARP]
         /            \
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
- **Funzionalità**: HTTPS enforcement, CORS, security headers

#### **Auth Service**

- **Ruolo**: Gestione utenti e autenticazione
- **Database**: PostgreSQL dedicato (`papernote_auth`)
- **Funzionalità**: Register/login, JWT tokens, password hashing Argon2id
- **API**: Pubbliche (quelle per auth) + interne (risoluzione username per Notes)

#### **Notes Service**

- **Ruolo**: Gestione note collaborative
- **Database**: PostgreSQL dedicato (`papernote_notes`)
- **Funzionalità**: CRUD note, condivisione, tag, full-text search
- **Sicurezza**: tutti endpoint protetti da JWT Bearer

### Database e Cache

#### **SQL Database: PostgreSQL (2 database separati)**

- **`papernote_auth`**: Utenti, credenziali, refresh tokens
- **`papernote_notes`**: Note, condivisioni, tag
- **Full-Text Search**: PostgresSQL tsvector e indici GIN per ricerche testuali veloci (su db notes)
- **Vantaggi**: isolamento dati, scaling indipendente, autonomia servizi

#### **NoSQL Database: Redis Cache**

Scopo:

1. **Note Cache**: Cache singole note e liste per performance (anche dei risultati di ricerca)
2. **Rate Limiting**: Protezione tentativi login (non per IP ma per username)
3. **Token Blacklist**: JWT revocati per logout immediato (jti blacklist)
4. **User Resolution cache**: Cache username <=> UserID per comunicazione inter-service

### Clean Architecture

Ogni microservizio usa Clean Architecture con 3 layer per separare responsabilità, testabilità e indipendenza:

```
┌─────────────────┐
│   API Layer     │  Controllers, Middleware
├─────────────────┤
│Infrastructure   │  Database, Cache, HTTP
├─────────────────┤
│  Core/Domain    │  Business Logic, Entities
└─────────────────┘
```

### Frontend (Angular 18)

- **Architettura**: SPA con lazy loading, senza NgModule
- **API Client**: autogenerato da specifiche OpenAPI dei servizi backend (script in cartella `/frontend/scripts`)
- **Aspetti chiave implementati (oltre a tutte le funzionalità)**:
  - JWT Authentication con automatic token refresh
  - Sanitizzazione hyperlink in service Angular dedicato
  - Angular Material per UI consistency enterprise
  - Reactive forms (fortemente tipizzati) con validation patterns
  - Route guards per authorization enforcement
  - HTTP interceptors per cross-cutting concerns
  - Signal-based state management per performance

### Sicurezza

#### Autenticazione

- **JWT**: HMAC-SHA256 tokens con refresh rotation
- **Password**: Hashing Argon2id con salt
- **Sessions**: Blacklist JTI per logout immediato (su Redis)

#### Gateway Security

- **HTTPS**: Enforcement con security headers
- **CORS**: Configurazione restrictive per origins
- **Headers**: CSP, X-Frame-Options, HSTS

## Eseguire App in Locale

### 1. Prerequisiti

#### Per esecuzione con Docker (Raccomandato)

- Git
- Docker con account

#### Per esecuzione manuale (senza Docker)

- Git
- .NET 8 SDK (https://dotnet.microsoft.com/it-it/download/dotnet/8.0)
- Entity Framework Core CLI (per db migrations): da terminale `dotnet tool install --global dotnet-ef`
- Node.js v20.19.5 (https://nodejs.org/en/download)
- Docker per istanze di PostgreSQL 16 e Redis 7

### 2. Clone repository

Clone del Repository in una cartella locale e tramite terminale:

```bash
git clone https://github.com/antonespo/papernote.git
cd papernote
```

### 3. Avviare l'applicazione

#### 3.1 - Opzione 1: Docker Compose (Raccomandato) - un comando per tutto

Avvia tutto lo stack con un comando (include database migration):

```bash
cd docker
docker-compose -f compose.dev.yml up --build
```

#### 3.2 - Opzione 2: Esecuzione Manuale

##### 3.2.1 - Avvia PostgreSQL e Redis con Docker

```bash
cd docker
docker-compose -f compose.dev.yml up postgres redis -d
```

##### 3.2.2 - Applica Migrations Database (Ricorda di installare Entity Framework come descritto nei prerequisiti)

```bash
cd backend/Papernote

# Migrations per Auth Service (database papernote_auth)
dotnet ef database update --project Papernote.Auth.Infrastructure --startup-project Papernote.Auth.API

# Migrations per Notes Service (database papernote)
dotnet ef database update --project Papernote.Notes.Infrastructure --startup-project Papernote.Notes.API
```

##### 3.2.3 - Avvia Backend Services

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

Terminal 4 - Frontend:

```bash
cd frontend
npm install
npm start
```

## Endpoints e Servizi

### URL Servizi

| Servizio          | DOCKER                | LOCALE                 |
| ----------------- | --------------------- | ---------------------- |
| **Gateway**       | http://localhost:5005 | https://localhost:7000 |
| **Auth Service**  | http://localhost:5003 | https://localhost:7001 |
| **Notes Service** | http://localhost:5004 | https://localhost:7002 |
| **Frontend**      | http://localhost:4200 | http://localhost:4200  |

### Database e Cache

| Servizio       | URL            |
| -------------- | -------------- |
| **PostgreSQL** | localhost:5432 |
| **Redis**      | localhost:6379 |

### Health Check Endpoints

| Servizio    | DOCKER                       | LOCALE                        |
| ----------- | ---------------------------- | ----------------------------- |
| **Gateway** | http://localhost:5005/health | https://localhost:7000/health |
| **Auth**    | http://localhost:5003/health | https://localhost:7001/health |
| **Notes**   | http://localhost:5004/health | https://localhost:7002/health |

### API Endpoints

#### Auth Service

**Base URL**: http://localhost:5003/api/v1/auth (Docker) | https://localhost:7001/api/v1/auth (locale)

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

**Base URL**: http://localhost:5004/api/v1/notes (Docker) | https://localhost:7002/api/v1/notes (locale)

| Endpoint | Method | Autenticazione | Descrizione                                               |
| -------- | ------ | -------------- | --------------------------------------------------------- |
| `/`      | GET    | JWT Bearer     | Lista note con filtri (owned/shared, ricerca testo, tags) |
| `/`      | POST   | JWT Bearer     | Creazione nuova nota                                      |
| `/{id}`  | GET    | JWT Bearer     | Dettaglio nota specifica                                  |
| `/{id}`  | PUT    | JWT Bearer     | Modifica nota esistente                                   |
| `/{id}`  | DELETE | JWT Bearer     | Eliminazione nota in soft delete                          |

### Documentazione API Completa con Swagger

| Servizio      | Swagger UI DOCKER      | Swagger UI LOCALE       |
| ------------- | ---------------------- | ----------------------- |
| **Auth API**  | http://localhost:5003/ | https://localhost:7001/ |
| **Notes API** | http://localhost:5004/ | https://localhost:7002/ |

**Specifiche OpenAPI**: Disponibili in `/api-specs/auth-api.json` e `/api-specs/notes-api.json`

## Testing con Postman

Nella cartella `/postman` sono disponibili:

- **`PaperNote.API.postman_collection.json`**: Collezione completa con tutti gli endpoint e scenario di test automatizzato (3 utenti, 13 note con condivisioni)
- **`PaperNote.Docker.postman_environment.json`**: Environment per setup Docker (HTTP, porte 5003-5005)
- **`PaperNote.Local.postman_environment.json`**: Environment per esecuzione locale (HTTPS, porte 7000-7002)

### Utenti di Test

Lo scenario automatico crea i seguenti utenti:

| Username     | Password    |
| ------------ | ----------- |
| **pippo**    | Pippo123    |
| **pluto**    | Pluto123    |
| **paperino** | Paperino123 |

### Utilizzo

Per utilizzare: importa la collezione e l'environment appropriato in Postman, quindi esegui lo scenario "Setup - Scenario Completo" per popolare automaticamente il sistema con tutti i dati di test inclusi gli utenti sopra indicati.

## Miglioramenti Futuri

Roadmap di evoluzione per trasformare PaperNote in una soluzione enterprise-grade.

### Robustezza API e User Experience

- **Input Validation più forte**: validazioni avanzate con FluentValidation
- **Hyperlink Validation anche lato BE**: validazione esistenza e sicurezza link esterni
- **Locking edit concorrenti**: gestione conflitti edit simultaneo note con ETag/versioning
- **Real-time Updates**: SignalR per notifiche modifiche in tempo reale
- **Paginazione Infinita**: infinite scroll per migliorare UX su liste lunghe
- **Resilienza Cache**: Redis è comune ad auth e notes, graceful degradation se Redis non disponibile

### Testing e Quality

- **Unit Tests**: coverage >80% per business logic
- **Integration Tests**: API testing con database reale
- **End-to-End Tests**: Playwright per user journey completi
- **Performance Tests**: load testing con K6 o NBomber
- **Static Analysis**: SonarQube per code quality metrics
- **Dependency Scanning**: automated vulnerability assessment

### Monitoring e Osservabilità

- **Distributed Tracing**: OpenTelemetry completo con Jaeger
- **Metrics Collection**: Prometheus + Grafana dashboards
- **Error Tracking**: Sentry per exception monitoring
- **Performance KPIs**: response time, throughput, error rates

### Sicurezza Enterprise

- **OAuth2**: integration con identity providers (Azure AD, Google)
- **Certificate Management**: automated cert rotation
- **Secrets Management**: Azure Key Vault per gestione credenziali
- **Role-Based Access Control**: ruoli e permessi granulari
- **Audit Logging**: compliance tracking per azioni sensibili
- **Network Security**: VNet isolation, private endpoints

### Cloud-Native Deployment (possibile architettura)

- **Container Apps**: Serverless containers con auto-scaling
- **Azure Database**: PostgreSQL Flexible Server con backup automatici
- **Redis Cache**: Azure Cache for Redis con clustering
- **API Management**: centralized API gateway con policies
- **Infrastructure as Code**: Terraform per automation e versioning
- **Feature Flags**: deployment graduale nuove funzionalità
- **Blue-Green Deployment**: zero-downtime deployments
- **Canary Releases**: gradual rollout con monitoring automatico
- **Disaster Recovery**: cross-region backup e failover
