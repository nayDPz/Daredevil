using System.Collections.Generic;
using Daredevil.Components;
using RoR2;

namespace Daredevil
{
	public static class RicochetUtils
	{
		public enum RicochetPriority
		{
			None,
			StunnedBody,
			Body,
			Projectile,
			Explosive,
			Rocket,
			Bike,
			Coin
		}
		public static void BulletAttackShootableDamageCallback(BulletAttack bullet, ref BulletAttack.BulletHit hitInfo, DamageInfo damageInfo)
		{
			if (!hitInfo.hitHurtBox || !(hitInfo.hitHurtBox.healthComponent.gameObject != bullet.owner))
			{
				return;
			}

			List<IShootable> shootables = new List<IShootable>();
			hitInfo.hitHurtBox.healthComponent.GetComponents(shootables);

			if (shootables.Count > 0)
			{
				damageInfo.procCoefficient = 0f;
			}
			foreach (IShootable shootable in shootables)
			{
				if (shootable.CanBeShot())
				{
					shootable.OnShot(damageInfo);
				}
			}
		}
	}
}


