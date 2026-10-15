using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class PauseUIAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
{
    private Vector3 originalScale;
    private bool isHovered = false;
    private bool isPressed = false;
    
    // Simple state-based scaling for buttons
    void Awake()
    {
        originalScale = transform.localScale;
    }
    
    void Update()
    {
        if (originalScale == Vector3.zero) return;
        
        float targetScale = 1f;
        if (isPressed) targetScale = 0.95f;
        else if (isHovered) targetScale = 1.05f;
        
        Vector3 target = originalScale * targetScale;
        transform.localScale = Vector3.Lerp(transform.localScale, target, Time.unscaledDeltaTime * 15f);
    }
    
    public void OnPointerEnter(PointerEventData eventData) { isHovered = true; }
    public void OnPointerExit(PointerEventData eventData) { isHovered = false; isPressed = false; }
    public void OnPointerDown(PointerEventData eventData) { isPressed = true; }
    public void OnPointerUp(PointerEventData eventData) { isPressed = false; }
    public void OnSelect(BaseEventData eventData) { isHovered = true; }
    public void OnDeselect(BaseEventData eventData) { isHovered = false; }
}

public class PausePanelAnimator : MonoBehaviour
{
    public CanvasGroup group;
    public Transform contentPanel;
    
    void OnEnable()
    {
        if (group == null) group = GetComponent<CanvasGroup>();
        if (group != null)
        {
            group.alpha = 0f;
            StartCoroutine(FadeIn());
        }
        
        if (contentPanel != null)
        {
            contentPanel.localScale = Vector3.one * 0.85f;
            StartCoroutine(PopIn());
        }
        
        // Select first button
        UnityEngine.UI.Button[] btns = GetComponentsInChildren<UnityEngine.UI.Button>();
        if (btns.Length > 0 && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(btns[0].gameObject);
        }
    }
    
    IEnumerator FadeIn()
    {
        while (group != null && group.alpha < 1f)
        {
            group.alpha += Time.unscaledDeltaTime * 5f; // 0.2s
            yield return null;
        }
        if (group) group.alpha = 1f;
    }
    
    IEnumerator PopIn()
    {
        float t = 0;
        while (contentPanel != null && t < 1f)
        {
            t += Time.unscaledDeltaTime * 4f; // 0.25s
            float ease = 1f - Mathf.Pow(1f - t, 3f);
            
            // Add slight bounce overshoot
            float scale = Mathf.Lerp(0.85f, 1.05f, ease);
            if (t > 0.8f) scale = Mathf.Lerp(1.05f, 1f, (t-0.8f)*5f);
            
            contentPanel.localScale = Vector3.one * scale;
            yield return null;
        }
        if (contentPanel) contentPanel.localScale = Vector3.one;
    }
}
