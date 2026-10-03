using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Extensions;
using UnityEngine;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdHitWall : FsmStateAction
    {
        private static HeroController Hero => HeroController.instance;

        public override void OnEnter()
        {
            SsdHeroState.OnWall = true;
            SsdHeroState.Dashing = false;
            Hero.AffectedByGravity(true);

            Hero.PlayAnim(SsdAnims.Catch);
            SsdClones.GrabEffect.gameObject.SetActive(true);

            Hero.Body.linearVelocity = Vector2.zero;
        }

        public override void OnUpdate()
        {
            Hero.ClampFall();

            if (Hero.IsAnimPlaying(SsdAnims.Catch))
            {
                return;
            }

            Finish();
        }
    }
}