# superJumpFSM dump

Runtime dump of `HeroController.superJumpFSM`.
GameObject `Hero_Hornet(Clone)`, FsmName `Superjump`.

## PlayerData at dump time

```
silk           = 3
silkMax        = 18
SilkSkillCost  = 4
hasSuperJump   = True
hasHarpoonDash = True
```

## Full state listing

States marked `[no values]` have action names only.

### Inactive
```
transition DO MOVE -> Enough Silk?
```

### Init
```
GetOwner
FindChild x7, GetPosition, FindChild x6, FindGameObject, FindChild x3,
FindGameObject, FindChild x2, ActivateGameObject x2, FindChild,
ActivateGameObject, FindChild, ActivateGameObject
transition FINISHED -> Inactive
```

### Enough Silk?
```
GetPlayerDataVariable   VariableName = silk, StoreValue = 0
IntCompare              integer1 = 0, integer2 = 1, lessThan = <event>, everyFrame = False
transition CANCEL -> Inactive
transition FINISHED -> Relinquish Control
```

### Relinquish Control
```
SendMessage x2
transition FINISHED -> Start Delay
```

### Start Delay
```
DecelerateXY        decelerationX = 0.9, decelerationY = 0, brakeOnExit = False
ListenForSuperdash
Wait                time = 0.15
transition BUTTON UP -> Regain Control
transition FINISHED -> Ground Charge
```

### Ground Charge
```
SendMessage
DecelerateXY        decelerationX = 0.9, decelerationY = 0, brakeOnExit = True
AddUsingSilk        Amount = 1, UsingType = Normal, DidAddTracker = False
ActivateGameObject
Tk2dPlayAnimation
PlayAudioEvent x2
ActivateGameObject
Tk2dPlayAnimation
ActivateGameObject
Tk2dPlayAnimation
CallMethodProper
SetBoolValue
SetFsmBool
SendEventByName
ListenForSuperdash
ActivateGameObject
FadeNestedFadeGroup
FadeNestedFadeGroupV2
Wait                time = 0.8
GetVelocity2d
FloatCompare
transition BUTTON UP -> Charge Cancel Ground
transition WAIT -> Ground Charged
```

### Ground Charged
```
RemoveUsingSilk     Amount = 1, UsingType = Normal, DidAddTracker = False
TakeSilkV2          Amount = 1, TakeSource = Normal
SpawnObjectFromGlobalPoolV2   gameObject = Hornet_Super_Jump_Ready_Burst,
                              spawnPoint = Hero_Hornet(Clone),
                              position = (0, -1.5, 0), rotation = (0, 0, 0)
AudioStopV2, PlayAudioEvent, AudioPlayInState, SetAudioClip
ActivateGameObject, DoSpriteFlashNamed
SetBoolValue, SetFsmBool, SetBoolValue, SetBoolValue, SetFsmBool
SendEventByName
ListenForSuperdash
SetBoolValue
RayCast2d           direction = (0, -1), space = Self, distance = 2,
                    minDepth = 0, maxDepth = 0, hitEvent = null
BoolTest
transition BUTTON UP -> Throw Needle Start
```

### Charge Cancel Ground  `[no values]`
```
RemoveUsingSilk, ActivateGameObject, AudioStopV2, ActivateGameObject x2,
FadeNestedFadeGroup, CallMethodProper, Tk2dPlayAnimationWithEvents,
Tk2dPlayAnimation x2, SetFsmBool x2, SetVelocity2d
transition FINISHED -> Regain Control To Idle
```

### Throw Needle Start  `[no values]`
```
ActivateGameObject, FadeNestedFadeGroup, Tk2dPlayAnimation x2, CancelFlashByID,
SetBoolValue, SetFsmBool x2, SetBoolValue, SendMessageV2,
Tk2dPlayAnimationWithEvents, SendEventToRegister
transition FINISHED -> Get Distance
```

### Get Distance
```
GetPosition x2
FloatOperator
RayCast2dV2         direction = (0, 1), space = World, distance = 350,
                    minDepth = 0, maxDepth = 0
SuperJumpRaycast    Direction = (0, 1), Space = World, Distance = 350,
                    FromPosition = (0, 0), HitEvent = null, NoHitEvent = null
SetVector3XYZ
FloatCompare
ActivateGameObject
transition FINISHED -> Throw Needle
```

### Throw Needle
```
tk2dPlayAnimAfterPreviousComplete
PlayRandomAudioClipTable, PlayAudioEvent, PlayVibrationV2
ActivateGameObject, SetPosition, ActivateGameObject
SetVelocity2d       vector = (0, 150), x = 0, y = 0, everyFrame = False
CheckCollisionSideV2
Wait                time = 0.8
GetPosition x3, FloatAdd, FloatTestToBool, BoolFlipEveryFrame,
ActivateGameObject, FloatCompare
transition FINISHED -> Position Stick Needle Pre
transition DAMAGER HIT SPIKES -> Hit Spikes
```

### Position Stick Needle Pre  `[no values]`
```
ActivateGameObject x2, BoolTest, CallStaticMethod, BoolTest, HasComponent,
GetParent, SetTransformParent, GetPosition2D, SetPosition2D, GetPosition2D,
FloatOperator, Translate, BoolTest, CheckOutOfCamera, SpawnObjectFromGlobalPool
transition CANCEL -> Catch Wait
transition TRANSITION GATE -> Hit Transition Gate
transition FINISHED -> Position Stick Needle
```

### Position Stick Needle  `[no values]`
```
GetPosition, FloatAdd, FloatTestToBool, BoolTestToObject, PlayAudioEvent,
BoolTest, Tk2dWatchAnimationEvents
transition CANCEL -> Catch Wait
transition FINISHED -> Throw Wait
```

### Throw Wait  `[no values]`
```
Tk2dPlayAnimationWithEvents, GameObjectIsVisible, ConvertBoolToFloat, Wait
transition FINISHED -> Jump Antic
```

### Jump Antic  `[no values]`
```
AudioStopV2, PlayAudioEvent, Tk2dPlayAnimationWithEvents
transition FINISHED -> Dash Start
```

### Dash Start
```
SetBoolValue, SetFloatValue, CallMethodProper, PlayAudioEvent, PlayVibrationV2,
ActivateGameObject, Tk2dPlayAnimation, CallMethodProper x2, SendEventByName,
CallMethodProper
SetVelocity2d       x = 0, y = 33, everyFrame = False
CallMethodProper, SendEventToRegister, SendEvent
transition FINISHED -> Dashing
```

### Dashing
```
SetAudioClip, AudioPlayInState, VibrationPlayerPlayV2
Wait                time = 0.2
SetVelocity2d       x = 0, y = 33, everyFrame = True
CallMethodProper
SetGravity2dScale   gravityScale = 0
CheckCollisionSide
GetVelocity2d, FloatCompare, ActivateGameObjectDelay, DoCameraShakeV4
RayCast2dV2         direction = (0, 1), space = Self, distance = 10
BoolFlipEveryFrame, SetMeshRendererEveryFrame
transition WAIT -> Cancelable
transition HIT ROOF -> Hit Roof Hard
```

### Cancelable
```
ActivateGameObject, AudioPlayInState, VibrationPlayerPlayV2, BoolTest,
ListenForJump, ListenForAttackV2
SetVelocity2d       x = 0, y = 33, everyFrame = True
ListenForSuperdash, CheckCollisionSide, GetVelocity2d, FloatCompare, DoCameraShakeV4
RayCast2dV2         direction = (0, 1), space = Self, distance = 10
BoolFlipEveryFrame, SetMeshRendererEveryFrame
transition HIT ROOF -> Hit Roof Hard
transition NORM CANCEL -> Fall Needle Cancel
```

### Hit Roof Hard  `[no values]`
```
CallStaticMethod, CreateNoiseV2, ActivateGameObject x2, SpawnObjectFromGlobalPool,
PlayRandomAudioClipTable, DoCameraShakeV2, Tk2dPlayAnimationWithEvents,
CallMethodProper, SendEventToRegister, PlayAudioEvent, PlayVibrationV2, SetVelocity2d
transition FINISHED -> Hit Roof
```

### Hit Roof Soft  `[no values]`
```
SendMessage x2, DoCameraShakeV2, Tk2dPlayAnimationWithEvents, CallMethodProper,
SendEventToRegister
transition FINISHED -> Hit Roof
```

### Hit Roof  `[no values]`
```
ActivateGameObject x2, CallMethodProper x4, Tk2dWatchAnimationEvents,
SendMessageV2, SetVelocity2d
transition FINISHED -> Regain Control To Idle
```

### Fall Needle Cancel  `[no values]`
```
SetBoolValue, PlayAudioEvent, GetPosition x2, FloatOperator,
ActivateGameObject x2, SetVector3XYZ, ActivateGameObject, iTweenMoveBy,
DecelerateV2, CallMethodProper, SendEventToRegister
transition FINISHED -> Air Cancel
```

### Air Cancel  `[no values]`
```
ActivateGameObject, PlayRandomAudioClipTable, ActivateGameObject, SetPosition,
CallMethodProper x2, SendEventByName, DecelerateV2, SendMessageV2,
Tk2dPlayAnimationWithEvents
transition FINISHED -> Regain Control To Idle
```

### Catch Wait  `[no values]`
```
ActivateGameObject, Tk2dPlayAnimationWithEvents, Wait
transition FINISHED -> Fall Needle
```

### Fall Needle  `[no values]`
```
GetPosition x2, FloatOperator, ActivateGameObject, SetVector3XYZ, iTweenMoveBy
transition FINISHED -> Fall Catch Needle
```

### Fall Catch Needle  `[no values]`
```
AudioStopV2, ActivateGameObject, SetPosition, CallMethodProper,
ActivateGameObject, PlayAudioEvent, Tk2dPlayAnimationWithEvents, DebugLogConsole
transition FINISHED -> Regain Control To Idle
```

### Hit Spikes  `[no values]`
```
SetBoolValue, DoCameraShakeV4, GetPosition2D x2, FloatAdd, FloatCompare,
PlayRandomAudioClipTableV3, SetPosition2D
transition FINISHED -> Position Stick Needle Pre
```

### Hit Transition Gate  `[no values]`
```
ActivateGameObject, PlayAudioEvent, BoolTest, Tk2dWatchAnimationEvents
transition FINISHED -> Throw Wait
```

### Regain Control  `[no values]`
```
SendMessage x3
transition FINISHED -> Inactive
```

### Regain Control To Idle  `[no values]`
```
CallMethodProper x2, SendMessage x3
transition FINISHED -> Reset Effects
```

### Reset Effects  `[no values]`
```
CallMethodProper, SetBoolValue, ActivateGameObject x7, GameObjectCompare,
SetTransformParent, SendEventToRegister
transition FINISHED -> Inactive
```

### Cancel  `[no values]`
```
RemoveUsingSilk, ActivateGameObject x3, AudioStopV2 x2, CallMethodProper x3,
ActivateGameObject, BoolTest, CancelFlashByID
transition FINISHED -> Cancel Rumbling Focus
```

### Cancel Rumbling Focus / Cancel Rumbling Focus 2  `[no values]`
```
BoolTest, SetBoolValue, SetFsmBool
transition FINISHED -> Cancel Rumbling Focus 2  /  -> Reset Effects
```

### Leaving Scene  `[no values]`
```
BoolTest
transition CANCEL -> Cancel
transition LEVEL LOADED -> Cancel
```

### Pre Entered Jumping  `[no values]`
```
SetBoolValue, CallMethodProper
transition ENTER SUPERJUMPING -> Entered Jumping
```

### Entered Jumping  `[no values]`
```
WaitForFinishedEnteringScene
transition FINISHED -> Begin Jumping
```

### Begin Jumping  `[no values]`
```
SendMessage x2, SuperJumpRaycast
transition FINISHED -> Position Stick Needle Pre 2
```

### Position Stick Needle Pre 2  `[no values]`
```
ActivateGameObject x2, BoolTest x2, CallStaticMethod, BoolTest, HasComponent,
GetParent, SetTransformParent, GetPosition2D, SetPosition2D, GetPosition2D,
FloatOperator, Translate, BoolTest
transition CANCEL -> Queue Cancel
transition TRANSITION GATE -> Hit Transition Gate 2
transition FINISHED -> Dash Start Quick
```

### Hit Transition Gate 2  `[no values]`
```
ActivateGameObject
transition FINISHED -> Dash Start Quick
```

### Dash Start Quick  `[no values]`
```
ActivateGameObject, Tk2dPlayAnimation, CallMethodProper x3, SetVelocity2d,
CallMethodProper
transition FINISHED -> Dashing
```

### Queue Cancel  `[no values]`
```
SetFloatValue, SetBoolValue
transition FINISHED -> Dash Start Quick
```

## FSM variables (initial values at dump time)

```
FsmFloat  Cancelable Time              0.2
FsmFloat  Charge Time                  0.8
FsmFloat  Check Y                      0
FsmFloat  Current Throw Needle Y       0
FsmFloat  Initial Throw Needle Y       5.37
FsmFloat  Jump Speed                   33
FsmFloat  Main Cam Pos Y               0
FsmFloat  Speed                        0
FsmFloat  Stick Needle Offset X        0
FsmFloat  Stick Needle X               0
FsmFloat  Stick Needle Y               0
FsmFloat  Throw Needle Distance        0
FsmFloat  Throw Needle Pos Y           0
FsmFloat  Throw Needle Target Y        0
FsmFloat  Throw Needle X               0
FsmFloat  Throw Wait Time              0
FsmFloat  Y Speed                      0
FsmFloat  Throw Needle Pos Y Start     0
FsmInt    Current Silk                 0
FsmInt    Silk Cost                    1
FsmInt    Sprite Flash ID              0
FsmBool   Did Find Roof                False
FsmBool   Is Distant                   False
FsmBool   Needle Visible               False
FsmBool   On Ground                    False
FsmBool   Played Throw Wait            False
FsmBool   Show Thread                  False
FsmBool   Terrain Above                False
FsmBool   Did Add Using Silk           False
FsmBool   Test                         False
FsmBool   Did Start Flash              False
FsmBool   Started Rumbling Focus       False
FsmBool   Started Rumbling Focus 2     False
FsmBool   Is Transition Gate           False
FsmBool   Queued Cancel                False
FsmBool   Did Hit Spikes               False
FsmVector2 Ray Hit Point               (0, 0)
FsmVector3 Throw Needle Move By        (0, 0, 0)
FsmGameObject Antic Effect L           Super Jump Antic Effect L
FsmGameObject Antic Effect R           Super Jump Antic Effect R
FsmGameObject Attacks Folder           Special Attacks
FsmGameObject Camera Target            Camera Target
FsmGameObject Charged Effect           Super Jump Charged
FsmGameObject Charging Fader           Super Jump Charging Fader
FsmGameObject Damager                  Super Jump Damager
FsmGameObject Effects Folder           Effects
FsmGameObject Grab Effect              Super Jump Catch Effect
FsmGameObject Self                     Hero_Hornet(Clone)
FsmGameObject Stick Needle             Super Jump Needle Stick
FsmGameObject Stick Needle Parent      null
FsmGameObject Throw Needle             Super Jump Needle Throw
FsmGameObject Throw Needle Fall        Super Jump Needle Throw Fall
FsmGameObject Throw Needle Fall Target Move To
FsmGameObject Throw Needle Target      Move To
FsmGameObject Superjump Audio Loop     Superjump Loop
FsmGameObject Roof                     null
FsmGameObject Charge Audio             null
FsmGameObject Thread                   Super Jump Thread
FsmGameObject Spawned Audio Player     null
FsmGameObject Extra Throw Effect       Super Jump Extra Throw Effect
FsmGameObject Extra Ground Effect      Super Jump Extra Ground Effect
FsmGameObject Nail Art Ready           Nail Art Ready
FsmGameObject Throw Needle Damager     Damager
FsmGameObject Thread Loop              Super Jump Thread Loop
FsmObject     Clip                     null
```
