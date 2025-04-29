# decision_engine.py
from openai import OpenAI
from memory_manager import MemoryManager

class DecisionEngine:
    def __init__(self, base_url="http://localhost:8000/v1", model="meta-llama/Llama-3.1-8B-Instruct"):
        self.client = OpenAI(api_key="EMPTY", base_url=base_url)
        self.model = model
        self.memory = MemoryManager()

    def make_decision(self, food, water, energy, nearby_info, visible_terrain, current_position=(0, 0),
                      map_width=10, map_height=5, temperature=0.0):
        
        # Update memory with the full 5x3 matrix
        self.memory.update_seen_matrix(current_position, visible_terrain)

        x, y = current_position
        state_summary = f"Food: {food}, Water: {water}, Energy: {energy}, Current Position: ({x}, {y})"

        memory_context = self.memory.get_recent_context()
        tile_summary = self.memory.summarize_seen_map(current_position)
        vision_summary = self.summarize_visible_terrain(visible_terrain)

        prompt = f"""
You are the brain of a survival game player navigating a wilderness map.

GAME RULES:
- The goal is to move from the west side (x=0) to the east side (x={map_width}) of the map.
- The map dimensions are {map_width} squares wide and {map_height} squares tall.
- Moving consumes FOOD, WATER, and ENERGY depending on terrain costs.
- You cannot move outside the map boundaries.
- If you REST, you regain 2 ENERGY but still consume some food and water.
- You can TRADE with Traders if present, but it costs 3 GOLD.
- You can only TRADE if you are currently standing on a Trader tile.
- If you have less than 3 GOLD, you cannot TRADE.
- Reaching the east side (x >= {map_width}) means you have escaped and won.

PLAYER STATE:
{state_summary}

RECENT MEMORY (last few turns):
{memory_context}

WHAT YOU REMEMBER SEEING:
{tile_summary}

WHAT YOU CURRENTLY SEE:
{vision_summary}

Choose ONLY ONE action:
- MOVE <DIRECTION> (NORTH, SOUTH, EAST, WEST, NORTHEAST, NORTHWEST, SOUTHEAST, SOUTHWEST)
- REST
- TRADE
Respond with just one valid action.
"""

        response = self.client.chat.completions.create(
            model=self.model,
            messages=[
                {"role": "system", "content": "Respond with only one valid game action."},
                {"role": "user", "content": prompt}
            ],
            temperature=temperature,
            max_tokens=50
        )

        raw = response.choices[0].message.content.strip()
        decision = self._extract_action(raw)
        self.memory.add_turn(state_summary, decision)
        return decision

    def _extract_action(self, text):
        for line in text.strip().splitlines():
            if any(line.upper().startswith(cmd) for cmd in ["MOVE", "REST", "TRADE"]):
                return line.strip().upper()
        return "REST"  # fallback if nothing valid is returned

    def summarize_visible_terrain(self, visible_terrain):
        if not visible_terrain:
            return "No visible terrain."

        rows = []
        for y, row in enumerate(visible_terrain):
            row_summary = []
            for x, tile in enumerate(row):
                if tile is None:
                    row_summary.append("[???]")
                    continue

                terrain = tile.get("terrain", "Unknown")
                move_cost = tile.get("move_cost", "?")
                food_cost = tile.get("food_cost", "?")
                water_cost = tile.get("water_cost", "?")
                items = tile.get("items", [])
                items_str = ",".join(items) if items else "No Items"

                tile_info = f"{terrain} (Move: {move_cost}, Food: {food_cost}, Water: {water_cost}, Items: {items_str})"
                row_summary.append(tile_info)
            rows.append(" | ".join(row_summary))
        
        return "\n".join(rows)
