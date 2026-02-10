using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Dependencies")]
    public SeasonStateManager seasonStateManager;

    public Transform target;

    
    [Header("Orbit Settings")]
    public float distance = 5f;
    public float rotationSpeed = 8f;
    
    [Header("Isometric Angle")]
    public float isometricPitch = 35.264f; // Classic isometric angle
    
    private int currentState = 0; // 0, 1, 2, 3 (N, E, S, W)
    private float currentYaw;
    private float targetYaw;

    void Start()
    {
        if (!target)
        {
            Debug.LogError("CameraOrbit4State: No target assigned.");
            enabled = false;
            return;
        }
        
        currentYaw = targetYaw = currentState * 90f;
        UpdateCameraPosition();
    }

    void Update()
    {
        HandleInput();
        
        // Smoothly rotate towards target angle
        currentYaw = Mathf.LerpAngle(
            currentYaw,
            targetYaw,
            Time.deltaTime * rotationSpeed
        );
        
        UpdateCameraPosition();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentState = (currentState + 1) % 4;
            targetYaw = currentState * -90f;
            seasonStateManager.NextSeason();

        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentState = (currentState + 3) % 4;
            targetYaw = currentState * -90f;
            seasonStateManager.PreviousSeason();
        }
    }

    void UpdateCameraPosition()
    {
        // Calculate position using spherical coordinates
        // Yaw rotates around Y-axis (horizontal rotation)
        // Pitch tilts down from horizontal (isometric angle)
        
        float yawRad = currentYaw * Mathf.Deg2Rad;
        float pitchRad = isometricPitch * Mathf.Deg2Rad;
        
        // Spherical to Cartesian conversion
        Vector3 offset = new Vector3(
            Mathf.Sin(yawRad) * Mathf.Cos(pitchRad),
            Mathf.Sin(pitchRad),
            Mathf.Cos(yawRad) * Mathf.Cos(pitchRad)
        ) * distance;
        
        transform.position = target.position + offset;
        
        // Always look at the target
        transform.LookAt(target);
    }
}