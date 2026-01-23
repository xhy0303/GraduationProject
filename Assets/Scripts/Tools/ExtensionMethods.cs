using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class extensionMethod
{
    // 角度阈值（单位：度），表示在面前多少度范围内视为"面向目标"
    private static float angleThreshold = 170f;

    public static bool isFacingTarget(this Transform transform, Transform target)
    {
        Vector3 directionToTarget = (target.position - transform.position).normalized;

        // 将角度阈值转换为余弦值（点积阈值）
        float cosThreshold = Mathf.Cos(angleThreshold * Mathf.Deg2Rad);

        float dot = Vector3.Dot(transform.forward, directionToTarget);

        return dot >= cosThreshold;
    }
}