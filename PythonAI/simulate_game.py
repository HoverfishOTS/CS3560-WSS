# simulate_game.py
import requests
import random
import time

API_URL = "http://localhost:5000/decide"

MAP_WIDTH = 20
MAP_HEIGHT = 5

TERRAIN_TYPES = {
    "Plains": {"move": 1, "food": 1, "water": 1},
    "Forest": {"move": 2, "food": 2, "water": 1},
    "Desert": {"move": 3, "food": 3, "water": 4},
    "Swamp": {"move": 4, "food": 2, "water": 2},
    "Mountain": {"move": 5, "food": 3, "water": 3}
}

TERRAIN_LIST = list(TERRAIN_TYPES.keys())

def generate_map(width, height):
    map_data = {}
    for x in range(width):
        for y in range(height):
            terrain = random.choice(TERRAIN_LIST)
            items = []
            if random.random() < 0.3:  # 30% chance to spawn a bonus/trader
                items.append(random.choice(["Food Bonus", "Water Bonus", "Gold Bonus", "Trader"]))
            map_data[(x, y)] = {
                "terrain": terrain,
                "items": items
            }
    return map_data

def get_nearby(map_data, position):
    x, y = position
    nearby = {}
    directions = {
        "CURRENT": (0, 0),
        "NORTH": (0, 1),
        "SOUTH": (0, -1),
        "EAST": (1, 0),
        "WEST": (-1, 0),
        "NORTHEAST": (1, 1),
        "NORTHWEST": (-1, 1),
        "SOUTHEAST": (1, -1),
        "SOUTHWEST": (-1, -1)
    }
    for dir, (dx, dy) in directions.items():
        new_pos = (x + dx, y + dy)
        if new_pos in map_data:
            tile = map_data[new_pos]
            nearby[dir] = {
                "terrain": tile["terrain"],
                "items": tile.get("items", []),
                "costs": TERRAIN_TYPES[tile["terrain"]]
            }
    return nearby

def move_position(position, direction):
    x, y = position
    moves = {
        "NORTH": (0, 1),
        "SOUTH": (0, -1),
        "EAST": (1, 0),
        "WEST": (-1, 0),
        "NORTHEAST": (1, 1),
        "NORTHWEST": (-1, 1),
        "SOUTHEAST": (1, -1),
        "SOUTHWEST": (-1, -1)
    }
    dx, dy = moves.get(direction, (0, 0))
    return (x + dx, y + dy)

def print_map(map_data, start_position):
    print("\n========== FULL MAP LAYOUT ==========")
    for y in range(MAP_HEIGHT - 1, -1, -1):
        row = ""
        for x in range(MAP_WIDTH):
            if (x, y) == start_position:
                row += "[X]"
            elif (x, y) in map_data:
                tile_info = map_data[(x, y)]
                items = tile_info.get("items", [])
                if "Trader" in items:
                    row += "[T]"
                elif "Food Bonus" in items:
                    row += "[F]"
                elif "Water Bonus" in items:
                    row += "[W]"
                elif "Gold Bonus" in items:
                    row += "[$]"
                else:
                    symbol = tile_info["terrain"][0]
                    row += f"[{symbol}]"
            else:
                row += "[ ]"
        print(row)
    print("\nLegend: X=Player Start, T=Trader, F=Food Bonus, W=Water Bonus, $=Gold Bonus, P=Plains, F=Forest, D=Desert, S=Swamp, M=Mountain")
    print("=====================================")

def reset_ai_memory():
    response = requests.post("http://localhost:5000/reset")
    if response.ok:
        print("AI Memory Reset Successfully.")
    else:
        print("Failed to reset AI memory.")

def main():
    food = 50
    water = 50
    energy = 50
    gold = 30
    position = (0, random.randint(0, MAP_HEIGHT - 1))
    map_data = generate_map(MAP_WIDTH, MAP_HEIGHT)
    start_position = position
    print_map(map_data, start_position)

    turn = 0
    move_count = 0
    outcome = "Unknown"

    while True:
        if food <= 0 or water <= 0 or energy <= 0:
            outcome = "Died (Resource Depletion)"
            break
        if position[0] >= MAP_WIDTH:
            outcome = "Escaped Successfully"
            break

        nearby_info = get_nearby(map_data, position)

        retry_attempts = 0
        MAX_RETRIES = 5

        while True:
            payload = {
                "food": food,
                "water": water,
                "energy": energy,
                "gold": gold,
                "nearby": nearby_info,
                "current_position": position,
                "map_width": MAP_WIDTH,
                "map_height": MAP_HEIGHT
            }
            response = requests.post(API_URL, json=payload)

            if not response.ok:
                print(f"Error on turn {turn}")
                outcome = "Error"
                return

            decision = response.json()["decision"]
            print(f"Turn {turn}: At {position} - Decision: {decision}")

            if decision.startswith("MOVE"):
                direction = decision.split()[1]
                new_position = move_position(position, direction)

                if new_position[0] >= MAP_WIDTH:
                    position = new_position
                    move_count += 1
                    outcome = "Escaped Successfully"
                    break
                elif new_position in map_data:
                    tile = map_data[new_position]
                    costs = TERRAIN_TYPES[tile["terrain"]]
                    food = max(0, food - costs["food"])
                    water = max(0, water - costs["water"])
                    energy = max(0, energy - costs["move"])
                    position = new_position
                    move_count += 1

                    # Collect bonuses if any
                    items = tile.get("items", [])
                    collected = False
                    if "Food Bonus" in items:
                        print("Collected Food Bonus! +5 Food")
                        food += 5
                        items.remove("Food Bonus")
                        collected = True
                    if "Water Bonus" in items:
                        print("Collected Water Bonus! +5 Water")
                        water += 5
                        items.remove("Water Bonus")
                        collected = True
                    if "Gold Bonus" in items:
                        print("Collected Gold Bonus! +5 Gold")
                        gold += 5
                        items.remove("Gold Bonus")
                        collected = True
                    if collected:
                        map_data[position]["items"] = items
                    break
                else:
                    print(f"Invalid move attempted: {direction} from {position}. Asking AI again.")
                    retry_attempts += 1
                    if retry_attempts >= MAX_RETRIES:
                        print("Too many invalid moves. Forcing REST.")
                        energy = min(50, energy + 5)
                        food = max(0, food - 1)
                        water = max(0, water - 1)
                        break
            elif decision == "REST":
                energy = min(50, energy + 5)
                food = max(0, food - 1)
                water = max(0, water - 1)
                break
            elif decision == "TRADE":
                current_tile = map_data.get(position, {})
                if "Trader" in current_tile.get("items", []):
                    if gold >= 3:
                        print("Trade completed successfully.")
                        gold -= 3
                        food += 1
                        water += 1
                    else:
                        print("Trade attempted but not enough gold. Trade failed.")
                else:
                    print("Trade attempted but no Trader present. Trade failed.")
                break

            if food <= 0 or water <= 0 or energy <= 0:
                outcome = "Died (Resource Depletion)"
                return

        turn += 1
        time.sleep(0.5)

    print("\n========== GAME REPORT ==========")
    print(f"Outcome: {outcome}")
    print(f"Total Turns: {turn}")
    print(f"Total Moves: {move_count}")
    print(f"Starting Position: {start_position}")
    print(f"Ending Position: {position}")
    print(f"Distance Traveled East: {position[0] - start_position[0]}")
    print(f"Final Food: {food}")
    print(f"Final Water: {water}")
    print(f"Final Energy: {energy}")
    print(f"Final Gold: {gold}")
    print("==================================")

if __name__ == "__main__":
    reset_ai_memory()
    main()
