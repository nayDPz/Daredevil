using RoR2;
using UnityEngine;

namespace Daredevil.Components
{
	public class MeleeTracker : MonoBehaviour
	{
		public float meleeRange = 8f;
		private CharacterBody characterBody;
		private InputBankTest inputBank;
		private void Start()
		{
			this.characterBody = GetComponent<CharacterBody>();
			this.inputBank = GetComponent<InputBankTest>();
		}

		public bool HasTarget()
		{
			Vector3 baseDirection = this.inputBank.aimDirection;
			Vector3 origin = this.inputBank.aimOrigin;
			Vector3 direction = baseDirection;

			if (TestRaycastHit(Physics.RaycastAll(origin, direction, meleeRange)))
				return true;

			direction = baseDirection + Vector3.up * 0.5f;
			if (TestRaycastHit(Physics.RaycastAll(origin, direction, meleeRange)))
				return true;

			direction = baseDirection + Vector3.down * 0.5f;
			if (TestRaycastHit(Physics.RaycastAll(origin, direction, meleeRange)))
				return true;

			direction = baseDirection + Vector3.left * 0.5f;
			if (TestRaycastHit(Physics.RaycastAll(origin, direction, meleeRange)))
				return true;

			direction = baseDirection + Vector3.right * 0.5f;
			if (TestRaycastHit(Physics.RaycastAll(origin, direction, meleeRange)))
				return true;

			direction = baseDirection + Vector3.forward * 0.5f;
			if (TestRaycastHit(Physics.RaycastAll(origin, direction, meleeRange)))
				return true;

			direction = baseDirection + Vector3.back * 0.5f;
			if (TestRaycastHit(Physics.RaycastAll(origin, direction, meleeRange)))
				return true;

			return false;
		}

		public bool TestRaycastHit(RaycastHit[] hits)
		{
			foreach (RaycastHit raycastHit in hits)
			{
				Collider collider = raycastHit.collider;
				if (collider)
				{
					HurtBox hurtBox = collider.GetComponent<HurtBox>();
					if (hurtBox)
					{
						HealthComponent healthComponent = hurtBox.healthComponent;
						if (healthComponent && healthComponent.gameObject != this.characterBody.gameObject && healthComponent.alive)
						{
							return true;
						}
					}
				}

			}
			return false;
		}
	}
}


