# decision_engine.py
import requests
import json
from memory_manager import MemoryManager

class DecisionEngine:
    def __init__(self, base_url="http://localhost:11434", model="llama3"):
        self.base_url = base_url
        self.model = model
        self.memory = MemoryManager()

    def make_decision(self, food, water, energy, nearby_info, current_position=(0, 0),
                      map_width=10, map_height=5, temperature=0.0):
        """Standard brain that balances survival and exploration"""
        self.memory.update_seen_map(current_position, nearby_info)

        x, y = current_position
        state = f"Food: {food}, Water: {water}, Energy: {energy}, Current Position: ({x}, {y}), Nearby: {nearby_info}"
        memory_context = self.memory.get_recent_context()
        tile_summary = self.memory.summarize_seen_map(current_position)

        prompt = f"""
        You are the brain of a survival game player navigating a wilderness map. You must make ONE SINGLE TURN.

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

        STRATEGY GUIDELINES:
        - Your primary goal is to reach the east side (x >= {map_width}).
        - ALWAYS REST when energy is below 5 to avoid exhaustion.
        - Prefer moving EAST when possible and energy is sufficient (above 5).
        - Each move costs at least 1 ENERGY, more for difficult terrain.
        - Consider resting even at higher energy if upcoming terrain looks difficult.
        - Balance progress with resource management.

        Below is your recent memory of past turns:
        {memory_context}

        Below is what you currently see:
        {state}

        Below is what you remember seeing in the past:
        {tile_summary}

        IMPORTANT: Respond with ONLY ONE LINE containing your chosen action:
        - MOVE <DIRECTION> (NORTH, SOUTH, EAST, WEST, NORTHEAST, NORTHWEST, SOUTHEAST, SOUTHWEST)
        - REST
        - TRADE
        """

        return self._get_llm_response(prompt, temperature)

    def make_decision_survivalist(self, food, water, energy, nearby_info, current_position=(0, 0),
                      map_width=10, map_height=5, temperature=0.0):
        """Specialized brain focused on resource management and survival"""
        self.memory.update_seen_map(current_position, nearby_info)

        x, y = current_position
        state = f"Food: {food}, Water: {water}, Energy: {energy}, Current Position: ({x}, {y}), Nearby: {nearby_info}"
        memory_context = self.memory.get_recent_context()
        tile_summary = self.memory.summarize_seen_map(current_position)

        prompt = f"""
        You are a SURVIVALIST brain navigating a wilderness map. Your primary focus is on resource management and survival. You must make ONE SINGLE TURN.

        GAME RULES:
        - The goal is to move from the west side (x=0) to the east side (x={map_width}) of the map.
        - The map dimensions are {map_width} squares wide and {map_height} squares tall.
        - Moving consumes FOOD, WATER, and ENERGY depending on terrain costs.
        - If you move outside the map boundaries, the move is INVALID.
        - If you REST, you regain 2 ENERGY but still consume some food and water.
        - You can TRADE with Traders if present, but trading costs 3 GOLD.

        SURVIVALIST PRIORITIES:
        1. ALWAYS REST when energy is below 6 (you are a cautious survivalist)
        2. Maintain resources above 30% (Food & Water)
        3. REST preemptively if you see difficult terrain ahead
        4. Find the safest path east when well-rested (energy > 6)
        5. Make strategic trades when resources are low
        6. Avoid dangerous terrain unless absolutely necessary
        7. Consider resting at higher energy levels if resources allow

        Current State:
        {state}

        Recent Memory:
        {memory_context}

        Terrain Knowledge:
        {tile_summary}

        IMPORTANT: Respond with ONLY ONE LINE containing your chosen action:
        - MOVE <DIRECTION> (NORTH, SOUTH, EAST, WEST, NORTHEAST, NORTHWEST, SOUTHEAST, SOUTHWEST)
        - REST
        - TRADE
        """

        return self._get_llm_response(prompt, temperature)

    def make_decision_explorer(self, food, water, energy, nearby_info, current_position=(0, 0),
                      map_width=10, map_height=5, temperature=0.0):
        """Specialized brain focused on exploration and bonus collection"""
        self.memory.update_seen_map(current_position, nearby_info)

        x, y = current_position
        state = f"Food: {food}, Water: {water}, Energy: {energy}, Current Position: ({x}, {y}), Nearby: {nearby_info}"
        memory_context = self.memory.get_recent_context()
        tile_summary = self.memory.summarize_seen_map(current_position)

        prompt = f"""
        You are an EXPLORER brain navigating a wilderness map. Your focus is on efficient exploration and bonus collection. You must make ONE SINGLE TURN.

        GAME RULES:
        - The goal is to move from the west side (x=0) to the east side (x={map_width}) of the map.
        - The map dimensions are {map_width} squares wide and {map_height} squares tall.
        - Moving consumes FOOD, WATER, and ENERGY depending on terrain costs.
        - If you move outside the map boundaries, the move is INVALID.
        - If you REST, you regain 2 ENERGY but still consume some food and water.
        - You can TRADE with Traders if present, but trading costs 3 GOLD.

        EXPLORER PRIORITIES:
        1. REST when energy is below 4 (you take more risks than survivalist)
        2. Check unvisited squares when energy is above 4
        3. Collect nearby bonuses if energy allows
        4. Move east when no interesting options nearby
        5. Trade with traders when beneficial
        6. REST before attempting difficult terrain exploration
        7. Stay in current square and REST if surrounded by difficult terrain

        Current State:
        {state}

        Recent Memory:
        {memory_context}

        Terrain Knowledge:
        {tile_summary}

        IMPORTANT: Respond with ONLY ONE LINE containing your chosen action:
        - MOVE <DIRECTION> (NORTH, SOUTH, EAST, WEST, NORTHEAST, NORTHWEST, SOUTHEAST, SOUTHWEST)
        - REST
        - TRADE
        """

        return self._get_llm_response(prompt, temperature)

    def _get_llm_response(self, prompt, temperature=0.0):
        response = requests.post(
            f"{self.base_url}/api/generate",
            json={
                "model": self.model,
                "prompt": prompt,
                "stream": False,
                "options": {
                    "temperature": temperature
                }
            }
        )

        if response.status_code != 200:
            print(f"Error from Ollama API: {response.text}")
            return "REST"

        raw = response.json()["response"].strip()
        decision = self._extract_action(raw)
        self.memory.add_turn(raw, decision)
        return decision

    def _extract_action(self, text):
        for line in text.strip().splitlines():
            if any(line.upper().startswith(cmd) for cmd in ["MOVE", "REST", "TRADE"]):
                return line.strip().upper()
        return "REST"
