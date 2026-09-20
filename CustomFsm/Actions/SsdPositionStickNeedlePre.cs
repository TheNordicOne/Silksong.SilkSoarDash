using HutongGames.PlayMaker;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdPositionStickNeedlePre : FsmStateAction
    {
        
        public override void OnEnter()
        {
            // 1  Special Attacks/Super Jump Needle Throw OFF
            // 2  Special Attacks/Super Jump Needle Stick ON
            // 3  DidHit false -> cancel
            // 4  NoSuperJumpCollider.IsInside(HitPoint) true -> cancel
            // 5  HitObject has a NoSuperJumpCollider -> cancel
            // 6  store the stick needle's current parent
            // 7  unparent the stick needle, keep its world position
            // 8  read the throw needle Y
            // 9  move the stick needle to HitPoint
            // 10 read the stick needle Y
            // 11 offset = throw needle Y - stick needle Y
            // 12 translate the stick needle by offset on Y
            // 13 IsGate true -> transition gate
            // 14 stick needle off camera -> finish here

            // Presentation
            // - effect  Nail Terrain Hit Effect

            Finish();
        }
    }
}
