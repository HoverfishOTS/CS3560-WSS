from openai import OpenAI
from memory_manager import MemoryManager

class DecisionEngine:
    def __init__(self, base_url="http://localhost:8000/v1", model="mistralai/Mistral-7B-Instruct-v0.3"):
        self.client = OpenAI(api_key="EMPTY", base_url=base_url)
        self.model = model
        self.memory = MemoryManager()  # New memory manager!

    def make_decision(self, food, water, energy, nearby_info, temperature=0.0):
        state = f"Food: {food}, Water: {water}, Energy: {energy}, Nearby: {nearby_info}"
        memory_context = self.memory.get_recent_context()

        prompt = f"""
            You are the brain of a survival game player.
            Below is recent memory of the player's past turns.

            {memory_context}

            Now the player has:
            {state}

            Choose the next action. Respond with only one of:
            - MOVE <DIRECTION>
            - REST
            - TRADE
            """

        response = self.client.chat.completions.create(
            model=self.model,
            messages=[
                {"role": "system", "content": "Respond with one valid game action only."},
                {"role": "user", "content": prompt}
            ],
            temperature=temperature,
            max_tokens=50
        )

        raw = response.choices[0].message.content.strip()
        decision = self._extract_action(raw)
        self.memory.add_turn(state, decision)
        return decision
    
    def _extract_action(self, text):
        for line in text.strip().splitlines():
            if any(line.upper().startswith(cmd) for cmd in ["MOVE", "REST", "TRADE"]):
                return line.strip().upper()
        return "REST"  # fallback if nothing valid found
    
