using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class BuildingSelector : MonoBehaviour
{
    private static readonly string[] DefaultBuildingNames =
    {
        "Library",
        "Cafeteria",
        "Administration",
        "Hospital",
        "Keep Building",
        "Student Services",
        "Computing Lab"
    };

    private static readonly string[] DefaultBuildingDescriptions =
    {
        "A quiet space for study, research and access to learning resources.",
        "A central campus landmark used for academic and community activities.",
        "A shared dining and social space for students, staff and visitors.",
        "The main office for campus administration and student support services.",
        "A practical learning space for computing, digital skills and innovation.",
        "A healthcare facility for medical support and emergency care.",
        "A support centre for student welfare, guidance and essential services."
    };

    public Renderer[] buildings;
    [Tooltip("Names shown when keys 1–7 select the matching building. Leave an entry blank to use the object's name.")]
    public string[] buildingNames;
    [TextArea(2, 4), Tooltip("Information shown for the matching building. Leave an entry blank to use the built-in description.")]
    public string[] buildingDescriptions;

    [Header("Floating Building Label")]
    [Min(0.1f)] public float labelSize = 8f;
    [Min(0f)] public float labelDistance = 1f;
    public Color labelColor = Color.white;

    [Header("Camera Focus")]
    public bool focusCameraOnSelection = true;
    [Min(0.1f)] public float cameraFocusDuration = 1f;
    [Min(1f)] public float minimumCameraFocusDistance = 10f;
    [Min(0.1f)] public float cameraFocusDistanceMultiplier = 1.25f;

    private Material[] originalMaterials;
    private TextMeshPro buildingLabel;
    private TextMeshProUGUI buildingInfoText;
    private int selectedBuildingIndex = -1;
    private Camera focusedCamera;
    private CameraController cameraController;
    private bool isFocusingCamera;
    private float focusElapsedTime;
    private Vector3 focusStartPosition;
    private Vector3 focusTargetPosition;
    private Quaternion focusStartRotation;
    private Quaternion focusTargetRotation;
    private Vector3 startingCameraPosition;
    private Quaternion startingCameraRotation;
    private bool hasStartingCameraPose;

    private void Start()
    {
        originalMaterials = new Material[buildings.Length];

        for (int i = 0; i < buildings.Length; i++)
        {
            if (buildings[i] != null)
            {
                originalMaterials[i] = buildings[i].material;
            }
        }

        CreateBuildingLabel();
        CreateInformationPanel();
        CaptureStartingCameraPose();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame)
            HighlightBuilding(0);

        if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame)
            HighlightBuilding(1);

        if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame)
            HighlightBuilding(2);

        if (keyboard.digit4Key.wasPressedThisFrame || keyboard.numpad4Key.wasPressedThisFrame)
            HighlightBuilding(3);

        if (keyboard.digit5Key.wasPressedThisFrame || keyboard.numpad5Key.wasPressedThisFrame)
            HighlightBuilding(4);

        if (keyboard.digit6Key.wasPressedThisFrame || keyboard.numpad6Key.wasPressedThisFrame)
            HighlightBuilding(5);

        if (keyboard.digit7Key.wasPressedThisFrame || keyboard.numpad7Key.wasPressedThisFrame)
            HighlightBuilding(6);
    }

    private void HighlightBuilding(int index)
    {
        if (index < 0 || index >= buildings.Length || buildings[index] == null) return;

        // Pressing the selected building's number again clears the selection.
        if (selectedBuildingIndex == index)
        {
            DeselectBuilding();
            return;
        }

        RestoreOriginalMaterials();

        Material highlightMaterial = new Material(buildings[index].material)
        {
            color = Color.yellow
        };
        buildings[index].material = highlightMaterial;

        selectedBuildingIndex = index;
        UpdateBuildingLabel();
        ShowBuildingInformation(index);
        BeginCameraFocus(index);
    }

    private void DeselectBuilding()
    {
        RestoreOriginalMaterials();
        selectedBuildingIndex = -1;

        if (buildingLabel != null)
        {
            buildingLabel.gameObject.SetActive(false);
        }

        if (buildingInfoText != null)
        {
            buildingInfoText.transform.parent.gameObject.SetActive(false);
        }

        ReturnCameraToStartingPosition();
    }

    private void RestoreOriginalMaterials()
    {
        for (int i = 0; i < buildings.Length; i++)
        {
            if (buildings[i] != null && originalMaterials[i] != null)
            {
                buildings[i].material = originalMaterials[i];
            }
        }
    }

    private void LateUpdate()
    {
        if (selectedBuildingIndex >= 0)
        {
            UpdateBuildingLabel();
        }

        UpdateCameraFocus();
    }

    private void CreateBuildingLabel()
    {
        GameObject labelObject = new GameObject("Selected Building Label");
        labelObject.transform.SetParent(transform);

        buildingLabel = labelObject.AddComponent<TextMeshPro>();
        buildingLabel.alignment = TextAlignmentOptions.Center;
        buildingLabel.fontSize = labelSize;
        buildingLabel.color = labelColor;
        buildingLabel.textWrappingMode = TextWrappingModes.NoWrap;
        labelObject.SetActive(false);
    }

    private void CreateInformationPanel()
    {
        GameObject canvasObject = new GameObject("Building Information Canvas", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        GameObject panelObject = new GameObject("Building Information Panel", typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(canvasObject.transform, false);
        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0f, 45f);
        panelRect.sizeDelta = new Vector2(850f, 160f);
        panelObject.GetComponent<Image>().color = new Color(0.02f, 0.08f, 0.12f, 0.82f);

        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(panelObject.transform, false);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(25f, 16f);
        textRect.offsetMax = new Vector2(-25f, -16f);

        buildingInfoText = textObject.GetComponent<TextMeshProUGUI>();
        buildingInfoText.alignment = TextAlignmentOptions.Center;
        buildingInfoText.fontSize = 28f;
        buildingInfoText.color = Color.white;
        buildingInfoText.textWrappingMode = TextWrappingModes.Normal;
        panelObject.SetActive(false);
    }

    private void UpdateBuildingLabel()
    {
        if (buildingLabel == null || selectedBuildingIndex >= buildings.Length ||
            buildings[selectedBuildingIndex] == null)
        {
            return;
        }

        Renderer selectedBuilding = buildings[selectedBuildingIndex];
        Camera playerCamera = Camera.main;
        Bounds bounds = selectedBuilding.bounds;
        Vector3 directionToCamera = playerCamera != null
            ? (playerCamera.transform.position - bounds.center).normalized
            : Vector3.forward;

        buildingLabel.text = GetBuildingName(selectedBuildingIndex);
        buildingLabel.transform.position = bounds.center +
                                           directionToCamera * (bounds.extents.magnitude + labelDistance);

        if (playerCamera != null)
        {
            buildingLabel.transform.LookAt(playerCamera.transform);
            // TextMeshPro's readable face is opposite its forward direction.
            buildingLabel.transform.Rotate(0f, 180f, 0f, Space.Self);
        }

        buildingLabel.gameObject.SetActive(true);
    }

    private void ShowBuildingInformation(int index)
    {
        if (buildingInfoText == null) return;

        buildingInfoText.text = "<b>" + GetBuildingName(index) + "</b>\n" + GetBuildingDescription(index);
        buildingInfoText.transform.parent.gameObject.SetActive(true);
    }

    private void BeginCameraFocus(int index)
    {
        if (!focusCameraOnSelection || Camera.main == null) return;

        focusedCamera = Camera.main;
        cameraController = focusedCamera.GetComponent<CameraController>();
        cameraController?.SetInputLocked(true);

        Bounds bounds = buildings[index].bounds;
        Vector3 horizontalDirection = focusedCamera.transform.position - bounds.center;
        horizontalDirection.y = 0f;
        if (horizontalDirection.sqrMagnitude < 0.01f)
        {
            horizontalDirection = -buildings[index].transform.forward;
            horizontalDirection.y = 0f;
        }
        horizontalDirection.Normalize();

        float distance = Mathf.Max(bounds.extents.magnitude * cameraFocusDistanceMultiplier, minimumCameraFocusDistance);
        focusStartPosition = focusedCamera.transform.position;
        focusStartRotation = focusedCamera.transform.rotation;
        focusTargetPosition = bounds.center + horizontalDirection * distance;
        focusTargetPosition.y = bounds.center.y + Mathf.Max(bounds.extents.y * 0.35f, 2f);

        Vector3 lookTarget = bounds.center + Vector3.up * bounds.extents.y * 0.15f;
        focusTargetRotation = Quaternion.LookRotation(lookTarget - focusTargetPosition, Vector3.up);
        focusElapsedTime = 0f;
        isFocusingCamera = true;
    }

    private void CaptureStartingCameraPose()
    {
        Camera playerCamera = Camera.main;
        if (playerCamera == null) return;

        CameraController controller = playerCamera.GetComponent<CameraController>();
        if (controller != null && controller.HasStartingPose)
        {
            startingCameraPosition = controller.StartingPosition;
            startingCameraRotation = controller.StartingRotation;
        }
        else
        {
            startingCameraPosition = playerCamera.transform.position;
            startingCameraRotation = playerCamera.transform.rotation;
        }
        hasStartingCameraPose = true;
    }

    private void ReturnCameraToStartingPosition()
    {
        if (Camera.main == null) return;

        focusedCamera = Camera.main;
        cameraController = focusedCamera.GetComponent<CameraController>();
        if (cameraController != null && cameraController.HasStartingPose)
        {
            startingCameraPosition = cameraController.StartingPosition;
            startingCameraRotation = cameraController.StartingRotation;
            hasStartingCameraPose = true;
        }

        if (!hasStartingCameraPose) return;
        cameraController?.SetInputLocked(true);

        focusStartPosition = focusedCamera.transform.position;
        focusStartRotation = focusedCamera.transform.rotation;
        focusTargetPosition = startingCameraPosition;
        focusTargetRotation = startingCameraRotation;
        focusElapsedTime = 0f;
        isFocusingCamera = true;
    }

    private void UpdateCameraFocus()
    {
        if (!isFocusingCamera || focusedCamera == null) return;

        focusElapsedTime += Time.deltaTime;
        float progress = Mathf.Clamp01(focusElapsedTime / cameraFocusDuration);
        float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
        focusedCamera.transform.SetPositionAndRotation(
            Vector3.Lerp(focusStartPosition, focusTargetPosition, easedProgress),
            Quaternion.Slerp(focusStartRotation, focusTargetRotation, easedProgress));

        if (progress >= 1f)
        {
            isFocusingCamera = false;
            cameraController?.SetViewRotation(focusTargetRotation);
            cameraController?.SetInputLocked(false);
        }
    }

    private string GetBuildingName(int index)
    {
        if (buildingNames != null && index < buildingNames.Length &&
            !string.IsNullOrWhiteSpace(buildingNames[index]))
        {
            return buildingNames[index];
        }

        if (index < DefaultBuildingNames.Length)
        {
            return DefaultBuildingNames[index];
        }

        return buildings[index].gameObject.name;
    }

    private string GetBuildingDescription(int index)
    {
        if (buildingDescriptions != null && index < buildingDescriptions.Length &&
            !string.IsNullOrWhiteSpace(buildingDescriptions[index]))
        {
            return buildingDescriptions[index];
        }

        if (index < DefaultBuildingDescriptions.Length)
        {
            return DefaultBuildingDescriptions[index];
        }

        return "Campus building information is not available yet.";
    }
}
