using Daredevil.Components;
using JetBrains.Annotations;
using RoR2;
using RoR2.Skills;
using UnityEngine;

[CreateAssetMenu(menuName = "Daredevil/ComboSkillDef")]
public class ComboSkillDef : SkillDef
{
	

	public int comboToActivate;

	public override bool IsReady([NotNull] GenericSkill skillSlot)
	{
		ComboController comboController = ((ComboSkillDef.InstanceData)skillSlot.skillInstanceData).comboController;
		return base.IsReady(skillSlot) && comboController && comboController.comboCount >= comboToActivate;
	}

	public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
	{
		return new InstanceData
		{
			comboController = skillSlot.GetComponent<ComboController>()
		};
	}

	protected class InstanceData : BaseSkillInstanceData
	{
		public ComboController comboController;
	}
}
