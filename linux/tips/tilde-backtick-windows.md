# Tilde ~ e Backtick ` su Windows

Nella shell di Linux capita molto spesso di dover inserire i caratteri tilde (~) e backtick \` che non sono facilmente inseribili su una tastiera con layout italiano. Esistono almeno un paio di modi per poter inserire questi caratteri quando serve:

- Utilizzare una combinazione di tasti su Windows, sfruttando il tastierino numerico
- Utilizzare un layout aggiuntivo o modificato della tastiera italiana (in questo caso non c'è bisogno del tastierino numerico)
- Utilizzare utility come [AutoHotKey](https://www.autohotkey.com/)
  
## Tilde ~ e Backtick ` su Windows con il tastierino numerico

Questa soluzione consiste nel digitare un codice sul tastierino numerico mentre viene tenuto premuto il tasto `Alt`.

Per ottenere il carattere tilde (~) bisogna digitare il codice `126`, quindi la combinazione di tasti sarà:

    Alt + 126
    
Per ottenere il carattere backtick (\`) bisogna digitare il codice `96`, quindi la combinazione di tasti sarà:

    Alt + 96

## Tilde ~ e Backtick ` su Windows senza il tastierino numerico

Seguendo questa [guida su GitHub](https://gist.github.com/federicocarboni/36de8d690588ae729a4dc2d026173ce5) si può facilmente installare una seconda tastiera italiana con alcuni tasti modificati:
La combinazione di tasti scelta per i caratteri è la stessa trovata sui
sistemi operativi Linux, quindi il \` (backtick) si digita con AltGr + '
(Alt Gr e la virgoletta singola), e la ~ (tilde) con AltGr + ì.

### Istruzioni

- Scarica e installa [Microsoft Keyboard Layout Creator](https://www.microsoft.com/en-us/download/details.aspx?id=102134)
- Clicca su `File`, `Load Existing Keyboard...` e seleziona `Italian`
- Adesso dobbiamo modificare il Layout, seleziona il checkbox sotto `Shiftstates`, `Alt+Ctrl (AltGr)`
- Seleziona il tasto a sinistra del `Backspace` e digita la tilde usando `Alt+126`o copiala `~`
- Seleziona ora il tasto a sinistra della nuova tilde e digita il backtick usando`Alt+96` o copialo `\``
- Prima di continuare, puoi testare il nuovo layout cliccando su `Project`, `TestKeyboard- Layout...`
- Per completare il layout clicca su `Project`, `Build DLL and Setup Package`
- Ti chiederà di dare un nome ed una descrizione al layout, il nome deve essere 8caratteri o meno e non può contenere spazi
- Clicca su `OK`
- Ti chiederà se vuoi aprire i log, normalmente non è necessario
- Quando ha finito ti chiederà se vuoi aprire la cartella in cui ha messo il pacchetto di Setup
- Apri la cartella ed esegui `setup.exe`
- Quando il Setup ha finito, apri le impostazioni e cerca le impostazioni avanzate per la tastiera e seleziona il tuo layout

## Metodo alternativo con AutoHotkey

Seguendo questa [guida su GitHub](https://gist.github.com/velut/3664354aae6ff4baebca0a6fa7d71861) si può ottenere lo stesso effetto delle soluzioni precedenti, installando [AutoHotkey](https://autohotkey.com/) e definendo delle combinazioni di tasti personalizzate che, una volta premute, il programma sostituirà con i caratteri desiderati.

I passi da seguire sono i seguenti:

1. Scaricare e installare [AutoHotkey](https://autohotkey.com/).

2. In una cartella a piacere fare click destro e selezionare `Nuovo -> AutoHotkey Script`.\
   Come nome del file usare ad esempio `tilde.ahk`.

   Se la voce `AutoHotkey Script` non fosse presente nel menu `Nuovo` di Windows, scegliere `Nuovo -> Documento di testo` e rinominare il documento creato in `tilde.ahk` (**N.B.**: l'estensione del file deve essere `.ahk` e non `.txt`; controllare che Windows sia impostato per mostrare le estensioni dei file).

3. Fare click destro sul file creato e selezionare `Edit Script` per poter modificare lo script.

   Se la voce `Edit Script` non fosse presente, controllare di aver creato correttamente il file `tilde.ahk` con l'estensione `.ahk`.\
   In alternativa selezionare la voce `Apri con -> Blocco note`.

4. Se il file di script è stato correttamente creato, all'apertura del Blocco note dovrebbe essere visibile il seguente testo:

        #NoEnv  ; Recommended for performance and compatibility with future AutoHotkey releases.
        ; #Warn  ; Enable warnings to assist with detecting common errors.
        SendMode Input  ; Recommended for new scripts due to its superior speed and reliability.
        SetWorkingDir %A_ScriptDir%  ; Ensures a consistent starting directory.

5. Sotto il testo già presente copiare e incollare il seguente codice:

        ; Combinazione AltGr + ì (i accentata) per inserire il carattere tilde
        ^>!ì::~
        
        ; Combinazione AltGr + ' (apostrofo) per inserire il carattere backtick
        ^>!'::`

6. Alla fine il contenuto dello script dovrebbe essere il seguente:

        #NoEnv  ; Recommended for performance and compatibility with future AutoHotkey releases.
        ; #Warn  ; Enable warnings to assist with detecting common errors.
        SendMode Input  ; Recommended for new scripts due to its superior speed and reliability.
        SetWorkingDir %A_ScriptDir%  ; Ensures a consistent starting directory.
        
        ; Combinazione AltGr + ì (i accentata) per inserire il carattere tilde
        ^>!ì::~
        
        ; Combinazione AltGr + ' (apostrofo) per inserire il carattere backtick
        ^>!'::`

7. Salvare il file e chiudere il Blocco note.

   Se si usa un editor di testo diverso da Blocco note, impostare la codifica del file come `UTF-8 with BOM` per codificare correttamente la i accentata.

8. Fare click destro sul file di script e selezionare la voce `Run Script` o in alternativa fare doppio click sul file di script.\
   La presenza nell'area di notifica (vicino all'orologio) di un'icona con una **_H_** su sfondo verde segnala che lo script è in esecuzione.

9. Premere i tasti `AltGr + ì` per scrivere il carattere tilde e i tasti `AltGr + '` per scrivere il carattere backtick.

Per maggiori informazioni su come usare diverse combinazioni di tasti, consultare la [documentazione](https://autohotkey.com/docs/Hotkeys.htm) di AutoHotkey.

Per far sì che lo script venga eseguito ad ogni avvio di Windows seguire [questa guida](https://www.maketecheasier.com/schedule-autohotkey-startup-windows/).
