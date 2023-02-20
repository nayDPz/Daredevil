using System.Linq;
using RoR2;
using UnityEngine;

namespace Daredevil.Components
{
	public class WeaponController : MonoBehaviour
	{
		private ChildLocator childLocator;
		private Animator modelAnimator;

		public ModelLocator modelLocator;
		public SkillLocator skillLocator;

		public bool swordIsLeft;
		private float swordStopwatch;
		private float swordResetDuration = 2f;

		public string[] allWeaponTransforms = new string[] 
		{ 
			"Pistol", 
			"Revolver", 
			"Sword", 
			"Nailgun", 
			"Railgun", 
			"RocketLauncher", 
			"Shotgun", 
			"SpecialRevolver" 
		};
		public string[] allWeaponLayers = new string[] 
		{ 
			"Body, Sword",
			"Body, BigGun" 
		};

		private DaredevilWeaponDef.WeaponEquipInfo weaponEquipInfo;
		public DaredevilWeaponDef currentWeapon;
		public DaredevilWeaponDef[] specialWeaponDefs;
		private float weaponStopwatch;

		public bool InSpecialWeapon
		{
			get
			{
				if (this.specialWeaponDefs != null)
				{
					return this.specialWeaponDefs.Contains(this.currentWeapon);
				}
				return false;
			}
		}

		private void Awake()
		{
			if (this.modelLocator && this.modelLocator.modelTransform)
			{
				this.childLocator = this.modelLocator.modelTransform.gameObject.GetComponentInChildren<ChildLocator>();
				this.modelAnimator = this.modelLocator.modelTransform.gameObject.GetComponentInChildren<Animator>();
			}
			this.weaponEquipInfo = new DaredevilWeaponDef.WeaponEquipInfo
			{
				gameObject = base.gameObject,
				primary = this.skillLocator.primary,
				secondary = this.skillLocator.secondary,
				utility = this.skillLocator.utility,
				special = this.skillLocator.special
			};
			EquipWeapon(DaredevilContent.WeaponDefs.BaseGuns); /////////// ?
		}

		public DaredevilWeaponDef GetNextWeapon() //////// DO THIS PROPERLY LATER
		{
			int i = Mathf.FloorToInt(Random.Range(0, this.specialWeaponDefs.Length));
			return specialWeaponDefs[i];
		}

		public void EquipWeapon(DaredevilWeaponDef weaponDef)
		{
			float timeEquipped = this.weaponStopwatch;
			this.weaponStopwatch = 0f;

			if (this.currentWeapon)
				this.currentWeapon.UnequipWeapon(this.weaponEquipInfo, timeEquipped);

			weaponDef.EquipWeapon(weaponEquipInfo);
			this.currentWeapon = weaponDef;


			if (this.childLocator)
			{
				foreach (string weapon in this.allWeaponTransforms)
				{
					if (!weaponDef.transformsToEnable.Contains(weapon))
					{
						Transform transform = childLocator.FindChild(weapon);
						if (transform)
						{
							transform.gameObject.SetActive(false);
						}
					}
				}
				for (int i = 0; i < weaponDef.transformsToEnable.Length; i++)
				{
					Transform transform2 = childLocator.FindChild(weaponDef.transformsToEnable[i]);
					if (transform2)
					{
						transform2.gameObject.SetActive(true);
					}
				}
			}
			if (this.modelAnimator)
			{
				foreach (string layer in this.allWeaponLayers)
				{
					if (layer != weaponDef.animationLayer)
					{
						this.modelAnimator.SetLayerWeight(this.modelAnimator.GetLayerIndex(layer), 0f);
					}
				}
				this.modelAnimator.SetLayerWeight(this.modelAnimator.GetLayerIndex(weaponDef.animationLayer), 1f);
				this.modelAnimator.SetFloat("inBigGun", weaponDef.animationLayer.Equals("Body, BigGun") ? 1 : 0);
			}
			
		}

		private void OnDestroy()
		{
			if (this.currentWeapon)
			{
				this.currentWeapon.UnequipWeapon(this.weaponEquipInfo);
			}
		}

		public void SwingSword()
		{
			this.swordIsLeft = !this.swordIsLeft;
		}

		private void FixedUpdate()
		{
			if (this.currentWeapon)
				this.weaponStopwatch += Time.fixedDeltaTime;

			if (this.swordIsLeft)
				this.swordStopwatch += Time.fixedDeltaTime;
			else
				this.swordStopwatch = 0f;

			if (this.swordStopwatch >= this.swordResetDuration)
			{
				this.swordIsLeft = false;
				this.swordStopwatch = 0f;
			}
		}

		public void GunsOut()
		{
			EquipWeapon(DaredevilContent.WeaponDefs.BaseGuns);
		}

		public void SwordOut()
		{
			EquipWeapon(DaredevilContent.WeaponDefs.Sword);
		}

		public void SwordOut2Handed()
		{
			EquipWeapon(DaredevilContent.WeaponDefs.Sword2Handed);
		}
	}
}


