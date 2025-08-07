using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SkillSlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Button clearBtn;

    public SkillData Assigned { get; private set; }
    public bool IsEmpty => Assigned == null;

    void Awake()
    {
        // Inspector boþ býrakýlmýþsa çocuk objelerden çek
        if (!iconImage) iconImage = GetComponent<Image>();
        if (!clearBtn) clearBtn = GetComponentInChildren<Button>(true);

        if (clearBtn) clearBtn.onClick.AddListener(Clear);
        Clear();
    }

    public bool Assign(SkillData data)
    {
        if (!IsEmpty) return false;

        Assigned = data;
        iconImage.enabled = true;
        iconImage.sprite = data.icon;
        if (clearBtn) clearBtn.gameObject.SetActive(true);
        return true;
    }

    public void Clear()
    {
        Assigned = null;
        iconImage.enabled = false;
        if (clearBtn) clearBtn.gameObject.SetActive(false);
    }
}
