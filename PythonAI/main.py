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

    decision = engine.make_decision(food, water, energy, nearby)
    return jsonify({"decision": decision})

if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5000)

