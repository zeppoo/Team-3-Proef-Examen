using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

[ExecuteInEditMode]
public class CameraController : MonoBehaviour
{
    public enum CameraState { NorthEast, SouthEast, SouthWest, NorthWest }

    [Header("Dependencies")]
    public SeasonStateManager seasonStateManager;

    public Transform target;

    [Header("Height")]
    public Transform heightTarget;
    public float heightOffset = 0f;

    [Header("Orbit Settings")]
    public float distance = 5f;
    public float rotationSpeed = 8f;

    [Header("Isometric Angle")]
    public float isometricPitch = 35.264f; // Classic isometric angle

    [Header("State")]
    public CameraState startState = CameraState.NorthEast;

    private int currentState = 0;
    private float currentYaw;
    private float targetYaw;

    void Start()
    {
        if (!target) return;

        currentState = (int)startState;
        currentYaw = targetYaw = currentState * -90f + 45f;
        UpdateCameraPosition();
    }

    void Update()
    {
        if (!target) return;

        if (Application.isPlaying)
        {
            HandleInput();

            currentYaw = Mathf.LerpAngle(
                currentYaw,
                targetYaw,
                Time.deltaTime * rotationSpeed
            );
        }

        UpdateCameraPosition();
    }

    void OnValidate()
    {
        if (!target) return;
        currentState = (int)startState;
        currentYaw = targetYaw = currentState * -90f + 45f;
        UpdateCameraPosition();
    }

    void HandleInput()
    {
        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            currentState = (currentState + 1) % 4;
            targetYaw = currentState * -90f + 45f;
            seasonStateManager.NextSeason();

        }
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            currentState = (currentState + 3) % 4;
            targetYaw = currentState * -90f + 45f;
            seasonStateManager.PreviousSeason();
        }
    }

    void UpdateCameraPosition()
    {
        // Use heightTarget's Y if assigned, otherwise use target's Y
        Vector3 lookAtPos = target.position;
        if (heightTarget != null)
        {
            lookAtPos.y = heightTarget.position.y + heightOffset;
        }

        float yawRad = currentYaw * Mathf.Deg2Rad;
        float pitchRad = isometricPitch * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(
            Mathf.Sin(yawRad) * Mathf.Cos(pitchRad),
            Mathf.Sin(pitchRad),
            Mathf.Cos(yawRad) * Mathf.Cos(pitchRad)
        ) * distance;

        transform.position = lookAtPos + offset;
        transform.LookAt(lookAtPos);
    }
}