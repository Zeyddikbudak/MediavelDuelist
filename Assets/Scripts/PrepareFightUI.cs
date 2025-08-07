using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;  // Eğer TextMeshPro kullanıyorsan, değilse kaldırabilirsin

public class PrepareFightUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform slotParent;
    [SerializeField] private SkillSlot slotPrefab;
    [SerializeField] private Button skillButtonPrefab;
    [SerializeField] private Transform skillContentParent;
    [SerializeField] private Button startButton;

    [Header("Gameplay")]
    [SerializeField] private BattleManager battleManager;

    private SkillSlot[] slots;

    void Awake()
    {
        slots = slotParent.GetComponentsInChildren<SkillSlot>(true);
        if (slots.Length == 0)
        {
            if (slotPrefab == null)
            {
                Debug.LogError("PrepareFightUI ► slotPrefab atanmamış!", this);
                return;
            }
            slots = new SkillSlot[6];
            for (int i = 0; i < slots.Length; i++)
                slots[i] = Instantiate(slotPrefab, slotParent, false);

            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)slotParent);
        }
    }

    void Start()
    {
        LoadSkills();

        if (startButton == null)
            Debug.LogError("PrepareFightUI ► startButton atanmamış!", this);
        else
            startButton.onClick.AddListener(OnStartFight);
    }

    private void LoadSkills()
    {
        var all = Resources.LoadAll<SkillData>("Skills");
        Debug.Log($"PrepareFightUI ► {all.Length} skill yüklendi.");

        foreach (var sd in all)
        {
            if (!sd.isAttack) continue; // savunma yeteneklerini listeleme
            var copy = sd;
            var btn = Instantiate(skillButtonPrefab, skillContentParent, false);
            btn.name = $"SkillBtn_{copy.name}";

            // Boyut sabitle
            var rt = btn.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(80, 80);
            var le = btn.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = 80;
            le.preferredHeight = 80;

            // Icon
            var img = btn.GetComponent<Image>();
            if (img != null) img.sprite = copy.icon;

            // Label (TextMeshProUGUI)
            var label = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = copy.name;

            btn.onClick.AddListener(() => TryAssignToFreeSlot(copy));
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)skillContentParent);
    }

    private void TryAssignToFreeSlot(SkillData data)
    {
        foreach (var s in slots)
            if (s.Assign(data))
                return;
        Debug.Log("PrepareFightUI ► Boş slot kalmadı!");
    }

    private void OnStartFight()
    {
        if (!ValidateSelection()) return;

        // Seçilen animasyon klip adlarını topla
        var attack = new List<string>();
        var defend = new List<string>();
        foreach (var s in slots)
        {
            if (s.Assigned == null) continue;
            if (s.Assigned.isAttack)
                attack.Add(s.Assigned.animatorClipName);
            else
                defend.Add(s.Assigned.animatorClipName);
        }

        // Eğer savunma listesi tamamen boşsa, P2 için ilk saldırıyı 3 kez ekle
        if (defend.Count == 0 && attack.Count > 0)
        {
            string fallback = attack[0];
            for (int i = 0; i < 3; i++)
                defend.Add(fallback);
            Debug.Log($"PrepareFightUI ► Def listesi boştu, P2 için varsayılan '{fallback}' atandı.");
        }

        if (battleManager == null)
        {
            Debug.LogError("PrepareFightUI ► battleManager atanmamış!", this);
            return;
        }

        Debug.Log($"PrepareFightUI ► StartBattle çağrılıyor. Atk:{attack.Count}, Def:{defend.Count}");
        battleManager.SetPlayerSkillSet(attack.ToArray(), defend.ToArray());
        battleManager.StartBattle();

        // UI nesnesi ile BattleManager aynı GameObject üzerinde olabilir. Bu durumda
        // tüm GameObject'i kapatmak, savaş döngüsünü de sonlandıracağı için yalnızca
        // UI bileşenlerini gizle.
        if (battleManager && battleManager.gameObject == gameObject)
        {
            if (slotParent) slotParent.gameObject.SetActive(false);
            if (skillContentParent) skillContentParent.gameObject.SetActive(false);
            if (startButton) startButton.gameObject.SetActive(false);
            enabled = false; // sadece bu script'i devre dışı bırak
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private bool ValidateSelection()
    {
        int filled = 0;
        foreach (var s in slots)
            if (!s.IsEmpty) filled++;

        if (filled < 3)
        {
            Debug.LogWarning("PrepareFightUI ► En az 3 skill seçmelisin!");
            return false;
        }
        return true;
    }
}
