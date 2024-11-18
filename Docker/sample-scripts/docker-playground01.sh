#!/usr/bin/env bash

## riferimenti per i comandi docker
# https://docs.docker.com/reference/cli/docker/ 

## Primo esempio
# https://docs.docker.com/reference/cli/docker/container/run/
# Usage:	docker container run [OPTIONS] IMAGE [COMMAND] [ARG...]
# Aliases: docker run

docker run hello-world

## Vedere i container attivi (running)
# https://docs.docker.com/reference/cli/docker/container/ls/
# Usage:	docker container ls [OPTIONS]
# Aliases:  docker container list <-> docker container ps <-> docker ps
docker ps

# vedere i container attivi (running) e non attivi (stopped)
docker ps -a

## Terminare un container - docker stop
# https://docs.docker.com/reference/cli/docker/container/stop/
# Usage:    docker container stop [OPTIONS] CONTAINER [CONTAINER...]
# Aliases:  docker stop
docker stop container_id
# oppure
docker stop container_name

# proviamo a lanciare un container con l'immagine di Ubuntu
docker run ubuntu
# nell'esempio precedente viene creato e lanciato un container con Ubuntu
# che si ferma istantaneamente, perché il comando predefinito è
# /bin/bash e siccome non c'è nessuno script passato alla bash
# il processo del container viene subito terminato

# proviamo a lanciare Ubuntu in modo che venga eseguita un'azione per
# alcuni secondi
docker run ubuntu sleep 20
# il comando precedente lancia un container in foreground con la
# shell collegata. Per avviare un container in background, senza
# collegarlo alla shell corrente occorre utilizzare il parametro -d

## Avviare un container in modalità detached
# https://docs.docker.com/reference/cli/docker/container/run/#detach
# The --detach (or -d) flag starts a container as a background process that doesn't occupy
# your terminal window. By design, containers started in detached mode exit when the root
# process used to run the container exits, unless you also specify the --rm option.
# If you use -d with --rm, the container is removed when it exits or when the daemon exits,
# whichever happens first.

docker run -d ubuntu sleep 200
# vediamo che il container è attivo
docker ps

## Eseguire un nuovo comando in un container attivo (running)
# https://docs.docker.com/reference/cli/docker/container/exec/
# Usage:	docker container exec [OPTIONS] CONTAINER COMMAND [ARG...]
# Aliases:  docker exec
# Ad esempio, se un container di Ubuntu è running, possiamo eseguire su di esso
# un comando con la sintassi:
docker exec container_id comando
# oppure
docker exec container_name comando
# ad esempio, supponendo che il container_name con l'immagine di Ubuntu sia stoic_mayer e
# che il comando sia cat /etc/hosts possiamo eseguire il comando
docker exec stoic_mayer cat /etc/hosts

# facciamo partire un container che rimane attivo dopo averlo lanciato
# lanciamo nginx (web server e reverse proxy) con l'opzione detach
docker run -d nginx
# verifichiamo che il container sia attivo
docker ps
# per terminare il container possiamo usare il comando
docker stop container_id
# oppure
docker stop container_name

## Rimuovere un container
# https://docs.docker.com/reference/cli/docker/container/rm/
# Usage:    docker container rm [OPTIONS] CONTAINER [CONTAINER...]
# Alias:    docker container remove <-> docker rm

# per rimuovere un container (sintassi abbreviata)
docker rm container_id
# oppure
docker rm container_name

# per rimuovere un container (sintassi estesa)
docker container remove
# è anche possibile rimuovere più container contemporaneamente mettendo più identificativi/nomi separati da spazi
# per gli identificativi è anche possibile specificare solo le prime tre o quattro cifre esadecimali

# attenzione! per rimuovere un container bisogna che questo sia stato precedentemente fermato (stopped)
# altrimenti si ottiene un errore

## Eseguire un container con l'opzione di rimozione automatica all'uscita del processo --rm
# https://docs.docker.com/reference/cli/docker/container/run/#rm
# By default, a container's file system persists even after the container exits. This makes
# debugging a lot easier,since you can inspect the container's final state and you retain all
# your data. If you'd like Docker to automatically clean up the container and remove the file
# system when the container exits, use the --rm flag
# Eseguiamo un task con un container ubuntu che poi verrà automaticamente rimosso
docker run -d --rm ubuntu sleep 20
docker ps
# passati i 20 secondi verifichiamo che il container sia stato rimosso
docker ps -a

## Elenco delle immagini locali sul Docker host
# https://docs.docker.com/reference/cli/docker/image/ls/
# Usage:	docker image ls [OPTIONS] [REPOSITORY[:TAG]]
# Aliases:  docker image list <-> docker images

# Elenco delle immagini docker (sintassi abbreviata)
docker images
# Elenco delle immagini docker (sintassi estesa)
docker image ls

## Rimozione di immagini
# https://docs.docker.com/reference/cli/docker/image/rm/
# Usage:	docker image rm [OPTIONS] IMAGE [IMAGE...]
# Aliases:  docker image remove <-> docker rmi

# Rimozione di una immagine docker (sintassi abbreviata)
docker rmi image_id
# oppure
docker rmi image_name

# Rimozione di una immagine docker (sintassi estesa)
docker image rm image_id
# oppure
docker image rm image_name

## Esempio di un container che ospita una web app
#
# proviamo a lanciare un container a partire dall'immagine kodekloud/simple-webapp
# il container in questione fa partire una web application che mostra una pagina
# di un colore random, oppure di un colore che viene impostato come variabile d'ambiente

# se provassimo a lanciare il container con il comando
docker run kodekloud/simple-webapp
# la nostra shell rimarrebbe collegata al container, ma anche cambiando shell e provando
# aprire il browser all'indirizzo indicato nella shell del container non vedemmo nulla

# l'output a console è
# This is a sample web application that displays a colored background.
#  A color can be specified in two ways.

#  1. As a command line argument with --color as the argument. Accepts one of red,green,blue,blue2,pink,darkblue
#  2. As an Environment variable APP_COLOR. Accepts one of red,green,blue,blue2,pink,darkblue
#  3. If none of the above then a random color is picked from the above list.
#  Note: Command line argument precedes over environment variable.

# No command line argument or environment variable. Picking a Random Color =blue
#  * Serving Flask app "app" (lazy loading)
#  * Environment: production
#    WARNING: Do not use the development server in a production environment.
#    Use a production WSGI server instead.
#  * Debug mode: off
#  * Running on http://0.0.0.0:8080/ (Press CTRL+C to quit)

# per terminare il container a cui siamo collegati basta digitare CTRL+C

# se provassimo con il comando
docker run -d kodekloud/simple-webapp
# non vedremmo comunque la pagina colorata perché il container non è collegato
# all'interfaccia di rete del nostro computer (vedremo meglio i dettagli nella sezione networking)
docker ps
docker stop container_id

# il modo corretto di eseguire l'avvio del container è
docker run -d -p 8080:8080 kodekloud/simple-webapp
# aprire il browser all'indirizzo localhost:8080
# verificare che il container è in esecuzione
docker ps
# aprire un altro container della stessa immagine su un'altra porta
docker run -d -p 8081:8080 kodekloud/simple-webapp
# aprire il browser all'indirizzo localhost:8081
# verificare che entrambi i container sono in esecuzione
docker ps
# è anche possibile passare un parametro corrispondente al colore
docker run -d -p 8082:8080 -e APP_COLOR=green kodekloud/simple-webapp
# con l'esempio appena visto abbiamo lanciato tre container della stessa immagine.

# la cosa più importante in questo esempio è nell'utilizzo del parametro -p che permette di
# effettuare il port mapping tra il container docker e il Docker Host (il nostro computer)
# il parametro -p host_post:container_port permette di associare una porta sul computer host
# con la porta su cui è in ascolto il web server all'interno del container.

# fermiamo i container associati all'immagine kodekloud/simple-webapp
docker ps
docker stop container_id1 container_id2 container_id3
# rimuoviamo i container
docker rm container_id1 container_id2 container_id3

## Lanciare un container in modalità interattiva
# https://docs.docker.com/reference/cli/docker/container/run/#interactive
# The --interactive (or -i) flag keeps the container's STDIN open, and lets you send input
# to the container through standard input.
# Ad esempio
echo "ciao mondo" | docker run --rm -i ubuntu cat
# L'opzione -i permette solo di avere accesso allo standard input del container, ma per
# poter interagire con esso occorre di solito abilitare anche l'opzione -t

## Abilitare uno pseudo-terminale (pseudo-TTY) sul container
# https://docs.docker.com/reference/cli/docker/container/run/#tty
# The --tty (or -t) flag attaches a pseudo-TTY to the container, connecting your terminal
# to the I/O streams of the container. Allocating a pseudo-TTY to the container means that
# you get access to input and output feature that TTY devices provide.
# Ad esempio, lanciamo un container ubuntu con le opzioni -it (abbreviazione di -i -t)
docker run -it --rm ubuntu
# É anche possibile lanciare un container in modalità detached con le opzioni -it
docker run -itd ubuntu
# In questo modo il container viene lanciato con le opzioni -i e -t abilitate, ma la shell
# corrente non è collegata ad esso. Sarà possibile collegarsi al container successivamente
# con un comando come docker attach, oppure sarà possibile eseguire un comando con docker exec
docker run -itd --rm ubuntu
# oppure
docker run -i -t -d --rm ubuntu
# Si noti che, nell'esempio precedente, con l'opzione -it il container non viene chiuso immediatamente
# poiché il comando predefinito è /bin/bash e il container ha lo STDIN aperto. In questo caso
# il container è in attesa che venga impartito un input sullo STDIN. Per chiudere il container
# in questo caso vedremo che esistono alcune opzioni (vedere gli esempi successivi).

# Se si prova ad eseguire il comando seguente, si vedrà che il comando eseguito dal container
# è sleep 20 e non /bin/bash e allo scadere del tempo stabilito il container va nello
# stopped e quindi viene automaticamente rimosso grazie all'opzione --rm
docker run -itd --rm ubuntu sleep 30
docker ps

## Uscita da un container
# https://phoenixnap.com/kb/exit-docker-container
# Supponiamo di far partire un container con l'opzione -it. Inoltre, per distinguere il container
# appena lanciato dagli altri eventualmente attivi, utilizziamo il parametro --name prima del nome
# dell'immagine
docker run -it --name my_server ubuntu
# Con il comando precedente ci ritroviamo come utenti root all'interno di un container di ubuntu.
# Proviamo ad eseguire qualche comando come, ad esempio:
ls -al
pwd
whoami
cat /etc/*release*
# Per uscire dal container esistono due possibilità:
# 1) uscire in maniera definitiva, chiudendo anche il container (che verrà posto nello stato stopped)
# 2) uscire senza chiudere il container

# 1) Per uscire in maniera definitiva da un container e porlo nello stato stopped basta inserire la
# sequenza CTRL+D. Se il container sta eseguendo un processo, ad esempio sta eseguendo un ping, si
# deve prima inserire CTRL+C per inviare il segnale SIGINT per fermare il processo e poi CTRL+D per
# uscire. In alternativa, è anche possibile digitare exit per uscire dal container e porlo nello stato
# stopped.

docker run -it --name my_server2 ubuntu
# 2) Per uscire da un container senza chiuderlo si utilizza la combinazione di tasti CTRL+P seguito da CTRL+Q.
# Dopo questa sequenza il container continua la sua esecuzione in background.

# ATTENZIONE! se si utilizza il terminale di VS Code è possibile che le combinazioni CTRL+P e CTRL+Q siano
# già associate a scorciatoie di VS Code. In questo caso basta connettersi con un terminale esterno a
# VS Code, utilizzando il comando docker attach

## Attach a un container
# https://docs.docker.com/reference/cli/docker/container/attach/
# Usage:	docker container attach [OPTIONS] CONTAINER
# Aliases:  docker attach
# Esempio:
# Eseguire il comando `docker ps` per vedere i container attivi
# Eseguire il comando `docker attach my_server2` per connettersi al container.
# Eseguire qualche comando nella shell del container e poi digitare CTRL+P seguito da CTRL+Q per uscire

## Opzione --detach-keys
# Sia il comando `docker run` che il comando `docker attach` hanno anche un'opzione che
# consente di effettuare l'override della sequenza di tasti per effettuare il detach dal container.
# https://docs.docker.com/reference/cli/docker/container/run/#detach-keys
# https://docs.docker.com/reference/cli/docker/container/attach/#detach-keys
# Ad esempio:
docker run -it --detach-keys=ctrl-u,ctrl-u --name my_server5 ubuntu
docker ps
docker attach --detach-keys=ctrl-u,ctrl-u my_server5
docker ps

# Rimozione di tutti i container nello stato stopped
# https://docs.docker.com/reference/cli/docker/container/prune/
# https://docs.docker.com/reference/cli/docker/container/prune/#filter
docker container prune
# oppure, se si vuole il comando che non richiede conferma (utile negli script), si utilizza la
# versione con l'opzione -f
docker container prune -f

## Docker networking - primi elementi
# Come visto nelle slide del corso su Docker, se si utilizzano le impostazioni di default di Docker,
# i container vengono creati all'interno di una network con driver di tipo `bridge`
# https://docs.docker.com/engine/network/
# https://docs.docker.com/engine/network/drivers/
# https://docs.docker.com/engine/network/drivers/bridge/
# https://docs.docker.com/engine/network/tutorials/standalone/

# In una network di tipo bridge, i container sono all'interno di una rete isolata nella quale possono
# comunicare tra di loro mediante i loro indirizzi ip privati, ma non possono comunicare direttamente
# con gli host esterni alla sottorete nella quale si trovano.
#
# In terms of Docker, a bridge network uses a software bridge which lets containers connected
# to the same bridge network communicate, while providing isolation from containers that aren't
# connected to that bridge network. The Docker bridge driver automatically installs rules in the
# host machine so that containers on different bridge networks can't communicate directly with each other.
#
# When you start Docker, a default bridge network (also called bridge) is created automatically,
# and newly-started containers connect to it unless otherwise specified. You can also create
# user-defined custom bridge networks.

# Important! -->User-defined bridge networks are superior to the default bridge network.

# Per capire le differenze tra il default bridge e le user defined bridge si veda la documentazione
# di Docker al link:
# https://docs.docker.com/engine/network/drivers/bridge/#differences-between-user-defined-bridges-and-the-default-bridge
#
# Quali sono le famiglie di indirizzi privati?
# https://whatismyipaddress.com/private-ip

# La comunicazione dei container con gli host esterni alla sottorete bridge avviene tramite NAT
# (Network Address Translation) effettuata dal Docker Host.
# https://k21academy.com/docker-kubernetes/docker-networking-different-types-of-networking-overview-for-beginners/

## Esercitazione: svolgimento della prima parte del tutorial
# https://docs.docker.com/engine/network/tutorials/standalone/#use-the-default-bridge-network

## Docker networking - port mapping
# https://docs.docker.com/engine/network/#published-ports
# By default, when you create or run a container using docker create or docker run,
# containers on bridge networks don't expose any ports to the outside world.
# Use the --publish or -p flag to make a port available to services outside the bridge network.
# This creates a firewall rule in the host, mapping a container port to a port on the Docker
# host to the outside world. Here are some examples:
# Flag value	            Description
# -p 8080:80	                Map port 8080 on the Docker host to TCP port 80 in the container.
# -p 192.168.1.100:8080:80	    Map port 8080 on the Docker host IP 192.168.1.100 to TCP port 80 in the container.
# -p 8080:80/udp	            Map port 8080 on the Docker host to UDP port 80 in the container.
# -p 8080:80/tcp -p 8080:80/udp	Map TCP port 8080 on the Docker host to TCP port 80 in the container,
#                               and map UDP port 8080 on the Docker host to UDP port 80 in the container.
#
# Warning! when you publish a container's ports it becomes available not only to the Docker host,
# but to the outside world as well.
#
# If you include the localhost IP address (127.0.0.1, or ::1) with the publish flag, only the Docker host
# and its containers can access the published container port.
#
## Ip address and hostname
# https://docs.docker.com/engine/network/#ip-address-and-hostname
# By default, the container gets an IP address for every Docker network it attaches to.
# A container receives an IP address out of the IP subnet of the network.
# The Docker daemon performs dynamic subnetting and IP address allocation for containers.
# Each network also has a default subnet mask and gateway.
#
## DNS service
# https://docs.docker.com/engine/network/#dns-services
# Containers use the same DNS servers as the host by default, but you can override this with --dns.
# By default, containers inherit the DNS settings as defined in the /etc/resolv.conf configuration file.
# Containers that attach to the default bridge network receive a copy of this file.
# Containers that attach to a custom network use Docker's embedded DNS server.
# The embedded DNS server forwards external DNS lookups to the DNS servers configured on the host.
# I dettagli sulle user defined bridge network verranno analizzati più avanti
# https://docs.docker.com/engine/network/tutorials/standalone/#use-user-defined-bridge-networks

## Esempi di container con port mapping
# Lanciamo Nginx nella sua configurazione di default, in modo che mostri solo la pagina di benvenuto
docker run --name my_web_server --rm -d -p 8080:80 nginx
# Se si apre il browser all'indirizzo localhost:8080 si vedrà la pagina di benvenuto di Nginx
# Lanciamo un container a partire dall'immagine kodekloud/simple-webapp
docker run --name my_web_app --rm -d -p 8081:8080 kodekloud/simple-webapp

## Docker storage - primi elementi
# https://docs.docker.com/engine/storage/
# By default all files created inside a container are stored on a writable container layer.
# This means that:
# The data doesn't persist when that container no longer exists, and it can be difficult to get
# the data out of the container if another process needs it.
# A container's writable layer is tightly coupled to the host machine where the container is running. You can't easily move the data somewhere else.

# Docker has two options for containers to store files on the host machine, so that the files are
# persisted even after the container stops:
# volumes
# bind mounts.
# Docker also supports containers storing files in-memory on the host machine.
# Such files are not persisted.

## Quale opzione di storage scegliere per i container docker?

# Volumes:
# Volumes are stored in a part of the host filesystem which is managed by Docker
# (/var/lib/docker/volumes/ on Linux). Non-Docker processes should not modify this part
# of the filesystem. Volumes are the best way to persist data in Docker.

# Bind mounts:
# Bind mounts may be stored anywhere on the host system. They may even be important system
# files or directories. Non-Docker processes on the Docker host or a Docker container can
# modify them at any time.

# tmpfs:
# tmpfs mounts are stored in the host system's memory only, and are never written to the
# host system's filesystem.

# Dove sono memorizzati i volumi di Docker?
# https://forums.docker.com/t/how-can-i-find-my-volumes-in-windows-11/136934
# In linux sono nella cartella /var/lib/docker/volumes/
# In Windows sono nella cartella
# \\wsl.localhost\docker-desktop-data\data\docker\volumes
# lo stesso percorso si può scrivere anche come:
# \\wsl$\docker-desktop-data\data\docker\volumes
# si veda anche https://superuser.com/questions/1726309/convert-wsl-path-to-uri-compliant-wsl-localhost

# Attenzione! --> la sintassi per il montaggio dei volumi e delle cartelle (bind mount) è cambiata
# nel tempo e, per compatibilità con i primi comandi docker, esistono più modi per connettere
# uno spazio di storage ad un container.
# Bind mounts and volumes can both be mounted into containers using the `-v` or `--volume` flag,
# but the syntax for each is slightly different. For tmpfs mounts, you can use the `--tmpfs`` flag.
# We recommend using the `--mount` flag for both containers and services, for bind mounts, volumes,
# or tmpfs mounts, as the syntax is more clear.

# Volumes
# https://docs.docker.com/engine/storage/#volumes (overview)
# https://docs.docker.com/engine/storage/volumes/ (documentazione dettagliata)
# Volumes are created and managed by Docker. You can create a volume explicitly using the docker
# volume create command, or Docker can create a volume during container or service creation.

# When you create a volume, it's stored within a directory on the Docker host. When you mount
# the volume into a container, this directory is what's mounted into the container. This is similar
# to the way that bind mounts work, except that volumes are managed by Docker and are isolated from
# the core functionality of the host machine.

# A given volume can be mounted into multiple containers simultaneously. When no running container
# is using a volume, the volume is still available to Docker and isn't removed automatically. You can
# remove unused volumes using `docker volume prune`.

# When you mount a volume, it may be named or anonymous. Anonymous volumes are given a random name
# that's guaranteed to be unique within a given Docker host. Just like named volumes, anonymous volumes
# persist even if you remove the container that uses them, except if you use the --rm flag when creating
# the container, in which case the anonymous volume is destroyed.

# If you create multiple containers after each other that use anonymous volumes, each container creates
# its own volume. Anonymous volumes aren't reused or shared between containers automatically. To share
#  an anonymous volume between two or more containers, you must mount the anonymous volume using the
# random volume ID.

# Bind mounts
# https://docs.docker.com/engine/storage/#bind-mounts (overview)
# https://docs.docker.com/engine/storage/bind-mounts/ (documentazione dettagliata)
# Bind mounts have limited functionality compared to volumes. When you use a bind mount, a file
# or directory on the host machine is mounted into a container. The file or directory is referenced
# by its full path on the host machine. The file or directory doesn't need to exist on the Docker
# host already. It is created on demand if it doesn't yet exist. Bind mounts are fast, but they rely
# on the host machine's filesystem having a specific directory structure available. If you are
# developing new Docker applications, consider using named volumes instead. You can't use Docker CLI
# commands to directly manage bind mounts.

# Important: Bind mounts allow write access to files on the host by default.

# Good use cases for volumes
# https://docs.docker.com/engine/storage/#good-use-cases-for-volumes

# Good use cases for bind mounts
# https://docs.docker.com/engine/storage/#good-use-cases-for-bind-mounts

# https://phoenixnap.com/kb/how-to-ssh-into-docker-container

## Esempi di container che utilizzano volumi o bind mount

### Setup di Nginx con un sito web
# Pagina di documentazione dell'immagine di Nginx
# https://hub.docker.com/_/nginx
# Primo esempio con bind mount e port mapping

# Supponiamo di avere le pagine web in una cartella nella wsl:
# ~/my-dev/static-sites/site-demo
# Si può prendere come esempio il sito condiviso su Teams

# Il primo esempio della documentazione di Nginx sulla pagina ufficiale di Docker Hub è
docker run --name some-nginx -v /some/content:/usr/share/nginx/html:ro -d nginx
# L'esempio mostrato è un bind mount perché viene associata la cartella /some/content
# all'interno del Docker Host (il nostro computer) alla cartella /usr/share/nginx/html
# all'interno del container di Nginx. L'opzione :ro serve nel caso in cui si voglia montare
# lo spazio di storage in sola lettura (read only).

# Modifichiamo l'esempio riportato su Docker Hub per adattarlo al nostro esempio (da eseguire nella WSL):
docker run --name my_nginx -d --rm -v ~/my-dev/static-sites/site-demo:/usr/share/nginx/html:ro -p 8080:80 nginx

# Se docker venisse lanciato da PowerShell il bind mount andrebbe scritto usando la notazione PowerShell
# per il percorso che punta alla cartella del sito web.
# Supponendo di creare una cartella in Windows (e non in WSL questa volta) in
# $env:USERPROFILE\source\repos\my-dev\static-files\site-demo

# il seguente è un comando per PowerShell o CMD:
docker run --name my_nginx2 -d --rm -v "$env:USERPROFILE\source\repos\my-dev\static-files\site-demo:/usr/share/nginx/html:ro" -p 8082:80 nginx

# Osservazione importante: quando si scrive il percorso per il bind mount occorre tener presente che
# con l'opzione `-v` oppure `--volume` il percorso è scritto nella forma:
# -v host_path:container_path dove:
# il percorso nella parte di sinistra del `:` è il percorso alla cartella sul Docker host (il nostro computer),
# il precorso nella parte destra del `:` è il percorso all'interno del container docker.

# --> Ogni percorso va scritto secondo le convenzioni specifiche dell'OS a cui si riferisce. Quindi,
# ad esempio, se il percorso al docker host è per Windows deve essere scritto usando il formalismo di Windows;
# se il percorso all'interno del container docker è riferito a un sistema Linux, va scritto secondo il
# formalismo di Linux.
# --> Nel caso di Powershell è importante mettere tutta la stringa che rappresenta il mapping tra doppi apici

# In PowerShell, oltre alla variabile d'ambiente $env:USERPROFILE (che punta alla home dell'utente corrente)
# è possibile usare anche $PWD, oppure ${PDW} (che punta alla working directory).
# In Powershell, PWD è l'alias del comando Get-Location
# In PowerShell basta scrivere `Get-Alias -Name pwd` e vedere che è in realtà è Get-Location

# Osservazione importante: per questioni di ottimizzazione delle prestazioni è raccomandato di non creare
# container con bind mount direttamente in cartelle di Windows, ma di fare il bind mount su cartelle della
# wsl2. Questo è dovuto al fatto che con un bind mount tra due sistemi operativi diversi l'accesso ai file è
# più lento. Si veda a tal proposito:
# https://www.docker.com/blog/docker-desktop-wsl-2-best-practices/
# https://code.visualstudio.com/remote/advancedcontainers/improve-performance

### Osservazione importante! --> per accedere all'interno del container di nginx dopo che è stato lanciato si potrebbe
# utilizzare l'istruzione docker attach che, come riportato dalla documentazione, "Attach local standard input,
# output, and error streams to a running container".
# Se eseguissimo il comando docker attach sul container di nginx non riusciremmo a fare molto perché il
# container viene eseguito con lo script docker-entrypoint.sh e non vedremmo nulla.
# Se avessimo fatto partire il container con anche le opzioni -it, facendo l'attach vedremmo i messaggi di log, ma
# non potremmo eseguire comandi...
# Il modo migliore per entrare nel container di nginx dopo che è stato avviato, indipendentemente dal fatto che sia
# stata utilizzata l'opzione -it, è quella di eseguire il comando:

docker exec -it container_id /bin/bash
# oppure
docker exec -it container_name /bin/bash

### Setup del database MySQL con port mapping e volumi
# L'immagine ufficiale docker di MySQL si trova al link
# https://hub.docker.com/_/mysql
# Altra documentazione sul deployment di MySQL attraverso Docker si trova sul sito ufficiale di MySQL
# https://dev.mysql.com/doc/refman/9.0/en/linux-installation-docker.html
# https://dev.mysql.com/doc/refman/9.0/en/docker-mysql-getting-started.html
# Dalla pagina di Docker hub di MySQL:
# What is MySQL?
# MySQL is the world's most popular open source database. With its proven performance, reliability and
# ease-of-use, MySQL has become the leading database choice for web-based applications, covering the
# entire range from personal projects and websites, via e-commerce and information services, all the
# way to high profile web properties including Facebook, Twitter, YouTube, Yahoo! and many more.
#
# Per far partire un container con MySQL possiamo lanciare un comando come il seguente:
docker run --name mysql-server1 -e MYSQL_ROOT_PASSWORD=my-secret-pw -d -p 3306:3306 mysql:latest
# ad esempio
docker run --name mysql-server1 -e MYSQL_ROOT_PASSWORD=root -d -p 3306:3306 mysql:latest
# oppure
docker run --name mysql-server1 -e MYSQL_ROOT_PASSWORD=my-secret-pw -d -p 3306:3306 mysql
# oppure per far partire una specifica versione
docker run --name mysql-server1 -e MYSQL_ROOT_PASSWORD=my-secret-pw -d -p 3306:3306 mysql:9.0.1

# per connetterci a MySQL possiamo procedere in diversi modi:

# 1) lanciamo una shell bash direttamente sul container del server di MySQL:
# apriamo un'altra finestra del nostro terminale su WSL (Ubuntu) e digitiamo il comando seguente
docker exec -it mysql-server1 /bin/bash
# nella shell del container di mysql eseguiamo i seguente comandi:
pwd
cat /etc/*release*
# per effettuare la connessione al server di mysql, utilizziamo il programma client di mysql
mysql -u root -p
# il comando precedente può anche essere scritto come
mysql -uroot -p
# ossia mettendo il nome utente direttamente attaccato a -u
# se volessimo inserire subito la password (ad esempio root) potremmo scrivere
mysql -uroot -proot
# questo modo di accedere è considerato insicuro se si accede direttamente sulla
# command line, ma può essere utilizzato quando si esegue uno script
# per verificare che il database dia funzionante eseguiamo la seguente query SQL:

# select user, host from mysql.user;
# Usciamo dalla sessione interattiva con mysql, digitando il comando:
# exit
# Per uscire dal container possiamo digitare exit

# 2) Effettuiamo la connessione da un applicativo client del Docker Host (il nostro PC)

# 2.1) Utilizziamo VS Code con il plugin per MySQL chiamato MySQL con ID = cweijan.vscode-mysql-client2
# effettuiamo la connessione specificando i seguenti parametri:
# host --> 127.0.0.1 oppure localhost
# port --> 3306 (è la porta utilizzata per il docker host e che è stata mappata sulla 3306 del container)
# username --> root
# password --> quella usata all'atto della creazione del container

# 2.1) Effettuiamo la connessione utilizzando mysql client della distribuzione Linux della WSL.
# Ad esempio, per Ubuntu 24.04 è possibile installare mysql-client-core con il comando sudo apt install mysql-client-core-8.0
# Dopo aver installato il pacchetto suddetto, la connessione può avvenire direttamente dalla shell della
# distribuzione Linux della WSL con il comando:
# mysql -uroot -h 127.0.0.1 -p
# In questo caso occorre inserire l'indirizzo IP del server e non il nome DNS (localhost) altrimenti la connessione
# non avrà successo.

# Importante: quando si utilizza una distribuzione WSL a scuola e si vuole accedere a internet dalla shell, ad esempio per eseguire
# il comando curl, oppure per scaricare o aggiornare un pacchetto tramite apt, oppure apt-get, occorre configurare il proxy.
# Alcune applicazioni di Linux hanno una configurazione specifica per il proxy. Si veda il file di scripting relativo alla configurazione
# del proxy in Ubuntu.

# 2.2) Utilizziamo l'applicativo MySQL Workbench installato sul nostro computer (se disponibile) e facciamo la connessione con i
# parametri richiesti (username e password) e specificando come host l'indirizzo 127.0.0.1
# MySQL Workbench può essere scaricato all'indirizzo:
# https://dev.mysql.com/downloads/workbench/
# Si potrebbe utilizzare anche il programma mysql.exe per Windows che viene distribuito nella versione di MySQL per
# Windows, ma in tal caso, occorrerebbe utilizzare una versione del client che supporta il plugin per la sicurezza
# caching_sha2_password, oppure bisogna procedere come descritto qui:
# https://chrisshennan.com/blog/fixing-authentication-plugin-cachingsha2password-cannot-be-loaded-errors

# 2.3) Effettuiamo la connessione da un altro container docker. In questo caso possiamo scegliere diverse
# opzioni:
# a) usare la stessa immagine di mysql per connettersi al server, come indicato nella documentazione
# di MySQL su Docker Hub https://hub.docker.com/_/mysql

# docker run -it --network some-network --rm mysql mysql -hsome-mysql -uexample-user -p

# Nell'istruzione precedente, l'ultima parte del comando docker è mysql -hsome-mysql -uexample-user -p
# ed è usata per lanciare il client di mysql verso un altro container dove è in esecuzione il server di mysql (mysqld)
# L'esempio precedente assume che sia stata creata una user-defined network per i container in modo che sia abilitata
# la risoluzione DNS dei nomi dei container. In alternativa, si può:

# a.1) utilizzare l'indirizzo privato del server al posto del nome host (some-host). Questo indirizzo privato può
# essere recuperato eseguendo il comando `docker container inspect` sul container su cui è in esecuzione il server di
# MySQL.
docker inspect mysql-server1
# l'estrazione dell'indirizzo ip, si può ottenere con la sintassi del linguaggio Go (con cui è scritto Docker)
docker inspect -f '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' mysql-server1
container_ip=$(docker inspect -f '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' mysql-server1)
# dove:
# -f '{{...}}':

# L'opzione -f specifica un formato personalizzato per l'output di docker inspect.
# La stringa tra apici singoli ('...') è una template string che indica come estrarre i dati dal JSON restituito da docker inspect.
# '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}':

# Questa è una Go template syntax utilizzata per navigare nel JSON e ottenere valori specifici.
# {{range ...}}: Inizia un ciclo per iterare su un array o una mappa. Qui stiamo iterando su .NetworkSettings.Networks, che contiene informazioni sulle reti del container.
# {{.IPAddress}}: All'interno del ciclo, . rappresenta l'elemento corrente (una rete), e .IPAddress estrae l'indirizzo IP di quella rete.
# {{end}}: Termina il ciclo range. Se ci sono più reti, il ciclo itera su ciascuna di esse

# oppure, mediante il comando jq - commandline JSON processor
# https://jqlang.github.io/jq/manual/
# Installazione di jq, se non già presente sulla propria distribuzione
sudo apt-get install jq
container_ip=$(docker inspect mysql-server1 | jq -r '.[0].NetworkSettings.Networks[].IPAddress')

docker run -it --name my-client --rm mysql:latest mysql -h"$container_ip" -uroot -p
# È anche possibile lanciare il container con la bash ed eseguire altri comandi prima di
# lanciare il client mysql
docker run -it --name my-client --rm -v ~/my-dev/sql-stuff:/sql-stuff mysql:latest /bin/bash

# a.2) lanciare il container docker per il client di MySQL con l'opzione --link (legacy e possibilmente da evitare), come
# descritto nella pagina della documentazione docker:
# https://docs.docker.com/engine/network/links/
# https://docs.docker.com/engine/network/links/#communication-across-links
# https://docs.docker.com/engine/network/links/#environment-variables

# b) usare un container con l'immagine di MySQL Workbench, come ad esempio
# https://hub.docker.com/r/linuxserver/mysql-workbench

# Attenzione! --> L'immagine da scaricare è abbastanza grande (dell'ordine di un paio di GB)

# Scaricare *a casa* l'immagine di My workbench con il comando
docker pull lscr.io/linuxserver/mysql-workbench:latest

# Si può far partire il container con l'istruzione seguente
docker run -d \
    --name=mysql-workbench \
    -e PUID=1000 \
    -e PGID=1000 \
    -e TZ=Etc/UTC \
    -p 3000:3000 \
    -p 3001:3001 \
    -v ~/my-dev/mysql-workbench-config:/config \
    --cap-add="IPC_LOCK" \
    --restart unless-stopped \
    lscr.io/linuxserver/mysql-workbench:latest
# Nell'istruzione precedente si è creata la cartella ~/my-dev/config nella distribuzione Linux della WSL per
# consentire il salvataggio delle impostazioni dell'applicazione MySQL Workbench.

# Per utilizzare l'applicazione si apre il browser all'indirizzo http://localhost:3000 per connessioni http, oppure all'indirizzo
# https://localhost:3001 per connessioni https (in questo caso occorre accettare il certificato privato del server)

# Per effettuare le connessioni al server di MySQL con questo container occorre fare le stesse considerazioni fatte
# per le altre modalità di accesso tra container docker. In questo caso la connessione può essere stabilita utilizzando
# l'indirizzo privato del container docker di MySQL server, oppure creando una user-define custom network e usare il nome
# del container come nome host a cui connettersi.
# Per ottenere l'indirizzo privato del container a cui ci si vuole connettere si può utilizzare il comando
docker container inspect container_id
# oppure
docker container inspect container_name

# c) usare un container con l'immagine di phpMyAdmin
# Scaricare *a casa* l'immagine di phpMyAdmin con il comando
docker pull phpmyadmin
# https://hub.docker.com/_/phpmyadmin
docker run --name phpmyadmin -d --link mysql-server1:db -p 8080:80 phpmyadmin

## Docker Networking - User defined networks (importante!)
# https://docs.docker.com/engine/network/#user-defined-networks
# You can create custom, user-defined networks, and connect multiple containers to the same network. Once connected to a
# user-defined network, containers can communicate with each other using container IP addresses or container names.
# Creiamo la rete my-net con l'istruzione seguente:
docker network create -d bridge my-net
# Verifichiamo che la rete sia stata creata con il comando
docker network ls
# Verifichiamo le proprietà della rete creata con il comando:
docker network inspect my-net
# Fermiamo e rimuoviamo i container precedentemente creati per MySQL e per le relative applicazioni client.
docker ps
docker stop id1 id2 id3
# rimuoviamo tutti i container non in esecuzione
docker container prune -f
# Verifichiamo che il volume anonimo creato per MySQL sia ancora presente:
docker volume ls
# eliminiamo il volume creato dal container di MySQL

## Eliminazione di un volume
# https://docs.docker.com/reference/cli/docker/volume/rm/
# Usage: docker volume rm [OPTIONS] VOLUME [VOLUME...]
# Aliases: docker volume remove
# Nel caso di volumi indicati tramite id, occorre specificare tutto l'identificativo e non solo le prime cifre
docker volume remove volume_id

#
## Ricreiamo i container per metterli tutti collegati alla stessa sottorete (importante!)
#
# Creiamo nuovamente un container per MySQL server, questa volta usando la rete docker creata e usando volumi con nome:
# https://docs.docker.com/engine/storage/volumes/#create-and-manage-volumes
docker volume create mysql-server1-vol
docker volume ls
docker volume inspect mysql-server1-vol

# Creiamo il container per MySQL:
docker run -d \
    --name mysql-server1 \
    --network my-net \
    --restart unless-stopped \
    -v mysql-server1-vol:/var/lib/mysql \
    -e MYSQL_ROOT_PASSWORD=root \
    -p 3306:3306 mysql:latest

# Verifichiamo le caratteristiche del container
docker container inspect mysql-server1
# Analizziamo i volumi, la rete, l'indirizzo ip e i nomi dns che sono stati utilizzati per il container
# Verifichiamo che anche la rete my-net sia stata aggiornata
docker network inspect my-net

# Lanciamo un container docker di phpMyAdmin che si connette al database
docker run -d \
    --name phpmyadmin \
    --network my-net \
    -e PMA_HOST=mysql-server1 \
    -p 8080:80 phpmyadmin

# Lanciamo un container docker di MySQL Workbench nella network my-net
docker run -d \
    --name=mysql-workbench \
    --network my-net \
    -e PUID=1000 \
    -e PGID=1000 \
    -e TZ=Etc/UTC \
    -p 3000:3000 \
    -p 3001:3001 \
    -v ~/my-dev/mysql-workbench-config:/config \
    -v ~/my-dev/sql-stuff:/sql-stuff \
    --cap-add="IPC_LOCK" \
    --restart unless-stopped \
    lscr.io/linuxserver/mysql-workbench

# Con la sottorete my-net è possibile usare i nomi dei container come nomi host al posto degli indirizzi IP
# Dopo aver aperto MySQL Workbench, nel pannello di configurazione della connessione al server di MySQL mysql-server1 è
# possibile specificare come host proprio il nome del container mysql-server1. Questo è dovuto al fatto che nelle user-defined
# custom network docker abilita il servizio DNS che risolve i nomi dei container con i rispettivi indirizzi ip
# all'interno della sottorete. Questa funzionalità non è disponibile se si utilizza la sottorete di default di docker.

## connessione al database server di MySQL tramite container per l'applicativo client.
#
# In questo caso creiamo un container a partire dall'immagine di mysql con il solo scopo di
# utilizzare l'applicativo client mysql.
docker run -it \
    --name my-client \
    --network my-net \
    --rm \
    -v ~/my-dev/sql-stuff:/sql-stuff \
    mysql:latest /bin/bash
#
# Dopo aver creato il container possiamo far partire l'applicativo client utilizzando come nome host
# del server direttamente il nome del container mysql-server1. In questo caso utilizziamo il
# meccanismo di risoluzione dei nomi dei container in indirizzi IP fornito da Docker quando si
# utilizza una "user defined network" (my-net nel nostro caso).
# Per la connessione al database server l'istruzione da eseguire è:
mysql -u root -h mysql-server1 -p
# una volta connessi al server che si trova nell'altro container (mysql-server1) è, ad esempio,
# possibile eseguire gli script contenuti nella cartella /sql-stuff con il comando source, come
# mostrato nel tutorial https://www.mysqltutorial.org/mysql-administration/execute-sql-file-in-mysql/:
#
# supponendo di avere lo script con le query SQL nel file all'indirizzo:
# /sql-stuff/path/to/scripts/script.sql
# il comando di mysql per importare un database è:
# source /sql-stuff/path/to/scripts/script.sql
# ad esempio, supponendo di avere lo script per la creazione del database classicmodels nel percorso:
# /sql-stuff/mysqlsampledatabase/mysqlsampledatabase.sql
# il comando diventa:
# source /sql-stuff/mysqlsampledatabase/mysqlsampledatabase.sql

## Creazione di un container di MariaDb
#
# DBMS MariaDb
# What is MariaDB?
# MariaDB Server is one of the most popular database servers in the world. It's made by the original developers of MySQL
# and guaranteed to stay open source. Notable users include Wikipedia, DBS Bank, and ServiceNow.
# The intent is also to maintain high compatibility with MySQL, ensuring a library binary equivalency and exact matching
# with MySQL APIs and commands. MariaDB developers continue to develop new features and improve performance to better
# serve its users
# Perché MariaDB?
# https://www.cloudways.com/blog/mariadb-vs-mysql/
# https://aws.amazon.com/compare/the-difference-between-mariadb-vs-mysql/
# Documentazione ufficiale di MariaDB
# https://mariadb.com/kb/en/
# https://mariadb.org/
# https://mariadb.com/
# https://mariadb.com/kb/en/docker-and-mariadb/
# https://mariadb.com/kb/en/installing-and-using-mariadb-via-docker/
# https://hub.docker.com/_/mariadb

# Scarichiamo MariaDB nella versione LTS (Long Term Support)
docker pull mariadb:lts
# creazione di un volume per lo storage dei dati
docker volume create mariadb-server1-vol
# verifichiamo che il volume sia stato creato correttamente
docker volume ls
docker volume inspect mariadb-server1-vol

# creazione e avvio di un container di MariaDB con configurazione minimale.
# È possibile anche configurare il riavvio automatico come descritto in:
# https://mariadb.com/kb/en/installing-and-using-mariadb-via-docker/

docker run -d \
    --name mariadb-server1 \
    --network my-net \
    --restart unless-stopped \
    -p 3306:3306 \
    -v mariadb-server1-vol:/var/lib/mysql \
    --env MARIADB_ROOT_PASSWORD=root \
    mariadb:lts

# È anche possibile usare le variabili della shell. Ad esempio, la password può essere letta da una variabile
root_password="root"
docker run -d \
    --name mariadb-server1 \
    --network my-net \
    --restart unless-stopped \
    -p 3306:3306 \
    -v mariadb-server1-vol:/var/lib/mysql \
    --env MARIADB_ROOT_PASSWORD="$root_password" \
    mariadb:lts

# verifichiamo che il container sia partito
docker ps

# Accesso a MariaDB dall'interno del container con il comando exec: permette di accedere come se avessimo il server installato localmente
# Importante! -->usiamo una shell esterna a VS Code, oppure utilizziamo
# l'opzione --detach-keys=ctrl-u,ctrl-u
docker exec -it mariadb-server1 /bin/bash

# localmente:
# mariadb -uroot -p

# oppure
# mariadb -hlocalhost -uroot -p

# Creazione di un container client per MariaDb

# Come applicativo client per il database server di MariaDb è anche possibile utilizzare
# un altro container di MariaDb collegato alla stessa subnet. In questo container verrà lanciata solo
# l'applicazione client e non anche il server.

docker run -it --rm \
    --name mariadb-client \
    --network my-net \
    mariadb:lts \
    mariadb -hmariadb-server1 -uroot -proot
# oppure
docker run -it --rm \
    --name mariadb-client \
    --network my-net \
    -v ~/my-dev/sql-stuff:/sql-stuff \
    mariadb:lts \
    mariadb -hmariadb-server1 -uroot -p$root_password

# È anche possibile lanciare un container a partire dall'immagine di mariadb e utilizzare la shell Bash
# In questo caso l'accesso al server verrà eseguito con un successivo comando interno al container.
# Questa configurazione è utile quando si vuole prima operare con la bash e poi successivamente collegarsi
# al server. Nel comando di avvio del container mostrato sotto p anche stato fatto un bind mount su una
# cartella contenente script SQL.
docker run -it --rm \
    --name mariadb-client \
    --network my-net \
    -v ~/my-dev/sql-stuff:/sql-stuff \
    mariadb:lts \
    /bin/bash

# oppure accedere al server di MariaDB con il comando:
# mariadb -u root -h mariadb-server1 -p

# per connettersi direttamente dentro al container di mariadb
docker exec -it mariadb-server1 /bin/bash

# stop del container
docker stop mariadb-server1

# esecuzione di un container già creato in precedenza
docker start mariadb-server1

# riavvio del container
docker restart mariadb-server1

# rimozione del container
docker rm mariadb-server1

# Rimozione del container e dei volumi anonimi collegati
docker rm -v mariadb-server1

# Connessione su una porta diversa da quella di default
#
# Lanciare un server di MySQL su una porta diversa da quella standard.
# Nell'esempio seguente viene lanciato un container di MySQL con un port mapping 3307:3306
docker run -d \
    --name mysql-server2 \
    --network my-net \
    --restart unless-stopped \
    -v mysql-server2-vol:/var/lib/mysql \
    -e MYSQL_ROOT_PASSWORD=root \
    -p 3307:3306 \
    mysql:latest

# Per connettersi con l'applicativo MySQL Monitor (dalla shell) occorre specificare anche il parametro
# -P port_number, come nell'esempio seguente (dalla WSL):
mysql -u root -h 127.0.0.1 -P 3307 -p
# Nota: il parametro -P permette di specificare la porta di connessione
# oppure da un altro container
mysql -u root -h mysql-server2 -P 3307 -p

# Lanciare un server di MariaDB su una porta diversa da quella standard.
# Nell'esempio seguente viene lanciato un container di MariaDB con un port mapping 3308:3306
docker run -d \
    --name mariadb-server2 \
    --network my-net \
    --restart unless-stopped \
    -p 3308:3306 \
    -v mariadb-server2-vol:/var/lib/mysql \
    --env MARIADB_ROOT_PASSWORD=root \
    mariadb:lts

# Creazione di un container di Microsoft SQL Server
# verrà mostrata tra qualche lezione ...

# Per connettersi con l'applicativo MySQL Monitor (dalla shell) occorre specificare anche il parametro
# -P port_number, come nell'esempio seguente (dalla WSL):
mariadb -u root -h 127.0.0.1 -P 3308 -p
# oppure da un altro container
mariadb -u root -h mariadb-server2 -P 3308 -p

## Backup, restore, or migrate data volumes
# https://docs.docker.com/engine/storage/volumes/#back-up-restore-or-migrate-data-volumes
## Backup di un volume
# https://docs.docker.com/engine/storage/volumes/#back-up-a-volume

# domanda: cosa succederebbe se lanciassimo un container con la notazione -v /dbdata ?
# risposta: in questo caso non essendo specificato il volume da associare alla cartella /dbdata nel
# container verrebbe creato un volume anonimo associato alla cartella /dbdata
docker run -v /dbdata --name dbstore ubuntu /bin/bash
# provare a lanciare dbstore e poi
docker ps
docker container inspect dbstore
docker volume ls
docker rm -v dbstore

# È possibile lanciare un container con un volume che non esiste ancora. In tal caso il volume verrà
# creato nel momento in cui viene lanciato il container. Ad esempio, lanciando
docker run -itd -v dbstore_volume:/dbdata --name dbstore ubuntu /bin/bash
docker attach dbstore
# 1) creare alcuni file nella cartella dbdata
# 2) uscire dal container
# 3) eventualmente si può provare a fermare (stop) e poi far ripartire (start) lo stesso container dbstore
# Per effettuare il backup di un volume si può procedere come indicato dalla documentazione di Docker:
# https://docs.docker.com/engine/storage/volumes/#back-up-a-volume
# Importante --> Creiamo un altro container con un immagine di Linux (ad esempio Ubuntu) che monta i volumi del container dbstore e,
# in aggiunta, ha un bind mount con una cartella del Docker host dove vogliamo mettere il backup di dbdata.
# Questo container è usato per lanciare il comando [tar](https://ss64.com/bash/tar.html) per creare una cartella compressa di tutto
# il contenuto della cartella dbdata del container dbstore. Il file compresso viene creato nella cartella /backup del container
# che è mappato sulla working directory.
docker run --rm --volumes-from dbstore -v $(pwd):/backup ubuntu tar cvf /backup/backup.tar /dbdata

## Configurazioni del DBMS
#
# Lettura delle impostazioni del DBMS Server - MariaDb
#
# Le impostazioni del server, descritte nella documentazione ufficiale di MariaDb:
# https://mariadb.com/kb/en/server-system-variables/
# https://mariadb.com/kb/en/configuring-mariadb-with-option-files/
# possono essere consultate in due modi:
# 1. dal client di MariaDb
#   SHOW VARIABLES \G
#   oppure:
#   SELECT * FROM INFORMATION_SCHEMA.GLOBAL_VARIABLES \G

# 2. dalla shell del sistema operativo (dall'interno del container Docker) si potrà, ad esempio, vedere la configurazione del server, con il comando:
#   mariadbd --verbose --help | grep time_zone
#
# 3. dall'applicazione client (monitor)
# https://stackoverflow.com/questions/930900/how-do-i-set-the-time-zone-of-mysql
# SELECT @@global.time_zone;
#
# Impostazioni del DBMS - MariaDb
# Le impostazioni del DBMS possono essere eseguite in diversi modi:

# 1. alla partenza del server (mariadbd) come command line argument
# https://mariadb.com/kb/en/server-system-variables/#setting-server-system-variables
# Ad esempio, dalla shell del container di MariaDb si può eseguire il comando mariadbd --default-time-zone="+02:00"
# Se si utilizza un container Docker, si può, ad esempio, lanciare il container di prova come mostrato di seguito (questo container verrà rimosso automaticamente quando verrà fermato):
docker run -d --rm \
    --name mariadb-server-test \
    --network my-net \
    -p 3309:3306 \
    --env MARIADB_ROOT_PASSWORD=root \
    mariadb:lts --default-time-zone "+02:00"

# 2. Utilizzando un file di configurazione .cnf, come mostrato di seguito
#
## Creazione di un container di MariaDb con configurazione diversa da quella minimale.
#
# In alcune circostanze occorre creare un container di MariaDb con configurazioni diverse da quelle predefinite.
# MariaDb ha un sistema di configurazioni che permettono di definire il comportamento del DBMS in base alle proprie esigenze.
# La configurazione di MariaDb può essere cambiata, seguendo la documentazione ufficiale:
# https://mariadb.com/kb/en/configuring-mariadb-with-option-files/
# https://mariadb.com/kb/en/server-system-variables/
# Alla partenza, MariaDb legge una serie di configurazioni da alcuni file che sono specificati nella documentazione:
# https://mariadb.com/kb/en/configuring-mariadb-with-option-files/#default-option-file-locations-on-linux-unix-mac

# Seguendo le indicazioni riportate nella documentazione dell'immagine Docker https://hub.docker.com/_/mariadb
# nella sezione "Using a custom MariaDB configuration file", si deduce che è possibile impostare parametri specifici di configurazione
# scrivendo, un file con estensione .cnf che deve essere montato in sola lettura nella cartella /etc/mysql/conf.d
# Infatti i file di configurazione messi nella cartella /etc/mysql/conf.d vengono incorporati nella configurazione generale di MariaDb richiamata
# dal file /etc/mysql/my.cnf

# Come esempio di configurazione del DBMS creiamo un file di testo chiamato my-config.cnf in una cartella della WSL Ubuntu,
# ad esempio ~/my-dev/mariadb-config/mariadb-server1. La cartella ~/my-dev/mariadb-config/mariadb-server1 verrà collegata in bind mount
# con la cartella /etc/mysql/conf.d. Siccome il file /etc/mysql/my.cnf richiama eventuali file di configurazioni presenti nella cartella
# /etc/mysql/conf.d, l'effetto complessivo sarà quello di applicare le configurazioni del file my-config.cnf, assieme alle altre
# configurazioni del DBMS.

# Nel file my-config.cnf inseriamo il seguente contenuto:
# [server]
# default-time-zone=+02:00

# L'impostazione inserita consente di aver il time zone di MariaDb impostato sull'ora di Roma.
# Si può verificare che, senza questa configurazione, il server è impostato su UTC 00:00.
# come verifica si può eseguire l'istruzione SELECT NOW();  e vedere il risultato

# Per applicare le impostazioni fermiamo il server di mariadb (nel caso fosse in esecuzione)
docker stop mariadb-server1
# Rimuoviamo il container
docker rm mariadb-server1
# Facciamo ripartire un nuovo container con le impostazioni inserite nel file my-config.cnf con il seguente comando:
docker run -d \
    --name mariadb-server1 \
    --network my-net \
    --restart unless-stopped \
    -p 3306:3306 \
    -v mariadb-server1-vol:/var/lib/mysql \
    -v ~/my-dev/mariadb-config/mariadb-server1:/etc/mysql/conf.d:ro \
    --env MARIADB_ROOT_PASSWORD=root \
    mariadb:lts

# 3. Utilizzando l'applicazione client mariadb (come utente amministrativo)
# https://stackoverflow.com/questions/930900/how-do-i-set-the-time-zone-of-mysql
#   SET GLOBAL time_zone='+02:00';

## Configurazioni del DBMS
#
# Lettura delle impostazioni del DBMS Server - MySQL
#
# Le impostazioni del server, descritte nella documentazione ufficiale di MySQL:
# https://dev.mysql.com/doc/refman/9.0/en/server-configuration.html
# https://dev.mysql.com/doc/refman/9.0/en/server-options.html
# https://dev.mysql.com/doc/refman/9.0/en/server-system-variables.html
# possono essere consultate in due modi:
# 1. dal client di MySQL
#   SHOW VARIABLES \G

# 2. dalla shell del sistema operativo (dall'interno del container Docker) si potrà, ad esempio, vedere la configurazione del server, con il comando:
#   mysqld --verbose --help

#
# Impostazioni del DBMS - MySQL
# Le impostazioni del DBMS possono essere eseguite in diversi modi:

# 1. alla partenza del server (mysqld) come command line argument
# https://dev.mysql.com/doc/refman/9.0/en/server-options.html
# Ad esempio, dalla shell del container di MySQL si può eseguire il comando mysqld --default-time-zone="+02:00"
# Se si utilizza un container Docker, si può, ad esempio, lanciare il container di prova come mostrato di seguito (questo container verrà rimosso automaticamente quando verrà fermato):
docker run -d --rm \
    --name mysql-server-test \
    --network my-net \
    -e MYSQL_ROOT_PASSWORD=root \
    -p 3307:3306 \
    mysql:latest --default-time-zone "+02:00"

# 2. Utilizzando un file di configurazione .cnf, come mostrato di seguito
#
#
# Creazione di un container di MySQL con configurazione diversa da quella minimale.
#
# In alcune circostanze occorre creare un container di MySQL con configurazioni diverse da quelle predefinite.
# MySQL ha un sistema di configurazioni che permettono di definire il comportamento del DBMS in base alle proprie esigenze.
# La configurazione di MySQL può essere cambiata, seguendo la documentazione ufficiale:
# https://dev.mysql.com/doc/refman/9.0/en/server-configuration.html
# https://dev.mysql.com/doc/refman/9.0/en/server-options.html
# https://dev.mysql.com/doc/refman/9.0/en/server-system-variables.html
# https://hub.docker.com/_/mysql

# Seguendo le indicazioni riportate nella documentazione dell'immagine Docker https://hub.docker.com/_/mysql
# nella sezione "Using a custom MySQL configuration file", si deduce che è possibile impostare parametri specifici di configurazione
# scrivendo, un file con estensione .cnf che deve essere montato in sola lettura nella cartella /etc/mysql/conf.d
# Infatti i file di configurazione messi nella cartella /etc/mysql/conf.d vengono incorporati nella configurazione generale di MySQL richiamata
# dal file /etc/mysql/my.cnf

# Come esempio di configurazione del DBMS creiamo un file di testo chiamato my-config.cnf in una cartella della WSL Ubuntu,
# ad esempio ~/my-dev/mysql-config/mysql-server1. La cartella ~/my-dev/mariadb-config/mariadb-server1 verrà collegata in bind mount
# con la cartella /etc/mysql/conf.d. Siccome il file /etc/mysql/my.cnf richiama eventuali file di configurazioni presenti nella cartella
# /etc/mysql/conf.d, l'effetto complessivo sarà quello di applicare le configurazioni del file my-config.cnf, assieme alle altre
# configurazioni del DBMS.

# Nel file my-config.cnf inseriamo il seguente contenuto:
# [server]
# default-time-zone=+02:00

# L'impostazione inserita consente di aver il time zone di MariaDb impostato sull'ora di Roma.
# Si può verificare che, senza questa configurazione, il server è impostato su UTC 00:00.
# come verifica si può eseguire l'istruzione SELECT NOW();  e vedere il risultato.

# Per applicare le impostazioni fermiamo i server che fossero eventualmente in ascolto sulla stessa porta del Docker host (nel caso fosse in esecuzione)

# lanciamo il server di MySQL e verifichiamo che l'impostazione inserita nel file di configurazione sia stata accettata correttamente
docker run -d \
    --name mysql-server1 \
    --network my-net \
    --restart unless-stopped \
    -v mysql-server1-vol:/var/lib/mysql \
    -v ~/my-dev/mysql-config/mysql-server1:/etc/mysql/conf.d:ro \
    -e MYSQL_ROOT_PASSWORD=root \
    -p 3306:3306 \
    mysql:latest

# 3. utilizzando un comando dall'applicazione client come utente amministrativo:
# https://stackoverflow.com/questions/930900/how-do-i-set-the-time-zone-of-mysql
# SET @@global.time_zone = '+02:00'