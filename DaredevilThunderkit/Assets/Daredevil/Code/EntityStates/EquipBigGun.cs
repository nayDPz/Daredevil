using Daredevil.Components;
using EntityStates;
using UnityEngine.Networking;

namespace Daredevil.States
{
	public class EquipBigGun : BaseSkillState
	{
		public DaredevilWeaponDef weaponDef;
		public ComboController comboController;
		public WeaponController weaponController;
		public float duration = 1f;

		public override void OnEnter()
		{
			base.OnEnter();

			this.weaponController = GetComponent<WeaponController>();
			this.weaponDef = weaponController.GetNextWeapon();
			this.weaponController.EquipWeapon(this.weaponDef);


			// SHOULD PROBABLY CONSUME COMBO FROM SKILL DEF??
			this.comboController = GetComponent<ComboController>();
			this.comboController.ConsumeCombo();

			base.PlayAnimation("Gesture, Override", "EquipBigGun");
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.fixedAge >= duration)
			{
				this.outer.SetNextState(new BigGunState
				{
					weaponDef = this.weaponDef
				});
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


