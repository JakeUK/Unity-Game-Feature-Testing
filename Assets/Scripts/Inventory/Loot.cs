using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loot : MonoBehaviour {

    InventoryContainer containedBag;

    private void Start() {
        containedBag = new InventoryContainer("Ground", new Vector2Int(6, 6), new Vector2Int(1, 1));
    }

    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;
        other.transform.Find("HUD").GetComponent<InventoryManager>().AssignAndConstructContainer(containedBag, InventoryManager.BagLocation.Ground);
    }

    private void OnTriggerExit(Collider other) {
        if (!other.CompareTag("Player")) return;
        other.transform.Find("HUD").GetComponent<InventoryManager>().UnregisterBag(containedBag, InventoryManager.BagLocation.Ground);
    }
}
