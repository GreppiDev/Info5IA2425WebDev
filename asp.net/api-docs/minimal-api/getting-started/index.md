# Minimal API in ASP.NET Core

- [Minimal API in ASP.NET Core](#minimal-api-in-aspnet-core)
  - [Introduzione alle API Web](#introduzione-alle-api-web)
  - [Richiamo alle specifiche REST](#richiamo-alle-specifiche-rest)
  - [Creazione di un progetto di Minimal ASP.NET Core](#creazione-di-un-progetto-di-minimal-aspnet-core)
    - [Utilizzo di .NET CLI e VS Code](#utilizzo-di-net-cli-e-vs-code)
      - [Creazione di un progetto di Minimal API ASP.NET Core con il template `web` (progetto web vuoto)](#creazione-di-un-progetto-di-minimal-api-aspnet-core-con-il-template-web-progetto-web-vuoto)
      - [Creazione di un progetto di Minimal API ASP.NET Core con il template `webapi`](#creazione-di-un-progetto-di-minimal-api-aspnet-core-con-il-template-webapi)
      - [Creazione di un progetto di Minimal API ASP.NET Core con `C# Dev Kit`](#creazione-di-un-progetto-di-minimal-api-aspnet-core-con-c-dev-kit)
    - [Strumenti per il testing di API](#strumenti-per-il-testing-di-api)
      - [Postman](#postman)
      - [REST Client](#rest-client)
      - [Thunder](#thunder)
    - [Configure endpoints for the ASP.NET Core Kestrel web server](#configure-endpoints-for-the-aspnet-core-kestrel-web-server)
      - [Set the URLs for an ASP.NET Core app](#set-the-urls-for-an-aspnet-core-app)
    - [Accessing ASP.NET Core Web Server from WSL](#accessing-aspnet-core-web-server-from-wsl)
      - [Accessing Linux networking apps from Windows (localhost)](#accessing-linux-networking-apps-from-windows-localhost)
      - [Accessing Windows networking apps from Linux (host IP)](#accessing-windows-networking-apps-from-linux-host-ip)
    - [ASP.NET Core web API documentation with Swagger / OpenAPI](#aspnet-core-web-api-documentation-with-swagger--openapi)
      - [Generate OpenAPI documents](#generate-openapi-documents)
      - [Get started with NSwag and ASP.NET Core](#get-started-with-nswag-and-aspnet-core)
      - [OpenAPI support in ASP.NET Core API apps](#openapi-support-in-aspnet-core-api-apps)

## Introduzione alle API Web

Le API web (Application Programming Interface) permettono la comunicazione tra diverse applicazioni attraverso il web.
 Le API sono essenziali per lo sviluppo di applicazioni moderne, consentendo l'integrazione di servizi e dati tra sistemi
 diversi. Le API web possono essere utilizzate per vari scopi, come l'accesso ai dati di un database, l'interazione con
  servizi esterni, o la gestione di operazioni di autenticazione e autorizzazione.

## Richiamo alle specifiche REST

REST (Representational State Transfer) è uno stile architetturale per la progettazione di API web, studiato in dettaglio nel corso di informatica di quarta. In queste note si richiamano brevemente gli aspetti fondamentali dell'architettura REST.

 Le API RESTful utilizzano i metodi HTTP standard (GET, POST, PUT, DELETE) per eseguire operazioni sui dati. Le caratteristiche principali delle API RESTful includono:

- **Stateless**: Ogni richiesta del client al server deve contenere tutte le informazioni necessarie per comprendere e processare la richiesta. Il server non mantiene lo stato tra le richieste.
- **Cacheable**: Le risposte devono indicare se possono essere memorizzate nella cache o meno, migliorando l'efficienza e le
 prestazioni.
- **Uniform Interface**: Un'interfaccia uniforme che consente l'interazione tra client e server in modo standardizzato. Questo include l'uso di URL per identificare le risorse e l'uso di metodi HTTP per operare su di esse.
- **Client-Server**: Separazione delle preoccupazioni tra client e server, migliorando la scalabilità e la portabilità. Il client gestisce l'interfaccia utente e l'interazione con l'utente, mentre il server gestisce la logica di business e l'archiviazione dei dati.
- **Layered System**: L'architettura può essere composta da più livelli, migliorando la scalabilità e la gestione della sicurezza.

## Creazione di un progetto di Minimal ASP.NET Core

### Utilizzo di .NET CLI e VS Code

#### Creazione di un progetto di Minimal API ASP.NET Core con il template `web` (progetto web vuoto)

Per creare un progetto di Minimal API utilizzando la .NET CLI, si può seguire il tutorial
 [Create a minimal API with ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/tutorials/min-web-api?tabs=visual-studio-code) che spiega, passo dopo passo, come realizzare un'applicazione ASP.NET Core con la tecnologia delle `Minimal API`, che sono una tecnologia alternativa a quella basata sui `Controllers` per lo sviluppo di `Web API` in ASP.NET Core.

 Per prima cosa occorre definire la cartella dove verrà creata la soluzione con all'interno il progetto web. In questa cartella si apre il terminale (con `Powershell`, oppure con `Git Bash`)

```ps1
dotnet new sln -o TodoApi
cd TodoApi
dotnet new web -o TodoApi
dotnet sln add TodoApi/TodoApi.csproj
code .
```

Rispetto al codice che c'è nel tutorial, in questo caso si è preferito creare una soluzione con al suo interno un progetto web vuoto.

Per capire tutti i dettagli relativi all'uso della `.NET CLI` si può consultare la [documentazione di `.NET CLI`](https://learn.microsoft.com/en-us/dotnet/core/tools/), che mostra con diversi esempi come [creare un progetto a partire da un modello](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-new), oppure come [creare una soluzione](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-sln).

Per il resto si puà seguire il tutorial, passo dopo passo, dal momento che spiega in dettaglio tutti i passaggi e i concetti fondamentali.

Per l'aggiunta dei pacchetti `NuGet` al progetto si può procedere in diversi modi:

- Utilizzando la `.NET CLI` dalla shell
  - In questo caso occorre impostare correttamente il proxy, eventualmente presente
  
  ```ps1
  # Nel caso di Powershell
  $env:http_proxy="proxy.intranet:3128"
  $env:https_proxy="proxy.intranet:3128"
  # per verificare il valore delle variabili
  echo $env:http_proxy
  echo $env:https_proxy
  # le variabili impostate in questo modo sono attive per la sessione corrente
  ```

  ```sh
  # Nel caso di Bash
  http_proxy="proxy.intranet:3128"
  https_proxy="proxy.intranet:3128"
  # per verificare il valore delle variabili
  echo $http_proxy
  echo $https_proxy
  ```

  ```cmd
  :: Nel caso di CMD
  set http_proxy=proxy.intranet:3128
  set https_proxy=proxy.intranet:3128
  :: per verificare il valore delle variabili
  echo %http_proxy%
  echo %https_proxy%
  ```

  ```ps1
  # Aggiunta di Pacchetti NuGet
   dotnet add package Microsoft.EntityFrameworkCore.InMemory
   dotnet add package Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore
  ```

- Utilizzando un plugin di VS Code come, ad esempio, `Nuget Gallery`
- Utilizzando la funzionalità di `C# Dev Kit`, che permette di aggiungere pacchetti NuGet dal `Solution Explorer`

#### Creazione di un progetto di Minimal API ASP.NET Core con il template `webapi`

#### Creazione di un progetto di Minimal API ASP.NET Core con `C# Dev Kit`

### Strumenti per il testing di API

#### Postman

#### REST Client

https://github.com/Huachao/vscode-restclient

https://youtu.be/Kxp5h8tXdFE?si=W5XovIN_2iSjlRmF

#### Thunder

### Configure endpoints for the ASP.NET Core Kestrel web server

https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints

#### Set the URLs for an ASP.NET Core app

https://andrewlock.net/8-ways-to-set-the-urls-for-an-aspnetcore-app/

### Accessing ASP.NET Core Web Server from WSL

https://learn.microsoft.com/en-us/windows/wsl/networking

#### Accessing Linux networking apps from Windows (localhost)

https://learn.microsoft.com/en-us/windows/wsl/networking#accessing-linux-networking-apps-from-windows-localhost

#### Accessing Windows networking apps from Linux (host IP)

https://learn.microsoft.com/en-us/windows/wsl/networking#accessing-windows-networking-apps-from-linux-host-ip

https://superuser.com/questions/1679757/accessing-windows-localhost-from-wsl2

### ASP.NET Core web API documentation with Swagger / OpenAPI

https://learn.microsoft.com/en-us/aspnet/core/tutorials/web-api-help-pages-using-swagger

#### Generate OpenAPI documents

https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi

#### Get started with NSwag and ASP.NET Core

https://learn.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-nswag

#### OpenAPI support in ASP.NET Core API apps

https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/overview
