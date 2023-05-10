using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RandomItemSpawner : MonoBehaviour
{
    public enum EMode
    {
        Mode_2D,
        Mode_3D
    };

    [SerializeField] EMode Mode = EMode.Mode_2D;
    [SerializeField] GameObject PrefabToSpawn;
    [SerializeField] int NumToSpawn = 1000;
    [SerializeField] GameObject SpawnZone;
    [SerializeField] float MinRadius = 0.5f;
    [SerializeField] float MaxRadius = 2f;

    [SerializeField] UnityEvent<Rect> On2DBoundsCalculated = new();
    [SerializeField] UnityEvent<Bounds> On3DBoundsCalculated = new();

    [SerializeField] UnityEvent<GameObject> OnItemSpawned = new();
    [SerializeField] UnityEvent<List<GameObject>> OnAllItemsSpawned = new();

    // Start is called before the first frame update
    void Start()
    {
        PerformSpawning();
    }

    void PerformSpawning()
    {
        var SpawnBounds = SpawnZone.GetComponent<MeshRenderer>().bounds;

        // cleanup any already spawned
        for (int ChildIndex = SpawnZone.transform.childCount - 1; ChildIndex >= 0; --ChildIndex)
        {
            var ChildGO = SpawnZone.transform.GetChild(ChildIndex);
            Destroy(ChildGO);
        }

        List<GameObject> Items = new List<GameObject>(NumToSpawn);
        if (Mode == EMode.Mode_2D)
        {
            var SpawnRect = new Rect(SpawnBounds.min.x, SpawnBounds.min.z, SpawnBounds.size.x, SpawnBounds.size.z);

            On2DBoundsCalculated.Invoke(SpawnRect);

            for (int index = 0; index < NumToSpawn; ++index)
            {
                Vector3 SpawnPos = new Vector3(Random.Range(SpawnRect.xMin, SpawnRect.xMax),
                                               0f,
                                               Random.Range(SpawnRect.yMin, SpawnRect.yMax));

                float Radius = Random.Range(MinRadius, MaxRadius);

                var NewGO = GameObject.Instantiate(PrefabToSpawn, SpawnPos, Quaternion.identity);
                NewGO.transform.localScale = new Vector3(Radius, 1f, Radius);
                NewGO.transform.SetParent(SpawnZone.transform);

                Items.Add(NewGO);
                OnItemSpawned.Invoke(NewGO);
            }
        }
        else if (Mode == EMode.Mode_3D)
        {
            On3DBoundsCalculated.Invoke(SpawnBounds);

            for (int index = 0; index < NumToSpawn; ++index)
            {
                Vector3 SpawnPos = new Vector3(Random.Range(SpawnBounds.min.x, SpawnBounds.max.x),
                                               Random.Range(SpawnBounds.min.y, SpawnBounds.max.y),
                                               Random.Range(SpawnBounds.min.z, SpawnBounds.max.z));

                float Radius = Random.Range(MinRadius, MaxRadius);

                var NewGO = GameObject.Instantiate(PrefabToSpawn, SpawnPos, Quaternion.identity);
                NewGO.transform.localScale = new Vector3(Radius, Radius, Radius);
                NewGO.transform.SetParent(SpawnZone.transform);

                Items.Add(NewGO);
                OnItemSpawned.Invoke(NewGO);
            }
        }

        OnAllItemsSpawned.Invoke(Items);
    }
}
