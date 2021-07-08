using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [System.NonSerialized]
    public float velocity;
    [System.NonSerialized]
    public float damage;
    [System.NonSerialized]
    public float shotOffset;
    [System.NonSerialized]
    public Vector3 shotPosOffset;

    [SerializeField]
    private Rigidbody2D rb;
    private Vector2 direction;

    private void Start()
    {
        //rb = gameObject.GetComponent<Rigidbody2D>();
        direction = transform.right;
        transform.position += shotPosOffset;
        rb.AddForce(direction.normalized * velocity, ForceMode2D.Impulse);

        //NOTE: Direction is a necessary variable
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<Player>().TakeDamage(damage);
            //ServerSend.DamagePlayer(collision.GetComponent<Player>().id, damage);

            //Instantiate(bloodParticles, collision.GetComponent<Collider2D>().ClosestPoint(transform.position), transform.rotation);
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Wall"))
        {
            //Instantiate(wallParticles, collision.GetComponent<Collider2D>().ClosestPoint(transform.position), transform.rotation);
            Destroy(gameObject);
        }
    }
}
