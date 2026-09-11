using UnityEngine;

public class Slimeny : MonoBehaviour
{
    public DetectionZone biteZone;  // For attacking (kept as-is)
    Animator animator;
    Rigidbody2D rb;
    Transform playerTransform;

    public GameObject player;
    public float speed = 2f;
    public float detectionRadius = 5f;  // How far the enemy can see the player

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

    public bool CanMove
    {
        get
        {
            try
            {
                return animator.GetBool("canMove");
            }
            catch
            {
                return true;
            }
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
        // Distance-based detection for approaching
        if (playerTransform != null)
        {
            float distance = Vector2.Distance(transform.position, playerTransform.position);
            HasTarget = distance < detectionRadius; // going to bite the player based on this
                                                    // I don't want a radius, I want rectangle coming out of the Slimeny's mouth which will be resized on playtest.
                                                    // by Distance, the player could be next to the Slimeny's mouth, behind Slimeny, or up/down the Slimeny. What I want is only to be next to the Slimeny's mouth.

            // I am thinking about using X-axis only.
            // I also think that if I somehow only modify the x-axis, the behind will be considered at a commensurate rate.
        }

        // The biteZone is still used for attacking logic elsewhere
    }

    void FixedUpdate()
    {
        if (playerTransform == null)
        {
            if (player != null)
                playerTransform = player.transform;
            else
                return;
        }

        if (CanMove)
        {
            // monving towards the player based on this
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            Vector2 targetPosition = rb.position + direction * speed * Time.fixedDeltaTime;
            rb.MovePosition(targetPosition);
        }
    }
}