using Daredevil;
using RoR2;
using RoR2.Skills;
using UnityEngine;

[CreateAssetMenu(menuName = "Daredevil/DaredevilWeaponDef")]
public class DaredevilWeaponDef : ScriptableObject
{
	
	public SkillDef primarySkill;
	public SkillDef secondarySkill;
	public SkillDef utilitySkill;
	public Texture2D iconTexture;

	public string animationLayer = "Body, BigGun";
	public string equipSoundString;
	public string unequipSoundString;
	public string[] transformsToEnable;

	public GameObject thrownWeaponProjectilePrefab;

	public bool EquipSkillOverride
	{
		get => primarySkill || secondarySkill || utilitySkill;
	}

	public struct WeaponEquipInfo
	{
		public GameObject gameObject;
		public Transform modelTransform;
		public GenericSkill primary;
		public GenericSkill secondary;
		public GenericSkill utility;
		public GenericSkill special;
	}

	public void EquipWeapon(WeaponEquipInfo weaponEquipInfo)
	{
		Util.PlaySound(equipSoundString, weaponEquipInfo.gameObject);
		
		if (this.primarySkill && weaponEquipInfo.primary)
		{
			weaponEquipInfo.primary.SetSkillOverride(weaponEquipInfo.gameObject, this.primarySkill, GenericSkill.SkillOverridePriority.Upgrade);
		}
		if (this.secondarySkill && weaponEquipInfo.secondary)
		{
			weaponEquipInfo.secondary.SetSkillOverride(weaponEquipInfo.gameObject, this.secondarySkill, GenericSkill.SkillOverridePriority.Upgrade);
		}
		if (this.utilitySkill && weaponEquipInfo.utility)
		{
			weaponEquipInfo.utility.SetSkillOverride(weaponEquipInfo.gameObject, this.utilitySkill, GenericSkill.SkillOverridePriority.Upgrade);
		}
		if (weaponEquipInfo.special && EquipSkillOverride)
		{
			weaponEquipInfo.special.SetSkillOverride(weaponEquipInfo.gameObject, Assets.unequipSkill, GenericSkill.SkillOverridePriority.Upgrade);
		}
	}

	public void UnequipWeapon(WeaponEquipInfo weaponEquipInfo)
	{
		UnequipWeapon(weaponEquipInfo, 0f);
	}

	public void UnequipWeapon(WeaponEquipInfo weaponEquipInfo, float timeEquipped)
	{
		Util.PlaySound(unequipSoundString, weaponEquipInfo.gameObject);
		
		if (primarySkill && weaponEquipInfo.primary)
		{
			weaponEquipInfo.primary.UnsetSkillOverride(weaponEquipInfo.gameObject, this.primarySkill, GenericSkill.SkillOverridePriority.Upgrade);
			weaponEquipInfo.primary.RunRecharge(timeEquipped);
			
		}
		if (secondarySkill && weaponEquipInfo.secondary)
		{
			weaponEquipInfo.secondary.UnsetSkillOverride(weaponEquipInfo.gameObject, this.secondarySkill, GenericSkill.SkillOverridePriority.Upgrade);
			weaponEquipInfo.secondary.RunRecharge(timeEquipped);	
		}
		if (utilitySkill && weaponEquipInfo.utility)
		{
			weaponEquipInfo.utility.UnsetSkillOverride(weaponEquipInfo.gameObject, this.utilitySkill, GenericSkill.SkillOverridePriority.Upgrade);
			weaponEquipInfo.utility.RunRecharge(timeEquipped);
		}
		if (weaponEquipInfo.special && EquipSkillOverride)
		{
			weaponEquipInfo.special.UnsetSkillOverride(weaponEquipInfo.gameObject, Assets.unequipSkill, GenericSkill.SkillOverridePriority.Upgrade);
		}
	}
}
