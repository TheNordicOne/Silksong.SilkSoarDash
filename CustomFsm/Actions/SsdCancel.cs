using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdCancel : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;
        private static SilkSpool Spool => SilkSpool.Instance;

        private static readonly string[] EffectPaths =
        {
            SsdObjects.ExtraGroundEffect,
            SsdObjects.ExtraThrowEffect,
            SsdObjects.Thread,
            SsdObjects.Damager
        };

        public override void OnEnter()
        {
            if (Fsm.GetFsmBool(SsdVars.DidAddUsingSilk).Value)
            {
                Spool.RemoveUsing(SilkSpool.SilkUsingFlags.Normal, SsdVars.SilkCost);
            }

            foreach (var path in EffectPaths)
            {
                var effect = Hero.transform.Find(path);
                if (effect != null)
                {
                    effect.gameObject.SetActive(false);
                }
            }

            // audio  stop the charge loop
            // audio  stop Sounds/Superjump Loop

            Hero.SetCState(SsdCStates.SuperDashing, false);
            Hero.SetCState(SsdCStates.FreezeCharge, false);

            // CameraTarget.SetSuperJump is vertical only. Skipped

            if (!Fsm.GetFsmBool(SsdVars.DidStartFlash).Value)
            {
                Finish();
                return;
            }

            // flash  cancel the sprite flash by its stored id

            Finish();
        }
    }
}
