using UnityEngine;

public class Attack1AnimationEvents : MonoBehaviour
{
    /* Referanslar BattleManager tarafýndan doldurulur */
    [HideInInspector] public GameObject opponent;
    [HideInInspector] public Animator opponentAnimator;
    [HideInInspector] public BattleManager battleManager;

    [Header("Hasar Miktarý")]
    public int damageAmount = 10;

    /* Son oynanan hurt klibini hatýrlamak için */
    private int lastHurtIndex = -1;

    /* ---------- Animation Event Giriþleri ---------- */
    // Animasyon klibinde vurma anýna ekleyeceðin tek event:
    public void OnAttack1Hit() => ApplyDamage();

    /* ---------------- Ana Ýþlev ------------------- */
    private void ApplyDamage()
    {
        // Eðer battleManager yoksa, dövüþ bitti ya da rakip zaten ölü ise çýk
        if (battleManager == null || battleManager.FightOver || battleManager.IsDead(opponent))
            return;

        // Rastgele bir hasar alma klibi seç (soldan veya saðdan)
        string[] hurtClips = { "Take Damage From Left", "Take Damage From Right" };
        int pick;
        do
        {
            pick = Random.Range(0, hurtClips.Length);
        }
        while (pick == lastHurtIndex);
        lastHurtIndex = pick;

        string clipName = hurtClips[pick];

        // Opponent'ýn animator'ýnda hurt klibini oynat
        opponentAnimator.Play(clipName, 0, 0f);

        // HP düþür
        battleManager.ApplyDamage(opponent, damageAmount);

        Debug.Log($"Attack1 ? {opponent.name} vuruldu ({clipName}) -{damageAmount} HP");
    }
}
