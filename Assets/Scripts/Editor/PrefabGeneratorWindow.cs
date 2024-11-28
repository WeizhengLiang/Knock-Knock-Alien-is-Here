using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class PrefabGeneratorWindow : EditorWindow
{
    private enum ObjectType { Normal, Heavy, Fragile, Collectible }

    // 预设材质路径
    private const string NORMAL_MATERIAL_PATH = "Assets/Art/Material/Mat_Default.mat";
    private const string DRAGGING_MATERIAL_PATH = "Assets/Art/Material/Mat_Pickup.mat";
    private const string INVALID_MATERIAL_PATH = "Assets/Art/Material/Mat_Red.mat";
    private const string CAN_MERGE_MATERIAL_PATH = "Assets/Art/Material/Mat_Green.mat";
    private const string CAN_PICKUP_MATERIAL_PATH = "Assets/Art/Material/Mat_CanPickup.mat";
    private const string OBJECT_ANIMATOR_PATH = "Assets/Art/Animation/ObjectAnim.controller";
    
    // 缓存材质引用
    private Material normalMaterial;
    private Material draggingMaterial;
    private Material invalidMaterial;
    private Material canMergeMaterial;
    private Material canPickupMaterial;
    private RuntimeAnimatorController objectAnimController;

    private class ObjectTemplate
    {
        public string prefabName = "NewPrefab";
        public ObjectType selectedType = ObjectType.Normal;
        public Sprite objectSprite;
        public GameObject brokenPrefab;
        public float mass = 1f;
    }

    private class CollectibleTemplate
    {
        // 基础属性 (来自 ObjectTemplate)
        public string prefabName = "NewCollectible";
        public Sprite objectSprite;
        public float mass = 1f;
        
        // 收集物特有属性
        public CollectibleType collectibleType = CollectibleType.Declaration;
        public UnlockMethod unlockMethod = UnlockMethod.Drop;
        
        // 基本信息
        public string itemName = "";
        public string description = "";
        public string alienResponse = "";
        
        // 视觉元素
        public Sprite comicSprite;
        
        // 特殊设置
        public float checkRadius = 0.5f;
        public float breakForce = 8f;
    }

    private class CollectibleTriggerTemplate
    {
        public string prefabName = "NewTrigger";
        public Sprite objectSprite;
        public float mass = 1f;
        public string triggerTag = "PowerSource"; // 默认为电源标签
    }

    private List<ObjectTemplate> templates = new List<ObjectTemplate>();
    private List<CollectibleTemplate> collectibleTemplates = new List<CollectibleTemplate>();
    private List<CollectibleTriggerTemplate> triggerTemplates = new List<CollectibleTriggerTemplate>();

    private Vector2 scrollPosition;

    [MenuItem("Tools/Prefab Generator")]
    public static void ShowWindow()
    {
        GetWindow<PrefabGeneratorWindow>("Prefab Generator");
    }

    private void OnEnable()
    {
        // 加载材质
        LoadMaterials();
    }

    private void LoadMaterials()
    {
        normalMaterial = AssetDatabase.LoadAssetAtPath<Material>(NORMAL_MATERIAL_PATH);
        draggingMaterial = AssetDatabase.LoadAssetAtPath<Material>(DRAGGING_MATERIAL_PATH);
        invalidMaterial = AssetDatabase.LoadAssetAtPath<Material>(INVALID_MATERIAL_PATH);
        canMergeMaterial = AssetDatabase.LoadAssetAtPath<Material>(CAN_MERGE_MATERIAL_PATH);
        canPickupMaterial = AssetDatabase.LoadAssetAtPath<Material>(CAN_PICKUP_MATERIAL_PATH);
        objectAnimController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(OBJECT_ANIMATOR_PATH);
        
        if (normalMaterial == null || draggingMaterial == null || invalidMaterial == null || 
            canMergeMaterial == null || canPickupMaterial == null || objectAnimController == null)
        {
            Debug.LogError("无法加载材质或动画控制器！请确保文件存在于正确的路径中。");
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Item Prefab Generator", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Item Template"))
        {
            templates.Add(new ObjectTemplate());
        }

        if (GUILayout.Button("Add Collectible Template"))
        {
            collectibleTemplates.Add(new CollectibleTemplate());
        }

        if (GUILayout.Button("Add Trigger Template"))
        {
            triggerTemplates.Add(new CollectibleTriggerTemplate());
        }

        EditorGUILayout.EndHorizontal();

        // 开始滚动视图
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // 显示普通物品模板
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

        // 显示收集物模板
        for (int i = 0; i < collectibleTemplates.Count; i++)
        {
            GUILayout.BeginVertical("box");
            GUILayout.Label($"Collectible Template {i + 1}", EditorStyles.boldLabel);

            collectibleTemplates[i].prefabName = EditorGUILayout.TextField("Prefab Name", collectibleTemplates[i].prefabName);
            collectibleTemplates[i].objectSprite = (Sprite)EditorGUILayout.ObjectField("Item Sprite", collectibleTemplates[i].objectSprite, typeof(Sprite), false);
            collectibleTemplates[i].mass = EditorGUILayout.Slider("Mass", collectibleTemplates[i].mass, 1f, 18f);

            collectibleTemplates[i].collectibleType = (CollectibleType)EditorGUILayout.EnumPopup("Collectible Type", collectibleTemplates[i].collectibleType);
            collectibleTemplates[i].unlockMethod = (UnlockMethod)EditorGUILayout.EnumPopup("Unlock Method", collectibleTemplates[i].unlockMethod);

            collectibleTemplates[i].itemName = EditorGUILayout.TextField("Item Name", collectibleTemplates[i].itemName);
            collectibleTemplates[i].description = EditorGUILayout.TextField("Description", collectibleTemplates[i].description);
            collectibleTemplates[i].alienResponse = EditorGUILayout.TextField("Alien Response", collectibleTemplates[i].alienResponse);

            collectibleTemplates[i].comicSprite = (Sprite)EditorGUILayout.ObjectField("Comic Sprite", collectibleTemplates[i].comicSprite, typeof(Sprite), false);

            collectibleTemplates[i].checkRadius = EditorGUILayout.Slider("Check Radius", collectibleTemplates[i].checkRadius, 0.1f, 2f);
            collectibleTemplates[i].breakForce = EditorGUILayout.Slider("Break Force", collectibleTemplates[i].breakForce, 1f, 20f);

            if (GUILayout.Button("Remove Template"))
            {
                collectibleTemplates.RemoveAt(i);
                i--; // Adjust index after removal
            }

            GUILayout.EndVertical();
        }

        // 显示触发器模板
        for (int i = 0; i < triggerTemplates.Count; i++)
        {
            GUILayout.BeginVertical("box");
            GUILayout.Label($"Trigger Template {i + 1}", EditorStyles.boldLabel);

            triggerTemplates[i].prefabName = EditorGUILayout.TextField("Prefab Name", triggerTemplates[i].prefabName);
            triggerTemplates[i].objectSprite = (Sprite)EditorGUILayout.ObjectField("Item Sprite", triggerTemplates[i].objectSprite, typeof(Sprite), false);
            triggerTemplates[i].mass = EditorGUILayout.Slider("Mass", triggerTemplates[i].mass, 1f, 18f);
            triggerTemplates[i].triggerTag = EditorGUILayout.TextField("Trigger Tag", triggerTemplates[i].triggerTag);

            if (GUILayout.Button("Remove Template"))
            {
                triggerTemplates.RemoveAt(i);
                i--; // Adjust index after removal
            }

            GUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Generate All Prefabs"))
        {
            GeneratePrefabs();
        }
    }

    private void GeneratePrefabs()
    {
        // 确保材质已加载
        if (normalMaterial == null || draggingMaterial == null || invalidMaterial == null || 
            canMergeMaterial == null || canPickupMaterial == null || objectAnimController == null)
        {
            Debug.LogError("材质或动画控制器未正确加载！");
            return;
        }

        // 生成普通物品预制体
        foreach (var template in templates)
        {
            if (string.IsNullOrEmpty(template.prefabName) || template.objectSprite == null || 
                (template.selectedType == ObjectType.Fragile && template.brokenPrefab == null))
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

            // 设置基础组件
            SetupBasicComponents(newObject, template);

            // 设置特殊组件
            if (template.selectedType == ObjectType.Fragile)
            {
                FragileObject fragile = newObject.GetComponent<FragileObject>();
                if (fragile != null)
                {
                    fragile.brokenPrefab = template.brokenPrefab;
                }
            }

            string localPath = "Assets/Prefabs/" + template.prefabName + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(newObject, localPath);
            DestroyImmediate(newObject);

            Debug.Log("Prefab generated successfully: " + localPath);
        }

        // 生成收集物预制体
        foreach (var template in collectibleTemplates)
        {
            GenerateCollectiblePrefab(template);
        }

        // 生成触发器预制体
        foreach (var template in triggerTemplates)
        {
            GenerateTriggerPrefab(template);
        }
    }

    private void SetupBasicComponents(GameObject obj, ObjectTemplate template)
    {
        // 设置材质
        DraggableObject draggable = obj.GetComponent<DraggableObject>();
        if (draggable != null)
        {
            SerializedObject serializedDraggable = new SerializedObject(draggable);
            SetMaterialProperties(serializedDraggable);
            serializedDraggable.ApplyModifiedProperties();
        }

        // 设置精灵
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = template.objectSprite;
        }

        // 设置刚体
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.mass = template.mass;
        }

        // 重新生成碰撞体
        RegenerateCollider(obj);

        // 设置动画控制器
        SetupAnimator(obj, draggable);
    }

    private void RegenerateCollider(GameObject obj)
    {
        PolygonCollider2D oldCollider = obj.GetComponent<PolygonCollider2D>();
        if (oldCollider != null)
        {
            PolygonCollider2D newCollider = obj.AddComponent<PolygonCollider2D>();
            DestroyImmediate(oldCollider);
        }
    }

    private void SetupAnimator(GameObject obj, DraggableObject draggable)
    {
        Animator animator = obj.AddComponent<Animator>();
        animator.runtimeAnimatorController = objectAnimController;

        if (draggable != null)
        {
            SerializedObject serializedDraggable = new SerializedObject(draggable);
            SerializedProperty animatorProp = serializedDraggable.FindProperty("objectAnimator");
            animatorProp.objectReferenceValue = animator;
            serializedDraggable.ApplyModifiedProperties();
        }
    }

    private void SetMaterialProperties(SerializedObject serializedObject)
    {
        SerializedProperty normalMatProp = serializedObject.FindProperty("normalMaterial");
        SerializedProperty draggingMatProp = serializedObject.FindProperty("draggingMaterial");
        SerializedProperty invalidMatProp = serializedObject.FindProperty("invalidMaterial");
        SerializedProperty canMergeMatProp = serializedObject.FindProperty("canMergeMaterial");
        SerializedProperty canPickupMatProp = serializedObject.FindProperty("canPickupMaterial");
        
        normalMatProp.objectReferenceValue = normalMaterial;
        draggingMatProp.objectReferenceValue = draggingMaterial;
        invalidMatProp.objectReferenceValue = invalidMaterial;
        canMergeMatProp.objectReferenceValue = canMergeMaterial;
        canPickupMatProp.objectReferenceValue = canPickupMaterial;
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

    private void GenerateCollectiblePrefab(CollectibleTemplate template)
    {
        if (string.IsNullOrEmpty(template.prefabName) || template.objectSprite == null)
        {
            Debug.LogError($"收集物 {template.prefabName} 缺少必要属性！");
            return;
        }

        // 首先生成收集物数据
        CollectibleData collectibleData = ScriptableObject.CreateInstance<CollectibleData>();
        collectibleData.type = template.collectibleType;
        collectibleData.itemName = template.itemName;
        collectibleData.description = template.description;
        collectibleData.alienResponse = template.alienResponse;
        collectibleData.unlockMethod = template.unlockMethod;
        collectibleData.icon = template.objectSprite;
        collectibleData.comicSprite = template.comicSprite;

        // 保存收集物数据
        string dataPath = $"Assets/Data/{template.prefabName}Data.asset";
        AssetDatabase.CreateAsset(collectibleData, dataPath);

        // 创建预制体对象
        GameObject newObject = new GameObject(template.prefabName);
        newObject.layer = LayerMask.NameToLayer("Draggable");

        // 添加基础组件
        SpriteRenderer sr = newObject.AddComponent<SpriteRenderer>();
        sr.sprite = template.objectSprite;
        sr.material = normalMaterial;
        
        Rigidbody2D rb = newObject.AddComponent<Rigidbody2D>();
        rb.mass = template.mass;
        
        PolygonCollider2D col = newObject.AddComponent<PolygonCollider2D>();

        // 根据解锁方式添加对应的收集物脚本
        CollectibleObject collectible = null;
        switch (template.unlockMethod)
        {
            case UnlockMethod.Drop:
                collectible = newObject.AddComponent<DeclarationCollectible>();
                break;
            case UnlockMethod.Break:
                var specimen = newObject.AddComponent<SpecimenCollectible>();
                specimen.breakForce = template.breakForce;
                collectible = specimen;
                break;
            case UnlockMethod.Position:
                collectible = newObject.AddComponent<WiretapperCollectible>();
                break;
            case UnlockMethod.PowerSource:
                collectible = newObject.AddComponent<LaserPointerCollectible>();
                break;
            case UnlockMethod.Disk:
                collectible = newObject.AddComponent<TranslatorCollectible>();
                break;
            case UnlockMethod.Rocket:
                collectible = newObject.AddComponent<LaunchPadCollectible>();
                break;
        }

        // 设置收集物属性
        SerializedObject serializedCollectible = new SerializedObject(collectible);
        
        // 设置材质
        SetMaterialProperties(serializedCollectible);
        
        // 设置数据引用
        SerializedProperty dataProp = serializedCollectible.FindProperty("data");
        dataProp.objectReferenceValue = collectibleData;
        
        // 设置碰撞层
        SerializedProperty collisionLayerProp = serializedCollectible.FindProperty("collisionLayer");
        collisionLayerProp.intValue = LayerMask.NameToLayer("Draggable");
        
        // 设置动画控制器
        Animator animator = newObject.AddComponent<Animator>();
        animator.runtimeAnimatorController = objectAnimController;
        SerializedProperty animatorProp = serializedCollectible.FindProperty("visualAnimator");
        animatorProp.objectReferenceValue = animator;
        
        serializedCollectible.ApplyModifiedProperties();

        // 保存预制体
        string prefabPath = $"Assets/Prefabs/Collectibles/{template.prefabName}.prefab";
        PrefabUtility.SaveAsPrefabAsset(newObject, prefabPath);
        DestroyImmediate(newObject);

        Debug.Log($"收集物预制体生成成功: {prefabPath}");
        Debug.Log($"收集物数据生成成功: {dataPath}");
    }

    private void GenerateTriggerPrefab(CollectibleTriggerTemplate template)
    {
        if (string.IsNullOrEmpty(template.prefabName) || template.objectSprite == null)
        {
            Debug.LogError($"触发器 {template.prefabName} 缺少必要属性！");
            return;
        }

        // 创建预制体对象
        GameObject newObject = new GameObject(template.prefabName);
        newObject.layer = LayerMask.NameToLayer("Draggable");
        newObject.tag = template.triggerTag;

        // 添加基础组件
        SpriteRenderer sr = newObject.AddComponent<SpriteRenderer>();
        sr.sprite = template.objectSprite;
        sr.material = normalMaterial;
        
        Rigidbody2D rb = newObject.AddComponent<Rigidbody2D>();
        rb.mass = template.mass;
        
        PolygonCollider2D col = newObject.AddComponent<PolygonCollider2D>();

        // 添加触发器组件
        TriggerObject trigger = newObject.AddComponent<TriggerObject>();
        
        // 设置属性
        SerializedObject serializedTrigger = new SerializedObject(trigger);
        
        // 设置材质
        SetMaterialProperties(serializedTrigger);
        
        // 设置碰撞层
        SerializedProperty collisionLayerProp = serializedTrigger.FindProperty("collisionLayer");
        collisionLayerProp.intValue = LayerMask.NameToLayer("Draggable");
        
        // 设置动画控制器
        Animator animator = newObject.AddComponent<Animator>();
        animator.runtimeAnimatorController = objectAnimController;
        SerializedProperty animatorProp = serializedTrigger.FindProperty("objectAnimator");
        animatorProp.objectReferenceValue = animator;
        
        // 设置触发器标签
        SerializedProperty triggerTagProp = serializedTrigger.FindProperty("triggerTag");
        triggerTagProp.stringValue = template.triggerTag;
        
        serializedTrigger.ApplyModifiedProperties();

        // 保存预制体
        string prefabPath = $"Assets/Prefabs/CollectibleTriggers/{template.prefabName}.prefab";
        PrefabUtility.SaveAsPrefabAsset(newObject, prefabPath);
        DestroyImmediate(newObject);

        Debug.Log($"触发器预制体生成成功: {prefabPath}");
    }
}
