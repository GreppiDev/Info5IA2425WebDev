# Stage base: Eseguito come root per permettere all'entrypoint di sistemare i permessi.
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
# NOTA: Non impostare 'USER app' qui; la logica dell'entrypoint incorporata gestirà il cambio utente.

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["EducationalGames.csproj", "./"]
RUN dotnet restore "EducationalGames.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "EducationalGames.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "EducationalGames.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Logica dell'entrypoint incorporata.
# Questo comando viene eseguito come root all'avvio del container.
# "$0" e "$@" si riferiscono al comando e agli argomenti specificati in CMD.

# Imposta i permessi per la directory delle chiavi di protezione dei dati.
# Utilizza la variabile d'ambiente DATA_PROTECTION_KEYS_PATH se definita,
# altrimenti usa un valore di default.
ENTRYPOINT ["sh", "-c", "\
    set -e; \
    TARGET_DP_KEYS_DIR=\"${DATA_PROTECTION_KEYS_PATH:-/app/shared_dp_keys}\"; \
    echo \"Entrypoint incorporato: Percorso chiavi Data Protection: ${TARGET_DP_KEYS_DIR}\"; \
    echo 'Entrypoint incorporato: Controllo e impostazione permessi...'; \
    mkdir -p \"${TARGET_DP_KEYS_DIR}\"; \
    chown -R app:app \"${TARGET_DP_KEYS_DIR}\"; \
    echo \"Entrypoint incorporato: Proprietario di ${TARGET_DP_KEYS_DIR} impostato su app:app.\"; \
    echo 'Entrypoint incorporato: Esecuzione del CMD come utente app...'; \
    exec runuser -u app -- \"$0\" \"$@\" \
    "]

# Il comando CMD che verrà eseguito dall'entrypoint come utente 'app'
CMD ["dotnet", "EducationalGames.dll"]
