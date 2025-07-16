using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System;

public class WavFilePoller : MonoBehaviour
{
    public TextMeshProUGUI statusText;
    public Button startButton;
    public DialogueManager dialogueManager; // 🆕 Reference to DialogueManager
    private int totalExpected = 0;

    private string folderPath = @"C:\Users\makan\rickAndMorty\Assets\Resources\AudioClips";
    
    private Dictionary<string, DateTime> seenFiles = new Dictionary<string, DateTime>();
    private float checkInterval = 1f;
    private float timer = 0f;
    private double minAgeSeconds = 2.0;

    void Start()
    {
        // Load dialogue to determine how many files to expect
        string jsonPath = Path.Combine(Application.streamingAssetsPath, "dialogue.json");
        if (File.Exists(jsonPath))
        {
            string json = File.ReadAllText(jsonPath);
            var lines = JsonHelper.FromJson<DialogueLine>(json);
            totalExpected = lines.Length;
        }
        else
        {
            Debug.LogError("❌ dialogue.json not found to calculate totalExpected");
            return;
        }

        if (!Directory.Exists(folderPath))
        {
            Debug.LogError("❌ Directory does not exist: " + folderPath);
            return;
        }

        // Clear .wav files
        foreach (string file in Directory.GetFiles(folderPath, "*.wav"))
        {
            try { File.Delete(file); } catch (Exception e) { Debug.LogError($"❌ {file}: {e.Message}"); }
        }
        // Clear .meta files
        foreach (string file in Directory.GetFiles(folderPath, "*.meta"))
        {
            try { File.Delete(file); } catch (Exception e) { Debug.LogError($"❌ {file}: {e.Message}"); }
        }
        seenFiles.Clear();
        UpdateUIText();
        Debug.Log("🔄 Folder cleared. Starting polling...");
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= checkInterval)
        {
            timer = 0f;
            PollFolder();
        }
    }

    void PollFolder()
    {
        string[] wavFiles = Directory.GetFiles(folderPath, "*.wav");

        foreach (string filePath in wavFiles)
        {
            string fileName = Path.GetFileName(filePath);
            if (!seenFiles.ContainsKey(fileName))
            {
                DateTime modifiedTime = File.GetLastWriteTime(filePath);
                TimeSpan age = DateTime.Now - modifiedTime;

                if (age.TotalSeconds >= minAgeSeconds)
                {
                    seenFiles[fileName] = modifiedTime;
                    Debug.Log($"📢 New stable voice file: {fileName}");
                    UpdateUIText();
                }
            }
        }

        // ✅ All expected .wav files found
        if (seenFiles.Count >= totalExpected)
        {
            Debug.Log("✅ All .wav files ready!");
            enabled = false; // stop polling

            // ⏩ Trigger dialogue playback
            dialogueManager.BeginDialogue();
        }
    }

    void UpdateUIText()
    {
        if (statusText != null)
        {
            int count = seenFiles.Count;
            statusText.text = $"{count}/{totalExpected} files generated";
            statusText.gameObject.SetActive(count < totalExpected);
        }
    }

    public void OnStartButtonClicked()
    {
        if (startButton != null)
            startButton.gameObject.SetActive(false);

        Debug.Log("▶️ Start button pressed. Starting process...");
    }
}
