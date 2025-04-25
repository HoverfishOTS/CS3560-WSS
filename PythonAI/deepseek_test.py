from openai import OpenAI

client = OpenAI(
    api_key="EMPTY",
    base_url="http://localhost:8000/v1",
)

# Construct a prompt manually
prompt = """
You are the AI Brain of a player in a wilderness survival simulation game.

You are given the current status of the player and the environment.
Your task is to choose the next best action for the player to survive and move east.

# Current Status
- Food: 10
- Water: 5
- Energy: 15
- Nearby resources: food to the EAST, trader to the SOUTH

Available actions:
- MOVE <DIRECTION>
- REST
- TRADE

Only output ONE action, and nothing else.
No examples. No explanations. No code. Absolutely no sample outputs. 

Next action:
"""


response = client.completions.create(
    model="deepseek-ai/deepseek-coder-6.7b-base",
    prompt=prompt,
    temperature=0.0,
    max_tokens=150
)

print(response.choices[0].text.strip())
