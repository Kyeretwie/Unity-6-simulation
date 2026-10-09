using UnityEditor;
using UnityEngine;

public static class FountainStageTools
{
    private const string MaterialFolder = "Assets/Materials/Generated";
    private const string LogoPath = "Assets/Materials/Logos/KNUST_Official_Emblem.jpg";

    [MenuItem("Tools/Campus Features/Create KNUST Logo Fountain")]
    private static void CreateKnustLogoFountain()
    {
        GameObject stage = Selection.activeGameObject;
        if (stage == null)
        {
            EditorUtility.DisplayDialog("Select Fountain Stage", "Select the Fountain_stage object first, then run this tool again.", "OK");
            return;
        }

        Transform existingFountain = stage.transform.Find("KNUST Logo Fountain");
        if (existingFountain != null)
        {
            bool replace = EditorUtility.DisplayDialog("Replace Fountain?",
                "A KNUST Logo Fountain already exists under this stage. Replace it?", "Replace", "Cancel");
            if (!replace) return;
            Undo.DestroyObjectImmediate(existingFountain.gameObject);
        }

        Material stone = GetOrCreateMaterial("FountainStone.mat", new Color(0.32f, 0.34f, 0.36f), null, false);
        Material water = GetOrCreateMaterial("FountainWater.mat", new Color(0.05f, 0.7f, 0.9f, 0.62f), null, true);
        Texture2D logoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(LogoPath);
        Material logo = GetOrCreateMaterial("KNUSTLogo.mat", Color.white, logoTexture, false, true);

        Bounds stageBounds = GetStageBounds(stage);
        float radius = stageBounds.size.x > 0f && stageBounds.size.z > 0f
            ? Mathf.Min(stageBounds.size.x, stageBounds.size.z) * 0.18f
            : 8f;
        radius = Mathf.Max(radius, 4f);

        GameObject fountain = new GameObject("KNUST Logo Fountain");
        Undo.RegisterCreatedObjectUndo(fountain, "Create KNUST Logo Fountain");
        fountain.transform.SetParent(stage.transform, false);
        fountain.transform.position = new Vector3(stageBounds.center.x, stageBounds.max.y, stageBounds.center.z);

        CreateCylinder("Stone Basin", fountain.transform, Vector3.up * radius * 0.075f,
            new Vector3(radius, radius * 0.075f, radius), stone);
        CreateCylinder("Water Surface", fountain.transform, Vector3.up * radius * 0.16f,
            new Vector3(radius * 0.88f, radius * 0.012f, radius * 0.88f), water)
            .AddComponent<FountainWaterRipple>();
        CreateCylinder("Center Pedestal", fountain.transform, Vector3.up * radius * 0.33f,
            new Vector3(radius * 0.25f, radius * 0.17f, radius * 0.25f), stone);

        CreateWaterJet("Central Water Jet", fountain.transform, new Vector3(0f, radius * 0.68f, 0f),
            radius * 0.075f, radius * 0.35f, water);

        for (int i = 0; i < 6; i++)
        {
            float angle = i * Mathf.PI * 2f / 6f;
            Vector3 position = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius * 0.55f;
            CreateWaterJet("Water Jet " + (i + 1), fountain.transform,
                position + Vector3.up * radius * 0.35f, radius * 0.035f, radius * 0.18f, water);
        }

        GameObject rotatingLogo = new GameObject("Rotating KNUST Emblem");
        rotatingLogo.transform.SetParent(fountain.transform, false);
        rotatingLogo.transform.localPosition = Vector3.up * radius * 1.05f;
        rotatingLogo.AddComponent<Spinner>();

        float logoWidth = radius * 0.55f;
        float logoHeight = radius * 0.7f;
        CreateLogoFace("KNUST Emblem Front", rotatingLogo.transform, Vector3.zero, Quaternion.identity,
            new Vector3(logoWidth, logoHeight, 1f), logo);
        CreateLogoFace("KNUST Emblem Back", rotatingLogo.transform, Vector3.zero, Quaternion.Euler(0f, 180f, 0f),
            new Vector3(logoWidth, logoHeight, 1f), logo);

        FountainProximityParticles proximityParticles = fountain.AddComponent<FountainProximityParticles>();
        proximityParticles.Configure(fountain.GetComponentsInChildren<ParticleSystem>(true), radius * 2.2f);

        Selection.activeGameObject = fountain;
        EditorUtility.SetDirty(stage);
    }

    private static Bounds GetStageBounds(GameObject stage)
    {
        Renderer[] renderers = stage.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            return new Bounds(stage.transform.position, new Vector3(40f, 0f, 40f));
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }

    private static GameObject CreateCylinder(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.name = name;
        cylinder.transform.SetParent(parent, false);
        cylinder.transform.localPosition = localPosition;
        cylinder.transform.localScale = localScale;
        cylinder.GetComponent<Renderer>().sharedMaterial = material;
        return cylinder;
    }

    private static void CreateWaterJet(string name, Transform parent, Vector3 localPosition, float radius, float height, Material material)
    {
        GameObject jetColumn = CreateCylinder(name, parent, localPosition, new Vector3(radius, height, radius), material);
        jetColumn.GetComponent<Collider>().enabled = false;

        GameObject particleObject = new GameObject(name + " Particles");
        particleObject.transform.SetParent(parent, false);
        particleObject.transform.localPosition = localPosition - Vector3.up * height;
        particleObject.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

        ParticleSystem particles = particleObject.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.loop = true;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = 1f;
        main.startSpeed = height * 3f;
        main.startSize = Mathf.Max(radius * 2f, 0.05f);
        main.startColor = new Color(0.35f, 0.9f, 1f, 0.75f);
        main.gravityModifier = 0.55f;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 45f;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.radius = radius;
        shape.angle = 5f;

        ParticleSystemRenderer particleRenderer = particleObject.GetComponent<ParticleSystemRenderer>();
        particleRenderer.renderMode = ParticleSystemRenderMode.Billboard;
        particleRenderer.sharedMaterial = material;
    }

    private static void CreateLogoFace(string name, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Material material)
    {
        GameObject face = GameObject.CreatePrimitive(PrimitiveType.Quad);
        face.name = name;
        face.transform.SetParent(parent, false);
        face.transform.localPosition = localPosition;
        face.transform.localRotation = localRotation;
        face.transform.localScale = localScale;
        face.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(face.GetComponent<Collider>());
    }

    private static Material GetOrCreateMaterial(string fileName, Color color, Texture2D texture, bool transparent, bool unlit = false)
    {
        EnsureFolder(MaterialFolder);
        string path = MaterialFolder + "/" + fileName;
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            string shaderName = unlit ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit";
            material = new Material(Shader.Find(shaderName) ?? Shader.Find("Standard"));
            AssetDatabase.CreateAsset(material, path);
        }

        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        if (texture != null && material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
        if (texture != null && material.HasProperty("_MainTex")) material.SetTexture("_MainTex", texture);

        if (transparent)
        {
            material.SetFloat("_Surface", 1f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = 3000;
        }

        EditorUtility.SetDirty(material);
        AssetDatabase.SaveAssets();
        return material;
    }

    private static void EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder)) return;
        EnsureFolder("Assets/Materials");
        AssetDatabase.CreateFolder("Assets/Materials", "Generated");
    }
}
