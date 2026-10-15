using UnityEngine;

public class CoinRotator : MonoBehaviour
{
    public float speed = 90f;
    void Update()
    {
        transform.Rotate(0, speed * Time.deltaTime, 0, Space.World);
    }
}
