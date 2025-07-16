using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class DialogueRequest : MonoBehaviour
{
    public string topic = "Interdimensional travel";

    void Start()
    {
        StartCoroutine(SendTopic(topic));
    }

    IEnumerator SendTopic(string topic)
    {
        string url = "http://localhost:5000/generate";
        string json = $"{{\"topic\": \"{topic}\"}}";

        using (UnityWebRequest req = UnityWebRequest.Put(url, json))
        {
            req.method = UnityWebRequest.kHttpVerbPOST;
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
                Debug.Log("✅ Dialogue: " + req.downloadHandler.text);
            else
                Debug.LogError("❌ Error: " + req.error);
        }
    }
}