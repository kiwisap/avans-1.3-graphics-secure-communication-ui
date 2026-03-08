using Assets.Code.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Assets.Code.Scripts.EnvironmentEditor
{
    public class EnvironmentEditorController : MonoBehaviour
    {
        [Header("References")]
        public Camera cam;
        public ObjectPalette palette;
        public EnvironmentSaveLoad saveLoad;

        [Header("Settings")]
        public float defaultScale = 0.1f;
        public float scrollSensitivity = 0.5f;
        public float minScale = 0.05f;
        public float maxScale = 0.5f;

        private CameraController cameraController;
        private Environment2DDto currentEnvironment;
        private float worldScale;

        private Sprite selectedSprite;
        private int selectedPrefabId;
        private GameObject previewObject;
        private List<GameObject> placedObjects = new List<GameObject>();

        private GameObject selectedObject;
        private bool isDragging = false;
        private Vector2 dragOffset;
        private bool justSelected = false;

        // Lifecycle

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

            StartCoroutine(InitializeAsync());
        }

        void Update()
        {
            Vector2 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);

            if (previewObject != null)
                HandlePlacementMode(mouseWorld);
            else
                HandleSelectionMode(mouseWorld);
        }

        // Placement

        void HandlePlacementMode(Vector2 mouseWorld)
        {
            previewObject.transform.position = mouseWorld;

            if (justSelected)
            {
                justSelected = false;
                return;
            }

            if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
                PlaceObject(mouseWorld);

            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
                CancelPlacement();
        }

        // Selection

        void HandleSelectionMode(Vector2 mouseWorld)
        {
            // Left click: select + start drag
            if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
            {
                // Debug: show all colliders in range
                Collider2D[] hits = Physics2D.OverlapCircleAll(mouseWorld, 0.15f);
                Debug.Log($"Clicks at {mouseWorld}, found {hits.Length} colliders");
                foreach (var h in hits)
                    Debug.Log($"  - {h.gameObject.name}, has PlaceableObject: {h.GetComponent<PlaceableObject>() != null}");

                TrySelectObject(mouseWorld);
                if (selectedObject != null)
                {
                    isDragging = true;
                    dragOffset = (Vector2)selectedObject.transform.position - mouseWorld;
                }
            }

            // Drag
            if (isDragging && selectedObject != null)
            {
                selectedObject.transform.position = ClampToBoundary(mouseWorld + dragOffset);

                if (Input.GetMouseButtonUp(0))
                    isDragging = false;
            }

            // Right click: deselect
            if (Input.GetMouseButtonDown(1) && selectedObject != null)
                Deselect();

            if (selectedObject == null) return;

            // Delete
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                placedObjects.Remove(selectedObject);
                Destroy(selectedObject);
                selectedObject = null;
                isDragging = false;
                return;
            }

            // Rotate
            if (Input.GetKeyDown(KeyCode.R))
                selectedObject.transform.Rotate(0, 0, 90);

            // Flip
            if (Input.GetKeyDown(KeyCode.F))
            {
                Vector3 s = selectedObject.transform.localScale;
                s.x *= -1;
                selectedObject.transform.localScale = s;
            }

            // Scroll to scale
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (cameraController != null)
                cameraController.blockScroll = scroll != 0f;

            if (scroll != 0f)
            {
                Vector3 s = selectedObject.transform.localScale;
                float delta = scroll * scrollSensitivity;
                float newX = Mathf.Clamp(Mathf.Abs(s.x) + delta, minScale, maxScale) * Mathf.Sign(s.x);
                float newY = Mathf.Clamp(s.y + delta, minScale, maxScale);
                selectedObject.transform.localScale = new Vector3(newX, newY, 1f);
            }
        }

        bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        // Object

        public void SelectSprite(Sprite sprite, int prefabId)
        {
            CancelPlacement();
            selectedSprite = sprite;
            selectedPrefabId = prefabId;

            previewObject = new GameObject("Preview_" + sprite.name);
            SpriteRenderer sr = previewObject.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(1f, 1f, 1f, 0.5f);
            sr.sortingOrder = 10;
            previewObject.transform.localScale = new Vector3(defaultScale, defaultScale, 1f);

            justSelected = true;
        }

        void PlaceObject(Vector2 position)
        {
            position = ClampToBoundary(position);

            GameObject obj = new GameObject(selectedSprite.name);
            obj.transform.position = position;
            obj.transform.rotation = previewObject.transform.rotation;
            obj.transform.localScale = new Vector3(defaultScale, defaultScale, 1f);

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = selectedSprite;
            sr.sortingOrder = 1;

            // Size collider to match sprite so clicks always register
            obj.AddComponent<BoxCollider2D>();

            PlaceableObject po = obj.AddComponent<PlaceableObject>();
            po.objectId = selectedSprite.name;
            po.prefabId = selectedPrefabId;

            placedObjects.Add(obj);
        }

        void TrySelectObject(Vector2 position)
        {
            Deselect();

            Collider2D[] hits = Physics2D.OverlapCircleAll(position, 0.15f);
            GameObject best = null;
            int bestOrder = int.MinValue;

            foreach (Collider2D hit in hits)
            {
                if (hit.GetComponent<PlaceableObject>() == null) continue;
                SpriteRenderer sr = hit.GetComponent<SpriteRenderer>();
                int order = sr != null ? sr.sortingOrder : 0;
                if (order > bestOrder)
                {
                    bestOrder = order;
                    best = hit.gameObject;
                }
            }

            if (best != null)
            {
                selectedObject = best;
                SetHighlight(selectedObject, true);
            }
        }

        void Deselect()
        {
            if (selectedObject != null)
                SetHighlight(selectedObject, false);
            selectedObject = null;
            isDragging = false;
            if (cameraController != null) cameraController.blockScroll = false;
        }

        void CancelPlacement()
        {
            if (previewObject != null)
                Destroy(previewObject);
            previewObject = null;
            selectedSprite = null;
        }

        void SetHighlight(GameObject obj, bool on)
        {
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = on ? new Color(0.7f, 1f, 0.7f, 1f) : Color.white;
        }

        // Boundary

        Vector2 ClampToBoundary(Vector2 pos)
        {
            Vector2 center = cam.transform.position;
            float halfW = (currentEnvironment.MaxLength * worldScale) / 2f;
            float halfH = (currentEnvironment.MaxHeight * worldScale) / 2f;
            pos.x = Mathf.Clamp(pos.x, center.x - halfW, center.x + halfW);
            pos.y = Mathf.Clamp(pos.y, center.y - halfH, center.y + halfH);
            return pos;
        }

        void SetupBoundary()
        {
            float halfW = (currentEnvironment.MaxLength * worldScale) / 2f;
            float halfH = (currentEnvironment.MaxHeight * worldScale) / 2f;
            Vector2 c = cam.transform.position;

            CreateBorderLine("Border_Bottom", new Vector2(c.x - halfW, c.y - halfH), new Vector2(c.x + halfW, c.y - halfH));
            CreateBorderLine("Border_Top", new Vector2(c.x - halfW, c.y + halfH), new Vector2(c.x + halfW, c.y + halfH));
            CreateBorderLine("Border_Left", new Vector2(c.x - halfW, c.y - halfH), new Vector2(c.x - halfW, c.y + halfH));
            CreateBorderLine("Border_Right", new Vector2(c.x + halfW, c.y - halfH), new Vector2(c.x + halfW, c.y + halfH));
        }

        void CreateBorderLine(string name, Vector2 start, Vector2 end)
        {
            GameObject obj = new GameObject(name);
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = CreateWhiteSprite();
            sr.color = Color.red;
            sr.sortingOrder = 99;

            Vector2 mid = (start + end) / 2f;
            obj.transform.position = new Vector3(mid.x, mid.y, 0f);

            float length = Vector2.Distance(start, end);
            bool horizontal = Mathf.Abs(end.y - start.y) < 0.01f;
            obj.transform.localScale = horizontal
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

        // Coordinate conversion

        Vector2 WorldToEnvironment(Vector2 worldPos)
        {
            Vector2 center = cam.transform.position;
            float halfW = (currentEnvironment.MaxLength * worldScale) / 2f;
            float halfH = (currentEnvironment.MaxHeight * worldScale) / 2f;
            return new Vector2(
                (worldPos.x - (center.x - halfW)) / (halfW * 2f) * currentEnvironment.MaxLength,
                (worldPos.y - (center.y - halfH)) / (halfH * 2f) * currentEnvironment.MaxHeight
            );
        }

        Vector2 EnvironmentToWorld(Vector2 envPos)
        {
            Vector2 center = cam.transform.position;
            float halfW = (currentEnvironment.MaxLength * worldScale) / 2f;
            float halfH = (currentEnvironment.MaxHeight * worldScale) / 2f;
            return new Vector2(
                (envPos.x / currentEnvironment.MaxLength) * (halfW * 2f) + (center.x - halfW),
                (envPos.y / currentEnvironment.MaxHeight) * (halfH * 2f) + (center.y - halfH)
            );
        }

        // Init

        IEnumerator InitializeAsync()
        {
            yield return null;
            yield return null;

            SetupBoundary();

            var loadTask = saveLoad.LoadEnvironmentAsync(currentEnvironment.Id);
            yield return new WaitUntil(() => loadTask.IsCompleted);

            foreach (var dto in loadTask.Result)
            {
                Sprite sprite = palette.GetSpriteById(dto.PrefabId);
                if (sprite == null) continue;

                GameObject obj = new GameObject(sprite.name);
                obj.transform.position = EnvironmentToWorld(new Vector2(dto.PositionX, dto.PositionY));
                obj.transform.localScale = new Vector3(dto.ScaleX, dto.ScaleY, 1f);
                obj.transform.eulerAngles = new Vector3(0f, 0f, dto.RotationZ);

                SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingOrder = dto.SortingLayer;

                obj.AddComponent<BoxCollider2D>();

                PlaceableObject po = obj.AddComponent<PlaceableObject>();
                po.objectId = sprite.name;
                po.prefabId = dto.PrefabId;

                placedObjects.Add(obj);
            }

            yield return null;
            Debug.Log($"{loadTask.Result.Count} objecten geladen.");
        }

        // UI buttons

        public void RotateSelected() => selectedObject?.transform.Rotate(0, 0, 90);

        public void FlipSelected()
        {
            if (selectedObject == null) return;
            Vector3 s = selectedObject.transform.localScale;
            s.x *= -1;
            selectedObject.transform.localScale = s;
        }

        public void DeleteSelected()
        {
            if (selectedObject == null) return;
            placedObjects.Remove(selectedObject);
            Destroy(selectedObject);
            selectedObject = null;
            isDragging = false;
        }

        public void SaveAll()
        {
            foreach (GameObject obj in placedObjects)
            {
                var po = obj.GetComponent<PlaceableObject>();
                if (po == null) continue;
                po.envPosition = WorldToEnvironment(obj.transform.position);
            }
            saveLoad.SaveEnvironment(placedObjects, currentEnvironment.Id, this);
        }

        public async void Close()
        {
            PlayerPrefs.DeleteKey("Environment_Id");
            PlayerPrefs.DeleteKey("Environment_Name");
            PlayerPrefs.DeleteKey("Environment_MaxHeight");
            PlayerPrefs.DeleteKey("Environment_MaxLength");
            await SceneManager.LoadSceneAsync("EnvironmentSelectorScene", LoadSceneMode.Single);
        }

        public List<GameObject> GetPlacedObjects() => placedObjects;
    }
}