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

            // Inspector GUI 변경 감지 시작 (이 드로어 내에서 발생하는 모든 변경)
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

            // 변경이 있었는지 확인
            if (EditorGUI.EndChangeCheck())
            {
                // 변경이 있었다면 Scene 뷰를 강제로 다시 그립니다.
                SceneView.RepaintAll();
            }

            EditorGUI.EndProperty();
        }
    }
}