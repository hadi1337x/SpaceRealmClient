using ENet;
using System;
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

    public TMP_InputField worldName;

    private void Start()
    {
        
    }
    private void FixedUpdate()
    {
        if (Connection.instance.isConnected)
        {
            disconnectionDialog.SetActive(false);
        }
        else
        {
            disconnectionDialog.SetActive(true);
        }
    }
    public void OnEnterWorld()
    {
        if (worldName.text != string.Empty)
        {
            string message = $"{"RetrieveWorld"}|{Connection.instance.myInfo.playerId}|{worldName.text}";
            byte[] messageData = System.Text.Encoding.UTF8.GetBytes(message);
            Debug.Log(message);
            Packet packet = default;
            packet.Create(messageData, PacketFlags.Reliable);
            Connection.instance.SendWorldPacket(packet);
        }
        else
        {
            string message = $"{"RetrieveWorld"}|{Connection.instance.myInfo.playerId}|{"WELCOME"}";
            byte[] messageData = System.Text.Encoding.UTF8.GetBytes(message);

            Packet packet = default;
            packet.Create(messageData, PacketFlags.Reliable);
            Connection.instance.SendWorldPacket(packet);
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
