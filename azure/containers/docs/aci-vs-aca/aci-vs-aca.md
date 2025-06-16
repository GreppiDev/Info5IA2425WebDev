# Deployment semplificato di applicazioni distribuite su Azure - Azure Container Instances (ACI) e Azure Container Apps (ACA)

- [Deployment semplificato di applicazioni distribuite su Azure - Azure Container Instances (ACI) e Azure Container Apps (ACA)](#deployment-semplificato-di-applicazioni-distribuite-su-azure---azure-container-instances-aci-e-azure-container-apps-aca)
  - [Azure Container Instances (ACI) 🌪️](#azure-container-instances-aci-️)
  - [Azure Container Apps (ACA) 🚀](#azure-container-apps-aca-)
  - [Tabella Comparativa Chiave 📊](#tabella-comparativa-chiave-)
  - [Vantaggi dell'Approccio PaaS di Azure per Alta Disponibilità, Scalabilità e Gestione Semplificata ✨](#vantaggi-dellapproccio-paas-di-azure-per-alta-disponibilità-scalabilità-e-gestione-semplificata-)
  - [Quale Scegliere per un Progetto Specifico? 🤔](#quale-scegliere-per-un-progetto-specifico-)

Questa guida è pensata per aiutare a comprendere le differenze tra Azure Container Instances (ACI) e Azure Container Apps (ACA). Entrambi i servizi consentono di eseguire container Docker in Azure senza la necessità di gestire macchine virtuali sottostanti, ma si rivolgono a scenari d'uso diversi.

## Azure Container Instances (ACI) 🌪️

- **Cos'è:** ACI rappresenta il modo più rapido e semplice per eseguire un singolo container o un piccolo gruppo di container co-locati in Azure. Offre container isolati on-demand.

- **Caratteristiche Principali:**

    - Avvio istantaneo dei container.

    - Fatturazione al secondo per vCPU e memoria.

    - Nessuna gestione di cluster o orchestratori.

    - Supporto per gruppi multi-container (simili a un pod Kubernetes semplificato) che condividono risorse (rete, storage, ciclo di vita).

    - Possibilità di montare volumi persistenti (Azure Files).

    - Esposizione diretta tramite IP pubblico.

- **Casi d'Uso Principali:**

    - Task di build e test in pipeline CI/CD.

    - Esecuzione di processi batch o attività pianificate.

    - Sviluppo e test rapido di applicazioni containerizzate.

    - Applicazioni semplici o microservizi che non richiedono scalabilità avanzata o un orchestratore completo.

    - Bursting da orchestratori come AKS (tramite Virtual Kubelet).

## Azure Container Apps (ACA) 🚀

- **Cos'è:** ACA è un servizio serverless completamente gestito, costruito su Kubernetes, che semplifica la distribuzione di microservizi e applicazioni containerizzate. Offre funzionalità di orchestrazione più ricche rispetto ad ACI, astraendo la complessità di Kubernetes.

- **Caratteristiche Principali:**

    - Scalabilità dinamica basata su traffico HTTP, eventi (tramite KEDA) o metriche CPU/memoria (inclusa la scalabilità a zero).

        - **Scalabilità a Zero (Scale-to-Zero) 💸:** Questa funzionalità permette di ridurre il numero di repliche di un'app container fino a zero quando non c'è traffico o attività.

            - **Vantaggi:** Il principale vantaggio è il **risparmio sui costi**, poiché non vengono addebitati costi per CPU e memoria quando non ci sono istanze attive.

            - **Svantaggi (Cold Start) 🥶:** Quando una richiesta arriva a un'applicazione che è scalata a zero, si verifica un "cold start". Ciò significa che ACA deve avviare una nuova istanza del container, il che include il provisioning della replica, il pull dell'immagine del container (se non è già in cache sul nodo), l'avvio del container e l'inizializzazione dell'applicazione al suo interno. Questo processo può introdurre una **latenza** per la prima richiesta.

            - **Quando usarla:** Ideale per ambienti di sviluppo/test, applicazioni con traffico molto sporadico o imprevedibile, e processi in background che possono tollerare una latenza iniziale.

            - **Impostazione:** Si abilita impostando il numero minimo di repliche (`min-replicas`) a `0`. Impostandolo a `1` o più, si garantisce che almeno un'istanza sia sempre in esecuzione, evitando i cold start ma sostenendo i costi continui per tale istanza.

    - Gestione integrata dell'ingress HTTPS con domini personalizzati e certificati TLS/SSL (gestiti da Azure o propri).

    - Service discovery interno e DNS tra le app container all'interno di un "Ambiente Container Apps".

    - Gestione delle revisioni per implementare aggiornamenti sicuri (es. blue/green).

    - Integrazione con Dapr (Distributed Application Runtime) per la creazione di microservizi resilienti.

    - Networking virtuale integrato per l'isolamento e la connessione ad altre risorse Azure.

    - Gestione sicura dei segreti e delle configurazioni.

    - Monitoraggio e logging integrati con Azure Monitor (Log Analytics).

- **Casi d'Uso Principali:**

    - Hosting di API web e microservizi.

    - Applicazioni web con frontend e backend.

    - Elaborazione di eventi e processi in background.

    - Modernizzazione di applicazioni esistenti in container.

    - Scenari che richiedono scalabilità automatica e resilienza senza la complessità della gestione di un cluster Kubernetes completo.

## Tabella Comparativa Chiave 📊

| **Caratteristica** | **Azure Container Instances (ACI)** | **Azure Container Apps (ACA)** |
| --- |  --- |  --- |
| **Livello di Astrazione** | Container come Servizio (CaaS) | Piattaforma Applicativa Serverless per Container (basata su K8s) |
| **Complessità Gestione** | Molto Bassa | Bassa/Media (K8s è astratto) |
| **Orchestrazione** | Minima (gruppi di container) | Avanzata (scalabilità, revisioni, ingress, Dapr) |
| **Scalabilità** | Manuale | Automatica (HTTP, KEDA, CPU/Memoria), scale-to-zero |
| **Networking** | IP pubblico per gruppo | Ingress HTTPS gestito, VNet, service discovery interna |
| **Dominio Personalizzato** | Manuale (tramite IP e DNS) | Integrato, con gestione certificati |
| **Gestione Revisioni** | No (sostituzione del gruppo) | Sì (supporto per deployment canary/blue-green) |
| **Service Discovery** | Limitata all'interno del gruppo | Integrata all'interno dell'ambiente ACA |
| **Integrazione Dapr** | No | Sì |
| **Ideale per** | Task semplici, build, test, app isolate | Applicazioni web, API, microservizi, workload scalabili |

## Vantaggi dell'Approccio PaaS di Azure per Alta Disponibilità, Scalabilità e Gestione Semplificata ✨

Prima di decidere quale servizio specifico utilizzare, è utile comprendere i benefici intrinseci dell'adozione di una piattaforma come servizio (PaaS) offerta da cloud provider come Azure (con logiche simili applicabili ad AWS con servizi come Elastic Beanstalk o Fargate, e Google Cloud con Cloud Run o App Engine). Azure Container Apps, in particolare, incarna molti di questi vantaggi PaaS per le applicazioni containerizzate.

1. **Load Balancer Gestito: Alta Disponibilità e Scalabilità Automatiche**

    - Quando si espone un'applicazione tramite Azure Container Apps, il servizio include un **bilanciatore del carico (load balancer) completamente gestito**.

    - **Alta Disponibilità (HA):** Non è necessario configurare o gestire manualmente l'HA del load balancer. Azure garantisce che l'infrastruttura di bilanciamento del carico sia resiliente ai guasti, mantenendo l'applicazione accessibile.

    - **Scalabilità Automatica:** Il load balancer scala automaticamente la sua capacità per gestire le fluttuazioni del traffico in ingresso verso le app container. Man mano che le istanze dell'applicazione scalano orizzontalmente, il load balancer distribuisce efficientemente le richieste.

    - **Semplificazione:** Questo elimina la complessità e l'onere operativo associati alla configurazione, al patching, al monitoraggio e alla scalabilità di un'infrastruttura di bilanciamento del carico autonoma.

2. **Mitigazione DDoS Integrata**

    - **Protezione di Base:** Tutte le risorse pubbliche di Azure, inclusi gli endpoint di ingresso di Azure Container Apps, beneficiano automaticamente di **Azure DDoS Protection Basic** senza costi aggiuntivi. Questa protezione di base è efficace contro gli attacchi DDoS volumetrici più comuni a livello di infrastruttura di rete.

    - **Protezione Avanzata (Opzionale):** Per una mitigazione DDoS più sofisticata e per funzionalità di Web Application Firewall (WAF), Azure offre servizi come **Azure Application Gateway (con WAF SKU)** o **Azure Front Door**. Questi possono essere integrati con Azure Container Apps per fornire livelli di sicurezza superiori, sebbene introducano una configurazione aggiuntiva.

    - **Vantaggio:** La protezione DDoS di base gestita dalla piattaforma riduce il rischio e la complessità della difesa contro attacchi comuni, un compito che sarebbe oneroso da gestire autonomamente.

3. **Database Gestiti nel Cloud: Semplificare HA e Scalabilità del Backend**

    - Come discusso in precedenza, implementare e gestire autonomamente l'alta disponibilità e la scalabilità per un database (ad esempio, MariaDB tramite Galera Cluster o configurazioni Master-Slave complesse) è un compito arduo e dispendioso in termini di tempo e competenze.

    - **Azure Database for MariaDB (e servizi simili per altri database):** Utilizzare un servizio di database PaaS come Azure Database for MariaDB trasferisce la responsabilità di HA, scalabilità, backup, patching e manutenzione generale del database ad Azure.

        - **HA e Failover Automatici:** Il servizio gestisce la ridondanza e il failover automatico in caso di problemi.

        - **Scalabilità Semplificata:** È possibile scalare le risorse del database (CPU, memoria, storage) con pochi clic o comandi, e spesso sono disponibili opzioni per repliche di lettura per scaricare il carico.

        - **Backup Gestiti:** I backup automatici e il ripristino point-in-time sono funzionalità standard.

    - **Impatto sul Codice Applicativo:** L'applicazione si connette al database gestito tramite una semplice stringa di connessione, senza dover implementare logiche complesse per la gestione del cluster, il failover o lo splitting read/write (a meno che non si scelga attivamente di farlo per ottimizzazioni specifiche).

    - **Focus sullo Sviluppo:** Questo approccio permette agli sviluppatori di concentrarsi sulla logica applicativa e sulle funzionalità, piuttosto che sulla complessa infrastruttura del database.

In conclusione, scegliere un approccio PaaS come quello offerto da Azure Container Apps e dai servizi di database gestiti di Azure permette di delegare gran parte della complessità operativa legata all'alta disponibilità, alla scalabilità e alla sicurezza dell'infrastruttura sottostante. Ciò si traduce in cicli di sviluppo più rapidi, minori costi operativi (in termini di tempo e risorse umane) e la possibilità di concentrarsi sul valore aggiunto dell'applicazione stessa.

## Quale Scegliere per un Progetto Specifico? 🤔

Per un progetto che consiste in una `webapp` ASP.NET Core e un database `MariaDB`, con la necessità di gestire segreti, persistenza dei dati, e potenzialmente un dominio personalizzato e autenticazione per una demo, **Azure Container Apps (ACA) si presenta come la soluzione più appropriata e versatile.**

**Motivazioni:**

1. **Gestione dei Servizi:** ACA permette di definire la `webapp` e `MariaDB` come due app container distinte ma interconnesse all'interno di un ambiente sicuro, facilitando la comunicazione interna.

2. **Ingress e Dominio Personalizzato:** La gestione integrata dell'ingress HTTPS e dei domini personalizzati semplifica notevolmente l'esposizione della webapp.

3. **Scalabilità e Resilienza:** Anche se per una demo la scalabilità estrema non è il focus, le funzionalità di ACA (come lo scale-to-zero) possono essere vantaggiose e didatticamente interessanti.

4. **Gestione dei Segreti:** ACA offre un sistema robusto per la gestione dei segreti.

5. **Esperienza Moderna:** Fornisce un'esperienza più vicina alle moderne pratiche di deployment di applicazioni cloud native senza la complessità della gestione di un cluster Kubernetes completo.
