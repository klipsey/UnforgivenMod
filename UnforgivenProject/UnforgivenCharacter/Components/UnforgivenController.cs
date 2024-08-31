using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.HudOverlay;
using System;
using UnforgivenMod.Unforgiven.Content;
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

        public bool bufferedSpin;

        public static float maxShieldGain = 100f;

        public float shieldAmount;

        private float shieldStopwatchInterval;

        private float bufferStopwatch;

        private Vector3 previousPosition = Vector3.zero;

        public Action onShieldChange;
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

                        NetworkIdentity identity = base.gameObject.GetComponent<NetworkIdentity>();
                        if (!identity) return;

                        new SyncIcon(identity.netId, true).Send(NetworkDestination.Clients);
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
            shieldStopwatchInterval += Time.deltaTime;

            if(bufferedSpin)
            {
                bufferStopwatch += Time.deltaTime;
                if(bufferStopwatch >= 4f / characterBody.attackSpeed) bufferedSpin = false;
            }

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
                onShieldChange?.Invoke();
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

            if (this.characterBody.skillLocator.secondary.skillDef.icon != UnforgivenAssets.secondaryIcon)
            {
                if (!this.characterBody.HasBuff(UnforgivenBuffs.stabMaxStacksBuff))
                {
                    NetworkIdentity identity = base.gameObject.GetComponent<NetworkIdentity>();
                    if (!identity) return;

                    new SyncIcon(identity.netId, false).Send(NetworkDestination.Clients);
                }
            }
        }

        public void Unsheath()
        {
            if(!this.animator.GetBool("isUnsheathed")) this.animator.SetBool("isUnsheathed", true);
        }
        private void OnDestroy()
        {
        }
    }
}
