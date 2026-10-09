using UnityEngine;

public class HighlightBuilding : MonoBehaviour
{
    private Renderer buildingRenderer;
    private Color originalColor;

    private void Start()
    {
        buildingRenderer = GetComponent<Renderer>();
        originalColor = buildingRenderer.material.color;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            buildingRenderer.material.color = Color.yellow;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            buildingRenderer.material.color = originalColor;
        }
    }
}
