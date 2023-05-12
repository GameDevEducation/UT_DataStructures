using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle3D : MonoBehaviour, ISpatialData3D
{
    [SerializeField] Collider LinkedCollider;
    [SerializeField] MeshRenderer LinkedMeshRender;

    Color OldColour;

    Vector3? CachedPosition;
    Bounds? CachedBounds;
    float? CachedRadius;

    bool IsCachedDataInvalid
    {
        get
        {
            if (CachedPosition == null || CachedBounds == null || CachedRadius == null)
                return true;

            return !Mathf.Approximately((transform.position - CachedPosition.Value).sqrMagnitude, 0f);
        }
    }

    void Start()
    {
        OldColour = LinkedMeshRender.material.color = Color.red;
    }

    public void AddHighlight()
    {
        LinkedMeshRender.material.color = Color.yellow;
    }

    public void RemoveHighlight()
    {
        LinkedMeshRender.material.color = OldColour;
    }

    Vector3 ISpatialData3D.GetLocation()
    {
        if (IsCachedDataInvalid)
            UpdateCachedData();

        return CachedPosition.Value;
    }

    Bounds ISpatialData3D.GetBounds()
    {
        if (IsCachedDataInvalid)
            UpdateCachedData();

        return CachedBounds.Value;
    }

    float ISpatialData3D.GetRadius()
    {
        if (IsCachedDataInvalid)
            UpdateCachedData();

        return CachedRadius.Value;
    }

    void UpdateCachedData()
    {
        CachedPosition = transform.position;
        CachedBounds = LinkedCollider.bounds;
        CachedRadius = LinkedCollider.bounds.extents.magnitude;
    }
}
