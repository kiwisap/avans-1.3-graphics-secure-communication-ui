using UnityEngine;
using System.Collections.Generic;

public class EnvironmentEditorController : MonoBehaviour
{
    [Header("Settings")]
    public float gridSize = 1f;
    public bool snapToGrid = true;

    private Sprite selectedSprite;
    private GameObject previewObject;
    private List<GameObject> placedObjects = new List<GameObject>();
    private GameObject selectedObject;

    private Camera cam;
    private bool justSelected = false; // Prevents placing on the same frame you click the palette

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        Vector2 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);

        if (previewObject != null)
        {
            Vector2 snapped = snapToGrid ? SnapToGrid(mouseWorld) : mouseWorld;
            previewObject.transform.position = snapped;

            // Skip placement on the first frame after selecting from palette
            if (justSelected)
            {
                justSelected = false;
                return;
            }

            // Place on left click (not over UI)
            if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
            {
                PlaceObject(snapped);
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
            if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
            {
                TrySelectObject(mouseWorld);
            }

            if (selectedObject != null && Input.GetKeyDown(KeyCode.Delete))
            {
                placedObjects.Remove(selectedObject);
                Destroy(selectedObject);
                selectedObject = null;
            }

            if (selectedObject != null && Input.GetKeyDown(KeyCode.R))
            {
                selectedObject.transform.Rotate(0, 0, 90);
            }

            if (selectedObject != null && Input.GetKeyDown(KeyCode.F))
            {
                Vector3 scale = selectedObject.transform.localScale;
                scale.x *= -1;
                selectedObject.transform.localScale = scale;
            }

            // Scroll to scale selected object
            if (selectedObject != null)
            {
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (scroll != 0f)
                {
                    float scaleDelta = scroll * 0.5f; // Adjust 0.5f to control scroll sensitivity
                    Vector3 s = selectedObject.transform.localScale;
                    float newX = Mathf.Clamp(Mathf.Abs(s.x) + scaleDelta, 0.05f, 5f) * Mathf.Sign(s.x);
                    float newY = Mathf.Clamp(s.y + scaleDelta, 0.05f, 5f);
                    selectedObject.transform.localScale = new Vector3(newX, newY, 1f);
                }
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
        previewObject.transform.localScale = new Vector3(0.3f, 0.3f, 1f); // Match default size

        justSelected = true;
    }

    void PlaceObject(Vector2 position)
    {
        Sprite spriteToBePlaced = selectedSprite;

        GameObject obj = new GameObject(spriteToBePlaced.name);
        obj.transform.position = position;
        obj.transform.rotation = previewObject.transform.rotation;
        obj.transform.localScale = new Vector3(0.3f, 0.3f, 1f); // Small default size

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

        Collider2D hit = Physics2D.OverlapPoint(position);
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

    Vector2 SnapToGrid(Vector2 pos)
    {
        float x = Mathf.Round(pos.x / gridSize) * gridSize;
        float y = Mathf.Round(pos.y / gridSize) * gridSize;
        return new Vector2(x, y);
    }

    bool IsPointerOverUI()
    {
        return UnityEngine.EventSystems.EventSystem.current != null &&
               UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
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