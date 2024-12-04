using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.SearchService;
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

    private NetDataWriter writer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
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
    public void Start()
    {
        writer = new NetDataWriter();
    }
    public void Update()
    {
        OnGameConnection();
    }
    public void OnLoginBtnClicked()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;
        SendRequest("OnLoginPacket", username, password, "");
    }
    public void OnRegBtnClicked()
    {
        string username = usernameInputReg.text;
        string password = passwordInputReg.text;
        string email = emailInput.text;
        SendRequest("OnRegPacket", username, password, email);
    }
    private void SendRequest(string action, string username, string password, string email)
    {
        if (Connection.Instance != null && Connection.Instance.IsConnected)
        {
            writer.Reset();
            writer.Put(action);
            writer.Put(username);
            writer.Put(password);
            writer.Put(email);

            Connection.Instance.Server.Send(writer, DeliveryMethod.ReliableOrdered);
        }
        else
        {
            Debug.LogError("Not connected to the server");
        }
    }
    public void OnGameConnection()
    {
        if (Connection.Instance.IsConnected)
        {
            ConnectionText.text = "Connecting...";
            StartCoroutine(FillConnectionBar());
            UI.SetActive(true);
            ConnectionUI.SetActive(false);
        }
        else
        {
            ConnectionText.text = "No Internet Connection.";
            UI.SetActive(false);
            ConnectionUI.SetActive(true);
        }
    }
    private IEnumerator FillConnectionBar()
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
        Debug.Log("SendingScene Transition");
        StartCoroutine(WaitAndLoadScene());
    }
    public IEnumerator WaitAndLoadScene()
    {
        SceneTrans.SetActive(true);
        yield return new WaitForSeconds(0.7f);
        SceneManager.LoadScene("MainMenu");
    }
}
