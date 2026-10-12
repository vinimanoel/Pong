using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

public class ScoreUI : MonoBehaviour
{
    [Header("Placar")]
    [SerializeField] private TextMeshProUGUI scoreTextA;
    [SerializeField] private TextMeshProUGUI scoreTextB;

    private Canvas canvas;
    private GameObject overlay;
    private RectTransform card;
    private TextMeshProUGUI overlayTitle;
    private TextMeshProUGUI overlayMessage;
    private TextMeshProUGUI selectionStatus;
    private Button startButton;
    private Button replayButton;
    private TextMeshProUGUI localControlsText;
    private TMP_InputField serverIpInput;
    private PongUdpClient networkClient;

    private bool selectingTeam;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }

        if (canvas == null)
        {
            canvas = FindFirstObjectByType<Canvas>();
        }

        if (canvas == null)
        {
            GameObject canvasObject = new GameObject(
                "Pong Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster)
            );
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        BuildScoreHud();
        BuildOverlay();
    }

    private void Start()
    {
        Time.timeScale = 0f;
        ShowMainMenu();
    }

    public void BindNetworkClient(PongUdpClient client)
    {
        networkClient = client;
        if (networkClient != null && networkClient.AssignedSlot > 0)
        {
            OnPlayerAssigned(networkClient.AssignedSlot);
        }
    }

    public void OnPlayerAssigned(int slot)
    {

        if (localControlsText != null)
        {
            localControlsText.text = "Sua posição: " + (slot <= 2 ? "Time A" : "Time B") +
                "  •  Paddle " + slot + "  •  Teclas: " + ControlsForSlot(slot);
        }
        UpdateSelectionStatus();
    }

    public void OnPlayerRequestFailed(string message)
    {
        if (selectionStatus != null)
        {
            selectionStatus.text = message;
        }

        if (startButton != null)
        {
            startButton.interactable =
                networkClient != null && networkClient.AssignedSlot > 0;
        }
    }

    public void UpdateScore(int scoreA, int scoreB)
    {
        if (scoreTextA != null)
        {
            scoreTextA.text = scoreA.ToString();
        }

        if (scoreTextB != null)
        {
            scoreTextB.text = scoreB.ToString();
        }
    }

    public void ShowMainMenu()
    {
        Time.timeScale = 0f;
        selectingTeam = false;
        SetOverlayVisible(true);
        ClearCard();
        SetCardSize(560f, 430f);

        overlayTitle.text = "PONG 2D";
        SetRect(overlayTitle.rectTransform, 0f, 75f, 520f, 60f);
        overlayMessage.fontSize = 20f;
        SetRect(overlayMessage.rectTransform, 0f, 10f, 520f, 82f);
        overlayMessage.text =
            "TIME A fica à esquerda  •  TIME B fica à direita\n" +
            "A primeira equipe a fazer 15 gols vence.";

        AddButton("INICIAR JOGO", -55f, ShowTeamSelection);
    }

    public void ShowTeamSelection()
    {
        selectingTeam = true;
        SetOverlayVisible(true);
        ClearCard();
        SetCardSize(660f, 700f);

        overlayTitle.text = "ESCOLHA SEU JOGADOR";
        SetRect(overlayTitle.rectTransform, 0f, 310f, 520f, 60f);
        overlayMessage.fontSize = 16f;
        SetRect(overlayMessage.rectTransform, 0f, 230f, 600f, 80f);
        overlayMessage.text =
            "IP desta máquina: " + GetLocalIPv4Address() + "\n" +
            "Host mantém 127.0.0.1; clientes digitam o IP do host abaixo.";

        CreateServerIpInput();

        TextMeshProUGUI teamALabel = CreateText(
            "Team A Player Choices", card, 16f, FontStyles.Bold, TextAlignmentOptions.Center
        );
        teamALabel.text = "TIME A • ESQUERDA";
        teamALabel.color = new Color(0.45f, 0.8f, 1f);
        SetRect(teamALabel.rectTransform, -150f, 60f, 270f, 30f);

        TextMeshProUGUI teamBLabel = CreateText(
            "Team B Player Choices", card, 16f, FontStyles.Bold, TextAlignmentOptions.Center
        );
        teamBLabel.text = "TIME B • DIREITA";
        teamBLabel.color = new Color(1f, 0.62f, 0.42f);
        SetRect(teamBLabel.rectTransform, 150f, 60f, 270f, 30f);

        AddPlayerButton("P1 • VERMELHO", -150f, 0f, 1, new Color(1f, 0.2f, 0.2f));
        AddPlayerButton("P2 • AZUL", -150f, -65f, 2, new Color(0.1f, 0.35f, 1f));
        AddPlayerButton("P3 • VERDE", 150f, 0f, 3, new Color(0.1f, 0.8f, 0.2f));
        AddPlayerButton("P4 • TURQUESA", 150f, -65f, 4, new Color(0f, 0.85f, 0.72f));

        selectionStatus = CreateText(
            "SelectionStatus", card, 17f, FontStyles.Normal, TextAlignmentOptions.Center
        );
        SetRect(selectionStatus.rectTransform, 0f, -135f, 560f, 48f);
        selectionStatus.text = GetCurrentSelectionMessage();

        startButton = AddButton("INICIAR PARTIDA", -215f, RequestStart);
        startButton.interactable =
            networkClient != null && networkClient.AssignedSlot > 0;

        UpdateSelectionStatus();
        AddButton("VOLTAR AO MENU", -285f, ShowMainMenu);
    }
    public void ShowGame()
    {
        selectingTeam = false;
        SetOverlayVisible(false);
    }

    public void ShowWinner(string winner, int scoreA, int scoreB)
    {
        Time.timeScale = 0f;
        selectingTeam = false;
        SetOverlayVisible(true);
        ClearCard();
        SetCardSize(600f, 500f);

        overlayTitle.text = winner.ToUpperInvariant() + " VENCEU!";
        SetRect(overlayTitle.rectTransform, 0f, 75f, 520f, 60f);
        overlayMessage.fontSize = 20f;
        SetRect(overlayMessage.rectTransform, 0f, 10f, 520f, 82f);
        overlayMessage.text =
            "Placar final\nTIME A  " + scoreA + "  ×  " + scoreB + "  TIME B";

        replayButton = AddButton(
            "JOGAR NOVAMENTE",
            -95f,
            RequestReplay
        );
        replayButton.interactable =
            networkClient != null && networkClient.AssignedSlot > 0;

        AddButton("VOLTAR AO MENU INICIAL", -175f, ShowMainMenu);
    }

    public void ShowWinner(string message)
    {
        string winner = message.Contains("A") ? "TIME A" : "TIME B";
        int scoreA = GameManager.Instance != null
            ? GameManager.Instance.teamAScore
            : 0;
        int scoreB = GameManager.Instance != null
            ? GameManager.Instance.teamBScore
            : 0;

        ShowWinner(winner, scoreA, scoreB);
    }

    private void ChoosePlayer(int slot)
    {
        if (networkClient == null)
        {
            OnPlayerRequestFailed("A conexão ainda está iniciando. Tente novamente.");
            return;
        }

        string serverAddress = serverIpInput != null
            ? serverIpInput.text.Trim()
            : "127.0.0.1";
        if (!networkClient.ConfigureServer(serverAddress))
        {
            OnPlayerRequestFailed("Digite um IPv4 válido para o host (ex.: 192.168.0.10).");
            return;
        }

        PlayerPrefs.SetString("PongServerIp", serverAddress);
        PlayerPrefs.Save();

        if (selectionStatus != null)
        {
            selectionStatus.text = "Solicitando o P" + slot + " ao servidor...";
        }
        startButton.interactable = false;
        networkClient.RequestPlayer(slot);
    }
    private void RequestStart()
    {
        if (networkClient == null || networkClient.AssignedSlot <= 0)
        {
            if (selectionStatus != null)
            {
                selectionStatus.text = "Escolha um jogador e aguarde a confirmação do servidor.";
            }

            return;
        }

        networkClient.RequestStart();
    }

    private void RequestReplay()
    {
        if (networkClient == null || networkClient.AssignedSlot <= 0)
        {
            return;
        }

        networkClient.RequestReplay();
    }

    private void UpdateSelectionStatus()
    {
        if (!selectingTeam || selectionStatus == null)
        {
            return;
        }

        selectionStatus.text = GetCurrentSelectionMessage();
        startButton.interactable =
            networkClient != null && networkClient.AssignedSlot > 0;
    }

    private string GetCurrentSelectionMessage()
    {
        if (networkClient == null || networkClient.AssignedSlot <= 0)
        {
            return "Escolha P1, P2, P3 ou P4 para entrar na partida.";
        }

        int slot = networkClient.AssignedSlot;
        string team = slot <= 2 ? "TIME A — ESQUERDA" : "TIME B — DIREITA";
        string controls = ControlsForSlot(slot);
        string color = slot == 1 ? "Vermelho" :
            slot == 2 ? "Azul" :
            slot == 3 ? "Verde" : "Turquesa";

        return "Você está no " + team +
               "\nP" + slot + " • " + color + "  •  Teclas: " + controls;
    }
    private string ControlsForSlot(int slot)
    {
        switch (slot)
        {
            case 1: return "W / S";
            case 2: return "↑ / ↓";
            case 3: return "I / K";
            case 4: return "O / L";
            default: return "aguardando";
        }
    }

    private void BuildScoreHud()
    {
        GameObject panelObject = new GameObject(
            "Score HUD",
            typeof(RectTransform),
            typeof(Image)
        );
        panelObject.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 1f);
        panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.anchoredPosition = new Vector2(0f, -12f);
        panelRect.sizeDelta = new Vector2(700f, 92f);

        Image background = panelObject.GetComponent<Image>();
        background.color = new Color(0.02f, 0.04f, 0.08f, 0.7f);
        background.raycastTarget = false;

        TextMeshProUGUI teamA = CreateText(
            "Team A Label",
            panelRect,
            20f,
            FontStyles.Bold,
            TextAlignmentOptions.MidlineRight
        );
        teamA.text = "TIME A\nESQUERDA";
        SetRect(teamA.rectTransform, -220f, -44f, 150f, 65f);
        teamA.color = new Color(0.45f, 0.8f, 1f);

        TextMeshProUGUI teamB = CreateText(
            "Team B Label",
            panelRect,
            20f,
            FontStyles.Bold,
            TextAlignmentOptions.MidlineLeft
        );
        teamB.text = "TIME B\nDIREITA";
        SetRect(teamB.rectTransform, 220f, -44f, 150f, 65f);
        teamB.color = new Color(1f, 0.62f, 0.42f);

        if (scoreTextA == null || scoreTextB == null)
        {
            TextMeshProUGUI scoreA = CreateText(
                "Team A Score",
                panelRect,
                36f,
                FontStyles.Bold,
                TextAlignmentOptions.Center
            );
            TextMeshProUGUI scoreB = CreateText(
                "Team B Score",
                panelRect,
                36f,
                FontStyles.Bold,
                TextAlignmentOptions.Center
            );
            scoreTextA = scoreA;
            scoreTextB = scoreB;
        }
        else
        {
            scoreTextA.transform.SetParent(panelRect, false);
            scoreTextB.transform.SetParent(panelRect, false);
        }

        ConfigureScoreText(scoreTextA, -75f);
        ConfigureScoreText(scoreTextB, 75f);

        localControlsText = CreateText(
            "Local Player Controls",
            canvas.transform,
            18f,
            FontStyles.Normal,
            TextAlignmentOptions.BottomLeft
        );
        localControlsText.text = "";
        localControlsText.rectTransform.anchorMin = Vector2.zero;
        localControlsText.rectTransform.anchorMax = Vector2.zero;
        localControlsText.rectTransform.pivot = Vector2.zero;
        localControlsText.rectTransform.anchoredPosition = new Vector2(20f, 18f);
        localControlsText.rectTransform.sizeDelta = new Vector2(480f, 40f);
        localControlsText.color = new Color(0.9f, 0.95f, 1f);
    }

    private void ConfigureScoreText(TextMeshProUGUI score, float x)
    {
        score.text = "0";
        score.fontSize = 36f;
        score.alignment = TextAlignmentOptions.Center;
        score.raycastTarget = false;
        SetRect(score.rectTransform, x, -43f, 82f, 68f);
    }

    private void BuildOverlay()
    {
        GameObject overlayObject = new GameObject(
            "Pong Menu Overlay",
            typeof(RectTransform),
            typeof(Image)
        );
        overlayObject.transform.SetParent(canvas.transform, false);
        overlay = overlayObject;

        RectTransform overlayRect = overlayObject.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        Image overlayImage = overlayObject.GetComponent<Image>();
        overlayImage.color = new Color(0.015f, 0.025f, 0.05f, 0.86f);
        overlayImage.raycastTarget = true;

        GameObject cardObject = new GameObject(
            "Menu Card",
            typeof(RectTransform),
            typeof(Image)
        );
        cardObject.transform.SetParent(overlay.transform, false);
        card = cardObject.GetComponent<RectTransform>();
        card.anchorMin = new Vector2(0.5f, 0.5f);
        card.anchorMax = new Vector2(0.5f, 0.5f);
        card.pivot = new Vector2(0.5f, 0.5f);

        Image cardImage = cardObject.GetComponent<Image>();
        cardImage.color = new Color(0.055f, 0.09f, 0.15f, 0.98f);
        cardImage.raycastTarget = false;

        overlayTitle = CreateText(
            "Overlay Title",
            card,
            38f,
            FontStyles.Bold,
            TextAlignmentOptions.Center
        );
        overlayTitle.color = Color.white;
        SetRect(overlayTitle.rectTransform, 0f, 75f, 520f, 60f);

        overlayMessage = CreateText(
            "Overlay Message",
            card,
            20f,
            FontStyles.Normal,
            TextAlignmentOptions.Center
        );
        overlayMessage.color = new Color(0.82f, 0.88f, 0.95f);
        SetRect(overlayMessage.rectTransform, 0f, 10f, 520f, 82f);
    }

    private void ClearCard()
    {
        for (int i = card.childCount - 1; i >= 0; i--)
        {
            Transform child = card.GetChild(i);
            if (child != overlayTitle.transform &&
                child != overlayMessage.transform)
            {
                Destroy(child.gameObject);
            }
        }

        selectionStatus = null;
        startButton = null;
        replayButton = null;
    }

    private void SetCardSize(float width, float height)
    {
        card.sizeDelta = new Vector2(width, height);
    }

    private void CreateServerIpInput()
    {
        GameObject inputObject = new GameObject(
            "Host IP Input", typeof(RectTransform), typeof(Image), typeof(TMP_InputField)
        );
        inputObject.transform.SetParent(card, false);
        SetRect(inputObject.GetComponent<RectTransform>(), 0f, 115f, 520f, 48f);

        Image background = inputObject.GetComponent<Image>();
        background.color = new Color(0.035f, 0.06f, 0.1f, 1f);

        TMP_InputField input = inputObject.GetComponent<TMP_InputField>();
        TextMeshProUGUI inputText = CreateText(
            "IP Text", inputObject.transform, 18f, FontStyles.Normal, TextAlignmentOptions.MidlineLeft
        );
        SetRect(inputText.rectTransform, 0f, 0f, 490f, 42f);
        inputText.color = Color.white;

        TextMeshProUGUI placeholder = CreateText(
            "IP Placeholder", inputObject.transform, 16f, FontStyles.Italic, TextAlignmentOptions.MidlineLeft
        );
        placeholder.text = "IP do host (ex.: 192.168.0.10)";
        placeholder.color = new Color(0.65f, 0.7f, 0.76f);
        SetRect(placeholder.rectTransform, 0f, 0f, 490f, 42f);

        input.textComponent = inputText;
        input.placeholder = placeholder;
        input.targetGraphic = background;
        input.lineType = TMP_InputField.LineType.SingleLine;
        input.characterLimit = 15;
        input.contentType = TMP_InputField.ContentType.Standard;
        input.text = PlayerPrefs.GetString("PongServerIp", "127.0.0.1");
        serverIpInput = input;
    }

    private string GetLocalIPv4Address()
    {
        try
        {
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in adapters)
            {
                if (adapter.OperationalStatus != OperationalStatus.Up ||
                    (adapter.NetworkInterfaceType != NetworkInterfaceType.Wireless80211 &&
                     adapter.NetworkInterfaceType != NetworkInterfaceType.Ethernet))
                {
                    continue;
                }

                foreach (UnicastIPAddressInformation address in adapter.GetIPProperties().UnicastAddresses)
                {
                    if (address.Address.AddressFamily == AddressFamily.InterNetwork &&
                        !IPAddress.IsLoopback(address.Address) &&
                        !address.Address.ToString().StartsWith("169.254.", System.StringComparison.Ordinal))
                    {
                        return address.Address.ToString();
                    }
                }
            }
        }
        catch (System.Exception)
        {
            // The IP can still be entered manually if Windows does not expose it.
        }

        return "não encontrado";
    }
    private Button AddPlayerButton(string label, float x, float y, int slot, Color playerColor)
    {
        Button button = AddButton(label, y, () => ChoosePlayer(slot));
        RectTransform buttonRect = button.GetComponent<RectTransform>();
        buttonRect.anchoredPosition = new Vector2(x, y);
        buttonRect.sizeDelta = new Vector2(270f, 58f);

        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.fontSize = 17f;
            buttonText.color = playerColor;
        }

        return button;
    }
    private Button AddButton(string label, float y, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = new GameObject(
            label + " Button",
            typeof(RectTransform),
            typeof(Image),
            typeof(Button)
        );
        buttonObject.transform.SetParent(card, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        SetRect(rect, 0f, y, 390f, 54f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.12f, 0.52f, 0.72f, 1f);

        Button button = buttonObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.12f, 0.52f, 0.72f, 1f);
        colors.highlightedColor = new Color(0.18f, 0.66f, 0.86f, 1f);
        colors.pressedColor = new Color(0.08f, 0.38f, 0.56f, 1f);
        colors.disabledColor = new Color(0.25f, 0.3f, 0.36f, 0.8f);
        button.colors = colors;
        button.onClick.AddListener(action);

        TextMeshProUGUI buttonText = CreateText(
            "Label",
            buttonObject.transform,
            20f,
            FontStyles.Bold,
            TextAlignmentOptions.Center
        );
        buttonText.text = label;
        buttonText.color = Color.white;
        buttonText.raycastTarget = false;
        RectTransform textRect = buttonText.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return button;
    }

    private TextMeshProUGUI CreateText(
        string objectName,
        Transform parent,
        float size,
        FontStyles style,
        TextAlignmentOptions alignment
    )
    {
        GameObject textObject = new GameObject(
            objectName,
            typeof(RectTransform)
        );
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        if (scoreTextA != null && scoreTextA.font != null)
        {
            text.font = scoreTextA.font;
        }
        else if (scoreTextB != null && scoreTextB.font != null)
        {
            text.font = scoreTextB.font;
        }

        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = Color.white;
        text.raycastTarget = false;
        text.enableWordWrapping = true;
        return text;
    }

    private void SetRect(
        RectTransform rect,
        float x,
        float y,
        float width,
        float height
    )
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(width, height);
    }

    private void SetOverlayVisible(bool visible)
    {
        if (overlay != null)
        {
            overlay.SetActive(visible);
        }

        if (localControlsText != null)
        {
            localControlsText.gameObject.SetActive(!visible);
        }
    }
}




