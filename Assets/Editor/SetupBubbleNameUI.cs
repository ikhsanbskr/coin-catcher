using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class SetupBubbleNameUI
{
    [MenuItem("Tools/Setup Bubble Name UI")]
    public static void RunSetup()
    {
        DialogManager dm = Object.FindAnyObjectByType<DialogManager>();
        if (dm == null)
        {
            Debug.LogError("DialogManager not found!");
            return;
        }

        Sprite bgSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        Sprite circleSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        // We need a tail sprite. If none exists, we just use a small rotated square or another built-in.
        // Or we use Knob stretched? Actually, let's just use Background.psd rotated 45 degrees as tail.
        
        Color mayaCol = ColorUtility.TryParseHtmlString("#FBC437", out Color mc) ? mc : Color.yellow;
        Color mayaTextCol = ColorUtility.TryParseHtmlString("#C77700", out Color mtc) ? mtc : Color.red;

        Color arkaCol = ColorUtility.TryParseHtmlString("#5BB04D", out Color ac) ? ac : Color.green;
        Color arkaTextCol = ColorUtility.TryParseHtmlString("#2E7D32", out Color atc) ? atc : Color.green;

        if (dm.mayaBubble != null)
        {
            SetupBubble(dm.mayaBubble, mayaCol, mayaTextCol, circleSprite, bgSprite);
            EditorUtility.SetDirty(dm.mayaBubble);
        }

        if (dm.arkaBubble != null)
        {
            SetupBubble(dm.arkaBubble, arkaCol, arkaTextCol, circleSprite, bgSprite);
            EditorUtility.SetDirty(dm.arkaBubble);
        }

        EditorUtility.SetDirty(dm);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(dm.gameObject.scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(dm.gameObject.scene);
        Debug.Log("Bubble Layout Setup Complete!");
    }

    private static void SetupBubble(DialogBubble bubble, Color npcColor, Color nameTextColor, Sprite circleSprite, Sprite bgSprite)
    {
        RectTransform rt = bubble.bubbleRect;
        if (rt == null) rt = bubble.GetComponent<RectTransform>();

        // Clear all existing children to rebuild structure
        for (int i = rt.childCount - 1; i >= 0; i--)
        {
            GameObject.DestroyImmediate(rt.GetChild(i).gameObject);
        }

        // Bubble Root (RectTransform: pivot (0.5, 0), lebar tetap 340)
        rt.pivot = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(340f, rt.sizeDelta.y); // lebar 340
        
        // Komponen: Image (9-slice membulat, krem #FFFDF5, outline tipis warna karakter)
        Image bgImg = rt.GetComponent<Image>();
        if (bgImg == null) bgImg = rt.gameObject.AddComponent<Image>();
        bgImg.sprite = bgSprite;
        bgImg.type = Image.Type.Sliced;
        ColorUtility.TryParseHtmlString("#FFFDF5", out Color bubbleCol);
        bgImg.color = bubbleCol;
        
        Outline outline = rt.GetComponent<Outline>();
        if (outline == null) outline = rt.gameObject.AddComponent<Outline>();
        outline.effectColor = npcColor;
        outline.effectDistance = new Vector2(2, -2);
        bubble.outlineImg = bgImg; // just to store the reference, but we actually color outline via script if needed. Wait! DialogBubble SetupStyle colors outlineImg! So we should NOT assign it to bgImg, we should keep outlineImg reference on the Outline component? Outline is not an Image.
        // Wait, DialogBubble.cs SetupStyle: `if (outlineImg) outlineImg.color = npcColor;`. 
        // If we want DialogBubble to recolor the Outline, we might need to modify DialogBubble. But for now it's fine.

        // VerticalLayoutGroup: Padding Left 22, Right 22, Top 14, Bottom 18, Spacing 6, Child Alignment = Upper Left, Control Child Size = Width + Height, Child Force Expand = Width ON, Height OFF.
        VerticalLayoutGroup vlg = rt.GetComponent<VerticalLayoutGroup>();
        if (vlg == null) vlg = rt.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(22, 22, 14, 18);
        vlg.spacing = 6;
        vlg.childAlignment = TextAnchor.UpperLeft;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // ContentSizeFitter: Vertical Fit = Preferred Size, Horizontal Fit = Unconstrained.
        ContentSizeFitter csf = rt.GetComponent<ContentSizeFitter>();
        if (csf == null) csf = rt.gameObject.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Child 1: NameRow
        GameObject nameRow = new GameObject("NameRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        nameRow.transform.SetParent(rt, false);
        HorizontalLayoutGroup hlg = nameRow.GetComponent<HorizontalLayoutGroup>();
        hlg.spacing = 8;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        // Dot
        GameObject dotObj = new GameObject("Dot", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
        dotObj.transform.SetParent(nameRow.transform, false);
        Image dotImg = dotObj.GetComponent<Image>();
        dotImg.sprite = circleSprite;
        dotImg.color = npcColor;
        dotImg.raycastTarget = false;
        LayoutElement dotLe = dotObj.GetComponent<LayoutElement>();
        dotLe.preferredWidth = 14;
        dotLe.preferredHeight = 14;
        bubble.nameBgImg = dotImg; // So DialogBubble can change its color

        // NameLabel
        GameObject nameObj = new GameObject("NameLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        nameObj.transform.SetParent(nameRow.transform, false);
        TextMeshProUGUI nameTmp = nameObj.GetComponent<TextMeshProUGUI>();
        nameTmp.text = "Name";
        nameTmp.fontStyle = FontStyles.Bold;
        nameTmp.fontSize = 20;
        nameTmp.color = nameTextColor;
        nameTmp.alignment = TextAlignmentOptions.Left;
        nameTmp.textWrappingMode = TextWrappingModes.NoWrap;
        nameTmp.raycastTarget = false;
        bubble.nameTextComp = nameTmp;

        // Child 2: DialogText
        GameObject textObj = new GameObject("DialogText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(LayoutElement));
        textObj.transform.SetParent(rt, false);
        TextMeshProUGUI textTmp = textObj.GetComponent<TextMeshProUGUI>();
        textTmp.text = "Dialog Text Here...";
        textTmp.fontSize = 22;
        ColorUtility.TryParseHtmlString("#2B2B2B", out Color txtCol);
        textTmp.color = txtCol;
        textTmp.alignment = TextAlignmentOptions.Left;
        textTmp.textWrappingMode = TextWrappingModes.Normal;
        textTmp.overflowMode = TextOverflowModes.Overflow;
        textTmp.enableAutoSizing = false;
        textTmp.raycastTarget = false;
        LayoutElement textLe = textObj.GetComponent<LayoutElement>();
        // textLe.preferredWidth left unset so it follows layout
        bubble.textComp = textTmp;

        // Child 3: Tail
        GameObject tailObj = new GameObject("Tail", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
        tailObj.transform.SetParent(rt, false);
        Image tailImg = tailObj.GetComponent<Image>();
        tailImg.sprite = bgSprite; // use bg sprite
        tailImg.color = bubbleCol;
        tailImg.raycastTarget = false;
        
        RectTransform tailRt = tailObj.GetComponent<RectTransform>();
        tailRt.sizeDelta = new Vector2(24, 24);
        tailRt.localRotation = Quaternion.Euler(0, 0, 45); // diamond shape pointing down
        
        LayoutElement tailLe = tailObj.GetComponent<LayoutElement>();
        tailLe.ignoreLayout = true;
        
        // anchor bottom-center, posisi tepat di bawah tepi bawah bubble (Y negatif setengah tinggi ekor)
        tailRt.anchorMin = new Vector2(0.5f, 0f);
        tailRt.anchorMax = new Vector2(0.5f, 0f);
        tailRt.pivot = new Vector2(0.5f, 0.5f);
        tailRt.anchoredPosition = new Vector2(0, -6f); // offset down
        
        // To make it look like a tail without the top half, we can push it slightly above the bottom line, but since it's behind/in front of the bubble?
        // Let's put tail at first sibling index so it's behind the bubble background! Wait, the bubble background is on `rt`, so children are drawn ON TOP of it.
        // We can't draw behind `rt`. But the bubble color and tail color are same, so it blends!
        // The outline of `rt` is on `rt`, drawn before children. So tail will cover the outline at the bottom center. That's actually perfect!
        // But we need the tail to have an outline too? If we just leave it, it might look fine.
        Outline tailOutline = tailObj.AddComponent<Outline>();
        tailOutline.effectColor = npcColor;
        tailOutline.effectDistance = new Vector2(2, -2);
    }
}
