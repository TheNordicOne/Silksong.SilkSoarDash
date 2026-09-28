using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Extensions;
using UnityEngine;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdDashStartQuick : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;
        
        public override void OnEnter()
        {
            // the harpoon dash lifts her off the ground first, so she flies instead of sliding along it
            if (Hero.cState.onGround)
            {
                Hero.transform.Translate(0f, SsdVars.KickUpHeight, 0f, Space.World);
            }

            LeaveEntryGate(Fsm.GetFsmFloat(SsdVars.Direction).Value);

            SsdClones.Damager.gameObject.SetActive(true);
            SsdClones.DashEffect.gameObject.SetActive(true);
            
            Hero.PlayAnim(SsdAnims.Loop);
            
            Hero.SetCState(SsdCStates.FreezeCharge, false);

            SsdHeroState.Dashing = true;
            
            Hero.RelinquishControlNotVelocity();
            
            Hero.AffectedByGravity(false);

            Hero.ApplySsdVelocity(Fsm);

            
            // CameraTarget.SetSuperJump is vertical only. Skipped until custom implementation
            
            Finish();
        }

        // a side gate shoves a hero facing away from it back to its edge with zero velocity, which the wall check would read as a hit
        private static void LeaveEntryGate(float direction)
        {
            var gate = Hero.sceneEntryGate;
            if (gate == null)
            {
                return;
            }

            var gateBounds = gate.GetComponent<Collider2D>().bounds;
            var heroBounds = Hero.GetComponent<Collider2D>().bounds;
            var shift = direction > 0f ? gateBounds.max.x - heroBounds.min.x : gateBounds.min.x - heroBounds.max.x;
            if (shift * direction <= 0f)
            {
                return;
            }

            Hero.transform.Translate(shift + SsdVars.EntryGateClearance * direction, 0f, 0f, Space.World);
        }
    }
}
