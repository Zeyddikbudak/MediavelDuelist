using UnityEngine;

public class Attack1AnimationEvents : MonoBehaviour
{
    /* Referanslar BattleManager taraf�ndan doldurulur */
    [HideInInspector] public GameObject opponent;
    [HideInInspector] public Animator opponentAnimator;
    [HideInInspector] public BattleManager battleManager;

    [Header("Hasar Miktar�")]
    public int damageAmount = 10;

    /* Son oynanan hurt klibini hat�rlamak i�in */
    private int lastHurtIndex = -1;

    /* ---------- Animation Event Giri�leri ---------- */
    // Animasyon klibinde vurma an�na ekleyece�in tek event:
    public void OnAttack1Hit() => ApplyDamage();

    /* ---------------- Ana ��lev ------------------- */
    private void ApplyDamage()
    {
        // E�er battleManager yoksa, d�v�� bitti ya da rakip zaten �l� ise ��k
        if (battleManager == null || battleManager.FightOver || battleManager.IsDead(opponent))
            return;

        // Rastgele bir hasar alma klibi se� (soldan veya sa�dan)
        string[] hurtClips = { "Take Damage From Left", "Take Damage From Right" };
        int pick;
        do
        {
            pick = Random.Range(0, hurtClips.Length);
        }
        while (pick == lastHurtIndex);
        lastHurtIndex = pick;

        string clipName = hurtClips[pick];

        // Opponent'�n animator'�nda hurt klibini oynat
        opponentAnimator.Play(clipName, 0, 0f);

        // HP d���r
        battleManager.ApplyDamage(opponent, damageAmount);

        Debug.Log($"Attack1 ? {opponent.name} vuruldu ({clipName}) -{damageAmount} HP");
    }
}
