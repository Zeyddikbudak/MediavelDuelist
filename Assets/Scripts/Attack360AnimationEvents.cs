using UnityEngine;

public class Attack360AnimationEvents : MonoBehaviour
{
    /* BattleManager tarafından PrepareFightUI tarafından doldurulacak referanslar */
    [HideInInspector] public GameObject opponent;
    [HideInInspector] public Animator opponentAnimator;
    [HideInInspector] public BattleManager battleManager;

    [Header("Hasar Miktarı")]
    public int damageAmount = 25;  // 360 Attack’e özel hasar miktarı

    // Son oynanan hurt klibini hatırlamak için
    private int lastHurtIndex = -1;

    /* ---------- Animation Event Girişi ---------- */
    // Animasyon klibindeki isabet anına ekleyeceğin tek event:
    public void On360AttackHit() => ApplyDamage();

    /* ---------------- Ana İşlev ------------------- */
    private void ApplyDamage()
    {
        // BattleManager yoksa, dövüş bittiyse ya da rakip ölü ise çık
        if (battleManager == null || battleManager.FightOver || battleManager.IsDead(opponent))
            return;

        // Rastgele bir hurt klibi seç (soldan/sağdan hasar animasyonu)
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

        Debug.Log($"Attack360 ► {opponent.name} vuruldu ({clipName}) -{damageAmount} HP");
    }
}
