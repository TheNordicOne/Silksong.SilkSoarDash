using HutongGames.PlayMaker;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdResetEffects : FsmStateAction
    {
        private static readonly string[] EffectPaths =
        {
            SsdObjects.AnticEffectL,
            SsdObjects.AnticEffectR,
            SsdObjects.ChargedEffect,
            SsdObjects.ChargingFader,
            SsdObjects.Thread,
            SsdObjects.ThrowNeedle,
            SsdObjects.ThrowNeedleFall,
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

            hero.RegainControl();
            hero.StartAnimationControlToIdle();

            ReattachStickNeedle();

            foreach (var path in EffectPaths)
            {
                var effect = hero.transform.Find(path);
                if (effect != null)
                {
                    effect.gameObject.SetActive(false);
                }
            }

            Finish();
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
