using HutongGames.PlayMaker;
using UnityEngine;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdCharge : FsmStateAction
    {

        private const float ChargeTime = 0.8f;

        private float _elapsed;

        public override void Reset()
        {
            _elapsed = 0f;
        }

        public override void OnEnter()
        {
            _elapsed = 0f;
        }

        public override void OnUpdate()
        {
            var ia = GameManager.instance?.inputHandler?.inputActions;
            if (ia == null)
            {
                return;
            }

            if (!ia.SuperDash.IsPressed)
            {
                Fsm.Event(SsdEvents.Cancelled);
                return;
            }

            _elapsed += Time.deltaTime;

            if (!(_elapsed >= ChargeTime))
            {
                return;
            }
            
            Finish();
        }
    }
}
