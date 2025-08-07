using UnityEngine;

public class PowerAttack2AnimationEvents : MonoBehaviour
{
    /* BattleManager tarafından PrepareFightUI tarafından doldurulacak referanslar */
    [HideInInspector] public GameObject opponent;
    [HideInInspector] public Animator opponentAnimator;
    [HideInInspector] public BattleManager battleManager;

    [Header("Hasar Miktarı")]
    public int damageAmount = 20;  // Power Attack 2’ye özel hasar miktarı

    // Son oynanan hurt klibini hatırlamak için
    private int lastHurtIndex = -1;

    /* ---------- Animation Event Girişleri ---------- */
    // Animasyon klibinde isabet anına ekleyeceğin tek event:
    public void OnPowerAttack2Hit() => ApplyDamage();

    /* ---------------- Ana İşlev ------------------- */
    private void ApplyDamage()
    {
        // BattleManager atanmamışsa, dövüş bittiyse veya rakip ölmüşse çık
        if (battleManager == null || battleManager.FightOver || battleManager.IsDead(opponent))
            return;

        // Rastgele bir hurt klibi seç (sol/sağ hasar reaksiyonu)
        string[] hurtClips = { "Take Damage From Left", "Take Damage From Right" };
        int pick;
        do
        {
            pick = Random.Range(0, hurtClips.Length);
        }
        while (pick == lastHurtIndex);
        lastHurtIndex = pick;

        string clipName = hurtClips[pick];

        // Opponent’ın animator’ında hurt klibini oynat
        opponentAnimator.Play(clipName, 0, 0f);

        // Rakibe hasarı uygula
        battleManager.ApplyDamage(opponent, damageAmount);

        Debug.Log($"PowerAttack2 ► {opponent.name} vuruldu ({clipName}) -{damageAmount} HP");
    }
}
