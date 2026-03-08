using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Assets.Code.Scripts.EnvironmentEditor
{
    public class ObjectPalette : MonoBehaviour
    {
        public List<Sprite> availableSprites;
        public GameObject paletteButtonPrefab;
        public Transform paletteContainer;
        public EnvironmentEditorController editorController;

        void Start()
        {
            for (int i = 0; i < availableSprites.Count; i++)
            {
                Sprite captured = availableSprites[i];
                int prefabId = i; // Index = PrefabId

                GameObject btn = Instantiate(paletteButtonPrefab, paletteContainer);
                btn.GetComponent<Image>().sprite = captured;
                btn.GetComponent<Button>().onClick.AddListener(() =>
                {
                    editorController.SelectSprite(captured, prefabId);
                });
            }
        }

        public Sprite GetSpriteById(int prefabId)
        {
            if (prefabId >= 0 && prefabId < availableSprites.Count)
                return availableSprites[prefabId];

            Debug.LogWarning($"PrefabId {prefabId} niet gevonden in palette.");
            return null;
        }
    }
}