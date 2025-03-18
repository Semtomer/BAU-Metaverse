
using System.Collections;
using UnityEngine;

public class FPSCamera : MonoBehaviour
{
    [SerializeField] float sensivityX, sensivityY;

    float xRotation, yRotation;
    float tempYRotation;

    Transform playerBody, playerHead;

    bool canSit;

    [HideInInspector] public static bool isStartingToSit = false;

    [HideInInspector] public static bool isSwitchedFPS;

    void Start()
    {
        playerBody = GameObject.FindWithTag("Player").transform;
        playerHead = GameObject.FindWithTag("Head").transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; 
    }

    void Update()
    {
        GetComponentInParent<Transform>().position = playerHead.position;

        float mouseX = Input.GetAxisRaw("Mouse X") * sensivityX * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensivityY * Time.deltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -75, 75);

        if (!PlayerController.isPaused && !PlayerController.isChatting)
        {
            if (PlayerController.runningAnimation != "Grabbing" && PlayerController.runningAnimation != "Sitting" && PlayerController.runningAnimation != "Piano" && !PlayerController.isSitting)
            {
                if (isSwitchedFPS)
                {
                    yRotation = playerBody.rotation.eulerAngles.y;
                    isSwitchedFPS = false;
                }

                canSit = true;

                transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
                playerBody.rotation = Quaternion.Euler(0f, yRotation, 0f);
            }
            else if (PlayerController.runningAnimation == "Piano" || PlayerController.PianoToSit != null)
            {
                RotationMacro(PlayerController.PianoToSit.transform);
            }
            else if (PlayerController.runningAnimation == "Sitting" || PlayerController.objectWantToSit != null)
            {
                RotationMacro(PlayerController.objectWantToSit.transform);
            }
        }       
    }

    void RotationMacro(Transform objectTransform) 
    {
        if (isStartingToSit)
        {
            tempYRotation = objectTransform.rotation.eulerAngles.y;
            yRotation = objectTransform.rotation.eulerAngles.y;

            isStartingToSit = false;

            playerBody.eulerAngles = new Vector3(0, objectTransform.transform.rotation.eulerAngles.y, 0);
        }

        if ((objectTransform.transform.rotation.eulerAngles.y > 90 && objectTransform.transform.rotation.eulerAngles.y <= 180) ||
            (objectTransform.transform.rotation.eulerAngles.y < -90 && objectTransform.transform.rotation.eulerAngles.y >= -180))
        {
            if (yRotation < 0f)
                yRotation += 360f;

            yRotation = Mathf.Clamp(yRotation, tempYRotation - 90f, tempYRotation + 90f);

            if (yRotation > 180)
                yRotation -= 360;
        }
        else 
        {
            yRotation = Mathf.Clamp(yRotation, tempYRotation - 90f, tempYRotation + 90f);
        }

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);

        StartCoroutine(waitForSittingComplete());
    }

    IEnumerator waitForSittingComplete()
    {
        if (canSit)
        {
            canSit = false;

            GetComponent<Camera>().nearClipPlane = 1.25f;

            yield return new WaitForSeconds(1);

            GetComponent<Camera>().nearClipPlane = 0.3f;        
        } 
    }
}
