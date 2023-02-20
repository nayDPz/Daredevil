using Daredevil.Components;
using EntityStates;
using EntityStates.Huntress;
using RoR2;
using UnityEngine;

namespace Daredevil.States
{
	public class UnequipBigGun : AimThrowableBase
	{
		public static string soundString = "";
		public static float bloom = 0.3f;
		public static float recoilAmplitude = 4f;
		public float duration;
		public static float force = 600f;


		public ComboController comboController;
		public WeaponController weaponController;
		private EntityStateMachine weaponSwapMachine;


		public override void OnEnter()
		{
			// esc
			this.arcVisualizerPrefab = Assets.lineVisualiserPrefab;
			this.endpointVisualizerPrefab = ArrowRain.areaIndicatorPrefab;
			this.rayRadius = 0.5f;
			this.useGravity = true;
			this.endpointVisualizerRadiusScale = 0.5f;
			this.maxDistance = 40f;
			this.weaponController = GetComponent<WeaponController>();
			this.weaponSwapMachine = EntityStateMachine.FindByCustomName(base.gameObject, "WeaponSwap");
			bool flag = weaponController.currentWeapon;
			GameObject projectilePrefab = this.weaponController.currentWeapon.thrownWeaponProjectilePrefab;
			if (!flag || (flag && !projectilePrefab))
			{
				this.outer.SetNextStateToMain();
				return;
			}
			base.projectilePrefab = projectilePrefab;
			this.damageCoefficient = 0.25f;
			this.baseMinimumDuration = 0.1f;
			base.OnEnter();
			base.PlayAnimation("Gesture, Override", "PrepThrow");
		}

		public override void OnExit()
		{
			base.OnExit();
			base.PlayAnimation("Gesture, Override", "ThrowBigGun");
			if (this.weaponSwapMachine)
			{
				this.weaponSwapMachine.SetNextStateToMain();
			}
			if (this.weaponController)
			{
				this.weaponController.EquipWeapon(DaredevilContent.WeaponDefs.BaseGuns);
			}
			if (base.skillLocator)
			{
				base.skillLocator.special.UnsetSkillOverride(base.gameObject, Assets.unequipSkill, GenericSkill.SkillOverridePriority.Contextual);
			}
		}
	}
}


