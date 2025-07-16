using UnityEngine;
using System.Diagnostics;
using System.IO;
using System.Threading;

public class RunPythonTTS : MonoBehaviour
{
    public string pythonScriptPath = "Assets/Scripts/testapi.py";

    void Start()
    {
        // Launch on background thread
        Thread thread = new Thread(RunPythonThread);
        thread.Start();
    }

    void RunPythonThread()
    {
        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = "python";
        psi.Arguments = $"\"{pythonScriptPath}\"";
        psi.UseShellExecute = false;
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;
        psi.CreateNoWindow = true;

        Process process = new Process();
        process.StartInfo = psi;

        process.OutputDataReceived += (sender, args) => {
            if (!string.IsNullOrEmpty(args.Data))
                UnityMainThreadDispatcher.Instance().Enqueue(() => UnityEngine.Debug.Log("PYTHON: " + args.Data));
        };

        process.ErrorDataReceived += (sender, args) => {
            if (!string.IsNullOrEmpty(args.Data))
                UnityMainThreadDispatcher.Instance().Enqueue(() => UnityEngine.Debug.LogError("PYTHON ERROR: " + args.Data));
        };


        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        process.WaitForExit();
    }
}