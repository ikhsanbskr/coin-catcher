using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class DialogBubble : MonoBehaviour
{
    public TextMeshProUGUI textComp;
    public RectTransform bubbleRect;
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI nameTextComp;
    public Image outlineImg;
    public Image nameBgImg;
    
    private Transform targetNPC;
    private Camera mainCam;
    private RectTransform parentCanvasRect;

    private Vector2 targetPos;
    private Vector2 currentPos;

    private Coroutine typeRoutine;

    private void Awake()
    {
        mainCam = Camera.main;
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null) parentCanvasRect = parentCanvas.GetComponent<RectTransform>();
        if (canvasGroup != null) canvasGroup.alpha = 0f;
    }

    private void LateUpdate()
    {
        if (targetNPC != null && canvasGroup.alpha > 0 && mainCam != null)
        {
            Vector3 worldPos = targetNPC.position + Vector3.up * 2.5f;
            Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos);
            
            if (screenPos.z < 0) screenPos.y = -1000f; 
            
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvasRect, screenPos, null, out localPoint);
            
            localPoint.y += 60f; // offset 60px above NPC head
            
            // Rebuild layout to ensure height is correct before clamping
            LayoutRebuilder.ForceRebuildLayoutImmediate(bubbleRect);
            
            float marginX = 60f;
            float marginY = 60f;
            float topHUDMargin = 120f;
            
            float halfWidth = bubbleRect.rect.width / 2f;
            float height = bubbleRect.rect.height; // pivot is 0 at bottom, so full height goes up
            
            float minX = -parentCanvasRect.rect.width / 2f + halfWidth + marginX;
            float maxX = parentCanvasRect.rect.width / 2f - halfWidth - marginX;
            float minY = -parentCanvasRect.rect.height / 2f + marginY; 
            float maxY = parentCanvasRect.rect.height / 2f - height - topHUDMargin; 
            
            // Put bubble below NPC if it is too high
            if (screenPos.y > Screen.height - 180f)
            {
                localPoint.y -= (height + 160f); // shift down completely
            }
            
            localPoint.x = Mathf.Clamp(localPoint.x, minX, maxX);
            localPoint.y = Mathf.Clamp(localPoint.y, minY, maxY);

            targetPos = localPoint;
            
            // Smooth follow
            currentPos = Vector2.Lerp(currentPos, targetPos, Time.deltaTime * 10f);

            // Floating effect
            float floatY = Mathf.Sin(Time.time * 3f) * 4f;

            bubbleRect.anchoredPosition = currentPos + new Vector2(0, floatY);
        }
    }

    public void SetupStyle(Color npcColor)
    {
        if (outlineImg) outlineImg.color = npcColor;
        if (nameBgImg) nameBgImg.color = npcColor; // Used for the Dot
    }

    public void Show(string name, string text, Transform target, Color npcColor)
    {
        targetNPC = target;
        SetupStyle(npcColor);
        if (nameTextComp != null) nameTextComp.text = name;
        
        gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
        
        if (typeRoutine != null) StopCoroutine(typeRoutine);
        typeRoutine = StartCoroutine(TypeAnim(text));

        StopCoroutine("PopAnimation");
        StopCoroutine("FadeOut");
        StartCoroutine("PopAnimation");

        CancelInvoke(nameof(Hide));
        Invoke(nameof(Hide), 4f);
    }

    private IEnumerator TypeAnim(string fullText)
    {
        textComp.text = "";
        float delay = 1f / 40f;
        for (int i = 0; i <= fullText.Length; i++)
        {
            textComp.text = fullText.Substring(0, i);
            LayoutRebuilder.ForceRebuildLayoutImmediate(bubbleRect); // Force rebuild every frame to prevent text cutting
            
            // Initial pos calculation at start of typing to prevent jumping
            if (i == 0 && mainCam != null)
            {
                Vector3 worldPos = targetNPC.position + Vector3.up * 2.5f;
                Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvasRect, screenPos, null, out currentPos);
                currentPos.y += 60f;
            }
            
            yield return new WaitForSeconds(delay);
        }
    }

    private IEnumerator PopAnimation()
    {
        float timer = 0f;
        float duration = 0.25f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            // ease out back approx
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            float val = 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
            
            bubbleRect.localScale = Vector3.one * val;
            yield return null;
        }
        bubbleRect.localScale = Vector3.one;
    }

    public void Hide()
    {
        if (gameObject.activeInHierarchy)
        {
            StopCoroutine("FadeOut");
            StartCoroutine("FadeOut");
        }
    }

    private IEnumerator FadeOut()
    {
        float timer = 0f;
        float duration = 0.3f;
        Vector2 startPos = currentPos;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            canvasGroup.alpha = 1f - t;
            currentPos = startPos - new Vector2(0, t * 20f);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
}
