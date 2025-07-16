import asyncio
import aiohttp
import uuid
import os
import json
import sys

sys.stdout.reconfigure(encoding='utf-8')

# ────────── CONFIGURATION ──────────


USERNAME = 
PASSWORD = 

VOICE_TOKENS = {
    "Rick": "weight_0f762jdzgsy1dhpb86qxy4ssm",
    "Morty": "weight_w4c4frd1vbnzg6gtctt6newt5"
}

DIALOGUE_PATH = "Assets\StreamingAssets\dialogue.json"
OUTPUT_DIR = "Assets\Resources\AudioClips"
os.makedirs(OUTPUT_DIR, exist_ok=True)

MAX_ACTIVE_JOBS = 2
SUBMIT_DELAY = 2.5  # Delay between each submission (prevents throttling)

# ────────── AUTH ──────────

async def login(session):
    async with session.post("https://api.fakeyou.com/v1/login", json={
        "username_or_email": USERNAME,
        "password": PASSWORD
    }) as resp:
        data = await resp.json()
        if not data.get("success"):
            raise Exception("❌ Login failed.")
        print("✅ Logged in.")

# ────────── TTS PROCESS ──────────

async def process_line(session, idx, speaker, text):
    voice_token = VOICE_TOKENS.get(speaker)
    if not voice_token:
        print(f"❌ Unknown speaker: {speaker}")
        return

    print(f"[{speaker}] 🎙️ Generating: {text}")

    # Submit job with retries
    job_token = None
    for attempt in range(10):
        try:
            job_uuid = str(uuid.uuid4())
            async with session.post("https://api.fakeyou.com/v1/tts/inference", json={
                "tts_model_token": voice_token,
                "uuid_idempotency_token": job_uuid,
                "inference_text": text
            }) as job_resp:
                job_data = await job_resp.json()
                job_token = job_data.get("inference_job_token")

            if job_token:
                break
        except Exception as e:
            print(f"❌ Error on job start (attempt {attempt + 1}): {e}")
        await asyncio.sleep(2 * (attempt + 1))  # Backoff

    if not job_token:
        print("❌ Giving up on job start.")
        return

    # Poll for job status
    audio_url = None
    for _ in range(30):
        await asyncio.sleep(3)
        try:
            async with session.get(f"https://api.fakeyou.com/tts/job/{job_token}") as status_resp:
                status_data = await status_resp.json()
                state = status_data.get("state", {})
                status = state.get("status")

                if status == "complete_success":
                    path = state.get("maybe_public_bucket_wav_audio_path")
                    if path:
                        audio_url = f"https://cdn-2.fakeyou.com{path}"
                    break
                elif status in ["dead", "failed"]:
                    print(f"❌ TTS failed for {speaker}.")
                    return
        except Exception as e:
            print(f"❌ Polling error: {e}")

    # Download audio
    if audio_url:
        try:
            async with session.get(audio_url) as audio_resp:
                audio_bytes = await audio_resp.read()
                filename = os.path.join(OUTPUT_DIR, f"{speaker}_{idx:04}.wav")
                with open(filename, "wb") as f:
                    f.write(audio_bytes)
                print(f"💾 Saved: {filename}")
        except Exception as e:
            print(f"❌ Failed to save: {e}")
    else:
        print("❌ No audio to download.")

# ────────── QUEUE SYSTEM ──────────

async def queued_job(session, semaphore, idx, speaker, text):
    async with semaphore:
        await process_line(session, idx, speaker, text)
        await asyncio.sleep(SUBMIT_DELAY)

# ────────── MAIN ──────────

async def main():
    async with aiohttp.ClientSession() as session:
        await login(session)

        with open(DIALOGUE_PATH, "r", encoding="utf-8") as f:
            lines = json.load(f)

        semaphore = asyncio.Semaphore(MAX_ACTIVE_JOBS)

        tasks = [
            asyncio.create_task(
                queued_job(session, semaphore, idx, line["speaker"], line["text"])
            )
            for idx, line in enumerate(lines)
        ]

        await asyncio.gather(*tasks)

# ────────── RUN ──────────

if __name__ == "__main__":
    asyncio.run(main())
