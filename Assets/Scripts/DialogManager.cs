using UnityEngine;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class DialogTrigger
{
    public string triggerName;
    public string speaker; // "Maya" or "Arka"
    public string[] lines;
}

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance { get; private set; }

    public NPC3DController mayaNPC;
    public NPC3DController arkaNPC;

    public DialogBubble mayaBubble;
    public DialogBubble arkaBubble;

    public TMP_Text nameLabelMaya;
    public TMP_Text nameLabelArka;

    [Header("Dialog Config")]
    public float globalCooldown = 4f;
    private float lastDialogTime = -10f;
    
    public List<DialogTrigger> triggers = new List<DialogTrigger>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        SetupDefaultTriggers();
    }

    private void SetupDefaultTriggers()
    {
        if (triggers.Count == 0)
        {
            triggers.Add(new DialogTrigger { triggerName = "StartArka", speaker = "Arka", lines = new string[] { "Kumpulkan koin sebanyak mungkin, tapi hati-hati dengan bom!", "Perhatikan bayangan di tanah, itu tempat benda jatuh.", "Hati-hati dengan batu dan pedang, gunakan untuk berlindung dari bom!" } });
            triggers.Add(new DialogTrigger { triggerName = "StartMaya", speaker = "Maya", lines = new string[] { "Semangat, Kai! Kamu pasti bisa!" } });
            triggers.Add(new DialogTrigger { triggerName = "CoinStreak", speaker = "Maya", lines = new string[] { "Hebat, Kai!", "Terus, terus!", "Kamu keren!" } });
            triggers.Add(new DialogTrigger { triggerName = "Score100", speaker = "Maya", lines = new string[] { "Wah, skor kamu makin tinggi!" } });
            triggers.Add(new DialogTrigger { triggerName = "HitBomb", speaker = "Arka", lines = new string[] { "Tenang, fokus. Jangan terburu-buru.", "Hati-hati, Kai!" } });
            triggers.Add(new DialogTrigger { triggerName = "LowLife", speaker = "Arka", lines = new string[] { "Waspada! Nyawamu tinggal satu." } });
            triggers.Add(new DialogTrigger { triggerName = "FirstDash", speaker = "Arka", lines = new string[] { "Bagus, gunakan dash untuk menghindar!" } });
            triggers.Add(new DialogTrigger { triggerName = "NewRecord", speaker = "Maya", lines = new string[] { "Rekor baru! Luar biasa!" } });
            triggers.Add(new DialogTrigger { triggerName = "GameOverMaya", speaker = "Maya", lines = new string[] { "Jangan menyerah, Kai! Coba lagi!" } });
            triggers.Add(new DialogTrigger { triggerName = "GameOverArka", speaker = "Arka", lines = new string[] { "Setiap percobaan membuatmu lebih baik." } });
        }
    }

    public void HideBubbles()
    {
        if (mayaBubble) mayaBubble.Hide();
        if (arkaBubble) arkaBubble.Hide();
    }

    public void TriggerDialog(string triggerName, bool ignoreCooldown = false)
    {
        if (GameManager3D.Instance != null && GameManager3D.Instance.CurrentState == GameManager3D.GameState.GameOver) return; // Don't show floating bubbles on Game Over

        if (!ignoreCooldown && Time.time - lastDialogTime < globalCooldown) return;

        DialogTrigger dt = triggers.Find(x => x.triggerName == triggerName);
        if (dt != null && dt.lines.Length > 0)
        {
            string line = dt.lines[UnityEngine.Random.Range(0, dt.lines.Length)];
            
            if (dt.speaker == "Maya" && mayaNPC != null && mayaBubble != null)
            {
                ColorUtility.TryParseHtmlString("#FBC437", out Color mayaCol);
                mayaBubble.Show("Maya", line, mayaNPC.transform, mayaCol);
                if (nameLabelMaya != null) { nameLabelMaya.text = "Maya"; nameLabelMaya.color = ColorUtility.TryParseHtmlString("#2B2B2B", out Color darkCol) ? darkCol : Color.black; }
                mayaNPC.React();
                lastDialogTime = Time.time;
            }
            else if (dt.speaker == "Arka" && arkaNPC != null && arkaBubble != null)
            {
                ColorUtility.TryParseHtmlString("#5BB04D", out Color arkaCol);
                arkaBubble.Show("Pak Arka", line, arkaNPC.transform, arkaCol);
                if (nameLabelArka != null) { nameLabelArka.text = "Pak Arka"; nameLabelArka.color = Color.white; }
                arkaNPC.React();
                lastDialogTime = Time.time;
            }
        }
    }
}
