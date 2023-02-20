using Daredevil.Components;
using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace Daredevil.States
{
	public class EnterBike : BaseSkillState
	{
		private ComboController comboCounter;
		public static GameObject bikeObject = Assets.mainAssetBundle.LoadAsset<GameObject>("DevilBike");
		public static float duration;
		private float stopwatch;
		private Animator animator;

		public override void OnEnter()
		{
			base.OnEnter();
			animator = GetModelAnimator();
			Ray aimRay = GetAimRay();
			GameObject bike = GameObject.Instantiate(bikeObject, aimRay.origin, Quaternion.LookRotation(aimRay.direction));
			bike.GetComponent<VehicleSeat>().AssignPassenger(base.gameObject);
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			this.stopwatch += Time.fixedDeltaTime;
			if (base.isAuthority && this.stopwatch >= duration)
			{
				this.outer.SetNextStateToMain();
			}
		}

		public override void OnExit()
		{
			base.OnExit();
		}

		public override void OnSerialize(NetworkWriter writer)
		{
			base.OnSerialize(writer);
		}

		public override void OnDeserialize(NetworkReader reader)
		{
			base.OnDeserialize(reader);
		}
	}
}


