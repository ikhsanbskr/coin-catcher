using UnityEngine;

public class CloudDrift : MonoBehaviour
{
    public float speed = 1.0f;
    public float range = 60f; // Added to fix BuildWorldDecoration.cs
    private float startX;

    private void Start()
    {
        startX = transform.position.x;
    }

    private void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime, Space.World);
        if (transform.position.x > startX + range)
        {
            transform.position = new Vector3(startX - range, transform.position.y, transform.position.z);
        }
    }
}
