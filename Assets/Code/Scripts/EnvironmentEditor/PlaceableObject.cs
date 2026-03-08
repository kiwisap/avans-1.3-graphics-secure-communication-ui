using UnityEngine;

namespace Assets.Code.Scripts.EnvironmentEditor
{
    public class PlaceableObject : MonoBehaviour
    {
        public string objectId;

        public int prefabId;

        public bool isFlippedX = false;

        public float rotation = 0f;

        public Vector2 envPosition;
    }
}