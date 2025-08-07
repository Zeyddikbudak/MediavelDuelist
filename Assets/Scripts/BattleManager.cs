using UnityEngine;
using System.Collections;

public enum AttackOutcome { None, Hit, Blocked, Dodged }

/// <summary>Tur-bazlı düello yöneticisi – sadeleştirilmiş sürüm</summary>
public class BattleManager : MonoBehaviour
{
    #region Inspector
    [Header("Karakter Objeleri")]
    public GameObject player1;
    public GameObject player2;

    [Header("Başlangıç Canı")][SerializeField] private int startHP = 100;

    [Header("Saldırı Dizileri")]
    private string[] p1Skills = new string[0];
    private string[] p2Skills = new string[0];

    [Header("Temel Klipler")]
    public string idleClip = "Standing";
    public string dieClip = "Dying";

    [Header("Dodge Klipleri")]
    public string dodgeClip = "Dodge";
    public string dodgeForwardClip = "Dodge Forward";

    [Header("Block Klipleri")]
    public string blockClip = "Block";
    public string defenderBlockReactClip = "Defender Block React";
    [Tooltip("Defender Block React bittikten sonra Standing'e blend")]
    public float defenderBlendTime = 0.15f;
    #endregion

    #region Alanlar
    private int p1HP, p2HP;
    private int p1Slot, p2Slot;
    private bool p1Turn = true;
    private bool fightOver = false;
    private GameObject currentAttacker;
    public AttackOutcome LastAttackOutcome { get; private set; } = AttackOutcome.None;
    #endregion

    #region Özellikler
    public bool FightOver => fightOver;
    public bool IsDead(GameObject g) => g == player1 ? p1HP <= 0 : p2HP <= 0;
    #endregion

    #region Unity
    private void Awake()
    {
        SetFixedAnimator(player1);
        SetFixedAnimator(player2);
    }
    #endregion

    #region Kurulum
    private void SetFixedAnimator(GameObject g)
    {
        var a = g.GetComponentInChildren<Animator>();
        if (a) { a.updateMode = AnimatorUpdateMode.Fixed; a.animatePhysics = true; }
    }

    public void SetPlayerSkillSet(string[] p1Attacks, string[] p2Attacks)
    {
        p1Skills = p1Attacks; p2Skills = p2Attacks;
    }

    public void StartBattle()
    {
        p1HP = p2HP = startHP;
        p1Slot = p2Slot = 0; p1Turn = true; fightOver = false;
        LastAttackOutcome = AttackOutcome.None;

        PlayIdle(player1.GetComponentInChildren<Animator>());
        PlayIdle(player2.GetComponentInChildren<Animator>());
        StartCoroutine(MainLoop());
    }
    #endregion

    #region Ana Döngü
    private IEnumerator MainLoop()
    {
        while (!fightOver)
        {
            if (p1Turn)
            {
                yield return RunSlot(player1, player2, p1Skills[p1Slot]);
                p1Slot = (p1Slot + 1) % p1Skills.Length;
                p1Turn = false;
            }
            else
            {
                yield return RunSlot(player2, player1, p2Skills[p2Slot]);
                p2Slot = (p2Slot + 1) % p2Skills.Length;
                p1Turn = true;
            }
            yield return new WaitForSeconds(.4f);
        }
    }
    #endregion

    #region Slot İşleyicisi
    private IEnumerator RunSlot(GameObject atk, GameObject def, string attackClip)
    {
        currentAttacker = atk;

        var atkStats = atk.GetComponent<CharacterStats>();
        var defStats = def.GetComponent<CharacterStats>();

        /* 1) Dodge / Block kararı */
        bool dodged = false, blocked = false;
        if (defStats)
        {
            if (Random.value < defStats.Dexterity * .02f) dodged = true;
            else if (Random.value < defStats.Strength * .02f) blocked = true;
        }
        LastAttackOutcome = dodged ? AttackOutcome.Dodged
                                   : blocked ? AttackOutcome.Blocked
                                             : AttackOutcome.Hit;

        /* 2) Animator referansları */
        var atkAnim = atk.GetComponentInChildren<Animator>();
        var defAnim = def.GetComponentInChildren<Animator>();

        /* 3) Hit-event script’leri */
        bool enableHandlers = !(dodged || blocked);
        if (blocked && attackClip.StartsWith("Attack 2")) enableHandlers = true;
        EnableHitHandlers(atk, def, defAnim, attackClip, enableHandlers);

        /* 4) Saldıran hız bonusu */
        float origSpeed = atkAnim.speed;
        if (atkStats) atkAnim.speed = 1f + atkStats.Dexterity * .01f;

        /* 5) Saldırı animasyonu */
        while (atkAnim.IsInTransition(0)) yield return null;
        atkAnim.CrossFade(attackClip, 0f);

        /* 6) Savunma senaryoları */
        if (dodged)
        {
            yield return PlayDodgeSequence(defAnim);
        }
        else if (blocked)
        {
            if (attackClip.StartsWith("Attack 2"))
            {
                if (defAnim.HasState(0, Animator.StringToHash(blockClip)))
                {
                    defAnim.CrossFade(blockClip, 0f, 0);
                    // Anında state'e geçiş yapıldı mı kontrolü için Update(0)
                    defAnim.Update(0f);
                    Debug.Log($"RunSlot ► Defender {def.name} Attack2'de block state'ine geçti");
                }
                yield return new WaitForSeconds(ClipLen(atkAnim, attackClip) + .05f);
            }
            else
            {
                yield return PlayBlockSequence(defAnim);   // attacker react kaldırıldı
                atkAnim.speed = origSpeed;
                PlayIdle(atkAnim);
                yield break;
            }
        }
        else
        {
            yield return new WaitForSeconds(ClipLen(atkAnim, attackClip) + .05f);
        }

        atkAnim.speed = origSpeed;
        if (!(blocked && attackClip.StartsWith("Attack 2")))
            PlayIdle(atkAnim);
        if (!dodged && !(blocked && attackClip.StartsWith("Attack 2")))
            PlayIdle(defAnim);
    }
    #endregion

    #region Dodge
    private IEnumerator PlayDodgeSequence(Animator defAnim)
    {
        if (!defAnim) yield break;

        float originalSpeed = defAnim.speed;
        defAnim.speed = 1.4f;

        bool originalRoot = defAnim.applyRootMotion;
        defAnim.applyRootMotion = false;

        int dodgeId = Animator.StringToHash(dodgeClip);
        if (defAnim.HasState(0, dodgeId))
        {
            defAnim.CrossFade(dodgeId, 0f, 0);
            defAnim.Update(0f);
            yield return new WaitForSeconds(ClipLen(defAnim, dodgeClip) / defAnim.speed);
        }

        int forwardId = Animator.StringToHash(dodgeForwardClip);
        if (defAnim.HasState(0, forwardId))
        {
            defAnim.CrossFade(forwardId, 0f, 0);
            defAnim.Update(0f);
            yield return new WaitForSeconds(ClipLen(defAnim, dodgeForwardClip) / defAnim.speed);
        }

        defAnim.applyRootMotion = originalRoot;
        defAnim.speed = originalSpeed;

    }
    #endregion

    

    #region Block (yalnızca defender react)
    private IEnumerator PlayBlockSequence(Animator defAnim)
    {
        /* Defender hemen Block klibine geçer */
        if (defAnim.HasState(0, Animator.StringToHash(blockClip)))
            defAnim.CrossFade(blockClip, 0f, 0);

        /* Block klibi bitince Defender React oynat */
        yield return new WaitForSeconds(ClipLen(defAnim, blockClip));

        if (defAnim.HasState(0, Animator.StringToHash(defenderBlockReactClip)))
            defAnim.CrossFade(defenderBlockReactClip, 0f, 0);

        yield return new WaitForSeconds(ClipLen(defAnim, defenderBlockReactClip));

        /* Idle’a blend */
        defAnim.CrossFade(idleClip, defenderBlendTime, 0);
    }
    #endregion

    #region Hit-Handler Yönetimi (kısaltılmış)
    private void EnableHitHandlers(GameObject atk, GameObject def, Animator defAnim,
                                   string clip, bool enable)
    {
        foreach (var h in atk.GetComponentsInChildren<MonoBehaviour>(true))
            if (h is Combo1AnimationEvents or Attack1AnimationEvents or Attack2AnimationEvents
                or PowerAttack2AnimationEvents or Attack360AnimationEvents)
                h.enabled = false;

        if (!enable) return;

        void Set(MonoBehaviour h)
        {
            h.enabled = true;
            switch (h)
            {
                case Combo1AnimationEvents c1: c1.opponent = def; c1.opponentAnimator = defAnim; c1.battleManager = this; break;
                case Attack1AnimationEvents a1: a1.opponent = def; a1.opponentAnimator = defAnim; a1.battleManager = this; break;
                case Attack2AnimationEvents a2: a2.opponent = def; a2.opponentAnimator = defAnim; a2.battleManager = this; break;
                case PowerAttack2AnimationEvents p2: p2.opponent = def; p2.opponentAnimator = defAnim; p2.battleManager = this; break;
                case Attack360AnimationEvents a360: a360.opponent = def; a360.opponentAnimator = defAnim; a360.battleManager = this; break;
            }
        }

        if (clip.StartsWith("Combo 1"))
        {
            foreach (var c in atk.GetComponentsInChildren<Combo1AnimationEvents>(true)) Set(c);
        }
        else if (clip.StartsWith("Attack 1"))
        {
            foreach (var a1 in atk.GetComponentsInChildren<Attack1AnimationEvents>(true)) Set(a1);
        }
        else if (clip.StartsWith("Attack 2"))
        {
            foreach (var a2 in atk.GetComponentsInChildren<Attack2AnimationEvents>(true)) Set(a2);
        }
        else if (clip.StartsWith("Power Attack 2"))
        {
            foreach (var p2 in atk.GetComponentsInChildren<PowerAttack2AnimationEvents>(true)) Set(p2);
        }
        else if (clip.Contains("360"))
        {
            foreach (var a360 in atk.GetComponentsInChildren<Attack360AnimationEvents>(true)) Set(a360);
        }
    }
    #endregion

        #region Hasar & Ölüm
    public void ApplyDamage(GameObject defender, int baseDamage)
    {
        int dmg = baseDamage;
        var stats = currentAttacker?.GetComponent<CharacterStats>();
        if (stats) dmg += stats.Strength;

        if (defender == player1) p1HP -= dmg;
        else p2HP -= dmg;

        if (!fightOver && IsDead(defender))
        {
            fightOver = true;
            TriggerDeath(defender);
        }
    }

    private void TriggerDeath(GameObject victim)
    {
        var anim = victim.GetComponentInChildren<Animator>();
        if (anim)
        {
            int id = Animator.StringToHash(dieClip);
            if (anim.HasState(0, id)) anim.CrossFade(id, 0f, 0);
        }
    }
    #endregion

    #region Yardımcı
    private float ClipLen(Animator anim, string clip, float defLen = .6f)
    {
        foreach (var c in anim.runtimeAnimatorController.animationClips)
            if (c.name == clip) return c.length;
        return defLen;
    }
    private void PlayIdle(Animator anim)
    {
        if (!anim) return;
        int id = Animator.StringToHash(idleClip);
        if (anim.HasState(0, id)) anim.CrossFade(id, 0f, 0);
    }
    #endregion
}
