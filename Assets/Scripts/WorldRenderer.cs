using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class WorldRenderer : MonoBehaviour
{
    public Tilemap backgroundTilemap;
    public Tilemap foregroundTilemap;
    public Tilemap doorTileMap;

    private void Start()
    {
        if (backgroundTilemap == null)
        {
            Debug.LogError("Background tilemap is not assigned.");
        }
        if (foregroundTilemap == null)
        {
            Debug.LogError("Foreground tilemap is not assigned.");
        }
        if (doorTileMap == null)
        {
            Debug.LogError("Door tilemap is not assigned.");
        }
    }

    public void RenderWorld(List<TileData> tiles)
    {
        if (backgroundTilemap == null || foregroundTilemap == null || doorTileMap == null)
        {
            Debug.LogError("Tilemaps are not assigned.");
            return;
        }

        foreach (TileData tileData in tiles)
        {
            TileBase fgTile = TileManager.Instance.GetTile(tileData.ItemID_FG);
            TileBase bgTile = TileManager.Instance.GetTile(tileData.ItemID_BG);
            TileBase doorTile = TileManager.Instance.GetTile(tileData.ItemID_Door);

            Vector3Int tilePosition = new Vector3Int(tileData.X, tileData.Y, 0);

            foregroundTilemap.SetTile(tilePosition, fgTile);
            backgroundTilemap.SetTile(tilePosition, bgTile);
            doorTileMap.SetTile(tilePosition, doorTile);
        }
    }
}