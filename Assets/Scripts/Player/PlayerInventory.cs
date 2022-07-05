using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour {

    public InventoryContainer bag1;
    public InventoryContainer bag2;
    public InventoryContainer bag3;
    public InventoryContainer bag4;

    private void Awake() {
        bag1 = new InventoryContainer("Default Bag", new Vector2Int(2, 5), new Vector2Int(1, 1));
        bag2 = new InventoryContainer("Another Default Bag", new Vector2Int(6, 2), new Vector2Int(1, 1));
        bag3 = new InventoryContainer("theres no way theres another bag", new Vector2Int(4, 6), new Vector2Int(1, 1));
        bag4 = new InventoryContainer("cereal2", new Vector2Int(6, 6), new Vector2Int(1, 1));
    }

}

