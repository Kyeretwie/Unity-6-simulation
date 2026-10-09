using TMPro;
using UnityEngine;

public class CampusInfo : MonoBehaviour
{
    public TMP_Text infoText;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            infoText.text = "LIBRARY\n\nCentral learning facility";
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            infoText.text = "CAFETERIA\n\nMeals and social area";
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            infoText.text = "ADMINISTRATION\n\nStudent services";
        }
    }
}
