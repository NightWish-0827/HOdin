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

        public void Initialize(GameComponentEditor editor, string propertyName, string currentText)
        {
            this.editor = editor;
            this.propertyName = propertyName;
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
                if (string.IsNullOrEmpty(headerText))
                {
                    editor.customHeaderTexts.Remove(propertyName);
                }
                else
                {
                    editor.customHeaderTexts[propertyName] = headerText;
                }
                
                editor.SaveSettingsToJson();
                editor.Repaint();
                Close();
            }
        }
    }
}