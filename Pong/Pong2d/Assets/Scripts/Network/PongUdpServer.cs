using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Globalization;

public class PongUdpServer : MonoBehaviour
{
    private UdpClient server;
    private Thread receiveThread;

    private Dictionary<string, int> clientIds =
        new Dictionary<string, int>();

    private Dictionary<int, IPEndPoint> clientEndpoints =
        new Dictionary<int, IPEndPoint>();

    private readonly object clientsLock = new object();

    private int nextId = 1;

    [Header("Paddles")]
    [SerializeField] private Transform player1;
    [SerializeField] private Transform player2;
    [SerializeField] private Transform player3;
    [SerializeField] private Transform player4;

    [Header("Ball")]
    [SerializeField] private Transform ball;

    [Header("Movement")]
    [SerializeField] private float paddleSpeed = 5f;
    [SerializeField] private float minY = -4f;
    [SerializeField] private float maxY = 4f;

    private volatile int player1Input = 0;
    private volatile int player2Input = 0;
    private volatile int player3Input = 0;
    private volatile int player4Input = 0;

    private float stateTimer = 0f;

    private void Start()
    {
        server = new UdpClient(5001);

        receiveThread = new Thread(ReceiveData);
        receiveThread.Start();

        Debug.Log("Servidor Pong iniciado na porta 5001");
    }

    private void Update()
    {
        MovePaddle(player1, player1Input);
        MovePaddle(player2, player2Input);
        MovePaddle(player3, player3Input);
        MovePaddle(player4, player4Input);

        stateTimer += Time.deltaTime;

        if (stateTimer >= 0.05f)
        {
            stateTimer = 0f;
            SendStateToClients();
        }
    }

    private void MovePaddle(
        Transform paddle,
        int input
    )
    {
        if (paddle == null)
        {
            return;
        }

        Vector3 position = paddle.position;

        position.y +=
            input *
            paddleSpeed *
            Time.deltaTime;

        position.y = Mathf.Clamp(
            position.y,
            minY,
            maxY
        );

        paddle.position = position;
    }

    private void ReceiveData()
    {
        IPEndPoint senderEP =
            new IPEndPoint(
                IPAddress.Any,
                0
            );

        while (true)
        {
            try
            {
                byte[] data =
                    server.Receive(ref senderEP);

                string msg =
                    Encoding.UTF8.GetString(data);

                string key =
                    senderEP.Address.ToString() +
                    ":" +
                    senderEP.Port;

                int id;

                lock (clientsLock)
                {
                    if (!clientIds.ContainsKey(key))
                    {
                        id = nextId++;

                        clientIds[key] = id;

                        clientEndpoints[id] =
                            new IPEndPoint(
                                senderEP.Address,
                                senderEP.Port
                            );

                        string assignMsg =
                            "ASSIGN:" + id;

                        byte[] assignData =
                            Encoding.UTF8.GetBytes(
                                assignMsg
                            );

                        server.Send(
                            assignData,
                            assignData.Length,
                            senderEP
                        );

                        Debug.Log(
                            "ASSIGN enviado para " +
                            senderEP.Address +
                            ":" +
                            senderEP.Port +
                            " → " +
                            assignMsg
                        );
                    }
                    else
                    {
                        id = clientIds[key];
                    }
                }

                if (msg == "HELLO PONG")
                {
                    Debug.Log(
                        "Recebido do ID " +
                        id +
                        ": HELLO PONG"
                    );
                }
                else if (msg.StartsWith("INPUT:"))
                {
                    ProcessInput(
                        id,
                        msg
                    );
                }
            }
            catch (SocketException)
            {
                break;
            }
        }
    }

    private void ProcessInput(
        int id,
        string msg
    )
    {
        string[] parts =
            msg.Split(';');

        if (parts.Length != 4)
        {
            return;
        }

        if (id == 1)
        {
            player1Input =
                ConvertInput(parts[1]);

            player2Input =
                ConvertInput(parts[3]);
        }
        else if (id == 2)
        {
            player3Input =
                ConvertInput(parts[1]);

            player4Input =
                ConvertInput(parts[3]);
        }

        Debug.Log(
            "ID " + id +
            " → " +
            parts[0] + ": " + parts[1] +
            " | " +
            parts[2] + ": " + parts[3]
        );
    }

    private int ConvertInput(string input)
    {
        if (input == "UP")
        {
            return 1;
        }

        if (input == "DOWN")
        {
            return -1;
        }

        return 0;
    }

    private void SendStateToClients()
    {
        if (server == null)
        {
            return;
        }

        if (player1 == null ||
            player2 == null ||
            player3 == null ||
            player4 == null ||
            ball == null)
        {
            return;
        }

        string state =
            "STATE:" +
            player1.position.x.ToString("F3", CultureInfo.InvariantCulture) + ";" +
            player1.position.y.ToString("F3", CultureInfo.InvariantCulture) + ";" +
            player2.position.x.ToString("F3", CultureInfo.InvariantCulture) + ";" +
            player2.position.y.ToString("F3", CultureInfo.InvariantCulture) + ";" +
            player3.position.x.ToString("F3", CultureInfo.InvariantCulture) + ";" +
            player3.position.y.ToString("F3", CultureInfo.InvariantCulture) + ";" +
            player4.position.x.ToString("F3", CultureInfo.InvariantCulture) + ";" +
            player4.position.y.ToString("F3", CultureInfo.InvariantCulture) + ";" +
            ball.position.x.ToString("F3", CultureInfo.InvariantCulture) + ";" +
            ball.position.y.ToString("F3", CultureInfo.InvariantCulture) + ";" +
            GameManager.Instance.teamAScore + ";" +
            GameManager.Instance.teamBScore;

        byte[] data =
            Encoding.UTF8.GetBytes(state);

        lock (clientsLock)
        {
            foreach (IPEndPoint endpoint in clientEndpoints.Values)
            {
                try
                {
                    server.Send(
                        data,
                        data.Length,
                        endpoint
                    );
                }
                catch (SocketException)
                {
                    // Cliente pode ter desconectado.
                }
            }
        }
    }

    private void OnApplicationQuit()
    {
        if (server != null)
        {
            server.Close();
        }
    }
}