using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Reflection;


public class PlayerController : MonoBehaviour
{
    private NavMeshAgent agent;
    private GameObject attackTarget;
    private float lastAttackTime;
    private PlayerStats characterStats;

    private bool isDead = false;

    private float stoppoingDisance;
    private float speed;

    public Animator animator;

    public GameObject humanForm;
    public GameObject yokaiForm;
    public GameObject invisiableForm;


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        characterStats = GetComponent<PlayerStats>();

        stoppoingDisance = agent.stoppingDistance;
        speed = agent.speed;
    }

    private void OnEnable()
    {
        GameManager.Instance.RigisterPlayer(characterStats);

        MouseManager.Instance.OnMouseCliked += MoveTo;//在MouseManager的OnMouseCliked事件中添加方法
        MouseManager.Instance.OnEnemyClicked += EventAttack;
    }

    private void OnDisable()
    {
        if (MouseManager.IsInitialized)
        {
            MouseManager.Instance.OnMouseCliked -= MoveTo;
            MouseManager.Instance.OnEnemyClicked -= EventAttack;
        }
    }


    private void Update()
    {

        isDead = characterStats.CurrentHealth <= 0;
        if (isDead)
        {
            GameManager.Instance.NotifyObservers();
            GameOverManager.Instance.ShowGameOver(false);
        }

        if (Input.GetKey(KeyCode.V) && GameManager.Instance.playerStats.EagleVision)//鹰眼视觉
        {
            Debug.Log("按下 V 键");

            foreach(var enemy in FindObjectsOfType<EnemyController>())
            {
                enemy.gameObject.GetComponent<Outline>().enabled = true;
            }
            
        }

        if (Input.GetKeyUp(KeyCode.V))
        {
            Debug.Log("松开 V 键");

            foreach (var enemy in FindObjectsOfType<EnemyController>())
            {
                enemy.gameObject.GetComponent<Outline>().enabled = false;
            }

        }


        SwitchAnimation();

        lastAttackTime -= Time.deltaTime;
    }

    public void ShapeShift(string formName)
    {
        
        switch (formName)
        {
            case "隐形":
                GameManager.Instance.playerStats.currentShape = PlayerStats.PlayerShape.Invisiable;

                Instantiate(invisiableForm, gameObject.transform);
                if (gameObject.transform.childCount > 3)
                {
                    Destroy(gameObject.transform.GetChild(gameObject.transform.childCount - 2).gameObject);
                }
                animator.avatar = invisiableForm.gameObject.GetComponent<ShapeShiftController>().avatar;

                StartCoroutine(RestartAnimator());
                //使用协程延迟一帧后再重新启用 Animator

                //animator.enabled = false;
                //animator.enabled = true;
                //在代码中简单地切换 animator 的 enabled 状态
                //不会像手动操作那样彻底重置动画状态机，
                //因为内部状态没有完全重置

                break;

            case "兽人":
                GameManager.Instance.playerStats.currentShape = PlayerStats.PlayerShape.Yokai;

                Instantiate(yokaiForm, gameObject.transform);
                if (gameObject.transform.childCount > 3)
                {
                    Destroy(gameObject.transform.GetChild(gameObject.transform.childCount - 2).gameObject);
                }
                animator.avatar = yokaiForm.gameObject.GetComponent<ShapeShiftController>().avatar;

                StartCoroutine(RestartAnimator());


                break;

            case "人形":
                GameManager.Instance.playerStats.currentShape = PlayerStats.PlayerShape.Human;

                Instantiate(humanForm, gameObject.transform);
                if (gameObject.transform.childCount > 3)//如果子对象数目大于3，说明是切换形态，则要删除旧形态
                {
                    Destroy(gameObject.transform.GetChild(gameObject.transform.childCount - 2).gameObject);
                }
                //Destroy(gameObject.transform.GetChild(gameObject.transform.childCount - 2).gameObject);
                
                animator.avatar = humanForm.gameObject.GetComponent<ShapeShiftController>().avatar;

                StartCoroutine(RestartAnimator());


                break;
        }
        InventoryManager.Instance.UpdateEquipmentSlot(GameManager.Instance.playerStats.currentShape);



    }
    IEnumerator RestartAnimator()
    {
        animator.enabled = false;
        yield return null; // 等待一帧
        animator.Rebind();
        animator.Update(0f);
        animator.enabled = true;
    }

    public void SwitchAnimation()
    {
        animator.SetFloat("Speed", agent.velocity.sqrMagnitude);
        animator.SetBool("Death", isDead);

    }

    void MoveTo(Vector3 destination)//移动到鼠标点击的位置
    {
        StopAllCoroutines();

        if (isDead) return;
        if (GameManager.Instance.playerStats.weaponData == null)
        {
            agent.stoppingDistance = stoppoingDisance;
            agent.speed = speed;
        }
        else
        {
            agent.stoppingDistance = GameManager.Instance.playerStats.AttackRange;
            if (GameManager.Instance.playerStats.shieldData == null)
                agent.speed = MathF.Max(8.5f,
                    speed - GameManager.Instance.playerStats.weaponData.weight);
            else
                agent.speed = MathF.Max(8.5f,
                                        speed 
                                        - GameManager.Instance.playerStats.weaponData.weight
                                        - GameManager.Instance.playerStats.shieldData.weight);
        }

        agent.isStopped = false;
        agent.SetDestination(destination);
    }

    void EventAttack(GameObject target)//攻击敌人
    {

        if (isDead) return;
        if (target != null)
        {
            attackTarget = target;

            characterStats.isCritical = 
                UnityEngine.Random.value < characterStats.CriticalChance;
            StartCoroutine(MoveToAttackTarget());
        }
    }
    IEnumerator MoveToAttackTarget()
    {
        agent.isStopped = false;
        transform.LookAt(attackTarget.transform);

        agent.stoppingDistance = characterStats.AttackRange;

        while (Vector3.Distance(attackTarget.transform.position, transform.position) 
            > characterStats.AttackRange)//玩家和攻击对象的距离和攻击距离的比较
        {
            agent.destination = attackTarget.transform.position;
            yield return null;
        }

        agent.isStopped = true;

        if (lastAttackTime < 0)
        {
            animator.SetTrigger("Attack");
            animator.SetBool("Critical", characterStats.isCritical);

            lastAttackTime = characterStats.CoolDown;
        }
    }

    void Hit()
    {
        if (attackTarget.CompareTag("Attackable"))
        {
            if (attackTarget.GetComponent<RockController>()
               /* && attackTarget.GetComponent<rockController>().state == rockController.states.HitNothing*/)
            //如果不是hitnothing，则在空中也可以反击
            {

                attackTarget.GetComponent<RockController>().state = RockController.states.HitEnemy;
                attackTarget.GetComponent<Rigidbody>().velocity = Vector3.one;
                attackTarget.GetComponent<Rigidbody>().AddForce(
                    (transform.forward).normalized
                    * attackTarget.GetComponent<RockController>().force,
                    ForceMode.Impulse);
            }
        }
        else
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            targetStats.TakeDamage(characterStats, targetStats);
        }
    }

}
