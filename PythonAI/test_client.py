# test_client.py
import requests
import random
import time

API_URL = "http://localhost:5000/decide"

# Simulate a few fake nearby visions
def generate_fake_nearby():
    terrains = ["Plains", "Forest", "Desert", "Swamp", "Mountain"]
    items = [[], ["Food Bonus"], ["Water Bonus"], ["Gold Bonus"], ["Trader"]]
    directions = ["NORTH", "SOUTH", "EAST", "WEST", "NORTHEAST", "NORTHWEST", "SOUTHEAST", "SOUTHWEST"]
    
    nearby = {}
    for dir in random.sample(directions, random.randint(2, 5)):
        nearby[dir] = {
            "terrain": random.choice(terrains),
            "items": random.choice(items),
            "costs": {
                "move": random.randint(1, 5),
                "food": random.randint(0, 3),
                "water": random.randint(0, 3)
            }
        }
    return nearby

def main():
    food = 50
    water = 40
    energy = 60
    current_position = (0, 0)

    for turn in range(10):
        nearby = generate_fake_nearby()
        
        payload = {
            "food": food,
            "water": water,
            "energy": energy,
            "nearby": nearby,
            "current_position": current_position  # <-- modified to match your engine
        }
        response = requests.post(API_URL, json=payload)

        if response.ok:
            decision = response.json()["decision"]
            print(f"Turn {turn+1}: Decision: {decision}")
        else:
            print(f"Turn {turn+1}: Error - {response.status_code}")
        
        # Simulate resource consumption
        food = max(0, food - random.randint(1, 5))
        water = max(0, water - random.randint(1, 4))
        energy = max(0, energy - random.randint(1, 6))

        time.sleep(0.5)  # small pause between turns

if __name__ == "__main__":
    main()
