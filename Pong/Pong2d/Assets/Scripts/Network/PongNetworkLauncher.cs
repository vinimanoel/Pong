using UnityEngine;

public class PongNetworkLauncher : MonoBehaviour
{
    private enum NetworkMode
    {
        Host,
        Server,
        Client,
        LocalTest
    }

    private NetworkMode mode = NetworkMode.Host;

    [SerializeField] private PongUdpServer server;
    [SerializeField] private PongUdpClient client;

    private bool roleSelected;

    public bool IsClientMode => roleSelected && mode == NetworkMode.Client;

    private void Awake()
    {
        StopNetworking();
    }

    public void StartAsHost()
    {
        StartNetworkMode(NetworkMode.Host);
    }

    public void StartAsClient()
    {
        StartNetworkMode(NetworkMode.Client);
    }

    public void StopNetworking()
    {
        roleSelected = false;

        if (client != null)
        {
            client.enabled = false;
        }

        if (server != null)
        {
            server.enabled = false;
        }
    }

    private void StartNetworkMode(NetworkMode selectedMode)
    {
        StopNetworking();
        mode = selectedMode;
        roleSelected = true;

        ConfigurePaddles();
        ConfigureBallPhysics();
        ConfigureGoals();

        if (server != null)
        {
            server.enabled = mode == NetworkMode.Host;
        }

        if (client != null)
        {
            client.enabled = true;
        }
    }

    private void ConfigurePaddles()
    {
        PaddleController[] paddles =
            FindObjectsByType<PaddleController>(
                FindObjectsSortMode.None
            );

        foreach (PaddleController paddle in paddles)
        {
            paddle.enabled = false;
        }
    }

    private void ConfigureBallPhysics()
    {
        BallController ballController =
            FindFirstObjectByType<BallController>();

        if (ballController == null)
        {
            return;
        }

        Rigidbody2D ballRb =
            ballController.GetComponent<Rigidbody2D>();

        if (ballRb == null)
        {
            return;
        }

        if (mode == NetworkMode.Client)
        {
            ballController.enabled = false;
            ballRb.simulated = false;
        }
        else
        {
            ballController.enabled = true;
            ballRb.simulated = true;
        }
    }

    private void ConfigureGoals()
    {
        GoalController[] goals =
            FindObjectsByType<GoalController>(
                FindObjectsSortMode.None
            );

        foreach (GoalController goal in goals)
        {
            goal.enabled = mode != NetworkMode.Client;
        }
    }
}
