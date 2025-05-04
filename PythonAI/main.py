# main.py
import logging
import sys
import traceback
from flask import Flask, request, jsonify
from decision_engine import DecisionEngine 

# --- Configure Logging (File and Console) ---
log_formatter = logging.Formatter("%(asctime)s %(levelname)s [%(name)s]: %(message)s") 
log_level = logging.INFO # Use INFO or DEBUG

# Setup handlers
file_handler = logging.FileHandler("llm_decision_log.txt")
file_handler.setFormatter(log_formatter)

console_handler = logging.StreamHandler(sys.stderr)
console_handler.setFormatter(log_formatter)
console_handler.setLevel(log_level) # Explicitly set level for console handler

# Configure the root logger
root_logger = logging.getLogger()
root_logger.setLevel(log_level)

# Add handlers unconditionally (remove previous 'if not any(...)' checks)
root_logger.addHandler(file_handler)
root_logger.addHandler(console_handler)

logging.info("--- Logging configured for main.py (File & Console) ---")
# --- End Logging Config ---

app = Flask(__name__)

# --- Configuration ---
# Model ID for Ollama
OLLAMA_MODEL_ID = "llama3"  # Model to use with Ollama
# Recommended sampling params
LLM_TEMPERATURE = 0.6

# Path to the prompt template file (relative to decision_engine.py)
PROMPT_FILE_PATH = "prompts/default.txt" 
# Flask server settings
SERVER_HOST = "0.0.0.0"
SERVER_PORT = 5000
# --- End Configuration ---


# --- Initialize Decision Engine ---
try:
    logging.info(f"Initializing DecisionEngine with model: {OLLAMA_MODEL_ID}...")
    # Pass sampling parameters during initialization
    engine = DecisionEngine(
        model=OLLAMA_MODEL_ID, 
        prompt_file=PROMPT_FILE_PATH,
        temperature=LLM_TEMPERATURE
    )
    logging.info("DecisionEngine initialized successfully.")
except Exception as e:
    logging.exception("CRITICAL: Failed to initialize DecisionEngine!")
    engine = None

print(f"Flask server setup starting...") # Print for immediate visibility

# --- Flask Routes ---

@app.route("/decide", methods=["POST"])
def decide():
    """Handles decision requests from the Unity client."""
    if engine is None:
        logging.error("Engine not initialized. Cannot process /decide request.")
        return jsonify({"error": "Decision engine failed to initialize"}), 500
        
    logging.info("Received request on /decide endpoint.")
    try:
        data = request.get_json()
        if not data:
            logging.warning("Received non-JSON request or empty data.")
            return jsonify({"error": "Request must be JSON"}), 400

        # Extract data, using .get for safety
        food = data.get("food")
        water = data.get("water")
        energy = data.get("energy")
        gold = data.get("gold") 
        nearby = data.get("nearby") # Not currently used in prompt, but accept it
        visible_terrain = data.get("visibleTerrain") 
        current_pos_data = data.get("current_position", {}) 
        current_position = (current_pos_data.get("x", 0), current_pos_data.get("y", 0))
        map_width = data.get("map_width", 10) 
        map_height = data.get("map_height", 5) 
        
        # Validate essential data
        required_fields = {"food": food, "water": water, "energy": energy, "visibleTerrain": visible_terrain}
        missing_fields = [k for k, v in required_fields.items() if v is None]
        if missing_fields:
             error_msg = f"Missing required player state data: {', '.join(missing_fields)}"
             logging.error(error_msg)
             return jsonify({"error": error_msg}), 400

        logging.debug(f"Processing decision for state: food={food}, water={water}, energy={energy}, pos={current_position}")

        # Call engine - temperature/sampling params are now handled inside the engine via __init__
        decision = engine.make_decision(
            food, water, energy, nearby, visible_terrain, 
            current_position, map_width, map_height 
            # Removed temperature=LLM_TEMPERATURE from this call
        )
        logging.info(f"Decision received from engine: {decision}")
        
        return jsonify({"decision": decision})

    except Exception as e:
        logging.exception("CRITICAL: Unhandled exception in /decide endpoint!") 
        return jsonify({"error": "Internal server error processing decision"}), 500

@app.route("/memory", methods=["GET"])
def memory():
    """Returns the current state of the AI's memory."""
    if engine is None:
        logging.error("Engine not initialized. Cannot process /memory request.")
        return jsonify({"error": "Decision engine failed to initialize"}), 500
        
    logging.info("Received request on /memory endpoint.")
    try:
        move_history = engine.memory.move_history
        # Convert coordinate tuple keys to strings for JSON
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

# --- Main Execution Guard ---
if __name__ == "__main__":
    # Optional: Add test logs here if needed before starting server
    # logging.warning("--- TEST LOG BEFORE APP.RUN ---")
    # print("--- PRINT TEST BEFORE APP.RUN ---")
    
    logging.info(f"Starting Flask server on {SERVER_HOST}:{SERVER_PORT}")
    print(f"Flask server starting on {SERVER_HOST}:{SERVER_PORT}") 
    app.run(host=SERVER_HOST, port=SERVER_PORT, threaded=True, debug=False, use_reloader=False) # use_reloader=False can help debugging startup/logging