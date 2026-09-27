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

    [SerializeField] private NetworkMode mode =
        NetworkMode.LocalTest;

    [SerializeField] private PongUdpServer server;
    [SerializeField] private PongUdpClient client;

    private void Awake()
    {
        ConfigurePaddles();
        ConfigureBallPhysics();
        ConfigureGoals();

        switch (mode)
        {
            case NetworkMode.Host:
                server.enabled = true;
                client.enabled = true;
                break;

            case NetworkMode.Server:
                server.enabled = true;
                client.enabled = false;
                break;

            case NetworkMode.Client:
                server.enabled = false;
                client.enabled = true;
                break;

            case NetworkMode.LocalTest:
                server.enabled = true;
                client.enabled = true;
                break;
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

        if (mode == NetworkMode.Client)
        {
            foreach (GoalController goal in goals)
            {
                goal.enabled = false;
            }
        }
    }
}