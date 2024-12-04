using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance { get; private set; }

    public Dictionary<int, Items> items = new Dictionary<int, Items>();

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

    public void AddItem(Items item)
    {
        if (!items.ContainsKey(item.itemID))
        {
            items.Add(item.itemID, item);
        }
    }

    public Items GetItem(int itemID)
    {
        return items.ContainsKey(itemID) ? items[itemID] : null;
    }

}