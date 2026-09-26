using HutongGames.PlayMaker;
using UnityEngine;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdThrowWait : FsmStateAction
    {
        private float _waitTime;
        private float _elapsed;
        private const float WaitTimeOffScreen = 0.5f;

        public override void OnEnter()
        {
            // 1  play the throw wait animation
            //    - anim  Super Jump Throw Wait
            // 2  read isNeedleVisible
            var isNeedleVisible = !Fsm.GetFsmBool(SsdVars.NeedleOffScreen).Value;

            // 3  Set waitTime
            _waitTime = isNeedleVisible ? 0f : WaitTimeOffScreen;
            _elapsed = 0f;
        }

        public override void OnUpdate()
        {
            // 4  finish once waitTime has elapsed
            _elapsed += Time.deltaTime;

            if (_elapsed > _waitTime)
            {
                Finish();
            }
        }
    }
}