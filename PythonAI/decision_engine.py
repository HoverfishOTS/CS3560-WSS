# decision_engine.py
import os
import logging
import requests
from typing import Optional
from memory_manager import MemoryManager # Assumes MemoryManager class is defined here or imported

class DecisionEngine:
    """
    Interfaces with Ollama's Llama 3 server to make game decisions based on state and prompts.
    """
    def __init__(self, 
                 base_url: str = "http://localhost:11434", 
                 model: str = "llama3",
                 prompt_file: str = "prompts/game_prompt.txt",
                 temperature: Optional[float] = None,
                 top_p: Optional[float] = None,
                 presence_penalty: Optional[float] = None):
        """
        Initializes the DecisionEngine.

        Args:
            base_url: Base URL of the Ollama server (default: http://localhost:11434).
            model: Model name in Ollama (default: llama3).
            prompt_file: Path to the prompt template file (relative to this file).
            temperature: Optional sampling temperature.
            top_p: Optional nucleus sampling parameter.
            presence_penalty: Optional presence penalty parameter.
        """
        if not model:
             raise ValueError("A model ID must be provided.")
             
        self.base_url = base_url
        self.model = model 
        self.memory = MemoryManager()
        
        # Store sampling parameters passed during initialization
        self.temperature = temperature
        self.top_p = top_p
        self.presence_penalty = presence_penalty
        
        self.prompt_template = self.load_prompt_template(prompt_file) 
        
        logging.info(f"DecisionEngine initialized for model: {self.model}")
        logging.info(f"Sampling Params: Temp={self.temperature}, TopP={self.top_p}, PresencePenalty={self.presence_penalty}")
        if not self.prompt_template:
             # Handle missing prompt template appropriately
             logging.error("CRITICAL: Prompt template failed to load.")
             self.prompt_template = "ERROR: PROMPT TEMPLATE MISSING. State: {state_summary}"

    def load_prompt_template(self, filename: str) -> Optional[str]:
        """Loads prompt text from a file."""
        path = os.path.join(os.path.dirname(__file__), filename)
        try:
            with open(path, "r", encoding="utf-8") as f:
                logging.info(f"Loading prompt template from: {path}")
                return f.read()
        except Exception as e:
            logging.exception(f"Failed to load prompt template from {path}: {e}")
            return None

    def make_decision(self, food: int, water: int, energy: int, nearby_info, 
                      visible_terrain: list, current_position: tuple = (0, 0),
                      map_width: int = 10, map_height: int = 5) -> str:
        """Generates prompt, calls LLM, extracts action."""
        if not self.prompt_template:
             logging.error("Cannot make decision: Prompt template not loaded.")
             return "REST" # Safe default

        # --- Prepare prompt context ---
        x, y = current_position
        state_summary = f"Food: {food}, Water: {water}, Energy: {energy}, Position: ({x}, {y})" 
        memory_context = self.memory.get_recent_context() 
        tile_summary = self.memory.summarize_seen_map(current_position) 
        vision_summary = self.summarize_visible_terrain(visible_terrain) 

        # --- Format prompt ---
        try:
            prompt = self.prompt_template.format(
                map_width=map_width,
                map_height=map_height,
                state_summary=state_summary,
                memory_context=memory_context if memory_context else "None.", 
                tile_summary=tile_summary if tile_summary else "None.", 
                vision_summary=vision_summary if vision_summary else "None." 
            )
        except Exception as e:
             logging.exception(f"Error formatting prompt: {e}")
             prompt = f"Error formatting prompt. State: {state_summary}" # Fallback

        logging.debug(f"Formatted Prompt Sent:\n{prompt}")
        logging.info(f"Sending request to model: {self.model}")
        
        # --- Build API parameters ---
        system_prompt = """You are a survival game AI. Your goal is to survive and reach the end of the map.
IMPORTANT RULES:
1. If energy is 5 or less, you MUST REST to avoid losing the game
2. If food or water is 5 or less, prioritize finding resources
3. Only move if you have enough energy (at least 4) to handle potential terrain costs
4. Moving into mountains costs 3 energy, plains cost 2, and forests cost 1
5. Always explain your reasoning for the chosen action
6. MAX RESOURCES FOR ENERGY IS 15, FOOD IS 15, AND WATER IS 15. SO DONT REST UNLESS YOU HAVE TO.
7. STOP RESTING WHEN YOU HAVE ENOUGH ENERGY ALREADY. YOU ARE JUST USING RESOURCES FOR NOTHING.
8. IF YOU HAVE ENOUGH RESOURCES, YOU SHOULD TRADE.
9. DO NOT REST IF ENERGY IS ABOVE 5 UNLESS THERE'S A SPECIFIC REASON (like being near a trader)

Output format:
ACTION
Reason: [your explanation]"""

        api_params = {
            "model": self.model,
            "prompt": f"System: {system_prompt}\nUser: {prompt}",
            "stream": False
        }
        
        # Add sampling params only if they were set during init
        if self.temperature is not None:
            api_params["temperature"] = self.temperature
        if self.top_p is not None:
            api_params["top_p"] = self.top_p

        # --- Make API Call ---
        try:
            logging.debug(f"API Call Parameters: {api_params}")
            response = requests.post(
                f"{self.base_url}/api/generate",
                json=api_params
            )
            response.raise_for_status()
            response_data = response.json()

            # --- Log response ---
            logging.debug(f"Full API Response JSON:\n{response_data}")

            if "response" in response_data:
                raw_response = response_data["response"].strip()
                logging.debug(f"Raw response content from LLM: {raw_response}")
            else:
                logging.error("LLM response is missing 'response' field")
                raw_response = "REST\nReason: Invalid response structure from LLM."

        except Exception as e:
            logging.exception(f"Failed API call to LLM or response processing: {e}") 
            raw_response = "REST\nReason: API call/processing failed." 

        # Force REST if energy is critically low, but prevent REST if energy is high
        if energy <= 5:
            raw_response = "REST\nReason: Energy is critically low (5 or less). Must rest to avoid losing the game."
        elif energy > 5 and "REST" in raw_response.upper():
            # If energy is above 5 and the AI chose to REST, force it to MOVE EAST instead
            raw_response = "MOVE EAST\nReason: Energy is above 5, no need to rest. Moving to explore and find resources."

        decision = self._extract_action(raw_response) 
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
            """Summarizes the 5x3 visible terrain matrix, prepending relative coordinates."""
            if not visible_terrain:
                return "No visible terrain data provided."

            lines = []
            # Basic check for expected nested list structure
            if not isinstance(visible_terrain, list) or len(visible_terrain) == 0 or not isinstance(visible_terrain[0], list):
                logging.warning(f"visible_terrain has unexpected structure: {visible_terrain}")
                # Providing the raw structure might help debug if it's just slightly off
                return f"Visible terrain data is malformed or has unexpected structure: {str(visible_terrain)}"

            expected_rows = 5
            expected_cols = 3
            if len(visible_terrain) != expected_rows:
                logging.warning(f"visible_terrain has {len(visible_terrain)} rows, expected {expected_rows}")
                # You might return an error or try to process anyway depending on desired robustness

            for y, row in enumerate(visible_terrain):
                # Ensure row is a list before iterating and check length
                if not isinstance(row, list) or len(row) != expected_cols:
                    logging.warning(f"Row {y} in visible_terrain is not a list or has {len(row)} cols, expected {expected_cols}: {row}")
                    # Pad with error message for this row to maintain structure if desired
                    lines.append(" | ".join([f"[Malformed Row {y}]"] * expected_cols))
                    continue # Skip processing this malformed row

                row_summary = []
                for x, tile in enumerate(row):
                    # Calculate relative coordinates BEFORE checking if tile is None
                    # This allows showing coordinates even for unseen tiles if desired later
                    rel_x = x
                    rel_y = -(2 - y) # Based on C# logic: worldY = playerY - (2 - y)

                    if tile is None:
                        # Represent unseen tiles - decide if you want coords shown
                        # Option 1: Just placeholder
                        # row_summary.append("[???]")
                        # Option 2: Placeholder with coords
                        row_summary.append(f"({rel_x:+},{rel_y:+}) : [???]")
                        continue # Go to next tile in row

                    # --- Process visible tile data ---
                    # Safely get tile attributes using .get() with defaults
                    terrain = tile.get("terrain", "Unknown")
                    move_cost = tile.get("move_cost", "?")
                    food_cost = tile.get("food_cost", "?")
                    water_cost = tile.get("water_cost", "?")
                    items = tile.get("items", []) # Default to empty list
                    items_str = ", ".join(items) if items else "No Items" # Handle empty list

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

                    # Prepend relative coordinates to the tile info string
                    tile_info = (
                        # The format string uses '+' to always show sign for coordinates
                        f"({rel_x:+},{rel_y:+}) : {terrain} (Move: -{move_cost}, Food: -{food_cost}, Water: -{water_cost}, "
                        f"Items: {items_str} | Bonuses: {bonuses})"
                    )
                    row_summary.append(tile_info)
                    # --- End processing visible tile ---

                lines.append(" | ".join(row_summary)) # Join tiles in the current row

            # Join all processed rows with newlines
            return "\n".join(lines) if lines else "Visible terrain processing resulted in empty output."
