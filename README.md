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

- **Development**: docker-compose.dev.yml (attualmente in uso)
- **CI**: docker/compose.ci.yml
- **Test**: docker/compose.test.yml
- **Production**: docker/compose.prod.yml
