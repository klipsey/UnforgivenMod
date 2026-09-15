using System.Collections.Generic;
using RoR2;
using UnityEngine;

namespace UnforgivenMod.Unforgiven.Components
{
    public class AirborneTargetLineController : MonoBehaviour
    {
        private CharacterBody body;
        private SkillLocator skillLocator;
        private AirborneTargetLines lines;
        private readonly List<CharacterBody> targets = new List<CharacterBody>();

        private void Awake()
        {
            body = GetComponent<CharacterBody>();
            skillLocator = GetComponent<SkillLocator>();
            if (!body || !skillLocator)
            {
                Log.Error("Airborne target lines require a CharacterBody and SkillLocator.");
                enabled = false;
            }
        }

        private void LateUpdate()
        {
            if (!body || !body.isActiveAndEnabled || !body.healthComponent || !body.healthComponent.alive)
            {
                ReleaseLines();
                return;
            }

            Camera camera = GetLocalCamera();
            if (!camera)
            {
                ReleaseLines();
                return;
            }

            GenericSkill special = skillLocator.special;
            if (!special || !(special.skillDef is UnforgivenSpecialTrackerSkillDef specialDef) || !special.CanExecute())
            {
                targets.Clear();
                return;
            }

            specialDef.GetEligibleTargets(special, targets);
            if (targets.Count == 0)
            {
                return;
            }

            if (lines == null)
            {
                lines = new AirborneTargetLines();
                if (!lines.IsValid)
                {
                    ReleaseLines();
                    enabled = false;
                    return;
                }
            }

            lines.Draw(body.corePosition, targets, camera);
        }

        private Camera GetLocalCamera()
        {
            foreach (LocalUser localUser in LocalUserManager.readOnlyLocalUsersList)
            {
                if (localUser.cachedBody != body || localUser.isUIFocused)
                {
                    continue;
                }

                CameraRigController rig = localUser.cameraRigController;
                if (rig && rig.target == gameObject && rig.localUserViewer == localUser &&
                    rig.sceneCam && rig.sceneCam.isActiveAndEnabled)
                {
                    return rig.sceneCam;
                }
            }
            return null;
        }

        private void OnDisable()
        {
            ReleaseLines();
        }

        private void OnDestroy()
        {
            ReleaseLines();
        }

        private void ReleaseLines()
        {
            if (lines != null)
            {
                lines.Dispose();
                lines = null;
            }
            targets.Clear();
        }
    }
}
