using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using LiteNetLib;
using LiteNetLib.Utils;
using System.IO.Compression;
using System.IO;
using System.Text;
using System.Collections;

public class Connection : MonoBehaviour
{
    public static Connection Instance;

    public static NetManager client;
    public static EventBasedNetListener listener;

    public bool IsConnected { get; private set; }

    public NetPeer Server { get; private set; }

    public string MyUsername;
    public int MyPeerID;

    public GameObject playerPrefab;

    public Vector3 playerSpawnPosition;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        listener = new EventBasedNetListener();
        client = new NetManager(listener);
        client.Start();

        Debug.Log("Client Created");

        listener.PeerConnectedEvent += peer => {
            Debug.Log("Connected to server");
            IsConnected = true;
            Server = peer;
        };

        listener.NetworkReceiveEvent += (peer, reader, channel, deliveryMethod) => {
            try
            {
                string message = reader.GetString();
                Debug.Log("Packet|" + message);

                if (message == "UsernameTaken")
                {
                    LoginSceneHandle.Instance.FailedRegister();
                }
                else if (message == "OnWrongPassword")
                {
                    LoginSceneHandle.Instance.FailedLogin();
                }
                else if (message == "RegSuccess" || message == "LoginSuccess")
                {
                    LoginSceneHandle.Instance.LoadNextScene();
                }
                else if (message == "WORLD_EXIST")
                {
                    Debug.Log("World Exist Loading Tile Map");
                }
                else if (message == "WorldCreated")
                {
                    Debug.Log("World Created Loading Tile Map");
                }
                else if (message == "WorldData")
                {
                    SceneManager.LoadScene("Realm");

                    int dataLength = reader.GetInt();
                    if (dataLength <= 0 || dataLength > reader.AvailableBytes)
                    {
                        Debug.LogError($"Invalid data length received: {dataLength}. Available bytes: {reader.AvailableBytes}");
                        return;
                    }
                    byte[] compressedData = new byte[dataLength];
                    reader.GetBytes(compressedData, dataLength);

                    string jsonResponse = DecompressData(compressedData);
                    Debug.Log($"Received World Data: {jsonResponse}");

                    WorldData worldData = JsonUtility.FromJson<WorldData>(jsonResponse);

                    if (worldData == null || worldData.Tiles == null)
                    {
                        Debug.LogError("Failed to deserialize WorldData or tiles are null.");
                        return;
                    }

                    List<TileData> tiles = new List<TileData>();
                    foreach (var tile in worldData.Tiles)
                    {
                        tiles.Add(new TileData
                        {
                            ItemID_BG = tile.ItemID_BG,
                            ItemID_FG = tile.ItemID_FG,
                            ItemID_Door = tile.ItemID_Door,
                            X = tile.X,
                            Y = tile.Y
                        });
                    }

                    StartCoroutine(WaitForWorldRendererAndRender(tiles));

                    if (worldData.Players != null)
                    {
                        foreach (var playerData in worldData.Players)
                        {
                            Debug.Log($"Spawning Player at pos x {playerSpawnPosition.x} and pos y {playerSpawnPosition.y}");
                            SpawnPlayerWithDelay(playerData.PeerID, playerData.InWorldName, playerSpawnPosition, 0.5f);
                        }
                    }
                }
                else if (message == "OnUpdateItemsDat")
                {
                    Debug.Log("Received: " + message);

                    int itemCount = reader.GetInt();
                    Debug.Log("Item Count: " + itemCount);

                    for (int i = 0; i < itemCount; i++)
                    {
                        Items item = new Items
                        {
                            itemID = reader.GetInt(),
                            itemName = reader.GetString(),
                            itemType = reader.GetInt(),
                            description = reader.GetString(),
                            itemRarity = reader.GetInt(),
                            maxStock = reader.GetInt(),
                            WearableType = reader.GetInt(),
                            extraClothe1 = reader.GetString(),
                            extraClothe2 = reader.GetString(),
                            isBackground = reader.GetInt(),
                            isFarmable = reader.GetInt()
                        };

                        ItemDatabase.Instance.AddItem(item);

                        Debug.Log($"Item Added: {item.itemName} (ID: {item.itemID})");
                    }
                }

                else if (message == "GetPlayerData")
                {
                    string playerUID = reader.GetString();
                    string username = reader.GetString();
                    string displayName = reader.GetString();
                    Debug.Log($"Received data: {message}, {playerUID}, {username}, {displayName}, ...");
                    string email = reader.GetString();
                    string ip = reader.GetString();
                    string macAddress = reader.GetString();
                    string headItem = reader.GetString();
                    string hairItem = reader.GetString();
                    string faceItem = reader.GetString();
                    string wingItem = reader.GetString();
                    string handItem = reader.GetString();
                    string pantItem = reader.GetString();
                    string shirtItem = reader.GetString();
                    string footItem = reader.GetString();
                    int gemsCount = reader.GetInt();
                    int adminLevel = reader.GetInt();
                    int onQuestID = reader.GetInt();
                    int onQuestStep = reader.GetInt();
                    int currentLevel = reader.GetInt();
                    int xpCount = reader.GetInt();
                    string worldName = reader.GetString();

                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.SetPlayerData(new Player
                        {
                            PeerID = MyPeerID,
                            PlayerUID = playerUID,
                            Username = username,
                            DisplayName = displayName,
                            Email = email,
                            IP = ip,
                            MACAddress = macAddress,
                            GemsCount = gemsCount,
                            AdminLevel = adminLevel,
                            OnQuestID = onQuestID,
                            OnQuestStep = onQuestStep,
                            CurrentLevel = currentLevel,
                            XPCount = xpCount,
                            InWorldName = worldName
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Error receiving data: " + ex.Message);
            }
        };

        listener.PeerDisconnectedEvent += (peer, disconnectInfo) => {
            Debug.Log("Disconnected from server");
            IsConnected = false;
        };
        ConnectToServer();
    }
    private System.Collections.IEnumerator WaitForWorldRendererAndRender(List<TileData> tiles)
    {
        while (FindObjectOfType<WorldRenderer>() == null)
        {
            yield return null;
        }

        WorldRenderer worldRenderer = FindObjectOfType<WorldRenderer>();
        if (worldRenderer != null)
        {
            worldRenderer.RenderWorld(tiles);
        }
        else
        {
            Debug.LogError("WorldRenderer still not found after waiting.");
        }
    }
    public void SpawnPlayer(int peerId, string worldName, Vector3 position)
    {
        position = playerSpawnPosition;
        Debug.Log($"Spawning player {peerId} in world {worldName} at position ({position.x}, {position.y}, {position.z})");

        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab is not assigned!");
            return;
        }

        GameObject newPlayer = Instantiate(playerPrefab, position, Quaternion.identity);
        PlayerMovement playerController = newPlayer.GetComponent<PlayerMovement>();
        playerController.SetPlayerName(GameManager.CurrentPlayerData.DisplayName);
        if (newPlayer == null)
        {
            Debug.LogError("Failed to instantiate player prefab.");
            return;
        }
        Camera mainCamera = Camera.main ?? GameObject.FindObjectOfType<Camera>();

        if (mainCamera != null)
        {
            mainCamera.GetComponent<CameraFollow>().player = newPlayer.transform;
        }
        else
        {
            Debug.LogError("Main camera not found or not tagged as 'MainCamera'.");
        }

        newPlayer.name = $"Player_{peerId}";
        Debug.Log($"Player {peerId} successfully spawned.");
    }
    public void SpawnPlayerWithDelay(int peerId, string worldName, Vector3 position, float delay)
    {
        StartCoroutine(DelayedSpawnPlayer(peerId, worldName, position, delay));
    }
    private IEnumerator DelayedSpawnPlayer(int peerId, string worldName, Vector3 position, float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnPlayer(peerId, worldName, position);
    }
    private void ConnectToServer()
    {
        client.Connect("127.0.0.1", 9050, "8ef9as101");
    }

    private void OnDestroy()
    {
        client.Stop();
    }

    private void Update()
    {
        client.PollEvents();
    }
    private string DecompressData(byte[] compressedData)
    {
        using (var ms = new MemoryStream(compressedData))
        using (var gzip = new GZipStream(ms, CompressionMode.Decompress))
        using (var reader = new StreamReader(gzip, Encoding.UTF8))
        {
            return reader.ReadToEnd();
        }
    }
}
[System.Serializable]
public class TileData
{
    public int ItemID_BG;
    public int ItemID_FG;
    public int ItemID_Door;
    public int X;
    public int Y;
}

[System.Serializable]
public class WorldData
{
    public string WorldName;
    public int WorldLength;
    public int WorldWidth;
    public TileData[] Tiles;
    public List<Player> Players;
}