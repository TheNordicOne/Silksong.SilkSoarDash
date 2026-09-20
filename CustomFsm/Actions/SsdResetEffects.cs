using HutongGames.PlayMaker;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdResetEffects : FsmStateAction
    {
        private static readonly string[] EffectPaths =
        {
            "Effects/Super Jump Antic Effect L",
            "Effects/Super Jump Antic Effect R",
            "Effects/Super Jump Charged",
            "Effects/Super Jump Charging Fader",
            "Effects/Super Jump Thread",
            "Special Attacks/Super Jump Needle Throw",
            "Special Attacks/Super Jump Needle Throw Fall",
            "Special Attacks/Super Jump Needle Stick"
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
