using EntityStates;
using HG;
using JetBrains.Annotations;
using RoR2;
using RoR2.Skills;
using UnityEngine;

[CreateAssetMenu(menuName = "Daredevil/SequenceSkillDef")]
public class SequenceSkillDef : SkillDef
{	
	public SerializableEntityStateType[] sequenceList;
	public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
	{
		return new InstanceData
		{
			sequenceCounter = 0
		};
	}
	protected SerializableEntityStateType GetNextStateType(GenericSkill skillSlot)
	{
		InstanceData instanceData = (SequenceSkillDef.InstanceData)skillSlot.skillInstanceData;
		SerializableEntityStateType state = ArrayUtils.GetSafe(sequenceList, instanceData.sequenceCounter);
		if (skillSlot.stateMachine.state.GetType() == state.stateType)
		{
			IncrementSequence(ref instanceData.sequenceCounter);
			state = ArrayUtils.GetSafe(sequenceList, instanceData.sequenceCounter);
		}
		return state;
	}

	public override EntityState InstantiateNextState(GenericSkill skillSlot)
	{
		return EntityStateCatalog.InstantiateState(GetNextStateType(skillSlot));
	}

	public override void OnExecute(GenericSkill skillSlot)
	{
		base.OnExecute(skillSlot);
		InstanceData instanceData = (InstanceData)skillSlot.skillInstanceData;
		IncrementSequence(ref instanceData.sequenceCounter);
	}
	private void IncrementSequence(ref int i)
	{
		i++;
		if (i >= sequenceList.Length)
		{
			i = 0;
		}
	}
	protected class InstanceData : BaseSkillInstanceData
	{
		public int sequenceCounter;
	}
}
