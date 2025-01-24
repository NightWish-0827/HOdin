using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

public class GameComponentEditorManager : AssetModificationProcessor
{
    private const string SETTINGS_PATH = "Assets/Editor/ComponentEditorSettings.json";
    private static EditorSettingsData settingsData;
    public static event System.Action OnSettingsChanged;

    private static readonly Queue<Action> pendingCallbacks = new Queue<Action>();
    private static bool isLoading = false;
    private static System.DateTime lastWriteTime;

    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        EditorApplication.update += CheckForFileChanges;

        try
        {
            // Editor 폴더 경로 생성
            string editorPath = "Assets/Editor";
            if (!Directory.Exists(editorPath))
            {
                Directory.CreateDirectory(editorPath);
                AssetDatabase.Refresh(); // Unity에 폴더 생성을 알림
            }

            // 설정 파일 초기화
            if (File.Exists(SETTINGS_PATH))
            {
                lastWriteTime = File.GetLastWriteTimeUtc(SETTINGS_PATH);
                _ = LoadSettingsAsync();
            }
            else
            {
                settingsData = new EditorSettingsData();
                SaveSettings(); // 초기 설정 파일 생성
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"초기화 실패: {e.Message}\n{e.StackTrace}");
            settingsData = new EditorSettingsData(); // 폴백 설정
        }
    }

    public static void UpdateComponentData(ComponentEditorData newData)
    {
        if (settingsData == null)
        {
            settingsData = new EditorSettingsData();
        }

        var existingData = settingsData.components
            .FirstOrDefault(c => c.componentTypeName == newData.componentTypeName);

        if (existingData != null)
        {
            existingData.colorHtml = newData.colorHtml;
            existingData.headerEntries = newData.headerEntries;
        }
        else
        {
            settingsData.components.Add(newData);
        }

        SaveSettings();
    }

    private static async void CheckForFileChanges()
    {
        if (!File.Exists(SETTINGS_PATH)) return;

        try
        {
            DateTime currentWriteTime;
            try
            {
                currentWriteTime = File.GetLastWriteTimeUtc(SETTINGS_PATH);
            }
            catch
            {
                return; // 파일 접근 실패시 무시
            }

            if (currentWriteTime != lastWriteTime)
            {
                lastWriteTime = currentWriteTime;
                await LoadSettingsAsync();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"파일 변경 감지 실패: {e.Message}");
        }
    }

    private static async Task LoadSettingsAsync()
    {
        if (isLoading) return;
        isLoading = true;

        try
        {
            if (!File.Exists(SETTINGS_PATH))
            {
                settingsData = new EditorSettingsData();
                isLoading = false;
                return;
            }

            string json;
            try
            {
                using (var stream = new FileStream(SETTINGS_PATH, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(stream))
                {
                    json = await reader.ReadToEndAsync();
                }
            }
            catch (IOException)
            {
                isLoading = false;
                return; // 파일 접근 실패시 현재 상태 유지
            }

            EditorApplication.delayCall += () =>
            {
                try
                {
                    settingsData = JsonUtility.FromJson<EditorSettingsData>(json);
                    ProcessPendingCallbacks();
                    OnSettingsChanged?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError($"설정 파싱 실패: {e.Message}");
                    LoadFallbackSettings();
                }
                finally
                {
                    isLoading = false;
                }
            };
        }
        catch (Exception e)
        {
            Debug.LogError($"설정 로드 실패: {e.Message}");
            LoadFallbackSettings();
            isLoading = false;
        }
    }

    private static void ProcessPendingCallbacks()
    {
        while (pendingCallbacks.Count > 0)
        {
            var callback = pendingCallbacks.Dequeue();
            try
            {
                callback?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"콜백 처리 실패: {e.Message}");
            }
        }
    }

    private static void LoadFallbackSettings()
    {
        settingsData = new EditorSettingsData();
    }

    public static void AccessSettings(Action<EditorSettingsData> action)
    {
        if (settingsData != null)
        {
            action(settingsData);
        }
        else
        {
            pendingCallbacks.Enqueue(() => action(settingsData));
            _ = LoadSettingsAsync();
        }
    }

    public static ComponentEditorData GetComponentData(string typeName)
    {
        if (settingsData == null)
        {
            var tcs = new TaskCompletionSource<ComponentEditorData>();
            pendingCallbacks.Enqueue(() =>
            {
                var data = settingsData?.components.FirstOrDefault(c => c.componentTypeName == typeName);
                tcs.SetResult(data);
            });
            _ = LoadSettingsAsync();
            return null;
        }
        return settingsData.components.FirstOrDefault(c => c.componentTypeName == typeName);
    }

    public static void SaveSettings()
    {
        try
        {
            // Editor 폴더가 없다면 생성
            string directoryPath = Path.GetDirectoryName(SETTINGS_PATH);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                AssetDatabase.Refresh();
            }

            string json = JsonUtility.ToJson(settingsData, true);
            File.WriteAllText(SETTINGS_PATH, json);
            
            lastWriteTime = File.GetLastWriteTimeUtc(SETTINGS_PATH);
            
            EditorApplication.delayCall += () =>
            {
                AssetDatabase.Refresh();
                OnSettingsChanged?.Invoke();
            };
        }
        catch (Exception e)
        {
            Debug.LogError($"설정 저장 실패: {e.Message}\n{e.StackTrace}");
        }
    }
}