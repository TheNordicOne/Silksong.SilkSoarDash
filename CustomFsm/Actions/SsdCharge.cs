using BepInEx.Logging;
using HutongGames.PlayMaker;
using SilkSoarDash.CustomFsm.Constants;
using SilkSoarDash.Extensions;
using SilkSoarDash.Logging;

namespace SilkSoarDash.CustomFsm.Actions
{
    public class SsdCharge : FsmStateAction
    {
        private static readonly ManualLogSource SsdLog = SilkLog.For<SsdCharge>();

        private static HeroController Hero => HeroController.instance;
        private static SilkSpool Spool =>  SilkSpool.Instance;
        
        public override void OnEnter()
        {
            Hero.StopAnimationControl();
         
           var didAddUsingSilk =  Spool.AddUsing(SilkSpool.SilkUsingFlags.Normal, SsdVars.SilkCost);
           Fsm.GetFsmBool(SsdVars.DidAddUsingSilk).Value = didAddUsingSilk;

           var groundEffect = SsdClones.ExtraGroundEffect;
           if (groundEffect != null)
           {
               groundEffect.gameObject.SetActive(true);
           }

           Hero.GetComponent<tk2dSpriteAnimator>().Play(SsdAnims.Antic);

           // - audio   hornet_superjump_pt_1_into_position
           // - audio   hornet_superjump_pt_2_charge_2d
           // - effect  Effects/Super Jump Antic Effect L
           // - effect  Effects/Super Jump Antic Effect R
           // - anim    Super Jump Antic Effect
           StartRumblingFocus();

           // - effect  Effects/Super Jump Charging Fader
           
           Hero.SetCState(SsdCStates.FreezeCharge, true);
        }

        private void StartRumblingFocus()
        {
            var cameraParent = GameCameras.instance.cameraParent.gameObject;
            var cameraShake = FSMUtility.LocateFSM(cameraParent, SsdCamera.ShakeFsm);

            FSMUtility.SetBool(cameraShake, SsdCamera.RumblingFocus, true);
            FSMUtility.SendEventToGameObject(cameraParent, SsdCamera.FocusRumble);

            Fsm.GetFsmBool(SsdVars.StartedRumblingFocus).Value = true;
        }

        public override void OnUpdate()
        {
            var ia = GameManager.instance?.inputHandler?.inputActions;
            if (ia == null)
            {
                return;
            }

            var released = !ia.SuperDash.IsPressed;
            var falling = Hero.IsFalling();

            if (released || falling)
            {
                SsdLog.Debug("cancelled released={Released} falling={Falling}", released, falling);
                Fsm.Event(SsdEvents.Cancelled);
            }
            
        }
    }
}
