using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Keyboard Movement")]
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private InputActionReference moveAction;

    [Header("Mouse Drag")]
    [SerializeField] private InputActionReference dragAction;
    [SerializeField] private InputActionReference pointerPositionAction;

    [Header("Zoom")]
    [SerializeField] private InputActionReference zoomAction;
    [SerializeField] private float zoomSpeed = 0.15f;
    [SerializeField] private float minZoom = 3f;

    [Header("Bounds")]
    [SerializeField] private GameObject background;

    [Header("Startup")]
    [SerializeField] private float startupZoomMultiplier = 0.95f;

    private Camera _cam;
    private BoxCollider2D _backgroundCollider;

    public bool blockScroll = false;

    private Vector2 _lastPointerPosition;
    private bool _wasDraggingLastFrame;
    private float _maxZoom;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        if (_cam == null)
        {
            _cam = Camera.main;
        }

        if (background != null)
        {
            _backgroundCollider = background.GetComponent<BoxCollider2D>();
        }
    }

    private void Start()
    {
        RefreshBackgroundCollider();
        CenterCameraAndSetInitialZoom();
        ClampToBackground();
    }

    private void OnEnable()
    {
        moveAction?.action.Enable();
        dragAction?.action.Enable();
        pointerPositionAction?.action.Enable();
        zoomAction?.action.Enable();
    }

    private void OnDisable()
    {
        moveAction?.action.Disable();
        dragAction?.action.Disable();
        pointerPositionAction?.action.Disable();
        zoomAction?.action.Disable();
    }

    private void Update()
    {
        if (blockScroll)
        {
            return;
        }

        HandleKeyboardMovement();
        HandleMouseDrag();
        HandleZoom();
        ClampToBackground();
    }

    private void RefreshBackgroundCollider()
    {
        if (background == null)
        {
            Debug.LogWarning("CameraController: Background is not assigned.");
            return;
        }

        _backgroundCollider = background.GetComponent<BoxCollider2D>();

        if (_backgroundCollider == null)
        {
            Debug.LogWarning($"CameraController: No BoxCollider2D found on {background.name}.");
        }
    }

    private void CenterCameraAndSetInitialZoom()
    {
        if (_cam == null || _backgroundCollider == null || !_cam.orthographic)
        {
            return;
        }

        Bounds bounds = _backgroundCollider.bounds;

        transform.position = new Vector3(
            bounds.center.x,
            bounds.center.y,
            transform.position.z
        );

        float verticalSize = bounds.size.y * 0.5f;
        float horizontalSize = bounds.size.x / (2f * _cam.aspect);

        // Maximum zoom that still stays fully inside the background.
        _maxZoom = Mathf.Min(verticalSize, horizontalSize);

        // Start slightly zoomed in so no border is visible.
        float startupZoom = _maxZoom * startupZoomMultiplier;
        _cam.orthographicSize = Mathf.Clamp(startupZoom, minZoom, _maxZoom);
    }

    private void HandleKeyboardMovement()
    {
        if (moveAction == null)
        {
            return;
        }

        Vector2 input = moveAction.action.ReadValue<Vector2>();
        Vector3 moveDirection = new Vector3(input.x, input.y, 0f).normalized;

        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    private void HandleMouseDrag()
    {
        if (_cam == null || dragAction == null || pointerPositionAction == null)
        {
            return;
        }

        bool isDragging = dragAction.action.IsPressed();
        Vector2 currentPointerPosition = pointerPositionAction.action.ReadValue<Vector2>();

        if (isDragging)
        {
            if (_wasDraggingLastFrame)
            {
                Vector3 lastWorld = _cam.ScreenToWorldPoint(
                    new Vector3(_lastPointerPosition.x, _lastPointerPosition.y, -_cam.transform.position.z)
                );

                Vector3 currentWorld = _cam.ScreenToWorldPoint(
                    new Vector3(currentPointerPosition.x, currentPointerPosition.y, -_cam.transform.position.z)
                );

                Vector3 delta = lastWorld - currentWorld;
                delta.z = 0f;

                transform.position += delta;
            }

            _lastPointerPosition = currentPointerPosition;
        }

        _wasDraggingLastFrame = isDragging;
    }

    private void HandleZoom()
    {
        if (_cam == null || zoomAction == null || !_cam.orthographic)
        {
            return;
        }

        Vector2 scroll = zoomAction.action.ReadValue<Vector2>();
        if (Mathf.Abs(scroll.y) < 0.01f)
        {
            return;
        }

        Vector2 mouseScreen = pointerPositionAction != null
            ? pointerPositionAction.action.ReadValue<Vector2>()
            : new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        Vector3 beforeZoomWorld = _cam.ScreenToWorldPoint(
            new Vector3(mouseScreen.x, mouseScreen.y, -_cam.transform.position.z)
        );

        float newZoom = _cam.orthographicSize * (1f - (scroll.y * zoomSpeed * Time.deltaTime));
        _cam.orthographicSize = Mathf.Clamp(newZoom, minZoom, _maxZoom);

        Vector3 afterZoomWorld = _cam.ScreenToWorldPoint(
            new Vector3(mouseScreen.x, mouseScreen.y, -_cam.transform.position.z)
        );

        Vector3 worldDelta = beforeZoomWorld - afterZoomWorld;
        worldDelta.z = 0f;
        transform.position += worldDelta;
    }

    private void ClampToBackground()
    {
        if (_cam == null || _backgroundCollider == null || !_cam.orthographic)
        {
            return;
        }

        Bounds b = _backgroundCollider.bounds;

        float camHalfHeight = _cam.orthographicSize;
        float camHalfWidth = camHalfHeight * _cam.aspect;

        float minX = b.min.x + camHalfWidth;
        float maxX = b.max.x - camHalfWidth;
        float minY = b.min.y + camHalfHeight;
        float maxY = b.max.y - camHalfHeight;

        Vector3 pos = transform.position;
        pos.x = minX > maxX ? b.center.x : Mathf.Clamp(pos.x, minX, maxX);
        pos.y = minY > maxY ? b.center.y : Mathf.Clamp(pos.y, minY, maxY);

        transform.position = pos;
    }
}