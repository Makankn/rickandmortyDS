using UnityEngine;
using System.IO;

public class DialogueLoader : MonoBehaviour
{
    public string fileName = "dialogue.json";
    public DialogueData dialogue;

    void Start()
    {
        string path = Path.Combine(Application.streamingAssetsPath, fileName);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            DialogueLine[] lines = JsonHelper.FromJson<DialogueLine>(json);

            dialogue = new DialogueData();
            dialogue.lines = lines;

            Debug.Log("✅ Loaded " + dialogue.lines.Length + " dialogue lines.");
            foreach (var line in dialogue.lines)
            {
                Debug.Log($"{line.speaker}: {line.text}");
            }
        }
        else
        {
            Debug.LogError("❌ File not found: " + path);
        }
    }
}