using UnityEngine;

public class WindmillSpin : MonoBehaviour
{
    public float speed = 30f;

    private void Update()
    {
        transform.Rotate(0, 0, speed * Time.deltaTime);
    }
}
