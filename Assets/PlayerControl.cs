using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public Animator animator;
    public Joystick joystick;
    public Slider slider;

    public float speed = 1.2f;
    public float charge = 100.0f;

    [Range(0, .3f)] [SerializeField] private float movementSmoothing = .1f;
    [SerializeField] private float jumpForce = 1000f;

    [SerializeField] private BoxCollider2D groundCollider;
    [SerializeField] private LayerMask groundLayer;

    const float groundedRadius = .2f;

    private Vector2 lastPosition;

    private Vector3 velocity = Vector3.zero;

    private float horizontalMovement = 0f;
    private float gravityDrag = 4f;

    private ContactFilter2D filter;

    private bool isGrounded = true;
    private bool isJumping = false;
    private bool isCharging = false;
    private bool isFalling = false;

    private Vector2 pointA;
    private Vector2 pointB;

    private bool facingRight = true;

    private Rigidbody2D rigidBody;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        filter = new ContactFilter2D();
        filter.SetLayerMask(groundLayer);
        Flip();
    }

    private void FixedUpdate()
    {
        bool wasGrounded = isGrounded;
        isGrounded = IsPlayerGrounded();
        slider.value = charge;
        
        if (isGrounded)
        {
            gravityDrag = 4f;
            if (!wasGrounded)
            {
                isFalling = false;
                animator.SetBool("IsJumping", false);
                animator.SetBool("IsFalling", false);
            }

            if (isCharging)
            {
                if (charge < 500) charge += 5;
                animator.SetBool("IsCrouching", true);
            }
        }
        else
        {
            isFalling = true;
            animator.SetBool("IsFalling", false);
        }


        Vector2 offset = new Vector2(horizontalMovement, 0f);
        Vector2 direction = Vector2.ClampMagnitude(offset, 1.0f);
        moveCharacter(direction);


    }

    void moveCharacter(Vector2 direction)
    {
        Vector2 targetVelocity;


        if (direction.x > 0 && !facingRight)
        {
            Flip();
        }
        else if (direction.x < 0 && facingRight)
        {
            Flip();
        }

        if (isCharging)
        {
            targetVelocity = new Vector2(direction.x * 0f, direction.y);
            rigidBody.velocity = Vector3.SmoothDamp(rigidBody.velocity, targetVelocity, ref velocity, movementSmoothing);
            animator.SetBool("IsCrouching", true);
            animator.SetBool("IsFalling", false);
            animator.SetBool("IsJumping", false);
            isJumping = false;
            isGrounded = true;
            isFalling = false;
        }
        else if (isJumping)
        {
            float value = jumpForce * (charge / 180) + 80;
            rigidBody.AddForce(new Vector2(direction.x * value * speed, value));
            charge = 100;
            isJumping = false;
            isCharging = false;
            isGrounded = false;
            isFalling = true;
            animator.SetBool("IsJumping", true);
            animator.SetBool("IsFalling", true);
        }
        else
        {
            float xmultiplier = 5f;
            if (isFalling)
            {
                animator.SetBool("IsFalling", true);
                if (gravityDrag < 16f) gravityDrag += 0.2f;
                xmultiplier *= 2.5f;
            }

            targetVelocity = new Vector2(direction.x * xmultiplier * speed, direction.y -= gravityDrag);
            rigidBody.velocity = Vector3.SmoothDamp(rigidBody.velocity, targetVelocity, ref velocity, movementSmoothing);
            animator.SetFloat("Speed", Mathf.Abs(direction.x));
            animator.SetBool("IsCrouching", false);
        }


    }

    // Update is called once per frame
    void Update()
    {

        horizontalMovement = joystick.Horizontal;

        if (Input.GetButtonDown("Jump")) Charge();
        if (Input.GetButtonUp("Jump")) Jump();

    }

    public bool IsPlayerGrounded()
    {
        RaycastHit2D[] hits = new RaycastHit2D[10];

        int numberOfHits = groundCollider.Cast(Vector2.down, filter, hits, 0.1f);

        for (int i = 0; i < numberOfHits; i++)
        {
            if (!hits[i].collider.isTrigger) return true;
        }

        return false;
    }

    public void Charge()
    {
        if (isGrounded) isCharging = true;
    }

    public void Jump()
    {
        if (isGrounded)
        {
            isJumping = true;
            isCharging = false;
        }
    }


    private void Flip()
    {
        facingRight = !facingRight;

        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}



