using UnityEditor;
using UnityEngine;

public static class CampusGroundTools
{
    [MenuItem("Tools/Campus Grounds/Create Roads, Lawn and Trees")]
    private static void CreateCampusGrounds()
    {
        GameObject ground = Selection.activeGameObject;
        Renderer groundRenderer = ground.GetComponent<Renderer>();
        Bounds bounds = groundRenderer.bounds;

        Transform previousLandscaping = ground.transform.Find("Campus Landscaping");
        if (previousLandscaping != null)
        {
            Undo.DestroyObjectImmediate(previousLandscaping.gameObject);
        }

        GameObject landscaping = new GameObject("Campus Landscaping");
        Undo.RegisterCreatedObjectUndo(landscaping, "Create campus landscaping");
        landscaping.transform.SetParent(ground.transform, false);

        Material asphalt = CreateMaterial("Asphalt", new Color(0.09f, 0.10f, 0.11f), 0.18f);
        Material grass = CreateMaterial("Grass", new Color(0.13f, 0.42f, 0.16f), 0.25f);
        Material bark = CreateMaterial("Tree Bark", new Color(0.25f, 0.12f, 0.05f), 0.32f);
        Material leaves = CreateMaterial("Tree Leaves", new Color(0.06f, 0.32f, 0.09f), 0.38f);

        float surfaceY = bounds.max.y + 0.015f;
        float roadDepth = Mathf.Clamp(bounds.size.z * 0.13f, 1.2f, 3.2f);
        float sideRoadWidth = Mathf.Clamp(bounds.size.x * 0.10f, 1.1f, 2.8f);
        float lawnWidth = Mathf.Max(2f, bounds.size.x - sideRoadWidth * 2f - 0.5f);
        float lawnDepth = Mathf.Max(2f, bounds.size.z - roadDepth * 2f - 0.5f);

        CreateBox("Front Asphalt Road", landscaping.transform,
            new Vector3(bounds.center.x, surfaceY, bounds.max.z - roadDepth * 0.5f),
            new Vector3(bounds.size.x, 0.04f, roadDepth), asphalt);
        CreateBox("Rear Asphalt Road", landscaping.transform,
            new Vector3(bounds.center.x, surfaceY, bounds.min.z + roadDepth * 0.5f),
            new Vector3(bounds.size.x, 0.04f, roadDepth), asphalt);
        CreateBox("Left Asphalt Road", landscaping.transform,
            new Vector3(bounds.min.x + sideRoadWidth * 0.5f, surfaceY, bounds.center.z),
            new Vector3(sideRoadWidth, 0.04f, bounds.size.z - roadDepth * 2f), asphalt);
        CreateBox("Right Asphalt Road", landscaping.transform,
            new Vector3(bounds.max.x - sideRoadWidth * 0.5f, surfaceY, bounds.center.z),
            new Vector3(sideRoadWidth, 0.04f, bounds.size.z - roadDepth * 2f), asphalt);

        CreateBox("Central Grass Courtyard", landscaping.transform,
            new Vector3(bounds.center.x, surfaceY + 0.025f, bounds.center.z),
            new Vector3(lawnWidth, 0.05f, lawnDepth), grass);

        Vector3[] treeOffsets =
        {
            new Vector3(-0.30f, -0.28f), new Vector3(0.28f, -0.25f),
            new Vector3(-0.32f, 0.25f), new Vector3(0.30f, 0.28f),
            new Vector3(0f, 0f), new Vector3(-0.08f, 0.36f), new Vector3(0.12f, -0.38f)
        };

        for (int i = 0; i < treeOffsets.Length; i++)
        {
            Vector3 treePosition = new Vector3(
                bounds.center.x + treeOffsets[i].x * lawnWidth,
                surfaceY + 0.05f,
                bounds.center.z + treeOffsets[i].y * lawnDepth);
            CreateTree($"Courtyard Tree {i + 1}", landscaping.transform, treePosition, bark, leaves);
        }

        Selection.activeGameObject = landscaping;
        EditorUtility.SetDirty(ground);
    }

    [MenuItem("Tools/Campus Grounds/Create Roads, Lawn and Trees", true)]
    private static bool CanCreateCampusGrounds()
    {
        return Selection.activeGameObject != null &&
               Selection.activeGameObject.GetComponent<Renderer>() != null;
    }

    private static void CreateTree(string treeName, Transform parent, Vector3 position, Material bark, Material leaves)
    {
        GameObject tree = new GameObject(treeName);
        Undo.RegisterCreatedObjectUndo(tree, "Create courtyard tree");
        tree.transform.SetParent(parent, true);

        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Trunk";
        Undo.RegisterCreatedObjectUndo(trunk, "Create tree trunk");
        Object.DestroyImmediate(trunk.GetComponent<Collider>());
        trunk.transform.position = position + Vector3.up * 0.65f;
        trunk.transform.localScale = new Vector3(0.18f, 0.65f, 0.18f);
        trunk.transform.SetParent(tree.transform, true);
        trunk.GetComponent<Renderer>().sharedMaterial = bark;

        GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        canopy.name = "Canopy";
        Undo.RegisterCreatedObjectUndo(canopy, "Create tree canopy");
        Object.DestroyImmediate(canopy.GetComponent<Collider>());
        canopy.transform.position = position + Vector3.up * 1.75f;
        canopy.transform.localScale = new Vector3(1.15f, 1.25f, 1.15f);
        canopy.transform.SetParent(tree.transform, true);
        canopy.GetComponent<Renderer>().sharedMaterial = leaves;
    }

    private static void CreateBox(string objectName, Transform parent, Vector3 position, Vector3 size, Material material)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = objectName;
        Undo.RegisterCreatedObjectUndo(box, "Create campus ground feature");
        Object.DestroyImmediate(box.GetComponent<BoxCollider>());
        box.transform.position = position;
        box.transform.localScale = size;
        box.transform.SetParent(parent, true);
        box.GetComponent<Renderer>().sharedMaterial = material;
    }

    private static Material CreateMaterial(string materialName, Color color, float smoothness)
    {
        const string folder = "Assets/TutorialInfo/GeneratedMaterials";
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
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0f);
        EditorUtility.SetDirty(material);
        return material;
    }
}
