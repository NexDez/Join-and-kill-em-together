namespace Jaket.Net.Types;

using UnityEngine;

using Jaket.IO;

/// <summary> Common to all enemies in the game class. Performs a small number of functions. </summary>
public class Enemy : OwnableEntity
{
    /// <summary> Position of the enemy in the world space. </summary>
    protected FloatLerp x = new(), y = new(), z = new();

    /// <summary> Increases the health of the enemy and adds a boss bar when certain conditions are reached. </summary>
    protected void Boss(bool cond, float bossHealth, int layers = 1, string nameOverride = null)
    {
        if (!cond) return;
        if (!LobbyController.IsOwner) float.TryParse(LobbyController.Lobby?.GetData("ppp"), out LobbyController.PPP);
        LobbyController.ScaleHealth(ref bossHealth);

        // override the name of the enemy
        if (nameOverride != null) Tools.Field<EnemyIdentifier>("overrideFullName").SetValue(EnemyId, nameOverride);

        // boss health can be very different from the health of its prefab
        if (EnemyId.drone) EnemyId.drone.health = bossHealth;
        if (EnemyId.spider) EnemyId.spider.health = bossHealth;
        if (EnemyId.zombie) EnemyId.zombie.health = bossHealth;
        if (EnemyId.statue) EnemyId.statue.health = bossHealth;
        if (EnemyId.machine) EnemyId.machine.health = bossHealth;

        // Minos' hand has no boss bar
        if (layers == 0) return;

        // create a boss bar or update the already existing one
        if (!TryGetComponent(out BossHealthBar bar)) bar = gameObject.AddComponent<BossHealthBar>();

        bar.healthLayers = new HealthLayer[layers];
        for (int i = 0; i < layers; i++) bar.healthLayers[i] = new() { health = bossHealth / layers };

        bar.enabled = false;
        Events.Post(() => bar.enabled = true);
    }

    /// <summary> Repeats the original spawn effect for clients. </summary>
    protected void SpawnEffect()
    {
        if (IsOwner) return;

        transform.position = new(x.Last = x.Target, y.Last = y.Target, z.Last = z.Target);
        if (EnemyId.spawnEffect)
            Instantiate(EnemyId.spawnEffect, TryGetComponent(out Collider col) ? col.bounds.center : transform.position, transform.rotation);
    }

    public override void Kill(Reader r)
    {
        if (!Dead) OnDied();
        if (r != null) Kill();
    }

    /// <summary> This method is called after the death of the enemy, local or caused remotely. </summary>
    public virtual void OnDied() => base.Kill(null);

    /// <summary> This method is called only after the remote death of the enemy. </summary>
    public virtual void Kill() => EnemyId.InstaKill();
}
