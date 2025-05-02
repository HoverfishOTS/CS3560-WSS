class MemoryManager:
    def __init__(self):
        self.history = []  # Past turns
        self.seen_map = {}  # (x, y) -> {'terrain': str, 'items': list, 'costs': dict}
        self.move_history = []  # List of past decisions
        self.terrain_stats = {}  # terrain_type -> {'move_cost', 'food_cost', 'water_cost'}

    def add_turn(self, state, decision):
        entry = f"Turn:\nState: {state}\nDecision: {decision}"
        self.history.append(entry)
        self.move_history.append(decision)

    def summarize_seen_map(self, current_position):
        lines = []
        x0, y0 = current_position
        for (x, y), info in self.seen_map.items():
            dx = x - x0
            dy = y - y0
            terrain = info.get('terrain', 'Unknown')
            move_cost = info.get('costs', {}).get('move', '?')
            food_cost = info.get('costs', {}).get('food', '?')
            water_cost = info.get('costs', {}).get('water', '?')
            items = info.get('items', [])
            items_str = ", ".join(items) if items else "None"
            lines.append(f"Tile at ({dx:+}, {dy:+}): {terrain} (Move: {move_cost}, Food: {food_cost}, Water: {water_cost}, Items: {items_str})")
        return "\n".join(lines[:30])  # Limit to avoid overloading prompt

    def update_seen_map(self, current_position, nearby_info):
        """
        (Legacy) Update the memory with new tiles seen through nearby_info dictionary.
        Args:
            current_position: (x, y) tuple of the player's position.
            nearby_info: dict of direction -> tile data.
        """
        directions = {
            "NORTH": (0, 1),
            "SOUTH": (0, -1),
            "EAST": (1, 0),
            "WEST": (-1, 0),
            "NORTHEAST": (1, 1),
            "NORTHWEST": (-1, 1),
            "SOUTHEAST": (1, -1),
            "SOUTHWEST": (-1, -1),
            "CURRENT": (0, 0)
        }
        x, y = current_position
        for direction, info in nearby_info.items():
            dx, dy = directions.get(direction.upper(), (0, 0))
            coord = (x + dx, y + dy)
            self.seen_map[coord] = info

    def update_seen_matrix(self, current_position, visible_terrain):
        """
        Update memory with a 5x3 vision matrix around the player.
        visible_terrain is a 5x3 list of tile data dictionaries or None.
        """
        if not visible_terrain or not isinstance(visible_terrain, list):
            print("Warning: visible_terrain is missing or invalid — skipping memory update.")
            return

        x0, y0 = current_position

        for y in range(len(visible_terrain)):  # 0 to 4
            row = visible_terrain[y]
            if not isinstance(row, list):
                continue

            for x in range(len(row)):  # 0 to 2
                tile = row[x]
                if tile is None:
                    continue

                dx = x  # since player is at (2,0) in matrix
                dy = -(2 - y)  # matrix top-left origin

                world_x = x0 + dx
                world_y = y0 + dy

                self.seen_map[(world_x, world_y)] = {
                    "terrain": tile.get("terrain", "Unknown"),
                    "items": tile.get("items", []),
                    "costs": {
                        "move": tile.get("move_cost", "?"),
                        "food": tile.get("food_cost", "?"),
                        "water": tile.get("water_cost", "?")
                    },
                    "bonuses": {
                        "food": tile.get("food_bonus", 0),
                        "food_repeating": tile.get("food_repeating", False),
                        "water": tile.get("water_bonus", 0),
                        "water_repeating": tile.get("water_repeating", False),
                        "gold": tile.get("gold_bonus", 0),
                        "trader": tile.get("has_trader", False)
                    }
                }

    def set_terrain_stats(self, terrain_type, move_cost, food_cost, water_cost):
        self.terrain_stats[terrain_type] = {
            "move_cost": move_cost,
            "food_cost": food_cost,
            "water_cost": water_cost
        }

    def get_tile_info(self, coord):
        return self.seen_map.get(coord, None)

    def get_recent_context(self, max_turns=5):
        return "\n\n".join(self.history[-max_turns:])

    def reset(self):
        self.history.clear()
        self.seen_map.clear()
        self.move_history.clear()
        self.terrain_stats.clear()

    def dump_memory(self):
        print("\n=== Move History ===")
        for i, move in enumerate(self.move_history):
            print(f"Turn {i+1}: {move}")
        
        print("\n=== Seen Map ===")
        for coord, info in sorted(self.seen_map.items()):
            print(f"Tile {coord}: Terrain={info.get('terrain')}, Items={info.get('items')}, Costs={info.get('costs')}")
        
        print("\n=== Terrain Stats ===")
        for terrain, stats in self.terrain_stats.items():
            print(f"Terrain {terrain}: {stats}")
