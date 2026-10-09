using UnityEditor;
using UnityEngine;

public static class ParkingLotTools
{
    private const string MaterialFolder = "Assets/Materials/Generated";

    [MenuItem("Tools/Campus Features/Create Parking Bays and Two Cars")]
    private static void CreateParkingBaysAndCars()
    {
        GameObject parkingPlane = Selection.activeGameObject;
        Renderer parkingRenderer = parkingPlane != null ? parkingPlane.GetComponent<Renderer>() : null;
        if (parkingRenderer == null)
        {
            EditorUtility.DisplayDialog("Select a Parking Plane", "Select Parking_space1 or Parking_space2 in the Hierarchy first.", "OK");
            return;
        }

        Transform existing = parkingPlane.transform.Find("Parking Bays and Cars");
        if (existing != null)
        {
            if (!EditorUtility.DisplayDialog("Replace Parking Setup?", "Replace the existing markings and cars on this plane?", "Replace", "Cancel"))
                return;
            Undo.DestroyObjectImmediate(existing.gameObject);
        }

        Material line = GetOrCreateMaterial("ParkingLine.mat", Color.white);
        Material red = GetOrCreateMaterial("CampusCarRed.mat", new Color(0.75f, 0.05f, 0.04f));
        Material blue = GetOrCreateMaterial("CampusCarBlue.mat", new Color(0.03f, 0.18f, 0.65f));
        Material glass = GetOrCreateMaterial("CampusCarGlass.mat", new Color(0.08f, 0.45f, 0.65f));
        Material tyre = GetOrCreateMaterial("CampusCarTyre.mat", new Color(0.03f, 0.03f, 0.03f));

        Bounds bounds = parkingRenderer.bounds;
        bool longerAlongX = bounds.size.x >= bounds.size.z;
        float longSize = longerAlongX ? bounds.size.x : bounds.size.z;
        float shortSize = longerAlongX ? bounds.size.z : bounds.size.x;
        float bayWidth = longSize * 0.42f;
        float bayDepth = shortSize * 0.78f;
        float lineThickness = Mathf.Max(shortSize * 0.018f, 0.08f);
        float surfaceY = bounds.max.y + lineThickness * 0.55f;

        GameObject setup = new GameObject("Parking Bays and Cars");
        Undo.RegisterCreatedObjectUndo(setup, "Create Parking Bays and Cars");
        setup.transform.SetParent(parkingPlane.transform, true);

        for (int i = 0; i < 2; i++)
        {
            float offset = i == 0 ? -longSize * 0.24f : longSize * 0.24f;
            Vector3 bayCenter = bounds.center + (longerAlongX ? Vector3.right : Vector3.forward) * offset;
            bayCenter.y = surfaceY;

            float sizeX = longerAlongX ? bayWidth : bayDepth;
            float sizeZ = longerAlongX ? bayDepth : bayWidth;
            CreateBayMarkings("Parking Bay " + (i + 1), setup.transform, bayCenter, sizeX, sizeZ, lineThickness, line);

            Quaternion carRotation = longerAlongX ? Quaternion.identity : Quaternion.Euler(0f, 90f, 0f);
            float carWidth = bayWidth * 0.58f;
            float carLength = bayDepth * 0.64f;
            CreateCampusCar("Parked Car " + (i + 1), setup.transform,
                bayCenter + Vector3.up * (carWidth * 0.08f), carRotation, carWidth, carLength,
                i == 0 ? red : blue, glass, tyre);
        }

        Selection.activeGameObject = setup;
        EditorUtility.SetDirty(parkingPlane);
    }

    private static void CreateBayMarkings(string name, Transform parent, Vector3 center, float sizeX, float sizeZ, float thickness, Material material)
    {
        GameObject bay = new GameObject(name);
        bay.transform.SetParent(parent, true);
        CreateBox("Front Line", bay.transform, center + Vector3.forward * (sizeZ * 0.5f), new Vector3(sizeX, thickness, thickness), material);
        CreateBox("Back Line", bay.transform, center - Vector3.forward * (sizeZ * 0.5f), new Vector3(sizeX, thickness, thickness), material);
        CreateBox("Left Line", bay.transform, center - Vector3.right * (sizeX * 0.5f), new Vector3(thickness, thickness, sizeZ), material);
        CreateBox("Right Line", bay.transform, center + Vector3.right * (sizeX * 0.5f), new Vector3(thickness, thickness, sizeZ), material);
    }

    private static void CreateCampusCar(string name, Transform parent, Vector3 position, Quaternion rotation, float width, float length,
        Material bodyMaterial, Material glassMaterial, Material tyreMaterial)
    {
        GameObject car = new GameObject(name);
        car.transform.SetParent(parent, true);
        car.transform.position = position;
        car.transform.rotation = rotation;

        float bodyHeight = width * 0.24f;
        GameObject body = CreateBox("Body", car.transform, position + Vector3.up * bodyHeight,
            new Vector3(width, bodyHeight, length), bodyMaterial);
        body.transform.rotation = rotation;
        GameObject cabin = CreateBox("Cabin", car.transform,
            position + Vector3.up * bodyHeight * 2.05f + car.transform.forward * length * 0.06f,
            new Vector3(width * 0.62f, bodyHeight * 0.8f, length * 0.48f), glassMaterial);
        cabin.transform.rotation = rotation;

        float wheelRadius = width * 0.16f;
        float wheelOffsetX = width * 0.52f;
        float wheelOffsetZ = length * 0.3f;
        CreateWheel("Wheel FL", car.transform, position, -wheelOffsetX, wheelOffsetZ, wheelRadius, tyreMaterial);
        CreateWheel("Wheel FR", car.transform, position, wheelOffsetX, wheelOffsetZ, wheelRadius, tyreMaterial);
        CreateWheel("Wheel BL", car.transform, position, -wheelOffsetX, -wheelOffsetZ, wheelRadius, tyreMaterial);
        CreateWheel("Wheel BR", car.transform, position, wheelOffsetX, -wheelOffsetZ, wheelRadius, tyreMaterial);
    }

    private static void CreateWheel(string name, Transform car, Vector3 carPosition, float x, float z, float radius, Material material)
    {
        GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        wheel.name = name;
        wheel.transform.SetParent(car, true);
        wheel.transform.position = carPosition + car.right * x + car.forward * z + Vector3.up * radius;
        wheel.transform.rotation = car.rotation * Quaternion.Euler(0f, 0f, 90f);
        wheel.transform.localScale = new Vector3(radius, radius * 0.55f, radius);
        wheel.GetComponent<Renderer>().sharedMaterial = material;
    }

    private static GameObject CreateBox(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.SetParent(parent, true);
        box.transform.position = position;
        box.transform.localScale = scale;
        box.GetComponent<Renderer>().sharedMaterial = material;
        return box;
    }

    private static Material GetOrCreateMaterial(string fileName, Color color)
    {
        if (!AssetDatabase.IsValidFolder(MaterialFolder))
        {
            AssetDatabase.CreateFolder("Assets/Materials", "Generated");
        }

        string path = MaterialFolder + "/" + fileName;
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            AssetDatabase.CreateAsset(material, path);
        }

        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        EditorUtility.SetDirty(material);
        AssetDatabase.SaveAssets();
        return material;
    }
}
