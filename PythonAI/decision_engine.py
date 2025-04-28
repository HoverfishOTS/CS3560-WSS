# decision_engine.py
from openai import OpenAI
from memory_manager import MemoryManager

class DecisionEngine:
    def __init__(self, base_url="http://localhost:8000/v1", model="meta-llama/Llama-3.1-8B-Instruct"):
        self.client = OpenAI(api_key="EMPTY", base_url=base_url)
        self.model = model
        self.memory = MemoryManager()

    def make_decision(self, food, water, energy, nearby_info, current_position=(0, 0),
                      map_width=10, map_height=5, temperature=0.0):
        self.memory.update_seen_map(current_position, nearby_info)

        x, y = current_position
        state = f"Food: {food}, Water: {water}, Energy: {energy}, Current Position: ({x}, {y}), Nearby: {nearby_info}"
        memory_context = self.memory.get_recent_context()
        tile_summary = self.memory.summarize_seen_map(current_position)

        prompt = f"""
        You are the brain of a survival game player navigating a wilderness map.

        GAME RULES:
        - The goal is to move from the west side (x=0) to the east side (x={map_width}) of the map.
        - The map dimensions are {map_width} squares wide and {map_height} squares tall.
        - Moving consumes FOOD, WATER, and ENERGY depending on terrain costs.
        - If you move outside the map boundaries, the move is INVALID, and you must choose again.
        - If you REST, you regain 2 ENERGY but still consume some food and water.
        - You can TRADE with Traders if present, but trading costs 3 GOLD.
        - You can only TRADE if you are currently on a tile with a Trader.
        - If you have less than 3 GOLD, you cannot TRADE.
        - Reaching the east side (x >= {map_width}) means you have successfully escaped.

        Below is your recent memory of past turns:
        {memory_context}

        Below is what you currently see:
        {state}

        Below is what you remember seeing in the past:
        {tile_summary}

        Choose ONLY ONE action:
        - MOVE <DIRECTION> (NORTH, SOUTH, EAST, WEST, NORTHEAST, NORTHWEST, SOUTHEAST, SOUTHWEST)
        - REST
        - TRADE
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
        self.memory.add_turn(state, decision)
        return decision

    def _extract_action(self, text):
        for line in text.strip().splitlines():
            if any(line.upper().startswith(cmd) for cmd in ["MOVE", "REST", "TRADE"]):
                return line.strip().upper()
        return "REST"
