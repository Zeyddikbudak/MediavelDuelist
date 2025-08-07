using UnityEngine;

/// <summary>
/// Karakterin temel özelliklerini (stats) tutan komponent.
/// Strength     → saldırı hasarına artı değer katar.
/// Dexterity    → animasyon hızını artırır (her +1 Dexterity = +0.01 hız).
/// Constitution → maksimum cana etki eder (ileride).
/// Intelligence → özel yetenekler/mana için (ileride).
/// </summary>
public class CharacterStats : MonoBehaviour
{
    [Header("Primary Stats")]
    [Tooltip("Her +1 Strength, saldırı hasarına +1 ekler.")]
    public int Strength = 1;

    [Tooltip("Her +1 Dexterity, animasyon hızına +0.01 ekler.")]
    public int Dexterity = 1;

    [Tooltip("Her +1 Constitution, maksimum cana +X ekler (ileride).")]
    public int Constitution = 1;

    [Tooltip("Her +1 Intelligence, özel yetenek gücüne etki eder (ileride).")]
    public int Intelligence = 1;
}
