# 🧪 Rick and Morty AI Scene Generator

This project is a Unity-based interactive scene generator that creates dynamic Rick and Morty conversations based on any topic you provide. Using AI for script generation, text-to-speech voice cloning, and animated 3D characters, it brings original episodes to life — all fully automated from prompt to animation.

## 🚀 Features

- 🎤 **AI Dialogue Generation**: Uses [Mistral-7B-Instruct](https://huggingface.co/mistralai/Mistral-7B-Instruct-v0.2) on Hugging Face to generate JSON-format Rick and Morty conversations.
- 🗣️ **Animation Scene Control using LLMS**: Uses [TinyLlama-1.1B](https://huggingface.co/TinyLlama/TinyLlama-1.1B-Chat-v1.0) to Control.
- 🗣️ **Voice Cloning with FakeYou**: Clones Rick and Morty's voices using FakeYou TTS API.
- 🎭 **3D Character Animation in Unity**:
  - Characters dynamically switch between idle, walking, angry, and talking animations.
  - Walking animation moves characters toward random waypoints in the scene.
  - Characters face each other and animate in sync with the dialogue.
- 🧾 **Subtitle System**: Shows real-time subtitles while the characters talk.
- 🔄 **End-to-End Pipeline**: Unity triggers the entire process from input → dialogue generation → TTS → animation and playback.

## 🧩 Tech Stack

- **Unity**: Scene, character control, animation logic.
- **Python**: Scripts for AI generation and voice synthesis.
  - [`huggingface_hub`](https://pypi.org/project/huggingface-hub/)
  - [`aiohttp`](https://pypi.org/project/aiohttp/)
  - [`dotenv`](https://pypi.org/project/python-dotenv/)
- **LLM Model**: [`mistralai/Mistral-7B-Instruct-v0.2`](https://huggingface.co/mistralai/Mistral-7B-Instruct-v0.2) and [TinyLlama-1.1B](https://huggingface.co/TinyLlama/TinyLlama-1.1B-Chat-v1.0)
- **TTS Provider**: [FakeYou.com](https://fakeyou.com/)
- **Animation**: Unity Animator with triggers: `isTalking`, `isWalking`, `isAngry`.

---



## 🔐 Environment Variables

Create a `.env` file in the root with the following:

```dotenv
USERNAME=your_fakeyou_email
PASSWORD=your_fakeyou_password
````

---

## 🛠️ Requirements

* Python 3.10+
* Unity 2022 or newer
* Python packages:

```bash
pip install huggingface_hub aiohttp python-dotenv
```

---

## ▶️ Running the Full Flow

1. **In Unity**:

   * Press `Start`.
   * Input a topic like "space lizards".
   * Unity will run the generation and then auto-play the animated scene.




