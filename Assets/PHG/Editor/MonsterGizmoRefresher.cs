#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class MonsterGizmoRefresher
{
    static MonsterGizmoRefresher()
    {
        SceneView.duringSceneGui += _ => SceneView.RepaintAll();
    }
}
#endif