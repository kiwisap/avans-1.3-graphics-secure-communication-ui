using Assets.Code.Models;
using Assets.Code.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Code.Scripts.EnvironmentEditor
{
    public class EnvironmentSaveLoad : MonoBehaviour
    {
        private Object2DService _objectService;

        private void Awake()
        {
            GameObject serviceObj = new GameObject("Object2DService");
            _objectService = serviceObj.AddComponent<Object2DService>();
        }

        public async void SaveEnvironment(List<GameObject> placedObjects, int environmentId, EnvironmentEditorController controller)
        {
            Debug.Log("Opslaan...");

            var existing = await _objectService.GetObjectsByEnvironmentAsync(environmentId);
            if (!existing.Ok)
            {
                Debug.LogError($"Ophalen voor verwijderen mislukt: {existing.Error}");
                return;
            }

            foreach (var obj in existing.Value)
            {
                var deleteResult = await _objectService.DeleteObjectAsync(obj.Id);
                if (!deleteResult.Ok)
                    Debug.LogError($"Verwijderen object {obj.Id} mislukt: {deleteResult.Error}");
            }

            foreach (GameObject obj in placedObjects)
            {
                PlaceableObject po = obj.GetComponent<PlaceableObject>();
                if (po == null) continue;

                var dto = new Object2DDto
                {
                    EnvironmentId = environmentId,
                    PrefabId = po.prefabId,
                    PositionX = po.envPosition.x,
                    PositionY = po.envPosition.y,
                    ScaleX = obj.transform.localScale.x,
                    ScaleY = obj.transform.localScale.y,
                    RotationZ = obj.transform.eulerAngles.z,
                    SortingLayer = obj.GetComponent<SpriteRenderer>()?.sortingOrder ?? 1
                };

                var result = await _objectService.CreateObjectAsync(dto);
                if (!result.Ok)
                    Debug.LogError($"Opslaan object mislukt: {result.Error}");
            }

            Debug.Log("Omgeving opgeslagen!");
        }

        public async Task<List<Object2DDto>> LoadEnvironmentAsync(int environmentId)
        {
            var result = await _objectService.GetObjectsByEnvironmentAsync(environmentId);
            if (!result.Ok)
            {
                Debug.LogError($"Laden mislukt: {result.Error}");
                return new List<Object2DDto>();
            }

            return new List<Object2DDto>(result.Value);
        }
    }
}