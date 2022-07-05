using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public enum EditorClickMode { Add, Remove, Destroy, Move };
    public enum BagLocation { Inventory, Ground };

    public KeyCode inventoryOpenKey = KeyCode.Tab;
    public KeyCode rotateHeldItemKey = KeyCode.R;
    public PlayerInventory inventory;
    public PlayerCamera playerCam;

    public InventoryItem itemToSpawn;
    public Sprite emptySlotSprite;

    public InventoryContainer.Rotation inputRotation;
    public EditorClickMode editorClickMode;

    #region Prefabs
    public GameObject containerUI;
    public GameObject slotUI;
    public GameObject heldItemUI;
    #endregion

    public int textHeight = 20;
    public int slotHeight = 50;
    public int padding = 10;

    private GameObject inventoryHUD;
    private GameObject groundHUD;
    private int bagsOnGround;
    private List<InventoryContainer> containers = new List<InventoryContainer>();
    private Dictionary<InventoryContainer, GameObject> objectToGameObjectDictionary = new Dictionary<InventoryContainer, GameObject>();

    #region Item Drag and drop
    private InventoryItem itemHeldByMouse;
    private InventoryContainer.Rotation mouseHeldRotation;

    //Used for cancelling mouse selection
    private GameObject lastSlotClicked;
    private InventoryContainer.Rotation pickedUpRotation;
    private InventoryContainer bagOfItemHeld;
    private Vector2Int locationOfItemHeldInOldBag;

    #endregion

    private bool isInventoryOpen = false;
    private bool spamToggleInv = false;
    private bool spamToggleRot = false;

    private void Start() {
        inventoryHUD = transform.Find("Inventory").gameObject;
        groundHUD = transform.Find("Ground").gameObject;
        heldItemUI = Instantiate(heldItemUI, transform);

        AssignAndConstructContainer(inventory.bag1, BagLocation.Inventory);
        AssignAndConstructContainer(inventory.bag2, BagLocation.Inventory);
        AssignAndConstructContainer(inventory.bag3, BagLocation.Inventory);
        AssignAndConstructContainer(inventory.bag4, BagLocation.Inventory);
    }

    private void Update() {
        if (Input.GetKeyDown(inventoryOpenKey) && !spamToggleInv) {
            spamToggleInv = true;
            isInventoryOpen = !isInventoryOpen;
        }else if (Input.GetKeyUp(inventoryOpenKey)) {
            spamToggleInv = false;
        }

        if(Input.GetKeyDown(rotateHeldItemKey) && !spamToggleRot) {
            spamToggleRot = true;
            SwapRotation();
        }else if (Input.GetKeyUp(rotateHeldItemKey)) {
            spamToggleRot = false;
        }

        playerCam.cameraCanMove = (!isInventoryOpen);
        playerCam.lockCursor = (!isInventoryOpen);
        inventoryHUD.SetActive(isInventoryOpen);
        groundHUD.SetActive(isInventoryOpen && bagsOnGround != 0);

        #region Cursor
        if (!isInventoryOpen && itemHeldByMouse != null) {
            ResetItemOnCursor();
        }
        if(itemHeldByMouse != null) {
            heldItemUI.GetComponent<Image>().sprite = itemHeldByMouse.icon;
            if (itemHeldByMouse.IsStackable()) heldItemUI.transform.Find("Stack").GetComponent<TMPro.TextMeshProUGUI>().text = itemHeldByMouse.stackSize.ToString();
            else heldItemUI.transform.Find("Stack").GetComponent<TMPro.TextMeshProUGUI>().text = "";

            heldItemUI.GetComponent<RectTransform>().sizeDelta = new Vector2(slotHeight / 1.6f * itemHeldByMouse.itemSize.x, slotHeight / 1.6f * itemHeldByMouse.itemSize.y);
            Vector2 itemLocalPosition;
            if (mouseHeldRotation == InventoryContainer.Rotation.Horizontal) {
                itemLocalPosition = new Vector2(slotHeight / 4 * itemHeldByMouse.itemSize.x, -slotHeight / 4 * itemHeldByMouse.itemSize.y);
                heldItemUI.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);
            } else {
                itemLocalPosition = new Vector2(slotHeight / 4 * itemHeldByMouse.itemSize.y, -slotHeight / 4 * itemHeldByMouse.itemSize.x);
                heldItemUI.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, -90);
            }
            heldItemUI.transform.position = new Vector2(Input.mousePosition.x + itemLocalPosition.x, Input.mousePosition.y + itemLocalPosition.y);
            //heldItemUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(Input.mousePosition.x + itemLocalPosition.x, Input.mousePosition.y + itemLocalPosition.y);
        } else {
            heldItemUI.GetComponent<Image>().sprite = emptySlotSprite;
            heldItemUI.transform.Find("Stack").GetComponent<TMPro.TextMeshProUGUI>().text = "";
        }
        #endregion
    }

    #region Bag Management

    private void UpdateInventoryUI(InventoryContainer bag, InventoryContainer.ChangeType type) {
        Debug.Log("Change to " + bag.label);

        if(type == InventoryContainer.ChangeType.Edit) {
            ReloadBag(bag);
        }
        if(type == InventoryContainer.ChangeType.Destroy) {
            Destroy(objectToGameObjectDictionary[bag]);
            containers.Remove(bag);
            objectToGameObjectDictionary.Remove(bag);
        }
    }

    private void ReloadBag(InventoryContainer bag) {
        GameObject bagGO = objectToGameObjectDictionary[bag];
        //Reset name
        bagGO.transform.Find("Name").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = bag.label + " (" + bag.GetNumberOfItems() + "/" + bag.GetCapacity() + ")";
        Transform parent = bagGO.transform.Find("ItemsParent").transform;
        List<InventoryItem> rollingCheck = new List<InventoryItem>();

        //Change icons
        for (int y = 0; y < bag.containerSize.y; y++) {
            for(int x = 0; x < bag.containerSize.x; x++) {
                InventoryItem currentItem = bag.AccessItem(new Vector2Int(x, y));

                int current = (y * bag.containerSize.x) + x;
                Transform icon = parent.GetChild(current).Find("Icon");
                icon.Find("Stack").GetComponent<TMPro.TextMeshProUGUI>().text = "";

                //Setting the sprite
                if (currentItem == null) {
                    icon.GetComponent<Image>().sprite = emptySlotSprite;
                    continue;
                }else if (rollingCheck.Contains(currentItem)) {
                    icon.GetComponent<Image>().sprite = emptySlotSprite;
                    continue;
                }
                icon.GetComponent<Image>().sprite = currentItem.icon;
                if(currentItem.IsStackable()) icon.Find("Stack").GetComponent<TMPro.TextMeshProUGUI>().text = currentItem.stackSize.ToString();

                //Figure out rotation of item in bag
                InventoryContainer.Rotation rotation = bag.GetRotationOfItem(new Vector2Int(x, y));

                //Sets size of that icon
                RectTransform iconRect = icon.GetComponent<RectTransform>();
                iconRect.sizeDelta = new Vector2(slotHeight * currentItem.itemSize.x, slotHeight * currentItem.itemSize.y);

                //Sets rotation and size of that item
                if (rotation == InventoryContainer.Rotation.Horizontal) {
                    Debug.Log("Slot made Horizontal");
                    iconRect.anchoredPosition = new Vector2(slotHeight / 2 * currentItem.itemSize.x, -slotHeight / 2 * currentItem.itemSize.y);
                    iconRect.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                } else if (rotation == InventoryContainer.Rotation.Vertical){
                    Debug.Log("Slot made Vertical");
                    iconRect.rotation = Quaternion.Euler(new Vector3(0, 0, -90));
                    iconRect.anchoredPosition = new Vector2(slotHeight / 2 * currentItem.itemSize.y, -slotHeight / 2 * currentItem.itemSize.x);
                }
                rollingCheck.Add(currentItem);
            }
        }
    }

    public void AssignAndConstructContainer(InventoryContainer bag, BagLocation insert) {
        GameObject target;

        switch (insert) {
            case BagLocation.Inventory:
                target = inventoryHUD;
                break;
            case BagLocation.Ground:
                target = groundHUD;
                bagsOnGround++;
                break;
            default:
                target = groundHUD;
                break;
        }

        GameObject newContainerUI = Instantiate(containerUI, Vector3.zero, Quaternion.identity);
        newContainerUI.transform.SetParent(target.transform.Find("Viewport").Find("Content"));

        //Set container height on inventory panel
        Vector3 currentScale = newContainerUI.GetComponent<RectTransform>().sizeDelta;
        newContainerUI.GetComponent<RectTransform>().sizeDelta = new Vector2(currentScale.x, textHeight + (slotHeight * bag.containerSize.y) + (padding * 3));
        newContainerUI.transform.Find("ItemsParent").GetComponent<RectTransform>().sizeDelta = new Vector2(0, slotHeight * bag.containerSize.y);

        //Set name
        newContainerUI.transform.Find("Name").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = bag.label + " (" + bag.GetNumberOfItems() + "/" + bag.GetCapacity() + ")";

        //Create item slots
        Vector2 slotOffset = new Vector2(slotHeight / 2, -slotHeight / 2);

        Transform parentTransform = newContainerUI.transform.Find("ItemsParent");
        for(int y = 0; y < bag.containerSize.y; y++) {
            for(int x = 0; x < bag.containerSize.x; x++) {
                GameObject slot = Instantiate(slotUI, Vector3.zero, Quaternion.identity);
                slot.transform.SetParent(parentTransform);
                slot.GetComponent<RectTransform>().anchoredPosition = new Vector2(slotHeight * x, -slotHeight * y) + slotOffset;
                slot.name = "Slot " + ((y * bag.containerSize.x) + x);

                //Set up OnClick listener
                int inX = new int();
                inX = x;
                int inY = new int();
                inY = y;
                slot.GetComponent<Button>().onClick.AddListener(delegate { BagButtonOnClick(bag, inX, inY); });
            }
        }

        //Assign to variables
        bag.onItemChangedCallback += UpdateInventoryUI;
        containers.Add(bag);
        objectToGameObjectDictionary.Add(bag, newContainerUI);
        ReloadBag(bag);
    }

    public void UnregisterBag(InventoryContainer bag, BagLocation location) {
        if (location == BagLocation.Ground) bagsOnGround--;
        bag.DestroyBag();
    }

    public void BagButtonOnClick(InventoryContainer bag, int x, int y) {
        Vector2Int clickLocation = new Vector2Int(x, y);

        switch (editorClickMode) {
            case EditorClickMode.Add:
                bag.AddItem(itemToSpawn.Clone(), clickLocation, inputRotation);
                return;
            case EditorClickMode.Destroy:
                Debug.Log("Destroy not enabled");
                //UnregisterBag(bag);
                return;
            case EditorClickMode.Remove:
                bag.RemoveItem(clickLocation);
                return;
            case EditorClickMode.Move:

                Vector2Int firstOccurance = new Vector2Int(-1, -1);
                InventoryItem clickedItem = bag.AccessItem(clickLocation);
                if(clickedItem != null) {
                    firstOccurance = bag.LocateFirstOccurance(clickedItem);
                }

                //Get first occurence of Item of slot clicked
                //Should never be null
                GameObject slotClicked = null;
                GameObject firstOccuranceSlotClicked = null;
                foreach (Transform child in objectToGameObjectDictionary[bag].transform.Find("ItemsParent")) {
                    if (child.name.Equals("Slot " + ((y * bag.containerSize.x) + x))) {
                        slotClicked = child.gameObject;
                        if(firstOccuranceSlotClicked != null) {
                            break;
                        }
                    }
                    if(child.name.Equals("Slot " + ((firstOccurance.y * bag.containerSize.x) + firstOccurance.x))) {
                        firstOccuranceSlotClicked = child.gameObject;
                        if(slotClicked != null) {
                            break;
                        }
                    }

                }

                if (itemHeldByMouse == null) {
                    lastSlotClicked = slotClicked;
                    firstOccuranceSlotClicked.transform.Find("Icon").gameObject.SetActive(false);
                    //Rotation has to be done first to get the rotation of the item
                    SwapRotation(bag.GetRotationOfItem(clickLocation));
                    pickedUpRotation = mouseHeldRotation;
                    InventoryItem itemClicked = bag.AccessItem(clickLocation);
                    if (itemClicked == null) return;
                    itemHeldByMouse = itemClicked;
                    bagOfItemHeld = bag;
                    locationOfItemHeldInOldBag = firstOccurance;
                    return;
                }
                //Item held needs to be moved
                else {
                    //Try add the item to the slot (is true for cases of item added to slot, item added to stack or item consumed by stack
                    bagOfItemHeld.RemoveItem(locationOfItemHeldInOldBag);
                    bool insertSuccess = bag.AddItem(itemHeldByMouse, clickLocation, mouseHeldRotation);
                    if (insertSuccess) {
                        if (itemHeldByMouse.IsStackable()) {
                            if(itemHeldByMouse.stackSize == 0) {
                                //Item added, stack size reduced to 0
                                lastSlotClicked.transform.Find("Icon").gameObject.SetActive(true);
                                slotClicked.transform.Find("Icon").gameObject.SetActive(true);
                                itemHeldByMouse = null;
                                bagOfItemHeld = null;
                                return;
                            } else {
                                //Item added in same spot
                                if (bag == bagOfItemHeld && clickLocation.Equals(locationOfItemHeldInOldBag)) {
                                    lastSlotClicked.transform.Find("Icon").gameObject.SetActive(true);
                                    slotClicked.transform.Find("Icon").gameObject.SetActive(true);
                                    itemHeldByMouse = null;
                                    bagOfItemHeld = null;
                                    return;
                                }
                                //Item added to stack
                                if(bag.AccessItem(clickLocation) != itemHeldByMouse) {
                                    //Stay on cursor
                                    return;
                                }
                                //Item added to new slot
                                lastSlotClicked.transform.Find("Icon").gameObject.SetActive(true);
                                slotClicked.transform.Find("Icon").gameObject.SetActive(true);
                                itemHeldByMouse = null;
                                bagOfItemHeld = null;
                                return;
                            }
                        }
                        //Item added, non stackable
                        lastSlotClicked.transform.Find("Icon").gameObject.SetActive(true);
                        slotClicked.transform.Find("Icon").gameObject.SetActive(true);
                        itemHeldByMouse = null;
                        bagOfItemHeld = null;
                        return;
                    } else {
                        //Insert not successful nor stack successful
                        bagOfItemHeld.AddItem(itemHeldByMouse, locationOfItemHeldInOldBag, pickedUpRotation);
                        return;
                    }
                }
            default:
                return;
        }
    }

    #endregion

    #region Inventory Mouse Management

    private void ResetItemOnCursor() {
        lastSlotClicked.transform.Find("Icon").gameObject.SetActive(true);
        itemHeldByMouse = null;
        bagOfItemHeld = null;
    }

    private void SwapRotation() {
        if(mouseHeldRotation == InventoryContainer.Rotation.Horizontal) {
            SwapRotation(InventoryContainer.Rotation.Vertical);
        }
        else if(mouseHeldRotation == InventoryContainer.Rotation.Vertical) {
            SwapRotation(InventoryContainer.Rotation.Horizontal);
        }
    }

    private void SwapRotation(InventoryContainer.Rotation newRotation) {
        mouseHeldRotation = newRotation;
        //Reset rotation for square items
        if(itemHeldByMouse != null) {
            if(itemHeldByMouse.itemSize.x == itemHeldByMouse.itemSize.y) {
                mouseHeldRotation = InventoryContainer.Rotation.Horizontal;
            }
        }
    }

    #endregion

}
