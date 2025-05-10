# Wilderness Survival System (WSS)

## Overview

The Wilderness Survival System (WSS) is a 2D grid-based survival game developed as a group project. Players, controlled by either a human or an AI, navigate a procedurally generated wilderness with the primary objective of reaching an escape point on the eastern edge of the map. Survival depends on managing critical resources: Food, Water, and Energy. These resources are consumed through actions like moving across different terrain types or resting. The game features an AI player driven by a Large Language Model (LLM), which makes strategic decisions based on its perception of the game world, its current state, and a detailed set of rules and goals.

Demo here: https://youtu.be/4v-tuuYspa4 

## Core Features

* **Dynamic Gameplay:** Experience a challenging survival scenario where resource management and strategic movement are key.
* **Procedural Map Generation:** Each playthrough offers a unique map with diverse terrain types (Plains, Forest, Jungle, Swamp, Mountain, Desert), each impacting movement and resource costs.
* **Resource Management:** Monitor and maintain Food, Water, and Energy. Depletion of any resource results in game over. Gold can also be acquired.
* **Player Actions:**
    * **Move:** Navigate in 8 directions (North, South, East, West, and diagonals).
    * **Rest:** Recover Energy at the cost of Food and Water.
    * **Trade:** Interact with traders found on the map.
* **AI-Powered Player:**
    * An advanced AI player option driven by a Python-based backend leveraging a Large Language Model (currently Qwen1.5-14B-Chat-AWQ).
    * The AI analyzes a 5x3 vision grid, player status, and environmental data to make decisions.
    * Sophisticated prompt engineering guides the AI's reasoning for movement, resting, and trading strategies.
    * Accurate coordinate system mapping ensures the AI's directional understanding aligns with the game world.
* **Trading System:**
    * Encounter traders with distinct personalities ("generous," "normal," "stingy") and limited, randomized stock.
    * Engage in a negotiation process: offer Gold for Food/Water, receive counter-offers, and make your own.
    * The AI is equipped with logic to participate in these trade negotiations.
    * Human players have a UI for trading, including a haggling minigame.
* **Vision System:** Provides the player/AI with a 5x3 view of the surrounding tiles, crucial for decision-making. The data is carefully structured for AI consumption.
* **User Interface:** Includes a main menu, configurable game setup (difficulty, player type), in-game HUD for player stats and map interaction, tile information tooltips, and game summary screens.
* **Configurability:** Game parameters such as map difficulty and initial player settings can be adjusted.

## Technology Stack

* **Game Engine:** Unity (C#)
* **AI Decision Logic:** Python
* **AI Model:** Large Language Model (LLM) - specifically configured for Qwen1.5-14B-Chat-AWQ.
* **AI Serving Framework:** The Python backend utilizes Flask to serve the AI's decisions. Model loading and inference leverages vLLM.
* **Data Exchange:** HTTP and JSON are used for communication between the Unity game client and the Python AI server.

## Setup and Installation

To run the WSS project, you'll need to set up both the Unity game client and the Python AI server.

### 1. Unity Game Client

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/HoverfishOTS/CS3560-WSS
    cd CS3560-WSS 
    ```
2.  **Open in Unity:** Open the `CS3560-WSS` folder as a project in the Unity Hub (ensure you have compatible Unity version 6000.0.35f1).
3.  **Scenes:** The main scenes are found in `Assets/Scenes/`. The starting scene is `Main Menu.unity`.
4.  **Required Package:** Go to the package manager and Unity and install Newtonsoft.Json (Json.NET)

### 2. Python AI Server

The Python AI server requires a specific environment and model setup.

1.  **Navigate to the PythonAI directory:**
    ```bash
    cd PythonAI
    ```
2.  **Create a Python Virtual Environment (Recommended):**
    ```bash
    python -m venv venv
    source venv/bin/activate  # On Windows: venv\Scripts\activate
    ```
3.  **Install Dependencies:**
    A `requirements.txt` file should list all necessary Python packages. If not present, you'll need to install core libraries. Key libraries likely include:
    * `Flask` (for the server)
    * `torch` and `transformers` (for LLM interaction)
    * `accelerate`
    * Packages for AWQ support if not bundled with `transformers` (e.g., `autoawq`)
    * Other dependencies specific to your LLM loading/serving mechanism (e.g., `vllm` if used directly).
    ```bash
    pip install -r requirements.txt 
    ```
4.  **LLM Model Setup:**
    * The AI is configured to use `Qwen/Qwen1.5-14B-Chat-AWQ`. This model typically needs to be downloaded from the Hugging Face Hub. The Python scripts might handle this automatically on first run if using `transformers` with a model identifier, or you may need to download it manually and place it in an expected directory.
    * **GPU Requirements:** Running a 14B parameter model, even AWQ quantized, benefits greatly from a dedicated NVIDIA GPU with sufficient VRAM (ideally 10GB+ for the model, KV cache, and overhead). Ensure you have appropriate NVIDIA drivers installed.
    * **WSL2 Users:** If running the Python server in WSL2 for GPU support:
        * Ensure WSL2 is properly configured with GPU passthrough.
        * Have up-to-date NVIDIA drivers on your Windows host.
        * Install a compatible CUDA toolkit version within your WSL2 distribution.
        * Consider configuring WSL2's memory and processor allocation via `.wslconfig` for optimal performance.

5.  **Configure AI Server (if necessary):**
    * Check `PythonAI/server.py` or related configuration files for any host, port, or model path settings. By default, it runs on `localhost:5000`.
    * The AI prompt is located in `PythonAI/prompts/default.txt`.

## How to Run

1.  **Start the Python AI Server:**
    * Activate your Python virtual environment.
    * Navigate to the `PythonAI` directory.
    * Run the server script:
        ```bash
        python server.py 
        ```
    * Wait for the server to initialize and load the LLM. This may take some time, especially on the first run or if the model needs to be downloaded. Look for log messages indicating the server is ready and listening for requests (e.g., `* Running on http://127.0.0.1:5000/`).

2.  **Run the Unity Game:**
    * Open the WSS project in Unity.
    * Open the `Main Menu` scene.
    * Press the Play button in the Unity Editor.
    * Alternatively, build the game for your desired platform and run the executable.

3.  **Gameplay:**
    * From the main menu, configure your game (e.g., select map difficulty, choose player type: Human or AI).
    * If Human player: Use UI buttons for movement and actions like Rest or Trade.
    * If AI player: The game will automatically request decisions from the Python AI server.
    * The objective is to move from the west side of the map to the east side while keeping Food, Water, and Energy above zero.

## Project Structure Overview

* **`/` (Root Directory):** Contains the Unity project and the `PythonAI` folder.
* **`Assets/`:** Standard Unity assets folder.
    * `Assets/Scenes/`: Contains main game scenes like `Main Menu`, `Gameplay`, etc.
    * `Assets/Scripts/`: Contains all C# scripts for game logic.
        * `Config/`: Game configuration scripts.
        * `Map/`: Map generation, display, and tile logic.
        * `Player/`: Player class, brain interface (`IBrain`), specific brains (`UserBrain`, `AIBrain`), and vision system (`Vision.cs` and implementations like `GenerateField()` logic).
        * `Trader/`: Trader logic and `TradeManager`.
        * `UI/`: Scripts for managing UI elements, buttons, displays.
        * `GameManager.cs`, `SummaryManager.cs`, etc.: Core game orchestration.
    * `Assets/Prefabs/`: GameObjects used for map tiles, UI elements, etc.
    * `Assets/Materials/`, `Assets/Sprites/`: Visual assets.
* **`PythonAI/`:** Contains the Python AI server and related files.
    * `server.py`: Flask application to handle requests from Unity.
    * `decision_engine.py`: Core Python class that processes game state and uses the LLM to make decisions.
    * `prompts/default.txt`: The main prompt template for the LLM.
    * `venv/` (if created): Python virtual environment.
    * `requirements.txt`: Lists Python dependencies.

## Key Design Aspects and Learnings

This project emphasized a modular design through the principles of Object-Oriented Design (OOD). Key classes like `Player`, `Map`, `Vision`, `Trader`, and `AIBrain` encapsulate their respective functionalities, interacting through well-defined interfaces. This approach was instrumental in managing complexity, facilitating debugging (especially the critical AI coordinate system alignment), and allowing for concurrent development. The use of an `IBrain` interface for polymorphism allowed the game to seamlessly switch between human and AI control. The separation of the game client (Unity/C#) and the AI decision-making server (Python/LLM) via a JSON/HTTP API is a strong example of modularity, promoting independent development and scalability of the AI component. The iterative refinement of the AI's prompt and the logic in `decision_engine.py` to accurately interpret game state and vision data were significant learning experiences in integrating LLMs into game mechanics.

## Future Work & Potential Enhancements

While the current version of WSS is functionally complete, several avenues exist for future development:

* **Advanced AI Player Strategies:** Explore using more powerful LLMs (given access to suitable hardware/APIs) or fine-tuning existing models for more sophisticated, context-aware long-term planning and resource management.
* **Multiple AI Instances:** Implement functionality for several AI players to compete on the same map simultaneously, introducing dynamic resource competition.
* **Item System Depth:** Expand the item system with unique tools (e.g., for faster resource gathering or movement), item upgrades, crafting recipes, and more diverse consumable effects that the AI can strategically utilize.
* **Advanced AI Trader Decisions & Interactions:** Develop more complex AI logic for trading, allowing the AI to evaluate trade offers more dynamically based on its long-term needs, current resource levels, trader personality, and potentially initiate more nuanced negotiation tactics or even offer items (if the system were expanded to allow it).

## Contributors

This project was a collaborative effort by:
  * Ryan Wei – Prompt Engineered the Brain interaction with the game(Trader, Game Manager, etc.). Wrote interfacing between LLM, Python, and Unity. 
  * Edmund Cheung – Game logic, sprites, frontend design
  * Aaron Lo – Tile Direction, logistics, mapping
  * Henry Tran – Game balancing and prompt engineering & brainstorm
  * Chenrui Zhang – create presentation slides and documentations

## Model Citations and Acknowledgements

1.  **Qwen3-14B-AWQ by Qwen Team (Alibaba Cloud)**
    * **Model Card:** [https://huggingface.co/Qwen/Qwen3-14B-AWQ](https://huggingface.co/Qwen/Qwen3-14B-AWQ)
      ```bibtex
      @misc{qwen3,
        title  = {Qwen3},
        url    = {https://qwenlm.github.io/blog/qwen3/},
        author = {Qwen Team},
        month  = {April},
        year   = {2025}
      }
      ```


2.  **Llama-3.1-8B-Instruct by Meta**
    * **Model Card:** [https://huggingface.co/meta-llama/Llama-3.1-8B-Instruct](https://huggingface.co/meta-llama/Llama-3.1-8B-Instruct)
        ```bibtex
        @article{llama3_1_herd,
          title={The Llama 3.1 Herd of Models},
          author={Aaron Grattafiori and 558 others}, 
          year={2024},
          journal={arXiv preprint arXiv:2407.21783} 
        }
        ```