using HutongGames.PlayMaker;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdThrowWait : FsmStateAction
    {
        
        public override void OnEnter()
        {
            // 1  play the throw wait animation
            //    - anim  Super Jump Throw Wait
            // 2  read isNeedleVisible
            // 3  waitTime = 0 if isNeedleVisible, 0.5 otherwise
        }

        public override void OnUpdate()
        {
            // 4  finish once waitTime has elapsed
            Finish();
        }
    }
}
