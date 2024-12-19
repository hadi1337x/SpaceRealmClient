using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class ItemsDatManager : MonoBehaviour
{
    public static ItemsDatManager get;  // Singleton reference

    public TileBase blankTile;
    public TileBase dirtTile;
    public TileBase caveBack;
    public TileBase lavaBlock;
    public TileBase bedrockBlock;
    public TileBase mainDoor;

    private void Awake()
    {
        if (get == null)
        {
            get = this;
            DontDestroyOnLoad(gameObject);  
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public TileBase getTileById(int itemID)
    {
        switch (itemID)
        {
            case 0: return blankTile;
            case 1: return dirtTile;
            case 2: return caveBack;
            case 3: return lavaBlock;
            case 4: return bedrockBlock;
            case 5: return mainDoor;
            default: return blankTile;
        }
    }
}
