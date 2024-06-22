using RoR2;
using RoR2.HudOverlay;
using System;
using UnityEngine;

namespace UnforgivenMod.Unforgiven.Components
{
    public class UnforgivenController : MonoBehaviour
    {
        private CharacterBody characterBody;
        private ModelSkinController skinController;
        private ChildLocator childLocator;
        private CharacterModel characterModel;
        private Animator animator;
        private SkillLocator skillLocator;
        public string currentSkinNameToken => this.skinController.skins[this.skinController.currentSkinIndex].nameToken;
        public string altSkinNameToken => UnforgivenSurvivor.UNFORGIVEN_PREFIX + "MASTERY_SKIN_NAME";

        public bool pauseTimer = false;

        public static float maxShieldGain = 100f;

        public float shieldAmount;

        private float shieldStopwatchInterval;
        private Vector3 previousPosition;

        public Action onShieldChange;
        private void Awake()
        {
            this.characterBody = this.GetComponent<CharacterBody>();
            ModelLocator modelLocator = this.GetComponent<ModelLocator>();
            this.childLocator = modelLocator.modelBaseTransform.GetComponentInChildren<ChildLocator>();
            this.animator = modelLocator.modelBaseTransform.GetComponentInChildren<Animator>();
            this.characterModel = modelLocator.modelBaseTransform.GetComponentInChildren<CharacterModel>();
            this.skillLocator = this.GetComponent<SkillLocator>();
            this.skinController = modelLocator.modelTransform.gameObject.GetComponent<ModelSkinController>();
        }
        private void Start()
        {
            previousPosition = base.transform.position;
        }
        #region tooMuchCrap

        #endregion
        private void FixedUpdate()
        {
            shieldStopwatchInterval += Time.fixedDeltaTime;
            if(shieldStopwatchInterval >= 1f)
            {
                shieldStopwatchInterval = 0f;
                if (shieldAmount < 100f) shieldAmount += (base.transform.position - previousPosition).magnitude;
                else shieldAmount = 100f;
                previousPosition = base.transform.position;
                onShieldChange.Invoke();
            }
        }

        private void OnDestroy()
        {
        }
    }
}
