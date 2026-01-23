using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RockController : MonoBehaviour
{
    public enum states { HitPlayer, HitGround, HitEnemy, HitNothing }
    public states state;

    private Rigidbody rb;

    [Header("Basic Settings")]

    public float force;
    public int damage;
    public GameObject target;
    public GameObject breakEffect;

    private Vector3 direction;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.one;
        FlyToTarget();

        state = states.HitPlayer;
    }

    private void FixedUpdate()
    {
        //if(rb.velocity.sqrMagnitude < 1f)
        //{
        //    state = states.HitNothing;
        //}
    }

    public void FlyToTarget()
    {
        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player");
        }
        direction = (target.transform.position - transform.position + Vector3.up).normalized;

        rb.AddForce(direction * force, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            state = states.HitNothing;
        }

        switch (state)
        {
            case states.HitPlayer:
                if (collision.gameObject.tag == "Player")
                {
                    collision.gameObject.GetComponent<NavMeshAgent>().isStopped = true;
                    collision.gameObject.GetComponent<NavMeshAgent>().velocity = direction * force;
                    //Debug.Log(direction);
                    collision.gameObject.GetComponent<Animator>().SetTrigger("Dizzy");

                    collision.gameObject.GetComponent<CharacterStats>().TakeDamage(damage,
                        collision.gameObject.GetComponent<CharacterStats>());
                    Instantiate(breakEffect, transform.position, Quaternion.identity);
                    Destroy(gameObject);
                    state = states.HitNothing;
                }
                break;

            case states.HitEnemy:
                if (collision.gameObject.GetComponent<GolemController>())
                {
                    var enemyStats = collision.gameObject.GetComponent<CharacterStats>();
                    enemyStats.TakeDamage(damage, enemyStats);
                    Instantiate(breakEffect, transform.position, Quaternion.identity);

                    Destroy(gameObject);
                }
                break;
        }
    }
}
