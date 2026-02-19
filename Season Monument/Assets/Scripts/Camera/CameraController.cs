using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

[ExecuteInEditMode]
public class CameraController : MonoBehaviour
{
    public enum CameraState { NorthEast, SouthEast, SouthWest, NorthWest }

    [Header("Dependencies")]
    [SerializeField] private SeasonStateManager seasonStateManager;

    [SerializeField] private Transform target;

    [Header("Height")]
    [SerializeField] private Transform heightTarget;
    [SerializeField] private float heightOffset = 0f;

    [Header("Orbit Settings")]
    [SerializeField] private float distance = 5f;
    [SerializeField] private float rotationSpeed = 8f;

    [Header("Isometric Angle")]
    [SerializeField] private float isometricPitch = 35.264f; // Classic isometric angle

    [Header("State")]
    [SerializeField] private CameraState startState = CameraState.NorthEast;

    private int currentState = 0;
    private float currentYaw;
    private float targetYaw;

    private float currentHeight;
    private float targetHeight;

    void Start()
    {
        if (!target) return;

        currentState = (int)startState;
        currentYaw = targetYaw = currentState * -90f + 45f;

        float h = heightTarget != null ? heightTarget.position.y + heightOffset : target.position.y;
        currentHeight = targetHeight = h;

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

            currentHeight = Mathf.Lerp(
                currentHeight,
                targetHeight,
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
        if (Keyboard.current == null) Debug.LogWarning("No keyboard detected for camera input.");

        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            currentState = (currentState + 1) % 4;
            targetYaw = currentState * -90f + 45f;
            UpdateTargetHeight();
            seasonStateManager.NextSeason();
        }
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            currentState = (currentState + 3) % 4;
            targetYaw = currentState * -90f + 45f;
            UpdateTargetHeight();
            seasonStateManager.PreviousSeason();
        }
    }

    void UpdateTargetHeight()
    {
        if (heightTarget != null)
            targetHeight = heightTarget.position.y + heightOffset;
        else
            targetHeight = target.position.y;
    }

    void UpdateCameraPosition()
    {
        Vector3 lookAtPos = target.position;
        lookAtPos.y = currentHeight;

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