using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CommunicationManager : MonoBehaviour
{
    public string suiteIP = "172.16.136.237";
    public int sendPort = 25567;
    public int receivePort = 25568;

    private UdpClient udpClient;
    private Thread receiveThread;
    public bool isRunning = false;

    public Button button;
    public TMP_InputField inputIP;
    public HapticManager hapticManager;

    void Start()
    {
        inputIP.text = suiteIP;
        //InitUDP();
    }

    void Update()
    {
        
    }

    void InitUDP()
    {
        try
        {
            udpClient = new UdpClient(receivePort);
            isRunning = true;
            receiveThread = new Thread(ReadMessageFromPi) { IsBackground = true };
            receiveThread.Start();
            button.image.color = Color.green;
            Debug.Log("UDP initialized");
        }
        catch (Exception e)
        {
            Debug.LogError($"UDP init failed: {e.Message}");
            button.image.color = Color.red;
        }
    }

    public void ConnectToPi()
    {
        if (!isRunning)
        {
            InitUDP();
            hapticManager.TestSync();
            //SendMessageToPi(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());
        }
        else
        {
            isRunning = false;
            udpClient?.Close();
            receiveThread?.Abort();
            button.image.color = Color.red;
            Debug.Log("UDP stopped");
        }
    }

    public void SendMessageToPi(string message)
    {
        try
        {
            message += "$";
            byte[] data = Encoding.ASCII.GetBytes(message);
            udpClient.Send(data, data.Length, inputIP.text, sendPort);
            Debug.Log($"Sent UDP message: {message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"UDP send error: {e.Message}");
            button.image.color = Color.red;
        }
    }

    void ReadMessageFromPi()
    {
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, receivePort);
        while (isRunning)
        {
            try
            {
                byte[] data = udpClient.Receive(ref remoteEndPoint);
                string message = Encoding.ASCII.GetString(data);
                Debug.Log($"Received from Pi: {message}");
            }
            catch (Exception e)
            {
                if (isRunning) Debug.LogError($"UDP receive error: {e.Message}");
            }
        }
    }

    void OnDestroy()
    {
        isRunning = false;
        udpClient?.Close();
        receiveThread?.Abort();
    }
}
