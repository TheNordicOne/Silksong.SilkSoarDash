using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using UnityEngine;


namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdThreadLoop : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;
        private MeshRenderer _threadLoop;

        public override void OnEnter()
        {
            var threadLoop = Hero.transform.Find(SsdObjects.ThreadLoop);
            _threadLoop = threadLoop == null ? null : threadLoop.GetComponent<MeshRenderer>();
        }

        public override void OnUpdate()
        {
            if (_threadLoop == null)
            {
                return;
            }

            var direction = Fsm.GetFsmFloat(SsdVars.Direction).Value;
            var origin = Hero.transform.position;
            var ahead = new Vector2(direction, 0f);
            var hit = Physics2D.Raycast(origin, ahead, SsdVars.ThreadRayDistance, 1 << SsdVars.TerrainLayer);

            _threadLoop.enabled = hit.collider == null;
        }
    }
}