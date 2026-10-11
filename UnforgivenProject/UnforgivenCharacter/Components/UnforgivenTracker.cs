using RoR2;
using UnforgivenMod.Unforgiven.Content;
using UnityEngine;


namespace UnforgivenMod.Unforgiven.Components
{
    public class UnforgivenTracker : MonoBehaviour
    {
        public float maxTrackingDistance = 30f;

        public float maxTrackingAngle = 40f;

        public float trackerUpdateFrequency = 10f;

        private HurtBox trackingTarget;

        private CharacterBody characterBody;

        private TeamComponent teamComponent;

        private InputBankTest inputBank;

        private float trackerUpdateStopwatch;

        private Indicator indicator;
        private GameObject indicatorPrefab;

        private readonly BullseyeSearch search = new BullseyeSearch();

        private void Awake()
        {
            UpdateIndicator();
        }

        private void UpdateIndicator()
        {
            GameObject prefab = UnforgivenSkinVFX.GetEffect(UnforgivenAssets.unforgivenIndicator, gameObject);
            if (prefab == indicatorPrefab)
            {
                return;
            }
            if (indicator != null)
            {
                indicator.active = false;
            }
            indicatorPrefab = prefab;
            indicator = new Indicator(gameObject, prefab);
            indicator.active = isActiveAndEnabled;
        }

        private void Start()
        {
            characterBody = GetComponent<CharacterBody>();
            inputBank = GetComponent<InputBankTest>();
            teamComponent = GetComponent<TeamComponent>();
        }

        public HurtBox GetTrackingTarget()
        {
            return trackingTarget;
        }

        private void OnEnable()
        {
            indicator.active = true;
        }

        private void OnDisable()
        {
            indicator.active = false;
        }

        private void FixedUpdate()
        {
            UpdateIndicator();
            trackerUpdateStopwatch += Time.fixedDeltaTime;
            if (trackerUpdateStopwatch >= 1f / trackerUpdateFrequency)
            {
                trackerUpdateStopwatch -= 1f / trackerUpdateFrequency;
                Ray aimRay = new Ray(inputBank.aimOrigin, inputBank.aimDirection);
                SearchForTarget(aimRay);
                indicator.targetTransform = (trackingTarget ? trackingTarget.transform : null);
            }
        }

        private void SearchForTarget(Ray aimRay)
        {
            search.teamMaskFilter = TeamMask.GetUnprotectedTeams(teamComponent.teamIndex);
            search.filterByLoS = true;
            search.searchOrigin = aimRay.origin;
            search.searchDirection = aimRay.direction;
            search.sortMode = BullseyeSearch.SortMode.Distance;
            search.maxDistanceFilter = maxTrackingDistance;
            search.maxAngleFilter = maxTrackingAngle;
            search.RefreshCandidates();
            search.FilterOutGameObject(base.gameObject);
            trackingTarget = null;
            foreach (HurtBox hurt in search.GetResults())
            {
                if (hurt && hurt.healthComponent && hurt.healthComponent.body &&
                    !hurt.healthComponent.body.HasBuff(UnforgivenBuffs.dashCooldownBuff))
                {
                    trackingTarget = hurt;
                    break;
                }
            }
        }
    }
}
