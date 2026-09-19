Main path

Inactive
--DO MOVE-->        Enough Silk?        GetPlayerDataVariable, IntCompare
--FINISHED-->       Relinquish Control
--FINISHED-->       Start Delay         DecelerateXY, Wait, ListenForSuperdash
--FINISHED-->       Ground Charge       AddUsingSilk, charge anim, Wait
--WAIT-->           Ground Charged      TakeSilkV2, RayCast2d
--BUTTON UP-->      Throw Needle Start
--FINISHED-->       Get Distance        SuperJumpRaycast
--FINISHED-->       Throw Needle        SetVelocity2d on the needle
--FINISHED-->       Position Stick Needle Pre
--FINISHED-->       Position Stick Needle
--FINISHED-->       Throw Wait
--FINISHED-->       Jump Antic
--FINISHED-->       Dash Start          SetVelocity2d on Hornet
--FINISHED-->       Dashing             SetVelocity2d, SetGravity2dScale, RayCast2dV2
--WAIT-->           Cancelable          ListenForJump, ListenForAttackV2
--HIT ROOF-->       Hit Roof Hard
--FINISHED-->       Hit Roof
--FINISHED-->       Regain Control To Idle
--FINISHED-->       Reset Effects
--FINISHED-->       Inactive

The four phases

1. Gate and charge. Check silk, take control, decelerate, charge on the ground. Silk is consumed at Ground Charged.
2. Throw. Get Distance runs SuperJumpRaycast to decide how far up. Throw Needle launches the needle. Position Stick Needle plants it.
3. Travel. Jump Antic, then Dash Start sets Hornet's velocity, then Dashing sustains it with gravity disabled.
4. End. Roof hit, or player cancel with jump or attack, then unwind through Regain Control To Idle and Reset Effects.

Branches worth knowing

- Release too early. Start Delay and Ground Charge both watch for BUTTON UP and abort.
- Spikes. Throw Needle has DAMAGER HIT SPIKES -> Hit Spikes.
- Room transition. Position Stick Needle Pre has TRANSITION GATE -> Hit Transition Gate. The separate chain Pre Entered Jumping -> Entered Jumping -> Begin Jumping -> Dash Start Quick resumes the ascent in the next room. That is what your "ENTER SUPERJUMPING" event drives.
- Cancel mid-flight. Cancelable -> Fall Needle Cancel -> Air Cancel.
