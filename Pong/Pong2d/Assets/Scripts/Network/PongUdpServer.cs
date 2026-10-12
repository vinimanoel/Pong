using UnityEngine;
using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;

public class PongUdpServer : MonoBehaviour
{
    private const int MaxClients = 4;

    public static PongUdpServer Instance { get; private set; }

    private UdpClient server;
    private Thread receiveThread;
    private volatile bool running;

    private readonly Dictionary<string, int> clientIds =
        new Dictionary<string, int>();

    private readonly Dictionary<int, IPEndPoint> clientEndpoints =
        new Dictionary<int, IPEndPoint>();

    private readonly object clientsLock = new object();
    private readonly object sendLock = new object();

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

    private volatile int player1Input;
    private volatile int player2Input;
    private volatile int player3Input;
    private volatile int player4Input;

    private float stateTimer;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        try
        {
            server = new UdpClient(5001);
            running = true;
            receiveThread = new Thread(ReceiveData)
            {
                IsBackground = true,
                Name = "Pong UDP receive"
            };
            receiveThread.Start();

            Debug.Log("Servidor Pong iniciado na porta 5001");
        }
        catch (SocketException exception)
        {
            Debug.LogError("Não foi possível iniciar o servidor Pong: " + exception.Message);
            enabled = false;
        }
    }

    private void Update()
    {
        MovePaddle(player1, player1Input);
        MovePaddle(player2, player2Input);
        MovePaddle(player3, player3Input);
        MovePaddle(player4, player4Input);

        stateTimer += Time.unscaledDeltaTime;
        if (stateTimer >= 0.05f)
        {
            stateTimer = 0f;
            SendStateToClients();
        }
    }

    private void MovePaddle(Transform paddle, int input)
    {
        if (paddle == null)
        {
            return;
        }

        Vector3 position = paddle.position;
        position.y = Mathf.Clamp(
            position.y + input * paddleSpeed * Time.deltaTime,
            minY,
            maxY
        );
        paddle.position = position;
    }

    private void ReceiveData()
    {
        UdpClient socket = server;
        if (socket == null)
        {
            return;
        }

        while (running)
        {
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            byte[] data;

            try
            {
                data = socket.Receive(ref sender);
            }
            catch (SocketException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }

            string message = Encoding.UTF8.GetString(data);
            string key = sender.Address + ":" + sender.Port;

            if (message.StartsWith("JOIN:", StringComparison.Ordinal))
            {
                AssignPlayer(key, sender, message.Substring(5));
                continue;
            }

            int id;
            lock (clientsLock)
            {
                if (!clientIds.TryGetValue(key, out id))
                {
                    continue;
                }
            }

            if (message.StartsWith("INPUT:", StringComparison.Ordinal))
            {
                ProcessInput(id, message.Substring(6));
            }
            else if (message == "START" || message == "RESTART")
            {
                Broadcast(message);
            }
        }
    }

    private void AssignPlayer(string key, IPEndPoint endpoint, string selection)
    {
        int desiredSlot;
        lock (clientsLock)
        {
            clientIds.TryGetValue(key, out int currentSlot);

            if (selection == "A" || selection == "B")
            {
                desiredSlot = FindAvailableSlot(selection, currentSlot);
            }
            else if (!int.TryParse(selection, out desiredSlot) ||
                     desiredSlot < 1 || desiredSlot > MaxClients)
            {
                desiredSlot = 0;
            }

            if (desiredSlot <= 0 ||
                (desiredSlot != currentSlot && clientEndpoints.ContainsKey(desiredSlot)))
            {
                SendMessage("PLAYER_TAKEN:" + currentSlot, endpoint);
                return;
            }

            if (currentSlot > 0 && currentSlot != desiredSlot)
            {
                clientEndpoints.Remove(currentSlot);
                SetPlayerInput(currentSlot, 0);
            }

            clientIds[key] = desiredSlot;
            clientEndpoints[desiredSlot] =
                new IPEndPoint(endpoint.Address, endpoint.Port);

            SendMessage("ASSIGN:" + desiredSlot, endpoint);
        }
    }
    private int FindAvailableSlot(string team, int currentSlot)
    {
        int first = team == "A" ? 1 : 3;
        int last = team == "A" ? 2 : 4;

        if (currentSlot >= first && currentSlot <= last)
        {
            return currentSlot;
        }

        for (int slot = first; slot <= last; slot++)
        {
            if (!clientEndpoints.ContainsKey(slot))
            {
                return slot;
            }
        }

        return 0;
    }

    private void ProcessInput(int id, string input)
    {
        int value = input == "UP" ? 1 : input == "DOWN" ? -1 : 0;
        SetPlayerInput(id, value);
    }

    private void SetPlayerInput(int id, int value)
    {
        switch (id)
        {
            case 1: player1Input = value; break;
            case 2: player2Input = value; break;
            case 3: player3Input = value; break;
            case 4: player4Input = value; break;
        }
    }

    private void SendStateToClients()
    {
        if (server == null ||
            player1 == null ||
            player2 == null ||
            player3 == null ||
            player4 == null ||
            ball == null)
        {
            return;
        }

        int scoreA = GameManager.Instance != null
            ? GameManager.Instance.teamAScore
            : 0;
        int scoreB = GameManager.Instance != null
            ? GameManager.Instance.teamBScore
            : 0;

        string state =
            "STATE:" +
            Format(player1.position.x) + ";" + Format(player1.position.y) + ";" +
            Format(player2.position.x) + ";" + Format(player2.position.y) + ";" +
            Format(player3.position.x) + ";" + Format(player3.position.y) + ";" +
            Format(player4.position.x) + ";" + Format(player4.position.y) + ";" +
            Format(ball.position.x) + ";" + Format(ball.position.y) + ";" +
            scoreA + ";" + scoreB;

        BroadcastBytes(Encoding.UTF8.GetBytes(state));
    }

    public void BroadcastGameOver(string winner, int scoreA, int scoreB)
    {
        if (isActiveAndEnabled)
        {
            Broadcast("END:" + winner + ":" + scoreA + ":" + scoreB);
        }
    }

    private void Broadcast(string message)
    {
        BroadcastBytes(Encoding.UTF8.GetBytes(message));
    }

    private void BroadcastBytes(byte[] data)
    {
        lock (clientsLock)
        {
            foreach (IPEndPoint endpoint in clientEndpoints.Values)
            {
                SendBytes(data, endpoint);
            }
        }
    }

    private string Format(float value)
    {
        return value.ToString("F3", CultureInfo.InvariantCulture);
    }

    private void SendMessage(string message, IPEndPoint endpoint)
    {
        SendBytes(Encoding.UTF8.GetBytes(message), endpoint);
    }

    private void SendBytes(byte[] data, IPEndPoint endpoint)
    {
        if (server == null)
        {
            return;
        }

        try
        {
            lock (sendLock)
            {
                server.Send(data, data.Length, endpoint);
            }
        }
        catch (SocketException)
        {
            // O cliente pode ter sido desconectado.
        }
        catch (ObjectDisposedException)
        {
            // O aplicativo está encerrando.
        }
    }

    private void OnDestroy()
    {
        running = false;

        if (server != null)
        {
            server.Close();
        }

        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Join(500);
        }

        server = null;
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
