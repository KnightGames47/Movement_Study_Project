using UnityEngine;

public class FirstPerson_PlayerLook : MonoBehaviour
{
    public Camera mainCam;
    public float xSensitivity = 30f;
    public float ySensitivity = 30f;
    public float minClampX = -80f;
    public float maxClampX = 80f;

    private float xRotation = 0f;

    public void ProcessLook(Vector2 input)
    {
        xRotation -= (input.y * Time.deltaTime) * ySensitivity;
        xRotation = Mathf.Clamp(xRotation, minClampX, maxClampX);
        mainCam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);//rotates the camera up and down

        transform.Rotate(Vector3.up * (input.x * Time.deltaTime) * xSensitivity);//rotate the player to look left and right

    }
}
