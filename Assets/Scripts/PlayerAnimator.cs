using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    public Transform body;
    public Transform head;
    public Transform leftArm;
    public Transform rightArm;
    public Transform leftLeg;
    public Transform rightLeg;
    
    private Vector3 lastPosition;
    private float moveTime = 0f;
    private float walkSpeed = 15f;
    private float swingAngle = 45f;
    
    private bool isHit = false;
    private float hitTimer = 0f;
    private bool isGameOver = false;

    private Material[] mats;
    private Color[] origColors;
    private Vector3 origBodyPos;
    private Quaternion origBodyRot;

    private void Awake()
    {
        if (body != null)
        {
            origBodyPos = body.localPosition;
            origBodyRot = body.localRotation;
        }
    }

    private void Start()
    {
        lastPosition = transform.position;
        if (body != null)
        {
            var renderers = GetComponentsInChildren<MeshRenderer>();
            mats = new Material[renderers.Length];
            origColors = new Color[renderers.Length];
            for(int i=0; i<renderers.Length; i++) 
            {
                mats[i] = renderers[i].material;
                origColors[i] = mats[i].color;
            }
        }

        if (GameManager3D.Instance != null)
        {
            GameManager3D.Instance.OnGameOver += TriggerDefeat;
            GameManager3D.Instance.OnLivesChanged += OnLivesChangedHandler;
            GameManager3D.Instance.OnGameStarted += ResetAnimator;
        }
    }

    private void OnLivesChangedHandler(int lives)
    {
        TriggerHit();
    }

    private void OnDestroy()
    {
        if (GameManager3D.Instance != null)
        {
            GameManager3D.Instance.OnGameOver -= TriggerDefeat;
            GameManager3D.Instance.OnLivesChanged -= OnLivesChangedHandler;
            GameManager3D.Instance.OnGameStarted -= ResetAnimator;
        }
    }

    private void ResetAnimator()
    {
        isGameOver = false;
        isHit = false;
        hitTimer = 0f;
        moveTime = 0f;

        // Reset body rotation (stand back up)
        if (body != null)
        {
            body.localRotation = origBodyRot;
            body.localPosition = origBodyPos;
        }

        // Reset all limbs
        if (leftArm != null) leftArm.localRotation = Quaternion.identity;
        if (rightArm != null) rightArm.localRotation = Quaternion.identity;
        if (leftLeg != null) leftLeg.localRotation = Quaternion.identity;
        if (rightLeg != null) rightLeg.localRotation = Quaternion.identity;

        // Restore original colors
        if (mats != null && origColors != null)
        {
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] != null) mats[i].color = origColors[i];
            }
        }

        // Reset position to center
        transform.position = new Vector3(0, transform.position.y, 0);
        transform.rotation = Quaternion.identity;
        lastPosition = transform.position;
    }

    private void Update()
    {
        if (isGameOver)
        {
            if (body != null) body.localRotation = Quaternion.Lerp(body.localRotation, Quaternion.Euler(90, 0, 0), Time.deltaTime * 5f);
            return;
        }

        Vector3 velocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;
        
        float speed = new Vector3(velocity.x, 0, velocity.z).magnitude;

        if (speed > 0.1f)
        {
            moveTime += Time.deltaTime;
            float angle = Mathf.Sin(moveTime * walkSpeed) * swingAngle;
            if (leftArm != null) leftArm.localRotation = Quaternion.Euler(angle, 0, 0);
            if (rightArm != null) rightArm.localRotation = Quaternion.Euler(-angle, 0, 0);
            if (leftLeg != null) leftLeg.localRotation = Quaternion.Euler(-angle, 0, 0);
            if (rightLeg != null) rightLeg.localRotation = Quaternion.Euler(angle, 0, 0);
            
            // Bobbing
            if (body != null) 
            {
                body.localPosition = origBodyPos + new Vector3(0, Mathf.Abs(Mathf.Sin(moveTime * walkSpeed)) * 0.1f, 0);
                body.localRotation = origBodyRot * Quaternion.Euler(5f, 0, 0);
            }
            
            // Rotate facing direction smoothly
            if (velocity.sqrMagnitude > 0.1f)
            {
                Quaternion targetRot = Quaternion.LookRotation(new Vector3(velocity.x, 0, velocity.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
            }
        }
        else
        {
            moveTime = 0;
            // Return to idle
            if (leftArm != null) leftArm.localRotation = Quaternion.Lerp(leftArm.localRotation, Quaternion.identity, Time.deltaTime * 10f);
            if (rightArm != null) rightArm.localRotation = Quaternion.Lerp(rightArm.localRotation, Quaternion.identity, Time.deltaTime * 10f);
            if (leftLeg != null) leftLeg.localRotation = Quaternion.Lerp(leftLeg.localRotation, Quaternion.identity, Time.deltaTime * 10f);
            if (rightLeg != null) rightLeg.localRotation = Quaternion.Lerp(rightLeg.localRotation, Quaternion.identity, Time.deltaTime * 10f);
            if (body != null) 
            {
                body.localPosition = Vector3.Lerp(body.localPosition, origBodyPos + new Vector3(0, Mathf.Sin(Time.time * 3f) * 0.05f, 0), Time.deltaTime * 10f);
                body.localRotation = Quaternion.Lerp(body.localRotation, origBodyRot, Time.deltaTime * 10f);
            }
        }

        if (isHit)
        {
            hitTimer -= Time.deltaTime;
            if (hitTimer <= 0)
            {
                isHit = false;
                for(int i=0; i<mats.Length; i++) if(mats[i] != null) mats[i].color = origColors[i];
            }
            else
            {
                float flash = Mathf.PingPong(Time.time * 10f, 1f);
                for(int i=0; i<mats.Length; i++) if(mats[i] != null) mats[i].color = Color.Lerp(origColors[i], Color.red, flash);
            }
        }
    }

    public void TriggerHit()
    {
        isHit = true;
        hitTimer = 0.5f;
    }

    public void TriggerDefeat()
    {
        isGameOver = true;
    }
}
