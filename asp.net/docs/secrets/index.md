# ASP.NET Secrets

- [ASP.NET Secrets](#aspnet-secrets)
	- [Perché usare i Secrets?](#perché-usare-i-secrets)
	- [Uso dei Secret Manager tool in ASP.NET](#uso-dei-secret-manager-tool-in-aspnet)
	- [Dove sono memorizzati i secrets?](#dove-sono-memorizzati-i-secrets)
	- [Come usare i parametri di configurazione in ASP.NET](#come-usare-i-parametri-di-configurazione-in-aspnet)
	- [Quando non usare i secrets?](#quando-non-usare-i-secrets)
	- [Cenni alle variabili d'ambiente](#cenni-alle-variabili-dambiente)

La gestione sicura delle credenziali e delle chiavi API è fondamentale per evitare fughe di dati e garantire la protezione delle applicazioni. In ASP.NET, possiamo utilizzare **User Secrets**, **variabili d'ambiente** e **Azure Key Vault** per archiviare informazioni sensibili senza inserirle nel codice sorgente.

## Perché usare i Secrets?

- **Evitare di effettuare commit di dati sensibili** nei repository Git.
- **Facilitare la configurazione** tra ambienti di sviluppo, test e produzione.
- **Migliorare la sicurezza** separando le credenziali dal codice.

## [Uso dei Secret Manager tool in ASP.NET](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets#secret-manager)

Secret Manager tool è un sistema integrato in ASP.NET Core per gestire configurazioni sensibili in ambienti di sviluppo. Questo strumento memorizza i secrets localmente sul **proprio computer di sviluppo, fuori dalla directory del progetto**, così non si rischia che vengano accidentalmente inseriti in commit del progetto.

I passaggi per la gestione dei secrets in fase di sviluppo sono riportati di seguito:

1. Aprire il file del progetto (il file con estensione `.csproj`) e aggiungere manualmente un `PropertyGroup` come indicato di seguito:

	```xml
		<PropertyGroup>
			<UserSecretsId></UserSecretsId>
		</PropertyGroup>
	```

2. Inserire all'interno del tag `UserSecretId` un identificativo univoco che sia facilmente riconducibile al progetto. Per creare questo identificativo si può, ad esempio, utilizzare il nome del progetto seguito da un GUID (Globally Unique Identifier). Per generare un GUID valido si può utilizzare una delle tante estensioni disponibili per VS Code, come ad esempio, `Insert GUID` (Extension ID = `heaths.vscode-guid`). Una volta installata l'estensione si potrà inserire un UserSecretId come mostrato di seguito:

	```xml
		<PropertyGroup>
			<UserSecretsId>NomeProgetto-acd69e4f-34c8-4275-85e2-34100e896360</UserSecretsId>
		</PropertyGroup>
	```

3. Aprire il terminale nella root del progetto (dove si trova il file con estensione `.csproj`) ed eseguire:

	```sh
		# Nella directory del progetto, si inizializza il secret manager
		dotnet user-secrets init
	```

4. Aggiungere i secrets con il comando `dotnet user-secrets "key" "value"`. Ad esempio:

	```sh
		dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=myserver;Database=mydb;User Id=myuser;Password=mypassword;"
	```

	Si noti che nel file `appsettings.json` la struttura dei valori di configurazione è del tipo:

	```json
	{
		"Logging": {
			"LogLevel": {
			"Default": "Information",
			"Microsoft.AspNetCore": "Warning"
			}
		},
		"AllowedHosts": "*",
		"ApiKeys": {
			"MySecretKey": "chiave-segreta"
		}
	}
	```

	ma l'aggiunta dei valori nei secrets va fatta con la sintassi che prevede l'utilizzo dei `:` per definire i livelli di raggruppamento del `JSON` di `appsettings.json`:

	```sh
	dotnet user-secrets set "ApiKeys:MySecretKey" "chiave-segreta"
	```

5. Una volta che le coppie chiave-valore dei segreti sono stati inserite nel file dei secrets è possibile eliminare i valori delle chiavi dal file `appsettings.json`, perché il sistema di configurazione di ASP.NET leggerà i valori delle chiavi sovrascrivendo chiavi eventualmente presenti in `appsettings.json`. Dopo questa operazione il file `appsettings.json` sarà del tipo seguente:

	```json
	{
		"Logging": {
			"LogLevel": {
			"Default": "Information",
			"Microsoft.AspNetCore": "Warning"
			}
		},
		"AllowedHosts": "*",
		"ApiKeys": {
			"MySecretKey": ""
		}
	}

	```

	Si noti che si sarebbe potuto eliminare del tutto la sezione `"ApiKeys:MySecretKey"` dal file `appsettings.json` ma non è una buona idea perché il fatto che le chiavi esistano nel file di configurazione (ma senza un valore associato) è utile per capire velocemente quali siano le dipendenze dai secrets dell'applicazione.

## Dove sono memorizzati i secrets?

In Windows:

```sh
	%APPDATA%\Microsoft\UserSecrets\<user_secrets_id>\secrets.json
```

In Linux/MacOS:

```sh
	~/.microsoft/usersecrets/<user_secrets_id>/secrets.json
```

## Come usare i parametri di configurazione in ASP.NET

Indipendentemente dal fatto che le coppie chiave-valore siano nel file `appsettings.json` oppure nel file `secrets.json` del progetto (i secrets, se presenti, sovrascrivono i valori delle coppie chiave-valore di `appsettings.json`) si può accedere a tali coppie usando la `Dependency Injection`.

1. Primo esempio

	```cs
	var builder = WebApplication.CreateBuilder(args);
	builder.Configuration.AddUserSecrets<Program>();

	var secretValue = builder.Configuration["ApiKeys:MySecretKey"];
	Console.WriteLine($"La mia chiave segreta è: {secretValue}");
	```

2. Secondo esempio

	Nell'esempio seguente viene configurata la `Dependency Injection` nella classe che deve accedere ai parametri di configurazione (potrebbero essere parametri in `appsettings.json` oppure nel file `secrets.json`, oppure in variabili d'ambiente come si vedrà più avanti). nel codice mostrato di seguito si ipotizza che la chiave memorizzata nei secrets sia necessaria per il funzionamento del front-end:

	```cs
	//Si definisce una interfaccia per accedere a un servizio.
	//In questo caso si dà all'interfaccia il nome IKeyService, ma 
	//si può scegliere il nome che si vuole, rispettando le convenzioni .net (l'interfaccia deve iniziare con I)
	public interface IKeyService
	{
		string GetSecretKey();
	}
	```

	```cs
	//classe che implementa l'interfaccia IKeyService
	public class KeyService : IKeyService
	{
		private readonly IConfiguration _configuration;

		public KeyService(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public string GetSecretKey()
		{
			return _configuration["ApiKeys:MySecretKey"] ?? throw new InvalidOperationException("API key not configured");
		}
	}
	```

	```cs
	//Nel Program.CS si configura il servizio KeyService e da quel punto in poi, il servizio è disponibile ovunque sia necessario nell'applicazione

	var builder = WebApplication.CreateBuilder(args);

	// Add services to the container.
	builder.Services.AddOpenApi();

	// Register KeyService
	builder.Services.AddScoped<IKeyService, KeyService>();

	var app = builder.Build();

	// Configure the HTTP request pipeline.
	if (app.Environment.IsDevelopment())
	{
		app.MapOpenApi();
		app.UseDeveloperExceptionPage();
	}

	app.UseHttpsRedirection();

	// Enable serving static files from wwwroot
	app.UseDefaultFiles();
	app.UseStaticFiles();

	// use key key service to provide the key to front-end
	app.MapGet("/api/key", (IKeyService keyService) =>
	{
		try
		{
			var key = keyService.GetSecretKey();
			return Results.Ok(new { key });
		}
		catch (Exception)
		{
			return Results.StatusCode(500);
		}
	})
	.WithName("GetKey");

	app.Run();
	```

## Quando non usare i secrets?

I secrets sono ottimi in fase di sviluppo, ma non sono adatti all'ambiente di produzione. In Produzione si utilizzano le **variabili d'ambiente**, oppure meccanismi di gestione dei secrets specifici della piattaforma cloud che si utilizza come, ad esempio, **Azure KeyVault**.

## Cenni alle variabili d'ambiente

Per ambienti di produzione, usa le variabili d’ambiente anziché i secrets locali.

1. Impostare una Variabile d'Ambiente
   Su Windows (PowerShell):

   ```sh
   $env:ApiKeys__MySecretKey="super-secret-value"
   ```

   Su Linux/macOS (Bash):

   ```sh
   export ApiKeys__MySecretKey="super-secret-value"
   ```

   ASP.NET leggerà automaticamente queste variabili, utilizzando `Configuration`.
