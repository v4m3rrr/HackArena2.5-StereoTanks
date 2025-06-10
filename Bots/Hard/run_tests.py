import os
import subprocess
import argparse
from concurrent.futures import ThreadPoolExecutor
import socket
import time
import datetime
import json
from collections import defaultdict

# Example how to run: python3 run_tests.py -n 20 -b basic basic -r --ticks 300
# Path constants
SERVER_PATH = "../linux-x64/GameServer"
BOT_BINARIES_PATH = "../bot_binaries"
DATA_DIR = "./data"


def find_available_port(base_port, used_ports):
    """Finds the next available port starting from base_port that is not in used_ports."""
    port = base_port
    while port in used_ports or not is_port_available(port):
        port += 1
    used_ports.add(port)
    return port


def is_port_available(port):
    """Check if a port is available."""
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as sock:
        result = sock.connect_ex(("localhost", port))
        return result != 0


def run_server(test_id, port, num_players, args, experiment_dir):
    replay_filepath = os.path.join(experiment_dir, f"replay_{test_id}.json")
    command = [
        SERVER_PATH,
        "--host",
        args.host,
        "--port",
        str(port),
        "--number-of-players",
        str(num_players),
        "--grid-dimension",
        str(args.grid_dimension),
        "--ticks",
        str(args.ticks),
        "--broadcast-interval",
        str(args.broadcast_interval),
        "--eager-broadcast",
        "--match-name",
        f"match_{test_id}",
    ]
    if args.sandbox:
        command.append("--sandbox")
    if args.replay:
        command.extend(
            [
                "--save-replay",
                "--replay-filepath",
                replay_filepath,
                "--overwrite-replay-file",
            ]
        )

    log_file = os.path.join(experiment_dir, f"server_{test_id}.log")
    with open(log_file, "w") as log:
        print(
            f"[INFO] Starting server for test {test_id} on port {port} with {num_players} players"
        )
        print(f"[COMMAND] {' '.join(command)}")
        subprocess.Popen(command, stdout=log, stderr=log)
    time.sleep(10)  # Wait for 10 seconds to allow the server to initialize properly


def run_bot(test_id, bot_name, bot_id, port, host, experiment_dir):
    # Determine team and tank type based on bot_id
    team = 1 if bot_id <= 2 else 2
    tank_type = "heavy" if bot_id % 2 == 1 else "light"
    
    command = [
        os.path.join(BOT_BINARIES_PATH, bot_name),
        "--host",
        host,
        "--port",
        str(port),
        "--team-name",
        str(team),
        "--tank-type",
        tank_type,
    ]

    log_file = os.path.join(experiment_dir, f"bot_{test_id}_{bot_id}.log")
    with open(log_file, "w") as log:
        print(
            f"[INFO] Starting bot {bot_id} ({bot_name}) as {tank_type} tank for team {team} with tank-type {tank_type} for test {test_id} on port {port}"
        )
        print(f"[COMMAND] {' '.join(command)}")
        subprocess.run(command, stdout=log, stderr=log)


def summarize_results(experiment_dir, num_tests, num_players):
    team_scores = defaultdict(int)
    draws = 0
    fails = 0
    for i in range(num_tests):
        replay_file = os.path.join(experiment_dir, f"replay_{i}.json")
        if os.path.exists(replay_file):
            try:
                with open(replay_file, "r") as file:
                    if os.stat(replay_file).st_size == 0:
                        print(f"[WARNING] Replay file {replay_file} is empty.")
                        fails += 1
                        continue

                    replay_data = json.load(file)
                    
                    # Check if replay has gameEnd section
                    if "gameEnd" not in replay_data or "teams" not in replay_data["gameEnd"]:
                        print(f"[WARNING] Replay file {replay_file} has invalid format.")
                        fails += 1
                        continue
                    
                    teams = replay_data["gameEnd"]["teams"]
                    if len(teams) < 2:
                        print(f"[WARNING] Replay file {replay_file} has insufficient team data.")
                        fails += 1
                        continue
                    
                    # Find teams by name
                    team1 = next((t for t in teams if t["name"] == "1"), None)
                    team2 = next((t for t in teams if t["name"] == "2"), None)
                    
                    if not team1 or not team2:
                        print(f"[WARNING] Could not identify teams in {replay_file}.")
                        fails += 1
                        continue
                    
                    # Get team scores directly from the data
                    team1_score = team1.get("score", 0)
                    team2_score = team2.get("score", 0)
                    
                    if team1_score == team2_score:
                        draws += 1
                    elif team1_score > team2_score:
                        team_scores["Team 1"] += 1
                    else:
                        team_scores["Team 2"] += 1

            except json.JSONDecodeError:
                print(f"[ERROR] Failed to decode JSON in replay file {replay_file}.")
                fails += 1
            except Exception as e:
                print(f"[ERROR] An unexpected error occurred with {replay_file}: {e}")
                fails += 1

    # Summarize and print results
    print("\n=== Experiment Summary ===")
    print(f"Team 1: {team_scores['Team 1']} wins")
    print(f"Team 2: {team_scores['Team 2']} wins")
    print(f"Draws: {draws} matches")
    print(f"Failed matches: {fails}")
    print(f"Total matches processed: {team_scores['Team 1'] + team_scores['Team 2'] + draws + fails}/{num_tests}")
    print("=========================")


def cleanup_processes():
    """Kills all GameServer and bot processes."""
    try:
        # Kill all GameServer processes
        subprocess.run(["pkill", "-f", "GameServer"], check=True)
        # Kill all bot processes
        subprocess.run(["pkill", "-f", "bot_binaries"], check=True)
        print("[INFO] All GameServer and bot processes have been terminated.")
    except subprocess.CalledProcessError as e:
        print(f"[ERROR] Failed to kill processes: {e}")


def main():
    parser = argparse.ArgumentParser(description="Run parallel game tests.")
    parser.add_argument("-n", type=int, default=100, help="Number of tests to run")
    parser.add_argument(
        "-b",
        "--bots",
        nargs="+",
        required=True,
        help="List of bot binaries to use (up to 4)",
    )
    parser.add_argument(
        "--host", default="localhost", help="Host address for the game servers"
    )
    parser.add_argument(
        "--grid-dimension", type=int, default=24, help="Grid dimension for the game"
    )
    parser.add_argument(
        "--ticks", type=int, default=3000, help="Number of ticks per game"
    )
    parser.add_argument(
        "--broadcast-interval",
        type=int,
        default=500,
        help="Broadcast interval in milliseconds",
    )
    parser.add_argument(
        "-r", "--replay", action="store_true", help="Save replays of matches"
    )
    parser.add_argument(
        "--sandbox", action="store_true", help="Run servers in sandbox mode"
    )
    args = parser.parse_args()

    num_bots = len(args.bots)
    if num_bots > 2:
        print("[ERROR] You can only specify up to 2 bots.")
        return

    # Create a unique directory for this experiment
    timestamp = datetime.datetime.now().strftime("%Y%m%d_%H%M%S")
    experiment_dir = os.path.join(DATA_DIR, f"experiment_{timestamp}")
    os.makedirs(experiment_dir, exist_ok=True)

    used_ports = set()
    base_port = find_available_port(5000, used_ports)

    batch_spread = 10
    for batch_start in range(0, args.n, batch_spread):
        futures = []
        with ThreadPoolExecutor(max_workers=batch_spread * (num_bots + 1)) as executor:
            # Limit to batch_spread or remaining tests, whichever is smaller
            current_batch_size = min(batch_spread, args.n - batch_start)

            test_ids = []
            ports = []

            for i in range(current_batch_size):
                test_id = batch_start + i
                port = find_available_port(base_port, used_ports)
                test_ids.append(test_id)
                ports.append(port)

            for i in range(current_batch_size):
                executor.submit(
                    run_server, test_ids[i], ports[i], num_bots*2, args, experiment_dir
                )

            time.sleep(2)
            print("[INFO] Waiting for 2 second before starting bots...")

            for i in range(current_batch_size):
                for team_idx in range(1, 3):  # Teams 1 and 2
                    bot_name = args.bots[team_idx-1] if len(args.bots) > 1 else args.bots[0]
                    
                    heavy_bot_id = (team_idx * 2) - 1
                    futures.append(
                        executor.submit(
                            run_bot,
                            test_ids[i],
                            bot_name,
                            heavy_bot_id,
                            ports[i],
                            args.host,
                            experiment_dir,
                        )
                    )
                    
                    # Create light tank (bot_id = 2 for team 1, 4 for team 2)
                    light_bot_id = team_idx * 2
                    futures.append(
                        executor.submit(
                            run_bot,
                            test_ids[i],
                            bot_name,
                            light_bot_id,
                            ports[i],
                            args.host,
                            experiment_dir,
                        )
                    )
        
        print("[INFO] Waiting for games to complete...")
        for future in futures:
            future.result()

    # Summarize results after all experiments are done
    summarize_results(experiment_dir, args.n, len(args.bots))

    # Cleanup processes after summarizing
    cleanup_processes()


if __name__ == "__main__":
    main()
