using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TileManager : MonoBehaviour
{
    public static TileManager Instance { get; private set; }
    private Dictionary<int, TileBase> tileDictionary;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadTiles();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadTiles()
    {
        tileDictionary = new Dictionary<int, TileBase>();
        TileBase[] tiles = Resources.LoadAll<TileBase>("Tiles");

        foreach (TileBase tile in tiles)
        {
            int itemID;
            if (int.TryParse(tile.name, out itemID))
            {
                tileDictionary.Add(itemID, tile);
                Debug.Log($"Loaded tile: {tile.name} with itemID: {itemID}");
            }
            else
            {
                Debug.LogWarning($"Tile {tile.name} does not have a valid integer name.");
            }
        }
    }

    public TileBase GetTile(int itemID)
    {
        if (tileDictionary.ContainsKey(itemID))
        {
            return tileDictionary[itemID];
        }
        else
        {
            Debug.LogWarning("Tile not found for itemID: " + itemID);
            return null;
        }
    }
}
