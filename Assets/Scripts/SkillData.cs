/* Assets/Scripts/SkillData.cs */
using UnityEngine;

[CreateAssetMenu(menuName = "Fight/Skill")]
public class SkillData : ScriptableObject
{
    public string skillName;           // "Attack 1"
    public string animatorClipName;    // "Combo 1" vb.
    public Sprite icon;
    public bool isAttack = true;     // false = defence
}
