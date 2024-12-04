using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WorldManager : MonoBehaviour
{
    public GameObject disconnectionDialog;
    
    public GameObject RealmTab;
    public GameObject SettingTab;
    public GameObject StoreTab;

    public Button RealmBTN;
    public Button SettingBTN;
    public Button StoreBTN;

    public Sprite TabBTNClicked;
    public Sprite TabBTNUnClicked;

    private NetDataWriter writer;
    public TMP_InputField worldName;

    private void Start()
    {
        writer = new NetDataWriter();
    }
    public void OnEnterWorld()
    {
        if (worldName.text != string.Empty)
        {
            writer.Put("RetrieveWorld");
            writer.Put(worldName.text);
            Connection.Instance.Server.Send(writer, DeliveryMethod.ReliableOrdered);
        }
        else
        {
            writer.Put("RetrieveWorld");
            writer.Put("TUTORIAL");
            Connection.Instance.Server.Send(writer, DeliveryMethod.ReliableOrdered);
        }
    }
    public void RealmClicked()
    {
        RealmBTN.image.sprite = TabBTNClicked;
        SettingBTN.image.sprite = TabBTNUnClicked;
        StoreBTN.image.sprite = TabBTNUnClicked;

        RealmTab.SetActive(true);
        SettingTab.SetActive(false);
        StoreTab.SetActive(false);
    }
    public void SettingClicked()
    {
        RealmBTN.image.sprite = TabBTNUnClicked;
        SettingBTN.image.sprite = TabBTNClicked;
        StoreBTN.image.sprite = TabBTNUnClicked;

        RealmTab.SetActive(false);
        SettingTab.SetActive(true);
        StoreTab.SetActive(false);
    }
    public void StoreClicked()
    {
        RealmBTN.image.sprite = TabBTNUnClicked;
        SettingBTN.image.sprite = TabBTNUnClicked;
        StoreBTN.image.sprite = TabBTNClicked;

        RealmTab.SetActive(false);
        SettingTab.SetActive(false);
        StoreTab.SetActive(true);
    }
    void Update()
    {
        
    }
    public void DisconnectionBTN()
    {
        SceneManager.LoadScene("LoginScene");
    }
}
