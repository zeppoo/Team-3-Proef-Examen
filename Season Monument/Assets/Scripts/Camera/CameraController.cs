using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

[ExecuteInEditMode]
[DefaultExecutionOrder(-200)]
public class CameraController : MonoBehaviour
{
    public enum CameraState { NorthEast, SouthEast, SouthWest, NorthWest }

    [Header("Dependencies")]
    [SerializeField] private Transform target;
    private WorldStateSwitch worldStateSwitcher;

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

    [Header("Vertical Drag")]
    [SerializeField] private float verticalDragSpeed = 0.01f;
    [SerializeField] private float minDragToMove = 2f;

    private Vector2 dragStartPos;
    private bool isDragging = false;

    private Vector2 touchStartPos;
    private Vector2 touchEndPos;

    public static CameraState ActivePerspective { get; private set; } = CameraState.NorthEast;

    private void Awake()
    {
        currentState = (int)startState;
        ActivePerspective = (CameraState)currentState;
    }

    private int currentState = 0;
    private float currentYaw;
    private float targetYaw;
    private float lockedHeightY;

    private void Start()
    {
        if (!target) return;

        currentState = (int)startState;
        ActivePerspective = (CameraState)currentState;
        currentYaw = targetYaw = currentState * -90f + 45f;
        lockedHeightY = heightTarget != null ? heightTarget.position.y : 0f;
        UpdateCameraPosition();
        worldStateSwitcher = FindFirstObjectByType<WorldStateSwitch>();
    }

    private void Update()
    {
        if (!target) return;

        if (Application.isPlaying)
        {
            HandleInput();
            HandleSwipe();
            if(worldStateSwitcher != null && worldStateSwitcher.CurrentState == WorldStateSwitch.WorldState.Gameplay)
            {
                HandleVerticalDrag();
            }
                

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
        ActivePerspective = (CameraState)currentState;
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
            ActivePerspective = (CameraState)currentState;
            lockedHeightY = heightTarget != null ? heightTarget.position.y : lockedHeightY;
            CameraEvents.RaisePerspectiveChanged(ActivePerspective);
        }
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            currentState = (currentState + 3) % 4;
            targetYaw = currentState * -90f + 45f;
            ActivePerspective = (CameraState)currentState;
            lockedHeightY = heightTarget != null ? heightTarget.position.y : lockedHeightY;
            CameraEvents.RaisePerspectiveChanged(ActivePerspective);
        }
    }

    private void HandleSwipe()
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
            ActivePerspective = (CameraState)currentState;
            lockedHeightY = heightTarget != null ? heightTarget.position.y : lockedHeightY;
            CameraEvents.RaisePerspectiveChanged(ActivePerspective);
        }
        else
        {
            currentState = (currentState + 3) % 4;
            targetYaw = currentState * -90f + 45f;
            ActivePerspective = (CameraState)currentState;
            lockedHeightY = heightTarget != null ? heightTarget.position.y : lockedHeightY;
            CameraEvents.RaisePerspectiveChanged(ActivePerspective);
        }
    }

    private void UpdateCameraPosition()
    {
        Vector3 lookAtPos = target.position;

        if (heightTarget != null)
            lookAtPos.y = lockedHeightY + heightOffset;

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



    private void HandleVerticalDrag()
    {
        // TOUCH
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == UnityEngine.TouchPhase.Began)
            {
                dragStartPos = touch.position;
                isDragging = true;
            }
            else if (touch.phase == UnityEngine.TouchPhase.Moved && isDragging)
            {
                ProcessVerticalDrag(touch.position);
            }
            else if (touch.phase == UnityEngine.TouchPhase.Ended || touch.phase == UnityEngine.TouchPhase.Canceled)
            {
                isDragging = false;
            }

            return;
        }

        // MOUSE
        if (Input.GetMouseButtonDown(0))
        {
            dragStartPos = Input.mousePosition;
            isDragging = true;
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            ProcessVerticalDrag(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    private void ProcessVerticalDrag(Vector2 currentPos)
    {
        float deltaY = currentPos.y - dragStartPos.y;

        if (Mathf.Abs(deltaY) < minDragToMove)
            return;

        ApplyVerticalMovement(deltaY);

        dragStartPos = currentPos;
    }

    private void ApplyVerticalMovement(float deltaY)
    {
        float movement = deltaY * verticalDragSpeed;

        if (heightTarget != null)
        {
            Vector3 pos = heightTarget.position;
            pos.y += movement;
            heightTarget.position = pos;
        }
        else
        {
            heightOffset += movement;
        }
    }
}
