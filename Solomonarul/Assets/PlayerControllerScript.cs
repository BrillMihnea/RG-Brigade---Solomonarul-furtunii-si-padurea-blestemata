using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public int extraJumps;
    private int jumpsLeft;

    public float glideFallSpeed;

    public bool isActiveDJ;
    public bool isActiveDash;
    public bool isActiveGlide;

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
        if (isActiveGlide && Input.GetKey(KeyCode.LeftShift) && rb.velocity.y < 0) rb.velocity = new Vector2(rb.velocity.x, -glideFallSpeed);

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
        yield return new WaitForSeconds(dashTime);
        rb.gravityScale = originalGravity;
        isDashing = false;
    }
    private void KillAndRespawn(float x, float y)
    {
        transform.position = new Vector3(x, y, 0);
    }
}