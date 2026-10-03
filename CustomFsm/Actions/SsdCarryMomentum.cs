using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using UnityEngine;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdCarryMomentum : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;

        // like the harpoon dash end, regaining control clears these, so this has to come after it
        public override void OnEnter()
        {
            if (!Hero.cState.onGround)
            {
                Hero.AddExtraAirMoveVelocity(new HeroController.DecayingVelocity
                {
                    Velocity = new Vector2(Hero.Body.linearVelocity.x, 0f),
                    Decay = SsdVars.CancelMomentumDecay,
                    CancelOnTurn = true,
                    SkipBehaviour = HeroController.DecayingVelocity.SkipBehaviours.WhileMoving
                });
            }

            Finish();
        }
    }
}
