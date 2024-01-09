using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] GameObject describer;
    [SerializeField] GameObject headSlot;
    [SerializeField] GameObject waistSlot;
    [SerializeField] GameObject leftHandSlot;
    [SerializeField] GameObject rightHandSlot;
    [SerializeField] GameObject leftFootSlot;
    [SerializeField] GameObject rightFootSlot;

    [SerializeField] private InventoryInfo inventory;
    private GameObject inventoryMenu;

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
    }

    void Start()
    {
        inventoryMenu = inventory.transform.parent.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
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

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.ReadValue<float>()>0.0f)
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Pickup")
        {
            int target_id = Convert.ToInt32(other.name);
            int target_index = inventory.FindStackSlot(target_id);
            if (target_index == -1)//Cannot stack
            {
                //add to an empty slot
                target_index = inventory.FindAvailableSlot();
                if (target_index != -1)
                {
                    inventory.AddItem(other.gameObject, target_id, target_index);
                }
                else
                {
                    //Inventory is full
                }
            }
            else
            {
                //stack to an existing stack
                inventory.StackItem(other.gameObject, target_index);
            }
        }
    }
}
