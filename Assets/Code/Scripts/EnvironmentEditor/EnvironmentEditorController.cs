using Assets.Code.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentEditorController : MonoBehaviour
{
    [Header("Camera")]
    public Camera cam;
    private CameraController cameraController;

    private Sprite selectedSprite;
    private GameObject previewObject;
    private List<GameObject> placedObjects = new List<GameObject>();
    private GameObject selectedObject;

    private bool justSelected = false;
    private bool isDragging = false;
    private Vector2 dragOffset;

    private float worldScale;

    private Environment2DDto currentEnvironment;

    void Start()
    {
        cam = Camera.main;
        cameraController = cam.GetComponent<CameraController>();

        currentEnvironment = new Environment2DDto
        {
            Id = PlayerPrefs.GetInt("Environment_Id", 0),
            Name = PlayerPrefs.GetString("Environment_Name", "New Environment"),
            MaxHeight = PlayerPrefs.GetInt("Environment_MaxHeight", 20),
            MaxLength = PlayerPrefs.GetInt("Environment_MaxLength", 10)
        };

        worldScale = 3f / currentEnvironment.MaxLength;

        StartCoroutine(SetupBoundaryNextFrame());
    }

    void Update()
    {
        Vector2 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);

        if (previewObject != null)
        {
            previewObject.transform.position = mouseWorld;

            // Skip placement on the first frame after selecting from palette
            if (justSelected)
            {
                justSelected = false;
                return;
            }

            // Place on left click (not over UI)
            if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
            {
                PlaceObject(mouseWorld);
                // Keep the same sprite selected so you can place multiple
                // If you want to stop after one placement, call CancelPlacement() instead
            }

            // Cancel with right click or Escape
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                CancelPlacement();
            }
        }
        else
        {
            // Select object on left click (not over UI)
            if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
            {
                TrySelectObject(mouseWorld);
                if (selectedObject != null)
                {
                    isDragging = true;
                    dragOffset = (Vector2)selectedObject.transform.position - mouseWorld;
                }
            }

            // Drag selected object
            if (isDragging && selectedObject != null)
            {
                float halfW = (currentEnvironment.MaxLength * worldScale) / 2f;
                float halfH = (currentEnvironment.MaxHeight * worldScale) / 2f;
                Vector2 center = cam.transform.position;
                Vector2 newPos = mouseWorld + dragOffset;
                newPos.x = Mathf.Clamp(newPos.x, center.x - halfW, center.x + halfW);
                newPos.y = Mathf.Clamp(newPos.y, center.y - halfH, center.y + halfH);
                selectedObject.transform.position = newPos;

                if (Input.GetMouseButtonUp(0))
                {
                    isDragging = false;
                }
            }

            // Deselect with right click
            if (Input.GetMouseButtonDown(1) && selectedObject != null)
            {
                SetObjectHighlight(selectedObject, false);
                selectedObject = null;
                isDragging = false;
            }

            // Delete with Delete key
            if (selectedObject != null && Input.GetKeyDown(KeyCode.Delete))
            {
                placedObjects.Remove(selectedObject);
                Destroy(selectedObject);
                selectedObject = null;
                isDragging = false;
            }

            // Rotate with R key
            if (selectedObject != null && Input.GetKeyDown(KeyCode.R))
            {
                selectedObject.transform.Rotate(0, 0, 90);
            }

            // Flip with F key
            if (selectedObject != null && Input.GetKeyDown(KeyCode.F))
            {
                Vector3 scale = selectedObject.transform.localScale;
                scale.x *= -1;
                selectedObject.transform.localScale = scale;
            }

            // Scale with mouse wheel
            if (selectedObject != null)
            {
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (scroll != 0f)
                {
                    if (cameraController != null) cameraController.blockScroll = true;

                    float scaleDelta = scroll * 0.5f;
                    Vector3 s = selectedObject.transform.localScale;
                    float newX = Mathf.Clamp(Mathf.Abs(s.x) + scaleDelta, 0.05f, 0.5f) * Mathf.Sign(s.x);
                    float newY = Mathf.Clamp(s.y + scaleDelta, 0.05f, 0.5f);
                    selectedObject.transform.localScale = new Vector3(newX, newY, 1f);
                }
                else
                {
                    if (cameraController != null) cameraController.blockScroll = false;
                }
            }
            else
            {
                if (cameraController != null) cameraController.blockScroll = false;
            }
        }
    }

    public void SelectSprite(Sprite sprite)
    {
        CancelPlacement();
        selectedSprite = sprite;

        previewObject = new GameObject("Preview_" + sprite.name);
        SpriteRenderer sr = previewObject.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = new Color(1, 1, 1, 0.5f);
        sr.sortingOrder = 10;
        previewObject.transform.localScale = new Vector3(0.1f, 0.1f, 1f);

        justSelected = true;
    }

    void PlaceObject(Vector2 position)
    {
        float halfW = (currentEnvironment.MaxLength * worldScale) / 2f;
        float halfH = (currentEnvironment.MaxHeight * worldScale) / 2f;
        Vector2 center = cam.transform.position;
        position.x = Mathf.Clamp(position.x, center.x - halfW, center.x + halfW);
        position.y = Mathf.Clamp(position.y, center.y - halfH, center.y + halfH);

        Sprite spriteToBePlaced = selectedSprite;

        GameObject obj = new GameObject(spriteToBePlaced.name);
        obj.transform.position = position;
        obj.transform.rotation = previewObject.transform.rotation;
        obj.transform.localScale = new Vector3(0.1f, 0.1f, 1f);

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = spriteToBePlaced;
        sr.sortingOrder = 1;

        obj.AddComponent<BoxCollider2D>();

        PlaceableObject po = obj.AddComponent<PlaceableObject>();
        po.objectId = spriteToBePlaced.name;

        placedObjects.Add(obj);
    }

    void TrySelectObject(Vector2 position)
    {
        if (selectedObject != null)
            SetObjectHighlight(selectedObject, false);

        Collider2D hit = Physics2D.OverlapCircle(position, 0.05f);
        if (hit != null && hit.GetComponent<PlaceableObject>() != null)
        {
            selectedObject = hit.gameObject;
            SetObjectHighlight(selectedObject, true);
        }
        else
        {
            selectedObject = null;
        }
    }

    void SetObjectHighlight(GameObject obj, bool highlight)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = highlight ? new Color(0.7f, 1f, 0.7f, 1f) : Color.white;
    }

    void CancelPlacement()
    {
        if (previewObject != null)
            Destroy(previewObject);
        previewObject = null;
        selectedSprite = null;
    }

    bool IsPointerOverUI()
    {
        return UnityEngine.EventSystems.EventSystem.current != null &&
               UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }


    IEnumerator SetupBoundaryNextFrame()
    {
        yield return null; // Wait one frame so camera is fully initialized
        SetupBoundary();
    }

    void SetupBoundary()
    {
        float halfW = (currentEnvironment.MaxLength * worldScale) / 2f;
        float halfH = (currentEnvironment.MaxHeight * worldScale) / 2f;
        Vector2 center = cam.transform.position;

        CreateBorderLine("Border_Bottom",
            new Vector2(center.x - halfW, center.y - halfH),
            new Vector2(center.x + halfW, center.y - halfH));
        CreateBorderLine("Border_Top",
            new Vector2(center.x - halfW, center.y + halfH),
            new Vector2(center.x + halfW, center.y + halfH));
        CreateBorderLine("Border_Left",
            new Vector2(center.x - halfW, center.y - halfH),
            new Vector2(center.x - halfW, center.y + halfH));
        CreateBorderLine("Border_Right",
            new Vector2(center.x + halfW, center.y - halfH),
            new Vector2(center.x + halfW, center.y + halfH));
    }

    void CreateBorderLine(string name, Vector2 start, Vector2 end)
    {
        GameObject obj = new GameObject(name);

        // Use a thin quad scaled to form a line instead of LineRenderer
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = CreateWhiteSprite();
        sr.color = Color.red;
        sr.sortingOrder = 99;

        // Position at midpoint between start and end
        Vector2 mid = (start + end) / 2f;
        obj.transform.position = new Vector3(mid.x, mid.y, 0f);

        // Scale to match the line length and a fixed thickness
        float length = Vector2.Distance(start, end);
        bool isHorizontal = Mathf.Abs(end.y - start.y) < 0.01f;
        obj.transform.localScale = isHorizontal
            ? new Vector3(length, 0.02f, 1f)
            : new Vector3(0.02f, length, 1f);
    }

    Sprite CreateWhiteSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    public void RotateSelected()
    {
        if (selectedObject)
        {
            selectedObject.transform.Rotate(0, 0, 90);
        }
    }
    public void FlipSelected()
    {
        if (selectedObject)
        {
            var s = selectedObject.transform.localScale; s.x *= -1;
            selectedObject.transform.localScale = s;
        }
    }

    public void DeleteSelected()
    {
        if (selectedObject)
        {
            placedObjects.Remove(selectedObject);
            Destroy(selectedObject);
            selectedObject = null;
        }
    }

    public List<GameObject> GetPlacedObjects() => placedObjects;
}