using EntityStates;
using RoR2;
using UnityEngine;

namespace Daredevil.States
{
	public class ShotgunFire : GenericBulletBaseState
	{
		public static GameObject tracerEffect = Assets.captainTracer;


		/// ESCYGESUCGEJYWEKHAGDAW
		public override void OnEnter()
		{
			minSpread = 1f;
			minSpread = 8f;
			baseDuration = 1.5f;
			bulletCount = 6;
			damageCoefficient = 1.25f;
			procCoefficient = 1f;
			recoilAmplitudeX = 3f;
			recoilAmplitudeY = 6f;
			spreadBloomValue = -0.3f;
			bulletRadius = 1f;
			useSmartCollision = true;
			force = 1000f;
			tracerEffectPrefab = tracerEffect;
			maxDistance = 90f;

			base.OnEnter();

		}

		public override void DoFireEffects()
		{
			base.DoFireEffects();
			Util.PlaySound(Sounds.shotgunShoot, base.gameObject);
		}

		public override void PlayFireAnimation()
		{
			base.PlayAnimation("Gesture, Override", "FireBig");
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Any;
		}
	}
}


