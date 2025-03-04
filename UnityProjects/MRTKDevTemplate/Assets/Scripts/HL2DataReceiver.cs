using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;  // Import TextMeshPro
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class HL2DataReceiver : MonoBehaviour
{
    public TMP_Text nameText;        // UI text to display name (TMP)
    public TMP_Text protocolText;    // UI text to display protocol (TMP)
    public TMP_Text runNumberText;   // UI text to display run number (TMP)
    public GameObject startButton;   // Button to start experiment

    private string receivedName = "Waiting for data...";
    private string receivedProtocol = "Waiting for data...";
    private int runNumber = 1;  // Default to Scene1

    private TcpListener listener;
    private Thread serverThread;
    private bool dataReceived = false;

    void Start()
    {
        // Start the TCP listener in a separate thread
        serverThread = new Thread(StartListening);
        serverThread.IsBackground = true;
        serverThread.Start();

        // Hide the Start Button until data is received
        startButton.SetActive(false);
    }

    void Update()
    {
        if (dataReceived)
        {
            // Update TMP UI with received values
            nameText.text = $"Name: {receivedName}";
            protocolText.text = $"Protocol: {receivedProtocol}";
            runNumberText.text = $"Run: {runNumber}";

            // Show Start Button
            startButton.SetActive(true);

            // Reset flag
            dataReceived = false;
        }
    }

    void StartListening()
    {
        try
        {
            int port = 5005;  // Match the port from the Python server
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Debug.Log("?? Listening for data from PC...");

            while (true)
            {
                using (TcpClient client = listener.AcceptTcpClient())
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    Debug.Log($"?? Received: {message}");

                    // Parse JSON
                    ProcessReceivedData(message);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"? Error in server: {e.Message}");
        }
    }

    void ProcessReceivedData(string json)
    {
        try
        {
            // Parse JSON manually (or use Newtonsoft.Json if available)
            string[] parts = json.Trim(new char[] { '{', '}' }).Split(',');

            foreach (string part in parts)
            {
                string[] keyValue = part.Split(':');
                string key = keyValue[0].Trim('"', ' ');
                string value = keyValue[1].Trim('"', ' ');

                if (key == "name") receivedName = value;
                else if (key == "protocol") receivedProtocol = value;
                else if (key == "run") int.TryParse(value, out runNumber);
            }

            dataReceived = true; // Trigger UI update in main thread
        }
        catch (Exception e)
        {
            Debug.LogError($"? JSON Parsing Error: {e.Message}");
        }
    }

    public void StartExperiment()
    {
        // Load scene based on received Run number
        string sceneToLoad = runNumber == 1 ? "Scene1" :
                             runNumber == 2 ? "Scene2" :
                             runNumber == 3 ? "Scene3" : "ExperimentStartMenu";

        Debug.Log($"?? Starting experiment: Loading {sceneToLoad}");
        SceneManager.LoadScene(sceneToLoad);
    }

    void OnApplicationQuit()
    {
        if (listener != null)
        {
            listener.Stop();
            serverThread.Abort();
        }
    }
}
