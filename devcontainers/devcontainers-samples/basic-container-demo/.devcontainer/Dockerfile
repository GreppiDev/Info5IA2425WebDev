FROM mcr.microsoft.com/devcontainers/dotnet:10.0-noble

USER root

# Installa strumenti utili per lo sviluppo
RUN apt-get update && DEBIAN_FRONTEND=noninteractive apt-get install -y \
    git \
    curl \
    vim \
    nano \
    htop \
    tree \
    jq \
    ca-certificates \
    gnupg \
    iproute2 \
    iputils-ping \
    dnsutils \
    netcat-openbsd \
    openssl \
    procps \
    lsof \
    mariadb-client \
    mariadb-server \
    httpie \
    yq \
    && rm -rf /var/lib/apt/lists/*

# Se il progetto ha come target net9.0 anche con SDK .NET 10 serve il runtime .NET 9 per eseguire app/tool net9.
# Installiamo quindi i runtime 9 affiancati a quelli 10 nel DOTNET_ROOT del container.
RUN curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh \
    && chmod +x /tmp/dotnet-install.sh \
    && /tmp/dotnet-install.sh --install-dir /usr/share/dotnet --runtime dotnet --channel 9.0 --no-path --skip-non-versioned-files \
    && /tmp/dotnet-install.sh --install-dir /usr/share/dotnet --runtime aspnetcore --channel 9.0 --no-path --skip-non-versioned-files \
    && rm /tmp/dotnet-install.sh

# Configura Git (opzionale)
RUN git config --system init.defaultBranch main
RUN git config --system --add safe.directory "/workspaces/*"

# Installa global tools .NET per l'utente con cui lavorerai nel Dev Container
USER vscode
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/home/vscode/.dotnet/tools"

# Avvio MariaDB (senza systemd) quando parte il container
USER root
COPY .devcontainer/mariadb-entrypoint.sh /usr/local/bin/mariadb-entrypoint.sh
RUN chmod +x /usr/local/bin/mariadb-entrypoint.sh

# Torna all'utente di default del devcontainer (l'avvio di MariaDB è gestito via devcontainer.json)
USER vscode
