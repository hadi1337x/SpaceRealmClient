using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Tilemaps;
using Unity.VisualScripting;

public class WorldBuild : MonoBehaviour
{
    public Tilemap Background;
    public Tilemap Foreground;
    public Tilemap Door;

    public static WorldBuild set;
    public ItemsDatManager itemsManager;

    public static Vector3 spawnPosition;
    private void Awake()
    {
        if (set == null)
        {
            set = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        if (itemsManager == null)
        {
            GameObject itemsManagerObject = GameObject.Find("ItemsManager");
            if (itemsManagerObject != null)
            {
                itemsManager = itemsManagerObject.GetComponent<ItemsDatManager>();
            }
            else
            {
                Debug.LogError("ItemsDatManager GameObject not found in the scene!");
            }
        }
    }

    public void BuildWorld(List<Tile> tiles)
    {
        foreach (Tile tile in tiles)
        {
            Vector3Int position = new Vector3Int(tile.X, tile.Y, 0);
            Background.SetTile(position, itemsManager.getTileById(tile.Itembg));
            Foreground.SetTile(position, itemsManager.getTileById(tile.Itemfg));
            Door.SetTile(position, itemsManager.getTileById(tile.Itemdoor));
            if (tile.Itemdoor == 5)
            {
                Vector3 spawn = new Vector3(position.x, position.y+1, 0);
                spawnPosition = spawn;
            }
        }
    }
    public Vector3 getSpawnPlayerPosition()
    {
        Vector3 pos;
        pos = spawnPosition;
        return pos;
    }
}
