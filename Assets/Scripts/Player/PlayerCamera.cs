using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    #region External

    public Camera playerCamera;

    public float fov = 90f;
    public bool invertCamera = false;
    public bool cameraCanMove = true;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 50f;

    public bool lockCursor = true;

    #endregion

    #region Internal

    private float yaw = 0f;
    private float pitch = 0f;

    #endregion

    private void Awake() {
        playerCamera.fieldOfView = fov;
    }

    private void Update() {
        if (cameraCanMove) {
            yaw = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivity;
            if (!invertCamera) {
                pitch -= mouseSensitivity * Input.GetAxis("Mouse Y");
            } else {
                pitch += mouseSensitivity * Input.GetAxis("Mouse Y");
            }

            pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

            //Locks player to verticle aswell
            transform.localEulerAngles = new Vector3(0, yaw, 0);
            playerCamera.transform.localEulerAngles = new Vector3(pitch, 0, 0);
            yaw = 0;
        }

        if (lockCursor) {
            Cursor.lockState = CursorLockMode.Locked;
        } else {
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
