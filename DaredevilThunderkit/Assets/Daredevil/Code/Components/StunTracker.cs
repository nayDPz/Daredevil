using System.Collections.Generic;
using System.Linq;
using RoR2;
using UnityEngine;

namespace Daredevil.Components
{
	public class StunTracker : MonoBehaviour
	{
		public float maxTrackingDistance = 50f;
		public float maxTrackingAngle = 70f;
		public float trackerUpdateFrequency = 10f;

		private List<HurtBox> targetList = new List<HurtBox>();
		public HurtBox trackingTarget;

		public CharacterBody characterBody;
		public TeamComponent teamComponent;
		public InputBankTest inputBank;
		public SkillLocator skillLocator;
		public WeaponController weaponController;

		public GenericSkill stunSkill;
		public SkillSlot skillSlotToOverride;

		private SkillSlot overriddenSkillSlot;
		private GenericSkill overriddenSkill;
		private bool overridden;

		private float trackerUpdateStopwatch;
		private readonly BullseyeSearch search = new BullseyeSearch();

		

		public List<HurtBox> GetTargetList()
		{
			return this.targetList;
		}

		private void FixedUpdate()
		{
			this.trackerUpdateStopwatch += Time.fixedDeltaTime;
			if (this.trackerUpdateStopwatch >= 1f / this.trackerUpdateFrequency)
			{
				this.trackerUpdateStopwatch -= 1f / this.trackerUpdateFrequency;
				
				Ray ray = this.inputBank.GetAimRay();
				SearchForTarget(ray);
				if (this.targetList.Count > 0 && this.weaponController && !this.weaponController.InSpecialWeapon)
				{
					SetSkillOverride(this.skillSlotToOverride);
				}
				else
				{
					UnsetSkillOverride(this.overriddenSkillSlot);
				}
			}
		}

		private void SetSkillOverride(SkillSlot skillSlotToOverride)
		{
			if (this.overridden)
			{
				if (skillSlotToOverride == this.overriddenSkillSlot)
					return;
				UnsetSkillOverride(this.overriddenSkillSlot);
			}

			this.overridden = true;
			this.overriddenSkillSlot = skillSlotToOverride;
			switch (skillSlotToOverride)
			{
				case SkillSlot.Primary:
					this.overriddenSkill = this.skillLocator.primary;
					this.skillLocator.primary = this.stunSkill;
					break;
				case SkillSlot.Secondary:
					this.overriddenSkill = this.skillLocator.secondary;
					this.skillLocator.secondary = this.stunSkill;
					break;
				case SkillSlot.Utility:
					this.overriddenSkill = this.skillLocator.utility;
					this.skillLocator.utility = this.stunSkill;
					break;
				case SkillSlot.Special:
					this.overriddenSkill = this.skillLocator.special;
					this.skillLocator.special = this.stunSkill;
					break;
			}
		}

		private void UnsetSkillOverride(SkillSlot skillSlotToOverride)
		{
			if (this.overridden)
			{
				this.overridden = false;
				SkillSlot skillSlot = skillSlotToOverride;
				this.overriddenSkillSlot = SkillSlot.None;
				switch (skillSlot)
				{
					case SkillSlot.Primary:
						this.skillLocator.primary = this.overriddenSkill;
						break;
					case SkillSlot.Secondary:
						this.skillLocator.secondary = this.overriddenSkill;
						break;
					case SkillSlot.Utility:
						this.skillLocator.utility = this.overriddenSkill;
						break;
					case SkillSlot.Special:
						this.skillLocator.special = this.overriddenSkill;
						break;
				}
			}
		}

		private void SearchForTarget(Ray aimRay)
		{
			this.search.teamMaskFilter = TeamMask.GetUnprotectedTeams(this.teamComponent.teamIndex);
			this.search.filterByLoS = false;
			this.search.searchOrigin = aimRay.origin;
			this.search.searchDirection = aimRay.direction;
			this.search.sortMode = BullseyeSearch.SortMode.None;
			this.search.maxDistanceFilter = this.maxTrackingDistance;
			this.search.maxAngleFilter = this.maxTrackingAngle;
			this.search.filterByDistinctEntity = true;
			this.search.RefreshCandidates();
			this.search.FilterOutGameObject(base.gameObject);

			foreach (HurtBox hurtBox in this.search.GetResults().ToList())
			{
				CharacterBody body = hurtBox.healthComponent.body;
				if (body && (!body.HasBuff(DaredevilContent.Buffs.stunMarked) || body.HasBuff(DaredevilContent.Buffs.stunMarkedCooldown)))
				{
					this.search.FilterOutGameObject(body.healthComponent.gameObject);
				}
			}
			this.targetList = this.search.GetResults().ToList();
		}
	}
}


