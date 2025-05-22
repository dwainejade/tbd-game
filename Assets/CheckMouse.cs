using UnityEngine;

public class RightClickTest : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) // Right mouse button
        {
            Debug.Log("Key E pressed");
        }
    }
}