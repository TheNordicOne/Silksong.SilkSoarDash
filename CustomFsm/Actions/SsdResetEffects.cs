using GlobalEnums;
using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using UnityEngine;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdResetEffects : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;

        public override void OnEnter()
        {
            var hero = HeroController.instance;
            if (hero == null)
            {
                Finish();
                return;
            }

            SsdHeroState.Dashing = false;
            SsdHeroState.OnWall = false;
            SsdHeroState.WallStart = false;
            SsdClones.AimForUprightHero();

            Hero.hero_state = ActorStates.idle;

            Fsm.GetFsmBool(SsdVars.DidStartFlash).Value = false;
            Fsm.GetFsmBool(SsdVars.AirTarget).Value = false;

            ReattachStickNeedle();

            Deactivate(SsdClones.AnticEffectL);
            Deactivate(SsdClones.AnticEffectR);
            Deactivate(SsdClones.ChargedEffect);
            Deactivate(SsdClones.ChargingFader);
            Deactivate(SsdClones.Thread);
            Deactivate(SsdClones.ThrowNeedle);
            Deactivate(SsdClones.RetractNeedle);
            Deactivate(SsdClones.StickNeedle);

            EventRegister.SendEvent(SsdRegisterEvents.SuperJumpEnded);

            Finish();
        }

        private static void Deactivate(Transform effect)
        {
            if (effect != null)
            {
                effect.gameObject.SetActive(false);
            }
        }

        private void ReattachStickNeedle()
        {
            var needleStick = Fsm.GetFsmGameObject(SsdVars.StickNeedle).Value;
            var parent = Fsm.GetFsmGameObject(SsdVars.StickNeedleParent).Value;

            if (needleStick == null || parent == null)
            {
                return;
            }

            // unparented it took Hornet's facing into its own scale, so the harpoon catch resets it
            needleStick.transform.SetParent(parent.transform, true);
            needleStick.transform.localScale = Vector3.one;
        }
    }
}
