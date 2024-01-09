using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryInitialiser : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] InventoryInfo inventory;

    [SerializeField] private int inventoryLength;
    [SerializeField] private int inventoryHeight;


    // Start is called before the first frame update
    void Awake()
    {
        //inventory.items = new ItemInfo[inventoryLength];
        int inventorySize = inventoryLength * inventoryHeight;
        inventory.items = new GameObject[inventorySize];
        inventory.item_id = new int[inventorySize];
        inventory.stack = new int[inventorySize];
        for (int j = 0; j < inventoryHeight; j++) 
        {
            for (int i = 0; i < inventoryLength; i++)
            {
                int index = j * inventoryLength + i;
                GameObject itemObject = new GameObject((index+1).ToString());
                itemObject.transform.parent = transform;
                Image image = itemObject.AddComponent<Image>();
                image.color = new Color(1f, 1f, 1f, 0.2f);
                itemObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(-50f+i*60f,100f-j*60f,0.0f);
                itemObject.GetComponent<RectTransform>().sizeDelta = new Vector2(50, 50);
                GameObject count = new GameObject("Count");
                count.transform.parent = itemObject.transform;
                TextMeshProUGUI text = count.AddComponent<TextMeshProUGUI>();
                text.text = "0";
                text.color = new Color(0.0f,0.0f,0.0f,1.0f);
                text.alignment = TextAlignmentOptions.Center;
                text.fontSize = 12;
                count.GetComponent<RectTransform>().anchoredPosition = new Vector3(20f, -20f, 0.0f);
                text.enabled = false;
                inventory.items[index] = itemObject;
                inventory.item_id[index] = 0;
                inventory.stack[index] = 0;
            }
        }
        transform.parent.gameObject.SetActive(false);
        Destroy(this);
    }
}
