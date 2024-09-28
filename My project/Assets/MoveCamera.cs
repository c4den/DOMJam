using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform cameraPosition;

    private void Start()
    {
        // Set the camera's initial position to the cameraPosition object
        transform.position = cameraPosition.position;
    }

    // Update is called once per frame
    private void Update()
    {
        if (cameraPosition != null)
        {
            transform.position = cameraPosition.position;
            Debug.Log("Camera moved to: " + transform.position);
        }
        else
        {
            Debug.LogError("cameraPosition is not assigned.");
        }
    }
}

