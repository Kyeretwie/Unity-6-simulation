using UnityEditor;
using UnityEngine;

public static class CampusBuildingTools
{
    private enum BuildingStyle
    {
        Library,
        Administration,
        Cafeteria
    }

    [MenuItem("Tools/Campus Buildings/Decorate Selected as Library")]
    private static void DecorateLibrary() => DecorateSelected(BuildingStyle.Library);

    [MenuItem("Tools/Campus Buildings/Decorate Selected as Administration")]
    private static void DecorateAdministration() => DecorateSelected(BuildingStyle.Administration);

    [MenuItem("Tools/Campus Buildings/Decorate Selected as Cafeteria")]
    private static void DecorateCafeteria() => DecorateSelected(BuildingStyle.Cafeteria);

    [MenuItem("Tools/Campus Buildings/Decorate Selected as Library", true)]
    [MenuItem("Tools/Campus Buildings/Decorate Selected as Administration", true)]
    [MenuItem("Tools/Campus Buildings/Decorate Selected as Cafeteria", true)]
    private static bool CanDecorateSelected()
    {
        return Selection.activeGameObject != null &&
               Selection.activeGameObject.GetComponent<Renderer>() != null;
    }

    private static void DecorateSelected(BuildingStyle style)
    {
        GameObject building = Selection.activeGameObject;
        Renderer buildingRenderer = building.GetComponent<Renderer>();
        Bounds bounds = buildingRenderer.bounds;

        Transform previousFeatures = building.transform.Find("Campus Features");
        if (previousFeatures != null)
        {
            Undo.DestroyObjectImmediate(previousFeatures.gameObject);
        }

        GameObject features = new GameObject("Campus Features");
        Undo.RegisterCreatedObjectUndo(features, "Add campus building features");
        features.transform.SetParent(building.transform, false);

        Color roofColor = style switch
        {
            BuildingStyle.Library => new Color(0.23f, 0.10f, 0.05f),
            BuildingStyle.Administration => new Color(0.12f, 0.15f, 0.19f),
            _ => new Color(0.06f, 0.28f, 0.31f)
        };

        Material roofMaterial = CreateMaterial($"{style} Roof", roofColor, false);
        Material frameMaterial = CreateMaterial("Window Frames", new Color(0.06f, 0.08f, 0.10f), false);
        Material doorMaterial = CreateMaterial("Entrance Doors", new Color(0.12f, 0.22f, 0.27f), style == BuildingStyle.Cafeteria);
        Material windowMaterial = CreateMaterial(
            style == BuildingStyle.Cafeteria ? "Cafeteria Glass" : "Building Windows",
            style == BuildingStyle.Cafeteria
                ? new Color(0.12f, 0.85f, 0.92f, 0.40f)
                : new Color(0.16f, 0.43f, 0.60f),
            style == BuildingStyle.Cafeteria);

        float roofHeight = Mathf.Max(0.25f, bounds.size.y * 0.08f);
        CreateBox("Roof", features.transform,
            new Vector3(bounds.center.x, bounds.max.y + roofHeight * 0.5f, bounds.center.z),
            new Vector3(bounds.size.x + 0.4f, roofHeight, bounds.size.z + 0.4f), roofMaterial);

        float doorWidth = Mathf.Min(2.2f, bounds.size.x * 0.30f);
        float doorHeight = Mathf.Min(2.8f, bounds.size.y * 0.62f);
        float frontZ = bounds.max.z + 0.035f;
        CreateBox("Main Entrance", features.transform,
            new Vector3(bounds.center.x, bounds.min.y + doorHeight * 0.5f, frontZ),
            new Vector3(doorWidth, doorHeight, 0.08f), doorMaterial);

        float frameThickness = 0.12f;
        CreateBox("Door Frame Top", features.transform,
            new Vector3(bounds.center.x, bounds.min.y + doorHeight + frameThickness * 0.5f, frontZ + 0.01f),
            new Vector3(doorWidth + frameThickness * 2f, frameThickness, 0.12f), frameMaterial);
        CreateBox("Door Frame Left", features.transform,
            new Vector3(bounds.center.x - doorWidth * 0.5f - frameThickness * 0.5f, bounds.min.y + doorHeight * 0.5f, frontZ + 0.01f),
            new Vector3(frameThickness, doorHeight, 0.12f), frameMaterial);
        CreateBox("Door Frame Right", features.transform,
            new Vector3(bounds.center.x + doorWidth * 0.5f + frameThickness * 0.5f, bounds.min.y + doorHeight * 0.5f, frontZ + 0.01f),
            new Vector3(frameThickness, doorHeight, 0.12f), frameMaterial);

        int frontWindowCount = style == BuildingStyle.Cafeteria ? 4 : 3;
        float windowHeight = style == BuildingStyle.Cafeteria
            ? bounds.size.y * 0.48f
            : bounds.size.y * 0.28f;
        float windowY = bounds.min.y + bounds.size.y * 0.68f;
        float windowWidth = Mathf.Min(1.8f, bounds.size.x / (frontWindowCount + 1f));

        for (int i = 0; i < frontWindowCount; i++)
        {
            float x = Mathf.Lerp(bounds.min.x + windowWidth * 0.65f, bounds.max.x - windowWidth * 0.65f,
                (i + 0.5f) / frontWindowCount);
            CreateBox($"Front Window {i + 1}", features.transform,
                new Vector3(x, windowY, frontZ + 0.02f),
                new Vector3(windowWidth, windowHeight, 0.06f), windowMaterial);
        }

        int sideWindowCount = style == BuildingStyle.Cafeteria ? 3 : 2;
        float sideX = bounds.max.x + 0.035f;
        for (int i = 0; i < sideWindowCount; i++)
        {
            float z = Mathf.Lerp(bounds.min.z + bounds.size.z * 0.22f, bounds.max.z - bounds.size.z * 0.22f,
                sideWindowCount == 1 ? 0.5f : (float)i / (sideWindowCount - 1));
            CreateBox($"Side Window {i + 1}", features.transform,
                new Vector3(sideX, windowY, z),
                new Vector3(0.06f, windowHeight, Mathf.Min(1.7f, bounds.size.z * 0.28f)), windowMaterial);
        }

        Selection.activeGameObject = features;
        EditorUtility.SetDirty(building);
    }

    private static void CreateBox(string objectName, Transform parent, Vector3 position, Vector3 size, Material material)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = objectName;
        Undo.RegisterCreatedObjectUndo(box, "Create building feature");
        Object.DestroyImmediate(box.GetComponent<BoxCollider>());
        box.transform.position = position;
        box.transform.localScale = size;
        box.transform.SetParent(parent, true);
        box.GetComponent<Renderer>().sharedMaterial = material;
    }

    private static Material CreateMaterial(string materialName, Color color, bool transparent)
    {
        string folder = "Assets/TutorialInfo/GeneratedMaterials";
        if (!AssetDatabase.IsValidFolder(folder))
        {
            AssetDatabase.CreateFolder("Assets/TutorialInfo", "GeneratedMaterials");
        }

        string path = $"{folder}/{materialName}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }

        material.color = color;
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", transparent ? 0.92f : 0.55f);
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", transparent ? 0.15f : 0.05f);

        if (transparent && material.HasProperty("_Surface"))
        {
            material.SetFloat("_Surface", 1f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        EditorUtility.SetDirty(material);
        return material;
    }
}
