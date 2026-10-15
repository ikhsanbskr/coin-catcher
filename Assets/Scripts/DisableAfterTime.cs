using UnityEngine;

public class DisableAfterTime : MonoBehaviour
{
    public float time = 1f;
    private float timer = 0f;

    void OnEnable()
    {
        timer = 0f;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= time)
        {
            gameObject.SetActive(false);
        }
    }
}
