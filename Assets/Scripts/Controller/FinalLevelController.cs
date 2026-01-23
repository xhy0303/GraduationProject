using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class FinalLevelController : MonoBehaviour
{
    [Header("敌人生成设置")]
    public Transform[] spawnPoints;
    public GameObject[] enemyPrefabs;
    [Tooltip("生成敌人时检测区域内的半径，防止重叠")]
    public float safeSpawnRadius = 1.0f;
    [Tooltip("敌人所在的Layer")]
    public LayerMask enemyLayer;

    [Header("波次设置")]
    public int totalWaves = 5;
    public int baseEnemiesPerWave = 3;
    public float spawnInterval = 1.5f;

    private int currentWave = 0;
    private int aliveEnemies = 0; // 活着的敌人数

    PlayableDirector director; // 用于播放动画

    [Header("获胜奖励")]
    public GameObject chest;

    void Start()
    {
        director = FindObjectOfType<PlayableDirector>();
        director.stopped += StartGame; // 订阅动画播放结束事件
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (GameManager.Instance.playerStats.currentShape != PlayerStats.PlayerShape.Yokai)
                director.Play();
            else
                OnFinalWaveComplete();
        }
    }

    public void StartGame(PlayableDirector obj)
    {
        StartCoroutine(HandleWaves());
    }

    IEnumerator HandleWaves()
    {
        while (currentWave < totalWaves)
        {
            currentWave++;
            // 不希望敌人越来越多，后续波次敌人数量减少
            int enemiesThisWave = baseEnemiesPerWave + (totalWaves - currentWave) * 2;

            Debug.Log($"开始第 {currentWave} 波，共 {enemiesThisWave} 个敌人。");

            for (int i = 0; i < enemiesThisWave; i++)
            {
                yield return StartCoroutine(SpawnEnemyRoutine());
                yield return new WaitForSeconds(spawnInterval);
            }

            // 等待当前波次的所有敌人被清理干净后再进入下一波
            yield return new WaitUntil(() => aliveEnemies <= 0);
            Debug.Log($"第 {currentWave} 波已清理完毕。");

            yield return new WaitForSeconds(2f); // 清完后稍等
        }

        Debug.Log("所有波次完成！");
        OnFinalWaveComplete();
    }

    // 利用协程生成敌人，确保生成区域内不会重叠
    IEnumerator SpawnEnemyRoutine()
    {
        // 设置最大尝试次数，避免长时间等待
        int maxAttempts = 30;
        int attempts = 0;
        Transform chosenSpawn = null;

        while (attempts < maxAttempts)
        {
            // 随机选取一个生成点
            chosenSpawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
            // 利用 OverlapSphere 检测该区域是否已有敌人
            Collider[] colliders = Physics.OverlapSphere(chosenSpawn.position, safeSpawnRadius, enemyLayer);
            if (colliders.Length == 0)
            {
                // 找到空闲的生成点，退出循环
                break;
            }
            else
            {
                attempts++;
                yield return null; // 等待下一帧重新检测
            }
        }

        // 如果经过多次尝试仍未找到空闲位置，则直接使用最后选择的点
        if (attempts >= maxAttempts)
        {
            Debug.LogWarning("尝试多次未找到空闲生成点，仍使用最后选定位置生成敌人。");
        }

        // 根据当前波次选择对应强度的敌人预制体（最多选到数组最后一个）
        int enemyIndex = Mathf.Min(currentWave - 1, enemyPrefabs.Length - 1);
        GameObject enemyPrefab = enemyPrefabs[enemyIndex];

        GameObject enemy = Instantiate(enemyPrefab, chosenSpawn.position, chosenSpawn.rotation);
        aliveEnemies++; // 生成时计数

        // 给生成的敌人附加死亡处理组件，用于检测死亡时调用回调
        EnemyDeathHandler deathHandler = enemy.AddComponent<EnemyDeathHandler>();
        deathHandler.controller = this;
    }

    // 供敌人死亡时调用（可由 EnemyDeathHandler 或敌人自身调用）
    public void OnEnemyKilled()
    {
        aliveEnemies--;
        aliveEnemies = Mathf.Max(aliveEnemies, 0); // 防止计数出错
    }

    void OnFinalWaveComplete()
    {
        Debug.Log("关卡完成！");
        // 获胜以后掉落宝箱，打开宝箱以后再显示胜利UI并处理胜利逻辑
        chest.SetActive(true);
    }
}
