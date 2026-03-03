using UnityEngine;

public class PrefabSpawner : MonoBehaviour
{
    public GameObject marioPrefab;

    public void SpawnMario()
    {
        Debug.Log("Spawn mario");
        GameObject newMario = Instantiate(marioPrefab, Vector3.zero, Quaternion.identity);
        Draggable draggable = newMario.GetComponent<Draggable>();
        draggable.StartDragging();
    }
}
