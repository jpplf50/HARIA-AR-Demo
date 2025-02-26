using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TCPServer : MonoBehaviour
{
    public TextMeshProUGUI gripStrengthText;
    public TextMeshProUGUI activityText;
    public TextMeshProUGUI batteryLevelText;
    public Image statusImage; // Reference to the UI Image
    public TextMeshProUGUI statusText;

    // Colors for different statuses
    public Color safeColor = Color.green;
    public Color cautionColor = Color.yellow;
    public Color dangerColor = Color.red;
    

    private TcpListener server;
    private TcpClient client;
    private NetworkStream stream;
    private byte[] buffer = new byte[1024];

    // Queue for thread-safe UI updates
    private Queue<Action> mainThreadActions = new Queue<Action>();

    void Start()
    {
        StartServer();
    }

    void StartServer()
    {
        try
        {
            server = new TcpListener(IPAddress.Any, 5000); // Listen on port 5000
            server.Start();

            // Log the server start on the main thread
            EnqueueMainThreadAction(() =>
            {
                Debug.Log("Server started...");
                UpdateStatusText("Server started. Waiting for client...");
            });

            server.BeginAcceptTcpClient(new AsyncCallback(OnClientConnected), null);
        }
        catch (Exception e)
        {
            // Log the error on the main thread
            EnqueueMainThreadAction(() =>
            {
                Debug.LogError("Server error: " + e.Message);
                UpdateStatusText("Server error: " + e.Message);
            });
        }
    }

    void OnClientConnected(IAsyncResult result)
    {
        try
        {
            client = server.EndAcceptTcpClient(result);
            stream = client.GetStream();

            // Log the connection on the main thread
            EnqueueMainThreadAction(() =>
            {
                Debug.Log("Client connected.");
                UpdateStatusText("Client connected. Receiving data...");
            });

            // Start reading data
            stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(OnDataReceived), null);
        }
        catch (Exception e)
        {
            // Log the error on the main thread
            EnqueueMainThreadAction(() =>
            {
                Debug.LogError("Error accepting client: " + e.Message);
                UpdateStatusText("Error accepting client: " + e.Message);
            });
        }
    }

    void OnDataReceived(IAsyncResult result)
    {
        try
        {
            int bytesRead = stream.EndRead(result);
            if (bytesRead > 0)
            {
                string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                // Log the received data on the main thread
                EnqueueMainThreadAction(() =>
                {
                    Debug.Log("Received data: " + receivedData);
                    UpdateHUD(receivedData);
                });

                // Continue reading
                stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(OnDataReceived), null);
            }
            else
            {
                // Log the connection closure on the main thread
                EnqueueMainThreadAction(() =>
                {
                    Debug.Log("No data received. Connection may be closed.");
                    UpdateStatusText("Connection closed.");
                });
            }
        }
        catch (Exception e)
        {
            // Log the error on the main thread
            EnqueueMainThreadAction(() =>
            {
                Debug.LogError("Error receiving data: " + e.Message);
                UpdateStatusText("Error receiving data: " + e.Message);
            });
        }
    }

    void UpdateHUD(string data)
    {
        // Split the data into parts
        string[] parts = data.Split(',');

        // Initialize variables
        string gripStrength = "";
        string activity = "";
        string batteryLevel = "";
        string safety = "";

        // Parse each part
        foreach (string part in parts)
        {
            if (part.Contains("Grip Strength"))
            {
                gripStrength = part.Split(':')[1].Trim(); // Extract grip strength
            }
            else if (part.Contains("Currently"))
            {
                activity = part.Split(':')[1].Trim(); // Extract activity
            }
            else if (part.Contains("Battery"))
            {
                batteryLevel = part.Split(':')[1].Trim(); // Extract battery level
            }
            else
            {
                safety = part.Trim(); // Extract emoji (last part)
            }
        }

        // Update the HUD TextMeshPro fields
        if (gripStrengthText != null)
            gripStrengthText.text = $"Grip Strength: {gripStrength}";

        if (activityText != null)
            activityText.text = $"Activity: {activity}";

        if (batteryLevelText != null)
            batteryLevelText.text = $"Battery: {batteryLevel}";

        switch (safety)
        {
            case "safe":
                statusImage.color = safeColor;
                break;
            case "caution":
                statusImage.color = cautionColor;
                break;
            case "danger":
                statusImage.color = dangerColor;
                break;
            default:
                Debug.LogWarning("Unknown status: " + safety);
                break;
        }

        // Update the status text
        UpdateStatusText("Connected: Receiving data...");
    }

    void UpdateStatusText(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    void EnqueueMainThreadAction(Action action)
    {
        // Add the action to the queue
        lock (mainThreadActions)
        {
            mainThreadActions.Enqueue(action);
        }
    }

    void Update()
    {
        // Process all actions in the queue on the main thread
        lock (mainThreadActions)
        {
            while (mainThreadActions.Count > 0)
            {
                Action action = mainThreadActions.Dequeue();
                action.Invoke();
            }
        }
    }

    void OnDestroy()
    {
        if (client != null) client.Close();
        if (server != null) server.Stop();

        UpdateStatusText("Server stopped.");
    }
}