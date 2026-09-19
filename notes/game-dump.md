# Silk Soar (SuperJump)

Internal name: `SuperJump`. No type, field or string in `Assembly-CSharp.dll` contains "Soar".

Source: runtime dump of `HeroController.superJumpFSM`.
GameObject `Hero_Hornet(Clone)`, FSM name `Superjump`.

## Locations

`Assembly-CSharp.dll`, global namespace unless stated.

| Member                                           | Type             | Access                          |
|--------------------------------------------------|------------------|---------------------------------|
| `HeroController.superJumpFSM`                    | `PlayMakerFSM`   | public field                    |
| `HeroController.CanSuperJump()`                  | `bool`           | public instance                 |
| `HeroController.CanHarpoonDash()`                | `bool`           | public instance                 |
| `HeroController.inputHandler`                    | `InputHandler`   | **private** field               |
| `GameManager.instance`                           | `GameManager`    | public static property          |
| `GameManager.inputHandler`                       | `InputHandler`   | public instance property        |
| `InputHandler.inputActions`                      | `HeroActions`    | public field                    |
| `PlayerData.hasSuperJump`                        | `bool`           | public field                    |
| `PlayerData.HasSeenSuperJump`                    | `bool`           | public field                    |
| `PlayerData.completedSuperJumpSequence`          | `bool`           | public field                    |
| `PlayerData.SilkSkillCost`                       | `int`            | public property                 |
| `HeroAnimationController.SetPlaySuperJumpFall()` | `void`           | public instance                 |
| `NoSuperJumpCollider.IsInside(Vector2)`          | `bool`           | public static                   |
| `SuperJumpRaycast`                               | `FsmStateAction` | `HutongGames.PlayMaker.Actions` |
| `CurrencyManager.AddGeo(int)`                    | `void`           | public static                   |

`PlayMaker.dll` holds `Fsm`, `FsmState`, `FsmTransition`, `FsmStateAction`, `NamedVariable`.
It does not hold the graph. The graph is serialized in the Hero prefab.

## C# trigger

`HeroController.LookForInput()` reads these actions:
`Attack`, `Dash`, `Down`, `Jump`, `Left`, `MoveVector`, `QuickCast`, `Right`, `SuperDash`.
`Up` is not among them.

```
SuperDash.WasPressed && IsPressingOnlyDown() && CanSuperJump()
  -> if controlReqlinquished: EventRegister.SendEvent(FsmCancel), RegainControl(), StartAnimationControlToIdle()
  -> superJumpFSM.SendEventSafe("DO MOVE")
```

`SuperDash.WasPressed && (!IsPressingOnlyDown() || !CanSuperJump())` falls through to
`CanHarpoonDash()`. `CanHarpoonDash()` has one call site.

`IsPressingOnlyDown()` is `Down.IsPressed && !Right.IsPressed && !Left.IsPressed`.

`CanSuperJump()` requires: `!gm.isPaused`, `hero_state != hard_landing`,
`hero_state != dash_landing`, `cState.onGround`, `!cState.dashing`, `!cState.hazardDeath`,
`!cState.hazardRespawning`, `!cState.backDashing`,
`!cState.attacking || attack_time >= Config.AttackRecoveryTime`, `CanDoFSMCancelMove()`,
`!cState.recoilFrozen`, `!cState.recoiling`, `!cState.transitioning`, `playerData.hasSuperJump`.

Scene entry sends `"PRE ENTER SUPERJUMPING"` and `"ENTER SUPERJUMPING"` when
`exitedSuperDashing` is set.

## FSM path

```
Inactive
--DO MOVE-->        Enough Silk?        GetPlayerDataVariable, IntCompare
--FINISHED-->       Relinquish Control
--FINISHED-->       Start Delay         DecelerateXY, ListenForSuperdash, Wait 0.15
--FINISHED-->       Ground Charge       DecelerateXY, AddUsingSilk, Tk2dPlayAnimation, Wait 0.8
--WAIT-->           Ground Charged      RemoveUsingSilk, TakeSilkV2, RayCast2d, ListenForSuperdash
--BUTTON UP-->      Throw Needle Start
--FINISHED-->       Get Distance        RayCast2dV2, SuperJumpRaycast
--FINISHED-->       Throw Needle        SetVelocity2d (0,150), CheckCollisionSideV2, Wait 0.8
--FINISHED-->       Position Stick Needle Pre
--FINISHED-->       Position Stick Needle
--FINISHED-->       Throw Wait
--FINISHED-->       Jump Antic
--FINISHED-->       Dash Start          SetVelocity2d y=33 once
--FINISHED-->       Dashing             SetVelocity2d y=33 everyFrame, SetGravity2dScale 0, Wait 0.2
--WAIT-->           Cancelable          ListenForJump, ListenForAttackV2, SetVelocity2d y=33 everyFrame
--HIT ROOF-->       Hit Roof Hard
--FINISHED-->       Hit Roof
--FINISHED-->       Regain Control To Idle
--FINISHED-->       Reset Effects
--FINISHED-->       Inactive
```

Other transitions:

| From                          | Event                 | To                            |
|-------------------------------|-----------------------|-------------------------------|
| `Enough Silk?`                | CANCEL                | `Inactive`                    |
| `Start Delay`                 | BUTTON UP             | `Regain Control`              |
| `Ground Charge`               | BUTTON UP             | `Charge Cancel Ground`        |
| `Throw Needle`                | DAMAGER HIT SPIKES    | `Hit Spikes`                  |
| `Position Stick Needle Pre`   | TRANSITION GATE       | `Hit Transition Gate`         |
| `Position Stick Needle Pre`   | CANCEL                | `Catch Wait`                  |
| `Position Stick Needle`       | CANCEL                | `Catch Wait`                  |
| `Cancelable`                  | NORM CANCEL           | `Fall Needle Cancel`          |
| `Fall Needle Cancel`          | FINISHED              | `Air Cancel`                  |
| `Leaving Scene`               | CANCEL / LEVEL LOADED | `Cancel`                      |
| `Pre Entered Jumping`         | ENTER SUPERJUMPING    | `Entered Jumping`             |
| `Entered Jumping`             | FINISHED              | `Begin Jumping`               |
| `Begin Jumping`               | FINISHED              | `Position Stick Needle Pre 2` |
| `Position Stick Needle Pre 2` | FINISHED              | `Dash Start Quick`            |
| `Position Stick Needle Pre 2` | CANCEL                | `Queue Cancel`                |
| `Dash Start Quick`            | FINISHED              | `Dashing`                     |
| `Hit Roof Soft`               | FINISHED              | `Hit Roof`                    |

`Begin Jumping` and `Get Distance` are the only states calling `SuperJumpRaycast`.

## Silk

|                                      |                                                                |
|--------------------------------------|----------------------------------------------------------------|
| `Enough Silk?`                       | reads `silk`, `IntCompare integer2 = 1`, lessThan fires CANCEL |
| `Silk Cost` (FSM var)                | 1                                                              |
| `AddUsingSilk` (`Ground Charge`)     | Amount 1, UsingType Normal                                     |
| `RemoveUsingSilk` (`Ground Charged`) | Amount 1, UsingType Normal                                     |
| `TakeSilkV2` (`Ground Charged`)      | Amount 1, TakeSource Normal                                    |

`PlayerData.SilkSkillCost` returns 4, or 3 when `GlobalSettings.Gameplay.FleaCharmTool.IsEquippedHud`
and `health >= CurrentMaxHealth`. It is not read by this FSM.

## Timing

| State           | Action | Value |
|-----------------|--------|-------|
| `Start Delay`   | Wait   | 0.15  |
| `Ground Charge` | Wait   | 0.8   |
| `Throw Needle`  | Wait   | 0.8   |
| `Dashing`       | Wait   | 0.2   |

FSM vars: `Charge Time` 0.8, `Cancelable Time` 0.2.

## Motion

| State           | Action            | Value                         |
|-----------------|-------------------|-------------------------------|
| `Dash Start`    | SetVelocity2d     | y 33, everyFrame False        |
| `Dashing`       | SetVelocity2d     | y 33, everyFrame True         |
| `Cancelable`    | SetVelocity2d     | y 33, everyFrame True         |
| `Dashing`       | SetGravity2dScale | 0                             |
| `Throw Needle`  | SetVelocity2d     | vector (0, 150)               |
| `Start Delay`   | DecelerateXY      | X 0.9, Y 0, brakeOnExit False |
| `Ground Charge` | DecelerateXY      | X 0.9, Y 0, brakeOnExit True  |

FSM vars: `Jump Speed` 33, `Initial Throw Needle Y` 5.37.

## Raycasts

| State            | Action             | Direction | Distance | Space |
|------------------|--------------------|-----------|----------|-------|
| `Get Distance`   | `SuperJumpRaycast` | (0, 1)    | 350      | World |
| `Get Distance`   | `RayCast2dV2`      | (0, 1)    | 350      | World |
| `Dashing`        | `RayCast2dV2`      | (0, 1)    | 10       | Self  |
| `Cancelable`     | `RayCast2dV2`      | (0, 1)    | 10       | Self  |
| `Ground Charged` | `RayCast2d`        | (0, -1)   | 2        | Self  |

`SuperJumpRaycast`: `contactFilter.layerMask = 8448`, `useTriggers = true`. Trigger hits are
skipped unless the collider has a `TransitionPoint` with `GetGatePosition() == 0`, which sets
`StoreIsTransitionGate`. A second cast adds `0x420000` to the mask and sets `StoreHitSpikes`
when a `DamageHero` with `hazardType == HazardType.SPIKES` is hit.
Outputs: `StoreDidHit`, `StoreHitObject`, `StoreHitPoint`, `StoreDistance`,
`StoreIsTransitionGate`, `StoreHitSpikes`.

## GameObjects (FSM variables)

```
Super Jump Needle Throw          Super Jump Needle Throw Fall
Super Jump Needle Stick          Super Jump Thread
Super Jump Thread Loop           Super Jump Damager
Super Jump Charging Fader        Super Jump Charged
Super Jump Antic Effect L / R    Super Jump Catch Effect
Super Jump Extra Throw Effect    Super Jump Extra Ground Effect
Superjump Loop                   Nail Art Ready
Camera Target                    Special Attacks
Effects                          Move To
```

`Ground Charged` spawns `Hornet_Super_Jump_Ready_Burst` at offset (0, -1.5, 0).

## Input actions

`HeroActions` fields:

```
Left Right Up Down MoveVector
RsUp RsDown RsLeft RsRight RightStick
Jump Evade Dash SuperDash DreamNail Attack Cast
QuickMap QuickCast Taunt Pause
MenuSubmit MenuCancel MenuExtra MenuSuper
PaneLeft PaneRight
OpenInventory OpenInventoryMap OpenInventoryJournal OpenInventoryTools OpenInventoryQuests
SwipeInventoryMap SwipeInventoryJournal SwipeInventoryTools SwipeInventoryQuests
```

Each is `InControl.PlayerAction` except `MoveVector` and `RightStick`, which are
`InControl.PlayerTwoAxisAction` (`.Vector`, `.X`, `.Y`).

`InControl.OneAxisInputControl` exposes `WasPressed`, `IsPressed`, `WasReleased`,
`HasChanged`, and an `implicit operator bool` returning `IsPressed`.

Gameplay readers per action:

| Action              | Read by                                                                                             |
|---------------------|-----------------------------------------------------------------------------------------------------|
| `RsLeft`, `RsRight` | `HeroActions`, `InputHandler`, `InventoryPaneInput` only                                            |
| `RightStick`        | `HeroActions`, `InputHandler` only                                                                  |
| `RsUp`, `RsDown`    | `HeroController.Update()`, `ListenForRsUp`, `ListenForRsDown`, `InventoryPaneInput`, `MenuScroller` |
| `Evade`             | `ListenForBackdash`                                                                                 |
| `Taunt`             | `ListenForTaunt`, `ListenForTauntV2`                                                                |
| `DreamNail`         | `ListenForDreamNail`, `Platform`                                                                    |
