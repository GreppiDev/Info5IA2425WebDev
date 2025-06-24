#!/bin/bash

# Script wrapper per i test di sampling con k6
echo -e "\033[32m=== Test di Sampling OpenTelemetry ===\033[0m"
echo ""

# Verifica se k6 è installato
if ! (command -v k6 &>/dev/null || command -v k6.exe &>/dev/null); then
    echo -e "\033[31mErrore: k6 non è installato.\033[0m"
    echo -e "\033[33mInstalla k6 da: https://k6.io/docs/getting-started/installation/\033[0m"
    echo ""
    echo -e "\033[36mLinux/macOS:\033[0m"
    echo "  # Ubuntu/Debian"
    echo "  sudo gpg -k && sudo gpg --no-default-keyring --keyring /usr/share/keyrings/k6-archive-keyring.gpg --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys C5AD17C747E3415A3642D57D77C6C491D6AC1D69"
    echo "  echo \"deb [signed-by=/usr/share/keyrings/k6-archive-keyring.gpg] https://dl.k6.io/deb stable main\" | sudo tee /etc/apt/sources.list.d/k6.list"
    echo "  sudo apt-get update && sudo apt-get install k6"
    echo ""
    echo "  # macOS"
    echo "  brew install k6"
    echo ""
    echo -e "\033[36mWindows:\033[0m"
    echo "  # Chocolatey"
    echo "  choco install k6"
    echo ""
    echo "  # Scoop"
    echo "  scoop install k6"
    exit 1
fi

# Determina il comando k6 corretto
K6_CMD="k6"
if command -v k6.exe &>/dev/null; then
    K6_CMD="k6.exe"
fi

echo -e "\033[32m✓ k6 trovato: $K6_CMD\033[0m"

# Funzione per verificare se l'applicazione è in esecuzione
check_app() {
    # Prova prima con curl se disponibile
    if command -v curl &>/dev/null; then
        if curl -s -o /dev/null -w "%{http_code}" --max-time 3 "http://localhost:8080/" | grep -q "200\|404\|301\|302"; then
            return 0
        else
            return 1
        fi
    # Fallback con wget se curl non è disponibile
    elif command -v wget &>/dev/null; then
        if wget --spider --quiet --timeout=3 "http://localhost:8080/" 2>/dev/null; then
            return 0
        else
            return 1
        fi
    # Ultimo fallback con PowerShell su Windows
    elif command -v powershell.exe &>/dev/null; then
        if powershell.exe -Command "try { (Invoke-WebRequest -Uri 'http://localhost:8080/' -TimeoutSec 3 -UseBasicParsing).StatusCode } catch { 0 }" 2>/dev/null | grep -q "200\|404\|301\|302"; then
            return 0
        else
            return 1
        fi
    else
        echo -e "\033[33mAvviso: Nessun comando HTTP disponibile (curl, wget, powershell). Assumo che l'app sia in esecuzione...\033[0m"
        return 0
    fi
}

# Verifica se l'applicazione è in esecuzione
echo -e "\033[33mVerifica che l'applicazione sia in esecuzione...\033[0m"
if ! check_app; then
    echo -e "\033[31mErrore: L'applicazione non è raggiungibile su http://localhost:8080/\033[0m"
    echo -e "\033[33mAvvia l'applicazione con: docker-compose up -d\033[0m"
    exit 1
fi

echo -e "\033[32m✓ Applicazione in esecuzione\033[0m"
echo ""

# Menu di selezione
echo -e "\033[36mSeleziona il tipo di test:\033[0m"
echo "1) Test base sampling (equivalente allo script PowerShell)"
echo "2) Test avanzato sampling (con trigger di tail sampling)"
echo "3) Esegui entrambi i test"
echo ""
read -p "Scegli un'opzione (1-3): " choice

case $choice in
1)
    echo -e "\033[33mEsecuzione test base sampling...\033[0m"
    $K6_CMD run test-sampling.js
    ;;
2)
    echo -e "\033[33mEsecuzione test avanzato sampling...\033[0m"
    $K6_CMD run test-sampling-advanced.js
    ;;
3)
    echo -e "\033[33mEsecuzione test base sampling...\033[0m"
    $K6_CMD run test-sampling.js
    echo ""
    echo -e "\033[33mEsecuzione test avanzato sampling...\033[0m"
    $K6_CMD run test-sampling-advanced.js
    ;;
*)
    echo -e "\033[31mScelta non valida\033[0m"
    exit 1
    ;;
esac

echo ""
echo -e "\033[32m=== Test completati! ===\033[0m"
echo -e "\033[34mControlla i risultati in:\033[0m"
echo -e "  🔍 Jaeger UI: http://localhost:16686"
echo -e "  📊 Prometheus: http://localhost:9090"
echo -e "  📈 Grafana: http://localhost:3000"
echo ""
echo -e "\033[33mCosa aspettarsi:\033[0m"
echo -e "  • Head sampling al 1%: Solo ~1% delle richieste normali visibili"
echo -e "  • Tail sampling: TUTTE le tracce con errori o latenza >500ms mantenute"
