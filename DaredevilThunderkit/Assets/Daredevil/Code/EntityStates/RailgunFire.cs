using System;
using EntityStates;
using RoR2;

namespace Daredevil.States
{
	public class RailgunFire : GenericBulletBaseState
	{
		public override void OnEnter()
		{
			//////////////////////////////////////////
			baseDuration = 1.5f;
			damageCoefficient = 35f;
			bulletRadius = 3f;
			force = 4000f;
			tracerEffectPrefab = Assets.railgunTracer;
			maxDistance = 1000f;
			muzzleName = "MuzzleBigGun";
			muzzleFlashPrefab = Assets.muzzleFlashRailgun;

			muzzleFlashPrefab.GetComponent<EffectComponent>().parentToReferencedTransform = true; // fuvking stupid
			base.OnEnter();
		}

		public override void DoFireEffects()
		{
			base.DoFireEffects();
			Util.PlaySound(Sounds.railgunShoot, base.gameObject);
			Util.PlaySound(Sounds.railgunEquipStop, base.gameObject);
		}

		public override void ModifyBullet(BulletAttack bulletAttack)
		{
			bulletAttack.stopperMask = 0;
			bulletAttack.tracerEffectPrefab = Assets.railgunTracer;
			bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
			bulletAttack.modifyOutgoingDamageCallback += RicochetUtils.BulletAttackShootableDamageCallback;
		}

		public override void PlayFireAnimation()
		{
			base.PlayAnimation("Gesture, Override", "FireBigBig");
		}
	}
}


