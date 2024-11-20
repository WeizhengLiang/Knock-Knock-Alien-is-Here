using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class PrefabGeneratorWindow : EditorWindow
{
    private enum ObjectType { Normal, Heavy, Fragile }

    private class ObjectTemplate
    {
        public string prefabName = "NewPrefab";
        public ObjectType selectedType = ObjectType.Normal;
        public Sprite objectSprite;
        public GameObject brokenPrefab;
        public float mass = 1f;
    }

    private List<ObjectTemplate> templates = new List<ObjectTemplate>();

    [MenuItem("Tools/Prefab Generator")]
    public static void ShowWindow()
    {
        GetWindow<PrefabGeneratorWindow>("Prefab Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Item Prefab Generator", EditorStyles.boldLabel);

        if (GUILayout.Button("Add Item Template"))
        {
            templates.Add(new ObjectTemplate());
        }

        for (int i = 0; i < templates.Count; i++)
        {
            GUILayout.BeginVertical("box");
            GUILayout.Label($"Item Template {i + 1}", EditorStyles.boldLabel);

            templates[i].prefabName = EditorGUILayout.TextField("Prefab Name", templates[i].prefabName);
            templates[i].selectedType = (ObjectType)EditorGUILayout.EnumPopup("Item Type", templates[i].selectedType);
            templates[i].objectSprite = (Sprite)EditorGUILayout.ObjectField("Item Sprite", templates[i].objectSprite, typeof(Sprite), false);
            templates[i].mass = EditorGUILayout.Slider("Mass", templates[i].mass, 1f, 18f);

            if (templates[i].selectedType == ObjectType.Fragile)
            {
                templates[i].brokenPrefab = (GameObject)EditorGUILayout.ObjectField("Broken Prefab", templates[i].brokenPrefab, typeof(GameObject), false);
            }

            if (GUILayout.Button("Remove Template"))
            {
                templates.RemoveAt(i);
                i--; // Adjust index after removal
            }

            GUILayout.EndVertical();
        }

        if (GUILayout.Button("Generate All Prefabs"))
        {
            GeneratePrefabs();
        }
    }

    private void GeneratePrefabs()
    {
        foreach (var template in templates)
        {
            if (string.IsNullOrEmpty(template.prefabName) || template.objectSprite == null || (template.selectedType == ObjectType.Fragile && template.brokenPrefab == null))
            {
                Debug.LogError("Please ensure all required fields are filled in!");
                return;
            }

            string templatePath = GetTemplatePath(template.selectedType);
            GameObject templatePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(templatePath);

            if (templatePrefab == null)
            {
                Debug.LogError($"Failed to load template prefab: {templatePath}");
                return;
            }

            GameObject newObject = Instantiate(templatePrefab);
            newObject.name = template.prefabName;

            SpriteRenderer sr = newObject.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = template.objectSprite;
            }

            Rigidbody2D rb = newObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.mass = template.mass;
            }

            if (template.selectedType == ObjectType.Fragile)
            {
                FragileObject fragile = newObject.GetComponent<FragileObject>();
                if (fragile != null)
                {
                    fragile.brokenPrefab = template.brokenPrefab;
                }
            }

            // 重新生成碰撞体
            PolygonCollider2D oldCollider = newObject.GetComponent<PolygonCollider2D>();
            if (oldCollider != null)
            {
                // 先添加新的碰撞体
                PolygonCollider2D newCollider = newObject.AddComponent<PolygonCollider2D>();

                // 再移除旧的碰撞体
                DestroyImmediate(oldCollider);
            }

            string localPath = "Assets/Prefabs/" + template.prefabName + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(newObject, localPath);
            DestroyImmediate(newObject);

            Debug.Log("Prefab generated successfully: " + localPath);
        }
    }

    private string GetTemplatePath(ObjectType type)
    {
        switch (type)
        {
            case ObjectType.Normal:
                return "Assets/Prefabs/DraggableObjectTemplates/normalObj.prefab";
            case ObjectType.Heavy:
                return "Assets/Prefabs/DraggableObjectTemplates/heavyObj.prefab";
            case ObjectType.Fragile:
                return "Assets/Prefabs/DraggableObjectTemplates/fragileObj.prefab";
            default:
                return null;
        }
    }
}