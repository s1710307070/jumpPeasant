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

    [Range(0, .3f)] [SerializeField] private float movementSmoothing = .1f;   // How much to smooth out the movement
    [SerializeField] private float jumpForce = 2000f;                           // Amount of force added when the player jumps.

    //[SerializeField] private Transform groundCheck;                           // A position marking where to check if the player is grounded.
    [SerializeField] private BoxCollider2D groundCollider;
    //[SerializeField] private Transform ceilingCheck;                            // A position marking where to check for ceilings
    [SerializeField] private LayerMask groundLayer;                            // A mask determining what is ground to the character

    const float groundedRadius = .2f; // Radius of the overlap circle to determine if grounded
    //const float ceilingRadius = .2f; // Radius of the overlap circle to determine if the player can stand up

    private Vector2 lastPosition;

    private Vector3 velocity = Vector3.zero;

    private float horizontalMovement = 0f;

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
    }

    private void FixedUpdate()
    {
        bool wasGrounded = isGrounded;
        isGrounded = false;
        slider.value = charge;
        
        RaycastHit2D raycastHit = Physics2D.Raycast(groundCollider.bounds.center, Vector2.down, groundCollider.bounds.extents.y + 0.01f, groundLayer);
        Color rayColor;
        if (raycastHit.collider != null)
        {
            rayColor = Color.green;
            isGrounded = true;
        }
        else
        {
            rayColor = Color.red;
        }
        Debug.DrawRay(groundCollider.bounds.center, Vector2.down * (groundCollider.bounds.extents.y + 0.01f));
        

        if (isGrounded)
        {
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
        }
        else if (isJumping)
        {
            float value = jumpForce * (charge / 100);
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
            float yAddition = 0f;
            if (isFalling)
            {
                animator.SetBool("IsFalling", true);
                yAddition = 12f;
                xmultiplier *= 2f;
            }

            targetVelocity = new Vector2(direction.x * xmultiplier * speed, direction.y -= yAddition);
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
        // Switch the way the player is labelled as facing.
        facingRight = !facingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}



