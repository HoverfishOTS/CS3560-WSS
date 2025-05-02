# decision_engine.py
import os
import logging
from openai import OpenAI
from memory_manager import MemoryManager

logging.basicConfig(
    filename="llm_decision_log.txt",
    level=logging.INFO,
    format="%(asctime)s %(levelname)s: %(message)s"
)

class DecisionEngine:
    def __init__(self, base_url="http://localhost:8000/v1", model="meta-llama/Llama-3.1-8B-Instruct", prompt_file="prompts/default.txt"):
        self.client = OpenAI(api_key="EMPTY", base_url=base_url)
        self.model = model
        self.memory = MemoryManager()
        self.prompt_template = self.load_prompt_template(prompt_file)

    def load_prompt_template(self, filename):
        path = os.path.join(os.path.dirname(__file__), filename)
        try:
            with open(path, "r", encoding="utf-8") as f:
                return f.read()
        except FileNotFoundError:
            print(f"[Warning] Prompt file {filename} not found.")
            return ""

    def make_decision(self, food, water, energy, nearby_info, visible_terrain, current_position=(0, 0),
                      map_width=10, map_height=5, temperature=0.0):
        
        self.memory.update_seen_matrix(current_position, visible_terrain)

        x, y = current_position
        state_summary = f"Food: {food}, Water: {water}, Energy: {energy}, Current Position: ({x}, {y})"

        memory_context = self.memory.get_recent_context()
        tile_summary = self.memory.summarize_seen_map(current_position)
        vision_summary = self.summarize_visible_terrain(visible_terrain)

        # Fill in prompt from template
        prompt = self.prompt_template.format(
            map_width=map_width,
            map_height=map_height,
            state_summary=state_summary,
            memory_context=memory_context,
            tile_summary=tile_summary,
            vision_summary=vision_summary
        )

        response = self.client.chat.completions.create(
            model=self.model,
            messages=[
                {"role": "system", "content": "Respond with only one valid game action."},
                {"role": "user", "content": prompt}
            ],
            temperature=temperature,
            max_tokens=100
        )

        raw = response.choices[0].message.content.strip()
        decision = self._extract_action(raw)
        self.memory.add_turn(state_summary, decision)
        return decision

    def _extract_action(self, text):
        lines = text.strip().splitlines()
        command = None
        reason = None

        for line in lines:
            if any(line.upper().startswith(cmd) for cmd in ["MOVE", "REST", "TRADE"]):
                command = line.strip().upper()
            elif line.lower().startswith("reason:"):
                reason = line[len("reason:"):].strip()

        if command is None:
            command = "REST"

        if reason:
            print(f"[LLM Reasoning] {reason}")
            logging.info(f"Decision: {command} | Reason: {reason}")
        else:
            print("[LLM Reasoning] No explanation provided.")
            logging.info(f"Decision: {command} | No explanation.")

        return command

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
                items_str = ", ".join(items) if items else "No Items"

                bonus_parts = []

                fb = tile.get("food_bonus", 0)
                if fb > 0:
                    rep = " (Repeating)" if tile.get("food_repeating", False) else ""
                    bonus_parts.append(f"+{fb} Food{rep}")

                wb = tile.get("water_bonus", 0)
                if wb > 0:
                    rep = " (Repeating)" if tile.get("water_repeating", False) else ""
                    bonus_parts.append(f"+{wb} Water{rep}")

                gb = tile.get("gold_bonus", 0)
                if gb > 0:
                    bonus_parts.append(f"+{gb} Gold")

                if tile.get("has_trader", False):
                    bonus_parts.append("Trader")

                bonuses = "; ".join(bonus_parts) if bonus_parts else "None"

                tile_info = (
                    f"{terrain} (Move: -{move_cost}, Food: -{food_cost}, Water: -{water_cost}, "
                    f"Items: {items_str} | Bonuses: {bonuses})"
                )

                row_summary.append(tile_info)
            rows.append(" | ".join(row_summary))
        
        return "\n".join(rows)

