using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;
using System;

public class MyButtonTrigger : MonoBehaviour
{
    public Button myButton;
    public CameraZoomController zoomController; // 👈 Reference to your zoom script

    void Start()
    {
        myButton.onClick.AddListener(OnButtonPressed);
    }

    void OnButtonPressed()
    {
        UnityEngine.Debug.Log("Button Pressed!");
        
        myButton.gameObject.SetActive(false);
        // Trigger the camera zoom
        if (zoomController != null)
        {
            zoomController.StartZoomOut();
        }

        // Run Python script
        string pythonExe = @"C:/Users/makan/AppData/Local/Programs/Python/Python311/python.exe";
        string scriptPath = @"C:/Users/makan/rickAndMorty/Assets/Scripts/testapi.py";

        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = pythonExe;
        psi.Arguments = $"\"{scriptPath}\"";
        psi.UseShellExecute = false;
        psi.CreateNoWindow = true;
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;

        Process process = new Process();
        process.StartInfo = psi;
        process.EnableRaisingEvents = true;

        process.OutputDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
                UnityEngine.Debug.Log("PYTHON: " + args.Data);
        };

        process.ErrorDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
                UnityEngine.Debug.LogError("PYTHON ERROR: " + args.Data);
        };

        process.Exited += (sender, args) =>
        {
            UnityEngine.Debug.Log("Python script finished.");
        };

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("Failed to run Python script: " + ex.Message);
        }
        
    }
}