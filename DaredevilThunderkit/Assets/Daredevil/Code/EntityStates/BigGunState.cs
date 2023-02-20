using EntityStates;
using RoR2;
using UnityEngine.Networking;

namespace Daredevil.States
{
	public class BigGunState : BaseSkillState
	{
		public DaredevilWeaponDef weaponDef;
		public float duration = 10f;
		private EntityStateMachine weapon;
		private EntityStateMachine weaponExtra;

		public override void OnEnter()
		{
			base.OnEnter();
			this.weapon = EntityStateMachine.FindByCustomName(base.gameObject, "Weapon");
			if (base.skillLocator)
			{
				base.skillLocator.special.SetSkillOverride(base.gameObject, Assets.unequipSkill, GenericSkill.SkillOverridePriority.Contextual);
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.fixedAge >= this.duration)
			{
				if (this.weapon)
				{
					this.weapon.SetNextState(new UnequipBigGun());
				}
				this.outer.SetNextStateToMain();
			}
		}

		public override void OnSerialize(NetworkWriter writer)
		{
		}

		public override void OnDeserialize(NetworkReader reader)
		{
		}
	}
}


