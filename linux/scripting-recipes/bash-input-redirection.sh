#!/usr/bin/env bash

# Nella shell bash, gli operatori `<`, `<<`, `<<<`, e `< <` sono utilizzati per il redirezionamento 
# dell'input. Ecco una spiegazione dettagliata delle loro differenze:

# 1. **`<` (Redirezione dell'input)**:
#    - Questo operatore viene utilizzato per redirigere l'input standard (stdin) da un file.
#    - Esempio:
#      ```bash
#      comando < file.txt
#      ```
#      In questo esempio, l'input per `comando` viene preso dal file `file.txt` invece che dalla
#      tastiera.

# 2. **`<<` (Here Document)**:
#    - Questo operatore viene utilizzato per creare un "here document", che è un tipo di redirezione 
#      che consente di fornire un blocco di input al comando.
#    - Esempio:
#      ```bash
#      comando << EOF
#      linea1
#      linea2
#      EOF
#      ```
#      In questo esempio, il comando `comando` riceve come input le righe comprese tra `<< EOF` e `EOF`.

# 3. **`<<<` (Here String)**:
#    - Questo operatore redirige una stringa come input al comando. È simile a `<<`, ma per una 
#      singola riga di input.
#    - Esempio:
#      ```bash
#      comando <<< "Questo è un input"
#      ```
#      In questo esempio, `comando` riceve come input la stringa "Questo è un input".

# 4. **`< <` (Process Substitution)**:
#    - Questo non è un singolo operatore, ma piuttosto una combinazione che viene usata nella
#      sostituzione di processi per redirigere l'output di un comando come input ad un altro comando.
#    - Esempio:
#      ```bash
#      comando1 < <(comando2)
#      ```
#      In questo esempio, l'output di `comando2` viene usato come input per `comando1`. Questo è 
#      utile quando si desidera utilizzare l'output di un comando come input per un altro senza creare un file temporaneo.

# ### Riepilogo

# - **`<`**: Redirige l'input da un file.
# - **`<<`**: Utilizza un here document per fornire più linee di input direttamente nello script.
# - **`<<<`**: Utilizza un here string per fornire una singola linea di input.
# - **`< <`**: Utilizza la sostituzione di processi per redirigere l'output di un comando come input
#              per un altro comando.

# Ognuno di questi operatori ha un caso d'uso specifico, ed è utile sapere quale utilizzare in base
# alle esigenze dello script o del comando che si sta eseguendo.