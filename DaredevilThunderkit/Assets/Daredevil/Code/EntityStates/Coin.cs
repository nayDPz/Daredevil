using Daredevil.Components;
using EntityStates;
using RoR2;
using UnityEngine;

namespace Daredevil.States
{
	public class Coin : GenericBulletBaseState
	{
		public static string abilityKey = "SecondaryCoin";
		public static float coinBounceHeight = 10f;
		public static float coinHorizontalSpeed = 5f;
		public static GameObject coinPrefab = Assets.coinProjectile;
		public static float ricochetMultiplier = 2f;

		public override void OnEnter()
		{
			this.baseDuration = 0.6f;
			base.OnEnter();
		}

		public override void PlayFireAnimation()
		{
			base.PlayAnimation("LeftArm, Additive", "ShootPistolIdle"); /////// ANIMMMMM
			Util.PlaySound(Sounds.coinShoot, base.gameObject);
		}

		public override void ModifyBullet(BulletAttack bulletAttack)
		{
			CoinProjectileController.CoinMethods.CommonCoinBulletAttack(bulletAttack, bulletAttack.owner, bulletAttack.origin, bulletAttack.aimVector, Coin.ricochetMultiplier);
		}
	}
}


