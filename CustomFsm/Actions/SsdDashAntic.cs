using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using UnityEngine;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdDashAntic : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;

        public override void OnEnter()
        {
            SsdEffects.StopLoop();
            SsdEffects.PlayOneShot(SsdAudio.JumpAntic, SsdAudio.WidePitchMin, SsdAudio.WidePitchMax);

            // the harpoon dash lifts her off the ground first, so she flies instead of sliding along it
            if (Hero.cState.onGround)
            {
                Hero.transform.Translate(0f, SsdVars.KickUpHeight, 0f, Space.World);
            }

            // the harpoon clips go straight from Harpoon Throw to Harpoon Dash, so there is no jump antic to play
            Finish();
        }
    }
}
