#!/usr/bin/env bash

# Importante: quando si utilizza una distribuzione WSL a scuola e si vuole accedere a internet dalla shell, ad esempio per
# eseguire il comando curl, oppure per scaricare o aggiornare un pacchetto tramite apt, oppure apt-get, occorre configurare
# il proxy.

## Configurazione del proxy per apt, apt-get
# per apt, apt-get la configurazione del proxy si effettua come segue:
# host --> proxy12
# port --> 3128

# L'impostazione del proxy può essere fatta con un solo comando:
sudo tee -a /etc/apt/apt.conf.d/proxy.conf <<EOF
Acquire::http::Proxy "http://proxy12:3128";
EOF

## Spiegazione del comando per impostare il proxy (da ChatGPT)
# - **`tee -a`**: Usa il comando `tee` con l'opzione `-a` per fare l'append del contenuto al file specificato.
#   Questo comando legge l'input dallo standard input e lo scrive nel file.
# - **`<<EOF`**: Questo è un here document standard che permette di passare testo multilinea come input al comando `tee`.

# ### Funzionamento passo passo del comando corretto:
# 1. Il comando `sudo tee -a /etc/apt/apt.conf.d/proxy.conf` con l'input rediretto tramite `<<EOF` scrive la configurazione
# fornita direttamente nel file di configurazione del proxy di APT.
# 2. L'input fornito tra `EOF` e `EOF` viene aggiunto al file `/etc/apt/apt.conf.d/proxy.conf`, senza sovrascrivere il
# contenuto precedente.

# Vediamo se il proxy per apt-get è stato configurato correttamente:
cat /etc/apt/apt.conf.d/proxy.conf
# il risultato dovrebbe essere
# Acquire::http::Proxy "http://proxy12:3128";

# Prima di installare i pacchetti conviene effettuare un aggiornamento degli elenchi dei repository
sudo apt update -y
# oppure
sudo apt-get update -y

# Se le impostazioni del proxy non venissero lette dopo aver configurato il file proxy.conf si dovrebbe fare un logout e un login
# nella WSL

# Per rimuovere il proxy si può utilizzare l'istruzione:
sudo sed -i '/Acquire::http::Proxy/d' /etc/apt/apt.conf.d/proxy.conf

## Spiegazione del comando sed per rimuovere il proxy (da ChatGPT)
# Per rimuovere la configurazione del proxy da un file come `/etc/apt/apt.conf.d/proxy.conf`, si può un'istruzione
# che elimina specificamente la riga contenente la configurazione del proxy, ad esempio usando il comando `sed` che
# è usato per modificare file di testo da riga di comando.

# Ecco un esempio:

# ```bash
# sudo sed -i '/Acquire::http::Proxy/d' /etc/apt/apt.conf.d/proxy.conf
# ```

# ### Spiegazione dettagliata:
# 1. **`sed`**: È un editor di flussi che consente di eseguire modifiche ai file direttamente dalla riga di comando.
# 2. **`-i`**: Questa opzione permette di modificare il file direttamente "in-place", ovvero senza bisogno di creare un
# file temporaneo.
# 3. **`'/Acquire::http::Proxy/d'`**: Questa espressione dice a `sed` di cercare qualsiasi riga che contenga il pattern
# `Acquire::http::Proxy` e di cancellarla (`d` sta per delete).
# 4. **`/etc/apt/apt.conf.d/proxy.conf`**: Questo è il file di configurazione dove si trova il proxy, che verrà modificato.

# ### Funzionamento:
# Il comando cerca nel file tutte le righe che contengono la stringa `Acquire::http::Proxy` e le elimina. Questo rimuove
# la configurazione del proxy senza toccare altre parti del file.
# Dopo aver eseguito questo comando, il proxy non sarà più configurato in `/etc/apt/apt.conf.d/proxy.conf`.
# l'output dovrebbe essere un file vuoto

# per verificare che il proxy sia stato eliminato:
cat /etc/apt/apt.conf.d/proxy.conf

## Configurazione del proxy per curl e wget
# curl
# https://curl.se/docs/manpage.html
# https://ss64.com/bash/curl.html
# Vedere anche gli appunti di informatica di quarta relativamente al comando curl

# wget
# https://www.gnu.org/software/wget/manual/wget.html
# Per impostare un proxy per `curl` in WSL (Ubuntu), è possibile usare le variabili d'ambiente che `curl` utilizza per gestire
# le connessioni HTTP e HTTPS attraverso un proxy. Si può impostare il proxy in modo temporaneo (per una singola sessione) o
# permanente (per tutte le sessioni).

### 1. **Impostazione temporanea del proxy (solo per la sessione corrente):**
#    Si può impostare il proxy per la sessione corrente del terminale usando le variabili d'ambiente `http_proxy` e `https_proxy`.

#    Si eseguano i comandi seguenti per configurare il proxy:

#    ```bash
export http_proxy="http://proxy12:3128"
export https_proxy="http://proxy12:3128"
#    ```

#    - **`http_proxy`**: imposta il proxy per le connessioni HTTP.
#    - **`https_proxy`**: imposta il proxy per le connessioni HTTPS.

#    Dopo aver eseguito questi comandi, `curl` utilizzerà automaticamente il proxy per tutte le richieste HTTP/HTTPS
# eseguite nella sessione corrente.

### 2. **Impostazione permanente del proxy:**
#    Per configurare il proxy in modo che venga utilizzato in tutte le sessioni future, si può aggiungere le variabili
#  d'ambiente al file `.bashrc` o `.bash_profile` nella propria home directory.

#    1. Aprire il file `.bashrc` o `.bash_profile` con un editor di testo (come `nano`):

#       ```bash
#       nano ~/.bashrc
#       ```

#    2. Aggiungere le seguenti righe alla fine del file:

#       ```bash
#       export http_proxy="http://proxy12:3128"
#       export https_proxy="http://proxy12:3128"
#       ```

#    3. Salvare il file e chiudere l'editor (`Ctrl + O`, `Invio` per salvare, e `Ctrl + X` per uscire).

#    4. Rendere effettive le modifiche eseguendo:

#       ```bash
#       source ~/.bashrc
#       ```

# ### 3. **Disabilitare il proxy temporaneamente:**
#    Se si è scelto di abilitare il proxy in maniera permanente, ma poi si desidera disabilitarlo per una singola esecuzione
#  di `curl`, si può eseguire il comando `curl` con l'opzione `--noproxy` per ignorare il proxy:

#    ```bash
curl --noproxy "*" http://example.com
#    ```

# Oppure nel caso di wget, per ignorare il proxy su una singola richiesta, si può eseguire il comando:

#    ```bash
wget --no-proxy http://example.com
#    ```

#   Questo comando fa sì che `curl` non utilizzi alcun proxy per quella richiesta.

### 4. **Testare il proxy:**
#    Dopo aver configurato il proxy, si può verificare che funzioni correttamente con `curl` utilizzando un sito di test,
#  come ad esempio:

#    ```bash
#    curl -I http://example.com
#    ```

# Se il proxy è configurato correttamente, `curl` utilizzerà il proxy specificato per tutte le connessioni HTTP e HTTPS.

#    ```bash
#    wget http://example.com

### Cancellazione delle variabili d'ambiente relative al proxy (come `http_proxy` e `https_proxy`)

#  Si può usare il comando `unset` nella tua sessione corrente. Questo comando rimuove la variabile d'ambiente,
# impedendo a strumenti come `curl` o `wget` di usarla.

# ### Cancellare le variabili d'ambiente per la sessione corrente:
# Eseguire questi comandi per eliminare le variabili proxy per la sessione corrente:

# ```bash
unset http_proxy
unset https_proxy
unset ftp_proxy
# ```

# Se sono state impostate variabili per i proxy su altri protocolli (come `ftp_proxy` o `all_proxy`), è possibile cancellarle
# allo stesso modo.

# ### Cancellare le variabili in modo permanente:
# Se si sono aggiunte le variabili nel file di configurazione (ad esempio in `~/.bashrc` o `~/.bash_profile`), bisogna rimuovere
# o commentare le righe che le definiscono.

# 1. **Modifica il file `.bashrc` o `.bash_profile`**:
#    Aprire il file in cui sono state aggiunte le variabili, ad esempio `.bashrc`:

#    ```bash
#    nano ~/.bashrc
#    ```

# 2. **Trovare e rimuovere le righe che impostano le variabili proxy**:
#    Cercare le righe simili a queste:

#    ```bash
#    export http_proxy="http://proxy12:3128"
#    export https_proxy="http://proxy12:3128"
#    ```

# 3. **Salvare e chiudere il file**:
#    Dopo aver rimosso o commentato (aggiungendo un `#` all'inizio della riga) queste righe, salvare e chiudere il file
#    (`Ctrl + O`, `Invio` per salvare, e `Ctrl + X` per uscire).

# 4. **Applica le modifiche**:
#    Dopo aver modificato il file, eseguire il seguente comando per rendere effettive le modifiche nella sessione corrente:

#    ```bash
#    source ~/.bashrc
#    ```

# ### Verificare la rimozione delle variabili:
# Puoi controllare se le variabili sono state rimosse correttamente eseguendo:

# ```bash
echo $http_proxy
echo $https_proxy
# ```

# Se le variabili sono state cancellate correttamente, non dovrebbe essere mostrato alcun output.

### Perché si usa `http` per entrambe le variabili http_proxy e https_proxy?

# I proxy (come quelli di tipo HTTP) non crittografano direttamente il traffico HTTPS. Quando si invia una richiesta HTTPS
# attraverso un proxy HTTP, il proxy stabilisce una connessione con il server di destinazione utilizzando il metodo `CONNECT`,
# ma la connessione tra il client (il nostro computer) e il server finale rimane cifrata grazie a HTTPS. Il traffico cifrato passa attraverso il
# proxy, ma il proxy non decrittografa la connessione.

# Quindi, il protocollo da usare nel proxy stesso, anche per connessioni HTTPS, è solitamente `http`, a meno che non si stia
# usando un proxy SSL o un altro tipo di proxy che supporta nativamente connessioni sicure (caso meno comune).

# ### Esempio pratico:
# Quando si imposta:

# ```bash
# export https_proxy="http://proxy.example.com:8080"
# ```

# Si sta dicendo a `curl`, `wget` o un altro strumento di inoltrare le richieste HTTPS al proxy tramite il protocollo HTTP.
# Il traffico verso il sito di destinazione sarà ancora cifrato (grazie a HTTPS), ma il proxy riceverà la richiesta usando HTTP.

# ### Quando usare `https`?

# Se il proxy supporta effettivamente connessioni HTTPS (un proxy SSL), allora si potrebbe usare il protocollo `https` nella
# variabile `https_proxy`, come in:

# ```bash
# export https_proxy="https://proxy.example.com:8080"
# ```

# Tuttavia, nella maggior parte dei casi, i proxy HTTP standard non usano HTTPS per ricevere le richieste, anche se il traffico
# destinato al server finale è cifrato.

# ### Conclusione:
# Nella maggior parte dei casi, è corretto usare `http` anche per la variabile `https_proxy`. Se il proxy supporta HTTPS
# nativo (caso raro), allora si potrebbe usare `https`, ma solitamente la configurazione standard prevede `http` per entrambe
# le variabili.
