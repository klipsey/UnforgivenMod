using RoR2;
using RoR2.HudOverlay;
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
                return state is DashSpin || state is EnterStab stab && stab.HasBufferedSpin;
            }
        }

        public static float maxShieldGain = 100f;

        public float shieldAmount;

        private float shieldStopwatchInterval;

        private Vector3 previousPosition = Vector3.zero;

        private OverlayController shieldOverlay;

        private void Awake()
        {
            this.characterBody = this.GetComponent<CharacterBody>();
            ModelLocator modelLocator = this.GetComponent<ModelLocator>();
            this.childLocator = modelLocator.modelTransform.GetComponentInChildren<ChildLocator>();
            this.animator = modelLocator.modelTransform.GetComponentInChildren<Animator>();
            this.characterModel = modelLocator.modelTransform.GetComponentInChildren<CharacterModel>();
            this.skillLocator = this.GetComponent<SkillLocator>();
            this.skinController = modelLocator.modelTransform.gameObject.GetComponent<ModelSkinController>();
        }

        private void OnEnable()
        {
            if (shieldOverlay != null)
            {
                return;
            }
            if (!UnforgivenAssets.shieldHudPrefab)
            {
                Log.Error("Cannot create the shield HUD because its prefab is missing.");
                return;
            }

            shieldOverlay = HudOverlayManager.AddOverlay(gameObject, new OverlayCreationParams
            {
                prefab = UnforgivenAssets.shieldHudPrefab,
                childLocatorEntry = "CrosshairExtras"
            });
            shieldOverlay.onInstanceAdded += OnShieldOverlayAdded;
            UpdateShieldOverlay();
        }

        private void OnDisable()
        {
            RemoveShieldOverlay();
        }

        private void OnDestroy()
        {
            RemoveShieldOverlay();
        }

        private void Update()
        {
            UpdateShieldOverlay();
        }

        private void UpdateShieldOverlay()
        {
            if (shieldOverlay == null)
            {
                return;
            }

            CharacterMaster master = characterBody ? characterBody.master : null;
            shieldOverlay.active = characterBody && characterBody.isActiveAndEnabled &&
                characterBody.healthComponent && characterBody.healthComponent.alive &&
                master && master.hasAuthority;
        }

        private void OnShieldOverlayAdded(OverlayController overlay, GameObject instance)
        {
            if (instance.TryGetComponent<PassiveShieldHudController>(out var shieldHud))
            {
                if (!shieldHud.SetSource(this))
                {
                    RemoveShieldOverlay();
                }
            }
            else
            {
                Log.Error("Shield HUD instance is missing its display controller.");
                RemoveShieldOverlay();
            }
        }

        private void RemoveShieldOverlay()
        {
            if (shieldOverlay == null)
            {
                return;
            }

            shieldOverlay.active = false;
            shieldOverlay.onInstanceAdded -= OnShieldOverlayAdded;
            HudOverlayManager.RemoveOverlay(shieldOverlay);
            shieldOverlay = null;
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
            shieldStopwatchInterval += Time.fixedDeltaTime;

            if(shieldStopwatchInterval >= 0.25f && base.transform)
            {
                shieldStopwatchInterval = 0f;
                if (shieldAmount < 100f) shieldAmount += (base.transform.position - previousPosition).magnitude / 2f;
                else
                {
                    if(!characterBody.HasBuff(UnforgivenBuffs.hasShieldBuff))
                    {
                        if(NetworkServer.active) characterBody.SetBuffCount(UnforgivenBuffs.hasShieldBuff.buffIndex, 1);
                    }
                    shieldAmount = 100f;
                }
                previousPosition = base.transform.position;
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
