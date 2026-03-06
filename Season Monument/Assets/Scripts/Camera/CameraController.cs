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
    [SerializeField] private float isometricPitch = 35.264f;

    [Header("State")]
    [SerializeField] private CameraState startState = CameraState.NorthEast;

    [Header("Swipe")]
    [SerializeField] private float minSwipeDistance = 100f;

    private Vector2 touchStartPos;
    private Vector2 touchEndPos;

    private int currentState = 0;
    private float currentYaw;
    private float targetYaw;

    private void Start()
    {
        if (!target) return;

        currentState = (int)startState;
        ActivePerspective = (CameraState)currentState;
        currentYaw = targetYaw = currentState * -90f + 45f;
        UpdateCameraPosition();
    }

    private void Update()
    {
        if (!target) return;

        if (Application.isPlaying)
        {
            HandleInput();
            HandleSwipe();

            currentYaw = Mathf.LerpAngle(
                currentYaw,
                targetYaw,
                Time.deltaTime * rotationSpeed
            );
        }

        UpdateCameraPosition();
    }

    private void OnValidate()
    {
        if (!target) return;
        currentState = (int)startState;
        currentYaw = targetYaw = currentState * -90f + 45f;
        UpdateCameraPosition();
    }

    private void HandleInput()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            currentState = (currentState + 1) % 4;
            targetYaw = currentState * -90f + 45f;
<<<<<<< HEAD
            UpdateTargetHeight();
            UpdateActivePerspective();
            seasonStateManager.NextSeason();
=======
            if (seasonStateManager) seasonStateManager.NextSeason();
>>>>>>> aa89aeef36a53df168215ca016b5a227710a5f51
        }
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            currentState = (currentState + 3) % 4;
            targetYaw = currentState * -90f + 45f;
<<<<<<< HEAD
            UpdateTargetHeight();
            UpdateActivePerspective();
            seasonStateManager.PreviousSeason();
        }
    }

    void UpdateActivePerspective()
    {
        ActivePerspective = (CameraState)currentState;
    }

    void UpdateTargetHeight()
=======
            if (seasonStateManager) seasonStateManager.PreviousSeason();
        }
    }

    private void HandleSwipe()
>>>>>>> aa89aeef36a53df168215ca016b5a227710a5f51
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == UnityEngine.TouchPhase.Began)
            {
                touchStartPos = touch.position;
            }
            else if (touch.phase == UnityEngine.TouchPhase.Ended || touch.phase == UnityEngine.TouchPhase.Canceled)
            {
                touchEndPos = touch.position;
                ProcessSwipe(touchStartPos, touchEndPos);
            }

            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            touchStartPos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            touchEndPos = Input.mousePosition;
            ProcessSwipe(touchStartPos, touchEndPos);
        }
    }

    private void ProcessSwipe(Vector2 start, Vector2 end)
    {
        Vector2 swipe = end - start;

        if (swipe.magnitude < minSwipeDistance) return;
        if (Mathf.Abs(swipe.x) <= Mathf.Abs(swipe.y)) return;

        if (swipe.x > 0)
        {
            currentState = (currentState + 1) % 4;
            targetYaw = currentState * -90f + 45f;
            if (seasonStateManager) seasonStateManager.NextSeason();
        }
        else
        {
            currentState = (currentState + 3) % 4;
            targetYaw = currentState * -90f + 45f;
            if (seasonStateManager) seasonStateManager.PreviousSeason();
        }
    }

    private void UpdateCameraPosition()
    {
        Vector3 lookAtPos = target.position;

        if (heightTarget != null)
            lookAtPos.y = heightTarget.position.y + heightOffset;

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
