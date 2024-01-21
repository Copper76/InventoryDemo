using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Reflection;

public struct ItemInfo
{
    public int item_id;
    public int stack;

    public ItemInfo(int id, int count)
    {
        item_id = id;
        stack = count;
    }
}

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] GameObject describer;

    [SerializeField] private InventoryInfo inventory;
    private GameObject inventoryMenu;

    [SerializeField] private GameObject selected;
    private ItemInfo selectedInfo;
    private int prevSlot;
    private GameObject prevEquipment;
    //private GameObject[] equipments;
    private bool fromSlot;

    [SerializeField] private GameObject descriptor;

    //Movement
    private Rigidbody2D rb;
    private BoxCollider2D bc;
    private float horizontal;
    private bool isGrounded;
    private bool isJumping;
    private float jumpTimeCounter;
    private float maxJumpTime;
    private float jumpForce;

    private float speed;

    // Start is called before the first frame update
    void Awake()
    {
        speed = 5f;
        jumpForce = 30.0f;
        maxJumpTime = 0.3f;
        jumpTimeCounter = maxJumpTime;
        rb = GetComponent<Rigidbody2D>();
        bc = GetComponent<BoxCollider2D>();
        //equipments = new GameObject[] { headSlot, waistSlot, leftHandSlot, rightHandSlot, leftFootSlot, rightFootSlot };
    }

    void Start()
    {
        inventoryMenu = inventory.transform.parent.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (selected.activeInHierarchy)
        {
            selected.GetComponent<RectTransform>().position = Input.mousePosition;
        }
        else
        {
            int slot = IsPointerOverSlot();
            if (slot > -1 && inventory.item_id[slot] != 0)
            {
                descriptor.SetActive(true);
                TextMeshProUGUI[] textFields = descriptor.GetComponentsInChildren<TextMeshProUGUI>();
                PickUpInfo itemInfo = inventory.pickUpPrefabs[inventory.item_id[slot]].GetComponent<PickUpInfo>();
                textFields[0].text = itemInfo.pickUpName;
                textFields[1].text = itemInfo.description;
            }
            else
            {
                descriptor.SetActive(false);
            }
        }
        if (descriptor.activeInHierarchy)
        {
            descriptor.GetComponent<RectTransform>().position = Input.mousePosition + new Vector3(50f,-100f,0f);
        }
    }

    private void FixedUpdate()
    {
        Bounds colliderBounds = bc.bounds;
        Vector3 groundCheckPos = colliderBounds.min + new Vector3(colliderBounds.size.x * 0.5f, 0f, 0f);

        Collider2D colliders = Physics2D.OverlapBox(groundCheckPos, new Vector3(colliderBounds.size.x * 0.9f, 0.01f, 0f), 0.0f, LayerMask.GetMask("Ground"));//3 is set to ground
        if (colliders != null && !isJumping)
        {
            Landed();
        }
        if (isGrounded)
        {
            RaycastHit2D GroundHit = Physics2D.Raycast(transform.position, Vector2.down, Mathf.Infinity, LayerMask.GetMask("Ground"));
            Vector2 normalDirection = Vector2.right - GroundHit.normal * Vector2.Dot(Vector2.right, GroundHit.normal);
            rb.velocity = new Vector2(speed * normalDirection.x * horizontal, speed * normalDirection.y * horizontal);
        }
        else
        {
            if (horizontal != 0)
            {
                rb.velocity = new Vector2(speed * horizontal, rb.velocity.y);
            }
        }
        if (horizontal > 0)
        {
            GetComponent<SpriteRenderer>().flipX = false;
        }
        if (horizontal < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontal = context.ReadValue<Vector2>().x;
    }

    public void ToggleInventory(InputAction.CallbackContext context)
    {
        if (context.performed)
        {

            inventoryMenu.SetActive(!inventoryMenu.activeInHierarchy);
        }
    }

    public void Press(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            int selectedSlot = IsPointerOverSlot();
            if (selectedSlot > -1 && inventory.item_id[selectedSlot] != 0)
            {
                selected.SetActive(true);
                descriptor.SetActive(false);
                Image image = inventory.items[selectedSlot].GetComponent<Image>();
                inventory.items[selectedSlot].GetComponentInChildren<TextMeshProUGUI>().enabled = false;
                selected.GetComponent<Image>().sprite = image.sprite;
                image.sprite = null;
                selectedInfo = new ItemInfo(inventory.item_id[selectedSlot], inventory.stack[selectedSlot]);
                inventory.item_id[selectedSlot] = 0;
                inventory.stack[selectedSlot] = 0;
                prevSlot = selectedSlot;
                fromSlot = true;
            }
        }
    }

    public void Release(InputAction.CallbackContext context)
    {
        if (context.performed && selected.activeInHierarchy)
        {
            int selectedSlot = IsPointerOverSlot();
            if (selectedSlot > -1)//The pointer is on a valid slot
            {
                if ((inventory.equipSlot[selectedInfo.item_id] != selectedSlot && selectedSlot < 6) || (inventory.item_id[selectedSlot] != 0 && inventory.equipSlot[inventory.item_id[selectedSlot]] != prevSlot && prevSlot < 6))//The target is invalid or the item in the target slot cannot be swapped to the current slot
                {
                    inventory.AddItem(selected.GetComponent<Image>().sprite, selectedInfo.item_id, prevSlot, selectedInfo.stack);
                }
                else if (inventory.item_id[selectedSlot] == selectedInfo.item_id)//The target slot has the same type of item
                {
                    if (inventory.stack[selectedSlot] + selectedInfo.stack <= inventory.maxStackDict[selectedInfo.item_id])//The item can be fully transferred
                    {
                        inventory.StackItem(selectedSlot, selectedInfo.stack);
                    }
                    else//There are too many items to transfer
                    {
                        inventory.AddItem(selected.GetComponent<Image>().sprite, selectedInfo.item_id, prevSlot, selectedInfo.stack + inventory.stack[selectedSlot] - inventory.maxStackDict[selectedInfo.item_id]);
                        inventory.StackItem(selectedSlot, inventory.maxStackDict[selectedInfo.item_id] - inventory.stack[selectedSlot]);

                    }
                    TextMeshProUGUI text = inventory.items[selectedSlot].GetComponentInChildren<TextMeshProUGUI>();
                    text.text = inventory.stack[selectedSlot].ToString();
                }
                else//The target slot has a different kind of item
                {
                    if (inventory.item_id[selectedSlot] != 0)
                    {
                        inventory.AddItem(inventory.items[selectedSlot].GetComponent<Image>().sprite, inventory.item_id[selectedSlot], prevSlot, inventory.stack[selectedSlot]);
                    }
                    inventory.AddItem(selected.GetComponent<Image>().sprite, selectedInfo.item_id, selectedSlot, selectedInfo.stack);
                }
            }
            else if (selectedSlot == -1)//The pointer is not in on a slot but still in menu, reset the transfer
            {
                inventory.AddItem(selected.GetComponent<Image>().sprite, selectedInfo.item_id, prevSlot, selectedInfo.stack);
            }
            else//The pointer is outside the menu so the items should be dropped
            {
                for (int i = 0; i < selectedInfo.stack; i++)
                {
                    GameObject droppedItem = Instantiate(inventory.pickUpPrefabs[selectedInfo.item_id], transform.position, Quaternion.identity);
                    droppedItem.name = selectedInfo.item_id.ToString();
                }
            }
            selected.SetActive(false);
        }
    }

    public void Split(InputAction.CallbackContext context)
    {
        if (context.performed && selected.activeInHierarchy)
        {
            int half = selectedInfo.stack / 2;
            if (half != 0)
            {
                inventory.AddItem(selected.GetComponent<Image>().sprite, selectedInfo.item_id, prevSlot, half);
                //inventory.items[prevSlot].GetComponent<Image>().sprite = selected.GetComponent<Image>().sprite;
                //inventory.item_id[prevSlot] = selectedInfo.item_id;
                //inventory.stack[prevSlot] = half;
                int emptySlot = inventory.FindAvailableSlot();
                if (emptySlot == -1)
                {
                    inventory.StackItem(prevSlot, selectedInfo.stack - half);
                }
                else
                {
                    inventory.AddItem(selected.GetComponent<Image>().sprite, selectedInfo.item_id, emptySlot, selectedInfo.stack - half);
                }
            }
            else
            {
                inventory.AddItem(selected.GetComponent<Image>().sprite, selectedInfo.item_id, prevSlot, selectedInfo.stack);
            }
            selected.SetActive(false);
        }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.ReadValue<float>() > 0.0f)
        {
            if (isGrounded && !isJumping)
            {
                rb.AddForce(new Vector2(0, 10f * jumpForce));
                //rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                isGrounded = false;
                isJumping = true;
            }

            if (isJumping && jumpTimeCounter > 0)
            {
                rb.AddForce(new Vector2(0, jumpForce));
                //rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                jumpTimeCounter -= Time.deltaTime;
            }
        }
        else
        {
            isJumping = false;
        }
    }

    void Landed()
    {
        isGrounded = true;
        jumpTimeCounter = maxJumpTime;
    }

    //Returns 'true' if we touched or hovering on Unity UI element.
    public int IsPointerOverSlot()
    {
        return IsPointerOverSlot(GetEventSystemRaycastResults());
    }


    //Returns 'true' if we touched or hovering on Unity UI element.
    private int IsPointerOverSlot(List<RaycastResult> eventSystemRaysastResults)
    {
        bool hitMenu = false;
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.layer == LayerMask.NameToLayer("Slot"))
            {
                return Convert.ToInt32(curRaysastResult.gameObject.name);
            }
            if (curRaysastResult.gameObject.layer == LayerMask.NameToLayer("InventoryMenu"))
            {
                hitMenu = true;
            }
        }
        return hitMenu ? -1 : -2;
    }


    //Gets all event system raycast results of current mouse or touch position.
    static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Pickup" && other.GetComponent<PickUpInfo>().recentlySpawned == false)
        {
            int target_id = other.GetComponent<PickUpInfo>().item_id;
            int target_index = inventory.FindStackSlot(target_id);
            if (target_index == -1)//Cannot stack
            {
                //add to an empty slot
                target_index = inventory.FindAvailableSlot();
                if (target_index != -1)
                {
                    inventory.AddItem(other.gameObject.GetComponent<SpriteRenderer>().sprite, target_id, target_index);
                    Destroy(other.gameObject);
                }
                else
                {
                    //Inventory is full
                }
            }
            else
            {
                //stack to an existing stack
                inventory.StackItem(target_index);
                Destroy(other.gameObject);
            }
        }
    }


    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Pickup")
        {
            other.GetComponent<PickUpInfo>().recentlySpawned = false;
        }
    }
}
