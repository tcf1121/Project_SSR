using UnityEditor;
using UnityEngine;

namespace PHG
{
    [CustomPropertyDrawer(typeof(MonsterStatEntry))]
    public class MonsterStatEntryDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float totalHeight = EditorGUIUtility.singleLineHeight;

            if (property.isExpanded)
            {
                SerializedProperty monsterTypeProp = property.FindPropertyRelative("monsterType");
                if (monsterTypeProp != null)
                {
                    totalHeight += EditorGUI.GetPropertyHeight(monsterTypeProp, true) + EditorGUIUtility.standardVerticalSpacing;
                }

                SerializedProperty currentProperty = property.Copy();
                SerializedProperty endProperty = property.GetEndProperty();

                if (currentProperty.NextVisible(true))
                {
                    do
                    {
                        if (SerializedProperty.EqualContents(currentProperty, endProperty))
                            break;

                        if (currentProperty.name == monsterTypeProp.name)
                            continue;

                        totalHeight += EditorGUI.GetPropertyHeight(currentProperty, true) + EditorGUIUtility.standardVerticalSpacing;

                    } while (currentProperty.NextVisible(false));
                }
            }

            return totalHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect currentPosition = position;
            currentPosition.height = EditorGUIUtility.singleLineHeight;

            SerializedProperty monsterTypeProp = property.FindPropertyRelative("monsterType");

            string displayName = "None (MonsterStatEntry)";
            if (monsterTypeProp != null && monsterTypeProp.enumNames.Length > monsterTypeProp.enumValueIndex && monsterTypeProp.enumValueIndex >= 0)
            {
                displayName = monsterTypeProp.enumNames[monsterTypeProp.enumValueIndex];
            }

            // Inspector GUI ���� ���� ���� (�� ��ξ� ������ �߻��ϴ� ��� ����)
            EditorGUI.BeginChangeCheck();

            property.isExpanded = EditorGUI.Foldout(currentPosition, property.isExpanded, new GUIContent(displayName), true);

            currentPosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                if (monsterTypeProp != null)
                {
                    currentPosition.height = EditorGUI.GetPropertyHeight(monsterTypeProp, true);
                    EditorGUI.PropertyField(currentPosition, monsterTypeProp, true);
                    currentPosition.y += currentPosition.height + EditorGUIUtility.standardVerticalSpacing;
                }

                SerializedProperty currentChildProperty = property.Copy();
                SerializedProperty endProperty = property.GetEndProperty();

                if (currentChildProperty.NextVisible(true))
                {
                    do
                    {
                        if (SerializedProperty.EqualContents(currentChildProperty, endProperty))
                            break;

                        if (currentChildProperty.name == monsterTypeProp.name)
                            continue;

                        currentPosition.height = EditorGUI.GetPropertyHeight(currentChildProperty, true);
                        EditorGUI.PropertyField(currentPosition, currentChildProperty, true);

                        currentPosition.y += currentPosition.height + EditorGUIUtility.standardVerticalSpacing;

                    } while (currentChildProperty.NextVisible(false));
                }

                EditorGUI.indentLevel--;
            }

            // ������ �־����� Ȯ��
            if (EditorGUI.EndChangeCheck())
            {
                // ������ �־��ٸ� Scene �並 ������ �ٽ� �׸��ϴ�.
                SceneView.RepaintAll();
            }

            EditorGUI.EndProperty();
        }
    }
}