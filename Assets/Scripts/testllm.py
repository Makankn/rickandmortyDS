from transformers import AutoTokenizer, AutoModelForCausalLM, TextStreamer
import torch
import json

class MistralDialogueGenerator:
    def __init__(self, model_id="mistralai/Mistral-7B-Instruct-v0.2"):
        print("🧠 Loading Mistral model...")
        self.tokenizer = AutoTokenizer.from_pretrained(model_id)
        self.model = AutoModelForCausalLM.from_pretrained(
            model_id,
            torch_dtype=torch.float16 if torch.cuda.is_available() else torch.float32,
            device_map="auto"
        )
        print("✅ Model loaded.")

    @staticmethod
    def build_prompt(topic: str) -> str:
        return f"""You are writing a JSON-style Rick and Morty dialogue script based on the topic: "{topic}".
Format the output as a JSON array of objects. Each object must have:
- "speaker": either "Rick" or "Morty"
- "text": the dialogue

Rules:
- Start with Rick.
- Alternate between Rick and Morty.
- Generate 10 total messages (5 each).
- Stay funny, chaotic, and on-topic.

Only output valid JSON. No extra explanation or markdown.

Topic: {topic}
"""

    def generate_dialogue(self, topic: str, output_file: str = "mistral_rick_morty.json"):
        prompt = self.build_prompt(topic)
        device = "cuda" if torch.cuda.is_available() else "cpu"
        inputs = self.tokenizer(prompt, return_tensors="pt").to(device)

        output = self.model.generate(
            **inputs,
            max_new_tokens=600,
            do_sample=True,
            temperature=0.7,
            streamer=TextStreamer(self.tokenizer, skip_prompt=True)
        )

        result = self.tokenizer.decode(output[0], skip_special_tokens=True)

        # Try extracting valid JSON
        try:
            start = result.index('[')
            end = result.rindex(']') + 1
            dialogue_json = result[start:end]
            dialogue = json.loads(dialogue_json)

            with open(output_file, "w", encoding="utf-8") as f:
                json.dump(dialogue, f, indent=2)

            print(f"✅ Dialogue saved to {output_file}")
            return dialogue
        except Exception as e:
            print("❌ Failed to parse JSON from model output.")
            print(result)
            raise e
