
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerControllerScript : MonoBehaviour
{
    //---------------------------------------VARIABILE(IN PRINCIPAL) DE MOVEMENT-------------------------------------

    public float moveSpeed;
    private float moveInput;
    private bool facingRight = true;

    public float jumpHeight;

    public Rigidbody2D rb;
    public Transform groundCheck;
    public float checkRadius;
    public LayerMask whatIsGround;

    public float maxFallSpeed;
    public float jumpGravity;
    public float fallGravity;
    public float buttonPressWindow;
    private float buttonPressedTime = 0;
    private bool isJumping;

    public float coyoteTime;
    private float coyoteTimeCounter;

    public float jumpBufferTime;
    private float jumpBufferCounter;

    private bool canDash = true;
    private bool isDashing = false;
    public float dashSpeed;
    public float dashTime;
    public float dashCooldown;
    private float dashCooldownCounter;
    public TrailRenderer tr;

    public int extraJumps;
    private int jumpsLeft;

    public float glideFallSpeed;
    private bool isGliding;

    public bool isActiveDJ;
    public bool isActiveDash;
    public bool isActiveGlide;

    public ParticleSystem LandingParticles;
    public Transform RespawnPoint;

    private void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex >= 6) isActiveDJ = true;
        else isActiveDJ = false;
        if (SceneManager.GetActiveScene().buildIndex >= 11) isActiveDash = true;
        else isActiveDash = false;
        isActiveGlide = false;
    }

    private void FixedUpdate()
    {
        if (isDashing) return;

        //------------------------------------------------MOVEMENT----------------------------------------
        moveInput = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        if (facingRight == false && moveInput > 0 || facingRight == true && moveInput < 0) Flip();
    }

    private void Update()
    {
        if (isDashing) return;

        //----------------------------------JUMP----------------------------------
        if (isGrouded()) coyoteTimeCounter = coyoteTime;
        else coyoteTimeCounter -= Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Space)) jumpBufferCounter = jumpBufferTime;
        else jumpBufferCounter -= Time.deltaTime;

        if (coyoteTimeCounter > 0) jumpsLeft = extraJumps;

        if (jumpsLeft > 0 && isActiveDJ || coyoteTimeCounter > 0)
        {
            if (jumpBufferCounter > 0)
            {
                rb.gravityScale = jumpGravity;
                float jumpVelocity = Mathf.Sqrt((Physics2D.gravity.y * rb.gravityScale) * -2 * jumpHeight);
                rb.velocity = new Vector2(rb.velocity.y, jumpVelocity);
                buttonPressedTime = 0;
                isJumping = true;
                jumpBufferCounter = 0;
                jumpsLeft--;
            }
        }

        //------------------------------VARIABLE JUMP HEIGHT && 2 TYPES OF GRAVITY-------------------------
        if (isJumping)
        {
            buttonPressedTime += Time.deltaTime;
            if (buttonPressedTime < buttonPressWindow && Input.GetKeyUp(KeyCode.Space))
            {
                rb.gravityScale = fallGravity;
                isJumping = false;
                buttonPressedTime = 0;
                coyoteTimeCounter = 0;
            }
            if (rb.velocity.y < 0)
            {
                rb.gravityScale = fallGravity;
                isJumping = false;
            }
        }

        //---------------------------------------------DASH----------------------------------------
        if (isActiveDash && Input.GetKeyDown(KeyCode.LeftControl) && canDash) StartCoroutine(Dash());
        if (coyoteTimeCounter > 0 && dashCooldownCounter <= 0)
        {
            dashCooldownCounter = dashCooldown;
            canDash = true;
        }
        else dashCooldownCounter -= Time.deltaTime;

        //------------------------------------GLIDE-----------------------------------------------
        if (isActiveGlide && Input.GetKey(KeyCode.LeftShift) && rb.velocity.y < 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, -glideFallSpeed);
            isGliding = true;
        }
        else isGliding = false;

        //---------------------------------------MAX FALL SPEED------------------------------------
        if (rb.velocity.y < -maxFallSpeed) rb.velocity = new Vector2(rb.velocity.x, -maxFallSpeed);
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 Scaler = transform.localScale;
        Scaler.x *= -1;
        transform.localScale = Scaler;
    }

    bool isGrouded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;
        if (facingRight) rb.velocity = new Vector2(dashSpeed, 0);
        else rb.velocity = new Vector2(-dashSpeed, 0);
        tr.emitting = true;
        yield return new WaitForSeconds(dashTime);
        rb.gravityScale = originalGravity;
        isDashing = false;
        tr.emitting = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isGrouded() && !isGliding) Instantiate(LandingParticles, groundCheck.position, groundCheck.rotation);
        if (collision.collider.tag == "Enemy")
        {
            rb.velocity = Vector2.zero;
            transform.position = RespawnPoint.position;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Complete") SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        gameObject.transform.position = new Vector3(-15, -6, 1);
    }
}