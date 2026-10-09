---
date: 2022-04-11
description: A study of procedural character locomotion, FABRIK inverse kinematics, and Unity Animation Rigging.
layout: default
title: Is It Worth Implementing a Custom Inverse Kinematic System?
---

# Is It Worth Implementing a Custom Inverse Kinematic System?

> **Original article:** [HVA GPE Game Lab — Spring 2022](https://summit-2122-sem2.game-lab.nl/2022/04/11/is-it-worth-implementing-a-custom-inverse-kinematic-system/)
>
> **Source code:** [RicoCoelen/Physical-Player-Body-Unity](https://github.com/RicoCoelen/Physical-Player-Body-Unity)

## Overview

This project explores whether a custom inverse-kinematics (IK) implementation is worth the development effort when Unity already provides animation-rigging tools. The focus is procedural locomotion for a humanoid character, especially reactive foot placement and foot alignment with uneven ground.

## Motivation and inspiration

Hand-authoring animations for every possible interaction can be time-consuming. Procedural techniques can help characters respond to their environment—for example, by adjusting their feet to the ground rather than relying entirely on fixed animation clips.

The project was inspired in part by the character locomotion presented in *Half-Life: Alyx*, where movement and foot placement respond to the character's path and surroundings.

- [Character Locomotion in Half-Life: Alyx — SIGGRAPH 2021](https://www.youtube.com/watch?v=RCu-NzH4zrs)

## Inverse kinematics and FABRIK

Inverse kinematics calculates joint positions so that an end effector—such as a hand or foot—reaches a target position. This is useful for procedural animation because the target can be driven by the game world.

The custom solver uses **FABRIK** (*Forward And Backward Reaching Inverse Kinematics*). In broad terms, it repeatedly adjusts joint positions from the end effector toward the root and then from the root back toward the end effector, while maintaining bone lengths. A separate rotation step is needed to orient the joints and rendered character correctly.

The original project discusses these stages:

- Main solver and target positioning
- Backward and forward reaching passes
- Hint directions to influence bending
- Joint rotation handling

See the [original article](https://summit-2122-sem2.game-lab.nl/2022/04/11/is-it-worth-implementing-a-custom-inverse-kinematic-system/) for the accompanying code screenshots and demonstrations.

## Procedural locomotion

The locomotion prototype uses the character's Rigidbody velocity to help determine foot targets. Targets are constrained to a chosen range, and interpolation smooths the transition between positions.

The prototype also uses downward raycasts to detect the ground. The raycast hit normal is used to orient each foot so it better follows the floor. The author reports that this improves the visual result, although the movement pattern and foot motion would benefit from further iteration.

The system combines several ideas:

- Velocity-informed foot placement
- A resting position for each foot
- Interpolation between foot targets
- Ground detection using raycasts
- Foot orientation based on the ground normal
- Movement-dependent offsets updated during `FixedUpdate()`

## Comparison with Unity Animation Rigging

The project compares the custom solver with Unity's **Animation Rigging** package, using a `TwoBoneIKConstraint` for the package-based version.

| Consideration | Custom IK implementation | Unity Animation Rigging |
| --- | --- | --- |
| Implementation effort | More time spent building and tuning the solver | Faster to set up |
| Control | Full control over the solver and its behavior | Uses the package's provided constraints |
| Rotation constraints | Requires additional implementation and tuning | Supported by the selected constraint workflow |
| Practical result in the test | Functional, but with areas that could be improved | Looked slightly better in the author's comparison |

In the author's informal comparison, four game developers did not immediately identify a clear difference between the two versions. The article therefore recommends spending less time rebuilding IK infrastructure and more time improving the overall quality of locomotion.

## Conclusion

For most projects, an existing IK solution—such as Unity Animation Rigging—is the more practical choice. It reduces implementation time and avoids having to solve difficult joint-rotation and constraint problems from scratch.

A custom implementation can still make sense when:

- The project has requirements that existing tools cannot meet.
- You need unusually fine-grained control over joint behavior.
- Building an IK solver is itself a learning or research goal.
- You have the time and technical experience to maintain and improve it.

**Main takeaway:** choose the built-in package for efficiency unless a specific requirement justifies the extra complexity of a custom solver.

## Possible future improvements

The original project identifies several directions for further work:

- Extend the approach from legs to arms and interactions with objects such as door handles.
- Add more robust joint-rotation constraints.
- Improve foot motion with heel/toe offsets.
- Add a clearer stepping arc so feet appear to lift from the ground.
- Extend the approach beyond bipedal humanoid characters.

## References

1. [Character Locomotion in Half-Life: Alyx — SIGGRAPH 2021](https://www.youtube.com/watch?v=RCu-NzH4zrs)
2. [C# Inverse Kinematics in Unity](https://www.youtube.com/watch?v=qqOAzn05fvk&t=428s)
3. Karim, A. A., Gaudin, T., Meyer, A., Buendia, A., & Bouakaz, S. (2012). "Procedural locomotion of multilegged characters in dynamic environments." *Computer Animation and Virtual Worlds*, 24(1), 3–15. <https://doi.org/10.1002/cav.1467>
4. [Inverse Kinematics for Procedural Animation — HVA GPE Game Lab, Fall 2021](https://summit2021b.game-lab.nl/2021/10/25/inverse-kinematics-for-procedural-animation/)
5. [iRadEntertainment: FABRIK experiments for procedural animation](https://www.reddit.com/r/godot/comments/o0jkpe/my_tests_with_inverse_kinematic_using_fabrik_for/)
6. [Johansen — Locomotion System](https://runevision.com/tech/locomotion/)
