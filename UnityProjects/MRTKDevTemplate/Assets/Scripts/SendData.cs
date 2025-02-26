using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.Text;
using TMPro;

public class SendData : MonoBehaviour
{
    public TMP_InputField nameInput;
    public TMP_InputField runTimeInput;
    public TMP_InputField protocolInput;
    public Button sendButton;
    public TextMeshProUGUI statusText;  // Optional for displaying status

    private string serverIP = "127.0.0.1";  // Python Server (Localhost)
    private int serverPort = 5005;  // Port for TCP connection

    void Start()
    {
        sendButton.onClick.AddListener(SendMessageToServer);
    }

    void SendMessageToServer()
    {
        // Create JSON message
        string message = $"{{ \"Name\": \"{nameInput.text}\", \"RunTime\": \"{runTimeInput.text}\", \"Protocol\": \"{protocolInput.text}\" }}";

        try
        {
            // Create TCP Client
            TcpClient client = new TcpClient(serverIP, serverPort);
            NetworkStream stream = client.GetStream();

            // Convert message to byte array
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);

            // Display success message
            statusText.text = "Data sent successfully!";
            Debug.Log("Sent: " + message);

            // Close connection
            stream.Close();
            client.Close();
        }
        catch (System.Exception e)
        {
            // Display error
            statusText.text = "Error: " + e.Message;
            Debug.LogError("Error sending data: " + e.Message);
        }
    }
}
