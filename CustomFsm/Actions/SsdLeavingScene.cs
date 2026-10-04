using BepInEx.Logging;
using GlobalEnums;
using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Extensions;
using SilkSoarDash.Logging;
using UnityEngine;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdLeavingScene : FsmStateAction
    {
        private static readonly ManualLogSource SsdLog = SilkLog.For<SsdLeavingScene>();

        public override void OnEnter()
        {
            // the game sends this on every room change, so ignore it while the ability is not running
            var previous = Fsm.PreviousActiveState;
            if (previous == null || previous.Name == SsdStates.Inactive)
            {
                Fsm.Event(SsdEvents.Cancelled);
                return;
            }

            // the game only continues a sprint through a side gate, so the pose and the hand-off stop at any other exit
            var exitGate = GameManager.instance.LastSceneLoad.SceneLoadInfo.HeroLeaveDirection;
            var sideExit = exitGate == GatePosition.left || exitGate == GatePosition.right;
            SsdHeroState.ExitedDashing = SsdHeroState.Dashing && sideExit;
            SsdHeroState.CrossingRoom = SsdHeroState.ExitedDashing;
            SsdHeroState.CancelQueued = false;
            if (SsdHeroState.ExitedDashing)
            {
                SsdHeroState.ExitHeight = HeroController.instance.HeightAboveDoorFloor(TouchedGate());
            }

            // makes the next room skip its walk-in and send ENTER SPRINTING, which SendEventSafePrefix turns into our EnterDashing
            if (SsdHeroState.ExitedDashing)
            {
                HeroController.instance.exitedSprinting = true;
            }

            SsdLog.Debug("leaving from={State} dashing={Dashing} height={Height}", previous.Name, SsdHeroState.Dashing, SsdHeroState.ExitHeight);
            Finish();
        }

        private static TransitionPoint TouchedGate()
        {
            var heroCollider = HeroController.instance.GetComponent<Collider2D>();
            foreach (var gate in TransitionPoint.TransitionPoints)
            {
                if (heroCollider.IsTouching(gate.GetComponent<Collider2D>()))
                {
                    return gate;
                }
            }

            return null;
        }
    }
}