using UnityEngine;

public class ObstacleVisual : MonoBehaviour
{
    private MeshRenderer[] renderers;
    private Color[] originalColors;
    private Material[][] originalMaterials;
    private Material[][] fadeMaterials;
    private bool isFaded = false;

    private float targetAlpha = 1f;
    private float currentAlpha = 1f;

    private Vector3 originalPos;
    private Quaternion originalRot;
    private float wobbleTimer = 0f;

    void Start()
    {
        renderers = GetComponentsInChildren<MeshRenderer>();
        originalColors = new Color[renderers.Length];
        fadeMaterials = new Material[renderers.Length][];
        originalMaterials = new Material[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                Material[] sharedMats = renderers[i].sharedMaterials;
                originalMaterials[i] = new Material[sharedMats.Length];
                fadeMaterials[i] = new Material[sharedMats.Length];
                
                // Keep color of first non-null for simplicity
                originalColors[i] = Color.white;
                for (int j = 0; j < sharedMats.Length; j++)
                {
                    if (sharedMats[j] != null)
                    {
                        originalMaterials[i][j] = sharedMats[j];
                        if (j == 0) // Just grab color from the first valid mat
                        {
                            if (sharedMats[j].HasProperty("_BaseColor"))
                                originalColors[i] = sharedMats[j].GetColor("_BaseColor");
                            else if (sharedMats[j].HasProperty("_Color"))
                                originalColors[i] = sharedMats[j].color;
                        }

                        // Create a transparent instance
                        fadeMaterials[i][j] = new Material(sharedMats[j]);
                        SetupMaterialWithBlendMode(fadeMaterials[i][j], 3); // Transparent mode
                    }
                }
            }
        }
        
        originalPos = transform.localPosition;
        originalRot = transform.localRotation;
    }

    private void SetupMaterialWithBlendMode(Material material, int blendMode)
    {
        material.SetFloat("_Surface", 1); // Transparent
        material.SetFloat("_Blend", 0); // Alpha
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }

    void Update()
    {
        // Handle fading
        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, Time.deltaTime * 5f);
        if (currentAlpha < 0.99f && !isFaded)
        {
            isFaded = true;
            for (int i = 0; i < renderers.Length; i++) 
            {
                if (renderers[i] != null && fadeMaterials[i] != null) 
                    renderers[i].materials = fadeMaterials[i];
            }
        }
        else if (currentAlpha >= 0.99f && isFaded)
        {
            isFaded = false;
            for (int i = 0; i < renderers.Length; i++) 
            {
                if (renderers[i] != null && originalMaterials[i] != null) 
                    renderers[i].materials = originalMaterials[i]; 
            }
        }

        if (isFaded)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (fadeMaterials[i] != null)
                {
                    Color c = originalColors[i];
                    c.a = currentAlpha;
                    for (int j = 0; j < fadeMaterials[i].Length; j++)
                    {
                        if (fadeMaterials[i][j] != null)
                        {
                            if (fadeMaterials[i][j].HasProperty("_BaseColor"))
                                fadeMaterials[i][j].SetColor("_BaseColor", c);
                            else if (fadeMaterials[i][j].HasProperty("_Color"))
                                fadeMaterials[i][j].color = c;
                        }
                    }
                }
            }
        }
        
        // Wobble effect
        if (wobbleTimer > 0)
        {
            wobbleTimer -= Time.deltaTime;
            float intensity = wobbleTimer * 5f;
            transform.localRotation = originalRot * Quaternion.Euler(Mathf.Sin(Time.time * 30f) * intensity, 0, Mathf.Cos(Time.time * 25f) * intensity);
        }
        else
        {
            transform.localRotation = originalRot;
        }
    }

    public void SetFaded(bool fade)
    {
        targetAlpha = fade ? 0.4f : 1f;
    }

    public void Wobble()
    {
        if (gameObject.name.Contains("Sword"))
            wobbleTimer = 0.5f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayHit();
            // Try spawn simple dust
            if (ObjectPooler3D.Instance != null)
            {
                // We'll reuse explosion VFX but scale it down as dust
                GameObject dust = ObjectPooler3D.Instance.SpawnFromPool("ExplosionVFX", collision.contacts[0].point, Quaternion.identity);
                if (dust != null)
                {
                    dust.transform.localScale = Vector3.one * 0.3f; // Make it small
                }
            }
        }
    }
}
