# MonoTanks Game for HackArena 2.0

This is a game written for HackArena 2.0, organized by KN init. This repo contains the game server and the GUI client. It is written in C# using [MonoGame](https://monogame.net/) framework. It can be played either by humans or by bots, for which API wrappers are provided.

Full guide to game mechanics can be found in [instruction](https://hackarena.pl/Assets/Game/HackArena%202.0%20-%20instrukcja.pdf).

## Server

### Running the server

#### Precompiled binaries (Windows)

- Download appropriate zip file for your platform from [releases](https://github.com/INIT-SGGW/HackArena2.0-MonoTanks/releases) page.
- Unzip the downloaded file.
- Run the server by double clicking on the `GameServer.exe` file.
- It will start server with default settings. To check what settings are available, type in the terminal:
    ```
    ./GameServer.exe --help
    ```

#### Precompiled binaries (Linux / macOS)

- Download appropriate zip file for your platform from [releases](https://github.com/INIT-SGGW/HackArena2.0-MonoTanks/releases) page.
- Unzip the downloaded file.
- Add execute permissions to the server executable by running:
    ```
    chmod +x GameServer
    ```
- Run the server by typing:
    ```
    ./GameServer
    ```
- It will start server with default settings. To check what settings are available, you can use `--help` flag.
    ```
    ./GameServer --help
    ```

#### Docker container

- Download appropriate zip file for your platform from [releases](https://github.com/INIT-SGGW/HackArena2.0-MonoTanks/releases) page.
- Unzip the downloaded file.
- Run the docker container
    ```bash
    docker run --rm -v ".:/app" -p "5000:5000" -it mcr.microsoft.com/dotnet/runtime:8.0 dotnet /app/GameServer.dll -- --host *
    ```

    > Running the server inside a docker container requires setting the host flag to `*` so clients can connect from outside docker.


#### Options

Server has a number of options that can be passed to it as flags. You can check what options are available by running the server with `--help` flag.

##### Join code
A join code can be set on server start to allow only players with the code to join the game.

##### Sandbox mode
Sandbox mode is a mode in which the game starts immediately after server starts and it lasts indefinitely. It is useful for testing purposes. In this mode players including bots can join and leave the game at any time without the need to restart the server. Secondary items on the map generate when there is a player in the game.

##### Eager broadcast
Eager broadcast option makes excecution of the game speed up significantly. To understand how it works, first we need to understand how the game is executed normally.

The game is running in a simple loop. Server sends to the clients the current state of the game and waits the broadcast interval time, which is for example 100ms. While waiting server is receiving what actions the players want to make and applies them to the game state. After this time of 100ms passes, server sends next state of the game to the clients.

When eager broadcast is enabled, server doesn't wait for the standard broadcast interval to pass. It sends the next state of the game to the clients immediately after receiving all the actions from all the players. If a bot does not respond within a standard interval, the server will send the next state anyway. This way the game execution is much faster, because there is no unnecessary delay between receiving all the actions and sending next state of the game.

#### Replays
When replay is enabled, server saves the game state in original form to a file on every tick. This file can be later used to replay the game or for other analysis purposes.

## GUI Client

GUI Client has 2 primary purposes:
1. Playing the game as a player to test the game mechanics.
2. Displaying the game as a spectator to watch the bots play.

### Running the GUI Client

#### Windows

- Download appropriate zip file for your platform from [releases](https://github.com/INIT-SGGW/HackArena2.0-MonoTanks/releases) page.
- Unzip the downloaded file.
- Run the client by double clicking on the `GameClient.exe` file.

#### Linux / macOS

- Download appropriate zip file for your platform from [releases](https://github.com/INIT-SGGW/HackArena2.0-MonoTanks/releases) page.
- Unzip the downloaded file.
- Add execute permissions to the client executable by running:
    ```
    chmod +x GameClient
    ```
- Run the client by typing:
    ```
    ./GameClient
    ```

#### Playing the game as a player
To play the game as a player in main menu select `JOIN` and enter:
- the nickname.
- the join code if it is set on the server.
- the url of the server. By default, if run locally, it should be `localhost:5000`.

Then press `JOIN` button.

##### Controlling the tank

- `W` - move forward.
- `S` - move backward.
- `A` - rotate left.
- `D` - rotate right.
- `Q` - rotate turret left.
- `E` - rotate turret right.
- `Space` - shoot a bullet.
- `1` - use laser.
- `2` - shoot a double bullet.
- `3` - use radar.
- `4` - drop a mine.

#### Watching the game as a spectator
To watch the game as a spectator in main menu select `JOIN` and enter:
- the join code if it is set on the server.
- the url of the server. By default, if run locally, it should be `localhost:5000`.

Then press eye icon in the top right corner.