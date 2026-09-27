using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;

public class PongUdpServer : MonoBehaviour
{
    private UdpClient server;
    private Thread receiveThread;

    private Dictionary<string, int> clientIds = new Dictionary<string, int>();
    private int nextId = 1;

    private void Start()
    {
        server = new UdpClient(5001);

        receiveThread = new Thread(ReceiveData);
        receiveThread.Start();

        Debug.Log("Servidor Pong iniciado na porta 5001");
    }

    private void ReceiveData()
    {
        IPEndPoint senderEP = new IPEndPoint(IPAddress.Any, 0);

        while (true)
        {
            try
            {
                byte[] data = server.Receive(ref senderEP);

                string msg = Encoding.UTF8.GetString(data);

                string key = senderEP.Address.ToString() + ":" + senderEP.Port;

                if (!clientIds.ContainsKey(key))
                {
                    clientIds[key] = nextId++;

                    string assignMsg = "ASSIGN:" + clientIds[key];

                    byte[] assignData = Encoding.UTF8.GetBytes(assignMsg);

                    server.Send(assignData, assignData.Length, senderEP);

                    Debug.Log(
                        "ASSIGN enviado para " +
                        senderEP.Address + ":" +
                        senderEP.Port +
                        " → " +
                        assignMsg
                    );
                }

                int id = clientIds[key];

                Debug.Log("Recebido do ID " + id + ": " + msg);
            }
            catch (SocketException)
            {
                break;
            }
        }
    }

}