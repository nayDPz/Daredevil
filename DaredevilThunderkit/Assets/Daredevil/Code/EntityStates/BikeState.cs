using Daredevil.Components;
using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace Daredevil.States
{
	public class BikeState : BaseSkillState
	{
		private ComboController comboCounter;
		public static float duration;
		private float stopwatch;
		private Animator animator;

		public override void OnEnter()
		{
			base.OnEnter();
			this.animator = GetModelAnimator();
			this.GetModelTransform().GetComponent<HurtBoxGroup>().mainHurtBox.gameObject.layer = LayerIndex.noCollision.intVal;
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			this.stopwatch += Time.fixedDeltaTime;
		}

		public override void OnExit()
		{
			this.GetModelTransform().GetComponent<HurtBoxGroup>().mainHurtBox.gameObject.layer = LayerIndex.entityPrecise.intVal;
			base.OnExit();
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Vehicle;
		}
	}

}

