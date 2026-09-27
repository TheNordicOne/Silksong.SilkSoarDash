using GlobalEnums;
using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Extensions;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdResetEffects : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;
        
        private static readonly string[] EffectPaths =
        {
            SsdObjects.AnticEffectL,
            SsdObjects.AnticEffectR,
            SsdObjects.ChargedEffect,
            SsdObjects.ChargingFader,
            SsdObjects.Thread,
            SsdObjects.ThrowNeedle,
            SsdObjects.RetractNeedle,
            SsdObjects.StickNeedle
        };

        public override void OnEnter()
        {
            var hero = HeroController.instance;
            if (hero == null)
            {
                Finish();
                return;
            }
            
            Hero.SetCState(SsdCStates.SuperDashOnWall, false);
            
            Hero.hero_state = ActorStates.idle;
            
            // clear the sprite flash tracker

            ReattachStickNeedle();

            foreach (var path in EffectPaths)
            {
                var effect = hero.transform.Find(path);
                if (effect != null)
                {
                    effect.gameObject.SetActive(false);
                }
            }

            EventRegister.SendEvent(SsdRegisterEvents.SuperJumpEnded);

            
            
            PointThreadsUp();
            Finish();
        }

        private static void PointThreadsUp()
        {
            var thread = Hero.transform.Find(SsdObjects.Thread);
            if (thread != null)
            {
                thread.PointUp();
            }

            var threadLoop = Hero.transform.Find(SsdObjects.ThreadLoop);
            if (threadLoop != null)
            {
                threadLoop.PointUp();
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

            needleStick.transform.SetParent(parent.transform, true);
        }
    }
}
