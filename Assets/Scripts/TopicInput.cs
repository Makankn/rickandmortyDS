using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;
using TMPro;


public class TopicInput : MonoBehaviour
{
    public TMP_InputField topicInput;

    public void OnGeneratePressed()
    {
        string topic = topicInput.text;
        string pythonPath = @"C:\Users\makan\AppData\Local\Programs\Python\Python311\python.exe"; // Change if needed
        string scriptPath = @"C:\Users\makan\rickAndMorty\Assets\Scripts\testllm2.py"; // Path to your script

        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = pythonPath;
        start.Arguments = $"\"{scriptPath}\" \"{topic}\"";
        start.UseShellExecute = false;
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true;
        
    }
}