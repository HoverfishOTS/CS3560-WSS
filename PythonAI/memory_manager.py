class MemoryManager:
    def __init__(self):
        self.history = []

    def add_turn(self, state, decision):
        entry = f"Turn:\nState: {state}\nDecision: {decision}"
        self.history.append(entry)

    def get_recent_context(self, max_turns=5):
        return "\n\n".join(self.history[-max_turns:])

    def reset(self):
        self.history.clear()
