using ENet;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginSceneHandle : MonoBehaviour
{
    public static LoginSceneHandle Instance;

    public GameObject SceneTrans;

    public Button LoginTabBTN;
    public Button RegisterTabBTN;

    public GameObject LoginPanel;
    public GameObject RegisterPanel;

    public Sprite TabBTNClicked;
    public Sprite TabBTNUnClicked;

    public GameObject UI;
    public GameObject ConnectionUI;

    public Slider ConnectionBar;

    public TMP_Text ConnectionText;

    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;

    public TMP_InputField usernameInputReg;
    public TMP_InputField passwordInputReg;
    public TMP_InputField emailInput;

    public Button loginButton;
    public Button registerButton;

    public GameObject ErrorLoginTxt;
    public GameObject ErrorRegTXT;

    private Connection connection;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "LoginScene")
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (connection.isConnected)
        {
            ConnectionUI.SetActive(false);
        }
        else
        {
            ConnectionUI.SetActive(true);
            ConnectionText.text = "No Internet Connection.";
            UI.SetActive(false);
            ConnectionUI.SetActive(true);
        }
    }

    private void Start()
    {
        connection = Connection.instance;

        if (connection == null)
        {
            Debug.LogError("ENetConnection instance not found!");
            return;
        }

        StartCoroutine(HandleConnection());
    }

    private IEnumerator HandleConnection()
    {
        while (true)
        {
            if (Connection.instance.isConnected)
            {
                ConnectionText.text = "Connected!";
                UI.SetActive(true);
                ConnectionUI.SetActive(false);
            }
            else
            {
                ConnectionText.text = "No Internet Connection.";
                UI.SetActive(false);
                ConnectionUI.SetActive(true);
            }

            yield return new WaitForSeconds(1f);
        }
    }

    public void OnLoginBtnClicked()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;
        SendRequest("LoginRequest", username, password, "");
    }

    public void OnRegBtnClicked()
    {
        string username = usernameInputReg.text;
        string password = passwordInputReg.text;
        string email = emailInput.text;
        SendRequest("RegisterRequest", username, password, email);
    }

    private void SendRequest(string action, string username, string password, string email)
    {
        string message = $"{action}|{username}|{password}|{email}";
        byte[] messageData = System.Text.Encoding.UTF8.GetBytes(message);

        Packet packet = default;
        packet.Create(messageData, PacketFlags.Reliable);
        if(action == "LoginRequest")
        {
            connection.SendLoginPacket(packet);
        }
        else
        {
            connection.SendRegisterPacket(packet);
        }
        Debug.Log($"Sent request: {message}");
    }

    public void FailedLogin()
    {
        ErrorLoginTxt.SetActive(true);
    }

    public void FailedRegister()
    {
        ErrorRegTXT.SetActive(true);
    }

    public void OnTabLoginBTNClicked()
    {
        RegisterTabBTN.image.sprite = TabBTNUnClicked;
        LoginTabBTN.image.sprite = TabBTNClicked;

        LoginPanel.SetActive(true);
        RegisterPanel.SetActive(false);
    }

    public void OnTabRegisterBTNClicked()
    {
        LoginTabBTN.image.sprite = TabBTNUnClicked;
        RegisterTabBTN.image.sprite = TabBTNClicked;

        LoginPanel.SetActive(false);
        RegisterPanel.SetActive(true);
    }

    public IEnumerator FillConnectionBar()
    {
        float elapsedTime = 0f;
        float duration = 10f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            ConnectionBar.value = Mathf.Lerp(0, 1, elapsedTime / duration);
            yield return null;
        }

        ConnectionBar.value = 1;
    }

    public void LoadNextScene()
    {
        Debug.Log("Sending Scene Transition");
        StartCoroutine(WaitAndLoadScene());
    }

    public IEnumerator WaitAndLoadScene()
    {
        SceneTrans.SetActive(true);
        yield return new WaitForSeconds(0.7f);
        SceneManager.LoadScene("MainMenu");
    }
}
