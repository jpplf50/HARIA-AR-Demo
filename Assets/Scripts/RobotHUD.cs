using UnityEngine;

public class RobotHUD : MonoBehaviour
{
    public GameObject hudCanvas; // Reference to the HUD Canvas
    public Transform robotTransform; // Reference to the robot's transform

    public GameObject radius; // Reference to the radius object
    public float maxDistance = 10f; // Max distance to detect the robot

    private Camera playerCamera;

    void Start()
    {
        playerCamera = Camera.main;
        hudCanvas.SetActive(false); // Start with the HUD hidden
        radius.SetActive(false); // Start with the radius hidden
    }

    void Update()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            if (hit.transform == robotTransform)
            {
                hudCanvas.SetActive(true); // Show HUD when looking at the robot
                radius.SetActive(true); // Start with the radius hidden
            }
            else
            {
                hudCanvas.SetActive(false); // Hide HUD when not looking at the robot
                radius.SetActive(false); // Start with the radius hidden
            }
        }
        else
        {
            hudCanvas.SetActive(false); // Hide HUD if nothing is hit
            radius.SetActive(false); // Start with the radius hidden
        }
    }
}