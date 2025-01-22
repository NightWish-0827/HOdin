using UnityEditor;
using UnityEngine;

public partial class GameComponentEditor
{
    public class ColorPicker : EditorWindow
    {
        public Color color;
        public GameComponentEditor editor;

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            color = EditorGUILayout.ColorField("색상", color);
            if (EditorGUI.EndChangeCheck() && editor != null)
            {
                editor.UpdateColor(color);
            }
        }
    }
    public class HeaderTextEditor : EditorWindow
    {
        private GameComponentEditor editor;
        private string propertyName;
        private string headerText;
        private string targetTypeName;

        public void Initialize(GameComponentEditor editor, string propertyName, string currentText)
        {
            this.editor = editor;
            this.propertyName = propertyName;
            this.targetTypeName = editor.target.GetType().Name;
            this.headerText = currentText;
            this.minSize = new Vector2(300, 100);
            this.maxSize = new Vector2(300, 100);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("헤더 텍스트 입력");
            headerText = EditorGUILayout.TextField(headerText);

            if (GUILayout.Button("저장"))
            {
                string key = $"{HeaderTextKey}{targetTypeName}_{propertyName}";
                EditorPrefs.SetString(key, headerText);
                editor.customHeaderTexts[propertyName] = headerText;
                editor.Repaint();
                Close();
            }
        }
    }
}