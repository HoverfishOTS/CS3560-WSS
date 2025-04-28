from decision_engine import DecisionEngine

engine = DecisionEngine(model="meta-llama/Llama-3.1-8B-Instruct")

decision = engine.make_decision(
    food=10,
    water=8,
    energy=20,
    nearby_info="food EAST, trader SOUTH",
    temperature=0.5
    )

print("AI Decision:", decision)
