using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "New Container", menuName = "Container")]
public class InventoryContainer : InventoryItem
{
    public enum Rotation { Vertical, Horizontal };
    public enum ChangeType { Edit, Destroy };

    public delegate void OnItemChanged(InventoryContainer bag, ChangeType changeType);
    public OnItemChanged onItemChangedCallback;

    public Vector2Int containerSize;
    private InventoryItem[,] storage;
    private int numberOfItemsContained;

    private bool open = false;

    public InventoryContainer(string label, Vector2Int containerSize, Vector2Int itemSize, InventoryItem[,] storage) : base(label, itemSize) {
        this.containerSize = containerSize;

        //This should be checked when transferring items
        this.storage = storage;

        for(int y = 0; y < containerSize.y; y++) {
            for(int x = 0; x < containerSize.x; x++) {
                if (storage[x, y] != null) {
                    numberOfItemsContained++;
                }
            }
        }
    }

    public InventoryContainer(string label, Vector2Int containerSize, Vector2Int itemSize) : base(label, itemSize){
        this.containerSize = containerSize;
        storage = new InventoryItem[containerSize.x, containerSize.y];
        numberOfItemsContained = 0;
    }

    public bool AddItem(InventoryItem newItem, Vector2Int location, Rotation rotation) {
        //Stack
        InventoryItem clickedItem = AccessItem(location);
        if(clickedItem != null) {
            if(clickedItem.id == newItem.id && clickedItem.IsStackable()) {
                Debug.Log("AddToStack called");
                if (clickedItem.AddToStack(newItem)) {
                    onItemChangedCallback.Invoke(this, ChangeType.Edit);
                    return true;
                }
                return false;
            }
        }

        //Enter new item
        int width = newItem.itemSize.x;
        int height = newItem.itemSize.y;
        if(rotation == Rotation.Vertical) {
            width = newItem.itemSize.y;
            height = newItem.itemSize.x;
        }

        if (numberOfItemsContained > containerSize.x * containerSize.y) return false;
        if (location.x + width > containerSize.x) return false;
        if (location.y + height > containerSize.y) return false;

        //Check for room in the storage
        for(int x = location.x; x < location.x + width; x++) {
            for(int y = location.y; y < location.y + height; y++) {
                if(x > containerSize.x - 1 || y > containerSize.y - 1) {
                    return false;
                }
                if (storage[x, y] != null) {
                    return false;
                }
            }
        }

        //If it makes it through all of this, add item to storage
        for (int x = location.x; x < location.x + width; x++) {
            for (int y = location.y; y < location.y + height; y++) {
                storage[x, y] = newItem;
                numberOfItemsContained++;
            }
        }
        onItemChangedCallback.Invoke(this, ChangeType.Edit);
        return true;
    }

    public InventoryItem RemoveItem(Vector2Int location) {
        InventoryItem itemToRemove = AccessItem(location);
        if (itemToRemove == null) return null;

        for(int y = 0; y < containerSize.y; y++) {
            for(int x = 0; x < containerSize.x; x++) {
                if (storage[x, y] == itemToRemove) {
                    storage[x, y] = null;
                    numberOfItemsContained--;
                }
            }
        }
        onItemChangedCallback.Invoke(this, ChangeType.Edit);
        return itemToRemove;
    }

    public void DestroyBag() {
        onItemChangedCallback.Invoke(this, ChangeType.Destroy);
    }

    public InventoryItem AccessItem(Vector2Int location) {
        try {
            return storage[location.x, location.y];
        }catch (Exception) {
            return null;
        }
    }

    public Rotation GetRotationOfItem(Vector2Int location) {
        InventoryItem searchItem = AccessItem(location);
        if (searchItem == null) return Rotation.Horizontal;
        if (searchItem.itemSize.x == searchItem.itemSize.y) return Rotation.Horizontal;

        //Find first occurance
        int tx = -1;
        int ty = -1;
        for(int y = 0; y < containerSize.y; y++) {
            for(int x = 0; x < containerSize.x; x++) {
                if(AccessItem(new Vector2Int(x, y)) == searchItem){
                    tx = x;
                    ty = y;
                    break;
                }
            }
            if (tx != -1 && ty != -1) break;
        }
        for(int y = ty; y < ty + searchItem.itemSize.y; y++) {
            for(int x = tx; x < tx + searchItem.itemSize.x; x++) {
                if(AccessItem(new Vector2Int(x, y)) != searchItem) {
                    return Rotation.Vertical;
                }
            }
        }
        return Rotation.Horizontal;
    }

    public Vector2Int LocateFirstOccurance(InventoryItem item) {
        for(int y = 0; y < containerSize.y; y++) {
            for(int x = 0; x < containerSize.x; x++) {
                Vector2Int loc = new Vector2Int(x, y);
                if (AccessItem(loc) == item) return loc;
            }
        }
        return new Vector2Int(-1, -1);
    }

    public int GetNumberOfItems() {
        return numberOfItemsContained;
    }

    public int GetCapacity() {
        return containerSize.x * containerSize.y;
    }

    public void Open() {
        open = true;
    }

    public void Close() {
        open = false;
    }

    public bool IsOpen() {
        return open;
    }

    public new InventoryContainer Clone() {
        InventoryContainer clone = new InventoryContainer(label, containerSize, itemSize, storage);
        clone.icon = icon;
        clone.id = id;
        return clone;
    }

}

[CustomEditor(typeof(InventoryContainer))]
public class InventoryContainerEditor : InventoryItemEditor{
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        InventoryContainer container = (InventoryContainer) target;

        container.containerSize = EditorGUILayout.Vector2IntField("Container Dimensions", container.containerSize);
        container.containerSize.Clamp(Vector2Int.one, Vector2Int.one * 6);
    }
}
