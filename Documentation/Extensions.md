# Extensions

Aknowledging the limitations of the Builder system, the Beta is going to be implementing an `Extensions System` to increase the possible complexity

Currently there is
* `Auto align`
* `Spawn Piece Target`
* `Attach At Startup`

### Auto Align
 * This script is added on the same object as build frame, and has settings to align, it is NOT season specific and as such usis a simple set of options

### Spawn Piece Target
 * can be added to any object and simply serves as a reference point for spawn stations on the field to target when spawning.

### Attach at Startup
* attach at startup is used for advanced Physics stuff. To minimize failures builder generates all physics elements at runtime, so this script is required to attach custom rigidbody elements to a robot.

### Future Extensions
* `Aim At point` which will point the drivetrain to a point
* `point at point` which will tell an arm to point at a target
* `SymmetryPlane` which will create a symmetric version of specific children


[Further Reading](FurtherReading.md)
