using UnityEngine;

public class Attack2AnimationEvents : MonoBehaviour
{
    /* Referanslar BattleManager tarafından atanır */
    [HideInInspector] public GameObject opponent;
    [HideInInspector] public Animator opponentAnimator;
    [HideInInspector] public BattleManager battleManager;

    [Header("Hasar Miktarı")]
    public int damageAmount = 15;

    [Header("State / Klip Adları")]
    public string blockStateName = "Block";
    public string defenderBlockReactClip = "Defender Block React";
    public string attackerIdleClip = "Standing";

    [Range(0.05f, .2f)]
    public float rewindSeconds = 0.10f;

    /* event tetikleyici */
    public void OnAttack2Hit() => HandleHit();

    /* ------------------ */
    private void HandleHit()
    {
        if (battleManager == null || battleManager.FightOver) return;

        if (IsBlocked())
        {
            PlayDefenderReact();
            RewindAttacker();
            return;  // Block’ta hasar uygulanmaz
        }

        ApplyDamageNormal();
    }

    /* Block tespiti: geçerli state veya geçişteki next-state Block ise true */
    private bool IsBlocked()
    {
        var info = opponentAnimator.GetCurrentAnimatorStateInfo(0);
        if (info.IsName(blockStateName)) return true;

        if (opponentAnimator.IsInTransition(0))
        {
            var next = opponentAnimator.GetNextAnimatorStateInfo(0);
            if (next.IsName(blockStateName)) return true;
        }
        return false;
    }

    /* Defender için anında Block React */
    /* Defender için anında Block React ve ardından Standing */
    private void PlayDefenderReact()
    {
        int hash = Animator.StringToHash(defenderBlockReactClip);
        if (opponentAnimator.HasState(0, hash))
        {
            opponentAnimator.Play(hash, 0, 0f);
            battleManager.StartCoroutine(ReturnDefenderToIdle());
        }
    }

    private System.Collections.IEnumerator ReturnDefenderToIdle()
    {
        float len = ClipLength(opponentAnimator, defenderBlockReactClip);
        yield return new WaitForSeconds(len);

        int idleHash = Animator.StringToHash(attackerIdleClip);
        if (opponentAnimator.HasState(0, idleHash))
            opponentAnimator.CrossFade(idleHash, 0.05f, 0);
    }

    private float ClipLength(Animator anim, string clip, float def = 0.6f)
    {
        foreach (var c in anim.runtimeAnimatorController.animationClips)
            if (c.name == clip) return c.length;
        return def;
    }

    /* Saldıran animasyonu 0.10 s geri sarıp kaldığı yerden devam ettir */
    private void RewindAttacker()
    {
        var anim = GetComponentInParent<Animator>();
        if (!anim) return;

        var info = anim.GetCurrentAnimatorStateInfo(0);
        float clipLen = info.length > 0f ? info.length : 0.5f;

        float newNorm = Mathf.Max(0f, info.normalizedTime - rewindSeconds / clipLen);
        anim.Play(info.fullPathHash, 0, newNorm);  // Aynı state, geriye sar
        anim.Update(0f);                           // Hemen uygula

        // İstersen idle’a da geç:
        int idleHash = Animator.StringToHash(attackerIdleClip);
        if (anim.HasState(0, idleHash))
            anim.CrossFade(idleHash, 0.05f, 0);
    }

    /* ---------- Standart Hasar Akışı ---------- */
    private int lastHurtIndex = -1;
    private void ApplyDamageNormal()
    {
        if (battleManager.IsDead(opponent)) return;

        string[] hurt = { "Take Damage From Left", "Take Damage From Right" };
        int pick;
        do { pick = Random.Range(0, hurt.Length); }
        while (pick == lastHurtIndex);
        lastHurtIndex = pick;

        opponentAnimator.Play(hurt[pick], 0, 0f);
        battleManager.ApplyDamage(opponent, damageAmount);
    }
}
