#!/usr/bin/env bash

# Uso di variabili nella bash
## Tutorial su Come Usare le Variabili nella Shell Bash

# Le variabili in Bash permettono di memorizzare e manipolare valori come stringhe, numeri o output
# di comandi. In questo tutorial vediamo i concetti fondamentali su come dichiarare, usare e gestire
# le variabili all'interno di script o terminale Bash.

# ### 1. **Dichiarare una Variabile**
# In Bash, è possibile dichiarare una variabile assegnando un valore senza utilizzare simboli 
# particolari (ad esempio, niente `$`). Gli spazi intorno all'operatore di assegnazione non sono
# ammessi.

# #### Sintassi:
# ```bash
# variabile=valore
# ```

# #### Esempi:
# ```bash
# nome="Mario"
# eta=30
# ```

# > **Nota**: Non si deve mai mettere gli spazi prima o dopo l'uguale (`=`), altrimenti Bash
# restituirà un errore.

# ### 2. **Accedere al Valore di una Variabile**
# Per utilizzare il valore di una variabile, bisogna anteporre il simbolo `$` al nome della variabile.

# #### Esempio:
# ```bash
# echo $nome
# ```
# Questo comando stamperà `Mario` sul terminale.

# Se si vuole accedere alla variabile all'interno di una stringa più lunga, è possibile usare le 
# parentesi graffe `{}` per evitare ambiguità:

# ```bash
# echo "Ciao, mi chiamo ${nome}!"
# ```

# ### 3. **Variabili nelle Stringhe**
# Puoi inserire variabili all'interno di stringhe usando le virgolette doppie (`" "`). Bash 
# espanderà le variabili all'interno delle stringhe racchiuse tra virgolette doppie.

#### Esempio:
# ```bash
# echo "Il mio nome è $nome e ho $eta anni."
# ```

# Le virgolette singole (' ') non espandono le variabili. Tutto viene trattato come testo letterale:

# ```bash
# echo 'Il mio nome è $nome e ho $eta anni.'
# ```
# Output:
# ```
# Il mio nome è $nome e ho $eta anni.
# ```

### 4. **Output di Comandi nelle Variabili**
# Si può memorizzare l'output di un comando all'interno di una variabile usando il backtick ( ` ) o 
# la sintassi `$(comando)`.

#### Esempio con il backtick:
# ```bash
# data=`date`
# ```

#### Esempio con la sintassi moderna `$(...)`:
# ```bash
# data=$(date)
# ```

# Entrambi gli approcci memorizzeranno l'output del comando `date` nella variabile `data`.

# ### 5. **Uso di Variabili in Comandi**
# È possibile utilizzare variabili all'interno di comandi come se fossero valori letterali. 
# Bisogna ricordare di racchiuderle tra virgolette doppie per evitare problemi di globbing o 
# word splitting (vedere sotto), soprattutto quando il valore della variabile può contenere spazi
# o caratteri speciali.

#### Esempio:
# ```bash
# file="/path/to/some file.txt"
# cat "$file"
# ```
# Se non si racchiude la variabile in virgolette, Bash interpreterà lo spazio come separatore tra
# argomenti, portando a un errore.

### 6. **Variabili di Ambiente**
# Le variabili di ambiente sono variabili disponibili a tutti i processi eseguiti dalla shell corrente.
# Per creare una variabile di ambiente, si usa il comando `export`.

#### Esempio:
# ```bash
# export PATH=$PATH:/usr/local/bin
# ```
# Questo aggiunge `/usr/local/bin` al percorso di ricerca dei comandi (PATH).

### 7. **Leggere Input dall'Utente**
# È possibile chiedere all'utente di inserire un valore e memorizzarlo in una variabile usando 
# il comando `read`.

#### Esempio:
# ```bash
# echo "Come ti chiami?"
# read nome
# echo "Ciao, $nome!"
# ```

### 8. **Variabili Predefinite**
# Bash ha alcune variabili predefinite che possono essere utili:

# - `$0` – Nome dello script corrente.
# - `$1`, `$2`, ... – Argomenti passati allo script.
# - `$#` – Numero di argomenti passati allo script.
# - `$@` – Tutti gli argomenti come una lista.
# - `$?` – Exit status dell'ultimo comando eseguito.
# - `$$` – ID del processo corrente.
  
#### Esempio:
# ```bash
# echo "Il nome dello script è $0"
# echo "Ho ricevuto $# argomenti: $@"
# ```

### 9. **Rendere le Variabili Solo in Lettura**
# È possibile proteggere una variabile da future modifiche usando il comando `readonly`.

#### Esempio:
# ```bash
# readonly costante="Non modificabile"
# ```

# Tentare di modificare `costante` dopo questo comando causerà un errore.

### 10. **Cancellare Variabili**
# Si può rimuovere una variabile usando il comando `unset`.

#### Esempio:
# ```bash
# unset nome
# ```

# Dopo questo comando, la variabile `nome` non sarà più disponibile.

### 11. **Uso di Variabili con Caratteri Speciali**
# Quando si lavora con valori che possono contenere caratteri speciali (come spazi o simboli), è 
# essenziale racchiudere la variabile tra virgolette doppie per evitare che Bash interpreti questi 
# caratteri in modi indesiderati.

# #### Esempio:
# ```bash
# file_name="My file.txt"
# echo "$file_name"
# ```

### Conclusione
# Le variabili in Bash sono strumenti potenti che permettono di gestire valori dinamici e 
# automatizzare task ripetitivi. Seguendo queste regole fondamentali, è possibile usare le variabili
# in modo sicuro e flessibile.

# Se si seguono le buone pratiche come l'uso delle virgolette doppie e la gestione accurata delle
# variabili, è possibile a evitare errori comuni come il globbing o il word splitting.

### Problemi connessi a Globbing e Word Splitting

# # Consideriamo i comandi:
# container_ip=$(docker inspect mysql-server1 | jq -r '.[0].NetworkSettings.Networks[].IPAddress')
# docker run -it --name my_client --rm mysql:latest mysql -h$container_ip -uroot -p

# # In questo caso la variabile container_ip è utilizzata per definire l'indirizzo ip del container 
# con nome my_client; tuttavia il linter della bash suggerisce di scrivere la variabile tra doppi
# apici per evitare il problema del Globbing e del Word Splitting. Vediamo nel dettaglio di cosa 
# si tratta:

# ### Globbing
# Il globbing è il processo attraverso cui la shell espande i caratteri jolly, come `*` o `?`, 
# in base ai file o directory che corrispondono a questi pattern nel filesystem. Ad esempio, se
# la variabile `container_ip` contenesse dei caratteri come `*`, Bash potrebbe interpretarlo
# come un pattern e cercare di espandere tale stringa in file esistenti.

# ### Word Splitting
# La **divisione in parole** (word splitting) si verifica quando una variabile contiene spazi e
# Bash la divide in più parole. Ad esempio, se `container_ip` contenesse un indirizzo IP con spazi
# indesiderati o non visibili, potrebbe causare un'interpretazione sbagliata dell'intero comando.

# ### Correzione del comando
# Per evitare questi problemi, è una buona pratica racchiudere la variabile tra virgolette doppie `"`. Così facendo, si impedisce a Bash di effettuare il globbing e la divisione in parole, mantenendo intatto il contenuto della variabile.

# Il comando corretto sarà:

# ```bash
# docker run -it --name my_client --rm mysql:latest mysql -h"$container_ip" -uroot -p
# ```

# In questo modo, la variabile `container_ip` viene interpretata correttamente come un singolo argomento senza rischio di espansioni indesiderate o divisioni.

### Quando usare le graffe per racchiudere una variabile all'interno di una stringa racchiusa tra 
# doppi apici

# In Bash, si può fare l'espansione di una variabile semplicemente anteponendo il simbolo `$` al suo
# nome, ma in alcuni casi è consigliabile racchiudere il nome della variabile tra parentesi graffe `{}`.
# Vediamo in dettaglio quando e perché usare le parentesi graffe e cosa cambia:

### Espansione Semplice Senza Parentesi Graffe
# Se si sta usando una variabile senza altri caratteri immediatamente attaccati al suo nome, è possibile
# accedere al valore della variabile semplicemente con `$variabile`.

#### Esempio:
# ```bash
# nome="Mario"
# echo "Ciao $nome!"
# ```

# **Output:**
# ```
# Ciao Mario!
# ```

### Quando Usare le Parentesi Graffe
# Le parentesi graffe `{}` vengono utilizzate quando è necessario **delimitare il nome della variabile**
# per evitare ambiguità con altri caratteri. Servono soprattutto quando si vuole concatenare la 
# variabile con del testo, numeri o simboli senza che Bash si confonda su dove finisce il nome della 
# variabile.

#### Esempio di Necessità di Parentesi Graffe
# Si immagini di voler accedere a una variabile e aggiungere del testo subito dopo il suo valore, 
# come un suffisso.

# ```bash
# nome="Mario"
# echo "Ciao $nome123"
# ```

# In questo caso, Bash pensa che tu si stia cercando una variabile chiamata `nome123`, che
# probabilmente non esiste. Questo perché non ci sono delimitatori che separano `nome` dal numero `123`.

#### Soluzione con Parentesi Graffe:
# Per indicare a Bash che la variabile è solo `nome` e che `123` è un testo da concatenare dopo il 
# valore della variabile, è possibile usare le parentesi graffe:

# ```bash
# echo "Ciao ${nome}123"
# ```

# **Output:**
# ```
# Ciao Mario123
# ```

# In questo caso Bash capisce correttamente che deve espandere solo la variabile `nome`, e poi
# concatenare il testo `123`.

### Altri Esempi di Utilizzo delle Parentesi Graffe

# 1. **Concatenazione di Testo:**
#    ```bash
#    prefisso="file"
#    echo "${prefisso}_config.txt"
#    ```

#    **Output:**
#    ```
#    file_config.txt
#    ```

# 2. **Uso con caratteri speciali:**
#    Se si vuole usare una variabile seguita da un carattere che potrebbe essere confuso con il 
#    nome della variabile, come un simbolo speciale o una lettera subito dopo.

#    ```bash
#    dir="/home/user"
#    echo "${dir}/file.txt"
#    ```

#    **Output:**
#    ```
#    /home/user/file.txt
#    ```

### Quando Non Servono le Parentesi Graffe
# Le parentesi graffe non sono necessarie se non c'è ambiguità nel modo in cui Bash interpreta il 
# nome della variabile. Se non si deve concatenare nulla subito dopo il nome della variabile, si può
# semplicemente usare `$nome`.

# #### Esempio:
# ```bash
# nome="Mario"
# echo "Ciao $nome"
# ```

# ### Conclusione
# - Si usano le **parentesi graffe `{}`** quando si ha bisogno di delimitare chiaramente il nome
# della variabile, ad esempio per concatenare testo subito dopo la variabile o evitare ambiguità
# con caratteri speciali.
# - Non sono necessarie le parentesi graffe quando il nome della variabile è chiaramente separato
# dagli altri caratteri, come spazi o simboli che non fanno parte del nome della variabile.