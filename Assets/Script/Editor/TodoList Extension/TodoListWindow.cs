using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

[InitializeOnLoad]
public class TodoListWindow : EditorWindow
{
    private const float WINDOW_MIN_WIDTH = 400f;
    private const float WINDOW_MIN_HEIGHT = 300f;
    private Vector2 scrollPosition;
    private string newTaskName = "";
    private string projectName;
    private List<TodoItem> todoItems = new List<TodoItem>();

    private enum TodoListType
    {
        [InspectorName("개인")] Personal,
        [InspectorName("프로젝트")] Team
    }

    private TodoListType currentListType = TodoListType.Personal;

    private const string TEAM_TODO_PATH = "Assets/Script/Editor/TodoList Extension/TodolistPath";
    private const string PERSONAL_TODO_FOLDER = "TodoList";

    private const string FIRST_LAUNCH_KEY = "TodoList_FirstLaunch";

    // 세션 체크를 위한 정적 변수
    private static bool sessionStarted = false;

    static TodoListWindow()
    {
        EditorApplication.delayCall += () =>
        {
            // 현재 세션에서 아직 실행되지 않았는지 확인
            if (!sessionStarted && !Application.isPlaying)
            {
                var windows = Resources.FindObjectsOfTypeAll<TodoListWindow>();
                if (windows == null || windows.Length == 0)
                {
                    ShowWindow();
                }
                sessionStarted = true;
            }
        };
    }

    [MenuItem("Tools/Todo List")]
    public static void ShowWindow()
    {
        var window = GetWindow<TodoListWindow>("Todo List");
        window.minSize = new Vector2(WINDOW_MIN_WIDTH, WINDOW_MIN_HEIGHT);
    }

    [Serializable]
    private class TodoItem
    {
        public string title;
        public TaskStatus status;
        public string modifiedTime;
    }

    [Serializable]
    private class TodoData
    {
        public List<TodoItem> items = new List<TodoItem>();
    }

    private void OnEnable()
    {
        projectName = Application.productName;
        ValidateSavePath(); // 경로 유효성 검사 및 생성
        LoadTodoList();
    }

    void OnGUI()
    {
        EditorGUILayout.Space(10);
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontSize = 16;
        titleStyle.richText = true;

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField($"<color=#59A5F2>{projectName}</color> TODO LIST", titleStyle, GUILayout.ExpandWidth(false));
        
        // 타입 선택 토글 추가
        EditorGUI.BeginChangeCheck();
        currentListType = (TodoListType)EditorGUILayout.EnumPopup(currentListType, GUILayout.Width(80));
        if (EditorGUI.EndChangeCheck())
        {
            LoadTodoList(); // 타입 변경시 리스트 다시 로드
        }
        
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        DrawNewTaskArea();
        DrawTodoLists();
    }

    private void DrawNewTaskArea()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("새 할일", GUILayout.Width(50));

        var textAreaStyle = new GUIStyle(EditorStyles.textArea);
        textAreaStyle.wordWrap = true;

        GUI.SetNextControlName("NewTaskInput");
        var currentEvent = Event.current;
        if (currentEvent.type == EventType.KeyDown && currentEvent.keyCode == KeyCode.Return && !currentEvent.shift)
        {
            if (!string.IsNullOrEmpty(newTaskName))
            {
                AddNewTask();
                currentEvent.Use();
            }
        }

        newTaskName = EditorGUILayout.TextArea(newTaskName, textAreaStyle, GUILayout.Height(20));

        if (GUILayout.Button("추가", GUILayout.Width(60), GUILayout.Height(20)))
        {
            if (!string.IsNullOrEmpty(newTaskName))
            {
                AddNewTask();
            }
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
    }

    private void AddNewTask()
    {
        var newItem = new TodoItem
        {
            title = newTaskName,
            status = TaskStatus.Todo,
            modifiedTime = DateTime.Now.ToString("o")
        };

        todoItems.Add(newItem);
        newTaskName = "";
        GUI.FocusControl(null);
        SaveTodoList();
    }

    private void DrawTodoLists()
    {
        EditorGUILayout.Space(10);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        DrawTaskArea("할 일", TaskStatus.Todo, new Color(1f, 1f, 1f, 1f));
        DrawTaskArea("진행 중", TaskStatus.InProgress, new Color(0.7f, 0.85f, 1f, 1f));
        DrawTaskArea("완료", TaskStatus.Done, new Color(0.7f, 1f, 0.7f, 1f));
        DrawTaskArea("이슈", TaskStatus.Issue, new Color(1f, 0.7f, 0.7f, 1f));

        EditorGUILayout.EndScrollView();
    }

    private void DrawTaskArea(string title, TaskStatus status, Color areaColor)
    {
        EditorGUILayout.Space(10);
        var titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 12;
        EditorGUILayout.LabelField(title, titleStyle);

        var originalColor = GUI.backgroundColor;
        GUI.backgroundColor = areaColor;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = originalColor;

        var tasksInStatus = todoItems.Where(item => item.status == status).ToList();

        if (tasksInStatus.Count == 0)
        {
            EditorGUILayout.LabelField("항목 없음", EditorStyles.centeredGreyMiniLabel);
        }

        for (int i = 0; i < tasksInStatus.Count; i++)
        {
            DrawTaskItem(tasksInStatus[i]);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawTaskItem(TodoItem item)
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField("상태:", GUILayout.Width(40));
        var newStatus = (TaskStatus)EditorGUILayout.EnumPopup(item.status, GUILayout.Width(80));

        var textStyle = new GUIStyle(EditorStyles.textArea);
        textStyle.wordWrap = true;
        var newTitle = EditorGUILayout.TextArea(item.title, textStyle, GUILayout.Height(20));

        if (EditorGUI.EndChangeCheck())
        {
            item.status = newStatus;
            item.title = newTitle;
            item.modifiedTime = DateTime.Now.ToString("o");
            SaveTodoList();
        }

        if (GUILayout.Button("삭제", GUILayout.Width(50)))
        {
            todoItems.Remove(item);
            SaveTodoList();
        }

        EditorGUILayout.EndHorizontal();
    }

    private void SaveTodoList()
    {
        var data = new TodoData { items = todoItems };
        var json = JsonUtility.ToJson(data);
        File.WriteAllText(GetSavePath(), json);
    }

    private void LoadTodoList()
    {
        var path = GetSavePath();
        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<TodoData>(json);
            todoItems = data.items;
        }
    }

    private string GetSavePath()
    {
        string basePath;
        string fileName = currentListType == TodoListType.Personal 
            ? $"PersonalTodoList_{projectName}.json"
            : $"TeamTodoList_{projectName}.json";
        
        if (currentListType == TodoListType.Personal)
        {
            // 개인용은 Unity Temp 폴더 사용
            basePath = Path.Combine(Application.temporaryCachePath, PERSONAL_TODO_FOLDER);
        }
        else
        {
            // 협업용은 프로젝트 내부 경로 사용
            basePath = TEAM_TODO_PATH;
        }

        // 폴더가 없으면 생성
        if (!Directory.Exists(basePath))
        {
            Directory.CreateDirectory(basePath);
        }

        return Path.Combine(basePath, fileName);
    }

    private void ValidateSavePath()
    {
        string basePath = currentListType == TodoListType.Personal 
            ? Path.Combine(Application.temporaryCachePath, PERSONAL_TODO_FOLDER)
            : TEAM_TODO_PATH;

        try
        {
            // 기본 경로가 없다면 생성
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
                Debug.Log($"Todo List 폴더 생성됨: {basePath}");
            }

            // 파일 경로 확인
            string filePath = GetSavePath();
            string fileDirectory = Path.GetDirectoryName(filePath);
            
            // 파일이 위치할 디렉토리가 없다면 생성
            if (!Directory.Exists(fileDirectory))
            {
                Directory.CreateDirectory(fileDirectory);
                Debug.Log($"Todo List 하위 폴더 생성됨: {fileDirectory}");
            }

            // 파일이 없다면 빈 Todo 리스트 생성
            if (!File.Exists(filePath))
            {
                var emptyData = new TodoData { items = new List<TodoItem>() };
                string jsonData = JsonUtility.ToJson(emptyData, true);
                File.WriteAllText(filePath, jsonData);
                Debug.Log($"새 Todo List 파일 생성됨: {filePath}");
            }

            // 쓰기 권한 테스트
            string testFile = Path.Combine(fileDirectory, "test.tmp");
            File.WriteAllText(testFile, "test");
            File.Delete(testFile);
        }
        catch (Exception e)
        {
            Debug.LogError($"Todo List 저장 경로 오류: {e.Message}");
            EditorUtility.DisplayDialog("오류", 
                "Todo List 저장 경로를 생성할 수 없습니다. 권한을 확인해주세요.", 
                "확인");
        }
    }
}

public enum TaskStatus
{
    [InspectorName("할 일")] Todo,
    [InspectorName("진행 중")] InProgress,
    [InspectorName("완료")] Done,
    [InspectorName("이슈")] Issue
}