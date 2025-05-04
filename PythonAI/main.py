# main.py
import logging
import sys
import traceback
from flask import Flask, request, jsonify

log_formatter = logging.Formatter("%(asctime)s %(levelname)s [%(name)s]: %(message)s")
log_level = logging.INFO
file_handler = logging.FileHandler("llm_decision_log.txt")
file_handler.setFormatter(log_formatter)
console_handler = logging.StreamHandler(sys.stderr)
console_handler.setFormatter(log_formatter)
console_handler.setLevel(log_level)
root_logger = logging.getLogger()
root_logger.setLevel(log_level)
# Clear existing handlers if necessary before adding new ones
if root_logger.hasHandlers():
    root_logger.handlers.clear()
root_logger.addHandler(file_handler)
root_logger.addHandler(console_handler)
logging.info("--- Logging configured for main.py (File & Console) ---")


app = Flask(__name__)

# Configuration
VLLM_MODEL_ID = "Qwen/Qwen3-14B-AWQ" # Or your chosen model
LLM_TEMPERATURE = 0.6
TOP_P = 0.95
PROMPT_FILE_PATH = "prompts/trader_test.txt"
TRADE_PROMPT_FILE_PATH = "prompts/trade_prompt.txt"
SERVER_HOST = "0.0.0.0"
SERVER_PORT = 5000

# Initialize Decision Engine
try:
    from decision_engine import DecisionEngine
    logging.info(f"Initializing DecisionEngine with model: {VLLM_MODEL_ID}...")
    engine = DecisionEngine(
        model=VLLM_MODEL_ID,
        prompt_file=PROMPT_FILE_PATH,
        trade_prompt_file=TRADE_PROMPT_FILE_PATH,
        temperature=LLM_TEMPERATURE,
        top_p=TOP_P,
    )
    logging.info("DecisionEngine initialized successfully.")
except ImportError:
    logging.exception("CRITICAL: Failed to import DecisionEngine! Check file location and dependencies.")
    engine = None
except Exception as e:
    logging.exception("CRITICAL: Failed to initialize DecisionEngine!")
    engine = None


# Flask Routes
@app.route("/decide", methods=["POST"])
def decide():
    """Handles general movement/rest/initiate-trade decision requests."""
    if engine is None:
        logging.error("Engine not initialized. Cannot process /decide request.")
        return jsonify({"error": "Decision engine failed to initialize"}), 500

    logging.info("Received request on /decide endpoint.")
    try:
        data = request.get_json()
        if not data:
            logging.warning("Received non-JSON request or empty data for /decide.")
            return jsonify({"error": "Request must be JSON"}), 400

        food = data.get("food")
        water = data.get("water")
        energy = data.get("energy")
        visible_terrain = data.get("visibleTerrain")
        current_pos_data = data.get("current_position", {})
        current_position = (current_pos_data.get("x", 0), current_pos_data.get("y", 0))
        map_width = data.get("map_width", 10)
        map_height = data.get("map_height", 5)

        required_fields = {"food": food, "water": water, "energy": energy, "visibleTerrain": visible_terrain}
        missing_fields = [k for k, v in required_fields.items() if v is None]
        if missing_fields:
             error_msg = f"Missing required player state data for /decide: {', '.join(missing_fields)}"
             logging.error(error_msg)
             return jsonify({"error": error_msg}), 400

        logging.debug(f"Processing main decision for state: food={food}, water={water}, energy={energy}, pos={current_position}")

        decision = engine.make_decision(
            food, water, energy, None,
            visible_terrain,
            current_position, map_width, map_height
        )
        logging.info(f"Decision received from engine: {decision}")

        return jsonify({"decision": decision})

    except Exception as e:
        logging.exception("CRITICAL: Unhandled exception in /decide endpoint!")
        return jsonify({"error": "Internal server error processing decision"}), 500


@app.route("/trade_decide", methods=["POST"])
def trade_decide():
    """Handles trade-specific decision requests (initial offer, accept/reject/counter)."""
    if engine is None:
        logging.error("Engine not initialized. Cannot process /trade_decide request.")
        return jsonify({"error": "Decision engine failed to initialize"}), 500

    logging.info("Received request on /trade_decide endpoint.")
    try:
        data = request.get_json()
        if not data:
            logging.warning("Received non-JSON /trade_decide request or empty data.")
            return jsonify({"error": "Request must be JSON"}), 400

        player_stats_data = data.get("player_stats", {})
        trader_info_data = data.get("trader_info", {})
        current_offer_data = data.get("current_offer", {})

        player_food = player_stats_data.get("player_food")
        player_water = player_stats_data.get("player_water")
        player_gold = player_stats_data.get("player_gold")
        player_max_food = player_stats_data.get("player_max_food", 20)
        player_max_water = player_stats_data.get("player_max_water", 20)

        trader_type = trader_info_data.get("trader_type")
        trader_food_stock = trader_info_data.get("trader_food_stock")
        trader_water_stock = trader_info_data.get("trader_water_stock")

        required_trade_fields = {
            "player_food": player_food, "player_water": player_water, "player_gold": player_gold,
            "trader_type": trader_type, "trader_food_stock": trader_food_stock, "trader_water_stock": trader_water_stock
        }
        missing_fields = [k for k, v in required_trade_fields.items() if v is None]
        if missing_fields:
             error_msg = f"Missing required trade state data: {', '.join(missing_fields)}"
             logging.error(error_msg)
             logging.debug(f"Received trade data structure: {data}")
             return jsonify({"error": error_msg}), 400

        logging.debug(f"Processing trade decision for Player(F:{player_food}, W:{player_water}, G:{player_gold}) "
                      f"Trader(T:{trader_type}, Fs:{trader_food_stock}, Ws:{trader_water_stock}) Offer Received:{current_offer_data or 'None (Initial Offer)'}")

        trade_action = engine.make_trade_decision(
            player_stats={
                "food": player_food, "water": player_water, "gold": player_gold,
                "max_food": player_max_food, "max_water": player_max_water
            },
            trader_info={
                "type": trader_type, "food_stock": trader_food_stock, "water_stock": trader_water_stock
            },
            current_offer=current_offer_data
        )
        logging.info(f"Trade decision received from engine: {trade_action}")

        return jsonify({"trade_action": trade_action})

    except Exception as e:
        logging.exception("CRITICAL: Unhandled exception in /trade_decide endpoint!")
        return jsonify({"error": "Internal server error processing trade decision", "trade_action": "REJECT"}), 500


@app.route("/memory", methods=["GET"])
def memory():
    """Returns the current state of the AI's memory."""
    if engine is None:
        logging.error("Engine not initialized. Cannot process /memory request.")
        return jsonify({"error": "Decision engine failed to initialize"}), 500

    logging.info("Received request on /memory endpoint.")
    try:
        move_history = engine.memory.move_history
        seen_map = {str(k): v for k, v in engine.memory.seen_map.items()}
        terrain_stats = engine.memory.terrain_stats

        return jsonify({
            "move_history": move_history,
            "seen_map": seen_map,
            "terrain_stats": terrain_stats
        })
    except Exception as e:
        logging.exception("Error retrieving memory data.")
        return jsonify({"error": "Internal server error retrieving memory"}), 500


@app.route("/reset", methods=["POST"])
def reset_memory():
    """Resets the AI's memory state."""
    if engine is None:
        logging.error("Engine not initialized. Cannot process /reset request.")
        return jsonify({"error": "Decision engine failed to initialize"}), 500

    logging.info("Received request on /reset endpoint.")
    try:
        engine.memory.reset()
        logging.info("AI memory reset successfully via /reset endpoint.")
        return jsonify({"status": "Memory cleared"})
    except Exception as e:
        logging.exception("Error resetting memory.")
        return jsonify({"error": "Internal server error resetting memory"}), 500


if __name__ == "__main__":
    logging.info(f"Starting Flask server on {SERVER_HOST}:{SERVER_PORT}")
    print(f"Flask server starting on {SERVER_HOST}:{SERVER_PORT}")
    app.run(host=SERVER_HOST, port=SERVER_PORT, threaded=True, debug=False, use_reloader=False)

