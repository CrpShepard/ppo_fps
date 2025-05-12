using UnityEngine;

public class SyncTransform : MonoBehaviour
{
    Transform parent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        parent = GetComponentInParent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = Vector3.zero;
        transform.rotation = parent.rotation;
    }
}
