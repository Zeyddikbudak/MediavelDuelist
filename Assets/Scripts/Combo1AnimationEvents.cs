using UnityEngine;

public class Combo1AnimationEvents : MonoBehaviour
{
    /* Referanslar BattleManager tarafından doldurulur */
    [HideInInspector] public GameObject opponent;
    [HideInInspector] public Animator opponentAnimator;
    [HideInInspector] public BattleManager battleManager;

    public int damageAmount = 10;

    /* Son oynanan hurt klibini hatırlamak için */
    private int lastHurtIndex = -1;

    /* ---------- Animation Event Girişleri ---------- */
    public void Event1() => ApplyDamage(1);
    public void Event2() => ApplyDamage(2);
    public void Event3() => ApplyDamage(3);

    /* ---------------- Ana İşlev ------------------- */
    private void ApplyDamage(int hitIndex)
    {
        /* Dövüş bittiyse ya da rakip zaten ölü ise işlem yapma */
        if (battleManager == null || battleManager.FightOver || battleManager.IsDead(opponent))
            return;

        /* --- HURT KLİBİ SEÇ --- */
        string[] hurtClips = { "Take Damage From Left", "Take Damage From Right" };
        int pick;
        do { pick = Random.Range(0, hurtClips.Length); }
        while (pick == lastHurtIndex);   // art arda aynı klipten kaçın

        string clipName = hurtClips[pick];
        lastHurtIndex = pick;

        /* Klip o an zaten oynuyorsa da restart etmek için Play(normalizedTime=0) */
        opponentAnimator.Play(clipName, 0, 0f);

        /* HP düşür */
        battleManager.ApplyDamage(opponent, damageAmount);

        Debug.Log($"Hit {hitIndex} ▸ {opponent.name}  ({clipName})  -{damageAmount} HP");
    }
}
