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
		public int maxTargets = 4;

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
		private List<HurtBox> targetList = new List<HurtBox>();

		private List<HurtBox> candidateList = new List<HurtBox>();

		private SphereSearch stunSearch = new SphereSearch();
		private readonly BullseyeSearch targetSearch = new BullseyeSearch();

		public GameObject indicatorPrefab;
		private Dictionary<HealthComponent, StunIndicator> indicators;
		
		

		private void Awake()
        {
			this.indicators = new Dictionary<HealthComponent, StunIndicator>();
		}
		public List<HurtBox> GetTargetList()
		{
			return this.targetList;
		}
		private void AddCandidate(HurtBox hurtBox)
        {
			StunIndicator indicator;
			if(!this.indicators.TryGetValue(hurtBox.healthComponent, out indicator))
            {
				indicator = GameObject.Instantiate(this.indicatorPrefab).GetComponent<StunIndicator>();
				indicator.owner = base.gameObject;
				indicator.targetBody = hurtBox.healthComponent.body;

				this.indicators.Add(hurtBox.healthComponent, indicator);
            }
			indicator.isTarget = this.targetList.Contains(hurtBox);
        }
		private void CleanCandidates()
        {
			for (int i = this.candidateList.Count - 1; i >= 0; i--)
			{
				HurtBox hurtBox = this.candidateList[i];
				if (!hurtBox.healthComponent || !hurtBox.healthComponent.alive || !IsBodyStunned(hurtBox.healthComponent.body))
				{
					HurtBox gone = this.candidateList[i];
					this.candidateList.RemoveAt(i);
					if(this.indicators.TryGetValue(gone.healthComponent, out StunIndicator indicator))
                    {
						GameObject.Destroy(indicator.gameObject);
						this.indicators.Remove(gone.healthComponent);
                    }						
				}
			}
		}
		private void FixedUpdate()
		{
			if (!Util.HasEffectiveAuthority(this.characterBody.gameObject))
				return;

			this.trackerUpdateStopwatch += Time.fixedDeltaTime;
			if (this.trackerUpdateStopwatch >= 1f / this.trackerUpdateFrequency)
			{
				this.trackerUpdateStopwatch -= 1f / this.trackerUpdateFrequency;
				CleanCandidates();

				
				
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

		private bool IsBodyStunned(CharacterBody body)
        {
			return body.HasBuff(DaredevilContent.Buffs.stunMarked) && !body.HasBuff(DaredevilContent.Buffs.stunMarkedCooldown);
		}

		private void SearchForTarget(Ray aimRay)
		{
			TeamMask mask = TeamMask.GetUnprotectedTeams(this.teamComponent.teamIndex);
			this.targetList = new List<HurtBox>(); // im breandead
			
			this.targetSearch.teamMaskFilter = mask;
			this.targetSearch.filterByLoS = false;
			this.targetSearch.searchOrigin = aimRay.origin;
			this.targetSearch.searchDirection = aimRay.direction;
			this.targetSearch.sortMode = BullseyeSearch.SortMode.Distance;
			this.targetSearch.maxDistanceFilter = this.maxTrackingDistance;
			this.targetSearch.maxAngleFilter = this.maxTrackingAngle;
			this.targetSearch.filterByDistinctEntity = true;
			this.targetSearch.RefreshCandidates();
			this.targetSearch.FilterOutGameObject(base.gameObject);

			foreach (HurtBox hurtBox in this.targetSearch.GetResults())
			{
				CharacterBody body = hurtBox.healthComponent.body;
				if (body && !IsBodyStunned(body))
				{
					this.targetSearch.FilterOutGameObject(body.healthComponent.gameObject);
				}
			}

			HurtBox[] targets = this.targetSearch.GetResults().ToArray();
			int count = Mathf.Min(targets.Length, this.maxTargets);
			for (int i = 0; i < count; i++)
            {
				this.targetList.Add(targets[i].hurtBoxGroup.mainHurtBox);
			}
			 


			this.stunSearch.origin = aimRay.origin;
			this.stunSearch.radius = this.maxTrackingDistance;
			this.stunSearch.mask = LayerIndex.entityPrecise.mask;
			this.stunSearch.queryTriggerInteraction = QueryTriggerInteraction.Ignore;
			this.candidateList = this.stunSearch.RefreshCandidates().FilterCandidatesByHurtBoxTeam(mask)
				.FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes().ToList();
			foreach (HurtBox hurtBox in this.candidateList)
			{
				CharacterBody body = hurtBox.healthComponent.body;
				if (body && IsBodyStunned(body))
				{
					
					this.AddCandidate(hurtBox.hurtBoxGroup.mainHurtBox);
				}
			}
		}
	}
}


