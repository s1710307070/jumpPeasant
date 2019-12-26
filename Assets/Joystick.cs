using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Joystick : MonoBehaviour
{
    public Animator animator;

    public float speed = 7.0f;
    public float charge = 100.0f;

    [Range(0, .3f)] [SerializeField] private float movementSmoothing = .1f;   // How much to smooth out the movement
    [SerializeField] private float jumpForce = 1000f;                           // Amount of force added when the player jumps.

    [SerializeField] private Transform groundCheck;                           // A position marking where to check if the player is grounded.
    [SerializeField] private Transform ceilingCheck;                            // A position marking where to check for ceilings
    [SerializeField] private LayerMask groundLayer;                            // A mask determining what is ground to the character

    const float groundedRadius = .2f; // Radius of the overlap circle to determine if grounded
    const float ceilingRadius = .2f; // Radius of the overlap circle to determine if the player can stand up

    private Vector2 lastPosition;

    private Vector3 velocity = Vector3.zero;

    private bool touchStart = false;
    private bool isGrounded = true;
    private bool isJumping = false;
    private bool isCharging = false;
    private bool isFalling = false;

    private Vector2 pointA;
    private Vector2 pointB;

    private bool facingRight = true;

    private Rigidbody2D rigidBody;

    public Transform circle;
    public Transform outerCircle;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        bool wasGrounded = isGrounded;
        isGrounded = false;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, 0.6f, groundLayer);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                isGrounded = true;
            }
        }


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
                if (charge < 500) charge += 10;
                animator.SetBool("IsCrouching", true);
            }
        }
        else if (wasGrounded)
        {
            isFalling = true;
            animator.SetBool("IsFalling", false);
        }


        if (touchStart)
        {
            Vector2 offset = pointB - pointA;
            Vector2 direction = Vector2.ClampMagnitude(offset, 1.0f);
            moveCharacter(direction);
            circle.transform.position = new Vector2(pointA.x + direction.x, pointA.y + direction.y);

        }
        else
        {
            circle.GetComponent<SpriteRenderer>().enabled = false;
            outerCircle.GetComponent<SpriteRenderer>().enabled = false;
            moveCharacter(new Vector2(0f, 0f));
        }

    }

    void moveCharacter(Vector2 direction)
    {
        //player.Translate(direction * speed * Time.deltaTime);
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
            rigidBody.AddForce(new Vector2(direction.x * 8000f, jumpForce * (charge / 50)));
            charge = 100;
            isJumping = false;
            isCharging = false;
            isGrounded = false;
            isFalling = true;
            animator.SetBool("IsJumping", true);
        }
        else
        {
            float xmultiplier = 5f;
            float yAddition = 0f;
            if (isFalling)
            {
                animator.SetBool("IsFalling", true);
                yAddition = 20f;
                xmultiplier *= 2f;
            }

            targetVelocity = new Vector2(direction.x * xmultiplier, direction.y -= yAddition);
            rigidBody.velocity = Vector3.SmoothDamp(rigidBody.velocity, targetVelocity, ref velocity, movementSmoothing);
            animator.SetFloat("Speed", Mathf.Abs(direction.x));
            animator.SetBool("IsCrouching", false);
        }


    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            pointA = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z));

            circle.transform.position = pointA;
            outerCircle.transform.position = pointA;
            circle.GetComponent<SpriteRenderer>().enabled = true;
            outerCircle.GetComponent<SpriteRenderer>().enabled = true;
        }
        if (Input.GetMouseButton(0))
        {
            touchStart = true;
            pointB = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z));
        }
        else
        {
            touchStart = false;
        }

        if (isGrounded)
        {
            if (Input.GetKey(KeyCode.W))
            {
                isCharging = true;
            }

            if (Input.GetButtonUp("Jump"))
            {
                isJumping = true;
                isCharging = false;
            }
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



