using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GolemController : EnemyController
{
    [Header("Skill Settings")]

    public float kickForce;

    public GameObject rockPrefab;
    public Transform hand;


    void KickOff()//Anmination Event
    {
        if (attackTarget != null && transform.isFacingTarget(attackTarget.transform))
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();

            Vector3 direction = attackTarget.transform.position - transform.position;
            direction.Normalize();

            targetStats.GetComponent<NavMeshAgent>().isStopped = true;
            targetStats.GetComponent<NavMeshAgent>().velocity = direction * kickForce;

            targetStats.GetComponent<Animator>().SetTrigger("Dizzy");//»÷ÔÎ¶¯»­
            //targetStats.GetComponent<NavMeshAgent>().destination = targetStats.transform.position;//Í£Ö¹ÒÆ¶¯
            targetStats.TakeDamage(characterStats, targetStats);
        }
    }

    void ThrowRock()//Anmination Event
    {
        if (attackTarget != null)
        {
            var rock = Instantiate(rockPrefab, hand.position, Quaternion.identity).GetComponent<Rigidbody>();
            //var targetStats = attackTarget.GetComponent<CharaterStats>();
            //Vector3 direction = attackTarget.transform.position - transform.position + Vector3.up;
            //rock.velocity = direction.normalized * 10;
            rock.GetComponent<RockController>().target = attackTarget;
        }

    }
}
