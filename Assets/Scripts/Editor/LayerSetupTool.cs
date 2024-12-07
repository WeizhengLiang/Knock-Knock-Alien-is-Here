using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class LayerSetupTool : EditorWindow
{
    private List<GameObject> prefabList = new List<GameObject>();
    private string layerName = "Coverage";
    private Vector2 scrollPosition;

    [MenuItem("Tools/Layer Setup Tool")]
    public static void ShowWindow()
    {
        GetWindow<LayerSetupTool>("Layer Setup Tool");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Set Layer for Square Objects", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Layer名称输入框
        layerName = EditorGUILayout.TextField("Layer Name", layerName);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Target Prefabs:", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 开始滚动视图
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // 显示预制体列表
        for (int i = 0; i < prefabList.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            
            prefabList[i] = (GameObject)EditorGUILayout.ObjectField(
                prefabList[i], 
                typeof(GameObject), 
                false
            );

            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                prefabList.RemoveAt(i);
                i--;
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        // 快捷操作按钮
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Clear List"))
        {
            prefabList.Clear();
        }

        if (GUILayout.Button("Add Selected Prefabs"))
        {
            AddSelectedPrefabs();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // 处理按钮
        GUI.enabled = prefabList.Count > 0;
        if (GUILayout.Button("Set Layers for All Prefabs"))
        {
            ProcessAllPrefabs();
        }
        GUI.enabled = true;
    }

    private void AddSelectedPrefabs()
    {
        Object[] selectedObjects = Selection.objects;
        foreach (Object obj in selectedObjects)
        {
            if (obj is GameObject prefab && PrefabUtility.IsPartOfAnyPrefab(prefab))
            {
                if (!prefabList.Contains(prefab))
                {
                    prefabList.Add(prefab);
                }
            }
        }
    }

    private void ProcessAllPrefabs()
    {
        int successCount = 0;
        int skipCount = 0;
        List<string> errorPrefabs = new List<string>();

        // 检查Layer是否存在
        int newLayer = LayerMask.NameToLayer(layerName);
        if (newLayer == -1)
        {
            EditorUtility.DisplayDialog("Error", 
                $"Layer '{layerName}' does not exist! Please create it first.", "OK");
            return;
        }

        // 处理每个预制体
        foreach (GameObject prefab in prefabList.ToArray())
        {
            if (prefab == null)
            {
                skipCount++;
                continue;
            }

            try
            {
                if (ProcessPrefab(prefab, newLayer))
                {
                    successCount++;
                }
                else
                {
                    skipCount++;
                }
            }
            catch (System.Exception e)
            {
                errorPrefabs.Add($"{prefab.name} ({e.Message})");
            }
        }

        // 显示结果
        string message = $"Processing complete!\n\n" +
                        $"Successfully processed: {successCount}\n" +
                        $"Skipped/No changes: {skipCount}\n";
        
        if (errorPrefabs.Count > 0)
        {
            message += $"\nErrors in prefabs:\n" + string.Join("\n", errorPrefabs);
        }

        EditorUtility.DisplayDialog("Result", message, "OK");
    }

    private bool ProcessPrefab(GameObject prefab, int newLayer)
    {
        string prefabPath = AssetDatabase.GetAssetPath(prefab);
        if (string.IsNullOrEmpty(prefabPath))
        {
            return false;
        }

        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
        bool madeChanges = false;

        try
        {
            Transform[] allChildren = prefabRoot.GetComponentsInChildren<Transform>(true);
            
            foreach (Transform child in allChildren)
            {
                if (child.name.StartsWith("Square", System.StringComparison.OrdinalIgnoreCase))
                {
                    if (child.gameObject.layer != newLayer)
                    {
                        child.gameObject.layer = newLayer;
                        madeChanges = true;
                    }
                }
            }

            if (madeChanges)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            }

            return madeChanges;
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }
}