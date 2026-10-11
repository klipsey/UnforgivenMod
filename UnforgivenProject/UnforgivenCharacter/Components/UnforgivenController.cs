using RoR2;
using UnforgivenMod.Unforgiven.Content;
using UnforgivenMod.Unforgiven.SkillStates;
using UnityEngine;
using UnityEngine.Networking;

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
        private const float gestureBlendDuration = 0.15f;
        private static readonly int isMovingParameter = Animator.StringToHash("isMoving");
        private static readonly int isSprintingParameter = Animator.StringToHash("isSprinting");
        private int gestureLayer = -1;
        private int runningGestureLayer = -1;
        private int sprintingGestureLayer = -1;
        private float runningGestureWeight;
        private float sprintingGestureWeight;
        public string currentSkinNameToken => this.skinController.skins[this.skinController.currentSkinIndex].nameToken;
        public string altSkinNameToken => UnforgivenSurvivor.UNFORGIVEN_PREFIX + "MASTERY_SKIN_NAME";

        public bool isUnsheathed => animator.GetBool("isUnsheathed");

        public bool pauseTimer = false;

        public bool bufferedSpin
        {
            get
            {
                if (!skillLocator || !skillLocator.secondary || !skillLocator.secondary.stateMachine)
                {
                    return false;
                }

                var state = skillLocator.secondary.stateMachine.state;
                return state is DashSpin spin && spin.HasPendingSpin || state is EnterStab stab && stab.HasBufferedSpin;
            }
        }

        public bool TryConfirmAirblade()
        {
            if (!bufferedSpin)
            {
                return false;
            }
            var state = skillLocator.secondary.stateMachine.state;
            if (state is DashSpin spin)
            {
                return spin.TryConfirmAirblade();
            }
            return state is EnterStab stab && stab.TryConfirmAirblade();
        }

        private const int maxShieldCharge = 100;
        private const float distanceForFullShield = 100f;
        private int shieldCharge;
        private float shieldChargeDistance;
        private Vector3 previousPosition;

        private void Awake()
        {
            this.characterBody = this.GetComponent<CharacterBody>();
            ModelLocator modelLocator = this.GetComponent<ModelLocator>();
            this.childLocator = modelLocator.modelTransform.GetComponentInChildren<ChildLocator>();
            this.animator = modelLocator.modelTransform.GetComponentInChildren<Animator>();
            this.characterModel = modelLocator.modelTransform.GetComponentInChildren<CharacterModel>();
            this.skillLocator = this.GetComponent<SkillLocator>();
            this.skinController = modelLocator.modelTransform.gameObject.GetComponent<ModelSkinController>();
            gestureLayer = animator.GetLayerIndex("Gesture, Override");
            runningGestureLayer = animator.GetLayerIndex("Gesture, Running");
            sprintingGestureLayer = animator.GetLayerIndex("Gesture, Sprinting");
        }

        private void OnEnable()
        {
            previousPosition = transform.position;
            UpdateGestureWeights(true);
            if (NetworkServer.active)
            {
                RefreshShieldBuffs();
            }
        }

        private void OnDisable()
        {
            runningGestureWeight = 0f;
            sprintingGestureWeight = 0f;
            ApplyGestureWeights();
            if (NetworkServer.active && characterBody)
            {
                characterBody.SetBuffCount(UnforgivenBuffs.shieldChargingBuff.buffIndex, 0);
                characterBody.SetBuffCount(UnforgivenBuffs.hasShieldBuff.buffIndex, 0);
            }
        }

        private void Update()
        {
            UpdateGestureWeights(false);
        }

        private void UpdateGestureWeights(bool immediate)
        {
            if (!animator || gestureLayer < 0 || runningGestureLayer < 0 || sprintingGestureLayer < 0)
            {
                return;
            }

            bool moving = animator.GetBool(isMovingParameter);
            bool sprinting = moving && animator.GetBool(isSprintingParameter);
            float runningTarget = moving && !sprinting ? 1f : 0f;
            float sprintingTarget = sprinting ? 1f : 0f;
            float step = Time.deltaTime / gestureBlendDuration;
            runningGestureWeight = immediate ? runningTarget : Mathf.MoveTowards(runningGestureWeight, runningTarget, step);
            sprintingGestureWeight = immediate ? sprintingTarget : Mathf.MoveTowards(sprintingGestureWeight, sprintingTarget, step);
            ApplyGestureWeights();
        }

        private void ApplyGestureWeights()
        {
            float remainingWeight = 1f - sprintingGestureWeight;
            float runningLayerWeight = remainingWeight > 0f ? Mathf.Clamp01(runningGestureWeight / remainingWeight) : 0f;
            animator.SetLayerWeight(gestureLayer, 1f);
            animator.SetLayerWeight(runningGestureLayer, runningLayerWeight);
            animator.SetLayerWeight(sprintingGestureLayer, sprintingGestureWeight);
        }

        private void RefreshShieldBuffs()
        {
            bool charged = shieldCharge >= maxShieldCharge;
            characterBody.SetBuffCount(UnforgivenBuffs.shieldChargingBuff.buffIndex, charged ? 0 : shieldCharge + 1);
            characterBody.SetBuffCount(UnforgivenBuffs.hasShieldBuff.buffIndex, charged ? 1 : 0);
        }

        public bool TryConsumeShield()
        {
            if (!NetworkServer.active || shieldCharge < maxShieldCharge)
            {
                return false;
            }

            shieldCharge = 0;
            shieldChargeDistance = 0f;
            previousPosition = transform.position;
            RefreshShieldBuffs();
            return true;
        }

        private void UpdateShieldCharge(float distanceTraveled)
        {
            if (shieldCharge >= maxShieldCharge)
            {
                return;
            }

            shieldChargeDistance = Mathf.Min(distanceForFullShield, shieldChargeDistance + distanceTraveled);
            int newCharge = Mathf.FloorToInt(shieldChargeDistance * maxShieldCharge / distanceForFullShield);
            if (shieldCharge != newCharge)
            {
                shieldCharge = newCharge;
                RefreshShieldBuffs();
            }
        }

        public void StackBehaviour(bool isNado = false)
        {
            if(NetworkServer.active)
            {
                if(!isNado) 
                {
                    int stacks = characterBody.GetBuffCount(UnforgivenBuffs.stabStackingBuff);
                    if (stacks == 1)
                    {
                        characterBody.AddTimedBuff(UnforgivenBuffs.stabMaxStacksBuff, 8f, 1);
                        characterBody.ClearTimedBuffs(UnforgivenBuffs.stabStackingBuff);
                        Util.PlaySound("sfx_unforgiven_max_stacks", base.gameObject);
                    }
                    else
                    {
                        characterBody.ClearTimedBuffs(UnforgivenBuffs.stabStackingBuff);
                        for (int i = 0; i < stacks + 1; i++)
                        {
                            characterBody.AddTimedBuff(UnforgivenBuffs.stabStackingBuff, 6f, 2);
                        }
                    }
                }
                else
                {
                    characterBody.ClearTimedBuffs(UnforgivenBuffs.stabStackingBuff);
                }
            }
        }
        private void FixedUpdate()
        {
            Vector3 position = transform.position;
            float distanceTraveled = (position - previousPosition).magnitude;
            previousPosition = position;
            if (NetworkServer.active && characterBody.healthComponent && characterBody.healthComponent.alive)
            {
                UpdateShieldCharge(distanceTraveled);
            }

            if(characterBody.HasBuff(UnforgivenBuffs.lastBreathBuff) && !childLocator.FindChild("EmpoweredSword").gameObject.activeSelf && 
                childLocator.FindChild("KatanaModel").gameObject.activeSelf)
            {
                childLocator.FindChild("EmpoweredSword").gameObject.SetActive(true);
                childLocator.FindChild("KatanaModel").gameObject.SetActive(false);
            }
            else if (!characterBody.HasBuff(UnforgivenBuffs.lastBreathBuff) && childLocator.FindChild("EmpoweredSword").gameObject.activeSelf &&
                !childLocator.FindChild("KatanaModel").gameObject.activeSelf)
            {
                childLocator.FindChild("EmpoweredSword").gameObject.SetActive(false);
                childLocator.FindChild("KatanaModel").gameObject.SetActive(true);
            }
        }

        public void Unsheath()
        {
            if(!this.animator.GetBool("isUnsheathed")) this.animator.SetBool("isUnsheathed", true);
        }
    }
}
