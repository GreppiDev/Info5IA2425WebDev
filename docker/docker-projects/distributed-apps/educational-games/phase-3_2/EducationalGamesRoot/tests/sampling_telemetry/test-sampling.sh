#!/bin/bash

# Test script per verificare il tail-based sampling
echo -e "\033[32mAvvio test delle policy di sampling...\033[0m"

# Genera 20 richieste rapide per testare il head sampling (1%)
echo -e "\033[33mGenerazione di 20 richieste rapide per testare head sampling (1%)...\033[0m"

for i in {1..20}; do
    if curl -s -o /dev/null -w "%{http_code}" --max-time 5 "http://localhost:8080/" >/dev/null 2>&1; then
        echo -e "\033[36mRichiesta $i completata\033[0m"
    else
        echo -e "\033[31mRichiesta $i fallita\033[0m"
    fi
    sleep 0.1
done

echo ""
echo -e "\033[32mTest completato! Controlla Jaeger UI per vedere le tracce filtrate.\033[0m"
echo -e "\033[34mURL Jaeger: http://localhost:16686\033[0m"
echo ""
echo -e "\033[33mCon head sampling al 1%, dovresti vedere circa 0-1 tracce dalle 20 richieste.\033[0m"
echo -e "\033[33mLe tracce con errori o latenza > 500ms saranno comunque mantenute dal tail sampling.\033[0m"
