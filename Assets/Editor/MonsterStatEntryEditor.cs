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

            // Inspector GUI ���� ���� ����
            EditorGUI.BeginChangeCheck();

            // MonsterStatEntry �ּ��� ��� �ʵ带 �⺻ ������� �׷��ݴϴ�.
            DrawDefaultInspector();

            // ������ �־����� Ȯ��
            if (EditorGUI.EndChangeCheck())
            {
                // ������ �־��ٸ� Scene �並 ������ �ٽ� �׸��ϴ�.
                // �̷� ���� OnDrawGizmos()�� �ٽ� ȣ��Ǿ� ����� ���� �ݿ��˴ϴ�.
                SceneView.RepaintAll();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}