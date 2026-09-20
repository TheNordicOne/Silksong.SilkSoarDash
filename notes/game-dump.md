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

## Main path

```
Inactive
--DO MOVE-->        Enough Silk?
--FINISHED-->       Relinquish Control
--FINISHED-->       Start Delay
--FINISHED-->       Ground Charge
--WAIT-->           Ground Charged
--BUTTON UP-->      Throw Needle Start
--FINISHED-->       Get Distance
--FINISHED-->       Throw Needle
--FINISHED-->       Position Stick Needle Pre
--FINISHED-->       Position Stick Needle
--FINISHED-->       Throw Wait
--FINISHED-->       Jump Antic
--FINISHED-->       Dash Start
--FINISHED-->       Dashing
--WAIT-->           Cancelable
--HIT ROOF-->       Hit Roof Hard
--FINISHED-->       Hit Roof
--FINISHED-->       Regain Control To Idle
--FINISHED-->       Reset Effects
--FINISHED-->       Inactive
```

`Init --FINISHED--> Inactive` runs once at startup.

## Off-path transitions

| From                        | Event                 | To                          |
|-----------------------------|-----------------------|-----------------------------|
| `Enough Silk?`              | CANCEL                | `Inactive`                  |
| `Start Delay`               | BUTTON UP             | `Regain Control`            |
| `Ground Charge`             | BUTTON UP             | `Charge Cancel Ground`      |
| `Charge Cancel Ground`      | FINISHED              | `Regain Control To Idle`    |
| `Throw Needle`              | DAMAGER HIT SPIKES    | `Hit Spikes`                |
| `Hit Spikes`                | FINISHED              | `Position Stick Needle Pre` |
| `Position Stick Needle Pre` | TRANSITION GATE       | `Hit Transition Gate`       |
| `Position Stick Needle Pre` | CANCEL                | `Catch Wait`                |
| `Position Stick Needle`     | CANCEL                | `Catch Wait`                |
| `Hit Transition Gate`       | FINISHED              | `Throw Wait`                |
| `Catch Wait`                | FINISHED              | `Fall Needle`               |
| `Fall Needle`               | FINISHED              | `Fall Catch Needle`         |
| `Fall Catch Needle`         | FINISHED              | `Regain Control To Idle`    |
| `Cancelable`                | NORM CANCEL           | `Fall Needle Cancel`        |
| `Fall Needle Cancel`        | FINISHED              | `Air Cancel`                |
| `Air Cancel`                | FINISHED              | `Regain Control To Idle`    |
| `Dashing`                   | HIT ROOF              | `Hit Roof Hard`             |
| `Hit Roof Soft`             | FINISHED              | `Hit Roof`                  |
| `Regain Control`            | FINISHED              | `Inactive`                  |
| `Leaving Scene`             | CANCEL / LEVEL LOADED | `Cancel`                    |
| `Cancel`                    | FINISHED              | `Cancel Rumbling Focus`     |
| `Cancel Rumbling Focus`     | FINISHED              | `Cancel Rumbling Focus 2`   |
| `Cancel Rumbling Focus 2`   | FINISHED              | `Reset Effects`             |

## Scene re-entry path

```
Pre Entered Jumping
--ENTER SUPERJUMPING-->   Entered Jumping
--FINISHED-->             Begin Jumping
--FINISHED-->             Position Stick Needle Pre 2
--FINISHED-->             Dash Start Quick
--FINISHED-->             Dashing
```

| From                          | Event           | To                      |
|-------------------------------|-----------------|-------------------------|
| `Position Stick Needle Pre 2` | TRANSITION GATE | `Hit Transition Gate 2` |
| `Position Stick Needle Pre 2` | CANCEL          | `Queue Cancel`          |
| `Hit Transition Gate 2`       | FINISHED        | `Dash Start Quick`      |
| `Queue Cancel`                | FINISHED        | `Dash Start Quick`      |

## Global transitions

Active from any state.

| Event                    | To                    |
|--------------------------|-----------------------|
| `HERO DAMAGED`           | `Cancel`              |
| `FSM CANCEL`             | `Cancel`              |
| `SUPERJUMP HIT ROOF`     | `Hit Roof Soft`       |
| `LEAVING SCENE`          | `Leaving Scene`       |
| `PRE ENTER SUPERJUMPING` | `Pre Entered Jumping` |

## Runtime capture

Values observed during real Silk Soars (short, long, and cancelled), captured by a
logging action injected into every state of the live `superJumpFSM`.

### Default throw distance = 9

`Move To` is a child of `Super Jump Needle Throw` at local `(0, 9, 0)`. Constant across
all runs. `Get Distance` subtracts the needle's world Y from the marker's world Y, which
always yields 9, then both raycasts overwrite it when they hit something.

One captured `Get Distance`:

```
Current Throw Needle Y = 33.83658          needle world Y
Throw Needle Target Y  = 42.83658          marker world Y
                                           difference = 9      (the default)
Throw Needle Distance  = 129.7609          what the raycast wrote instead
Ray Hit Point          = (21.60, 158.33)
Did Find Roof          = True
Throw Needle Move By   = (0, 129.76, 0)

[Throw Needle]          world=(21.15, 33.84, 0)  local=(-0.45, 5.27, 0)
[Throw Needle/Move To]  world=(21.15, 42.84, 0)  local=(0, 9, 0)  deltaWorld=(0, 9, 0)
```

### Observed distances

```
Throw Needle Distance    -11.7  |  0  |  3.945116  |  129.7609
Throw Needle Move By     (0, -11.70, 0) | (0, 0, 0) | (0, 3.95, 0) | (0, 129.76, 0)
```

`Throw Needle Distance` is shared by two different phases.

`Get Distance` writes it for the throw. Observed positive: 3.945116, 129.7609.

`Fall Needle Cancel` writes it again for the needle's return trip to Hornet, which is
where -11.7 comes from. It then lingers unchanged through `Air Cancel`,
`Cancel Rumbling Focus`, `Regain Control To Idle`, `Reset Effects` and `Inactive`, and
into the next soar's `Start Delay`, `Enough Silk?` and `Ground Charge`, until
`Get Distance` overwrites it.

So a negative value in those states is stale data from the previous cancel, not a
downward throw. The thrown needle always travels up.

### Observed state sequences

Captured from real soars. These are what the FSM actually walked, which is not always
what the static transition table implies.

**Cross-room soar, two room transitions in one soar, ending in a hit:**

```
Get Distance
Throw Needle
Throw Wait
Jump Antic
Dashing
Cancelable
Leaving Scene            <- room 1 exit
Reset Effects
Inactive
Reset Effects
Inactive
Pre Entered Jumping
Entered Jumping
Begin Jumping
Hit Transition Gate 2    <- gate ahead, see branch table below
Dash Start Quick
Dashing
Cancelable
Leaving Scene            <- room 2 exit, same chain repeats
Reset Effects
Inactive
Reset Effects
Inactive
Pre Entered Jumping
Entered Jumping
Begin Jumping
Hit Transition Gate 2
Dash Start Quick
Dashing
Cancelable
Reset Effects            <- hero damaged
Inactive
```

**Cross-room soar, two transitions, finished on the ceiling:**

```
Throw Needle Start
Get Distance
Throw Needle
Throw Wait
Jump Antic
Dashing
Cancelable
Leaving Scene                 <- room 1 exit
Reset Effects
Inactive
Reset Effects
Inactive
Pre Entered Jumping
Entered Jumping
Begin Jumping
Hit Transition Gate 2         <- another gate ahead, soar continues
Dash Start Quick
Dashing
Cancelable
Leaving Scene                 <- room 2 exit
Reset Effects
Inactive
Reset Effects
Inactive
Pre Entered Jumping
Entered Jumping
Begin Jumping
Position Stick Needle Pre 2   <- real terrain, final room
Dash Start Quick
Dashing
Cancelable
Hit Roof Hard
Hit Roof
Regain Control To Idle
Reset Effects
Inactive
```

`Begin Jumping` always goes to `Position Stick Needle Pre 2`. The branch is inside that
state, not in `Begin Jumping`:

| `Position Stick Needle Pre 2` | Goes to | Meaning |
|---|---|---|
| exits early, TRANSITION GATE | `Hit Transition Gate 2` | soar continues into the next room |
| runs to completion | `Dash Start Quick` | needle sticks, this is the last room |

Both occurred in the same soar. The earlier reading, that `Begin Jumping` itself branched,
was an artifact of a tail-only spy missing the early-exiting `Position Stick Needle Pre 2`.

**A normal soar:**

```
Enough Silk?
Relinquish Control
Start Delay
Ground Charge
Ground Charged
Throw Needle Start
Get Distance
Throw Needle
Position Stick Needle Pre
Position Stick Needle
Throw Wait
Jump Antic
Dashing
Cancelable
Hit Roof Hard
Hit Roof
Regain Control To Idle
Reset Effects
Inactive
```

**A cancelled charge:**

```
Enough Silk?
Relinquish Control
Start Delay
Ground Charge
Charge Cancel Ground
Regain Control To Idle
Reset Effects
Inactive
```

### States exiting early

PlayMaker stops calling `OnEnter` on the remaining actions in a state as soon as one of
them fires an event. Anything later in the array never runs.

This was first seen as states appearing to be skipped. They were not. A logging action
appended to the end of a state's array is invisible whenever that state exits early.

Capturing it properly needs two logging actions per state, one at index 0 and one at the
end. The first always runs. The second runs only when the state reaches the end of its
array, so its absence is the signal that an early exit happened.

**Short soar, distance 7.48:**

```
HEAD Enough Silk?          TAIL Enough Silk?
HEAD Relinquish Control    TAIL Relinquish Control
HEAD Start Delay           TAIL Start Delay
HEAD Ground Charge         TAIL Ground Charge
HEAD Ground Charged        TAIL Ground Charged
HEAD Throw Needle Start    TAIL Throw Needle Start
HEAD Get Distance          (no TAIL)     <- exited early
HEAD Throw Needle          TAIL Throw Needle
HEAD Position Stick Needle Pre / Position Stick Needle   both TAIL
HEAD Throw Wait            (no TAIL)     <- exited early
HEAD Jump Antic            TAIL Jump Antic
HEAD Dash Start            (no TAIL)     <- exited early
HEAD Dashing               TAIL Dashing
HEAD Cancelable            TAIL Cancelable
HEAD Hit Roof Hard / Hit Roof / Regain Control To Idle / Reset Effects / Inactive
```

### FloatCompare in Get Distance

`Get Distance` action order:

```
1 GetPosition
2 GetPosition
3 FloatOperator
4 RayCast2dV2
5 SuperJumpRaycast
6 SetVector3XYZ
7 FloatCompare        equal and lessThan both fire FINISHED, float2 = 12
8 ActivateGameObject  Super Jump Thread
```

When the throw distance is 12 or less, action 7 fires `FINISHED` and action 8 never runs.
So a short throw does not activate `Super Jump Thread`.

| Distance | FloatCompare | Action 8 runs | TAIL on Get Distance |
|---|---|---|---|
| 7.48 | fires | no | no |
| 22.88 | silent | yes | yes |
| 49.82 | silent | yes | yes |
| 114.45 | silent | yes | yes |

### Cancel paths

Both captured with head and tail spies. Every state logged both, so no early exits occur
anywhere in either chain.

**Cancelled during charge, before it completed:**

```
HEAD/TAIL  Enough Silk?
HEAD/TAIL  Relinquish Control
HEAD/TAIL  Start Delay
HEAD/TAIL  Ground Charge            <- completes, the cancel comes from OnUpdate
HEAD/TAIL  Charge Cancel Ground
HEAD/TAIL  Regain Control To Idle
HEAD/TAIL  Reset Effects
HEAD/TAIL  Inactive
```

`Ground Charge` logs a TAIL even though it was cancelled. Its `ListenForSuperdash` fires
`BUTTON UP` from `OnUpdate`, not `OnEnter`, so the whole array runs first.

At `Charge Cancel Ground`: `Did Add Using Silk = True`, `Current Silk = 11`.

**Cancelled mid-soar by pressing jump, distance 22.88:**

```
...
HEAD/TAIL  Dashing
HEAD/TAIL  Cancelable
HEAD/TAIL  Fall Needle Cancel
HEAD/TAIL  Air Cancel
HEAD/TAIL  Regain Control To Idle
HEAD/TAIL  Reset Effects
HEAD/TAIL  Inactive
```

Matches the documented `Cancelable --NORM CANCEL--> Fall Needle Cancel --FINISHED-->
Air Cancel`. The `Cancel` and `Cancel Rumbling Focus` states are not involved in a jump
cancel, so that path was never a contradiction.

### Early exits per state, by distance

`both` means HEAD and TAIL both logged, so the state ran its whole action array.
`HEAD` means only the head spy ran, so an action fired an event and the rest were skipped.

```
state                       7.48    22.88   49.82   114.45
Get Distance                HEAD    both    both    both
Position Stick Needle Pre   both    HEAD    HEAD    HEAD
Position Stick Needle       both    both    both    HEAD
Throw Wait                  HEAD    both    both    both
Dash Start                  HEAD    HEAD    HEAD    HEAD
```

`Get Distance` and `Throw Wait` exit early only on the shortest soar, matching the
`FloatCompare` threshold of 12.

`Dash Start` exits early on every run, so it is unconditional.

`Position Stick Needle Pre` completes only on the shortest soar. The second to last action
is `CheckOutOfCamera` on `Stick Needle` with `outsideEvent = FINISHED`. When the stick point
is off screen the state leaves there and the last action, `SpawnObjectFromGlobalPool` of
`Nail Terrain Hit Effect`, never runs. On the shortest soar the point is on camera, no event
fires, and the state runs to the end.

`Position Stick Needle` exits early only on the longest. The second to last action is
`BoolTest` on `Played Throw Wait` with `isTrue = FINISHED`. When that bool is already true
the state leaves there and the last action, `Tk2dWatchAnimationEvents`, never runs.

### Is Distant

Misleading name. It is the needle's damage box switch, and it is distance based, but
against a 30 unit radius around Hornet, not the throw distance.

`Throw Needle` runs these three every frame, in array order:

```
FloatTestToBool      Is Distant = (Throw Needle Pos Y > Check Y)
BoolFlipEveryFrame   Is Distant = !Is Distant
ActivateGameObject   Throw Needle Damager active = Is Distant
```

`Check Y` is Hornet's world Y plus 30, set earlier in the same state.

`FloatTestToBool` overwrites the variable each frame before `BoolFlipEveryFrame` inverts
it, so the flip is a fixed inversion rather than a toggle over time.

Net effect: the needle's `Damager` is active while the needle is within 30 units above
Hornet, and inactive beyond that.

Snapshot readings of this variable vary by where in the array they are taken, which is why
earlier captures looked random.

### Reading HEAD values

A `HEAD` snapshot is taken before the state's own actions run, so its values come from
whatever the previous state left behind. A state's own output is only visible in its
`TAIL` snapshot, or in the next state's `HEAD`.

For the 7.48 soar, `Get Distance` had no TAIL, so its output was read from
`Throw Needle`'s HEAD instead.

### Room transition, resolved

Recaptured with head and tail spies. The `Cancel` chain was always being taken. All three
of its states exit early, so a tail-only spy never saw them.

```
HEAD/TAIL  Cancelable
HEAD/TAIL  Leaving Scene
HEAD       Cancel                    <- exits early
HEAD       Cancel Rumbling Focus     <- exits early
HEAD       Cancel Rumbling Focus 2   <- exits early
HEAD/TAIL  Reset Effects
HEAD/TAIL  Inactive
HEAD       Cancel                    <- the whole chain repeats
HEAD       Cancel Rumbling Focus
HEAD       Cancel Rumbling Focus 2
HEAD/TAIL  Reset Effects
HEAD/TAIL  Inactive
HEAD/TAIL  Pre Entered Jumping
HEAD/TAIL  Entered Jumping
HEAD/TAIL  Begin Jumping
HEAD       Position Stick Needle Pre 2   <- gate ahead, exits early
HEAD/TAIL  Hit Transition Gate 2
HEAD/TAIL  Dash Start Quick
HEAD/TAIL  Dashing
HEAD/TAIL  Cancelable
```

The `Cancel` chain runs twice per room transition, which is why the doubled
`Reset Effects -> Inactive` pairs appear in earlier captures.

Final room, where the raycast finds terrain instead of a gate:

```
HEAD/TAIL  Begin Jumping
HEAD/TAIL  Position Stick Needle Pre 2   <- runs to completion
HEAD/TAIL  Dash Start Quick
HEAD/TAIL  Dashing
HEAD/TAIL  Cancelable
HEAD/TAIL  Hit Roof Hard
HEAD/TAIL  Hit Roof
HEAD/TAIL  Regain Control To Idle
HEAD/TAIL  Reset Effects
HEAD/TAIL  Inactive
```

### Hero damaged, resolved

Captured by taking damage during `Ground Charge`. Same `Cancel` chain as a room
transition, and it also runs twice.

```
HEAD/TAIL  Ground Charge
HEAD       Cancel                    <- exits early
HEAD/TAIL  Cancel Rumbling Focus     <- ran to completion
HEAD       Cancel Rumbling Focus 2   <- exits early
HEAD/TAIL  Reset Effects
HEAD/TAIL  Inactive
HEAD       Cancel                    <- chain repeats
HEAD       Cancel Rumbling Focus     <- exits early this time
HEAD       Cancel Rumbling Focus 2
HEAD/TAIL  Reset Effects
HEAD/TAIL  Inactive
```

`HERO DAMAGED -> Cancel` behaves as the global transition implies.

`Cancel Rumbling Focus` completed on the first pass and exited early on the second. Its
`BoolTest` on `Started Rumbling Focus` is the likely cause, the first pass having state to
tear down and the second not. Not confirmed.

### Variables that change at runtime

The other 35 variables held their initial values throughout. These 30 moved:

```
Charge Audio               Audio Player Actor 2D(Clone) (UnityEngine.GameObject) | null
Check Y                    0 | 43.56768 | 58.56768
Clip                       hornet_superjump_pt_5_needle_impact_2d (UnityEngine.AudioClip) | hornet_superjump_pt_5_needle_impact_2d_distant (UnityEngine.AudioClip) | null
Current Silk               14 | 15 | 16 | 17
Current Throw Needle Y     0 | 153.8282 | 33.83658 | 33.93768 | 72.81917
Did Add Using Silk         False | True
Did Find Roof              False | True
Did Start Flash            False | True
Is Distant                 False | True
Needle Visible             False | True
On Ground                  False | True
Played Throw Wait          False | True
Ray Hit Point              (0.00, 0.00) | (2.73, 32.51) | (21.60, 158.33)
Roof                       Plate (UnityEngine.GameObject) | Roof Collider_Basic (9) (UnityEngine.GameObject) | null
Show Thread                False | True
Sprite Flash ID            0 | 2 | 4 | 6
Started Rumbling Focus     False | True
Started Rumbling Focus 2   False | True
Stick Needle Offset X      -0.4500008 | 0
Stick Needle Parent        Special Attacks (UnityEngine.GameObject) | null
Stick Needle X             0 | 2.729032 | 21.59922
Stick Needle Y             0 | 158.3286 | 32.5128
Throw Needle Distance      -11.7 | 0 | 129.7609 | 3.945116
Throw Needle Move By       (0.00, -11.70, 0.00) | (0.00, 0.00, 0.00) | (0.00, 129.76, 0.00) | (0.00, 3.95, 0.00)
Throw Needle Pos Y         0 | 153.8282 | 153.9034 | 33.83658 | 33.93768
Throw Needle Pos Y Start   0 | 33.93768
Throw Needle Target Y      0 | 162.8282 | 42.83658 | 42.93768 | 61.11917
Throw Needle X             0 | 2.279031 | 21.14922
Throw Wait Time            0 | 0.5
Y Speed                    0 | 33
```

### Notes

`Is Distant` is a per-frame toggle for the needle's damage hitbox, not a distance flag.
See the `Is Distant` section above.

`Roof` resolved to real scene colliders: `Plate`, `Roof Collider_Basic (9)`.

`Clip` switches between `hornet_superjump_pt_5_needle_impact_2d` and its `_distant`
variant. What selects it is not identified; it is not `Is Distant`.

ENTER and EXIT snapshots were identical in every state. PlayMaker runs all `OnEnter`
calls in sequence before a state can exit, so a spy action appended to the array sees
post-computation values in both phases.

## Full state listing

Every state, every action, every action field value, every transition.

`<literal>` means an inline value rather than a named FSM variable.
`FsmOwnerDefault{...}` shows which GameObject an action targets.
`event:NAME` is an event an action fires.

### Inactive
```
    transition DO MOVE -> Enough Silk?
```

### Regain Control To Idle
```
    action CallMethodProper
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        behaviour = <literal> = HeroController
        methodName = <literal> = SetCState
        parameters = [FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue="superDashOnWall", vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = superDashOnWall, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=String, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}, FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = False, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=Bool, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}]
        storeResult = FsmVar{variableName="", objectType=null, useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4{x=0, y=0, z=0, w=0, normalized=Vector4, magnitude=0, sqrMagnitude=0}, objectReference=null, arrayValue=null, NamedVar=<literal> = 0, NamedVarType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554531, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.FsmFloat, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="FsmFloat", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.FsmFloat", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=False, IsSpecialName=False, IsClass=True, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=False, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData]}, EnumType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, EnumValue=None, ObjectType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, Type=Float, RealType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554812, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="System.Single, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", DeclaringType=null, Name="Single", Namespace="System", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="System.Single", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType, RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, SequentialLayout, Sealed, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=False, IsExplicitLayout=False, IsLayoutSequential=True, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=True, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData, CustomAttributeData]}, IsNone=True, vector2Value=Vector2{x=0, y=0, normalized=Vector2, magnitude=0, sqrMagnitude=0}, vector3Value=Vector3{x=0, y=0, z=0, normalized=Vector3, magnitude=0, sqrMagnitude=0}, colorValue=Color{r=0, g=0, b=0, a=0, grayscale=0, linear=Color, gamma=Color, maxColorComponent=0}, rectValue=Rect{x=0, y=0, position=Vector2, center=Vector2, min=Vector2, max=Vector2, width=0, height=0, size=Vector2, xMin=0, yMin=0, xMax=0, yMax=0, left=0, right=0, top=0, bottom=0}, quaternionValue=Quaternion{x=0, y=0, z=0, w=0, eulerAngles=Vector3, normalized=Quaternion}, gameObjectValue=null, materialValue=null, textureValue=null}
        EveryFrame = False
    action CallMethodProper
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        behaviour = <literal> = HeroController
        methodName = <literal> = SetCState
        parameters = [FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue="superDashing", vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = superDashing, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=String, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}, FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = False, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=Bool, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}]
        storeResult = FsmVar{variableName="", objectType=null, useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4{x=0, y=0, z=0, w=0, normalized=Vector4, magnitude=0, sqrMagnitude=0}, objectReference=null, arrayValue=null, NamedVar=<literal> = 0, NamedVarType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554531, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.FsmFloat, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="FsmFloat", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.FsmFloat", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=False, IsSpecialName=False, IsClass=True, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=False, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData]}, EnumType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, EnumValue=None, ObjectType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, Type=Float, RealType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554812, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="System.Single, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", DeclaringType=null, Name="Single", Namespace="System", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="System.Single", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType, RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, SequentialLayout, Sealed, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=False, IsExplicitLayout=False, IsLayoutSequential=True, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=True, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData, CustomAttributeData]}, IsNone=True, vector2Value=Vector2{x=0, y=0, normalized=Vector2, magnitude=0, sqrMagnitude=0}, vector3Value=Vector3{x=0, y=0, z=0, normalized=Vector3, magnitude=0, sqrMagnitude=0}, colorValue=Color{r=0, g=0, b=0, a=0, grayscale=0, linear=Color, gamma=Color, maxColorComponent=0}, rectValue=Rect{x=0, y=0, position=Vector2, center=Vector2, min=Vector2, max=Vector2, width=0, height=0, size=Vector2, xMin=0, yMin=0, xMax=0, yMax=0, left=0, right=0, top=0, bottom=0}, quaternionValue=Quaternion{x=0, y=0, z=0, w=0, eulerAngles=Vector3, normalized=Quaternion}, gameObjectValue=null, materialValue=null, textureValue=null}
        EveryFrame = False
    action SendMessage
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        delivery = SendMessage
        options = RequireReceiver
        functionCall = FunctionCall{FunctionName="RegainControl", BoolParameter=<literal> = False, FloatParameter=<literal> = 0, IntParameter=<literal> = 0, GameObjectParameter=<literal> = null, ObjectParameter=<literal> = null, StringParameter=<literal> = , Vector2Parameter=<literal> = (0.00, 0.00), Vector3Parameter=<literal> = (0.00, 0.00, 0.00), RectParamater=<literal> = (x:0.00, y:0.00, width:0.00, height:0.00), QuaternionParameter=<literal> = (0.00000, 0.00000, 0.00000, 0.00000), MaterialParameter=<literal> = null, TextureParameter=<literal> = null, ColorParameter=<literal> = RGBA(0.000, 0.000, 0.000, 1.000), EnumParameter=<literal> = None, ArrayParameter=<literal> = System.Object[], ParameterType="None"}
    action SendMessage
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        delivery = SendMessage
        options = RequireReceiver
        functionCall = FunctionCall{FunctionName="StartAnimationControlToIdle", BoolParameter=<literal> = False, FloatParameter=<literal> = 0, IntParameter=<literal> = 0, GameObjectParameter=<literal> = null, ObjectParameter=<literal> = null, StringParameter=<literal> = , Vector2Parameter=<literal> = (0.00, 0.00), Vector3Parameter=<literal> = (0.00, 0.00, 0.00), RectParamater=<literal> = (x:0.00, y:0.00, width:0.00, height:0.00), QuaternionParameter=<literal> = (0.00000, 0.00000, 0.00000, 0.00000), MaterialParameter=<literal> = null, TextureParameter=<literal> = null, ColorParameter=<literal> = RGBA(0.000, 0.000, 0.000, 1.000), EnumParameter=<literal> = None, ArrayParameter=<literal> = System.Object[], ParameterType="None"}
    action SendMessage
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        delivery = SendMessage
        options = DontRequireReceiver
        functionCall = FunctionCall{FunctionName="AffectedByGravity", BoolParameter=<literal> = True, FloatParameter=<literal> = 0, IntParameter=<literal> = 0, GameObjectParameter=<literal> = null, ObjectParameter=<literal> = null, StringParameter=<literal> = , Vector2Parameter=<literal> = (0.00, 0.00), Vector3Parameter=<literal> = (0.00, 0.00, 0.00), RectParamater=<literal> = (x:0.00, y:0.00, width:0.00, height:0.00), QuaternionParameter=<literal> = (0.00000, 0.00000, 0.00000, 0.00000), MaterialParameter=<literal> = null, TextureParameter=<literal> = null, ColorParameter=<literal> = RGBA(0.000, 0.000, 0.000, 1.000), EnumParameter=<literal> = None, ArrayParameter=<literal> = System.Object[], ParameterType="bool"}
    transition FINISHED -> Reset Effects
```

### Cancel
```
    action RemoveUsingSilk
        Amount = <literal> = 1
        UsingType = <literal> = Normal
        DidAddTracker = Did Add Using Silk = False
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Extra Ground Effect = Super Jump Extra Ground Effect (UnityEngine.GameObject)}
        activate = <literal> = False
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Extra Throw Effect = Super Jump Extra Throw Effect (UnityEngine.GameObject)}
        activate = <literal> = False
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Thread = Super Jump Thread (UnityEngine.GameObject)}
        activate = <literal> = False
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action AudioStopV2
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Charge Audio = null}
        fadeTime = <literal> = 0.4
        cancelOnEarlyExit = False
    action AudioStopV2
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Superjump Audio Loop = Superjump Loop (UnityEngine.GameObject)}
        fadeTime = <literal> = 0
        cancelOnEarlyExit = False
    action CallMethodProper
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        behaviour = <literal> = HeroController
        methodName = <literal> = SetCState
        parameters = [FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue="superDashing", vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = superDashing, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=String, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}, FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = False, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=Bool, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}]
        storeResult = FsmVar{variableName="", objectType=null, useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4{x=0, y=0, z=0, w=0, normalized=Vector4, magnitude=0, sqrMagnitude=0}, objectReference=null, arrayValue=null, NamedVar=<literal> = 0, NamedVarType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554531, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.FsmFloat, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="FsmFloat", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.FsmFloat", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=False, IsSpecialName=False, IsClass=True, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=False, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData]}, EnumType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, EnumValue=None, ObjectType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, Type=Float, RealType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554812, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="System.Single, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", DeclaringType=null, Name="Single", Namespace="System", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="System.Single", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType, RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, SequentialLayout, Sealed, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=False, IsExplicitLayout=False, IsLayoutSequential=True, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=True, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData, CustomAttributeData]}, IsNone=True, vector2Value=Vector2{x=0, y=0, normalized=Vector2, magnitude=0, sqrMagnitude=0}, vector3Value=Vector3{x=0, y=0, z=0, normalized=Vector3, magnitude=0, sqrMagnitude=0}, colorValue=Color{r=0, g=0, b=0, a=0, grayscale=0, linear=Color, gamma=Color, maxColorComponent=0}, rectValue=Rect{x=0, y=0, position=Vector2, center=Vector2, min=Vector2, max=Vector2, width=0, height=0, size=Vector2, xMin=0, yMin=0, xMax=0, yMax=0, left=0, right=0, top=0, bottom=0}, quaternionValue=Quaternion{x=0, y=0, z=0, w=0, eulerAngles=Vector3, normalized=Quaternion}, gameObjectValue=null, materialValue=null, textureValue=null}
        EveryFrame = False
    action CallMethodProper
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        behaviour = <literal> = HeroController
        methodName = <literal> = SetCState
        parameters = [FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue="freezeCharge", vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = freezeCharge, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=String, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}, FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = False, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=Bool, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}]
        storeResult = FsmVar{variableName="", objectType=null, useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4{x=0, y=0, z=0, w=0, normalized=Vector4, magnitude=0, sqrMagnitude=0}, objectReference=null, arrayValue=null, NamedVar=<literal> = 0, NamedVarType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554531, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.FsmFloat, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="FsmFloat", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.FsmFloat", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=False, IsSpecialName=False, IsClass=True, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=False, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData]}, EnumType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, EnumValue=None, ObjectType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, Type=Float, RealType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554812, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="System.Single, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", DeclaringType=null, Name="Single", Namespace="System", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="System.Single", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType, RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, SequentialLayout, Sealed, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=False, IsExplicitLayout=False, IsLayoutSequential=True, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=True, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData, CustomAttributeData]}, IsNone=True, vector2Value=Vector2{x=0, y=0, normalized=Vector2, magnitude=0, sqrMagnitude=0}, vector3Value=Vector3{x=0, y=0, z=0, normalized=Vector3, magnitude=0, sqrMagnitude=0}, colorValue=Color{r=0, g=0, b=0, a=0, grayscale=0, linear=Color, gamma=Color, maxColorComponent=0}, rectValue=Rect{x=0, y=0, position=Vector2, center=Vector2, min=Vector2, max=Vector2, width=0, height=0, size=Vector2, xMin=0, yMin=0, xMax=0, yMax=0, left=0, right=0, top=0, bottom=0}, quaternionValue=Quaternion{x=0, y=0, z=0, w=0, eulerAngles=Vector3, normalized=Quaternion}, gameObjectValue=null, materialValue=null, textureValue=null}
        EveryFrame = False
    action CallMethodProper
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Camera Target = Camera Target (UnityEngine.GameObject)}
        behaviour = <literal> = CameraTarget
        methodName = <literal> = SetSuperJump
        parameters = [FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = False, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=Bool, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}, FsmVar{variableName="Stick Needle Y", objectType="UnityEngine.Object", useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=Stick Needle Y = 0, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=Float, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}]
        storeResult = FsmVar{variableName="", objectType=null, useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4{x=0, y=0, z=0, w=0, normalized=Vector4, magnitude=0, sqrMagnitude=0}, objectReference=null, arrayValue=null, NamedVar=<literal> = False, NamedVarType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554527, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.FsmBool, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="FsmBool", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.FsmBool", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=False, IsSpecialName=False, IsClass=True, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=False, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData]}, EnumType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, EnumValue=None, ObjectType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, Type=Bool, RealType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554690, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", DeclaringType=null, Name="Boolean", Namespace="System", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="System.Boolean", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, SequentialLayout, Sealed, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=False, IsExplicitLayout=False, IsLayoutSequential=True, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=True, IsValueType=True, IsSignatureType=False, TypeInitializer=RuntimeConstructorInfo, IsInterface=False, CustomAttributes=[CustomAttributeData, CustomAttributeData]}, IsNone=True, vector2Value=Vector2{x=0, y=0, normalized=Vector2, magnitude=0, sqrMagnitude=0}, vector3Value=Vector3{x=0, y=0, z=0, normalized=Vector3, magnitude=0, sqrMagnitude=0}, colorValue=Color{r=0, g=0, b=0, a=0, grayscale=0, linear=Color, gamma=Color, maxColorComponent=0}, rectValue=Rect{x=0, y=0, position=Vector2, center=Vector2, min=Vector2, max=Vector2, width=0, height=0, size=Vector2, xMin=0, yMin=0, xMax=0, yMax=0, left=0, right=0, top=0, bottom=0}, quaternionValue=Quaternion{x=0, y=0, z=0, w=0, eulerAngles=Vector3, normalized=Quaternion}, gameObjectValue=null, materialValue=null, textureValue=null}
        EveryFrame = False
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Damager = Super Jump Damager (UnityEngine.GameObject)}
        activate = <literal> = False
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action BoolTest
        boolVariable = Did Start Flash = False
        isTrue = null
        isFalse = event:FINISHED
        everyFrame = False
    action CancelFlashByID
        Target = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        ID = Sprite Flash ID = 0
    transition FINISHED -> Cancel Rumbling Focus
```

### Relinquish Control
```
    action SendMessage
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        delivery = SendMessage
        options = DontRequireReceiver
        functionCall = FunctionCall{FunctionName="RelinquishControl", BoolParameter=<literal> = False, FloatParameter=<literal> = 0, IntParameter=<literal> = 0, GameObjectParameter=<literal> = null, ObjectParameter=<literal> = null, StringParameter=<literal> = , Vector2Parameter=<literal> = (0.00, 0.00), Vector3Parameter=<literal> = (0.00, 0.00, 0.00), RectParamater=<literal> = (x:0.00, y:0.00, width:0.00, height:0.00), QuaternionParameter=<literal> = (0.00000, 0.00000, 0.00000, 0.00000), MaterialParameter=<literal> = null, TextureParameter=<literal> = null, ColorParameter=<literal> = RGBA(0.000, 0.000, 0.000, 1.000), EnumParameter=<literal> = None, ArrayParameter=<literal> = System.Object[], ParameterType="None"}
    action SendMessage
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        delivery = SendMessage
        options = DontRequireReceiver
        functionCall = FunctionCall{FunctionName="StopAnimationControl", BoolParameter=<literal> = False, FloatParameter=<literal> = 0, IntParameter=<literal> = 0, GameObjectParameter=<literal> = null, ObjectParameter=<literal> = null, StringParameter=<literal> = , Vector2Parameter=<literal> = (0.00, 0.00), Vector3Parameter=<literal> = (0.00, 0.00, 0.00), RectParamater=<literal> = (x:0.00, y:0.00, width:0.00, height:0.00), QuaternionParameter=<literal> = (0.00000, 0.00000, 0.00000, 0.00000), MaterialParameter=<literal> = null, TextureParameter=<literal> = null, ColorParameter=<literal> = RGBA(0.000, 0.000, 0.000, 1.000), EnumParameter=<literal> = None, ArrayParameter=<literal> = System.Object[], ParameterType="None"}
    transition FINISHED -> Start Delay
```

### Ground Charge
```
    action SendMessage
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        delivery = SendMessage
        options = DontRequireReceiver
        functionCall = FunctionCall{FunctionName="StopAnimationControl", BoolParameter=<literal> = False, FloatParameter=<literal> = 0, IntParameter=<literal> = 0, GameObjectParameter=<literal> = null, ObjectParameter=<literal> = null, StringParameter=<literal> = , Vector2Parameter=<literal> = (0.00, 0.00), Vector3Parameter=<literal> = (0.00, 0.00, 0.00), RectParamater=<literal> = (x:0.00, y:0.00, width:0.00, height:0.00), QuaternionParameter=<literal> = (0.00000, 0.00000, 0.00000, 0.00000), MaterialParameter=<literal> = null, TextureParameter=<literal> = null, ColorParameter=<literal> = RGBA(0.000, 0.000, 0.000, 1.000), EnumParameter=<literal> = None, ArrayParameter=<literal> = System.Object[], ParameterType="None"}
    action DecelerateXY
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        decelerationX = <literal> = 0.9
        decelerationY = <literal> = 0
        brakeOnExit = True
    action AddUsingSilk
        Amount = <literal> = 1
        UsingType = <literal> = Normal
        DidAddTracker = Did Add Using Silk = False
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Extra Ground Effect = Super Jump Extra Ground Effect (UnityEngine.GameObject)}
        activate = <literal> = True
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action Tk2dPlayAnimation
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        animLibName = <literal> =
        clipName = <literal> = Super Jump Antic
    action PlayAudioEvent
        audioClip = <literal> = hornet_superjump_pt_1_into_position (UnityEngine.AudioClip)
        pitchMin = <literal> = 0.95
        pitchMax = <literal> = 1.05
        volume = <literal> = 1
        audioPlayerPrefab = <literal> = null
        spawnPoint = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        spawnPosition = <literal> = (0.00, 0.00, 0.00)
        SpawnedPlayerRef = <literal> = null
    action PlayAudioEvent
        audioClip = <literal> = hornet_superjump_pt_2_charge_2d (UnityEngine.AudioClip)
        pitchMin = <literal> = 1
        pitchMax = <literal> = 1
        volume = <literal> = 1
        audioPlayerPrefab = <literal> = Audio Player Actor 2D (UnityEngine.AudioSource)
        spawnPoint = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        spawnPosition = <literal> = (0.00, 0.00, 0.00)
        SpawnedPlayerRef = Charge Audio = null
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Antic Effect L = Super Jump Antic Effect L (UnityEngine.GameObject)}
        activate = <literal> = True
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action Tk2dPlayAnimation
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Antic Effect L = Super Jump Antic Effect L (UnityEngine.GameObject)}
        animLibName = <literal> =
        clipName = <literal> = Super Jump Antic Effect
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Antic Effect R = Super Jump Antic Effect R (UnityEngine.GameObject)}
        activate = <literal> = True
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action Tk2dPlayAnimation
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Antic Effect R = Super Jump Antic Effect R (UnityEngine.GameObject)}
        animLibName = <literal> =
        clipName = <literal> = Super Jump Antic Effect
    action CallMethodProper
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        behaviour = <literal> = HeroController
        methodName = <literal> = SetCState
        parameters = [FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue="freezeCharge", vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = freezeCharge, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=String, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}, FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=True, stringValue=null, vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = True, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=Bool, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}]
        storeResult = FsmVar{variableName="", objectType=null, useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4{x=0, y=0, z=0, w=0, normalized=Vector4, magnitude=0, sqrMagnitude=0}, objectReference=null, arrayValue=null, NamedVar=<literal> = 0, NamedVarType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554531, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.FsmFloat, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="FsmFloat", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.FsmFloat", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=False, IsSpecialName=False, IsClass=True, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=False, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData]}, EnumType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, EnumValue=None, ObjectType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, Type=Float, RealType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554812, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="System.Single, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", DeclaringType=null, Name="Single", Namespace="System", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="System.Single", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType, RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, SequentialLayout, Sealed, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=False, IsExplicitLayout=False, IsLayoutSequential=True, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=True, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData, CustomAttributeData]}, IsNone=True, vector2Value=Vector2{x=0, y=0, normalized=Vector2, magnitude=0, sqrMagnitude=0}, vector3Value=Vector3{x=0, y=0, z=0, normalized=Vector3, magnitude=0, sqrMagnitude=0}, colorValue=Color{r=0, g=0, b=0, a=0, grayscale=0, linear=Color, gamma=Color, maxColorComponent=0}, rectValue=Rect{x=0, y=0, position=Vector2, center=Vector2, min=Vector2, max=Vector2, width=0, height=0, size=Vector2, xMin=0, yMin=0, xMax=0, yMax=0, left=0, right=0, top=0, bottom=0}, quaternionValue=Quaternion{x=0, y=0, z=0, w=0, eulerAngles=Vector3, normalized=Quaternion}, gameObjectValue=null, materialValue=null, textureValue=null}
        EveryFrame = False
    action SetBoolValue
        boolVariable = Started Rumbling Focus = False
        boolValue = <literal> = True
        everyFrame = False
    action SetFsmBool
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=CameraParent = CameraParent (UnityEngine.GameObject)}
        fsmName = <literal> = CameraShake
        variableName = <literal> = RumblingFocus
        setValue = <literal> = True
        everyFrame = False
    action SendEventByName
        eventTarget = FsmEventTarget{target=GameObject, excludeSelf=<literal> = False, gameObject=FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=CameraParent = CameraParent (UnityEngine.GameObject)}, fsmName=<literal> = , sendToChildren=<literal> = False, fsmComponent=null}
        sendEvent = <literal> = FocusRumble
        delay = <literal> = 0
        everyFrame = False
    action ListenForSuperdash
        eventTarget = FsmEventTarget{target=Self, excludeSelf=<literal> = False, gameObject=FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}, fsmName=<literal> = , sendToChildren=<literal> = False, fsmComponent=null}
        wasPressed = null
        wasReleased = event:BUTTON UP
        isPressed = null
        isNotPressed = null
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Charging Fader = Super Jump Charging Fader (UnityEngine.GameObject)}
        activate = <literal> = True
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action FadeNestedFadeGroup
        Target = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Charging Fader = Super Jump Charging Fader (UnityEngine.GameObject)}
        ToAlpha = <literal> = 0
        FadeTime = <literal> = 0
    action FadeNestedFadeGroupV2
        Target = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Charging Fader = Super Jump Charging Fader (UnityEngine.GameObject)}
        ToAlpha = <literal> = 1
        FadeTime = Charge Time = 0.8
        Curve = FsmAnimationCurve{curve=AnimationCurve{keys=[Keyframe, Keyframe], length=2, preWrapMode=ClampForever, postWrapMode=ClampForever}}
    action Wait
        time = Charge Time = 0.8
        finishEvent = event:WAIT
        realTime = False
    action GetVelocity2d
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        vector = <literal> = (0.00, 0.00)
        x = <literal> = 0
        y = Y Speed = 0
        space = World
        everyFrame = True
    action FloatCompare
        float1 = Y Speed = 0
        float2 = <literal> = -0.1
        tolerance = <literal> = 0
        equal = null
        lessThan = event:BUTTON UP
        greaterThan = null
        everyFrame = True
    transition BUTTON UP -> Charge Cancel Ground
    transition WAIT -> Ground Charged
```

### Charge Cancel Ground
```
    action RemoveUsingSilk
        Amount = <literal> = 1
        UsingType = <literal> = Normal
        DidAddTracker = Did Add Using Silk = False
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Extra Ground Effect = Super Jump Extra Ground Effect (UnityEngine.GameObject)}
        activate = <literal> = False
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action AudioStopV2
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Charge Audio = null}
        fadeTime = <literal> = 0.4
        cancelOnEarlyExit = False
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Antic Effect L = Super Jump Antic Effect L (UnityEngine.GameObject)}
        activate = <literal> = False
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Antic Effect R = Super Jump Antic Effect R (UnityEngine.GameObject)}
        activate = <literal> = False
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action FadeNestedFadeGroup
        Target = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Charging Fader = Super Jump Charging Fader (UnityEngine.GameObject)}
        ToAlpha = <literal> = 0
        FadeTime = <literal> = 0.1
    action CallMethodProper
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        behaviour = <literal> = HeroController
        methodName = <literal> = SetCState
        parameters = [FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue="freezeCharge", vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = freezeCharge, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=String, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}, FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = False, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=Bool, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}]
        storeResult = FsmVar{variableName="", objectType=null, useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4{x=0, y=0, z=0, w=0, normalized=Vector4, magnitude=0, sqrMagnitude=0}, objectReference=null, arrayValue=null, NamedVar=<literal> = 0, NamedVarType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554531, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.FsmFloat, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="FsmFloat", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.FsmFloat", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=False, IsSpecialName=False, IsClass=True, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=False, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData]}, EnumType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, EnumValue=None, ObjectType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, Type=Float, RealType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554812, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="System.Single, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", DeclaringType=null, Name="Single", Namespace="System", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="System.Single", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType, RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, SequentialLayout, Sealed, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=False, IsExplicitLayout=False, IsLayoutSequential=True, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=True, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData, CustomAttributeData]}, IsNone=True, vector2Value=Vector2{x=0, y=0, normalized=Vector2, magnitude=0, sqrMagnitude=0}, vector3Value=Vector3{x=0, y=0, z=0, normalized=Vector3, magnitude=0, sqrMagnitude=0}, colorValue=Color{r=0, g=0, b=0, a=0, grayscale=0, linear=Color, gamma=Color, maxColorComponent=0}, rectValue=Rect{x=0, y=0, position=Vector2, center=Vector2, min=Vector2, max=Vector2, width=0, height=0, size=Vector2, xMin=0, yMin=0, xMax=0, yMax=0, left=0, right=0, top=0, bottom=0}, quaternionValue=Quaternion{x=0, y=0, z=0, w=0, eulerAngles=Vector3, normalized=Quaternion}, gameObjectValue=null, materialValue=null, textureValue=null}
        EveryFrame = False
    action Tk2dPlayAnimationWithEvents
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        clipName = <literal> = Super Jump Antic Cancel
        animationTriggerEvent = null
        animationCompleteEvent = event:FINISHED
    action Tk2dPlayAnimation
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Antic Effect L = Super Jump Antic Effect L (UnityEngine.GameObject)}
        animLibName = <literal> =
        clipName = <literal> = Super Jump Antic Effect End
    action Tk2dPlayAnimation
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Antic Effect R = Super Jump Antic Effect R (UnityEngine.GameObject)}
        animLibName = <literal> =
        clipName = <literal> = Super Jump Antic Effect End
    action SetFsmBool
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=CameraParent = CameraParent (UnityEngine.GameObject)}
        fsmName = <literal> = CameraShake
        variableName = <literal> = RumblingFocus
        setValue = <literal> = False
        everyFrame = False
    action SetFsmBool
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=CameraParent = CameraParent (UnityEngine.GameObject)}
        fsmName = <literal> = CameraShake
        variableName = <literal> = RumblingFocus2
        setValue = <literal> = False
        everyFrame = False
    action SetVelocity2d
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        vector = <literal> = (0.00, 0.00)
        x = <literal> = 0
        y = <literal> = 0
        everyFrame = False
    transition FINISHED -> Regain Control To Idle
```

### Ground Charged
```
    action RemoveUsingSilk
        Amount = <literal> = 1
        UsingType = <literal> = Normal
        DidAddTracker = Did Add Using Silk = False
    action TakeSilkV2
        Target = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        Amount = <literal> = 1
        TakeSource = <literal> = Normal
    action SpawnObjectFromGlobalPoolV2
        gameObject = <literal> = Hornet_Super_Jump_Ready_Burst (UnityEngine.GameObject)
        spawnPoint = Self = Hero_Hornet(Clone) (UnityEngine.GameObject)
        position = <literal> = (0.00, -1.50, 0.00)
        rotation = <literal> = (0.00, 0.00, 0.00)
        amount = <literal> = 1
        storeObject = <literal> = null
        setParent = <literal> = null
    action AudioStopV2
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Charge Audio = null}
        fadeTime = <literal> = 0.4
        cancelOnEarlyExit = False
    action PlayAudioEvent
        audioClip = <literal> = hornet_superjump_pt_3_charge_ready (UnityEngine.AudioClip)
        pitchMin = <literal> = 0.95
        pitchMax = <literal> = 1.05
        volume = <literal> = 1
        audioPlayerPrefab = <literal> = null
        spawnPoint = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        spawnPosition = <literal> = (0.00, 0.00, 0.00)
        SpawnedPlayerRef = <literal> = null
    action AudioPlayInState
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Nail Art Ready = Nail Art Ready (UnityEngine.GameObject)}
        volume = <literal> = 1
    action SetAudioClip
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Superjump Audio Loop = Superjump Loop (UnityEngine.GameObject)}
        audioClip = <literal> = hornet_dramatic_stance_crazy_cloak_loop (UnityEngine.AudioClip)
        autoPlay = <literal> = True
        stopOnExit = <literal> = False
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Charged Effect = Super Jump Charged (UnityEngine.GameObject)}
        activate = <literal> = True
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action DoSpriteFlashNamed
        Target = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        Flash = <literal> = FlashingSuperDash
        CancelOnExit = <literal> = False
        FlashID = Sprite Flash ID = 0
        WaitForFlash = <literal> = False
        FinishedFlashing = null
    action SetBoolValue
        boolVariable = Did Start Flash = False
        boolValue = <literal> = True
        everyFrame = False
    action SetFsmBool
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=CameraParent = CameraParent (UnityEngine.GameObject)}
        fsmName = <literal> = CameraShake
        variableName = <literal> = RumblingFocus
        setValue = <literal> = False
        everyFrame = False
    action SetBoolValue
        boolVariable = Started Rumbling Focus = False
        boolValue = <literal> = False
        everyFrame = False
    action SetBoolValue
        boolVariable = Started Rumbling Focus 2 = False
        boolValue = <literal> = True
        everyFrame = False
    action SetFsmBool
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=CameraParent = CameraParent (UnityEngine.GameObject)}
        fsmName = <literal> = CameraShake
        variableName = <literal> = RumblingFocus2
        setValue = <literal> = True
        everyFrame = False
    action SendEventByName
        eventTarget = FsmEventTarget{target=GameObject, excludeSelf=<literal> = False, gameObject=FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=CameraParent = CameraParent (UnityEngine.GameObject)}, fsmName=<literal> = , sendToChildren=<literal> = False, fsmComponent=null}
        sendEvent = <literal> = AverageShake
        delay = <literal> = 0
        everyFrame = False
    action ListenForSuperdash
        eventTarget = FsmEventTarget{target=Self, excludeSelf=<literal> = False, gameObject=FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}, fsmName=<literal> = , sendToChildren=<literal> = False, fsmComponent=null}
        wasPressed = null
        wasReleased = event:BUTTON UP
        isPressed = null
        isNotPressed = event:BUTTON UP
    action SetBoolValue
        boolVariable = On Ground = False
        boolValue = <literal> = True
        everyFrame = False
    action RayCast2d
        fromGameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        fromPosition = <literal> = (0.00, 0.00)
        direction = <literal> = (0.00, -1.00)
        space = Self
        distance = <literal> = 2
        minDepth = <literal> = 0
        maxDepth = <literal> = 0
        hitEvent = null
        storeDidHit = On Ground = False
        storeHitObject = <literal> = null
        storeHitPoint = <literal> = (0.00, 0.00)
        storeHitNormal = <literal> = (0.00, 0.00)
        storeHitDistance = <literal> = 0
        storeHitFraction = <literal> = 0
        repeatInterval = <literal> = 2
        layerMask = [<literal> = 8]
        invertMask = <literal> = False
        debugColor = <literal> = RGBA(1.000, 0.922, 0.016, 1.000)
        debug = <literal> = False
    action BoolTest
        boolVariable = On Ground = False
        isTrue = null
        isFalse = event:BUTTON UP
        everyFrame = True
    transition BUTTON UP -> Throw Needle Start
```

### Dash Start
```
    action SetBoolValue
        boolVariable = Queued Cancel = False
        boolValue = <literal> = False
        everyFrame = False
    action SetFloatValue
        floatVariable = Cancelable Time = 0.2
        floatValue = <literal> = 0.2
        everyFrame = False
    action CallMethodProper
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        behaviour = <literal> = HeroController
        methodName = <literal> = DoHardLandingEffectNoHit
        parameters = []
        storeResult = FsmVar{variableName="", objectType=null, useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4{x=0, y=0, z=0, w=0, normalized=Vector4, magnitude=0, sqrMagnitude=0}, objectReference=null, arrayValue=null, NamedVar=null, NamedVarType=null, EnumType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, EnumValue=None, ObjectType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, Type=Unknown, RealType=null, IsNone=True, vector2Value=Vector2{x=0, y=0, normalized=Vector2, magnitude=0, sqrMagnitude=0}, vector3Value=Vector3{x=0, y=0, z=0, normalized=Vector3, magnitude=0, sqrMagnitude=0}, colorValue=Color{r=0, g=0, b=0, a=0, grayscale=0, linear=Color, gamma=Color, maxColorComponent=0}, rectValue=Rect{x=0, y=0, position=Vector2, center=Vector2, min=Vector2, max=Vector2, width=0, height=0, size=Vector2, xMin=0, yMin=0, xMax=0, yMax=0, left=0, right=0, top=0, bottom=0}, quaternionValue=Quaternion{x=0, y=0, z=0, w=0, eulerAngles=Vector3, normalized=Quaternion}, gameObjectValue=null, materialValue=null, textureValue=null}
        EveryFrame = False
    action PlayAudioEvent
        audioClip = <literal> = hornet_superjump_pt_7_hornet_jump_big_2d (UnityEngine.AudioClip)
        pitchMin = <literal> = 0.95
        pitchMax = <literal> = 1.05
        volume = <literal> = 1
        audioPlayerPrefab = <literal> = Audio Player Actor 2D (UnityEngine.AudioSource)
        spawnPoint = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        spawnPosition = <literal> = (0.00, 0.00, 0.00)
        SpawnedPlayerRef = <literal> = null
    action PlayVibrationV2
        vibrationData = FSMVibrationData{lowFidelityVibration=<literal> = None, highFidelityVibration=<literal> = null, gamepadVibration=<literal> = null, ps5Vibration=<literal> = null, VibrationData=VibrationData{LowFidelityVibration=None, HighFidelityVibration=null, GamepadVibration=null, Strength=1, PS5VibrationAsset=null}}
        vibrationDataAsset = <literal> = super_jump_dash_burst (VibrationDataAsset)
        motors = <literal> = None
        loopTime = <literal> = 0
        loopAuto = <literal> = False
        tag = <literal> =
        stopOnStateExit = <literal> = False
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Damager = Super Jump Damager (UnityEngine.GameObject)}
        activate = <literal> = True
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action Tk2dPlayAnimation
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        animLibName = <literal> =
        clipName = <literal> = Super Jump Loop
    action CallMethodProper
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        behaviour = <literal> = HeroController
        methodName = <literal> = SetCState
        parameters = [FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue="freezeCharge", vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = freezeCharge, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=String, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}, FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = False, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=Bool, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}]
        storeResult = FsmVar{variableName="", objectType=null, useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4{x=0, y=0, z=0, w=0, normalized=Vector4, magnitude=0, sqrMagnitude=0}, objectReference=null, arrayValue=null, NamedVar=<literal> = 0, NamedVarType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554531, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.FsmFloat, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="FsmFloat", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.FsmFloat", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=False, IsSpecialName=False, IsClass=True, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=False, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData]}, EnumType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, EnumValue=None, ObjectType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, Type=Float, RealType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554812, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="System.Single, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", DeclaringType=null, Name="Single", Namespace="System", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="System.Single", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType, RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, SequentialLayout, Sealed, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=False, IsExplicitLayout=False, IsLayoutSequential=True, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=True, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData, CustomAttributeData]}, IsNone=True, vector2Value=Vector2{x=0, y=0, normalized=Vector2, magnitude=0, sqrMagnitude=0}, vector3Value=Vector3{x=0, y=0, z=0, normalized=Vector3, magnitude=0, sqrMagnitude=0}, colorValue=Color{r=0, g=0, b=0, a=0, grayscale=0, linear=Color, gamma=Color, maxColorComponent=0}, rectValue=Rect{x=0, y=0, position=Vector2, center=Vector2, min=Vector2, max=Vector2, width=0, height=0, size=Vector2, xMin=0, yMin=0, xMax=0, yMax=0, left=0, right=0, top=0, bottom=0}, quaternionValue=Quaternion{x=0, y=0, z=0, w=0, eulerAngles=Vector3, normalized=Quaternion}, gameObjectValue=null, materialValue=null, textureValue=null}
        EveryFrame = False
    action CallMethodProper
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        behaviour = <literal> = HeroController
        methodName = <literal> = SetCState
        parameters = [FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue="superDashing", vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = superDashing, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=String, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}, FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=True, stringValue=null, vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = True, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=Bool, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}]
        storeResult = FsmVar{variableName="", objectType=null, useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4{x=0, y=0, z=0, w=0, normalized=Vector4, magnitude=0, sqrMagnitude=0}, objectReference=null, arrayValue=null, NamedVar=<literal> = 0, NamedVarType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554531, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.FsmFloat, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="FsmFloat", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.FsmFloat", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=False, IsSpecialName=False, IsClass=True, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=False, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData]}, EnumType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, EnumValue=None, ObjectType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, Type=Float, RealType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554812, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="System.Single, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", DeclaringType=null, Name="Single", Namespace="System", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="System.Single", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType, RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, SequentialLayout, Sealed, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=False, IsExplicitLayout=False, IsLayoutSequential=True, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=True, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData, CustomAttributeData]}, IsNone=True, vector2Value=Vector2{x=0, y=0, normalized=Vector2, magnitude=0, sqrMagnitude=0}, vector3Value=Vector3{x=0, y=0, z=0, normalized=Vector3, magnitude=0, sqrMagnitude=0}, colorValue=Color{r=0, g=0, b=0, a=0, grayscale=0, linear=Color, gamma=Color, maxColorComponent=0}, rectValue=Rect{x=0, y=0, position=Vector2, center=Vector2, min=Vector2, max=Vector2, width=0, height=0, size=Vector2, xMin=0, yMin=0, xMax=0, yMax=0, left=0, right=0, top=0, bottom=0}, quaternionValue=Quaternion{x=0, y=0, z=0, w=0, eulerAngles=Vector3, normalized=Quaternion}, gameObjectValue=null, materialValue=null, textureValue=null}
        EveryFrame = False
    action SendEventByName
        eventTarget = FsmEventTarget{target=GameObject, excludeSelf=<literal> = False, gameObject=FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=CameraParent = CameraParent (UnityEngine.GameObject)}, fsmName=<literal> = , sendToChildren=<literal> = False, fsmComponent=null}
        sendEvent = <literal> = SuperDashShake
        delay = <literal> = 0
        everyFrame = False
    action CallMethodProper
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Self = Hero_Hornet(Clone) (UnityEngine.GameObject)}
        behaviour = <literal> = HeroController
        methodName = <literal> = AffectedByGravity
        parameters = [FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = False, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=Bool, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}]
        storeResult = FsmVar{variableName="", objectType=null, useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4{x=0, y=0, z=0, w=0, normalized=Vector4, magnitude=0, sqrMagnitude=0}, objectReference=null, arrayValue=null, NamedVar=<literal> = 0, NamedVarType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554531, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.FsmFloat, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="FsmFloat", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.FsmFloat", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=False, IsSpecialName=False, IsClass=True, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=False, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData]}, EnumType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, EnumValue=None, ObjectType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, Type=Float, RealType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554812, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="System.Single, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", DeclaringType=null, Name="Single", Namespace="System", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="System.Single", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType, RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, SequentialLayout, Sealed, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=False, IsExplicitLayout=False, IsLayoutSequential=True, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=True, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData, CustomAttributeData]}, IsNone=True, vector2Value=Vector2{x=0, y=0, normalized=Vector2, magnitude=0, sqrMagnitude=0}, vector3Value=Vector3{x=0, y=0, z=0, normalized=Vector3, magnitude=0, sqrMagnitude=0}, colorValue=Color{r=0, g=0, b=0, a=0, grayscale=0, linear=Color, gamma=Color, maxColorComponent=0}, rectValue=Rect{x=0, y=0, position=Vector2, center=Vector2, min=Vector2, max=Vector2, width=0, height=0, size=Vector2, xMin=0, yMin=0, xMax=0, yMax=0, left=0, right=0, top=0, bottom=0}, quaternionValue=Quaternion{x=0, y=0, z=0, w=0, eulerAngles=Vector3, normalized=Quaternion}, gameObjectValue=null, materialValue=null, textureValue=null}
        EveryFrame = False
    action SetVelocity2d
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        vector = <literal> = (0.00, 0.00)
        x = <literal> = 0
        y = Jump Speed = 33
        everyFrame = False
    action CallMethodProper
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Camera Target = Camera Target (UnityEngine.GameObject)}
        behaviour = <literal> = CameraTarget
        methodName = <literal> = SetSuperJump
        parameters = [FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=True, stringValue=null, vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = True, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=Bool, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}, FsmVar{variableName="Stick Needle Y", objectType="UnityEngine.Object", useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=Stick Needle Y = 0, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=Float, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}]
        storeResult = FsmVar{variableName="", objectType=null, useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4{x=0, y=0, z=0, w=0, normalized=Vector4, magnitude=0, sqrMagnitude=0}, objectReference=null, arrayValue=null, NamedVar=<literal> = False, NamedVarType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554527, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.FsmBool, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="FsmBool", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.FsmBool", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=False, IsSpecialName=False, IsClass=True, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=False, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData]}, EnumType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, EnumValue=None, ObjectType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, Type=Bool, RealType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554690, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", DeclaringType=null, Name="Boolean", Namespace="System", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="System.Boolean", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, SequentialLayout, Sealed, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=False, IsExplicitLayout=False, IsLayoutSequential=True, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=True, IsValueType=True, IsSignatureType=False, TypeInitializer=RuntimeConstructorInfo, IsInterface=False, CustomAttributes=[CustomAttributeData, CustomAttributeData]}, IsNone=True, vector2Value=Vector2{x=0, y=0, normalized=Vector2, magnitude=0, sqrMagnitude=0}, vector3Value=Vector3{x=0, y=0, z=0, normalized=Vector3, magnitude=0, sqrMagnitude=0}, colorValue=Color{r=0, g=0, b=0, a=0, grayscale=0, linear=Color, gamma=Color, maxColorComponent=0}, rectValue=Rect{x=0, y=0, position=Vector2, center=Vector2, min=Vector2, max=Vector2, width=0, height=0, size=Vector2, xMin=0, yMin=0, xMax=0, yMax=0, left=0, right=0, top=0, bottom=0}, quaternionValue=Quaternion{x=0, y=0, z=0, w=0, eulerAngles=Vector3, normalized=Quaternion}, gameObjectValue=null, materialValue=null, textureValue=null}
        EveryFrame = False
    action SendEventToRegister
        eventName = <literal> = SUPER JUMP LAUNCH
    action SendEvent
        eventTarget = FsmEventTarget{target=Self, excludeSelf=<literal> = False, gameObject=FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}, fsmName=<literal> = , sendToChildren=<literal> = False, fsmComponent=null}
        sendEvent = event:FINISHED
        delay = <literal> = 0
        everyFrame = False
    transition FINISHED -> Dashing
```

### Dashing
```
    action SetAudioClip
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Superjump Audio Loop = Superjump Loop (UnityEngine.GameObject)}
        audioClip = <literal> = hornet_flying_through_air_fast_loop (UnityEngine.AudioClip)
        autoPlay = <literal> = False
        stopOnExit = <literal> = False
    action AudioPlayInState
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Superjump Audio Loop = Superjump Loop (UnityEngine.GameObject)}
        volume = <literal> = 1
    action VibrationPlayerPlayV2
        target = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Superjump Audio Loop = Superjump Loop (UnityEngine.GameObject)}
        stopOnStateExit = <literal> = True
    action Wait
        time = Cancelable Time = 0.2
        finishEvent = event:WAIT
        realTime = False
    action SetVelocity2d
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        vector = <literal> = (0.00, 0.00)
        x = <literal> = 0
        y = Jump Speed = 33
        everyFrame = True
    action CallMethodProper
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Self = Hero_Hornet(Clone) (UnityEngine.GameObject)}
        behaviour = <literal> = HeroController
        methodName = <literal> = AffectedByGravity
        parameters = [FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = False, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=Bool, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}]
        storeResult = FsmVar{variableName="", objectType=null, useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4{x=0, y=0, z=0, w=0, normalized=Vector4, magnitude=0, sqrMagnitude=0}, objectReference=null, arrayValue=null, NamedVar=<literal> = 0, NamedVarType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554531, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.FsmFloat, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="FsmFloat", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.FsmFloat", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=False, IsSpecialName=False, IsClass=True, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=False, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData]}, EnumType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, EnumValue=None, ObjectType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, Type=Float, RealType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554812, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="System.Single, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", DeclaringType=null, Name="Single", Namespace="System", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="System.Single", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType, RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, SequentialLayout, Sealed, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=False, IsExplicitLayout=False, IsLayoutSequential=True, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=True, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData, CustomAttributeData]}, IsNone=True, vector2Value=Vector2{x=0, y=0, normalized=Vector2, magnitude=0, sqrMagnitude=0}, vector3Value=Vector3{x=0, y=0, z=0, normalized=Vector3, magnitude=0, sqrMagnitude=0}, colorValue=Color{r=0, g=0, b=0, a=0, grayscale=0, linear=Color, gamma=Color, maxColorComponent=0}, rectValue=Rect{x=0, y=0, position=Vector2, center=Vector2, min=Vector2, max=Vector2, width=0, height=0, size=Vector2, xMin=0, yMin=0, xMax=0, yMax=0, left=0, right=0, top=0, bottom=0}, quaternionValue=Quaternion{x=0, y=0, z=0, w=0, eulerAngles=Vector3, normalized=Quaternion}, gameObjectValue=null, materialValue=null, textureValue=null}
        EveryFrame = False
    action SetGravity2dScale
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Self = Hero_Hornet(Clone) (UnityEngine.GameObject)}
        gravityScale = <literal> = 0
    action CheckCollisionSide
        collidingObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        topHit = <literal> = False
        rightHit = <literal> = False
        bottomHit = <literal> = False
        leftHit = <literal> = False
        topHitEvent = event:HIT ROOF
        rightHitEvent = null
        bottomHitEvent = null
        leftHitEvent = null
        otherLayer = False
        otherLayerNumber = 0
        ignoreTriggers = <literal> = True
    action GetVelocity2d
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        vector = <literal> = (0.00, 0.00)
        x = <literal> = 0
        y = Y Speed = 0
        space = World
        everyFrame = True
    action FloatCompare
        float1 = Y Speed = 0
        float2 = <literal> = 0.1
        tolerance = <literal> = 0
        equal = event:HIT ROOF
        lessThan = event:HIT ROOF
        greaterThan = null
        everyFrame = True
    action ActivateGameObjectDelay
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Thread Loop = Super Jump Thread Loop (UnityEngine.GameObject)}
        activate = <literal> = True
        resetOnExit = True
        delay = <literal> = 0.1
    action DoCameraShakeV4
        Target = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        MaxCameraDistance = <literal> = (0.00, 0.00)
        Camera = <literal> = Main Camera (CameraManagerReference)
        Profile = <literal> = Tiny Rumble (CameraShakeProfile)
        DoFreeze = <literal> = False
        Delay = <literal> = 0
        CancelOnExit = True
        vibrate = <literal> = True
    action RayCast2dV2
        fromGameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        fromPosition = <literal> = (0.00, 0.00)
        direction = <literal> = (0.00, 1.00)
        space = Self
        distance = <literal> = 10
        minDepth = <literal> = 0
        maxDepth = <literal> = 0
        hitEvent = null
        noHitEvent = null
        storeDidHit = Show Thread = False
        storeHitObject = <literal> = null
        storeHitPoint = <literal> = (0.00, 0.00)
        storeHitNormal = <literal> = (0.00, 0.00)
        storeHitDistance = <literal> = 0
        storeDistance = <literal> = 0
        repeatInterval = <literal> = 1
        layerMask = [<literal> = 8]
        invertMask = <literal> = False
        ignoreTriggers = <literal> = False
        debugColor = <literal> = RGBA(1.000, 0.922, 0.016, 1.000)
        debug = <literal> = False
    action BoolFlipEveryFrame
        boolVariable = Show Thread = False
        everyFrame = True
    action SetMeshRendererEveryFrame
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Thread Loop = Super Jump Thread Loop (UnityEngine.GameObject)}
        active = Show Thread = False
    transition WAIT -> Cancelable
    transition HIT ROOF -> Hit Roof Hard
```

### Cancelable
```
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Thread Loop = Super Jump Thread Loop (UnityEngine.GameObject)}
        activate = <literal> = True
        recursive = <literal> = False
        resetOnExit = True
        everyFrame = False
    action AudioPlayInState
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Superjump Audio Loop = Superjump Loop (UnityEngine.GameObject)}
        volume = <literal> = 1
    action VibrationPlayerPlayV2
        target = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Superjump Audio Loop = Superjump Loop (UnityEngine.GameObject)}
        stopOnStateExit = <literal> = True
    action BoolTest
        boolVariable = Queued Cancel = False
        isTrue = event:NORM CANCEL
        isFalse = null
        everyFrame = False
    action ListenForJump
        eventTarget = FsmEventTarget{target=Self, excludeSelf=<literal> = False, gameObject=FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}, fsmName=<literal> = , sendToChildren=<literal> = False, fsmComponent=null}
        wasPressed = event:NORM CANCEL
        wasReleased = null
        isPressed = null
        isNotPressed = null
        queueBool = <literal> = False
        activeBool = <literal> = False
        stateEntryOnly = False
    action ListenForAttackV2
        EventTarget = FsmEventTarget{target=Self, excludeSelf=<literal> = False, gameObject=FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}, fsmName=<literal> = , sendToChildren=<literal> = False, fsmComponent=null}
        WasPressed = event:NORM CANCEL
        WasReleased = null
        IsPressed = null
        IsNotPressed = null
        queueBool = <literal> = False
        DelayBeforeActive = <literal> = 0
        IsActive = <literal> = True
    action SetVelocity2d
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        vector = <literal> = (0.00, 0.00)
        x = <literal> = 0
        y = Jump Speed = 33
        everyFrame = True
    action ListenForSuperdash
        eventTarget = FsmEventTarget{target=Self, excludeSelf=<literal> = False, gameObject=FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}, fsmName=<literal> = , sendToChildren=<literal> = False, fsmComponent=null}
        wasPressed = event:NORM CANCEL
        wasReleased = null
        isPressed = null
        isNotPressed = null
    action CheckCollisionSide
        collidingObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        topHit = <literal> = False
        rightHit = <literal> = False
        bottomHit = <literal> = False
        leftHit = <literal> = False
        topHitEvent = event:HIT ROOF
        rightHitEvent = null
        bottomHitEvent = null
        leftHitEvent = null
        otherLayer = False
        otherLayerNumber = 0
        ignoreTriggers = <literal> = True
    action GetVelocity2d
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        vector = <literal> = (0.00, 0.00)
        x = <literal> = 0
        y = Y Speed = 0
        space = World
        everyFrame = True
    action FloatCompare
        float1 = Y Speed = 0
        float2 = <literal> = 0.1
        tolerance = <literal> = 0
        equal = event:HIT ROOF
        lessThan = event:HIT ROOF
        greaterThan = null
        everyFrame = True
    action DoCameraShakeV4
        Target = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        MaxCameraDistance = <literal> = (0.00, 0.00)
        Camera = <literal> = Main Camera (CameraManagerReference)
        Profile = <literal> = Tiny Rumble (CameraShakeProfile)
        DoFreeze = <literal> = False
        Delay = <literal> = 0
        CancelOnExit = True
        vibrate = <literal> = True
    action RayCast2dV2
        fromGameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        fromPosition = <literal> = (0.00, 0.00)
        direction = <literal> = (0.00, 1.00)
        space = Self
        distance = <literal> = 10
        minDepth = <literal> = 0
        maxDepth = <literal> = 0
        hitEvent = null
        noHitEvent = null
        storeDidHit = Show Thread = False
        storeHitObject = <literal> = null
        storeHitPoint = <literal> = (0.00, 0.00)
        storeHitNormal = <literal> = (0.00, 0.00)
        storeHitDistance = <literal> = 0
        storeDistance = <literal> = 0
        repeatInterval = <literal> = 1
        layerMask = [<literal> = 8]
        invertMask = <literal> = False
        ignoreTriggers = <literal> = False
        debugColor = <literal> = RGBA(1.000, 0.922, 0.016, 1.000)
        debug = <literal> = False
    action BoolFlipEveryFrame
        boolVariable = Show Thread = False
        everyFrame = True
    action SetMeshRendererEveryFrame
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Thread Loop = Super Jump Thread Loop (UnityEngine.GameObject)}
        active = Show Thread = False
    transition HIT ROOF -> Hit Roof Hard
    transition NORM CANCEL -> Fall Needle Cancel
```

### Air Cancel
```
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Grab Effect = Super Jump Catch Effect (UnityEngine.GameObject)}
        activate = <literal> = True
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action PlayRandomAudioClipTable
        Table = <literal> = Grunt Hornet Voice (RandomAudioClipTable)
        AudioPlayerPrefab = <literal> = null
        SpawnPoint = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        SpawnPosition = <literal> = (0.00, 0.00, 0.00)
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Throw Needle Fall = Super Jump Needle Throw Fall (UnityEngine.GameObject)}
        activate = <literal> = False
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action SetPosition
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Throw Needle Fall = Super Jump Needle Throw Fall (UnityEngine.GameObject)}
        vector = <literal> = (0.00, 0.00, 0.00)
        x = <literal> = 0
        y = Initial Throw Needle Y = 5.37
        z = <literal> = 0
        space = Self
        everyFrame = False
        lateUpdate = False
    action CallMethodProper
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Camera Target = Camera Target (UnityEngine.GameObject)}
        behaviour = <literal> = CameraTarget
        methodName = <literal> = SetSuperJump
        parameters = [FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = False, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=Bool, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}, FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = 0, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=Float, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}]
        storeResult = FsmVar{variableName="", objectType=null, useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4{x=0, y=0, z=0, w=0, normalized=Vector4, magnitude=0, sqrMagnitude=0}, objectReference=null, arrayValue=null, NamedVar=<literal> = False, NamedVarType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554527, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.FsmBool, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="FsmBool", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.FsmBool", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=False, IsSpecialName=False, IsClass=True, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=False, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData]}, EnumType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, EnumValue=None, ObjectType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, Type=Bool, RealType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554690, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", DeclaringType=null, Name="Boolean", Namespace="System", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="System.Boolean", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, SequentialLayout, Sealed, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=False, IsExplicitLayout=False, IsLayoutSequential=True, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=True, IsValueType=True, IsSignatureType=False, TypeInitializer=RuntimeConstructorInfo, IsInterface=False, CustomAttributes=[CustomAttributeData, CustomAttributeData]}, IsNone=True, vector2Value=Vector2{x=0, y=0, normalized=Vector2, magnitude=0, sqrMagnitude=0}, vector3Value=Vector3{x=0, y=0, z=0, normalized=Vector3, magnitude=0, sqrMagnitude=0}, colorValue=Color{r=0, g=0, b=0, a=0, grayscale=0, linear=Color, gamma=Color, maxColorComponent=0}, rectValue=Rect{x=0, y=0, position=Vector2, center=Vector2, min=Vector2, max=Vector2, width=0, height=0, size=Vector2, xMin=0, yMin=0, xMax=0, yMax=0, left=0, right=0, top=0, bottom=0}, quaternionValue=Quaternion{x=0, y=0, z=0, w=0, eulerAngles=Vector3, normalized=Quaternion}, gameObjectValue=null, materialValue=null, textureValue=null}
        EveryFrame = False
    action CallMethodProper
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        behaviour = <literal> = HeroController
        methodName = <literal> = SetCState
        parameters = [FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue="superDashing", vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = superDashing, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=String, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}, FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = False, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=Bool, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}]
        storeResult = FsmVar{variableName="", objectType=null, useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4{x=0, y=0, z=0, w=0, normalized=Vector4, magnitude=0, sqrMagnitude=0}, objectReference=null, arrayValue=null, NamedVar=<literal> = 0, NamedVarType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554531, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.FsmFloat, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="FsmFloat", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.FsmFloat", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=False, IsSpecialName=False, IsClass=True, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=False, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData]}, EnumType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, EnumValue=None, ObjectType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, Type=Float, RealType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554812, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="System.Single, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", DeclaringType=null, Name="Single", Namespace="System", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="System.Single", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType, RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, SequentialLayout, Sealed, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=False, IsExplicitLayout=False, IsLayoutSequential=True, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=True, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData, CustomAttributeData]}, IsNone=True, vector2Value=Vector2{x=0, y=0, normalized=Vector2, magnitude=0, sqrMagnitude=0}, vector3Value=Vector3{x=0, y=0, z=0, normalized=Vector3, magnitude=0, sqrMagnitude=0}, colorValue=Color{r=0, g=0, b=0, a=0, grayscale=0, linear=Color, gamma=Color, maxColorComponent=0}, rectValue=Rect{x=0, y=0, position=Vector2, center=Vector2, min=Vector2, max=Vector2, width=0, height=0, size=Vector2, xMin=0, yMin=0, xMax=0, yMax=0, left=0, right=0, top=0, bottom=0}, quaternionValue=Quaternion{x=0, y=0, z=0, w=0, eulerAngles=Vector3, normalized=Quaternion}, gameObjectValue=null, materialValue=null, textureValue=null}
        EveryFrame = False
    action SendEventByName
        eventTarget = FsmEventTarget{target=GameObject, excludeSelf=<literal> = False, gameObject=FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=CameraParent = CameraParent (UnityEngine.GameObject)}, fsmName=<literal> = , sendToChildren=<literal> = False, fsmComponent=null}
        sendEvent = <literal> = EnemyKillShake
        delay = <literal> = 0
        everyFrame = False
    action DecelerateV2
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        deceleration = <literal> = 0.98
        brakeOnExit = False
    action SendMessageV2
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        delivery = SendMessage
        options = DontRequireReceiver
        functionCall = FunctionCall{FunctionName="SetStartWithUpdraftExit", BoolParameter=<literal> = False, FloatParameter=<literal> = 0, IntParameter=<literal> = 0, GameObjectParameter=<literal> = null, ObjectParameter=<literal> = null, StringParameter=<literal> = , Vector2Parameter=<literal> = (0.00, 0.00), Vector3Parameter=<literal> = (0.00, 0.00, 0.00), RectParamater=<literal> = (x:0.00, y:0.00, width:0.00, height:0.00), QuaternionParameter=<literal> = (0.00000, 0.00000, 0.00000, 0.00000), MaterialParameter=<literal> = null, TextureParameter=<literal> = null, ColorParameter=<literal> = RGBA(0.000, 0.000, 0.000, 1.000), EnumParameter=<literal> = None, ArrayParameter=<literal> = System.Object[], ParameterType="None"}
        everyFrame = False
    action Tk2dPlayAnimationWithEvents
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        clipName = <literal> = Super Jump Loop Cancel
        animationTriggerEvent = null
        animationCompleteEvent = event:FINISHED
    transition FINISHED -> Regain Control To Idle
```

### Hit Roof
```
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Stick Needle = Super Jump Needle Stick (UnityEngine.GameObject)}
        activate = <literal> = False
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Damager = Super Jump Damager (UnityEngine.GameObject)}
        activate = <literal> = False
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action CallMethodProper
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Camera Target = Camera Target (UnityEngine.GameObject)}
        behaviour = <literal> = CameraTarget
        methodName = <literal> = SetSuperJump
        parameters = [FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = False, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=Bool, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}, FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = 0, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=Float, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}]
        storeResult = FsmVar{variableName="", objectType=null, useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4{x=0, y=0, z=0, w=0, normalized=Vector4, magnitude=0, sqrMagnitude=0}, objectReference=null, arrayValue=null, NamedVar=<literal> = False, NamedVarType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554527, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.FsmBool, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="FsmBool", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.FsmBool", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=False, IsSpecialName=False, IsClass=True, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=False, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData]}, EnumType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, EnumValue=None, ObjectType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, Type=Bool, RealType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554690, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", DeclaringType=null, Name="Boolean", Namespace="System", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="System.Boolean", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, SequentialLayout, Sealed, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=False, IsExplicitLayout=False, IsLayoutSequential=True, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=True, IsValueType=True, IsSignatureType=False, TypeInitializer=RuntimeConstructorInfo, IsInterface=False, CustomAttributes=[CustomAttributeData, CustomAttributeData]}, IsNone=True, vector2Value=Vector2{x=0, y=0, normalized=Vector2, magnitude=0, sqrMagnitude=0}, vector3Value=Vector3{x=0, y=0, z=0, normalized=Vector3, magnitude=0, sqrMagnitude=0}, colorValue=Color{r=0, g=0, b=0, a=0, grayscale=0, linear=Color, gamma=Color, maxColorComponent=0}, rectValue=Rect{x=0, y=0, position=Vector2, center=Vector2, min=Vector2, max=Vector2, width=0, height=0, size=Vector2, xMin=0, yMin=0, xMax=0, yMax=0, left=0, right=0, top=0, bottom=0}, quaternionValue=Quaternion{x=0, y=0, z=0, w=0, eulerAngles=Vector3, normalized=Quaternion}, gameObjectValue=null, materialValue=null, textureValue=null}
        EveryFrame = False
    action CallMethodProper
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        behaviour = <literal> = HeroController
        methodName = <literal> = SetCState
        parameters = [FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue="superDashOnWall", vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = superDashOnWall, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=String, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}, FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=True, stringValue=null, vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = True, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=Bool, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}]
        storeResult = FsmVar{variableName="", objectType=null, useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4{x=0, y=0, z=0, w=0, normalized=Vector4, magnitude=0, sqrMagnitude=0}, objectReference=null, arrayValue=null, NamedVar=<literal> = False, NamedVarType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554527, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.FsmBool, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="FsmBool", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.FsmBool", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=False, IsSpecialName=False, IsClass=True, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=False, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData]}, EnumType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, EnumValue=None, ObjectType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, Type=Bool, RealType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554690, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", DeclaringType=null, Name="Boolean", Namespace="System", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="System.Boolean", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, SequentialLayout, Sealed, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=False, IsExplicitLayout=False, IsLayoutSequential=True, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=True, IsValueType=True, IsSignatureType=False, TypeInitializer=RuntimeConstructorInfo, IsInterface=False, CustomAttributes=[CustomAttributeData, CustomAttributeData]}, IsNone=True, vector2Value=Vector2{x=0, y=0, normalized=Vector2, magnitude=0, sqrMagnitude=0}, vector3Value=Vector3{x=0, y=0, z=0, normalized=Vector3, magnitude=0, sqrMagnitude=0}, colorValue=Color{r=0, g=0, b=0, a=0, grayscale=0, linear=Color, gamma=Color, maxColorComponent=0}, rectValue=Rect{x=0, y=0, position=Vector2, center=Vector2, min=Vector2, max=Vector2, width=0, height=0, size=Vector2, xMin=0, yMin=0, xMax=0, yMax=0, left=0, right=0, top=0, bottom=0}, quaternionValue=Quaternion{x=0, y=0, z=0, w=0, eulerAngles=Vector3, normalized=Quaternion}, gameObjectValue=null, materialValue=null, textureValue=null}
        EveryFrame = False
    action CallMethodProper
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        behaviour = <literal> = HeroController
        methodName = <literal> = SetCState
        parameters = [FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue="superDashing", vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = superDashing, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=String, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}, FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = False, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=Bool, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}]
        storeResult = FsmVar{variableName="", objectType=null, useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4{x=0, y=0, z=0, w=0, normalized=Vector4, magnitude=0, sqrMagnitude=0}, objectReference=null, arrayValue=null, NamedVar=null, NamedVarType=null, EnumType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, EnumValue=None, ObjectType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, Type=Unknown, RealType=null, IsNone=True, vector2Value=Vector2{x=0, y=0, normalized=Vector2, magnitude=0, sqrMagnitude=0}, vector3Value=Vector3{x=0, y=0, z=0, normalized=Vector3, magnitude=0, sqrMagnitude=0}, colorValue=Color{r=0, g=0, b=0, a=0, grayscale=0, linear=Color, gamma=Color, maxColorComponent=0}, rectValue=Rect{x=0, y=0, position=Vector2, center=Vector2, min=Vector2, max=Vector2, width=0, height=0, size=Vector2, xMin=0, yMin=0, xMax=0, yMax=0, left=0, right=0, top=0, bottom=0}, quaternionValue=Quaternion{x=0, y=0, z=0, w=0, eulerAngles=Vector3, normalized=Quaternion}, gameObjectValue=null, materialValue=null, textureValue=null}
        EveryFrame = False
    action CallMethodProper
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Self = Hero_Hornet(Clone) (UnityEngine.GameObject)}
        behaviour = <literal> = HeroController
        methodName = <literal> = AffectedByGravity
        parameters = [FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = False, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=Bool, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}]
        storeResult = FsmVar{variableName="", objectType=null, useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4{x=0, y=0, z=0, w=0, normalized=Vector4, magnitude=0, sqrMagnitude=0}, objectReference=null, arrayValue=null, NamedVar=<literal> = False, NamedVarType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554527, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.FsmBool, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="FsmBool", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.FsmBool", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=False, IsSpecialName=False, IsClass=True, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=False, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData]}, EnumType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, EnumValue=None, ObjectType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, Type=Bool, RealType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554690, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", DeclaringType=null, Name="Boolean", Namespace="System", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="System.Boolean", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, SequentialLayout, Sealed, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=False, IsExplicitLayout=False, IsLayoutSequential=True, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=True, IsValueType=True, IsSignatureType=False, TypeInitializer=RuntimeConstructorInfo, IsInterface=False, CustomAttributes=[CustomAttributeData, CustomAttributeData]}, IsNone=True, vector2Value=Vector2{x=0, y=0, normalized=Vector2, magnitude=0, sqrMagnitude=0}, vector3Value=Vector3{x=0, y=0, z=0, normalized=Vector3, magnitude=0, sqrMagnitude=0}, colorValue=Color{r=0, g=0, b=0, a=0, grayscale=0, linear=Color, gamma=Color, maxColorComponent=0}, rectValue=Rect{x=0, y=0, position=Vector2, center=Vector2, min=Vector2, max=Vector2, width=0, height=0, size=Vector2, xMin=0, yMin=0, xMax=0, yMax=0, left=0, right=0, top=0, bottom=0}, quaternionValue=Quaternion{x=0, y=0, z=0, w=0, eulerAngles=Vector3, normalized=Quaternion}, gameObjectValue=null, materialValue=null, textureValue=null}
        EveryFrame = False
    action Tk2dWatchAnimationEvents
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        animationTriggerEvent = null
        animationCompleteEvent = event:FINISHED
    action SendMessageV2
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        delivery = SendMessage
        options = RequireReceiver
        functionCall = FunctionCall{FunctionName="SetPlaySuperJumpFall", BoolParameter=<literal> = False, FloatParameter=<literal> = 0, IntParameter=<literal> = 0, GameObjectParameter=<literal> = null, ObjectParameter=<literal> = null, StringParameter=<literal> = , Vector2Parameter=<literal> = (0.00, 0.00), Vector3Parameter=<literal> = (0.00, 0.00, 0.00), RectParamater=<literal> = (x:0.00, y:0.00, width:0.00, height:0.00), QuaternionParameter=<literal> = (0.00000, 0.00000, 0.00000, 0.00000), MaterialParameter=<literal> = null, TextureParameter=<literal> = null, ColorParameter=<literal> = RGBA(0.000, 0.000, 0.000, 1.000), EnumParameter=<literal> = None, ArrayParameter=<literal> = System.Object[], ParameterType="None"}
        everyFrame = False
    action SetVelocity2d
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        vector = <literal> = (0.00, 0.00)
        x = <literal> = 0
        y = <literal> = 0
        everyFrame = False
    transition FINISHED -> Regain Control To Idle
```

### Init
```
    action GetOwner
        storeGameObject = Self = Hero_Hornet(Clone) (UnityEngine.GameObject)
    action FindChild
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        childName = <literal> = Effects
        storeResult = Effects Folder = Effects (UnityEngine.GameObject)
    action FindChild
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Effects Folder = Effects (UnityEngine.GameObject)}
        childName = <literal> = Super Jump Antic Effect R
        storeResult = Antic Effect R = Super Jump Antic Effect R (UnityEngine.GameObject)
    action FindChild
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Effects Folder = Effects (UnityEngine.GameObject)}
        childName = <literal> = Super Jump Antic Effect L
        storeResult = Antic Effect L = Super Jump Antic Effect L (UnityEngine.GameObject)
    action FindChild
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Effects Folder = Effects (UnityEngine.GameObject)}
        childName = <literal> = Super Jump Charging Fader
        storeResult = Charging Fader = Super Jump Charging Fader (UnityEngine.GameObject)
    action FindChild
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Effects Folder = Effects (UnityEngine.GameObject)}
        childName = <literal> = Super Jump Catch Effect
        storeResult = Grab Effect = Super Jump Catch Effect (UnityEngine.GameObject)
    action FindChild
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        childName = <literal> = Special Attacks
        storeResult = Attacks Folder = Special Attacks (UnityEngine.GameObject)
    action FindChild
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Attacks Folder = Special Attacks (UnityEngine.GameObject)}
        childName = <literal> = Super Jump Needle Throw
        storeResult = Throw Needle = Super Jump Needle Throw (UnityEngine.GameObject)
    action GetPosition
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Throw Needle = Super Jump Needle Throw (UnityEngine.GameObject)}
        vector = <literal> = (-0.45, 5.37, 0.00)
        x = <literal> = -0.4500008
        y = Initial Throw Needle Y = 5.37
        z = <literal> = 0
        space = Self
        everyFrame = False
    action FindChild
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Throw Needle = Super Jump Needle Throw (UnityEngine.GameObject)}
        childName = <literal> = Move To
        storeResult = Throw Needle Target = Move To (UnityEngine.GameObject)
    action FindChild
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Throw Needle = Super Jump Needle Throw (UnityEngine.GameObject)}
        childName = <literal> = Damager
        storeResult = Throw Needle Damager = Damager (UnityEngine.GameObject)
    action FindChild
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Attacks Folder = Special Attacks (UnityEngine.GameObject)}
        childName = <literal> = Super Jump Needle Throw Fall
        storeResult = Throw Needle Fall = Super Jump Needle Throw Fall (UnityEngine.GameObject)
    action FindChild
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Throw Needle Fall = Super Jump Needle Throw Fall (UnityEngine.GameObject)}
        childName = <literal> = Move To
        storeResult = Throw Needle Fall Target = Move To (UnityEngine.GameObject)
    action FindChild
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Attacks Folder = Special Attacks (UnityEngine.GameObject)}
        childName = <literal> = Super Jump Needle Stick
        storeResult = Stick Needle = Super Jump Needle Stick (UnityEngine.GameObject)
    action FindChild
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Attacks Folder = Special Attacks (UnityEngine.GameObject)}
        childName = <literal> = Super Jump Damager
        storeResult = Damager = Super Jump Damager (UnityEngine.GameObject)
    action FindGameObject
        objectName = <literal> =
        withTag = <literal> = CameraTarget
        store = Camera Target = Camera Target (UnityEngine.GameObject)
    action FindChild
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Effects Folder = Effects (UnityEngine.GameObject)}
        childName = <literal> = Super Jump Charged
        storeResult = Charged Effect = Super Jump Charged (UnityEngine.GameObject)
    action FindChild
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        childName = <literal> = Sounds/Superjump Loop
        storeResult = Superjump Audio Loop = Superjump Loop (UnityEngine.GameObject)
    action FindChild
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        childName = <literal> = Sounds/Nail Art Ready
        storeResult = Nail Art Ready = Nail Art Ready (UnityEngine.GameObject)
    action FindGameObject
        objectName = <literal> =
        withTag = <literal> = CameraTarget
        store = Camera Target = Camera Target (UnityEngine.GameObject)
    action FindChild
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Effects Folder = Effects (UnityEngine.GameObject)}
        childName = <literal> = Super Jump Thread
        storeResult = Thread = Super Jump Thread (UnityEngine.GameObject)
    action FindChild
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Effects Folder = Effects (UnityEngine.GameObject)}
        childName = <literal> = Super Jump Thread Loop
        storeResult = Thread Loop = Super Jump Thread Loop (UnityEngine.GameObject)
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Thread = Super Jump Thread (UnityEngine.GameObject)}
        activate = <literal> = False
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Thread Loop = Super Jump Thread Loop (UnityEngine.GameObject)}
        activate = <literal> = False
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action FindChild
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Effects Folder = Effects (UnityEngine.GameObject)}
        childName = <literal> = Super Jump Extra Throw Effect
        storeResult = Extra Throw Effect = Super Jump Extra Throw Effect (UnityEngine.GameObject)
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Extra Throw Effect = Super Jump Extra Throw Effect (UnityEngine.GameObject)}
        activate = <literal> = False
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action FindChild
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Effects Folder = Effects (UnityEngine.GameObject)}
        childName = <literal> = Super Jump Extra Ground Effect
        storeResult = Extra Ground Effect = Super Jump Extra Ground Effect (UnityEngine.GameObject)
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Extra Ground Effect = Super Jump Extra Ground Effect (UnityEngine.GameObject)}
        activate = <literal> = False
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    transition FINISHED -> Inactive
```

### Reset Effects
```
    action CallMethodProper
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        behaviour = <literal> = HeroController
        methodName = <literal> = SetCState
        parameters = [FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue="superDashOnWall", vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = superDashOnWall, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=String, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}, FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = False, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=Bool, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}]
        storeResult = FsmVar{variableName="", objectType=null, useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4{x=0, y=0, z=0, w=0, normalized=Vector4, magnitude=0, sqrMagnitude=0}, objectReference=null, arrayValue=null, NamedVar=<literal> = 0, NamedVarType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554531, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.FsmFloat, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="FsmFloat", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.FsmFloat", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=False, IsSpecialName=False, IsClass=True, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=False, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData]}, EnumType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, EnumValue=None, ObjectType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, Type=Float, RealType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554812, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="System.Single, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", DeclaringType=null, Name="Single", Namespace="System", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="System.Single", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType, RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, SequentialLayout, Sealed, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=False, IsExplicitLayout=False, IsLayoutSequential=True, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=True, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData, CustomAttributeData]}, IsNone=True, vector2Value=Vector2{x=0, y=0, normalized=Vector2, magnitude=0, sqrMagnitude=0}, vector3Value=Vector3{x=0, y=0, z=0, normalized=Vector3, magnitude=0, sqrMagnitude=0}, colorValue=Color{r=0, g=0, b=0, a=0, grayscale=0, linear=Color, gamma=Color, maxColorComponent=0}, rectValue=Rect{x=0, y=0, position=Vector2, center=Vector2, min=Vector2, max=Vector2, width=0, height=0, size=Vector2, xMin=0, yMin=0, xMax=0, yMax=0, left=0, right=0, top=0, bottom=0}, quaternionValue=Quaternion{x=0, y=0, z=0, w=0, eulerAngles=Vector3, normalized=Quaternion}, gameObjectValue=null, materialValue=null, textureValue=null}
        EveryFrame = False
    action SetBoolValue
        boolVariable = Did Start Flash = False
        boolValue = <literal> = False
        everyFrame = False
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Antic Effect L = Super Jump Antic Effect L (UnityEngine.GameObject)}
        activate = <literal> = False
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Antic Effect R = Super Jump Antic Effect R (UnityEngine.GameObject)}
        activate = <literal> = False
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Throw Needle = Super Jump Needle Throw (UnityEngine.GameObject)}
        activate = <literal> = False
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Throw Needle Fall = Super Jump Needle Throw Fall (UnityEngine.GameObject)}
        activate = <literal> = False
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Stick Needle = Super Jump Needle Stick (UnityEngine.GameObject)}
        activate = <literal> = False
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Charged Effect = Super Jump Charged (UnityEngine.GameObject)}
        activate = <literal> = False
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Charging Fader = Super Jump Charging Fader (UnityEngine.GameObject)}
        activate = <literal> = False
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action GameObjectCompare
        gameObjectVariable = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Stick Needle Parent = null}
        compareTo = <literal> = null
        equalEvent = event:FINISHED
        notEqualEvent = null
        storeResult = <literal> = True
        everyFrame = False
    action SetTransformParent
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Stick Needle = Super Jump Needle Stick (UnityEngine.GameObject)}
        parent = Stick Needle Parent = null
        worldPositionStays = <literal> = True
    action SendEventToRegister
        eventName = <literal> = SUPER JUMP ENDED
    transition FINISHED -> Inactive
```

### Throw Needle Start
```
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Extra Throw Effect = Super Jump Extra Throw Effect (UnityEngine.GameObject)}
        activate = <literal> = True
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action FadeNestedFadeGroup
        Target = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Charging Fader = Super Jump Charging Fader (UnityEngine.GameObject)}
        ToAlpha = <literal> = 0
        FadeTime = <literal> = 0.1
    action Tk2dPlayAnimation
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Antic Effect L = Super Jump Antic Effect L (UnityEngine.GameObject)}
        animLibName = <literal> =
        clipName = <literal> = Super Jump Antic Effect End
    action Tk2dPlayAnimation
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Antic Effect R = Super Jump Antic Effect R (UnityEngine.GameObject)}
        animLibName = <literal> =
        clipName = <literal> = Super Jump Antic Effect End
    action CancelFlashByID
        Target = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        ID = Sprite Flash ID = 0
    action SetBoolValue
        boolVariable = Did Start Flash = False
        boolValue = <literal> = False
        everyFrame = False
    action SetFsmBool
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=CameraParent = CameraParent (UnityEngine.GameObject)}
        fsmName = <literal> = CameraShake
        variableName = <literal> = RumblingFocus
        setValue = <literal> = False
        everyFrame = False
    action SetFsmBool
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=CameraParent = CameraParent (UnityEngine.GameObject)}
        fsmName = <literal> = CameraShake
        variableName = <literal> = RumblingFocus2
        setValue = <literal> = False
        everyFrame = False
    action SetBoolValue
        boolVariable = Started Rumbling Focus 2 = False
        boolValue = <literal> = False
        everyFrame = False
    action SendMessageV2
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        delivery = SendMessage
        options = DontRequireReceiver
        functionCall = FunctionCall{FunctionName="flashFocusHeal", BoolParameter=<literal> = False, FloatParameter=<literal> = 0, IntParameter=<literal> = 0, GameObjectParameter=<literal> = null, ObjectParameter=<literal> = null, StringParameter=<literal> = , Vector2Parameter=<literal> = (0.00, 0.00), Vector3Parameter=<literal> = (0.00, 0.00, 0.00), RectParamater=<literal> = (x:0.00, y:0.00, width:0.00, height:0.00), QuaternionParameter=<literal> = (0.00000, 0.00000, 0.00000, 0.00000), MaterialParameter=<literal> = null, TextureParameter=<literal> = null, ColorParameter=<literal> = RGBA(0.000, 0.000, 0.000, 1.000), EnumParameter=<literal> = None, ArrayParameter=<literal> = System.Object[], ParameterType="None"}
        everyFrame = False
    action Tk2dPlayAnimationWithEvents
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        clipName = <literal> = Super Jump Throw
        animationTriggerEvent = event:FINISHED
        animationCompleteEvent = null
    action SendEventToRegister
        eventName = <literal> = SUPER JUMP THROW NEEDLE
    transition FINISHED -> Get Distance
```

### Throw Needle
```
    action tk2dPlayAnimAfterPreviousComplete
        Target = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        AnimName = <literal> = Super Jump Throw Wait
        StoreDidPlay = Played Throw Wait = False
    action PlayRandomAudioClipTable
        Table = <literal> = Attack Heavy Hornet Voice (RandomAudioClipTable)
        AudioPlayerPrefab = <literal> = null
        SpawnPoint = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        SpawnPosition = <literal> = (0.00, 0.00, 0.00)
    action PlayAudioEvent
        audioClip = <literal> = hornet_superjump_pt_4_throw (UnityEngine.AudioClip)
        pitchMin = <literal> = 1
        pitchMax = <literal> = 1
        volume = <literal> = 1
        audioPlayerPrefab = <literal> = null
        spawnPoint = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        spawnPosition = <literal> = (0.00, 0.00, 0.00)
        SpawnedPlayerRef = <literal> = null
    action PlayVibrationV2
        vibrationData = FSMVibrationData{lowFidelityVibration=<literal> = None, highFidelityVibration=<literal> = null, gamepadVibration=<literal> = null, ps5Vibration=<literal> = null, VibrationData=VibrationData{LowFidelityVibration=None, HighFidelityVibration=null, GamepadVibration=null, Strength=1, PS5VibrationAsset=null}}
        vibrationDataAsset = <literal> = hornet_need_throw_superjump (VibrationDataAsset)
        motors = <literal> = None
        loopTime = <literal> = 0
        loopAuto = <literal> = False
        tag = <literal> =
        stopOnStateExit = <literal> = False
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Charged Effect = Super Jump Charged (UnityEngine.GameObject)}
        activate = <literal> = False
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action SetPosition
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Throw Needle = Super Jump Needle Throw (UnityEngine.GameObject)}
        vector = <literal> = (0.00, 0.00, 0.00)
        x = <literal> = 0
        y = Initial Throw Needle Y = 5.37
        z = <literal> = 0
        space = Self
        everyFrame = False
        lateUpdate = False
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Throw Needle = Super Jump Needle Throw (UnityEngine.GameObject)}
        activate = <literal> = True
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action SetVelocity2d
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Throw Needle = Super Jump Needle Throw (UnityEngine.GameObject)}
        vector = <literal> = (0.00, 150.00)
        x = <literal> = 0
        y = <literal> = 0
        everyFrame = False
    action CheckCollisionSideV2
        collidingObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Throw Needle = Super Jump Needle Throw (UnityEngine.GameObject)}
        topHit = <literal> = False
        rightHit = <literal> = False
        bottomHit = <literal> = False
        leftHit = <literal> = False
        topHitEvent = event:FINISHED
        rightHitEvent = null
        bottomHitEvent = null
        leftHitEvent = null
        otherLayer = True
        otherLayerNumber = 8
        ignoreTriggers = <literal> = True
        ignoreBodyVelocity = <literal> = True
    action Wait
        time = <literal> = 0.8
        finishEvent = event:FINISHED
        realTime = False
    action GetPosition
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Throw Needle = Super Jump Needle Throw (UnityEngine.GameObject)}
        vector = <literal> = (0.00, 0.00, 0.00)
        x = <literal> = 0
        y = Throw Needle Pos Y Start = 0
        z = <literal> = 0
        space = World
        everyFrame = False
    action GetPosition
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Throw Needle = Super Jump Needle Throw (UnityEngine.GameObject)}
        vector = <literal> = (0.00, 0.00, 0.00)
        x = <literal> = 0
        y = Throw Needle Pos Y = 0
        z = <literal> = 0
        space = World
        everyFrame = True
    action GetPosition
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        vector = <literal> = (0.00, 0.00, 0.00)
        x = <literal> = 0
        y = Check Y = 0
        z = <literal> = 0
        space = World
        everyFrame = False
    action FloatAdd
        floatVariable = Check Y = 0
        add = <literal> = 30
        everyFrame = False
        perSecond = False
    action FloatTestToBool
        float1 = Throw Needle Pos Y = 0
        float2 = Check Y = 0
        tolerance = <literal> = 0
        equalBool = <literal> = False
        lessThanBool = <literal> = False
        greaterThanBool = Is Distant = False
        everyFrame = True
    action BoolFlipEveryFrame
        boolVariable = Is Distant = False
        everyFrame = True
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Throw Needle Damager = Damager (UnityEngine.GameObject)}
        activate = Is Distant = False
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = True
    action FloatCompare
        float1 = Throw Needle Pos Y = 0
        float2 = Throw Needle Pos Y Start = 0
        tolerance = <literal> = 0.1
        equal = null
        lessThan = event:FINISHED
        greaterThan = null
        everyFrame = True
    transition FINISHED -> Position Stick Needle Pre
    transition DAMAGER HIT SPIKES -> Hit Spikes
```

### Position Stick Needle
```
    action GetPosition
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        vector = <literal> = (0.00, 0.00, 0.00)
        x = <literal> = 0
        y = Check Y = 0
        z = <literal> = 0
        space = World
        everyFrame = False
    action FloatAdd
        floatVariable = Check Y = 0
        add = <literal> = 15
        everyFrame = False
        perSecond = False
    action FloatTestToBool
        float1 = Stick Needle Y = 0
        float2 = Check Y = 0
        tolerance = <literal> = 0
        equalBool = <literal> = False
        lessThanBool = <literal> = False
        greaterThanBool = Is Distant = False
        everyFrame = False
    action BoolTestToObject
        Test = Is Distant = False
        ExpectedValue = <literal> = True
        TrueObject = <literal> = hornet_superjump_pt_5_needle_impact_2d_distant (UnityEngine.AudioClip)
        FalseObject = <literal> = hornet_superjump_pt_5_needle_impact_2d (UnityEngine.AudioClip)
        StoreResult = FsmVar{variableName="Clip", objectType="UnityEngine.AudioClip", useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4{x=0, y=0, z=0, w=0, normalized=Vector4, magnitude=0, sqrMagnitude=0}, objectReference=null, arrayValue=null, NamedVar=Clip = null, NamedVarType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554535, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.FsmObject, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="FsmObject", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.FsmObject", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=False, IsSpecialName=False, IsClass=True, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=False, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData]}, EnumType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, EnumValue=None, ObjectType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554449, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="UnityEngine.AudioClip, UnityEngine.AudioModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="AudioClip", Namespace="UnityEngine", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="UnityEngine.AudioClip", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo], DeclaredEvents=[RuntimeEventInfo, RuntimeEventInfo], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimeEventInfo, RuntimeEventInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeType, RuntimeType], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[RuntimeType, RuntimeType], DeclaredProperties=[RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo], ImplementedInterfaces=[], IsSerializable=False, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=True, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=False, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData, CustomAttributeData]}, Type=Object, RealType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554449, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="UnityEngine.AudioClip, UnityEngine.AudioModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="AudioClip", Namespace="UnityEngine", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="UnityEngine.AudioClip", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo], DeclaredEvents=[RuntimeEventInfo, RuntimeEventInfo], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimeEventInfo, RuntimeEventInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeType, RuntimeType], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[RuntimeType, RuntimeType], DeclaredProperties=[RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo], ImplementedInterfaces=[], IsSerializable=False, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=True, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=False, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData, CustomAttributeData]}, IsNone=False, vector2Value=Vector2{x=0, y=0, normalized=Vector2, magnitude=0, sqrMagnitude=0}, vector3Value=Vector3{x=0, y=0, z=0, normalized=Vector3, magnitude=0, sqrMagnitude=0}, colorValue=Color{r=0, g=0, b=0, a=0, grayscale=0, linear=Color, gamma=Color, maxColorComponent=0}, rectValue=Rect{x=0, y=0, position=Vector2, center=Vector2, min=Vector2, max=Vector2, width=0, height=0, size=Vector2, xMin=0, yMin=0, xMax=0, yMax=0, left=0, right=0, top=0, bottom=0}, quaternionValue=Quaternion{x=0, y=0, z=0, w=0, eulerAngles=Vector3, normalized=Quaternion}, gameObjectValue=null, materialValue=null, textureValue=null}
        EveryFrame = False
    action PlayAudioEvent
        audioClip = Clip = null
        pitchMin = <literal> = 0.95
        pitchMax = <literal> = 1.05
        volume = <literal> = 1
        audioPlayerPrefab = <literal> = Audio Player Actor 2D (UnityEngine.AudioSource)
        spawnPoint = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        spawnPosition = <literal> = (0.00, 10.00, 0.00)
        SpawnedPlayerRef = <literal> = null
    action BoolTest
        boolVariable = Played Throw Wait = False
        isTrue = event:FINISHED
        isFalse = null
        everyFrame = False
    action Tk2dWatchAnimationEvents
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        animationTriggerEvent = null
        animationCompleteEvent = event:FINISHED
    transition CANCEL -> Catch Wait
    transition FINISHED -> Throw Wait
```

### Fall Catch Needle
```
    action AudioStopV2
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Superjump Audio Loop = Superjump Loop (UnityEngine.GameObject)}
        fadeTime = <literal> = 0
        cancelOnEarlyExit = False
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Throw Needle Fall = Super Jump Needle Throw Fall (UnityEngine.GameObject)}
        activate = <literal> = False
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action SetPosition
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Throw Needle Fall = Super Jump Needle Throw Fall (UnityEngine.GameObject)}
        vector = <literal> = (0.00, 0.00, 0.00)
        x = <literal> = 0
        y = Initial Throw Needle Y = 5.37
        z = <literal> = 0
        space = Self
        everyFrame = False
        lateUpdate = False
    action CallMethodProper
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        behaviour = <literal> = HeroController
        methodName = <literal> = SetCState
        parameters = [FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue="freezeCharge", vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = freezeCharge, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=String, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}, FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = False, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=Bool, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}]
        storeResult = FsmVar{variableName="", objectType=null, useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4{x=0, y=0, z=0, w=0, normalized=Vector4, magnitude=0, sqrMagnitude=0}, objectReference=null, arrayValue=null, NamedVar=<literal> = 0, NamedVarType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554531, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.FsmFloat, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="FsmFloat", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.FsmFloat", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=False, IsSpecialName=False, IsClass=True, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=False, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData]}, EnumType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, EnumValue=None, ObjectType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, Type=Float, RealType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554812, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="System.Single, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", DeclaringType=null, Name="Single", Namespace="System", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="System.Single", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType, RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, SequentialLayout, Sealed, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=False, IsExplicitLayout=False, IsLayoutSequential=True, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=True, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData, CustomAttributeData]}, IsNone=True, vector2Value=Vector2{x=0, y=0, normalized=Vector2, magnitude=0, sqrMagnitude=0}, vector3Value=Vector3{x=0, y=0, z=0, normalized=Vector3, magnitude=0, sqrMagnitude=0}, colorValue=Color{r=0, g=0, b=0, a=0, grayscale=0, linear=Color, gamma=Color, maxColorComponent=0}, rectValue=Rect{x=0, y=0, position=Vector2, center=Vector2, min=Vector2, max=Vector2, width=0, height=0, size=Vector2, xMin=0, yMin=0, xMax=0, yMax=0, left=0, right=0, top=0, bottom=0}, quaternionValue=Quaternion{x=0, y=0, z=0, w=0, eulerAngles=Vector3, normalized=Quaternion}, gameObjectValue=null, materialValue=null, textureValue=null}
        EveryFrame = False
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Grab Effect = Super Jump Catch Effect (UnityEngine.GameObject)}
        activate = <literal> = True
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action PlayAudioEvent
        audioClip = <literal> = hornet_superjump_cancel (UnityEngine.AudioClip)
        pitchMin = <literal> = 0.98
        pitchMax = <literal> = 1.02
        volume = <literal> = 1
        audioPlayerPrefab = <literal> = null
        spawnPoint = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        spawnPosition = <literal> = (0.00, 0.00, 0.00)
        SpawnedPlayerRef = <literal> = null
    action Tk2dPlayAnimationWithEvents
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        clipName = <literal> = Super Jump Catch Cancel
        animationTriggerEvent = null
        animationCompleteEvent = event:FINISHED
    action DebugLogConsole
        logLevel = Error
        text = <literal> = Superjump couldn't find roof, cancelling
        sendToUnityLog = True
    transition FINISHED -> Regain Control To Idle
```

### Throw Wait
```
    action Tk2dPlayAnimationWithEvents
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        clipName = <literal> = Super Jump Throw Wait
        animationTriggerEvent = null
        animationCompleteEvent = null
    action GameObjectIsVisible
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Stick Needle = Super Jump Needle Stick (UnityEngine.GameObject)}
        trueEvent = null
        falseEvent = null
        storeResult = Needle Visible = False
        everyFrame = False
    action ConvertBoolToFloat
        boolVariable = Needle Visible = False
        floatVariable = Throw Wait Time = 0
        falseValue = <literal> = 0.5
        trueValue = <literal> = 0
        everyFrame = False
    action Wait
        time = Throw Wait Time = 0
        finishEvent = event:FINISHED
        realTime = False
    transition FINISHED -> Jump Antic
```

### Jump Antic
```
    action AudioStopV2
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Superjump Audio Loop = Superjump Loop (UnityEngine.GameObject)}
        fadeTime = <literal> = 0
        cancelOnEarlyExit = False
    action PlayAudioEvent
        audioClip = <literal> = hornet_superjump_pt_6_hornet_jump_antic (UnityEngine.AudioClip)
        pitchMin = <literal> = 0.95
        pitchMax = <literal> = 1.05
        volume = <literal> = 1
        audioPlayerPrefab = <literal> = null
        spawnPoint = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        spawnPosition = <literal> = (0.00, 0.00, 0.00)
        SpawnedPlayerRef = <literal> = null
    action Tk2dPlayAnimationWithEvents
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        clipName = <literal> = Super Jump Jump Antic
        animationTriggerEvent = null
        animationCompleteEvent = event:FINISHED
    transition FINISHED -> Dash Start
```

### Catch Wait
```
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Stick Needle = Super Jump Needle Stick (UnityEngine.GameObject)}
        activate = <literal> = False
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action Tk2dPlayAnimationWithEvents
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        clipName = <literal> = Super Jump Throw Wait
        animationTriggerEvent = null
        animationCompleteEvent = null
    action Wait
        time = <literal> = 1
        finishEvent = event:FINISHED
        realTime = False
    transition FINISHED -> Fall Needle
```

### Fall Needle
```
    action GetPosition
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Throw Needle Fall = Super Jump Needle Throw Fall (UnityEngine.GameObject)}
        vector = <literal> = (0.00, 0.00, 0.00)
        x = <literal> = 0
        y = Current Throw Needle Y = 0
        z = <literal> = 0
        space = World
        everyFrame = False
    action GetPosition
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Throw Needle Fall Target = Move To (UnityEngine.GameObject)}
        vector = <literal> = (0.00, 0.00, 0.00)
        x = <literal> = 0
        y = Throw Needle Target Y = 0
        z = <literal> = 0
        space = World
        everyFrame = False
    action FloatOperator
        float1 = Throw Needle Target Y = 0
        float2 = Current Throw Needle Y = 0
        operation = Subtract
        storeResult = Throw Needle Distance = 0
        everyFrame = False
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Throw Needle Fall = Super Jump Needle Throw Fall (UnityEngine.GameObject)}
        activate = <literal> = True
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action SetVector3XYZ
        vector3Variable = Throw Needle Move By = (0.00, 0.00, 0.00)
        vector3Value = <literal> = (0.00, 0.00, 0.00)
        x = <literal> = 0
        y = Throw Needle Distance = 0
        z = <literal> = 0
        everyFrame = False
    action iTweenMoveBy
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Throw Needle Fall = Super Jump Needle Throw Fall (UnityEngine.GameObject)}
        id = <literal> =
        vector = Throw Needle Move By = (0.00, 0.00, 0.00)
        time = <literal> = 0
        delay = <literal> = 0
        speed = <literal> = 150
        easeType = linear
        loopType = none
        space = World
        orientToPath = <literal> = False
        lookAtObject = <literal> = null
        lookAtVector = <literal> = (0.00, 0.00, 0.00)
        lookTime = <literal> = 0
        axis = none
        startEvent = null
        finishEvent = event:FINISHED
        realTime = <literal> = False
        stopOnExit = <literal> = True
        loopDontFinish = <literal> = False
    transition FINISHED -> Fall Catch Needle
```

### Enough Silk?
```
    action GetPlayerDataVariable
        VariableName = <literal> = silk
        StoreValue = FsmVar{variableName="Current Silk", objectType="UnityEngine.Object", useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4{x=0, y=0, z=0, w=0, normalized=Vector4, magnitude=0, sqrMagnitude=0}, objectReference=null, arrayValue=null, NamedVar=Current Silk = 0, NamedVarType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554533, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.FsmInt, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="FsmInt", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.FsmInt", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=False, IsSpecialName=False, IsClass=True, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=False, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData]}, EnumType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, EnumValue=None, ObjectType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33555133, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="UnityEngine.Object, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="Object", Namespace="UnityEngine", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="UnityEngine.Object", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo, RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeType], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[RuntimeType], DeclaredProperties=[RuntimePropertyInfo, RuntimePropertyInfo], ImplementedInterfaces=[], IsSerializable=False, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, SequentialLayout, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=False, IsSpecialName=False, IsClass=True, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=False, IsExplicitLayout=False, IsLayoutSequential=True, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=False, IsSignatureType=False, TypeInitializer=RuntimeConstructorInfo, IsInterface=False, CustomAttributes=[CustomAttributeData, CustomAttributeData, CustomAttributeData, CustomAttributeData]}, Type=Int, RealType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554761, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", DeclaringType=null, Name="Int32", Namespace="System", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="System.Int32", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType, RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, SequentialLayout, Sealed, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=False, IsExplicitLayout=False, IsLayoutSequential=True, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=True, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData, CustomAttributeData]}, IsNone=False, vector2Value=Vector2{x=0, y=0, normalized=Vector2, magnitude=0, sqrMagnitude=0}, vector3Value=Vector3{x=0, y=0, z=0, normalized=Vector3, magnitude=0, sqrMagnitude=0}, colorValue=Color{r=0, g=0, b=0, a=0, grayscale=0, linear=Color, gamma=Color, maxColorComponent=0}, rectValue=Rect{x=0, y=0, position=Vector2, center=Vector2, min=Vector2, max=Vector2, width=0, height=0, size=Vector2, xMin=0, yMin=0, xMax=0, yMax=0, left=0, right=0, top=0, bottom=0}, quaternionValue=Quaternion{x=0, y=0, z=0, w=0, eulerAngles=Vector3, normalized=Quaternion}, gameObjectValue=null, materialValue=null, textureValue=null}
    action IntCompare
        integer1 = Current Silk = 0
        integer2 = Silk Cost = 1
        equal = null
        lessThan = event:CANCEL
        greaterThan = null
        everyFrame = False
    transition CANCEL -> Inactive
    transition FINISHED -> Relinquish Control
```

### Fall Needle Cancel
```
    action SetBoolValue
        boolVariable = Queued Cancel = False
        boolValue = <literal> = False
        everyFrame = False
    action PlayAudioEvent
        audioClip = <literal> = hornet_superjump_cancel (UnityEngine.AudioClip)
        pitchMin = <literal> = 0.98
        pitchMax = <literal> = 1.02
        volume = <literal> = 1
        audioPlayerPrefab = <literal> = null
        spawnPoint = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        spawnPosition = <literal> = (0.00, 0.00, 0.00)
        SpawnedPlayerRef = <literal> = null
    action GetPosition
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Throw Needle Fall = Super Jump Needle Throw Fall (UnityEngine.GameObject)}
        vector = <literal> = (0.00, 0.00, 0.00)
        x = <literal> = 0
        y = Current Throw Needle Y = 0
        z = <literal> = 0
        space = World
        everyFrame = False
    action GetPosition
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Throw Needle Fall Target = Move To (UnityEngine.GameObject)}
        vector = <literal> = (0.00, 0.00, 0.00)
        x = <literal> = 0
        y = Throw Needle Target Y = 0
        z = <literal> = 0
        space = World
        everyFrame = False
    action FloatOperator
        float1 = Throw Needle Target Y = 0
        float2 = Current Throw Needle Y = 0
        operation = Subtract
        storeResult = Throw Needle Distance = 0
        everyFrame = False
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Throw Needle Fall = Super Jump Needle Throw Fall (UnityEngine.GameObject)}
        activate = <literal> = True
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Stick Needle = Super Jump Needle Stick (UnityEngine.GameObject)}
        activate = <literal> = False
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action SetVector3XYZ
        vector3Variable = Throw Needle Move By = (0.00, 0.00, 0.00)
        vector3Value = <literal> = (0.00, 0.00, 0.00)
        x = <literal> = 0
        y = Throw Needle Distance = 0
        z = <literal> = 0
        everyFrame = False
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Damager = Super Jump Damager (UnityEngine.GameObject)}
        activate = <literal> = False
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action iTweenMoveBy
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Throw Needle Fall = Super Jump Needle Throw Fall (UnityEngine.GameObject)}
        id = <literal> =
        vector = Throw Needle Move By = (0.00, 0.00, 0.00)
        time = <literal> = 0
        delay = <literal> = 0
        speed = <literal> = 150
        easeType = linear
        loopType = none
        space = World
        orientToPath = <literal> = False
        lookAtObject = <literal> = null
        lookAtVector = <literal> = (0.00, 0.00, 0.00)
        lookTime = <literal> = 0
        axis = none
        startEvent = null
        finishEvent = event:FINISHED
        realTime = <literal> = False
        stopOnExit = <literal> = True
        loopDontFinish = <literal> = False
    action DecelerateV2
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        deceleration = <literal> = 0.98
        brakeOnExit = False
    action CallMethodProper
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Camera Target = Camera Target (UnityEngine.GameObject)}
        behaviour = <literal> = CameraTarget
        methodName = <literal> = SetSuperJump
        parameters = [FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = False, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=Bool, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}, FsmVar{variableName="Stick Needle Y", objectType="UnityEngine.Object", useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=Stick Needle Y = 0, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=Float, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}]
        storeResult = FsmVar{variableName="", objectType=null, useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4{x=0, y=0, z=0, w=0, normalized=Vector4, magnitude=0, sqrMagnitude=0}, objectReference=null, arrayValue=null, NamedVar=<literal> = False, NamedVarType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554527, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.FsmBool, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="FsmBool", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.FsmBool", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=False, IsSpecialName=False, IsClass=True, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=False, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData]}, EnumType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, EnumValue=None, ObjectType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, Type=Bool, RealType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554690, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", DeclaringType=null, Name="Boolean", Namespace="System", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="System.Boolean", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, SequentialLayout, Sealed, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=False, IsExplicitLayout=False, IsLayoutSequential=True, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=True, IsValueType=True, IsSignatureType=False, TypeInitializer=RuntimeConstructorInfo, IsInterface=False, CustomAttributes=[CustomAttributeData, CustomAttributeData]}, IsNone=True, vector2Value=Vector2{x=0, y=0, normalized=Vector2, magnitude=0, sqrMagnitude=0}, vector3Value=Vector3{x=0, y=0, z=0, normalized=Vector3, magnitude=0, sqrMagnitude=0}, colorValue=Color{r=0, g=0, b=0, a=0, grayscale=0, linear=Color, gamma=Color, maxColorComponent=0}, rectValue=Rect{x=0, y=0, position=Vector2, center=Vector2, min=Vector2, max=Vector2, width=0, height=0, size=Vector2, xMin=0, yMin=0, xMax=0, yMax=0, left=0, right=0, top=0, bottom=0}, quaternionValue=Quaternion{x=0, y=0, z=0, w=0, eulerAngles=Vector3, normalized=Quaternion}, gameObjectValue=null, materialValue=null, textureValue=null}
        EveryFrame = False
    action SendEventToRegister
        eventName = <literal> = SUPER JUMP ENDED
    transition FINISHED -> Air Cancel
```

### Start Delay
```
    action DecelerateXY
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        decelerationX = <literal> = 0.9
        decelerationY = <literal> = 0
        brakeOnExit = False
    action ListenForSuperdash
        eventTarget = FsmEventTarget{target=Self, excludeSelf=<literal> = False, gameObject=FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}, fsmName=<literal> = , sendToChildren=<literal> = False, fsmComponent=null}
        wasPressed = null
        wasReleased = event:BUTTON UP
        isPressed = null
        isNotPressed = null
    action Wait
        time = <literal> = 0.15
        finishEvent = event:FINISHED
        realTime = False
    transition BUTTON UP -> Regain Control
    transition FINISHED -> Ground Charge
```

### Regain Control
```
    action SendMessage
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        delivery = SendMessage
        options = RequireReceiver
        functionCall = FunctionCall{FunctionName="RegainControl", BoolParameter=<literal> = False, FloatParameter=<literal> = 0, IntParameter=<literal> = 0, GameObjectParameter=<literal> = null, ObjectParameter=<literal> = null, StringParameter=<literal> = , Vector2Parameter=<literal> = (0.00, 0.00), Vector3Parameter=<literal> = (0.00, 0.00, 0.00), RectParamater=<literal> = (x:0.00, y:0.00, width:0.00, height:0.00), QuaternionParameter=<literal> = (0.00000, 0.00000, 0.00000, 0.00000), MaterialParameter=<literal> = null, TextureParameter=<literal> = null, ColorParameter=<literal> = RGBA(0.000, 0.000, 0.000, 1.000), EnumParameter=<literal> = None, ArrayParameter=<literal> = System.Object[], ParameterType="None"}
    action SendMessage
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        delivery = SendMessage
        options = RequireReceiver
        functionCall = FunctionCall{FunctionName="StartAnimationControl", BoolParameter=<literal> = False, FloatParameter=<literal> = 0, IntParameter=<literal> = 0, GameObjectParameter=<literal> = null, ObjectParameter=<literal> = null, StringParameter=<literal> = , Vector2Parameter=<literal> = (0.00, 0.00), Vector3Parameter=<literal> = (0.00, 0.00, 0.00), RectParamater=<literal> = (x:0.00, y:0.00, width:0.00, height:0.00), QuaternionParameter=<literal> = (0.00000, 0.00000, 0.00000, 0.00000), MaterialParameter=<literal> = null, TextureParameter=<literal> = null, ColorParameter=<literal> = RGBA(0.000, 0.000, 0.000, 1.000), EnumParameter=<literal> = None, ArrayParameter=<literal> = System.Object[], ParameterType="None"}
    action SendMessage
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        delivery = SendMessage
        options = DontRequireReceiver
        functionCall = FunctionCall{FunctionName="AffectedByGravity", BoolParameter=<literal> = True, FloatParameter=<literal> = 0, IntParameter=<literal> = 0, GameObjectParameter=<literal> = null, ObjectParameter=<literal> = null, StringParameter=<literal> = , Vector2Parameter=<literal> = (0.00, 0.00), Vector3Parameter=<literal> = (0.00, 0.00, 0.00), RectParamater=<literal> = (x:0.00, y:0.00, width:0.00, height:0.00), QuaternionParameter=<literal> = (0.00000, 0.00000, 0.00000, 0.00000), MaterialParameter=<literal> = null, TextureParameter=<literal> = null, ColorParameter=<literal> = RGBA(0.000, 0.000, 0.000, 1.000), EnumParameter=<literal> = None, ArrayParameter=<literal> = System.Object[], ParameterType="bool"}
    transition FINISHED -> Inactive
```

### Hit Roof Hard
```
    action CallStaticMethod
        className = <literal> = DeliveryQuestItem
        methodName = <literal> = TakeHit
        parameters = []
        storeResult = FsmVar{variableName="", objectType=null, useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4{x=0, y=0, z=0, w=0, normalized=Vector4, magnitude=0, sqrMagnitude=0}, objectReference=null, arrayValue=null, NamedVar=null, NamedVarType=null, EnumType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, EnumValue=None, ObjectType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, Type=Unknown, RealType=null, IsNone=True, vector2Value=Vector2{x=0, y=0, normalized=Vector2, magnitude=0, sqrMagnitude=0}, vector3Value=Vector3{x=0, y=0, z=0, normalized=Vector3, magnitude=0, sqrMagnitude=0}, colorValue=Color{r=0, g=0, b=0, a=0, grayscale=0, linear=Color, gamma=Color, maxColorComponent=0}, rectValue=Rect{x=0, y=0, position=Vector2, center=Vector2, min=Vector2, max=Vector2, width=0, height=0, size=Vector2, xMin=0, yMin=0, xMax=0, yMax=0, left=0, right=0, top=0, bottom=0}, quaternionValue=Quaternion{x=0, y=0, z=0, w=0, eulerAngles=Vector3, normalized=Quaternion}, gameObjectValue=null, materialValue=null, textureValue=null}
        everyFrame = False
    action CreateNoiseV2
        From = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        LocalOrigin = <literal> = (0.00, 0.00)
        Radius = <literal> = 5
        Intensity = <literal> = Normal
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Stick Needle = Super Jump Needle Stick (UnityEngine.GameObject)}
        activate = <literal> = False
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Damager = Super Jump Damager (UnityEngine.GameObject)}
        activate = <literal> = False
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action SpawnObjectFromGlobalPool
        gameObject = <literal> = Roof Slam Effect R (UnityEngine.GameObject)
        spawnPoint = Self = Hero_Hornet(Clone) (UnityEngine.GameObject)
        position = <literal> = (0.00, 0.00, 0.00)
        rotation = <literal> = (0.00, 0.00, 0.00)
        storeObject = <literal> = null
    action PlayRandomAudioClipTable
        Table = <literal> = Grunt Hornet Voice (RandomAudioClipTable)
        AudioPlayerPrefab = <literal> = null
        SpawnPoint = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        SpawnPosition = <literal> = (0.00, 0.00, 0.00)
    action DoCameraShakeV2
        Target = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        MaxCameraDistance = <literal> = (0.00, 0.00)
        Camera = <literal> = Main Camera (CameraManagerReference)
        Profile = <literal> = Average Shake (CameraShakeProfile)
        DoFreeze = <literal> = True
        Delay = <literal> = 0
        CancelOnExit = False
    action Tk2dPlayAnimationWithEvents
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        clipName = <literal> = Super Jump Hit Roof
        animationTriggerEvent = null
        animationCompleteEvent = event:FINISHED
    action CallMethodProper
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Camera Target = Camera Target (UnityEngine.GameObject)}
        behaviour = <literal> = CameraTarget
        methodName = <literal> = SetSuperJump
        parameters = [FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = False, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=Bool, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}, FsmVar{variableName="Stick Needle Y", objectType="UnityEngine.Object", useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=Stick Needle Y = 0, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=Float, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}]
        storeResult = FsmVar{variableName="", objectType=null, useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4{x=0, y=0, z=0, w=0, normalized=Vector4, magnitude=0, sqrMagnitude=0}, objectReference=null, arrayValue=null, NamedVar=<literal> = False, NamedVarType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554527, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.FsmBool, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="FsmBool", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.FsmBool", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=False, IsSpecialName=False, IsClass=True, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=False, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData]}, EnumType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, EnumValue=None, ObjectType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, Type=Bool, RealType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554690, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", DeclaringType=null, Name="Boolean", Namespace="System", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="System.Boolean", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, SequentialLayout, Sealed, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=False, IsExplicitLayout=False, IsLayoutSequential=True, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=True, IsValueType=True, IsSignatureType=False, TypeInitializer=RuntimeConstructorInfo, IsInterface=False, CustomAttributes=[CustomAttributeData, CustomAttributeData]}, IsNone=True, vector2Value=Vector2{x=0, y=0, normalized=Vector2, magnitude=0, sqrMagnitude=0}, vector3Value=Vector3{x=0, y=0, z=0, normalized=Vector3, magnitude=0, sqrMagnitude=0}, colorValue=Color{r=0, g=0, b=0, a=0, grayscale=0, linear=Color, gamma=Color, maxColorComponent=0}, rectValue=Rect{x=0, y=0, position=Vector2, center=Vector2, min=Vector2, max=Vector2, width=0, height=0, size=Vector2, xMin=0, yMin=0, xMax=0, yMax=0, left=0, right=0, top=0, bottom=0}, quaternionValue=Quaternion{x=0, y=0, z=0, w=0, eulerAngles=Vector3, normalized=Quaternion}, gameObjectValue=null, materialValue=null, textureValue=null}
        EveryFrame = False
    action SendEventToRegister
        eventName = <literal> = SUPER JUMP ENDED
    action PlayAudioEvent
        audioClip = <literal> = hornet_land_hard new (UnityEngine.AudioClip)
        pitchMin = <literal> = 1
        pitchMax = <literal> = 1
        volume = <literal> = 1
        audioPlayerPrefab = <literal> = null
        spawnPoint = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        spawnPosition = <literal> = (0.00, 0.00, 0.00)
        SpawnedPlayerRef = <literal> = null
    action PlayVibrationV2
        vibrationData = FSMVibrationData{lowFidelityVibration=<literal> = None, highFidelityVibration=<literal> = null, gamepadVibration=<literal> = null, ps5Vibration=<literal> = null, VibrationData=VibrationData{LowFidelityVibration=None, HighFidelityVibration=null, GamepadVibration=null, Strength=1, PS5VibrationAsset=null}}
        vibrationDataAsset = <literal> = hornet_land_hard (VibrationDataAsset)
        motors = <literal> = None
        loopTime = <literal> = 0
        loopAuto = <literal> = False
        tag = <literal> =
        stopOnStateExit = <literal> = False
    action SetVelocity2d
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        vector = <literal> = (0.00, 0.00)
        x = <literal> = 0
        y = <literal> = 0
        everyFrame = True
    transition FINISHED -> Hit Roof
```

### Hit Roof Soft
```
    action SendMessage
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        delivery = SendMessage
        options = DontRequireReceiver
        functionCall = FunctionCall{FunctionName="RelinquishControl", BoolParameter=<literal> = False, FloatParameter=<literal> = 0, IntParameter=<literal> = 0, GameObjectParameter=<literal> = null, ObjectParameter=<literal> = null, StringParameter=<literal> = , Vector2Parameter=<literal> = (0.00, 0.00), Vector3Parameter=<literal> = (0.00, 0.00, 0.00), RectParamater=<literal> = (x:0.00, y:0.00, width:0.00, height:0.00), QuaternionParameter=<literal> = (0.00000, 0.00000, 0.00000, 0.00000), MaterialParameter=<literal> = null, TextureParameter=<literal> = null, ColorParameter=<literal> = RGBA(0.000, 0.000, 0.000, 1.000), EnumParameter=<literal> = None, ArrayParameter=<literal> = System.Object[], ParameterType="None"}
    action SendMessage
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        delivery = SendMessage
        options = DontRequireReceiver
        functionCall = FunctionCall{FunctionName="StopAnimationControl", BoolParameter=<literal> = False, FloatParameter=<literal> = 0, IntParameter=<literal> = 0, GameObjectParameter=<literal> = null, ObjectParameter=<literal> = null, StringParameter=<literal> = , Vector2Parameter=<literal> = (0.00, 0.00), Vector3Parameter=<literal> = (0.00, 0.00, 0.00), RectParamater=<literal> = (x:0.00, y:0.00, width:0.00, height:0.00), QuaternionParameter=<literal> = (0.00000, 0.00000, 0.00000, 0.00000), MaterialParameter=<literal> = null, TextureParameter=<literal> = null, ColorParameter=<literal> = RGBA(0.000, 0.000, 0.000, 1.000), EnumParameter=<literal> = None, ArrayParameter=<literal> = System.Object[], ParameterType="None"}
    action DoCameraShakeV2
        Target = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        MaxCameraDistance = <literal> = (0.00, 0.00)
        Camera = <literal> = Main Camera (CameraManagerReference)
        Profile = <literal> = Enemy Kill (CameraShakeProfile)
        DoFreeze = <literal> = False
        Delay = <literal> = 0
        CancelOnExit = False
    action Tk2dPlayAnimationWithEvents
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        clipName = <literal> = Super Jump Hit Roof Q
        animationTriggerEvent = null
        animationCompleteEvent = null
    action CallMethodProper
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Camera Target = Camera Target (UnityEngine.GameObject)}
        behaviour = <literal> = CameraTarget
        methodName = <literal> = SetSuperJump
        parameters = [FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = False, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=Bool, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}, FsmVar{variableName="Stick Needle Y", objectType="UnityEngine.Object", useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=Stick Needle Y = 0, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=Float, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}]
        storeResult = FsmVar{variableName="", objectType=null, useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4{x=0, y=0, z=0, w=0, normalized=Vector4, magnitude=0, sqrMagnitude=0}, objectReference=null, arrayValue=null, NamedVar=<literal> = False, NamedVarType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554527, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.FsmBool, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="FsmBool", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.FsmBool", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=False, IsSpecialName=False, IsClass=True, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=False, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData]}, EnumType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, EnumValue=None, ObjectType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, Type=Bool, RealType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554690, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", DeclaringType=null, Name="Boolean", Namespace="System", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="System.Boolean", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, SequentialLayout, Sealed, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=False, IsExplicitLayout=False, IsLayoutSequential=True, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=True, IsValueType=True, IsSignatureType=False, TypeInitializer=RuntimeConstructorInfo, IsInterface=False, CustomAttributes=[CustomAttributeData, CustomAttributeData]}, IsNone=True, vector2Value=Vector2{x=0, y=0, normalized=Vector2, magnitude=0, sqrMagnitude=0}, vector3Value=Vector3{x=0, y=0, z=0, normalized=Vector3, magnitude=0, sqrMagnitude=0}, colorValue=Color{r=0, g=0, b=0, a=0, grayscale=0, linear=Color, gamma=Color, maxColorComponent=0}, rectValue=Rect{x=0, y=0, position=Vector2, center=Vector2, min=Vector2, max=Vector2, width=0, height=0, size=Vector2, xMin=0, yMin=0, xMax=0, yMax=0, left=0, right=0, top=0, bottom=0}, quaternionValue=Quaternion{x=0, y=0, z=0, w=0, eulerAngles=Vector3, normalized=Quaternion}, gameObjectValue=null, materialValue=null, textureValue=null}
        EveryFrame = False
    action SendEventToRegister
        eventName = <literal> = SUPER JUMP ENDED
    transition FINISHED -> Hit Roof
```

### Hit Spikes
```
    action SetBoolValue
        boolVariable = Did Find Roof = False
        boolValue = <literal> = False
        everyFrame = False
    action DoCameraShakeV4
        Target = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        MaxCameraDistance = <literal> = (0.00, 0.00)
        Camera = <literal> = Main Camera (CameraManagerReference)
        Profile = <literal> = Small Shake (CameraShakeProfile)
        DoFreeze = <literal> = False
        Delay = <literal> = 0
        CancelOnExit = False
        vibrate = <literal> = True
    action GetPosition2D
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Throw Needle = Super Jump Needle Throw (UnityEngine.GameObject)}
        vector = <literal> = (0.00, 0.00)
        x = <literal> = 0
        y = Throw Needle Pos Y = 0
        space = World
        everyFrame = False
    action GetPosition2D
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=MainCamera = tk2dCamera (UnityEngine.GameObject)}
        vector = <literal> = (0.00, 0.00)
        x = <literal> = 0
        y = Main Cam Pos Y = 0
        space = World
        everyFrame = False
    action FloatAdd
        floatVariable = Main Cam Pos Y = 0
        add = <literal> = 10
        everyFrame = False
        perSecond = False
    action FloatCompare
        float1 = Throw Needle Pos Y = 0
        float2 = Main Cam Pos Y = 0
        tolerance = <literal> = 0
        equal = null
        lessThan = event:FINISHED
        greaterThan = null
        everyFrame = False
    action PlayRandomAudioClipTableV3
        Table = <literal> = tink_effect (RandomAudioClipTable)
        AudioPlayerPrefab = <literal> = Audio Player Actor Lowpass (UnityEngine.AudioSource)
        SpawnPoint = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Throw Needle = Super Jump Needle Throw (UnityEngine.GameObject)}
        SpawnPosition = <literal> = (0.00, 0.00, 0.00)
        ForcePlay = <literal> = False
        StoreSpawned = Spawned Audio Player = null
    action SetPosition2D
        GameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Spawned Audio Player = null}
        Vector = <literal> = (0.00, 0.00)
        X = <literal> = 0
        Y = Main Cam Pos Y = 0
        Space = World
        EveryFrame = False
    transition FINISHED -> Position Stick Needle Pre
```

### Get Distance
```
    action GetPosition
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Throw Needle = Super Jump Needle Throw (UnityEngine.GameObject)}
        vector = <literal> = (0.00, 0.00, 0.00)
        x = <literal> = 0
        y = Current Throw Needle Y = 0
        z = <literal> = 0
        space = World
        everyFrame = False
    action GetPosition
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Throw Needle Target = Move To (UnityEngine.GameObject)}
        vector = <literal> = (0.00, 0.00, 0.00)
        x = <literal> = 0
        y = Throw Needle Target Y = 0
        z = <literal> = 0
        space = World
        everyFrame = False
    action FloatOperator
        float1 = Throw Needle Target Y = 0
        float2 = Current Throw Needle Y = 0
        operation = Subtract
        storeResult = Throw Needle Distance = 0
        everyFrame = False
    action RayCast2dV2
        fromGameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        fromPosition = <literal> = (0.00, 0.00)
        direction = <literal> = (0.00, 1.00)
        space = World
        distance = <literal> = 350
        minDepth = <literal> = 0
        maxDepth = <literal> = 0
        hitEvent = null
        noHitEvent = null
        storeDidHit = Did Find Roof = False
        storeHitObject = Roof = null
        storeHitPoint = Ray Hit Point = (0.00, 0.00)
        storeHitNormal = <literal> = (0.00, 0.00)
        storeHitDistance = <literal> = 0
        storeDistance = Throw Needle Distance = 0
        repeatInterval = <literal> = 0
        layerMask = [<literal> = 8]
        invertMask = <literal> = False
        ignoreTriggers = <literal> = False
        debugColor = <literal> = RGBA(1.000, 0.922, 0.016, 1.000)
        debug = <literal> = True
    action SuperJumpRaycast
        FromGameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        FromPosition = <literal> = (0.00, 0.00)
        Direction = <literal> = (0.00, 1.00)
        Space = World
        Distance = <literal> = 350
        HitEvent = null
        NoHitEvent = null
        StoreDidHit = Did Find Roof = False
        StoreHitObject = Roof = null
        StoreHitPoint = Ray Hit Point = (0.00, 0.00)
        StoreDistance = Throw Needle Distance = 0
        StoreIsTransitionGate = Is Transition Gate = False
        StoreHitSpikes = <literal> = False
    action SetVector3XYZ
        vector3Variable = Throw Needle Move By = (0.00, 0.00, 0.00)
        vector3Value = <literal> = (0.00, 0.00, 0.00)
        x = <literal> = 0
        y = Throw Needle Distance = 0
        z = <literal> = 0
        everyFrame = False
    action FloatCompare
        float1 = Throw Needle Distance = 0
        float2 = <literal> = 12
        tolerance = <literal> = 0
        equal = event:FINISHED
        lessThan = event:FINISHED
        greaterThan = null
        everyFrame = False
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Thread = Super Jump Thread (UnityEngine.GameObject)}
        activate = <literal> = True
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    transition FINISHED -> Throw Needle
```

### Position Stick Needle Pre
```
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Throw Needle = Super Jump Needle Throw (UnityEngine.GameObject)}
        activate = <literal> = False
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Stick Needle = Super Jump Needle Stick (UnityEngine.GameObject)}
        activate = <literal> = True
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action BoolTest
        boolVariable = Did Find Roof = False
        isTrue = null
        isFalse = event:CANCEL
        everyFrame = False
    action CallStaticMethod
        className = <literal> = NoSuperJumpCollider
        methodName = <literal> = IsInside
        parameters = [FsmVar{variableName="Ray Hit Point", objectType="UnityEngine.Object", useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=Ray Hit Point = (0.00, 0.00), NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=Vector2, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}]
        storeResult = FsmVar{variableName="Test", objectType="UnityEngine.Object", useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4{x=0, y=0, z=0, w=0, normalized=Vector4, magnitude=0, sqrMagnitude=0}, objectReference=null, arrayValue=null, NamedVar=Test = False, NamedVarType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554527, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.FsmBool, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="FsmBool", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.FsmBool", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=False, IsSpecialName=False, IsClass=True, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=False, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData]}, EnumType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, EnumValue=None, ObjectType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33555133, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="UnityEngine.Object, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="Object", Namespace="UnityEngine", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="UnityEngine.Object", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo, RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeType], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[RuntimeType], DeclaredProperties=[RuntimePropertyInfo, RuntimePropertyInfo], ImplementedInterfaces=[], IsSerializable=False, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, SequentialLayout, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=False, IsSpecialName=False, IsClass=True, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=False, IsExplicitLayout=False, IsLayoutSequential=True, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=False, IsSignatureType=False, TypeInitializer=RuntimeConstructorInfo, IsInterface=False, CustomAttributes=[CustomAttributeData, CustomAttributeData, CustomAttributeData, CustomAttributeData]}, Type=Bool, RealType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554690, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", DeclaringType=null, Name="Boolean", Namespace="System", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="System.Boolean", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, SequentialLayout, Sealed, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=False, IsExplicitLayout=False, IsLayoutSequential=True, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=True, IsValueType=True, IsSignatureType=False, TypeInitializer=RuntimeConstructorInfo, IsInterface=False, CustomAttributes=[CustomAttributeData, CustomAttributeData]}, IsNone=False, vector2Value=Vector2{x=0, y=0, normalized=Vector2, magnitude=0, sqrMagnitude=0}, vector3Value=Vector3{x=0, y=0, z=0, normalized=Vector3, magnitude=0, sqrMagnitude=0}, colorValue=Color{r=0, g=0, b=0, a=0, grayscale=0, linear=Color, gamma=Color, maxColorComponent=0}, rectValue=Rect{x=0, y=0, position=Vector2, center=Vector2, min=Vector2, max=Vector2, width=0, height=0, size=Vector2, xMin=0, yMin=0, xMax=0, yMax=0, left=0, right=0, top=0, bottom=0}, quaternionValue=Quaternion{x=0, y=0, z=0, w=0, eulerAngles=Vector3, normalized=Quaternion}, gameObjectValue=null, materialValue=null, textureValue=null}
        everyFrame = False
    action BoolTest
        boolVariable = Test = False
        isTrue = event:CANCEL
        isFalse = null
        everyFrame = False
    action HasComponent
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Roof = null}
        component = <literal> = NoSuperJumpCollider
        removeOnExit = <literal> = False
        trueEvent = event:CANCEL
        falseEvent = null
        store = <literal> = False
        everyFrame = False
    action GetParent
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Stick Needle = Super Jump Needle Stick (UnityEngine.GameObject)}
        storeResult = Stick Needle Parent = null
    action SetTransformParent
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Stick Needle = Super Jump Needle Stick (UnityEngine.GameObject)}
        parent = <literal> = null
        worldPositionStays = <literal> = True
    action GetPosition2D
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Throw Needle = Super Jump Needle Throw (UnityEngine.GameObject)}
        vector = <literal> = (0.00, 0.00)
        x = Throw Needle X = 0
        y = <literal> = 0
        space = World
        everyFrame = False
    action SetPosition2D
        GameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Stick Needle = Super Jump Needle Stick (UnityEngine.GameObject)}
        Vector = Ray Hit Point = (0.00, 0.00)
        X = <literal> = 0
        Y = <literal> = 0
        Space = World
        EveryFrame = False
    action GetPosition2D
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Stick Needle = Super Jump Needle Stick (UnityEngine.GameObject)}
        vector = <literal> = (0.00, 0.00)
        x = Stick Needle X = 0
        y = Stick Needle Y = 0
        space = World
        everyFrame = False
    action FloatOperator
        float1 = Throw Needle X = 0
        float2 = Stick Needle X = 0
        operation = Subtract
        storeResult = Stick Needle Offset X = 0
        everyFrame = False
    action Translate
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Stick Needle = Super Jump Needle Stick (UnityEngine.GameObject)}
        vector = <literal> = (0.00, 0.00, 0.00)
        x = Stick Needle Offset X = 0
        y = <literal> = 0
        z = <literal> = 0
        space = World
        perSecond = False
        everyFrame = False
        lateUpdate = False
        fixedUpdate = False
    action BoolTest
        boolVariable = Is Transition Gate = False
        isTrue = event:TRANSITION GATE
        isFalse = null
        everyFrame = False
    action CheckOutOfCamera
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Stick Needle = Super Jump Needle Stick (UnityEngine.GameObject)}
        margin = <literal> = 0
        outsideEvent = event:FINISHED
        insideEvent = null
        insideBool = <literal> = False
        outsideBool = <literal> = False
        everyFrame = False
    action SpawnObjectFromGlobalPool
        gameObject = <literal> = Nail Terrain Hit Effect (UnityEngine.GameObject)
        spawnPoint = Stick Needle = Super Jump Needle Stick (UnityEngine.GameObject)
        position = <literal> = (0.00, 0.00, 0.00)
        rotation = <literal> = (0.00, 0.00, 0.00)
        storeObject = <literal> = null
    transition CANCEL -> Catch Wait
    transition TRANSITION GATE -> Hit Transition Gate
    transition FINISHED -> Position Stick Needle
```

### Cancel Rumbling Focus
```
    action BoolTest
        boolVariable = Started Rumbling Focus = False
        isTrue = null
        isFalse = event:FINISHED
        everyFrame = False
    action SetBoolValue
        boolVariable = Started Rumbling Focus = False
        boolValue = <literal> = False
        everyFrame = False
    action SetFsmBool
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=CameraParent = CameraParent (UnityEngine.GameObject)}
        fsmName = <literal> = CameraShake
        variableName = <literal> = RumblingFocus
        setValue = <literal> = False
        everyFrame = False
    transition FINISHED -> Cancel Rumbling Focus 2
```

### Cancel Rumbling Focus 2
```
    action BoolTest
        boolVariable = Started Rumbling Focus 2 = False
        isTrue = null
        isFalse = event:FINISHED
        everyFrame = False
    action SetBoolValue
        boolVariable = Started Rumbling Focus 2 = False
        boolValue = <literal> = False
        everyFrame = False
    action SetFsmBool
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=CameraParent = CameraParent (UnityEngine.GameObject)}
        fsmName = <literal> = CameraShake
        variableName = <literal> = RumblingFocus2
        setValue = <literal> = False
        everyFrame = False
    transition FINISHED -> Reset Effects
```

### Hit Transition Gate
```
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Stick Needle = Super Jump Needle Stick (UnityEngine.GameObject)}
        activate = <literal> = False
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action PlayAudioEvent
        audioClip = <literal> = hornet_superjump_pt_5_needle_impact_2d_distant (UnityEngine.AudioClip)
        pitchMin = <literal> = 0.95
        pitchMax = <literal> = 1.05
        volume = <literal> = 1
        audioPlayerPrefab = <literal> = Audio Player Actor 2D (UnityEngine.AudioSource)
        spawnPoint = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        spawnPosition = <literal> = (0.00, 10.00, 0.00)
        SpawnedPlayerRef = <literal> = null
    action BoolTest
        boolVariable = Played Throw Wait = False
        isTrue = event:FINISHED
        isFalse = null
        everyFrame = False
    action Tk2dWatchAnimationEvents
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        animationTriggerEvent = null
        animationCompleteEvent = event:FINISHED
    transition FINISHED -> Throw Wait
```

### Leaving Scene
```
    action BoolTest
        boolVariable = Is Transition Gate = False
        isTrue = null
        isFalse = event:CANCEL
        everyFrame = False
    transition CANCEL -> Cancel
    transition LEVEL LOADED -> Cancel
```

### Entered Jumping
```
    action WaitForFinishedEnteringScene
        sendEvent = event:FINISHED
    transition FINISHED -> Begin Jumping
```

### Begin Jumping
```
    action SendMessage
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        delivery = SendMessage
        options = DontRequireReceiver
        functionCall = FunctionCall{FunctionName="RelinquishControl", BoolParameter=<literal> = False, FloatParameter=<literal> = 0, IntParameter=<literal> = 0, GameObjectParameter=<literal> = null, ObjectParameter=<literal> = null, StringParameter=<literal> = , Vector2Parameter=<literal> = (0.00, 0.00), Vector3Parameter=<literal> = (0.00, 0.00, 0.00), RectParamater=<literal> = (x:0.00, y:0.00, width:0.00, height:0.00), QuaternionParameter=<literal> = (0.00000, 0.00000, 0.00000, 0.00000), MaterialParameter=<literal> = null, TextureParameter=<literal> = null, ColorParameter=<literal> = RGBA(0.000, 0.000, 0.000, 1.000), EnumParameter=<literal> = None, ArrayParameter=<literal> = System.Object[], ParameterType="None"}
    action SendMessage
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        delivery = SendMessage
        options = DontRequireReceiver
        functionCall = FunctionCall{FunctionName="StopAnimationControl", BoolParameter=<literal> = False, FloatParameter=<literal> = 0, IntParameter=<literal> = 0, GameObjectParameter=<literal> = null, ObjectParameter=<literal> = null, StringParameter=<literal> = , Vector2Parameter=<literal> = (0.00, 0.00), Vector3Parameter=<literal> = (0.00, 0.00, 0.00), RectParamater=<literal> = (x:0.00, y:0.00, width:0.00, height:0.00), QuaternionParameter=<literal> = (0.00000, 0.00000, 0.00000, 0.00000), MaterialParameter=<literal> = null, TextureParameter=<literal> = null, ColorParameter=<literal> = RGBA(0.000, 0.000, 0.000, 1.000), EnumParameter=<literal> = None, ArrayParameter=<literal> = System.Object[], ParameterType="None"}
    action SuperJumpRaycast
        FromGameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        FromPosition = <literal> = (0.00, 0.00)
        Direction = <literal> = (0.00, 1.00)
        Space = World
        Distance = <literal> = 350
        HitEvent = null
        NoHitEvent = null
        StoreDidHit = Did Find Roof = False
        StoreHitObject = Roof = null
        StoreHitPoint = Ray Hit Point = (0.00, 0.00)
        StoreDistance = Throw Needle Distance = 0
        StoreIsTransitionGate = Is Transition Gate = False
        StoreHitSpikes = Did Hit Spikes = False
    transition FINISHED -> Position Stick Needle Pre 2
```

### Position Stick Needle Pre 2
```
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Throw Needle = Super Jump Needle Throw (UnityEngine.GameObject)}
        activate = <literal> = False
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Stick Needle = Super Jump Needle Stick (UnityEngine.GameObject)}
        activate = <literal> = True
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action BoolTest
        boolVariable = Did Find Roof = False
        isTrue = null
        isFalse = event:CANCEL
        everyFrame = False
    action BoolTest
        boolVariable = Did Hit Spikes = False
        isTrue = event:CANCEL
        isFalse = null
        everyFrame = False
    action CallStaticMethod
        className = <literal> = NoSuperJumpCollider
        methodName = <literal> = IsInside
        parameters = [FsmVar{variableName="Ray Hit Point", objectType="UnityEngine.Object", useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=Ray Hit Point = (0.00, 0.00), NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=Vector2, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}]
        storeResult = FsmVar{variableName="Test", objectType="UnityEngine.Object", useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4{x=0, y=0, z=0, w=0, normalized=Vector4, magnitude=0, sqrMagnitude=0}, objectReference=null, arrayValue=null, NamedVar=Test = False, NamedVarType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554527, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.FsmBool, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="FsmBool", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.FsmBool", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=False, IsSpecialName=False, IsClass=True, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=False, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData]}, EnumType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, EnumValue=None, ObjectType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33555133, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="UnityEngine.Object, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="Object", Namespace="UnityEngine", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="UnityEngine.Object", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo, RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeType], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[RuntimeType], DeclaredProperties=[RuntimePropertyInfo, RuntimePropertyInfo], ImplementedInterfaces=[], IsSerializable=False, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, SequentialLayout, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=False, IsSpecialName=False, IsClass=True, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=False, IsExplicitLayout=False, IsLayoutSequential=True, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=False, IsSignatureType=False, TypeInitializer=RuntimeConstructorInfo, IsInterface=False, CustomAttributes=[CustomAttributeData, CustomAttributeData, CustomAttributeData, CustomAttributeData]}, Type=Bool, RealType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554690, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", DeclaringType=null, Name="Boolean", Namespace="System", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="System.Boolean", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, SequentialLayout, Sealed, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=False, IsExplicitLayout=False, IsLayoutSequential=True, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=True, IsValueType=True, IsSignatureType=False, TypeInitializer=RuntimeConstructorInfo, IsInterface=False, CustomAttributes=[CustomAttributeData, CustomAttributeData]}, IsNone=False, vector2Value=Vector2{x=0, y=0, normalized=Vector2, magnitude=0, sqrMagnitude=0}, vector3Value=Vector3{x=0, y=0, z=0, normalized=Vector3, magnitude=0, sqrMagnitude=0}, colorValue=Color{r=0, g=0, b=0, a=0, grayscale=0, linear=Color, gamma=Color, maxColorComponent=0}, rectValue=Rect{x=0, y=0, position=Vector2, center=Vector2, min=Vector2, max=Vector2, width=0, height=0, size=Vector2, xMin=0, yMin=0, xMax=0, yMax=0, left=0, right=0, top=0, bottom=0}, quaternionValue=Quaternion{x=0, y=0, z=0, w=0, eulerAngles=Vector3, normalized=Quaternion}, gameObjectValue=null, materialValue=null, textureValue=null}
        everyFrame = False
    action BoolTest
        boolVariable = Test = False
        isTrue = event:CANCEL
        isFalse = null
        everyFrame = False
    action HasComponent
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Roof = null}
        component = <literal> = NoSuperJumpCollider
        removeOnExit = <literal> = False
        trueEvent = event:CANCEL
        falseEvent = null
        store = <literal> = False
        everyFrame = False
    action GetParent
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Stick Needle = Super Jump Needle Stick (UnityEngine.GameObject)}
        storeResult = Stick Needle Parent = null
    action SetTransformParent
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Stick Needle = Super Jump Needle Stick (UnityEngine.GameObject)}
        parent = <literal> = null
        worldPositionStays = <literal> = True
    action GetPosition2D
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Throw Needle = Super Jump Needle Throw (UnityEngine.GameObject)}
        vector = <literal> = (0.00, 0.00)
        x = Throw Needle X = 0
        y = <literal> = 0
        space = World
        everyFrame = False
    action SetPosition2D
        GameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Stick Needle = Super Jump Needle Stick (UnityEngine.GameObject)}
        Vector = Ray Hit Point = (0.00, 0.00)
        X = <literal> = 0
        Y = <literal> = 0
        Space = World
        EveryFrame = False
    action GetPosition2D
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Stick Needle = Super Jump Needle Stick (UnityEngine.GameObject)}
        vector = <literal> = (0.00, 0.00)
        x = Stick Needle X = 0
        y = Stick Needle Y = 0
        space = World
        everyFrame = False
    action FloatOperator
        float1 = Throw Needle X = 0
        float2 = Stick Needle X = 0
        operation = Subtract
        storeResult = Stick Needle Offset X = 0
        everyFrame = False
    action Translate
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Stick Needle = Super Jump Needle Stick (UnityEngine.GameObject)}
        vector = <literal> = (0.00, 0.00, 0.00)
        x = Stick Needle Offset X = 0
        y = <literal> = 0
        z = <literal> = 0
        space = World
        perSecond = False
        everyFrame = False
        lateUpdate = False
        fixedUpdate = False
    action BoolTest
        boolVariable = Is Transition Gate = False
        isTrue = event:TRANSITION GATE
        isFalse = null
        everyFrame = False
    transition CANCEL -> Queue Cancel
    transition TRANSITION GATE -> Hit Transition Gate 2
    transition FINISHED -> Dash Start Quick
```

### Hit Transition Gate 2
```
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Stick Needle = Super Jump Needle Stick (UnityEngine.GameObject)}
        activate = <literal> = False
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    transition FINISHED -> Dash Start Quick
```

### Dash Start Quick
```
    action ActivateGameObject
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Damager = Super Jump Damager (UnityEngine.GameObject)}
        activate = <literal> = True
        recursive = <literal> = False
        resetOnExit = False
        everyFrame = False
    action Tk2dPlayAnimation
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        animLibName = <literal> =
        clipName = <literal> = Super Jump Loop
    action CallMethodProper
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        behaviour = <literal> = HeroController
        methodName = <literal> = SetCState
        parameters = [FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue="freezeCharge", vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = freezeCharge, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=String, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}, FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = False, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=Bool, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}]
        storeResult = FsmVar{variableName="", objectType=null, useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4{x=0, y=0, z=0, w=0, normalized=Vector4, magnitude=0, sqrMagnitude=0}, objectReference=null, arrayValue=null, NamedVar=<literal> = 0, NamedVarType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554531, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.FsmFloat, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="FsmFloat", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.FsmFloat", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=False, IsSpecialName=False, IsClass=True, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=False, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData]}, EnumType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, EnumValue=None, ObjectType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, Type=Float, RealType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554812, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="System.Single, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", DeclaringType=null, Name="Single", Namespace="System", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="System.Single", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType, RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, SequentialLayout, Sealed, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=False, IsExplicitLayout=False, IsLayoutSequential=True, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=True, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData, CustomAttributeData]}, IsNone=True, vector2Value=Vector2{x=0, y=0, normalized=Vector2, magnitude=0, sqrMagnitude=0}, vector3Value=Vector3{x=0, y=0, z=0, normalized=Vector3, magnitude=0, sqrMagnitude=0}, colorValue=Color{r=0, g=0, b=0, a=0, grayscale=0, linear=Color, gamma=Color, maxColorComponent=0}, rectValue=Rect{x=0, y=0, position=Vector2, center=Vector2, min=Vector2, max=Vector2, width=0, height=0, size=Vector2, xMin=0, yMin=0, xMax=0, yMax=0, left=0, right=0, top=0, bottom=0}, quaternionValue=Quaternion{x=0, y=0, z=0, w=0, eulerAngles=Vector3, normalized=Quaternion}, gameObjectValue=null, materialValue=null, textureValue=null}
        EveryFrame = False
    action CallMethodProper
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        behaviour = <literal> = HeroController
        methodName = <literal> = SetCState
        parameters = [FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue="superDashing", vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = superDashing, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=String, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}, FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=True, stringValue=null, vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = True, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=Bool, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}]
        storeResult = FsmVar{variableName="", objectType=null, useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4{x=0, y=0, z=0, w=0, normalized=Vector4, magnitude=0, sqrMagnitude=0}, objectReference=null, arrayValue=null, NamedVar=<literal> = 0, NamedVarType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554531, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.FsmFloat, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="FsmFloat", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.FsmFloat", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=False, IsSpecialName=False, IsClass=True, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=False, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData]}, EnumType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, EnumValue=None, ObjectType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, Type=Float, RealType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554812, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="System.Single, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", DeclaringType=null, Name="Single", Namespace="System", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="System.Single", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType, RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, SequentialLayout, Sealed, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=False, IsExplicitLayout=False, IsLayoutSequential=True, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=True, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData, CustomAttributeData]}, IsNone=True, vector2Value=Vector2{x=0, y=0, normalized=Vector2, magnitude=0, sqrMagnitude=0}, vector3Value=Vector3{x=0, y=0, z=0, normalized=Vector3, magnitude=0, sqrMagnitude=0}, colorValue=Color{r=0, g=0, b=0, a=0, grayscale=0, linear=Color, gamma=Color, maxColorComponent=0}, rectValue=Rect{x=0, y=0, position=Vector2, center=Vector2, min=Vector2, max=Vector2, width=0, height=0, size=Vector2, xMin=0, yMin=0, xMax=0, yMax=0, left=0, right=0, top=0, bottom=0}, quaternionValue=Quaternion{x=0, y=0, z=0, w=0, eulerAngles=Vector3, normalized=Quaternion}, gameObjectValue=null, materialValue=null, textureValue=null}
        EveryFrame = False
    action CallMethodProper
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Self = Hero_Hornet(Clone) (UnityEngine.GameObject)}
        behaviour = <literal> = HeroController
        methodName = <literal> = AffectedByGravity
        parameters = [FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = False, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=Bool, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}]
        storeResult = FsmVar{variableName="", objectType=null, useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4{x=0, y=0, z=0, w=0, normalized=Vector4, magnitude=0, sqrMagnitude=0}, objectReference=null, arrayValue=null, NamedVar=<literal> = 0, NamedVarType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554531, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.FsmFloat, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="FsmFloat", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.FsmFloat", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=False, IsSpecialName=False, IsClass=True, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=False, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData]}, EnumType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, EnumValue=None, ObjectType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, Type=Float, RealType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554812, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="System.Single, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", DeclaringType=null, Name="Single", Namespace="System", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="System.Single", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType, RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, SequentialLayout, Sealed, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=False, IsExplicitLayout=False, IsLayoutSequential=True, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=True, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData, CustomAttributeData]}, IsNone=True, vector2Value=Vector2{x=0, y=0, normalized=Vector2, magnitude=0, sqrMagnitude=0}, vector3Value=Vector3{x=0, y=0, z=0, normalized=Vector3, magnitude=0, sqrMagnitude=0}, colorValue=Color{r=0, g=0, b=0, a=0, grayscale=0, linear=Color, gamma=Color, maxColorComponent=0}, rectValue=Rect{x=0, y=0, position=Vector2, center=Vector2, min=Vector2, max=Vector2, width=0, height=0, size=Vector2, xMin=0, yMin=0, xMax=0, yMax=0, left=0, right=0, top=0, bottom=0}, quaternionValue=Quaternion{x=0, y=0, z=0, w=0, eulerAngles=Vector3, normalized=Quaternion}, gameObjectValue=null, materialValue=null, textureValue=null}
        EveryFrame = False
    action SetVelocity2d
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        vector = <literal> = (0.00, 0.00)
        x = <literal> = 0
        y = Jump Speed = 33
        everyFrame = False
    action CallMethodProper
        gameObject = FsmOwnerDefault{OwnerOption=SpecifyGameObject, GameObject=Camera Target = Camera Target (UnityEngine.GameObject)}
        behaviour = <literal> = CameraTarget
        methodName = <literal> = SetSuperJump
        parameters = [FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=True, stringValue=null, vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = True, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=Bool, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}, FsmVar{variableName="Stick Needle Y", objectType="UnityEngine.Object", useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=Stick Needle Y = 0, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=Float, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}]
        storeResult = FsmVar{variableName="", objectType=null, useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4{x=0, y=0, z=0, w=0, normalized=Vector4, magnitude=0, sqrMagnitude=0}, objectReference=null, arrayValue=null, NamedVar=<literal> = False, NamedVarType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554527, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.FsmBool, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="FsmBool", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.FsmBool", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=False, IsSpecialName=False, IsClass=True, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=False, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData]}, EnumType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, EnumValue=None, ObjectType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, Type=Bool, RealType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554690, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", DeclaringType=null, Name="Boolean", Namespace="System", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="System.Boolean", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, SequentialLayout, Sealed, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=False, IsExplicitLayout=False, IsLayoutSequential=True, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=True, IsValueType=True, IsSignatureType=False, TypeInitializer=RuntimeConstructorInfo, IsInterface=False, CustomAttributes=[CustomAttributeData, CustomAttributeData]}, IsNone=True, vector2Value=Vector2{x=0, y=0, normalized=Vector2, magnitude=0, sqrMagnitude=0}, vector3Value=Vector3{x=0, y=0, z=0, normalized=Vector3, magnitude=0, sqrMagnitude=0}, colorValue=Color{r=0, g=0, b=0, a=0, grayscale=0, linear=Color, gamma=Color, maxColorComponent=0}, rectValue=Rect{x=0, y=0, position=Vector2, center=Vector2, min=Vector2, max=Vector2, width=0, height=0, size=Vector2, xMin=0, yMin=0, xMax=0, yMax=0, left=0, right=0, top=0, bottom=0}, quaternionValue=Quaternion{x=0, y=0, z=0, w=0, eulerAngles=Vector3, normalized=Quaternion}, gameObjectValue=null, materialValue=null, textureValue=null}
        EveryFrame = False
    transition FINISHED -> Dashing
```

### Pre Entered Jumping
```
    action SetBoolValue
        boolVariable = Queued Cancel = False
        boolValue = <literal> = False
        everyFrame = False
    action CallMethodProper
        gameObject = FsmOwnerDefault{OwnerOption=UseOwner, GameObject=<literal> = null}
        behaviour = <literal> = HeroController
        methodName = <literal> = SetCState
        parameters = [FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=False, stringValue="superDashing", vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = superDashing, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=String, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}, FsmVar{variableName="", objectType=null, useVariable=False, floatValue=0, intValue=0, boolValue=True, stringValue=null, vector4Value=Vector4, objectReference=null, arrayValue=null, NamedVar=<literal> = True, NamedVarType=RuntimeType, EnumType=RuntimeType, EnumValue=None, ObjectType=RuntimeType, Type=Bool, RealType=RuntimeType, IsNone=False, vector2Value=Vector2, vector3Value=Vector3, colorValue=Color, rectValue=Rect, quaternionValue=Quaternion, gameObjectValue=null, materialValue=null, textureValue=null}]
        storeResult = FsmVar{variableName="", objectType=null, useVariable=True, floatValue=0, intValue=0, boolValue=False, stringValue=null, vector4Value=Vector4{x=0, y=0, z=0, w=0, normalized=Vector4, magnitude=0, sqrMagnitude=0}, objectReference=null, arrayValue=null, NamedVar=<literal> = 0, NamedVarType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554531, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.FsmFloat, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="FsmFloat", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.FsmFloat", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimeConstructorInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[RuntimePropertyInfo, RuntimePropertyInfo, RuntimePropertyInfo], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=False, IsSpecialName=False, IsClass=True, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=False, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData]}, EnumType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, EnumValue=None, ObjectType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=True, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554530, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="HutongGames.PlayMaker.None, PlayMaker, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null", DeclaringType=null, Name="None", Namespace="HutongGames.PlayMaker", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="HutongGames.PlayMaker.None", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, Sealed, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=True, IsExplicitLayout=False, IsLayoutSequential=False, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=False, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[]}, Type=Float, RealType=RuntimeType{Module=RuntimeModule, Assembly=RuntimeAssembly, TypeHandle=RuntimeTypeHandle, BaseType=RuntimeType, UnderlyingSystemType=RuntimeType, IsEnum=False, GenericParameterAttributes=<error: Exception has been thrown by the target of an invocation.>, IsGenericTypeDefinition=False, IsGenericParameter=False, GenericParameterPosition=<error: Exception has been thrown by the target of an invocation.>, IsGenericType=False, IsConstructedGenericType=False, MemberType=TypeInfo, ReflectedType=null, MetadataToken=33554812, StructLayoutAttribute=StructLayoutAttribute, ContainsGenericParameters=False, GUID=Guid, DeclaringMethod=<error: Exception has been thrown by the target of an invocation.>, AssemblyQualifiedName="System.Single, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", DeclaringType=null, Name="Single", Namespace="System", IsSecurityTransparent=True, IsSecurityCritical=False, IsSecuritySafeCritical=False, FullName="System.Single", IsSZArray=False, IsByRefLike=False, IsTypeDefinition=True, GenericTypeParameters=[], DeclaredConstructors=[], DeclaredEvents=[], DeclaredFields=[RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMembers=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo, RuntimeFieldInfo], DeclaredMethods=[RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo, RuntimeMethodInfo], DeclaredNestedTypes=[], DeclaredProperties=[], ImplementedInterfaces=[RuntimeType, RuntimeType, RuntimeType, RuntimeType, RuntimeType, RuntimeType], IsSerializable=True, IsVisible=True, IsNested=False, IsArray=False, IsByRef=False, IsPointer=False, IsGenericTypeParameter=False, IsGenericMethodParameter=False, IsVariableBoundArray=False, HasElementType=False, GenericTypeArguments=[], Attributes=AutoLayout, AnsiClass, Class, Public, SequentialLayout, Sealed, Serializable, BeforeFieldInit, IsAbstract=False, IsImport=False, IsSealed=True, IsSpecialName=False, IsClass=False, IsNestedAssembly=False, IsNestedFamANDAssem=False, IsNestedFamily=False, IsNestedFamORAssem=False, IsNestedPrivate=False, IsNestedPublic=False, IsNotPublic=False, IsPublic=True, IsAutoLayout=False, IsExplicitLayout=False, IsLayoutSequential=True, IsAnsiClass=True, IsAutoClass=False, IsUnicodeClass=False, IsCOMObject=False, IsContextful=False, IsCollectible=True, IsMarshalByRef=False, IsPrimitive=True, IsValueType=True, IsSignatureType=False, TypeInitializer=null, IsInterface=False, CustomAttributes=[CustomAttributeData, CustomAttributeData]}, IsNone=True, vector2Value=Vector2{x=0, y=0, normalized=Vector2, magnitude=0, sqrMagnitude=0}, vector3Value=Vector3{x=0, y=0, z=0, normalized=Vector3, magnitude=0, sqrMagnitude=0}, colorValue=Color{r=0, g=0, b=0, a=0, grayscale=0, linear=Color, gamma=Color, maxColorComponent=0}, rectValue=Rect{x=0, y=0, position=Vector2, center=Vector2, min=Vector2, max=Vector2, width=0, height=0, size=Vector2, xMin=0, yMin=0, xMax=0, yMax=0, left=0, right=0, top=0, bottom=0}, quaternionValue=Quaternion{x=0, y=0, z=0, w=0, eulerAngles=Vector3, normalized=Quaternion}, gameObjectValue=null, materialValue=null, textureValue=null}
        EveryFrame = False
    transition ENTER SUPERJUMPING -> Entered Jumping
```

### Queue Cancel
```
    action SetFloatValue
        floatVariable = Cancelable Time = 0.2
        floatValue = <literal> = 0.05
        everyFrame = False
    action SetBoolValue
        boolVariable = Queued Cancel = False
        boolValue = <literal> = True
        everyFrame = False
    transition FINISHED -> Dash Start Quick
```

## FSM variables (initial values at dump time)

```
FsmFloat Cancelable Time = 0.2
FsmFloat Charge Time = 0.8
FsmFloat Check Y = 0
FsmFloat Current Throw Needle Y = 0
FsmFloat Initial Throw Needle Y = 5.37
FsmFloat Jump Speed = 33
FsmFloat Main Cam Pos Y = 0
FsmFloat Speed = 0
FsmFloat Stick Needle Offset X = 0
FsmFloat Stick Needle X = 0
FsmFloat Stick Needle Y = 0
FsmFloat Throw Needle Distance = 0
FsmFloat Throw Needle Pos Y = 0
FsmFloat Throw Needle Target Y = 0
FsmFloat Throw Needle X = 0
FsmFloat Throw Wait Time = 0
FsmFloat Y Speed = 0
FsmFloat Throw Needle Pos Y Start = 0
FsmInt Current Silk = 0
FsmInt Silk Cost = 1
FsmInt Sprite Flash ID = 0
FsmBool Did Find Roof = False
FsmBool Is Distant = False
FsmBool Needle Visible = False
FsmBool On Ground = False
FsmBool Played Throw Wait = False
FsmBool Show Thread = False
FsmBool Terrain Above = False
FsmBool Did Add Using Silk = False
FsmBool Test = False
FsmBool Did Start Flash = False
FsmBool Started Rumbling Focus = False
FsmBool Started Rumbling Focus 2 = False
FsmBool Is Transition Gate = False
FsmBool Queued Cancel = False
FsmBool Did Hit Spikes = False
FsmVector2 Ray Hit Point = (0.00, 0.00)
FsmVector3 Throw Needle Move By = (0.00, 0.00, 0.00)
FsmGameObject Antic Effect L = Super Jump Antic Effect L (UnityEngine.GameObject)
FsmGameObject Antic Effect R = Super Jump Antic Effect R (UnityEngine.GameObject)
FsmGameObject Attacks Folder = Special Attacks (UnityEngine.GameObject)
FsmGameObject Camera Target = Camera Target (UnityEngine.GameObject)
FsmGameObject Charged Effect = Super Jump Charged (UnityEngine.GameObject)
FsmGameObject Charging Fader = Super Jump Charging Fader (UnityEngine.GameObject)
FsmGameObject Damager = Super Jump Damager (UnityEngine.GameObject)
FsmGameObject Effects Folder = Effects (UnityEngine.GameObject)
FsmGameObject Grab Effect = Super Jump Catch Effect (UnityEngine.GameObject)
FsmGameObject Self = Hero_Hornet(Clone) (UnityEngine.GameObject)
FsmGameObject Stick Needle = Super Jump Needle Stick (UnityEngine.GameObject)
FsmGameObject Stick Needle Parent = null
FsmGameObject Throw Needle = Super Jump Needle Throw (UnityEngine.GameObject)
FsmGameObject Throw Needle Fall = Super Jump Needle Throw Fall (UnityEngine.GameObject)
FsmGameObject Throw Needle Fall Target = Move To (UnityEngine.GameObject)
FsmGameObject Throw Needle Target = Move To (UnityEngine.GameObject)
FsmGameObject Superjump Audio Loop = Superjump Loop (UnityEngine.GameObject)
FsmGameObject Roof = null
FsmGameObject Charge Audio = null
FsmGameObject Thread = Super Jump Thread (UnityEngine.GameObject)
FsmGameObject Spawned Audio Player = null
FsmGameObject Extra Throw Effect = Super Jump Extra Throw Effect (UnityEngine.GameObject)
FsmGameObject Extra Ground Effect = Super Jump Extra Ground Effect (UnityEngine.GameObject)
FsmGameObject Nail Art Ready = Nail Art Ready (UnityEngine.GameObject)
FsmGameObject Throw Needle Damager = Damager (UnityEngine.GameObject)
FsmGameObject Thread Loop = Super Jump Thread Loop (UnityEngine.GameObject)
FsmObject Clip = null
```
