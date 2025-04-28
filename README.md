# Wilderness Survival System (WSS) - AI Decision Server

## Project Overview

This project implements the AI decision-making backend for the Wilderness Survival System (WSS) game.

- The Unity (C#) game client sends the player's current status and local map vision to a Python Flask server.
- The Python server processes the data using an LLM-based DecisionEngine (Llama 3 via OpenAI-compatible API).
- The AI chooses one action each turn:
  - MOVE <DIRECTION> (NORTH, EAST, etc.)
  - REST
  - TRADE
- The chosen action is sent back to the Unity client, which updates the player's position, resources, or trades.

All AI memory (past moves, seen tiles, known terrain) is managed server-side.

---

## System Flow Per Turn

```
+----------------------+
|  Unity (C#)           |
|  Player / Brain Class |
+----------------------+
         |
         | Send current state and nearby vision
         V
+----------------------+
|  Flask API Server    |
|  (Python main.py)    |
+----------------------+
         |
         | Internal call to DecisionEngine
         V
+----------------------+
|  DecisionEngine.py    |
|  (Python AI Brain)    |
+----------------------+
         |
         | Update memory, build prompt
         | Query LLM model
         | Parse decision (MOVE / REST / TRADE)
         V
+----------------------+
|  Flask API Server    |
+----------------------+
         |
         | Return AI decision
         V
+----------------------+
|  Unity (C#)           |
|  Player / Brain Class |
+----------------------+
         |
         | Apply decision (move, rest, trade)
         | Update player stats
         | Advance to next turn
         V
(repeat each turn)
```

---

## Key Components

| Component             | Description |
|:----------------------|:------------|
| `simulate_game.py`     | Standalone simulation runner (Python) for local testing without Unity. |
| `main.py`              | Flask API server. Receives player state and returns AI decisions. |
| `decision_engine.py`   | Manages prompts, queries LLM, and interprets results. |
| `memory_manager.py`    | Stores move history, seen map tiles, and past terrain information. |

---

## Features

- **Memory-based AI**: Remembers explored tiles beyond immediate vision.
- **Bonus Collection**: Player collects Food, Water, or Gold bonuses upon stepping onto bonus tiles.
- **Trading System**: Trades are only valid if a Trader is physically present on the current tile.
- **Dynamic Map Generation**: Terrains and bonuses/traders are set at map creation, not randomly generated during vision.
- **Internal Creativity Control**: AI creativity (temperature) is fixed server-side and cannot be adjusted by the Unity client.

---

## Setup Instructions

1. Start the Flask server:
   ```bash
   python main.py
   ```

2. From Unity or the simulator, POST player information to:
   ```
   http://localhost:5000/decide
   ```
   with fields: `food`, `water`, `energy`, `gold`, `nearby`, `current_position`, `map_width`, `map_height`.

3. Receive the AI's decision (`MOVE <DIRECTION>`, `REST`, or `TRADE`) in JSON.

4. Apply the action inside the Unity game.

---

## Notes

- Every game run should reset AI memory by calling:
  ```
  POST http://localhost:5000/reset
  ```
- Terrain costs and map dimensions are customizable per simulation.
- The AI balances survival against goal progression dynamically based on visible tiles and past memory.

---