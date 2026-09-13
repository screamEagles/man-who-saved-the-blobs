using UnityEngine;

public class Slimeny : MonoBehaviour
{
    public DetectionZone biteZone;  // For attacking (kept as-is)
    Animator animator;
    Rigidbody2D rb;
    Transform playerTransform;

    public GameObject player;
    public float speed = 2f;
    public float detectionRadius = 5f;

    [Header("Bite Zone (local space, +X = out of the mouth)")]
    [Tooltip("Centre of the bite rectangle, relative to Slimeny's pivot.")]
    [SerializeField] private Vector2 biteZoneOffset = new Vector2(0.6f, 0f);
    [Tooltip("Full width and height of the bite rectangle.")]
    [SerializeField] private Vector2 biteZoneSize = new Vector2(0.8f, 0.5f);

    public bool _hasTarget = false;

    public bool HasTarget
    {
        get { return _hasTarget; }
        private set
        {
            _hasTarget = value;
            animator.SetBool("hasTarget", value);
        }
    }

    public bool PlayerInBiteZone { get; private set; }

    public bool CanMove
    {
        get
        {
            try { return animator.GetBool("canMove"); }
            catch { return true; }
        }
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        if (player != null)
            playerTransform = player.transform;
    }

    void Update()
    {
        if (playerTransform == null) return;

        // --- Approach detection (still a radius, that's fine) ---
        Vector2 mouthOrigin = transform.TransformPoint(biteZoneOffset);
        float distance = Vector2.Distance(mouthOrigin, playerTransform.position);
        HasTarget = distance < detectionRadius;

        // --- Bite detection (rectangle in front of the mouth) ---
        PlayerInBiteZone = CheckBiteZone();
    }

    private bool CheckBiteZone()
    {
        // Convert the player's world position into Slimeny's local space.
        // After flipping (localScale.x = -1), local +X is still "forward",
        // so the box always comes out of the mouth.
        Vector3 local = transform.InverseTransformPoint(playerTransform.position);

        float halfW = biteZoneSize.x * 0.5f;
        float halfH = biteZoneSize.y * 0.5f;

        bool insideX = Mathf.Abs(local.x - biteZoneOffset.x) <= halfW;
        bool insideY = Mathf.Abs(local.y - biteZoneOffset.y) <= halfH;

        return insideX && insideY;
    }

    void FixedUpdate()
    {
        if (playerTransform == null)
        {
            if (player != null) playerTransform = player.transform;
            else return;
        }

        // --- Flip sprite so it faces the player on the X axis ---
        float dx = playerTransform.position.x - transform.position.x;
        if (Mathf.Abs(dx) > 0.01f)
        {
            Vector3 scale = transform.localScale;
            scale.x = -(Mathf.Sign(dx) * Mathf.Abs(scale.x));
            transform.localScale = scale;
        }

        if (CanMove)
        {
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            Vector2 targetPosition = rb.position + direction * speed * Time.fixedDeltaTime;
            rb.MovePosition(targetPosition);
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
}