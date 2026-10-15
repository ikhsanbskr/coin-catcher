using UnityEngine;

public class FallingItem3D : MonoBehaviour
{
    public enum ItemType { Coin, Bomb }
    public ItemType type;
    public float fallSpeed = 5f;

    private float despawnY = -10f;
    private bool hasLanded = false;
    private float hoverTimer = 0f;
    private float groundY = 0.0f; // Surface is exactly at 0
    private float extentsY = 0.5f;
    
    private void OnEnable()
    {
        hasLanded = false;
        hoverTimer = 0f;
        Collider col = GetComponent<Collider>();
        if (col != null) extentsY = col.bounds.extents.y;
    }

    public void Initialize(float speed)
    {
        fallSpeed = speed;
    }

    private void Update()
    {
        if (GameManager3D.Instance == null || GameManager3D.Instance.CurrentState != GameManager3D.GameState.Playing) return;

        if (!hasLanded)
        {
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);

            float targetY = groundY + extentsY + (type == ItemType.Coin ? 0.3f : 0f);
            if (transform.position.y <= targetY)
            {
                transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
                
                // Check if inside obstacle
                Collider[] obs = Physics.OverlapSphere(transform.position, 0.5f, LayerMask.GetMask("Obstacle"));
                if (obs.Length > 0)
                {
                    Vector3 dir = (transform.position - obs[0].transform.position);
                    dir.y = 0;
                    if (dir.sqrMagnitude < 0.1f) dir = Vector3.right;
                    transform.position += dir.normalized * 1.5f;
                }

                hasLanded = true;
                
                if (type == ItemType.Bomb)
                {
                    Explode();
                }
            }
        }
        else if (type == ItemType.Coin)
        {
            hoverTimer += Time.deltaTime;
            // Hover animation
            transform.Rotate(0, 90f * Time.deltaTime, 0);
            transform.position += new Vector3(0, Mathf.Sin(Time.time * 5f) * 0.2f * Time.deltaTime, 0);

            if (hoverTimer >= 2.0f)
            {
                gameObject.SetActive(false);
            }
        }

        if (transform.position.y < despawnY)
        {
            gameObject.SetActive(false); // Fallback bounds check
        }
    }

    private void Explode()
    {
        // Play particle if available
        GameObject vfx = ObjectPooler3D.Instance != null ? ObjectPooler3D.Instance.SpawnFromPool("ExplosionVFX", transform.position, Quaternion.identity) : null;
        
        // Play bomb sound
        if (AudioManager.Instance != null) AudioManager.Instance.PlayBomb();

        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (type == ItemType.Coin)
            {
                if (GameManager3D.Instance != null) GameManager3D.Instance.AddScore(10);
                if (AudioManager.Instance != null) AudioManager.Instance.PlayCoin();
                gameObject.SetActive(false); // Return to pool
            }
            else if (type == ItemType.Bomb)
            {
                Player3DController pc = other.GetComponent<Player3DController>();
                if (pc != null && pc.IsDashing && pc.invulnerableDuringDash)
                {
                    // Invulnerable, don't take damage or explode on player
                    return;
                }
                
                if (GameManager3D.Instance != null) GameManager3D.Instance.TakeDamage();
                if (AudioManager.Instance != null) AudioManager.Instance.PlayHit();
                
                CameraFollow3D.Shake(0.25f, 0.3f);
                Explode();
            }
        }
    }
}
