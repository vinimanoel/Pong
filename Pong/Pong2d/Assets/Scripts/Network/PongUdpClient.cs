using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class PongUdpClient : MonoBehaviour
{
    private UdpClient client;
    private IPEndPoint serverEP;

    [SerializeField] private string serverIp = "127.0.0.1";
    [SerializeField] private int serverPort = 5001;

    [Header("Paddles")]
    [SerializeField] private Transform player1;
    [SerializeField] private Transform player2;
    [SerializeField] private Transform player3;
    [SerializeField] private Transform player4;

    [Header("Ball")]
    [SerializeField] private Transform ball;

    [Header("Score")]
    [SerializeField] private ScoreUI scoreUI;

    private int assignedSlot = -1;
    private int requestedSlot;
    private float joinTimer;
    private float inputTimer;

    private Vector3 targetPlayer1;
    private Vector3 targetPlayer2;
    private Vector3 targetPlayer3;
    private Vector3 targetPlayer4;
    private Vector3 targetBall;

    public int AssignedSlot => assignedSlot;

    private void OnEnable()
    {
        assignedSlot = -1;
        requestedSlot = 0;
        joinTimer = 0f;
        inputTimer = 0f;

        scoreUI = scoreUI != null
            ? scoreUI
            : FindFirstObjectByType<ScoreUI>();

        if (scoreUI != null)
        {
            scoreUI.BindNetworkClient(this);
        }

        try
        {
            client = new UdpClient();
            serverEP = new IPEndPoint(
                IPAddress.Parse(serverIp),
                serverPort
            );
            client.Connect(serverEP);

            targetPlayer1 = PositionOf(player1);
            targetPlayer2 = PositionOf(player2);
            targetPlayer3 = PositionOf(player3);
            targetPlayer4 = PositionOf(player4);
            targetBall = PositionOf(ball);
        }
        catch (Exception exception)
        {
            Debug.LogError("Não foi possível conectar ao servidor Pong: " + exception.Message);
            enabled = false;
        }
    }

    private void Update()
    {
        if (client == null)
        {
            return;
        }

        if (requestedSlot > 0 && requestedSlot != assignedSlot)
        {
            joinTimer += Time.unscaledDeltaTime;
            if (joinTimer >= 1f)
            {
                joinTimer = 0f;
                SendJoinRequest();
            }
        }

        while (client.Available > 0)
        {
            try
            {
                IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref remote);
                string message = Encoding.UTF8.GetString(data);
                ProcessMessage(message);
            }
            catch (SocketException)
            {
                break;
            }
        }

        if (assignedSlot > 0)
        {
            inputTimer += Time.unscaledDeltaTime;
            if (inputTimer >= 0.05f)
            {
                inputTimer = 0f;
                SendNetworkMessage("INPUT:" + GetLocalInput());
            }
        }

        UpdatePosition(player1, targetPlayer1);
        UpdatePosition(player2, targetPlayer2);
        UpdatePosition(player3, targetPlayer3);
        UpdatePosition(player4, targetPlayer4);
        UpdatePosition(ball, targetBall);
    }

    public bool ConfigureServer(string ipAddress)
    {
        if (!IPAddress.TryParse(ipAddress, out IPAddress address) ||
            address.AddressFamily != AddressFamily.InterNetwork)
        {
            return false;
        }

        if (client != null && serverEP != null && serverEP.Address.Equals(address))
        {
            return true;
        }

        UdpClient newClient = null;
        try
        {
            newClient = new UdpClient();
            IPEndPoint newEndpoint = new IPEndPoint(address, serverPort);
            newClient.Connect(newEndpoint);

            UdpClient previousClient = client;
            client = newClient;
            serverEP = newEndpoint;
            serverIp = address.ToString();
            assignedSlot = -1;
            requestedSlot = 0;

            if (previousClient != null)
            {
                previousClient.Close();
            }

            return true;
        }
        catch (Exception exception)
        {
            if (newClient != null)
            {
                newClient.Close();
            }

            Debug.LogError("Não foi possível configurar o servidor Pong: " + exception.Message);
            return false;
        }
    }

    public void RequestPlayer(int slot)
    {
        if (slot < 1 || slot > 4)
        {
            return;
        }

        requestedSlot = slot;
        joinTimer = 0f;
        SendJoinRequest();
    }
    public void RequestStart()
    {
        SendNetworkMessage("START");
    }

    public void RequestReplay()
    {
        SendNetworkMessage("RESTART");
    }

    private void SendJoinRequest()
    {
        if (requestedSlot >= 1 && requestedSlot <= 4)
        {
            SendNetworkMessage("JOIN:" + requestedSlot);
        }
    }
    private void ProcessMessage(string message)
    {
        if (message.StartsWith("ASSIGN:", StringComparison.Ordinal))
        {
            if (int.TryParse(message.Substring(7), out int slot))
            {
                assignedSlot = slot;
                if (scoreUI != null)
                {
                    scoreUI.OnPlayerAssigned(slot);
                }
            }
        }
        else if (message.StartsWith("PLAYER_TAKEN:", StringComparison.Ordinal) ||
                 message.StartsWith("TEAM_FULL:", StringComparison.Ordinal))
        {
            int prefixLength = message.StartsWith("PLAYER_TAKEN:", StringComparison.Ordinal) ? 13 : 10;
            int attemptedSlot = requestedSlot;
            int.TryParse(message.Substring(prefixLength), out assignedSlot);
            requestedSlot = assignedSlot;

            if (scoreUI != null)
            {
                if (assignedSlot > 0)
                {
                    scoreUI.OnPlayerAssigned(assignedSlot);
                }

                scoreUI.OnPlayerRequestFailed(
                    "P" + attemptedSlot + " já está ocupado. Escolha outro jogador."
                );
            }
        }
        else if (message == "START" || message == "RESTART")
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.BeginNetworkGame();
            }
        }
        else if (message.StartsWith("END:", StringComparison.Ordinal))
        {
            ProcessGameOver(message);
        }
        else if (message.StartsWith("STATE:", StringComparison.Ordinal))
        {
            ProcessState(message);
        }
    }

    private void ProcessGameOver(string message)
    {
        string[] parts = message.Split(':');
        if (parts.Length != 4 ||
            !int.TryParse(parts[2], out int scoreA) ||
            !int.TryParse(parts[3], out int scoreB))
        {
            return;
        }

        string winner = parts[1] == "A" ? "TIME A" : "TIME B";
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EndGameFromNetwork(winner, scoreA, scoreB);
        }
    }

    private Vector3 PositionOf(Transform target)
    {
        return target != null ? target.position : Vector3.zero;
    }

    private string GetLocalInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return "NONE";
        }

        switch (assignedSlot)
        {
            case 1:
                return ReadVertical(keyboard.wKey, keyboard.sKey);
            case 2:
                return ReadVertical(keyboard.upArrowKey, keyboard.downArrowKey);
            case 3:
                return ReadVertical(keyboard.iKey, keyboard.kKey);
            case 4:
                return ReadVertical(keyboard.oKey, keyboard.lKey);
            default:
                return "NONE";
        }
    }

    private string ReadVertical(KeyControl upKey, KeyControl downKey)
    {
        if (upKey.isPressed)
        {
            return "UP";
        }

        if (downKey.isPressed)
        {
            return "DOWN";
        }

        return "NONE";
    }

    private void ProcessState(string message)
    {
        string[] parts = message.Substring(6).Split(';');
        if (parts.Length != 12)
        {
            return;
        }

        float[] values = new float[10];
        for (int i = 0; i < values.Length; i++)
        {
            if (!float.TryParse(
                    parts[i],
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out values[i]))
            {
                return;
            }
        }

        if (!int.TryParse(parts[10], out int scoreA) ||
            !int.TryParse(parts[11], out int scoreB))
        {
            return;
        }

        targetPlayer1 = new Vector3(values[0], values[1], 0f);
        targetPlayer2 = new Vector3(values[2], values[3], 0f);
        targetPlayer3 = new Vector3(values[4], values[5], 0f);
        targetPlayer4 = new Vector3(values[6], values[7], 0f);
        targetBall = new Vector3(values[8], values[9], 0f);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ApplyNetworkScore(scoreA, scoreB);
        }
    }

    private void UpdatePosition(Transform target, Vector3 targetPosition)
    {
        if (target == null)
        {
            return;
        }

        target.position = Vector3.Lerp(
            target.position,
            targetPosition,
            Time.unscaledDeltaTime * 15f
        );
    }

    private void SendNetworkMessage(string message)
    {
        if (client == null)
        {
            return;
        }

        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            client.Send(data, data.Length);
        }
        catch (SocketException)
        {
            // A solicitação será enviada novamente no próximo ciclo.
        }
        catch (ObjectDisposedException)
        {
            // O aplicativo está encerrando.
        }
    }

    private void OnDisable()
    {
        if (client != null)
        {
            client.Close();
            client = null;
        }

        serverEP = null;
        assignedSlot = -1;
        requestedSlot = 0;

        if (scoreUI != null)
        {
            scoreUI.BindNetworkClient(null);
        }
    }

    private void OnDestroy()
    {
        OnDisable();
    }
}


