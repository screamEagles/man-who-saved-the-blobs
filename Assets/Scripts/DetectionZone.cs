using System.Collections.Generic;
using UnityEngine;

public class DetectionZone : MonoBehaviour
{
    public List<Collider2D> detectedColliders = new List<Collider2D>();
    Collider2D col;

    private GameObject player;



    private void Awake()
    {
        col = GetComponent<Collider2D>();

        Slimeny slimeny = GetComponentInParent<Slimeny>();
        if (slimeny != null)
        {
            player = slimeny.player;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            detectedColliders.Add(collision);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            detectedColliders.Remove(collision);
        }
    }
}
