using UnityEngine;

public class CameraFollow3D : MonoBehaviour
{
    private static CameraFollow3D instance;

    [SerializeField] private Transform target;
    [SerializeField] public Vector3 offset = new Vector3(0, 9f, -8f);
    [SerializeField] public float rotationX = 50f;
    [SerializeField] private float smoothTime = 0.15f;
    [SerializeField] public float margin = 3f;

    [Header("Focus Bounds")]
    [SerializeField] public float focusMinX = -7f;
    [SerializeField] public float focusMaxX = 7f;
    [SerializeField] public float focusMinZ = -7f;
    [SerializeField] public float focusMaxZ = 7f;

    private Vector3 velocity = Vector3.zero;
    private Camera cam;

    private float shakeTimer;
    private float shakeIntensity;

    private ObstacleVisual lastFadedObstacle = null;

    private void Awake()
    {
        instance = this;
        RecalculateBounds();
    }

    private void Start()
    {
        cam = GetComponent<Camera>();
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
        transform.rotation = Quaternion.Euler(rotationX, 0, 0);
        RecalculateBounds();
    }

    [ContextMenu("Recalculate Bounds")]
    public void RecalculateBounds()
    {
        float fMinX = float.MaxValue, fMaxX = float.MinValue;
        float fMinZ = float.MaxValue, fMaxZ = float.MinValue;
        GameObject env = GameObject.Find("Environment");
        if (env != null)
        {
            foreach (Transform child in env.transform)
            {
                if (child.name.Contains("Fence"))
                {
                    Vector3 pos = child.position;
                    if (pos.x < fMinX) fMinX = pos.x;
                    if (pos.x > fMaxX) fMaxX = pos.x;
                    if (pos.z < fMinZ) fMinZ = pos.z;
                    if (pos.z > fMaxZ) fMaxZ = pos.z;
                }
            }
            if (fMinX != float.MaxValue)
            {
                focusMinX = fMinX + margin;
                focusMaxX = fMaxX - margin;
                focusMinZ = fMinZ + margin;
                focusMaxZ = fMaxZ - margin;
                Debug.Log($"[CameraFollow3D] Bounds recalculated: X({focusMinX:F2} to {focusMaxX:F2}), Z({focusMinZ:F2} to {focusMaxZ:F2})");
            }
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
            else return;
        }

        Vector3 focusPoint = target.position;
        focusPoint.x = Mathf.Clamp(focusPoint.x, focusMinX, focusMaxX);
        focusPoint.z = Mathf.Clamp(focusPoint.z, focusMinZ, focusMaxZ);

        Vector3 targetCameraPos = focusPoint + offset;
        
        // Apply shake
        if (shakeTimer > 0)
        {
            targetCameraPos += Random.insideUnitSphere * shakeIntensity;
            shakeTimer -= Time.deltaTime;
        }

        transform.position = Vector3.SmoothDamp(transform.position, targetCameraPos, ref velocity, smoothTime);
        
        // Check for obstacles blocking the view
        if (Physics.Raycast(transform.position, target.position - transform.position, out RaycastHit hit, Vector3.Distance(transform.position, target.position), LayerMask.GetMask("Obstacle")))
        {
            ObstacleVisual ov = hit.collider.GetComponentInParent<ObstacleVisual>();
            if (ov != null)
            {
                if (lastFadedObstacle != null && lastFadedObstacle != ov) lastFadedObstacle.SetFaded(false);
                ov.SetFaded(true);
                lastFadedObstacle = ov;
            }
        }
        else if (lastFadedObstacle != null)
        {
            lastFadedObstacle.SetFaded(false);
            lastFadedObstacle = null;
        }
    }

    public static void Shake(float duration, float intensity)
    {
        if (instance != null)
        {
            instance.shakeTimer = duration;
            instance.shakeIntensity = intensity;
        }
    }
}
