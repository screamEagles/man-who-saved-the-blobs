using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float attackCooldown = 0.1f; // Prevent spam

    // Cached animator parameter hashes (avoids string hashing on every call)
    // they are in the AnimationStrings file

    private bool isKnockedBack;
    private float knockbackEndTime;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;
    private float lastAttackTime;
    private bool isAttacking;
    private Vector2 lastDirection = Vector2.down; // Store last valid direction

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Let physics own the velocity while being knocked back
        if (isKnockedBack)
        {
            if (Time.time >= knockbackEndTime)
                isKnockedBack = false;
            else
                return;
        }

        // Stop movement during attack, otherwise follow input
        rb.linearVelocity = isAttacking ? Vector2.zero : moveInput * moveSpeed;
    }

    public void Move(InputAction.CallbackContext context)
    {
        // ALWAYS handle cancellation (key release) regardless of attacking state
        if (context.canceled)
        {
            moveInput = Vector2.zero;
            animator.SetBool(AnimationStrings.IsWalking, false);

            // Keep LastInputX/Y so we remember the facing direction.
            // Only update InputX/Y if not attacking.
            if (!isAttacking)
            {
                animator.SetFloat(AnimationStrings.InputX, 0f);
                animator.SetFloat(AnimationStrings.InputY, 0f);
            }
            return;
        }

        // Don't process new movement input if currently attacking
        if (isAttacking) return;

        moveInput = context.ReadValue<Vector2>();

        animator.SetFloat(AnimationStrings.InputX, moveInput.x);
        animator.SetFloat(AnimationStrings.InputY, moveInput.y);

        if (moveInput != Vector2.zero)
        {
            lastDirection = moveInput;
            animator.SetBool(AnimationStrings.IsWalking, true);
            animator.SetFloat(AnimationStrings.LastInputX, moveInput.x);
            animator.SetFloat(AnimationStrings.LastInputY, moveInput.y);
        }
        else
        {
            animator.SetBool(AnimationStrings.IsWalking, false);
        }
    }

    public void Attack(InputAction.CallbackContext context)
    {
        // Only trigger on press, not on hold or release
        if (!context.started) return;

        // Don't attack if already attacking, or if still on cooldown
        if (isAttacking) return;
        float now = Time.time;
        if (now - lastAttackTime < attackCooldown) return;

        isAttacking = true;
        lastAttackTime = now;

        animator.SetBool(AnimationStrings.IsAttacking, true);

        // Moving -> use moveInput; standing still -> use lastDirection
        // (lastDirection starts as Vector2.down and is only ever set to a non-zero value)
        Vector2 attackDirection = moveInput != Vector2.zero ? moveInput : lastDirection;

        animator.SetFloat(AnimationStrings.InputX, attackDirection.x);
        animator.SetFloat(AnimationStrings.InputY, attackDirection.y);
    }

    // Called by Animation Event at the end of attack animation
    public void EndAttack()
    {
        isAttacking = false;
        animator.SetBool(AnimationStrings.IsAttacking, false);

        // Restore movement input
        animator.SetFloat(AnimationStrings.InputX, moveInput.x);
        animator.SetFloat(AnimationStrings.InputY, moveInput.y);
        animator.SetBool(AnimationStrings.IsWalking, moveInput != Vector2.zero);
    }

    public void ApplyKnockback(Vector2 direction, float force, float duration)
    {
        isKnockedBack = true;
        knockbackEndTime = Time.time + duration;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
    }
}