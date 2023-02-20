using Daredevil.Components;
using EntityStates;
using JetBrains.Annotations;
using RoR2;
using RoR2.Skills;
using UnityEngine;

[CreateAssetMenu(menuName = "Daredevil/DaredevilConditionalSkillDef")]
public class DaredevilConditionalSkillDef : SkillDef
{
	public bool hasMeleeCondition;

	public SerializableEntityStateType meleeState;

	public SerializableEntityStateType baseState;

	public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
	{
		return new InstanceData
		{
			meleeTracker = skillSlot.GetComponent<MeleeTracker>()
		};
	}

	public override EntityState InstantiateNextState([NotNull] GenericSkill skillSlot)
	{
		if (ShouldUseMeleeSkill(skillSlot))
		{
			return EntityStateCatalog.InstantiateState(meleeState);
		}
		return EntityStateCatalog.InstantiateState(baseState);
	}

	private bool ShouldUseMeleeSkill(GenericSkill skillSlot)
	{
		MeleeTracker meleeTracker = ((DaredevilConditionalSkillDef.InstanceData)skillSlot.skillInstanceData).meleeTracker;
		if (hasMeleeCondition)
		{
			return meleeTracker.HasTarget();
		}
		return false;
	}

	protected class InstanceData : BaseSkillInstanceData
	{
		public MeleeTracker meleeTracker;

		public StunTracker lockOnTracker;
	}
}
