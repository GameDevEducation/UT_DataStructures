using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle2D : MonoBehaviour
{
    [SerializeField] Collider LinkedCollider;
    [SerializeField] MeshRenderer LinkedMeshRender;

    Color OldColour;

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
}
