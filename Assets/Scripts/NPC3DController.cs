using UnityEngine;

public class NPC3DController : MonoBehaviour
{
    public Transform playerTransform;
    
    public enum NPCType { Maya, Arka }
    public NPCType npcType;

    private float timer = 0f;
    private float jumpTimer = 0f;
    private Vector3 startPos;
    
    private void Start()
    {
        startPos = transform.position;
        if (playerTransform == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p) playerTransform = p.transform;
        }
    }

    private void Update()
    {
        if (playerTransform != null)
        {
            Vector3 targetDir = playerTransform.position - transform.position;
            targetDir.y = 0;
            if (targetDir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(targetDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 3f);
            }
        }

        // Idle animation
        timer += Time.deltaTime;
        
        // Maya jumps occasionally, Arka is calm
        if (npcType == NPCType.Maya)
        {
            if (timer > 2f)
            {
                jumpTimer += Time.deltaTime * 5f;
                float yOffset = Mathf.Sin(jumpTimer) * 0.2f;
                if (jumpTimer > Mathf.PI) 
                {
                    jumpTimer = 0f;
                    timer = 0f;
                    yOffset = 0f;
                }
                transform.position = startPos + new Vector3(0, yOffset, 0);
            }
            else
            {
                transform.position = startPos + new Vector3(0, Mathf.Sin(Time.time * 2f) * 0.05f, 0);
            }
        }
        else
        {
            transform.position = startPos + new Vector3(0, Mathf.Sin(Time.time * 1.5f) * 0.03f, 0);
            if (isNodding)
            {
                nodTimer += Time.deltaTime * 10f;
                float rotX = Mathf.Sin(nodTimer) * 15f;
                transform.eulerAngles = new Vector3(rotX, transform.eulerAngles.y, transform.eulerAngles.z);
                if (nodTimer > Mathf.PI)
                {
                    isNodding = false;
                    transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, transform.eulerAngles.z);
                }
            }
        }
    }

    private bool isNodding = false;
    private float nodTimer = 0f;

    public void React()
    {
        if (npcType == NPCType.Maya)
        {
            jumpTimer = 0f; // Start jump
            timer = 2f; 
        }
        else
        {
            isNodding = true;
            nodTimer = 0f;
        }
    }
}
