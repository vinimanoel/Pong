using UnityEngine;
using UnityEngine.InputSystem;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class PongUdpClient : MonoBehaviour
{
    private UdpClient client;
    private IPEndPoint serverEP;

    private int myId = -1;
    private float helloTimer = 0f;

    private void Start()
    {
        client = new UdpClient();

        serverEP = new IPEndPoint(
            IPAddress.Parse("127.0.0.1"),
            5001
        );

        client.Connect(serverEP);

        Debug.Log("Cliente Pong conectado ao servidor");

        SendHello();
    }

    private void Update()
    {
        // Envia HELLO novamente até receber o ID.
        if (myId == -1)
        {
            helloTimer += Time.deltaTime;

            if (helloTimer >= 1f)
            {
                helloTimer = 0f;
                SendHello();
            }
        }

        // Verifica se existe algum pacote esperando.
        if (client.Available > 0)
        {
            try
            {
                IPEndPoint remoteEP = new IPEndPoint(
                    IPAddress.Any,
                    0
                );

                byte[] data = client.Receive(ref remoteEP);

                string msg = Encoding.UTF8.GetString(data);

                Debug.Log("Cliente recebeu: " + msg);

                if (msg.StartsWith("ASSIGN:"))
                {
                    myId = int.Parse(msg.Substring(7));

                    Debug.Log("Recebi meu ID: " + myId);
                }
            }
            catch (SocketException)
            {
                // Socket foi fechado ou interrompido.
            }
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("Meu ID atual: " + myId);
        }
    }

    private void SendHello()
    {
        string msg = "HELLO PONG";

        byte[] data = Encoding.UTF8.GetBytes(msg);

        client.Send(data, data.Length);

        Debug.Log("HELLO enviado ao servidor");
    }

    private void OnApplicationQuit()
    {
        client.Close();
    }
}