# ===== STAGE 1: BASE =====
# Immagine di runtime ASP.NET Core 9.0 per la fase finale
# Questa sarà l'immagine base per l'esecuzione dell'applicazione
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

# Imposta la directory di lavoro dove verrà eseguita l'app
WORKDIR /app

# Espone la porta 8080 per il traffico HTTP
# Questa è solo documentazione - non pubblica effettivamente la porta
EXPOSE 8080

# Configura ASP.NET Core per ascoltare su tutte le interfacce sulla porta 8080
ENV ASPNETCORE_URLS=http://+:8080

# Passa all'utente 'app' per motivi di sicurezza (non root)
USER app

# ===== STAGE 2: BUILD =====
# Immagine SDK .NET 9.0 per la compilazione, supporta multi-platform
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Parametro per specificare la configurazione di build (default: Release)
ARG configuration=Release

# Directory di lavoro per i file sorgente
WORKDIR /src

# Copia solo il file .csproj per sfruttare il layer caching di Docker
# Se le dipendenze non cambiano, Docker riutilizza i layer esistenti
COPY ["MyWebApiApp.csproj", "./"]

# Ripristina le dipendenze NuGet del progetto
RUN dotnet restore "MyWebApiApp.csproj"

# Copia tutti i file del progetto
COPY . .

# Torna nella directory sorgente
WORKDIR "/src/."

# Compila il progetto nella configurazione specificata
# Output nella cartella /app/build
RUN dotnet build "MyWebApiApp.csproj" -c $configuration -o /app/build

# ===== STAGE 3: PUBLISH =====
# Usa l'immagine di build precedente come base
FROM build AS publish

# Stesso parametro di configurazione
ARG configuration=Release

# Pubblica l'applicazione ottimizzata per il deployment
# UseAppHost=false evita di creare un eseguibile nativo
RUN dotnet publish "MyWebApiApp.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

# ===== STAGE 4: FINAL =====
# Ritorna all'immagine base di runtime (più leggera, senza SDK)
FROM base AS final

# Directory di lavoro per l'applicazione finale
WORKDIR /app

# Copia i file pubblicati dalla stage 'publish' nell'immagine finale
# Questo mantiene l'immagine finale piccola (solo runtime + app)
COPY --from=publish /app/publish .

# Comando di avvio dell'applicazione
# Esegue 'dotnet MyWebApiApp.dll' quando il container si avvia
ENTRYPOINT ["dotnet", "MyWebApiApp.dll"]