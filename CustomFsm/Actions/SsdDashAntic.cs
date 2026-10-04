using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Extensions;
using UnityEngine;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdDashAntic : FsmStateAction
    {
        private float _elapsed;

        private static HeroController Hero => HeroController.instance;

        public override void OnEnter()
        {
            SsdEffects.StopLoop();
            SsdEffects.PlayOneShot(SsdAudio.JumpAntic, SsdAudio.WidePitchMin, SsdAudio.WidePitchMax);
            _elapsed = 0f;

            if (SsdHeroState.WallStart)
            {
                Hero.PlayAnim(SsdAnims.JumpAntic);
            }
        }

        // the harpoon clips have no jump antic, so on the ground she holds the throw pose and leans into the dash pose for as long as vanilla's antic clip runs
        public override void OnUpdate()
        {
            _elapsed += Time.deltaTime;
            var duration = Hero.AnimSeconds(SsdAnims.JumpAntic);
            if (_elapsed < duration)
            {
                Lean(_elapsed / duration);
                return;
            }

            Lean(1f);

            // the harpoon dash lifts her off the ground first, so she flies instead of sliding along it
            if (Hero.cState.onGround)
            {
                Hero.transform.Translate(0f, SsdVars.KickUpHeight, 0f, Space.World);
            }

            Finish();
        }

        private void Lean(float progress)
        {
            if (SsdHeroState.WallStart)
            {
                return;
            }

            // slow at first and fastest at launch, so the turn runs straight into the dash
            Hero.LeanIntoDash(Fsm.GetFsmFloat(SsdVars.Direction).Value, progress * progress);
        }
    }
}
