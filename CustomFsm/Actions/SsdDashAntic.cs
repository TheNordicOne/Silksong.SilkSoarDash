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
        }

        // the harpoon clips have no jump antic, so the throw pose is held for as long as vanilla's antic clip runs
        public override void OnUpdate()
        {
            _elapsed += Time.deltaTime;
            if (_elapsed < Hero.AnimSeconds(SsdAnims.JumpAntic))
            {
                return;
            }

            // the harpoon dash lifts her off the ground first, so she flies instead of sliding along it
            if (Hero.cState.onGround)
            {
                Hero.transform.Translate(0f, SsdVars.KickUpHeight, 0f, Space.World);
            }

            Finish();
        }
    }
}
