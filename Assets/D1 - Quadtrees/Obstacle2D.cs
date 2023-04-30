using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle2D : MonoBehaviour, ISpatialData2D
{
    [SerializeField] Collider LinkedCollider;
    [SerializeField] MeshRenderer LinkedMeshRender;

    Color OldColour;

    Vector3 CachedPosition;
    Rect? CachedBounds;
    Vector2? Cached2DPosition;
    float? CachedRadius;

    bool HasMoved
    {
        get
        {
            return !Mathf.Approximately((transform.position - CachedPosition).sqrMagnitude, 0f);
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

    public Vector2 GetLocation()
    {
        if (Cached2DPosition == null)
            CachePositionData();

        return Cached2DPosition.Value;
    }

    public Rect GetBounds()
    {
        if (CachedBounds == null)
            CachePositionData();

        return CachedBounds.Value;
    }

    public float GetRadius()
    {
        if (CachedRadius == null)
            CachePositionData();

        return CachedRadius.Value;
    }

    void CachePositionData()
    {
        CachedPosition = transform.position;
        Cached2DPosition = new Vector2(transform.position.x, transform.position.z);

        float HalfWidth = LinkedCollider.bounds.size.x;
        float HalfHeight = LinkedCollider.bounds.size.z;

        CachedBounds = new Rect(transform.position.x - HalfWidth, transform.position.z - HalfHeight,
                                HalfWidth, HalfHeight);

        CachedRadius = Mathf.Sqrt(HalfWidth * HalfWidth + HalfHeight * HalfHeight);
    }
}
