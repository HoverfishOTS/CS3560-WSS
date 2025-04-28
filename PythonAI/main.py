# main.py
from flask import Flask, request, jsonify
from decision_engine import DecisionEngine

app = Flask(__name__)
engine = DecisionEngine()

@app.route("/decide", methods=["POST"])
def decide():
    data = request.get_json()
    food = data.get("food")
    water = data.get("water")
    energy = data.get("energy")
    nearby = data.get("nearby")
    current_position = tuple(data.get("current_position", (0, 0)))
    map_width = data.get("map_width", 10)  # Default fallback
    map_height = data.get("map_height", 5)
    
    server_temp = 0.7 # Adjust this value to control randomness

    decision = engine.make_decision(
        food, water, energy, nearby, current_position, map_width, map_height, server_temp
    )
    return jsonify({"decision": decision})

@app.route("/memory", methods=["GET"])
def memory():
    move_history = engine.memory.move_history
    seen_map = {str(k): v for k, v in engine.memory.seen_map.items()}
    terrain_stats = engine.memory.terrain_stats

    return jsonify({
        "move_history": move_history,
        "seen_map": seen_map,
        "terrain_stats": terrain_stats
    })
    
@app.route("/reset", methods=["POST"])
def reset_memory():
    engine.memory.reset()
    return jsonify({"status": "Memory cleared"})


if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5000)
