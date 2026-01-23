using UnityEngine;

public class EnemyDeathHandler : MonoBehaviour
{
    public FinalLevelController controller;

    private bool hasDied = false;

    void Update()
    {
        // 示例：当敌人死亡时调用
        if (!hasDied && IsDead())
        {
            hasDied = true;
            controller.OnEnemyKilled();
        }
    }

    bool IsDead()
    {
        // 根据你的敌人逻辑判断死亡状态（比如血量 <= 0，或者播放了死亡动画）
        // 这里做简单示例
        EnemyController ec = GetComponent<EnemyController>();
        return ec != null && ec.isDead; 
    }
}
