using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ENet;
using Server;
using Random = UnityEngine.Random;
using System.Text;
using NUnit.Framework.Interfaces;
using Unity.VisualScripting;
using static UnityEditor.Progress;
using static UnityEditor.U2D.ScriptablePacker;
using UnityEditor.SearchService;
using UnityEngine.SceneManagement;

public class Connection : MonoBehaviour
{
    public static Connection instance;

    public bool isConnected;
    private uint _myPlayerId;

    private Host _client;
    private Peer _peer;
    private int _skipFrame = 0;

    const int channelID = 0;

    private readonly object _enetLock = new object();

    public Player myInfo;

    public GameObject myPlayerFactory;
    public GameObject otherPlayerFactory;

    private GameObject _myPlayer;

    void Start()
    {
        Application.runInBackground = true;
        InitENet();
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject); // Prevent duplicate instances
        }
    }
    void FixedUpdate()
    {
        UpdateENet();

        if (++_skipFrame < 3)
            return;
        _skipFrame = 0;
    }

    void OnDestroy()
    {
        _client.Dispose();
        ENet.Library.Deinitialize();
    }

    private void InitENet()
    {
        const string ip = "127.0.0.1";
        const ushort port = 6005;
        ENet.Library.Initialize();
        _client = new Host();
        Address address = new Address();

        address.SetHost(ip);
        address.Port = port;
        _client.Create();
        Debug.Log("Connecting");
        _peer = _client.Connect(address);
        isConnected = true;
    }

    private void UpdateENet()
    {
        lock (_enetLock)
        {
            ENet.Event netEvent;

            if (_client.CheckEvents(out netEvent) <= 0)
            {
                if (_client.Service(15, out netEvent) <= 0)
                    return;
            }

            switch (netEvent.Type)
            {
                case ENet.EventType.None:
                    break;

                case ENet.EventType.Connect:
                    Debug.Log("Client connected to server - ID: " + _peer.ID);
                    break;

                case ENet.EventType.Disconnect:
                    Debug.Log("Client disconnected from server");
                    isConnected = false;
                    break;

                case ENet.EventType.Timeout:
                    Debug.Log("Client connection timeout");
                    isConnected = false;
                    break;

                case ENet.EventType.Receive:
                    Debug.Log("Packet received from server - Channel ID: " + netEvent.ChannelID + ", Data length: " + netEvent.Packet.Length);

                    if (netEvent.Packet.Length == 0)
                    {
                        Debug.LogWarning("(Client) Empty packet received. Skipping.");
                        return;
                    }

                    try
                    {
                        ParsePacket(ref netEvent);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"(Client) Exception during packet parsing: {ex}");
                    }
                    break;
            }
        }
    }


    private void ParsePacket(ref ENet.Event netEvent)
    {
        try
        {
            var readBuffer = new byte[netEvent.Packet.Length];
            netEvent.Packet.CopyTo(readBuffer);

            string packet = Encoding.UTF8.GetString(readBuffer).Trim('\0');
            Debug.Log($"(Client) Raw packet: {packet}");

            string[] parts = packet.Split('|');
            if (parts.Length < 5)
            {
                Debug.LogError($"(Client) Malformed packet: {packet}");
                return;
            }

            string packetType = parts[0];
            switch (packetType)
            {
                case "LoginSuccess":

                    string pid = parts[1];
                    string username = parts[2];
                    string displayName = parts[3];
                    string adminLvl = parts[4];

                    myInfo = new Player(uint.Parse(pid), username, displayName, int.Parse(adminLvl));

                    Debug.Log($"Logged In as {myInfo.DisplayName}");

                    LoginSceneHandle.Instance.LoadNextScene();
                    break;
                case "LoginFailed":
                    Debug.Log("Incorrect Username or Password");
                    LoginSceneHandle.Instance.FailedLogin();
                    break;
                case "ItemDatabase":
                    SaveItemDatabaseToFile(packet);
                    break;
                case "GetWorld":
                    string[] packetData = packet.Split(',');
                    string rawPacket = string.Join(",", packetData).Replace("GetWorld|", "");
                    ProcessWorldData(rawPacket);
                    break;
                default:
                    Debug.LogWarning($"(Client) Unknown packet type: {packetType}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"(Client) Error parsing packet: {ex}");
        }
        finally
        {
            netEvent.Packet.Dispose();
        }
    }
    public void ProcessWorldData(string rawPacket)
    {
        string[] tileData = rawPacket.Split(',');

        List<Tile> tiles = new List<Tile>();

        foreach (string data in tileData)
        {
            string[] parts = data.Split('|');

            if (parts.Length == 5)
            {
                int x = int.Parse(parts[0]);
                int y = int.Parse(parts[1]);
                int itembg = int.Parse(parts[2]);
                int itemfg = int.Parse(parts[3]);
                int itemdoor = int.Parse(parts[4]);

                tiles.Add(new Tile(x, y, itembg, itemfg, itemdoor));
            }
            else
            {
                Debug.Log("Invalid tile data: " + data);
            }
        }
        LoadRealmSceneAndBuildWorld(tiles);
    }
    private void LoadRealmSceneAndBuildWorld(List<Tile> tiles)
    {
        SceneManager.LoadSceneAsync("Realm").completed += (operation) =>
        {
            if (tiles != null && tiles.Count > 0)
            {
                WorldBuild.set.BuildWorld(tiles);
            }
            else
            {
                Debug.LogError("Tiles list is null or empty.");
            }
            _myPlayer = Instantiate(myPlayerFactory, WorldBuild.set.getSpawnPlayerPosition(), Quaternion.identity);
        };
    }
    public static void SaveItemDatabaseToFile(string rawData)
    {
        string gameDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string filePath = Path.Combine(gameDirectory, "items.txt");

        Debug.Log($"Saving to file: {filePath}");
        Debug.Log($"Raw Data: {rawData}");

        try
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                string[] lines = rawData.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                Debug.Log($"Total lines to write: {lines.Length}");

                bool isFirstLine = true;

                foreach (var line in lines)
                {
                    if (isFirstLine)
                    {
                        isFirstLine = false;
                        continue;
                    }

                    string[] parts = line.Split('|');
                    Debug.Log($"Line parts length: {parts.Length}");

                    if (parts.Length == 14)
                    {
                        writer.WriteLine($"ID: {parts[0]}");
                        writer.WriteLine($"Name: {parts[1]}");
                        writer.WriteLine($"Info: {parts[2]}");
                        writer.WriteLine($"Type: {parts[3]}");
                        writer.WriteLine($"Part: {parts[4]}");
                        writer.WriteLine($"Rarity: {parts[5]}");
                        writer.WriteLine($"Hardness: {parts[6]}");
                        writer.WriteLine($"Farmability: {parts[7]}");
                        writer.WriteLine($"Tradeable: {parts[8]}");
                        writer.WriteLine($"Trashable: {parts[9]}");
                        writer.WriteLine($"Droppable: {parts[10]}");
                        writer.WriteLine($"Lockable: {parts[11]}");
                        writer.WriteLine($"Vendable: {parts[12]}");
                        writer.WriteLine($"Solid: {parts[13]}");
                        writer.WriteLine();
                    }
                    else
                    {
                        Debug.LogWarning($"Skipping invalid line: {line}");
                    }
                }
                writer.Flush();
            }

            Debug.Log($"Item database saved to {filePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error saving item database: {ex.Message}");
        }
    }
    public void SendLoginPacket(ENet.Packet packet)
    {
        _peer.Send(0, ref packet);
        Debug.Log("(Client) Login packet sent.");
    }
    public void SendRegisterPacket(ENet.Packet packet)
    {
        _peer.Send(0, ref packet);
        Debug.Log("(Client) Register packet sent.");
    }
    public void SendWorldPacket(ENet.Packet packet)
    {
        _peer.Send(0, ref packet);
        Debug.Log("(Client) World packet sent.");
    }
}