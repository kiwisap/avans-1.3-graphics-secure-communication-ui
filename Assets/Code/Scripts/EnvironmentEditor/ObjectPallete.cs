using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ObjectPalette : MonoBehaviour
{
    public List<Sprite> availableSprites;
    public GameObject paletteButtonPrefab;
    public Transform paletteContainer;
    public EnvironmentEditorController editorController;

    void Start()
    {
        foreach (Sprite sprite in availableSprites)
        {
            GameObject btn = Instantiate(paletteButtonPrefab, paletteContainer);
            btn.GetComponent<Image>().sprite = sprite;

            Sprite captured = sprite;
            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                editorController.SelectSprite(captured);
            });
        }
    }
}