import json
import subprocess
import sys
from huggingface_hub import InferenceClient

# Step 1: Connect to HuggingFace Inference API
client = InferenceClient(
    model="mistralai/Mistral-7B-Instruct-v0.2",
    token=
)


topic = "Natural Language Processing"

# Step 3: Set up prompt
messages = [
    {
        "role": "system",
        "content": (
            "You are writing a Rick and Morty scene as a JSON array.\n"
            "Each entry must contain:\n"
            "- 'speaker': either 'Rick' or 'Morty'\n"
            "- 'text': a witty, chaotic, hilarious line related to the topic\n"
            "There must be 20 entries, starting with Rick and alternating between Rick and Morty.\n"
            "Return ONLY valid JSON, no extra text."
        ),
    },
    {
        "role": "user",
        "content": f"Generate a funny Rick and Morty dialogue about {topic}.",
    },
]

# Step 4: Make the API call
completion = client.chat_completion(messages=messages, max_tokens=2048)
raw_output = completion.choices[0].message["content"]
print("Raw Output:\n", raw_output)

# Step 5: Try parsing or fixing the output
def try_fix_and_parse(raw_text):
    try:
        return json.loads(raw_text)
    except json.JSONDecodeError:
        print("Malformed JSON, attempting to fix manually...")

    lines = raw_text.strip().splitlines()
    dialogue = []
    current = {}

    for line in lines:
        line = line.strip().strip(',')
        if line.startswith('"speaker"') or line.startswith("'speaker'"):
            if current:
                dialogue.append(current)
                current = {}
            current["speaker"] = line.split(":", 1)[1].strip().strip('"').strip("'")
        elif line.startswith('"text"') or line.startswith("'text'"):
            current["text"] = line.split(":", 1)[1].strip().strip('"').strip("'")
    if current:
        dialogue.append(current)

    return dialogue

# Step 6: Save the fixed version
fixed_dialogue = try_fix_and_parse(raw_output)

with open("Assets/StreamingAssets/dialogue.json", "w", encoding="utf-8") as f:
    json.dump(fixed_dialogue, f, indent=2, ensure_ascii=False)

print("✅ Saved cleaned JSON to: dialogue.json")

# Step 7: Call testapi.py
try:
    print("🎙️ Calling TTS script: testapi.py")
    subprocess.run(["python", "Assets/Scripts/testapi.py"], check=True)
    print("✅ testapi.py completed")
except subprocess.CalledProcessError as e:
    print(f"❌ testapi.py failed: {e}")
