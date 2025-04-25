from openai import OpenAI

client = OpenAI(
    api_key="EMPTY",
    base_url="http://localhost:8000/v1"
)

response = client.chat.completions.create(
    model="mistralai/Mistral-7B-Instruct-v0.3",
    messages=[
        {"role": "system", "content": "You are a decision-making AI for a wilderness survival game. You must return only one of the following actions: MOVE <DIRECTION>, REST, or TRADE."},
        {"role": "user", "content": "The player has 10 food, 5 water, and 15 energy. There is food to the EAST and a trader to the SOUTH. What should the player do?"}
    ],
    temperature=0.7,
    max_tokens=50
)

print(response.choices[0].message.content.strip())
