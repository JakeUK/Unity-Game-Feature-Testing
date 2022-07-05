using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Item", menuName = "Item")]
public class InventoryItem : ScriptableObject
{
    public int id;

    public string label;
    public Vector2Int itemSize;
    public Sprite icon;

    public bool stackable = false;
    public int stackCapacity;
    public int stackSize;

    public InventoryItem(string label, Vector2Int itemSize) {
        this.label = label;
        this.itemSize = itemSize;
    }

    protected void MakeStackable(int capacity, int size) {
        stackable = true;
        stackSize = size;
        stackCapacity = capacity;
    }

    public bool IsStackable() {
        return stackable;
    }

    public bool AddToStack(InventoryItem other) {
        if (other.IsStackable() && id == other.id && stackSize != stackCapacity) {
            stackSize += other.stackSize;
            if(stackSize >= stackCapacity) {
                other.stackSize = stackSize - stackCapacity;
                stackSize = stackCapacity;
                return true;
            } else {
                other.stackSize = 0;
                return true;
            }
        } else return false;
    }

    public void SetStackSize(int size) {
        stackSize = size;
        if (stackSize > stackCapacity) stackSize = stackCapacity;
    }

    public InventoryItem Clone() {
        InventoryItem clone = new InventoryItem(label, itemSize);
        clone.icon = icon;
        clone.id = id;
        if (stackable) {
            clone.MakeStackable(stackCapacity, stackSize);
        }
        return clone;
    }

}

[CustomEditor (typeof(InventoryItem))]
public class InventoryItemEditor : Editor {
    public override void OnInspectorGUI() {
        InventoryItem item = (InventoryItem) target;

        item.id = EditorGUILayout.IntField("ID", item.id);
        item.id = Mathf.Max(0, item.id);

        item.label = EditorGUILayout.TextField("Name", item.label);
        item.itemSize = EditorGUILayout.Vector2IntField("Dimensions", item.itemSize);
        item.itemSize.Clamp(Vector2Int.one, Vector2Int.one * 6);

        item.icon = EditorGUILayout.ObjectField("Icon", item.icon, typeof(Sprite), true) as Sprite;

        item.stackable = GUILayout.Toggle(item.stackable, "Stackable?");

        if (item.stackable) {
            item.stackCapacity = EditorGUILayout.IntField("Stack Capacity", item.stackCapacity);
            item.stackCapacity = Mathf.Max(1, item.stackCapacity);

            item.stackSize = EditorGUILayout.IntField("Items in Stack", item.stackSize);
            item.stackSize = Mathf.Clamp(item.stackSize, 1, item.stackCapacity);

        }
    }
}
