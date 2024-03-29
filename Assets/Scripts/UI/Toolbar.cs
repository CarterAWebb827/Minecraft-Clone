using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Toolbar : MonoBehaviour {
    private World world;
    public Player player;

    public RectTransform highlight;
    public ItemSlot[] itemSlots;

    int slotIndex;

    private void Start() {
        world = GameObject.Find("World").GetComponent<World>();
        player = GameObject.Find("Player").GetComponent<Player>();

        slotIndex = 0;

        foreach (ItemSlot slot in itemSlots) {
            slot.icon.sprite = world.blockTypes[slot.itemID].icon;
            slot.icon.enabled = true;
        }

        player.selectedBlockIndex = itemSlots[slotIndex].itemID;
    }

    private void Update() {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0) {
            if (scroll > 0) {
                slotIndex--;
            } else {
                slotIndex++;
            }

            if (slotIndex > itemSlots.Length - 1) {
                slotIndex = 0;
            }
            
            if (slotIndex < 0) {
                slotIndex = itemSlots.Length - 1;
            }
            
            highlight.position = itemSlots[slotIndex].icon.transform.position;
            player.selectedBlockIndex = itemSlots[slotIndex].itemID;
        }
    }
}

[System.Serializable]
public class ItemSlot {
    public byte itemID;
    public Image icon;
}