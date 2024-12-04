using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class Items
{
    public int itemID { get; set; }
    public string? itemName { get; set; }
    public int itemType { get; set; } //0 means wearable //1 means placeable item
    public TileBase tile { get; set; }
    public string? description { get; set; }
    public int itemRarity { get; set; }
    public int maxStock { get; set; }
    public int WearableType { get; set; }
    public string? extraClothe1 { get; set; }
    public string? extraClothe2 { get; set; }
    public int isBackground { get; set; } //0 means block //1 means background
    public int isFarmable { get; set; } // 0 means normal //1 means farmable
}