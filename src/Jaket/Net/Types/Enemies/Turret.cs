namespace Jaket.Net.Types;

using Jaket.Content;
using Jaket.IO;

/// <summary> Representation of a turret enemy. </summary>
public class Turret : Enemy
{
    global::Turret turret;

    /// <summary> Id of running animation. </summary>
    /// <see cref="UnityEngine.Animator.StringToHash(string)"/>
    public const int RUNNING_ID = -526601997;

    /// <summary> Whether the turret is aiming or running. </summary>
    private bool aiming, lastAiming, running;

    private void Awake()
    {
        Init(_ => Enemies.Type(EnemyId), true);
        InitTransfer(() => Cooldown(IsOwner ? 0f : 4200f));
        turret = GetComponent<global::Turret>();
    }

    private void Start() => SpawnEffect();

    private void Update()
    {
        if (IsOwner || Dead) return;

        transform.position = new(x.Get(LastUpdate), y.Get(LastUpdate), z.Get(LastUpdate));
        Animator.SetBool(RUNNING_ID, running);

        if (lastAiming != aiming)
        {
            if (lastAiming = aiming)
                turret.Invoke("StartWindup", 0f);
            else
                Events.Post2(() => Cooldown(4200f));
        }
    }

    private void Cooldown(float time) => Tools.Field<global::Turret>("cooldown").SetValue(turret, time);

    #region entity

    public override void Write(Writer w)
    {
        base.Write(w);
        w.Bool(turret.aiming);
        if (!turret.aiming)
        {
            w.Bool(Animator.GetBool(RUNNING_ID));
            w.Vector(transform.position);
        }
    }

    public override void Read(Reader r)
    {
        base.Read(r);
        aiming = r.Bool();
        if (!aiming)
        {
            running = r.Bool();
            x.Read(r); y.Read(r); z.Read(r);
        }
    }

    #endregion
}
