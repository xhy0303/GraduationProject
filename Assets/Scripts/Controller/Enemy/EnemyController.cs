using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyStates
{
    Guard, Patrol, Chase, Alart, Search, Dead
}



[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(EnemyStats))]
[RequireComponent(typeof(HealthBarUI))]

public class EnemyController : MonoBehaviour, IEndGameObserver
{
    private NavMeshAgent agent;
    private Animator animator;
    private Collider collider;
    protected EnemyStats characterStats;
    public EnemyStates enemyStates;

    [Header("Basic Settings")]
    public float sightRange;
    public float viewAngle = 90f; // 可视角度范围（面前左右各一半的角度）
    public bool isGuard;

    protected GameObject attackTarget;

    public void SetAttackTarget(GameObject attackTarget)
    {
        this.attackTarget = attackTarget;
    }

    private float speed; //移动速度

    public float alertDuration = 3f; // 观察持续时间
    private float alertTimer;         // 观察倒计时

    [Header("Patrol Settings")]
    //public float patrolRange;
    public float patrolTime;

    public GameObject patrolRount;
    public List<GameObject> patrolStops;
    int patrolStopIndex;

    private float remainPatrolTime;
    private float lastAttackTime;

    private Vector3 wayPos;
    private Vector3 guardPos;
    private Quaternion guardRot;

    [Header("Search Settings")]
    private Vector3 lastKnownPlayerPos;
    private Vector3 currentSearchTarget;
    public float searchRadius = 10f; // 可根据需求调整
    public float searchDuration = 6f; // 搜索状态持续时间
    private float searchTimer;


    //判断动画
    bool isWalk;
    bool isChase;
    bool isFollow;
    public bool isDead { get; private set; }

    bool playerDead = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        speed = agent.speed;
        animator = GetComponent<Animator>();
        collider = GetComponent<Collider>();

        guardPos = transform.position;
        guardRot = transform.rotation;

        remainPatrolTime = patrolTime;

        patrolStopIndex = 0;

        characterStats = GetComponent<EnemyStats>();
    }

    private void Start()
    {
        if (isGuard)
        {
            enemyStates = EnemyStates.Guard;
        }
        else
        {
            enemyStates = EnemyStates.Patrol;

            //获取巡逻路线
            patrolStops = new List<GameObject>();
            for (int i = 0; i < patrolRount.transform.childCount; i++)
            {
                patrolStops.Add(patrolRount.transform.GetChild(i).gameObject);
            }

            GetNewWayPos();
        }
        //注册观察者，场景切换后更改
        GameManager.Instance.AddObserver(this);
    }

    //场景切换后启用
    //void OnEnable()
    //{
    //    GameManager.Instance.AddObserver(this);
    //}

    void OnDisable()
    {
        if (!GameManager.IsInitialized)
            return;
        GameManager.Instance.RemoveObserver(this);

        if (GetComponent<LootSpawner>() && isDead)
        {
            GetComponent<LootSpawner>().SpawnLoot();
        }
        //if (QuestManager.IsInitialized && isDead)
        //{
        //    QuestManager.Instance.UpdateQuestProgress(this.name, 1);
        //}

    }


    private void Update()
    {
        if (characterStats.CurrentHealth <= 0)//敌人死亡
        {
            isDead = true;
            agent.isStopped = true;
        }


        if (!playerDead)
        {
            SwitchStates();
            SwitchAnimation();

            lastAttackTime -= Time.deltaTime;
        }
    }

    private void OnDrawGizmosSelected()//在场景视图中绘制一个球体，表示敌人的视野范围
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }

    void SwitchStates()
    {
        if (isDead)
        {
            enemyStates = EnemyStates.Dead;
        }
        else if (FoundPlayer() && enemyStates != EnemyStates.Chase)
        {
            // 如果处于巡逻或站桩状态时发现玩家，进入警戒状态并重置计时器
            if (enemyStates == EnemyStates.Guard || enemyStates == EnemyStates.Patrol)
            {
                enemyStates = EnemyStates.Alart;
                alertTimer = alertDuration; // 每次进入警戒状态时重置计时器
            }
        }

        switch (enemyStates)
        {
            case EnemyStates.Guard:
                isFollow = false;
                isChase = false;
                isWalk = true;
                agent.isStopped = false;
                if (transform.position != guardPos) // 没有到达站桩点
                {
                    agent.destination = guardPos;
                }
                if (Vector3.SqrMagnitude(transform.position - guardPos) <= agent.stoppingDistance) // 到达站桩点
                {
                    agent.isStopped = true;
                    isWalk = false;
                    transform.rotation = Quaternion.Lerp(transform.rotation, guardRot, 0.01f);
                }
                break;

            case EnemyStates.Patrol:
                isChase = false;
                isFollow = false;
                agent.isStopped = false;
                agent.speed = speed * 0.5f;
                // 检测是否到达巡逻点
                if (Vector3.Distance(transform.position, wayPos) < agent.stoppingDistance)
                {
                    isWalk = false;
                    if (remainPatrolTime > 0)
                        remainPatrolTime -= Time.deltaTime;
                    else
                        GetNewWayPos();
                }
                else
                {
                    isWalk = true;
                    agent.destination = wayPos;
                }
                break;

            case EnemyStates.Chase:

                GameManager.Instance.playerStats.ChangeExposure(PlayerStats.ExposureState.Chase);
                if (GetComponent<EnemyDialogueController>())
                {
                    gameObject.GetComponent<EnemyDialogueController>().SetAlertedDialogue();
                }

                isFollow = true;
                isWalk = false;
                isChase = true;
                agent.isStopped = false;
                agent.speed = speed * 2;

                if (!FoundPlayer())
                {
                    isFollow = false;
                    isChase = false;
                    isWalk = true;
                    agent.isStopped = false;
                    agent.speed = speed;

                    //攻击被躲开后不应该直接回到默认状态，而是进行搜索
                    //ReturnToDefaultState();
                    enemyStates = EnemyStates.Search;
                    searchTimer = searchDuration; // 初始化搜索计时器
                                                  // 初始化当前搜索目标为一个随机位置
                    currentSearchTarget = GetRandomSearchTarget(lastKnownPlayerPos, searchRadius);

                }
                else
                {
                    agent.destination = attackTarget.transform.position;

                    // 记录玩家最后位置
                    lastKnownPlayerPos = attackTarget.transform.position;
                }
                // 如果在攻击范围之内，则停止移动并攻击
                if (InAttackRange() || InSkillRange())
                {
                    isFollow = false;
                    agent.isStopped = true;
                    if (lastAttackTime < 0)
                    {
                        lastAttackTime = characterStats.CoolDown;
                        // 暴击判断
                        characterStats.isCritical = Random.value < characterStats.CriticalChance;
                        // 执行攻击
                        Attack();
                    }
                }
                break;

            case EnemyStates.Alart:

                GameManager.Instance.playerStats.ChangeExposure(
                    PlayerStats.ExposureState.Alert, 
                    alertDuration);
                if (GetComponent<EnemyDialogueController>())
                {
                    gameObject.GetComponent<EnemyDialogueController>().SetAlertedDialogue();
                }


                // 在警戒状态下持续检测玩家
                if (FoundPlayer())
                {
                    alertTimer -= Time.deltaTime;
                    agent.isStopped = true;
                    isChase = true;
                    transform.LookAt(attackTarget.transform);
                    if (alertTimer <= 0)
                    {
                        enemyStates = EnemyStates.Chase;
                    }

                    // 记录玩家最后位置，并切换到搜索状态
                    lastKnownPlayerPos = attackTarget.transform.position;
                }
                else
                {
                    // 记录玩家最后位置，并切换到搜索状态，在这里的话attackTarget是空对象
                    //lastKnownPlayerPos = attackTarget.transform.position;
                    enemyStates = EnemyStates.Search;
                    searchTimer = searchDuration; // 初始化搜索计时器
                                                  // 初始化当前搜索目标为一个随机位置
                    currentSearchTarget = GetRandomSearchTarget(lastKnownPlayerPos, searchRadius);
                }
                break;

            case EnemyStates.Search:
                isFollow = true;
                isWalk = false;
                isChase = true;
                agent.isStopped = false;
                agent.speed = speed;  // 使用默认速度
                                          // 如果到达当前随机目标点，则生成新的目标点
                if (Vector3.Distance(transform.position, currentSearchTarget) < agent.stoppingDistance)
                {
                    currentSearchTarget = GetRandomSearchTarget(lastKnownPlayerPos, searchRadius);
                }
                else
                {
                    agent.destination = currentSearchTarget;
                }

                searchTimer -= Time.deltaTime;
                // 在搜索过程中如果再次发现玩家，则重新进入警戒状态
                if (FoundPlayer())
                {
                    enemyStates = EnemyStates.Alart;
                    alertTimer = alertDuration;
                }
                else if (searchTimer <= 0)
                {
                    // 搜索时间结束，回到默认状态
                    ReturnToDefaultState();
                }
                break;

            case EnemyStates.Dead:
                collider.enabled = false;
                agent.radius = 0;
                //死亡以后不销毁，而是记录状态，避免重新加载场景时重新生成
                //Destroy(gameObject, 2f);

                transform.GetComponent<ObjectState>().UpdateActive(false, true);

                //死亡以后回复玩家暴露状态
                GameManager.Instance.playerStats.ChangeExposure(PlayerStats.ExposureState.Peace);

                break;
        }
    }

    /// <summary>
    /// 返回在给定中心点附近随机位置
    /// </summary>
    /// <param name="center">中心点</param>
    /// <param name="radius">搜索半径</param>
    /// <returns>随机目标点</returns>
    Vector3 GetRandomSearchTarget(Vector3 center, float radius)
    {
        Vector2 randomPoint = Random.insideUnitCircle * radius;
        Vector3 targetPos = new Vector3(center.x + randomPoint.x, transform.position.y, center.z + randomPoint.y);
        return targetPos;
    }


    void SwitchAnimation()
    {
        animator.SetBool("Walk", isWalk);
        animator.SetBool("Chase", isChase);
        animator.SetBool("Follow", isFollow);
        animator.SetBool("Critical", characterStats.isCritical);

        animator.SetBool("Death", isDead);
    }

    // 返回默认状态的方法
    private void ReturnToDefaultState()
    {
        agent.speed = speed;

        if (isGuard)
        {
            enemyStates = EnemyStates.Guard;
        }
        else
        {
            enemyStates = EnemyStates.Patrol;
        }

        //玩家暴露回到安全状态
        GameManager.Instance.playerStats.ChangeExposure(PlayerStats.ExposureState.Peace);
        //敌人对话数据回到平常版本
        if (GetComponent<EnemyDialogueController>())
        {
            gameObject.GetComponent<EnemyDialogueController>().SetRegularDialogue();
        }
    }

    bool FoundPlayer()
    {
        var colliders = Physics.OverlapSphere(transform.position, sightRange);
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Player")
                && GameManager.Instance.playerStats.currentShape == PlayerStats.PlayerShape.Human) 
            {
                if (transform.isFacingTarget(collider.transform)) 
                {
                    attackTarget = collider.gameObject;
                    return true;
                }

                //// 计算敌人到玩家的方向向量
                //Vector3 directionToPlayer = (collider.transform.position - transform.position).normalized;

                //// 计算该方向与敌人正前方的夹角
                //float angle = Vector3.Angle(transform.forward, directionToPlayer);

                //// 如果夹角在可视角度范围内
                //if (angle <= viewAngle * 0.5f)
                //{
                //    attackTarget = collider.gameObject;
                //    return true;
                //}
            }
        }

        attackTarget = null;
        return false;
    }

    void GetNewWayPos()
    {
        Vector3 newWayPos = new Vector3(
            patrolStops[patrolStopIndex].transform.position.x,
            transform.position.y,
            patrolStops[patrolStopIndex].transform.position.z);

        remainPatrolTime = patrolTime;

        wayPos = newWayPos;
        patrolStopIndex = (patrolStopIndex + 1) % patrolStops.Count;
    }

    //bool FoundPlayer()
    //{
    //    var colliders = Physics.OverlapSphere(transform.position, sightRange);//p12 10:17
    //    foreach (var collider in colliders)
    //    {
    //        if (collider.CompareTag("Player"))//TODO:后期添加角度功能（仅在敌人面前才会被发现
    //        {
    //            attackTarget = collider.gameObject;
    //            return true;
    //        }

    //    }

    //    attackTarget = null;
    //    return false;

    //}


    #region 判断攻击距离

    bool InAttackRange()
    {
        if (attackTarget != null)
        {
            return Vector3.Distance(transform.position, attackTarget.transform.position)
                <= characterStats.AttackRange;
        }
        else
            return false;
    }

    bool InSkillRange()
    {
        if (attackTarget != null)
        {
            return Vector3.Distance(transform.position, attackTarget.transform.position)
                <= characterStats.SkillRange;
        }
        else
            return false;
    }

    #endregion

    void Attack()
    {
        transform.LookAt(attackTarget.transform);

        if (InAttackRange())
        {
            //近身攻击动画
            animator.SetTrigger("Attack");
        }

        if (InSkillRange())
        {
            //技能攻击动画
            animator.SetTrigger("Skill");
        }
    }
    void Hit()
    {

        if (attackTarget != null && transform.isFacingTarget(attackTarget.transform))
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();

            targetStats.TakeDamage(characterStats, targetStats);
        }
    }

    public void EndNotify()
    {
        
        //停止所有移动，停止agent
        agent.isStopped = true;
        //获胜动画
        animator.SetBool("Win", true);

        playerDead = true;

        isChase = false;
        isFollow = false;
        isWalk = false;
        attackTarget = null;
    }
}
