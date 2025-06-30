using UnityEditor;
using UnityEngine;

namespace PHG
{
    [CustomEditor(typeof(MonsterStatEntry))]
    public class MonsterStatEntryEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Inspector GUI 변경 감지 시작
            EditorGUI.BeginChangeCheck();

            // MonsterStatEntry 애셋의 모든 필드를 기본 방식으로 그려줍니다.
            DrawDefaultInspector();

            // 변경이 있었는지 확인
            if (EditorGUI.EndChangeCheck())
            {
                // 변경이 있었다면 Scene 뷰를 강제로 다시 그립니다.
                // 이로 인해 OnDrawGizmos()가 다시 호출되어 변경된 값이 반영됩니다.
                SceneView.RepaintAll();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}