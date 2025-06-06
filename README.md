# Corso di informatica per la classe 5IA a.s. 2024/2025 - Prof. G. Malafronte

## Indice degli argomenti

1. Docker
   1. [Comandi di base per la gestione di container, volumi e networking](./docker/docker-docs/getting-started/index.md)
   2. [Deployment di applicazioni ASP.NET Core con Docker e Docker Compose](./docker/docker-docs/asp-net-docker/index.md)

2. SQL in MySQL e MariaDB
    1. [Elementi di base dell'SQL](./sql/sql-docs/sql-p1/index.md)
    2. [Inserimento dati, SELECT, modifica di una tabella, integrità referenziale](./sql/sql-docs/sql-p2/index.md)
    3. [Vincoli di integrità, indici, il tempo e le date](./sql/sql-docs/sql-p3/index.md)
    4. [DBMS e sicurezza](./sql/sql-docs/sql-p4/index.md)
    5. [Prodotto cartesiano ed operatori dell'SQL](./sql/sql-docs/sql-p5/index.md)
    6. [Vari tipi di JOIN](./sql/sql-docs/sql-p6/index.md)
    7. [Ordinamento, elaborazione, raggruppamento](./sql/sql-docs/sql-p7/index.md)
    8. [Subquery, insiemi, viste, tabelle temporanee](./sql/sql-docs/sql-p8/index.md)
    9. [Argomenti avanzati: Transactions, User Defined Domains, User Defined Variables, Prepared Statements, Triggers, Stored Routines, Cursori](./sql/sql-docs/sql-p9/index.md)
3. Algebra relazionale
   1. [Operatori dell'Algebra Relazionale e confronto con l'SQL](./algebra/index.md)
4. NoSQL Databases
    1. Key-value store in-memory database
       1. [Redis](./nosqldb/redis/index.md)

5. Minimal API ASP.NET Core
    1. [Introduzione alle Minimal API in ASP.NET Core](./asp.net/api-docs/minimal-api/getting-started/index.md)
    2. [Utilizzo dei database in ASP.NET con EF Core](./asp.net/api-docs/minimal-api/use-databases-p1/index.md)
    3. [Minimal API con MariaDB e Microsoft SQL Server: Progetto `AziendaAPI` con EF Core](./asp.net/api-docs/minimal-api/use-databases-p2/index.md)
    4. [Minimal API: OpenAPI Support, Route Handling, Error Handling](./asp.net/api-docs/minimal-api/configure-api-p1/index.md)
    5. [Minimal API: Middlewares, Routing, Filters](./asp.net/api-docs/minimal-api/configure-api-p2/index.md)
    6. [Minimal API: Data Binding e gestione dei form](./asp.net/api-docs/minimal-api/configure-api-p3/index.md)
    7. [Minimal API: Response Types, Service Lifetimes](./asp.net/api-docs/minimal-api/configure-api-p4/index.md)
    8. [Minimal API: Validazione dell'input](./asp.net/api-docs/minimal-api/configure-api-p5/index.md)
    9. [Minimal API: Meccanismi di protezione contro attacchi CSRF](./asp.net/api-docs/minimal-api/configure-api-p6/index.md)
    10. [Minimal API: Caching dei dati - In-memory Caching, Distributed Cache, HybridCache](./asp.net/api-docs/minimal-api/configure-api-p7/index.md)
    11. [Minimal API: Esecuzione di query SQL nel codice ASP.NET mediante EF Core](./asp.net/api-docs/minimal-api/configure-api-p8/index.md)

6. Web Architecture
   1. [Web mechanics](./web/web-docs/web-mechanics/index.md)
   2. [HTTP protocol](./web/web-docs/http-protocol/index.md)
   3. [HTML](./web/web-docs/html/index.md)
   4. [CSS](./web/web-docs/css/index.md)
   5. [JS](./web/web-docs/js/index.md)
   6. [Web forms handling](./web/web-docs/web-forms/index.md)
   7. [JavaScript Web API](./web/web-docs/js-web-api/index.md)
   8. [Architetture Distribuite - Database Distribuiti](./web/web-docs/distributed-systems/databases/scalability-falut-tolerance/index.md)
   9. [Il Ruolo e i Compiti del Database Administrator (DBA)](./web/web-docs/distributed-systems/databases/db-administration/index.md)

7. Frontend Development
   1. [Responsive User Interface](./web/web-docs/responsive-ui/index.md)

8. ASP.NET Configuration and Secrets Management
   1. [ASP.NET Secrets](./asp.net/docs/secrets/index.md)
9. Aspetti di sicurezza nelle Web App
   1. [Sicurezza nelle Web API basata sull'API Key: approcci possibili](./web/web-docs/security/api-key/index.md)
   2. [Sicurezza nelle pagine web: CSRF e XSS](./web/web-docs/security/csrf-xss/index.md)
10. Riconoscimento dell'Utente in Applicazioni Web e Mobile
       1. [Introduzione al riconoscimento dell'utente con i Cookie e protocollo HTTP Stateless](./web/web-docs/user-identity/cookies-basic-concepts/index.md)
       2. [Cookie Based Authentication and Authorization](./web/web-docs/user-identity/cookie-based-authentication-authorization/index.md)
       3. [Pericoli e Vulnerabilità dei Cookie](./web/web-docs/user-identity/cookie-security-issues/index.md)
       4. [Server Side Sessions in ASP.NET Core Minimal APIs](./web/web-docs/user-identity/server-side-session-state-with-cookies/index.md)
       5. [JWT Token Based Authentication and Authorization](./web/web-docs/user-identity/jwt-token-based-authentication-authorization/index.md)
       6. [Analisi dei token JWT in Architetture Stateless](./web/web-docs/user-identity/stateless-token-based-authentication-architecture/index.md)
       7. [Cookie Stateless in Architetture Distribuite](./web/web-docs/user-identity/stateless-cookie-in-distributed-architecture/index.md)
       8. [JWT Token Based Authentication and Authorization with improved Logout](./web/web-docs/user-identity/jwt-token-based-authentication-authorization-with-improved-logout/index.md)

11. Analisi dei requisiti, progettazione concettuale e logica, normalizzazione (in aggiunta rispetto al libro)
    1. [Requirements Analysis and Specification Document (RASD)](./rasd/rasd-doc/index.md)
    2. [Introduzione ai Diagrammi UML dei Casi d'Uso](./rasd/uml/use-cases/index.md)
    3. [Casi d'Uso, Progettazione Concettuale e Logica](./rasd/use-cases-er-requirements/index.md)
    4. [Normalizzazione](./rasd/normalizzazione/index.md)

12. [Cenni ai protocolli OAuth2.0 e OpenID Connect](./web/web-docs/oauth-oidc/index.md)
13. [Cenni al GDPR e Implicazioni per la Gestione di Siti Web](./web/web-docs/gdpr/index.md)
14. Esercitazioni finali e cumulative sull'SQL
    1. [Gestione Biblioteca Universitaria](./sql/sql-docs/sql-cumulative-exercize-1/index.md)
    2. [Gestione E-commerce](./sql/sql-docs/sql-cumulative-exercize-2/index.md)
    3. [Gestione Campionato Sportivo](./sql/sql-docs/sql-cumulative-exercize-3/index.md)
    4. [Considerazioni generali sulla ridondanza e sulle tecniche SQL utilizzate negli esercizi proposti](./sql/sql-docs/sql-cumulative-exercizes-overview/index.md)

15. Verso l'Esame di Stato
    1. [Traccia di informatica del 2017 - sessione ordinaria](./esame/tracce-svolte/2017/ordinaria/index.md)
    2. [Traccia di Informatica del 2023 - sessione ordinaria](./esame/tracce-svolte/2023/ordinaria/index.md)
    3. [Traccia di Informatica del 2023 - sessione suppletiva](./esame/tracce-svolte/2023/suppletiva/index.md)
