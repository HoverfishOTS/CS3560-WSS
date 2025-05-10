# decision_engine.py
import os
import logging
import json
from typing import Optional
from openai import OpenAI
from memory_manager import MemoryManager # Assumes MemoryManager class is defined here or imported

TRADE_PROMPT_FILE_PATH = "prompts/trade_prompt.txt"

class DecisionEngine:
    """
    Interfaces with a vLLM server to make game decisions based on state and prompts.
    Sampling parameters can be configured during initialization.
    """
    def __init__(self,
                 model: str,
                 prompt_file: str, # Main prompt file path passed from main.py
                 trade_prompt_file: str = TRADE_PROMPT_FILE_PATH, # Trade prompt file path
                 base_url: str = "http://localhost:8000/v1",
                 temperature: Optional[float] = None,
                 top_p: Optional[float] = None,
                 presence_penalty: Optional[float] = None):
        """
        Initializes the DecisionEngine.

        Args:
            model: Hugging Face model ID for the vLLM server.
            prompt_file: Path to the main decision prompt template file.
            trade_prompt_file: Path to the trade decision prompt template file.
            base_url: Base URL of the vLLM OpenAI-compatible server.
            temperature: Optional sampling temperature.
            top_p: Optional nucleus sampling parameter.
            presence_penalty: Optional presence penalty parameter.
        """
        if not model:
             raise ValueError("A model ID must be provided.")

        self.client = OpenAI(api_key="EMPTY", base_url=base_url)
        self.model = model
        self.memory = MemoryManager()

        # Store sampling parameters
        self.temperature = temperature
        self.top_p = top_p
        self.presence_penalty = presence_penalty

        # Load main prompt template
        self.prompt_template = self._load_prompt_template(prompt_file)
        if not self.prompt_template:
             logging.error("CRITICAL: Main prompt template failed to load.")
             self.prompt_template = "ERROR: PROMPT TEMPLATE MISSING. State: {state_summary}"

        # Load trade prompt template
        self.trade_prompt_template = self._load_prompt_template(trade_prompt_file)
        if not self.trade_prompt_template:
             logging.error("CRITICAL: Trade prompt template failed to load.")
             self.trade_prompt_template = "ERROR: TRADE PROMPT MISSING. Player: {player_stats}, Trader: {trader_info}, Offer: {current_offer_str}"

        logging.info(f"DecisionEngine initialized for model: {self.model}")
        logging.info(f"Sampling Params: Temp={self.temperature}, TopP={self.top_p}, PresencePenalty={self.presence_penalty}")
        logging.info(f"Main prompt loaded: {'Yes' if self.prompt_template else 'No'}")
        logging.info(f"Trade prompt loaded: {'Yes' if self.trade_prompt_template else 'No'}")


    def make_decision(self, food: int, water: int, energy: int, nearby_info, # nearby_info is legacy, not used by current prompt
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

        # --- Generate Vision Summary ---
        logging.debug(f"Received visible_terrain data structure:\n{json.dumps(visible_terrain, indent=2)}")
        vision_summary = self.summarize_visible_terrain(visible_terrain)
        logging.debug(f"Formatted Vision Summary for Prompt:\n{vision_summary}")


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

        logging.debug(f"Complete Formatted Prompt Sent to LLM:\n{prompt}")
        logging.info(f"Sending request to model: {self.model}")

        # --- Build API parameters ---
        api_params = {
            "model": self.model,
            "messages": [
                {"role": "system", "content": "Output only the action (MOVE <DIR>, REST, or TRADE) on the first line, then 'Reason:' and explanation on the next. Ensure the reason matches the chosen action's target tile."},
                {"role": "user", "content": prompt}
            ],
            # "max_tokens": 150 
        }

        if self.temperature is not None: api_params["temperature"] = self.temperature
        if self.top_p is not None: api_params["top_p"] = self.top_p
        if self.presence_penalty is not None: api_params["presence_penalty"] = self.presence_penalty

        # --- Make API Call ---
        try:
            logging.debug(f"API Call Parameters: {api_params}")
            response = self.client.chat.completions.create(**api_params)

            if response.choices and len(response.choices) > 0:
                choice = response.choices[0]
                finish_reason = choice.finish_reason
                logging.info(f"LLM generation finish reason: {finish_reason}")

                if choice.message and choice.message.content is not None:
                    raw_response = choice.message.content.strip()
                    logging.debug(f"Raw response content from LLM: {raw_response}")
                else:
                    # Check if finish reason indicates length limit was hit (even without max_tokens set explicitly, server might have own limit)
                    if finish_reason == 'length':
                         logging.error(f"LLM response content is None. Finish reason was: length (Server limit likely reached).")
                         raw_response = f"REST\nReason: LLM response truncated by server length limit."
                    else:
                         logging.error(f"LLM response content is None. Finish reason was: {finish_reason}")
                         raw_response = f"REST\nReason: LLM produced no content (finish_reason: {finish_reason})."
            else:
                logging.error("LLM response is missing 'choices' or choices list is empty.")
                raw_response = "REST\nReason: Invalid response structure from LLM."

        except Exception as e:
            logging.exception(f"Failed API call to LLM or response processing: {e}")
            raw_response = "REST\nReason: API call/processing failed."

        decision = self._extract_action(raw_response)
        self.memory.update_seen_matrix(current_position, visible_terrain)
        self.memory.add_turn(state_summary, decision)
        return decision


    def _extract_action(self, text):
        """Extracts the primary action (MOVE, REST, TRADE) from the LLM response."""
        lines = text.strip().splitlines()
        command = "REST"
        reason = "No explanation provided."

        if lines:
            first_line_command = lines[0].strip()
            if any(first_line_command.upper().startswith(cmd) for cmd in ["MOVE", "REST", "TRADE"]):
                 command = first_line_command
                 if command.upper().startswith("MOVE") and len(command.split()) < 2:
                      logging.warning(f"Extracted MOVE command without direction: '{command}'. Defaulting to REST.")
                      command = "REST"
            else:
                 logging.warning(f"Could not extract valid command from first line: '{lines[0]}'. Defaulting to REST.")
                 command = "REST"

            for line in lines[1:]:
                 if line.lower().strip().startswith("reason:"):
                     reason = line.strip()[len("reason:"):].strip()
                     break

        else:
             logging.warning("Received empty response text. Defaulting to REST.")
             command = "REST"


        print(f"[LLM Reasoning] {reason}")
        logging.info(f"Decision: {command} | Reason: {reason}")

        return command.upper()


    def summarize_visible_terrain(self, visible_terrain):
        """Summarizes the 5x3 visible terrain matrix, one tile per line."""
        if not visible_terrain:
            return "No visible terrain data provided."

        lines = [] # Will store each tile's info string
        if not isinstance(visible_terrain, list) or len(visible_terrain) == 0 or not isinstance(visible_terrain[0], list):
            logging.warning(f"visible_terrain has unexpected structure: {visible_terrain}")
            return f"Visible terrain data is malformed or has unexpected structure: {str(visible_terrain)}"

        expected_rows = 5
        expected_cols = 3
        if len(visible_terrain) != expected_rows:
            logging.warning(f"visible_terrain has {len(visible_terrain)} rows, expected {expected_rows}")

        # Iterate through rows (y) and columns (x) of the matrix
        for y, row in enumerate(visible_terrain):
            if not isinstance(row, list) or len(row) != expected_cols:
                logging.warning(f"Row {y} in visible_terrain is not a list or has {len(row)} cols, expected {expected_cols}: {row}")
                # Add placeholder lines for malformed rows if needed for debugging
                # for x_malformed in range(expected_cols):
                #    rel_x = x_malformed
                #    rel_y = -(2 - y) # Original line before change
                #    lines.append(f"({rel_x:+},{rel_y:+}) : [Malformed Row Data]")
                continue # Skip processing this malformed row

            for x, tile in enumerate(row):
                # Calculate relative coordinates for this tile
                rel_x = x
                # If (0,0) is bottom-left in game, and visual y=0 is top-most row of vision:
                # y=0 (top row) should be North (+Y for LLM). Player at y=2 (center).
                # rel_y = (2 - y) makes y=0 -> +2 (North), y=2 -> 0, y=4 -> -2 (South)
                rel_y = (y - 2) # Relative Y: 4 (top visual row) -> +2 (North+2), 3 -> +1 (North+1), 2 -> 0, 1 -> -1 (South+1), 0 (bottom visual row) -> -2 (South+2)

                # Format string for this specific tile
                tile_info_str = f"({rel_x:+},{rel_y:+}) : " # Start with coordinates

                if tile is None:
                    tile_info_str += "[???]" # Mark unseen tiles
                else:
                    # Extract data safely using .get()
                    terrain = tile.get("terrain", "Unk")
                    move_cost = tile.get("move_cost", "?")
                    food_cost = tile.get("food_cost", "?")
                    water_cost = tile.get("water_cost", "?")
                    items = tile.get("items", [])
                    items_str = ", ".join(items) if items else "N/A"

                    # Format bonuses compactly
                    bonus_parts = []
                    fb = tile.get("food_bonus", 0); wb = tile.get("water_bonus", 0); gb = tile.get("gold_bonus", 0)
                    if fb > 0: bonus_parts.append(f"+{fb}F{'[R]' if tile.get('food_repeating', False) else ''}")
                    if wb > 0: bonus_parts.append(f"+{wb}W{'[R]' if tile.get('water_repeating', False) else ''}")
                    if gb > 0: bonus_parts.append(f"+{gb}G")
                    if tile.get("has_trader", False): bonus_parts.append("Trader")
                    bonuses = "; ".join(bonus_parts) if bonus_parts else "N/A"

                    # Append details to the tile string
                    tile_info_str += (
                        f"{terrain} "
                        f"(-{move_cost}E -{food_cost}F -{water_cost}W) "
                        f"Items: {items_str} | Bonuses: {bonuses}"
                    )
                # Add the complete line for this tile to the list
                lines.append(tile_info_str)

        # Join all individual tile lines with newlines
        return "\n".join(lines) if lines else "Visible terrain processing resulted in empty output."


    # --- make_trade_decision with enhanced logging ---
    def make_trade_decision(self, player_stats: dict, trader_info: dict, current_offer: dict) -> str:
        """Generates a trade-specific prompt, calls LLM, extracts trade action."""
        # Log input arguments received by this function
        logging.debug(f"make_trade_decision called with player_stats: {player_stats}, trader_info: {trader_info}, current_offer: {current_offer}")

        if not self.trade_prompt_template:
             logging.error("Cannot make trade decision: Trade prompt template not loaded or is empty.")
             return "REJECT" # Safe default

        # --- Prepare trade prompt context ---
        personality_hint = "Wants deals" if trader_info.get('type') == 'generous' else \
                           "Fair trader" if trader_info.get('type') == 'normal' else \
                           "Tough negotiator" if trader_info.get('type') == 'stingy' else \
                           "Unknown"

        is_initial_offer_phase = not current_offer or all(v == 0 for k, v in current_offer.items() if k != 'traderType') # Check if offer dict is empty or all values zero

        if is_initial_offer_phase:
            current_offer_str = "None (You need to propose the first offer using COUNTER OFFER {json})"
            offer_food_to_player = 0; offer_water_to_player = 0; offer_gold_to_player = 0
            offer_food_to_trader = 0; offer_water_to_trader = 0; offer_gold_to_trader = 0
        else:
            offer_food_to_player = current_offer.get("foodToPlayer", 0)
            offer_water_to_player = current_offer.get("waterToPlayer", 0)
            offer_gold_to_player = current_offer.get("goldToPlayer", 0)
            offer_food_to_trader = current_offer.get("foodToTrader", 0)
            offer_water_to_trader = current_offer.get("waterToTrader", 0)
            offer_gold_to_trader = current_offer.get("goldToTrader", 0)
            current_offer_str = (
                f"Player Receives: +{offer_food_to_player} Food, +{offer_water_to_player} Water, +{offer_gold_to_player} Gold | "
                f"Trader Receives: +{offer_food_to_trader} Food, +{offer_water_to_trader} Water, +{offer_gold_to_trader} Gold"
            )

        # Build the context dictionary
        context = {
            "player_food": player_stats.get("food", 0), "player_water": player_stats.get("water", 0), "player_gold": player_stats.get("gold", 0),
            "player_max_food": player_stats.get("max_food", 20), "player_max_water": player_stats.get("max_water", 20),
            "trader_type": trader_info.get("type", "unknown"), "trader_personality_hint": personality_hint,
            "trader_food_stock": trader_info.get("food_stock", 0), "trader_water_stock": trader_info.get("water_stock", 0),
            "current_offer_str": current_offer_str,
            "food_to_player": offer_food_to_player, "water_to_player": offer_water_to_player, "gold_to_player": offer_gold_to_player,
            "food_to_trader": offer_food_to_trader, "water_to_trader": offer_water_to_trader, "gold_to_trader": offer_gold_to_trader,
        }

        # --- Log right before formatting ---
        logging.debug(f"Context keys provided to format: {list(context.keys())}")
        # Log the start of the template string being used
        template_start = self.trade_prompt_template[:200] if self.trade_prompt_template else "None"
        logging.debug(f"Using trade_prompt_template (start): {template_start}...")
        # --- End Pre-Format Logging ---

        # --- Format prompt ---
        try:
            prompt = self.trade_prompt_template.format(**context) # The line that throws the error
        except KeyError as e:
             # Log the specific key that was missing
             logging.exception(f"KeyError during trade prompt formatting! Missing key: '{e}'. Context keys were: {list(context.keys())}")
             # Also log the template start again to be sure
             logging.error(f"The trade_prompt_template being used starts with: {template_start}...")
             prompt = f"Error formatting trade prompt. Missing key: {e}" # Fallback
        except Exception as e:
             logging.exception(f"Error formatting trade prompt: {e}")
             prompt = f"Error formatting trade prompt. Context keys: {list(context.keys())}"

        # --- (Rest of the function remains the same: build api_params, call LLM, extract action) ---
        logging.debug(f"Formatted Trade Prompt Sent:\n{prompt}")
        logging.info(f"Sending trade request to model: {self.model}")

        api_params = {
            "model": self.model,
            "messages": [
                {"role": "system", "content": "Output only the trade action (ACCEPT, REJECT, or COUNTER OFFER {json}) on the first line, then 'Reason:' and explanation on the next."},
                {"role": "user", "content": prompt}
            ],
        }
        if self.temperature is not None: api_params["temperature"] = self.temperature
        if self.top_p is not None: api_params["top_p"] = self.top_p
        if self.presence_penalty is not None: api_params["presence_penalty"] = self.presence_penalty

        try:
            response = self.client.chat.completions.create(**api_params)
            if response.choices and len(response.choices) > 0:
                choice = response.choices[0]; finish_reason = choice.finish_reason
                logging.info(f"LLM trade generation finish reason: {finish_reason}")
                if choice.message and choice.message.content is not None:
                    raw_response = choice.message.content.strip()
                    logging.debug(f"Raw trade response content from LLM: {raw_response}")
                else:
                    raw_response = f"REJECT\nReason: LLM produced no content for trade (finish_reason: {finish_reason or 'unknown'})."
                    if finish_reason == 'length': logging.error(f"LLM trade response content None. Finish reason: length.")
                    else: logging.error(f"LLM trade response content None. Finish reason: {finish_reason}")
            else:
                raw_response = "REJECT\nReason: Invalid response structure (no choices)."
                logging.error(raw_response)
        except Exception as e:
            logging.exception(f"Failed API call/processing for trade: {e}")
            raw_response = "REJECT\nReason: API call/processing failed for trade."

        trade_action = self._extract_trade_action(raw_response)
        if is_initial_offer_phase and not trade_action.startswith("COUNTER OFFER"):
            logging.warning(f"AI failed to generate an initial COUNTER OFFER, received: {trade_action}. Defaulting to REJECT.")
            trade_action = "REJECT"
        return trade_action


    def _extract_trade_action(self, text):
        """Extracts trade action, handling potential COUNTER OFFER with JSON."""
        lines = text.strip().splitlines()
        command = "REJECT"
        reason = "No explanation provided."

        if not lines:
            logging.warning("Received empty response text for trade action extraction.")
            return command

        first_line = lines[0].strip()

        if first_line.upper() == "ACCEPT":
            command = "ACCEPT"
        elif first_line.upper() == "REJECT":
            command = "REJECT"
        elif first_line.upper().startswith("COUNTER OFFER"):
             command = first_line
             if "{" not in command or "}" not in command:
                 logging.warning(f"Extracted COUNTER OFFER but missing JSON structure: {command}. Defaulting to REJECT.")
                 command = "REJECT"

        for line in lines[1:]:
             line_lower_stripped = line.lower().strip()
             if line_lower_stripped.startswith("reason:"):
                 reason = line.strip()[len("reason:"):].strip()
                 break

        print(f"[LLM Trade Reasoning] {reason}")
        logging.info(f"Trade Decision Raw: {command} | Reason: {reason}")

        return command


    def _load_prompt_template(self, filename: str) -> Optional[str]:
         """Loads prompt text from a file."""
         base_dir = os.path.dirname(os.path.abspath(__file__))
         path = os.path.join(base_dir, filename)
         try:
             with open(path, "r", encoding="utf-8") as f:
                 logging.info(f"Loading prompt template from: {path}")
                 return f.read()
         except Exception as e:
             logging.exception(f"Failed to load prompt template from {path}: {e}")
             return None
