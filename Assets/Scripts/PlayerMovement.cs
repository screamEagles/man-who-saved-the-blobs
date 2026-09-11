using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float attackCooldown = 0.1f; // Prevent spam

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;
    private float lastAttackTime;
    private bool isAttacking;
    private Vector2 lastDirection = Vector2.down; // Store last valid direction

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Only allow movement if not attacking
        if (!isAttacking)
        {
            rb.linearVelocity = moveInput * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero; // Stop movement during attack
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        // ALWAYS handle cancellation (key release) regardless of attacking state
        if (context.canceled)
        {
            moveInput = Vector2.zero;
            animator.SetBool("isWalking", false);

            // When keys are released, keep LastInputX/Y as the last direction
            // (Don't set them to zero - we want to remember the facing direction)

            // Only update InputX/Y if not attacking
            if (!isAttacking)
            {
                animator.SetFloat("InputX", 0);
                animator.SetFloat("InputY", 0);
            }
            return;
        }

        // Don't process new movement input if currently attacking
        if (isAttacking) return;

        moveInput = context.ReadValue<Vector2>();

        // Update animator parameters
        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);

        // Save the last valid direction when moving
        if (moveInput != Vector2.zero)
        {
            lastDirection = moveInput;
            animator.SetBool("isWalking", true);

            // Update LastInputX/Y with the current movement direction
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }
    }

    public void Attack(InputAction.CallbackContext context)
    {
        // Only trigger on press, not on hold or release
        if (!context.started) return;

        // Check cooldown
        if (Time.time - lastAttackTime < attackCooldown) return;

        // Don't attack if already attacking
        if (isAttacking) return;

        // Start attack
        isAttacking = true;
        lastAttackTime = Time.time;

        // Set attack animation parameters
        animator.SetBool("isAttacking", true);

        // Determine attack direction:
        // 1. If moving, use moveInput
        // 2. If standing still, use lastDirection (last valid movement direction)
        // 3. If no last direction, default to down
        Vector2 attackDirection = moveInput;
        if (attackDirection == Vector2.zero)
        {
            attackDirection = lastDirection;

            // If still zero (shouldn't happen with default), use down
            if (attackDirection == Vector2.zero)
            {
                attackDirection = Vector2.down;
            }
        }

        // Update animator with attack direction
        animator.SetFloat("InputX", attackDirection.x);
        animator.SetFloat("InputY", attackDirection.y);
    }

    // Called by Animation Event at the end of attack animation
    public void EndAttack()
    {
        isAttacking = false;
        animator.SetBool("isAttacking", false);

        // Restore movement input
        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);

        if (moveInput != Vector2.zero)
        {
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }
    }
}