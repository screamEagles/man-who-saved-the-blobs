using System.Collections;
using UnityEngine;

public class Slimeny : MonoBehaviour
{
    public float playerTransparentTime = 4;
    // public DetectionZone biteZone;  // For attacking (kept as-is)
    Animator animator;
    Rigidbody2D rb;
    Transform playerTransform;
    Transform selfTransform;
    SpriteRenderer playerSprite;

    public GameObject player;
    public float speed = 2f;
    public float detectionRadius = 0.5f;

    [Header("Bite Zone (local space, +X = out of the mouth)")]
    [Tooltip("Centre of the bite rectangle, relative to Slimeny's pivot.")]
    [SerializeField] private Vector2 biteZoneOffset = new Vector2(0.6f, 0f);
    [Tooltip("Full width and height of the bite rectangle.")]
    [SerializeField] private Vector2 biteZoneSize = new Vector2(0.8f, 0.5f);

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 8f;
    [SerializeField] private float knockbackDuration = 0.2f;

    private static readonly int CanMoveHash = Animator.StringToHash("canMove");

    private PlayerMovement playerMovement;
    private Coroutine transparentRoutine;
    private float transparentUntil;

    public bool _hasTarget = false;

    public bool HasTarget
    {
        get { return _hasTarget; }
        private set
        {
            bool justAcquired = value && _hasTarget;

            _hasTarget = value;

            // animator.SetBool("hasTarget", value);

            if (justAcquired)
            {
                KnockbackPlayer();
                TurnTransparent();
            }
        }
    }

    public bool PlayerInBiteZone { get; private set; }

    public bool CanMove => animator == null || animator.GetBool(CanMoveHash);

    private void Awake()
    {
        selfTransform = transform;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        if (player != null)
            CachePlayer();
    }

    private void CachePlayer()
    {
        playerTransform = player.transform;
        playerMovement = player.GetComponent<PlayerMovement>();
        playerSprite = player.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (playerTransform == null) return;

        // --- Approach detection (squared distance, no sqrt) ---
        Vector2 mouthOrigin = selfTransform.TransformPoint(biteZoneOffset);
        Vector2 toPlayer = (Vector2)playerTransform.position - mouthOrigin;
        HasTarget = toPlayer.sqrMagnitude < detectionRadius * detectionRadius;

        // --- Bite detection (rectangle in front of the mouth) ---
        PlayerInBiteZone = CheckBiteZone();
    }

    private bool CheckBiteZone()
    {
        Vector3 local = selfTransform.InverseTransformPoint(playerTransform.position);

        return Mathf.Abs(local.x - biteZoneOffset.x) <= biteZoneSize.x * 0.5f
            && Mathf.Abs(local.y - biteZoneOffset.y) <= biteZoneSize.y * 0.5f;
    }

    void FixedUpdate()
    {
        if (playerTransform == null)
        {
            if (player != null) CachePlayer();
            else return;
        }

        Vector3 selfPos = selfTransform.position;
        Vector3 playerPos = playerTransform.position;

        // --- Flip sprite so it faces the player on the X axis ---
        float dx = playerPos.x - selfPos.x;
        if (Mathf.Abs(dx) > 0.01f)
        {
            Vector3 scale = selfTransform.localScale;
            scale.x = -(Mathf.Sign(dx) * Mathf.Abs(scale.x));
            selfTransform.localScale = scale;
        }

        if (CanMove)
        {
            Vector2 direction = ((Vector2)(playerPos - selfPos)).normalized;
            rb.MovePosition(rb.position + direction * (speed * Time.fixedDeltaTime));
        }
    }

    // Draw the bite zone in the Scene view so you can tune the numbers.
    private void OnDrawGizmosSelected()
    {
        Matrix4x4 old = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.color = new Color(1f, 0.25f, 0.25f, 0.35f);
        Gizmos.DrawCube(biteZoneOffset, biteZoneSize);

        Gizmos.color = new Color(1f, 0.25f, 0.25f, 1f);
        Gizmos.DrawWireCube(biteZoneOffset, biteZoneSize);

        Gizmos.matrix = old;
    }

    private void KnockbackPlayer()
    {
        if (playerMovement == null) return;

        Vector2 direction = playerTransform.position - selfTransform.position;
        playerMovement.ApplyKnockback(direction, knockbackForce, knockbackDuration);
    }

    // Extends the timer; only one coroutine ever runs.
    private void TurnTransparent()
    {
        transparentUntil = Time.unscaledTime + playerTransparentTime;
        if (transparentRoutine == null)
            transparentRoutine = StartCoroutine(TransparentRoutine());
    }

    private IEnumerator TransparentRoutine()
    {
        SetPlayerAlpha(0.5f);

        while (Time.unscaledTime < transparentUntil)
            yield return null;

        SetPlayerAlpha(1f);
        transparentRoutine = null;
    }

    private void SetPlayerAlpha(float a)
    {
        if (playerSprite == null) return;
        Color c = playerSprite.color;
        c.a = a;
        playerSprite.color = c;
    }
}