# View.Animation

## View.Animation.Recipe
This class is a scriptable object.

Represents one fake animation made of sequential phases.

This should be authored against `View.Animation.LogicalPart`, not exact transform names.
It should be reusable across several creatures that share a similar animation intent.

Composed of:
- display name
- phases
- loop flag if relevant

## View.Animation.Set
This class is a scriptable object.

Represents the full set of animation recipes used by one creature form, one archetype, or one model prefab.

This is likely the main asset you will assign to a `Creature.Form`.
It should act as a named collection of recipes that the animation system can query by string name.
That matches a runtime flow like "play animation `AttackMelee` on creature view X" without introducing a dedicated request class in the model.
Every creature should have an animation set, and every set should at minimum contain the standard animations needed by the game flow.
Those mandatory names should be:
- `Idle`
- `TakeDamage`
- `Death`

Ability definitions that request caster animations should use names expected to exist inside this set.

Composed of:
- serialized dictionary from animation name to recipe
- idle animation name, to allow the animation to return to a specific animation loop when ending the others

## View.Animation.Phase
This class is serializable.

Represents one sequential phase of a recipe.

All steps inside one phase should run in parallel.
Phases themselves should run one after another.

Composed of:
- duration
- steps

## View.Animation.Step
This class is serializable.

Represents one primitive fake-animation operation.

This should be the abstract base of a polymorphic step hierarchy, authored with managed-reference serialization and a custom inspector.
The custom inspector should let the creator choose the concrete step subtype, then expose only the fields relevant to that subtype.

Examples of concrete step subclasses:
- `View.Animation.MoveLocalStep`
- `View.Animation.RotateLocalStep`
- `View.Animation.ScaleStep`
- `View.Animation.ShakeStep`
- `View.Animation.FlashStep`
- `View.Animation.WaitStep`
- `View.Animation.SpawnVfxStep`
- `View.Animation.PlaySoundStep`

Composed of:
- the data shared by all step subtypes if any

## View.Animation.MoveLocalStep
This class is serializable.

Represents a local position offset applied to one logical part.

Composed of:
- target logical part
- local offset
- easing curve
- additive flag if relevant

## View.Animation.RotateLocalStep
This class is serializable.

Represents a local rotation offset applied to one logical part.

Composed of:
- target logical part
- local rotation offset
- easing curve
- additive flag if relevant

## View.Animation.ScaleStep
This class is serializable.

Represents a local scale offset applied to one logical part.

This is especially useful for blob, squash, or stretch-style fake animation.

Composed of:
- target logical part
- local scale multiplier or offset
- easing curve
- additive flag if relevant

## View.Animation.ShakeStep
This class is serializable.

Represents a short shake or jitter applied to one logical part or to the whole rig.

Composed of:
- target logical part
- amplitude
- frequency
- easing curve

## View.Animation.FlashStep
This class is serializable.

Represents a temporary visual flash on the model.

This is useful for hit reacts, charge-up cues, or ability feedback.

Composed of:
- target scope such as whole rig or specific part
- color or flash style
- intensity
- easing curve

## View.Animation.WaitStep
This class is serializable.

Represents a phase step that only consumes time without modifying transforms.

Composed of:
- optional note or label if relevant

## View.Animation.SpawnVfxStep
This class is serializable.

Represents a step that spawns a VFX on a logical part or world anchor during a recipe.

Composed of:
- target logical part or anchor
- VFX reference
- local offset if relevant

## View.Animation.PlaySoundStep
This class is serializable.

Represents a step that plays a sound during a recipe.

Composed of:
- sound reference
- target logical part or world anchor if relevant

## View.Animation.LogicalPart
Represents the enum-like list of logical targets used by fake animations instead of directly targeting concrete transform names.

This is what makes one recipe reusable across different creatures.
Animations should be authored against these logical parts, not against exact transform paths inside one prefab.

Examples:
- root
- body
- head
- front
- rear
- dominant limb
- off limb
- weapon
- jaw
- tail
- whole rig

## View.Animation.Rig
This class is a MonoBehaviour.

Represents the runtime component attached to the creature model prefab.

This is the object that maps logical animation parts to real transforms.
With a serializable dictionary available in Unity, this can directly store a dictionary from `LogicalPart` to `Transform`.
If a logical part is not present in that dictionary, it simply means that this model does not expose that part for animation, and any step targeting it should be skipped.
It should also capture the default local pose of each bound part so fake animations can return cleanly to rest.
That is what allows the same recipe to work on different body types without a separate capability system.

Composed of:
- serialized dictionary from logical part to transform

## View.Animation.Animator
This class is a MonoBehaviour.

Represents the runtime component that executes animation recipes on an instantiated creature view.

This should read the rig, resolve logical parts, apply steps relative to the captured rest pose, and restore parts cleanly when phases end.
It should stay simple at first:
- one main animation channel
- optional one additive overlay channel for hit flashes, recoil, or other short reAbilitys

Board movement itself should usually stay outside this recipe system.
The creature view can be moved from tile to tile by a separate movement tween, while the animation animator only adds body bob, lunge, recoil, squash, and similar fake-animation offsets.

Composed of:
- animation rig
- animation set
- current main recipe state
- current overlay recipe state if relevant
- runtime pose offsets