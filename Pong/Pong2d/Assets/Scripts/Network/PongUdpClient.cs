using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Globalization;

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

    private int myId = -1;

    private float helloTimer = 0f;
    private float inputTimer = 0f;

    private Vector3 targetPlayer1;
    private Vector3 targetPlayer2;
    private Vector3 targetPlayer3;
    private Vector3 targetPlayer4;
    private Vector3 targetBall;

    private void Start()
    {
        client = new UdpClient();

        serverEP = new IPEndPoint(
            IPAddress.Parse(serverIp),
            serverPort
        );

        client.Connect(serverEP);

        if (player1 != null)
        {
            targetPlayer1 = player1.position;
        }

        if (player2 != null)
        {
            targetPlayer2 = player2.position;
        }

        if (player3 != null)
        {
            targetPlayer3 = player3.position;
        }

        if (player4 != null)
        {
            targetPlayer4 = player4.position;
        }

        if (ball != null)
        {
            targetBall = ball.position;
        }

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

        // Recebe mensagens do servidor.
        while (client.Available > 0)
        {
            try
            {
                IPEndPoint remoteEP = new IPEndPoint(
                    IPAddress.Any,
                    0
                );

                byte[] data = client.Receive(
                    ref remoteEP
                );

                string msg =
                    Encoding.UTF8.GetString(data);

                if (msg.StartsWith("ASSIGN:"))
                {
                    myId = int.Parse(
                        msg.Substring(7)
                    );

                    Debug.Log(
                        "Recebi meu ID: " +
                        myId
                    );
                }
                else if (msg.StartsWith("STATE:"))
                {
                    ProcessState(msg);
                }
            }
            catch (SocketException)
            {
                break;
            }
        }

        // Só envia comandos depois de receber o ID.
        if (myId != -1)
        {
            inputTimer += Time.deltaTime;

            if (inputTimer >= 0.1f)
            {
                inputTimer = 0f;

                SendInputs();
            }
        }

        // Atualiza visualmente os paddles.
        UpdatePaddlePosition(
            player1,
            targetPlayer1
        );

        UpdatePaddlePosition(
            player2,
            targetPlayer2
        );

        UpdatePaddlePosition(
            player3,
            targetPlayer3
        );

        UpdatePaddlePosition(
            player4,
            targetPlayer4
        );

        // Atualiza visualmente a bola.
        UpdatePaddlePosition(
            ball,
            targetBall
        );
    }

    private void SendHello()
    {
        string msg = "HELLO PONG";

        byte[] data =
            Encoding.UTF8.GetBytes(msg);

        client.Send(
            data,
            data.Length
        );

        Debug.Log(
            "HELLO enviado ao servidor"
        );
    }

    private void SendInputs()
    {
        string player1Input;
        string player2Input;

        // Cliente 1 controla P1 e P2.
        if (myId == 1)
        {
            player1Input =
                GetVerticalInput(
                    Keyboard.current.wKey,
                    Keyboard.current.sKey
                );

            player2Input =
                GetVerticalInput(
                    Keyboard.current.eKey,
                    Keyboard.current.dKey
                );

            string msg =
                "INPUT:P1;" +
                player1Input +
                ";P2;" +
                player2Input;

            SendNetworkMessage(msg);
        }

        // Cliente 2 controla P3 e P4.
        else if (myId == 2)
        {
            player1Input =
                GetVerticalInput(
                    Keyboard.current.iKey,
                    Keyboard.current.kKey
                );

            player2Input =
                GetVerticalInput(
                    Keyboard.current.oKey,
                    Keyboard.current.lKey
                );

            string msg =
                "INPUT:P3;" +
                player1Input +
                ";P4;" +
                player2Input;

            SendNetworkMessage(msg);
        }
    }

    private string GetVerticalInput(
        KeyControl upKey,
        KeyControl downKey
    )
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

    private void ProcessState(string msg)
    {
        string data =
            msg.Substring(6);

        string[] parts =
            data.Split(';');

        // 8 posições dos paddles
        // + 2 posições da bola
        // + 2 pontos
        if (parts.Length != 12)
        {
            return;
        }

        float p1X = float.Parse(
            parts[0],
            CultureInfo.InvariantCulture
        );

        float p1Y = float.Parse(
            parts[1],
            CultureInfo.InvariantCulture
        );

        float p2X = float.Parse(
            parts[2],
            CultureInfo.InvariantCulture
        );

        float p2Y = float.Parse(
            parts[3],
            CultureInfo.InvariantCulture
        );

        float p3X = float.Parse(
            parts[4],
            CultureInfo.InvariantCulture
        );

        float p3Y = float.Parse(
            parts[5],
            CultureInfo.InvariantCulture
        );

        float p4X = float.Parse(
            parts[6],
            CultureInfo.InvariantCulture
        );

        float p4Y = float.Parse(
            parts[7],
            CultureInfo.InvariantCulture
        );

        float ballX = float.Parse(
            parts[8],
            CultureInfo.InvariantCulture
        );

        float ballY = float.Parse(
            parts[9],
            CultureInfo.InvariantCulture
        );

        int scoreA = int.Parse(parts[10]);
        int scoreB = int.Parse(parts[11]);

        targetPlayer1 =
            new Vector3(
                p1X,
                p1Y,
                0f
            );

        targetPlayer2 =
            new Vector3(
                p2X,
                p2Y,
                0f
            );

        targetPlayer3 =
            new Vector3(
                p3X,
                p3Y,
                0f
            );

        targetPlayer4 =
            new Vector3(
                p4X,
                p4Y,
                0f
            );

        targetBall =
            new Vector3(
                ballX,
                ballY,
                0f
            );

        if (scoreUI != null)
        {
            scoreUI.UpdateScore(
                scoreA,
                scoreB
            );
        }
    }

    private void UpdatePaddlePosition(
        Transform target,
        Vector3 targetPosition
    )
    {
        if (target == null)
        {
            return;
        }

        target.position =
            Vector3.Lerp(
                target.position,
                targetPosition,
                Time.deltaTime * 15f
            );
    }

    private void SendNetworkMessage(
        string msg
    )
    {
        byte[] data =
            Encoding.UTF8.GetBytes(msg);

        client.Send(
            data,
            data.Length
        );
    }

    private void OnApplicationQuit()
    {
        if (client != null)
        {
            client.Close();
        }
    }
}