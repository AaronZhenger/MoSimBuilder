# Setpoints

Builder Beta uses a setpoint based system.

All setpoints have a name, and a target

The name helps organize things as this extends onto the dropdown headers,
The target is where it goes when active

To sequence events you string together setpoints using the following provided tools:
* Hold Setpoints
* Last Pressed Setpoints
* Toggle
* Sequences

Hold setpoints require you to hold a button in order to remain at that setpoint,

Last Pressed, goes to and stays at the setpoint of the last pressed button, however changes immediately on the next.

Toggle, goes to a setpoint and requires that button bre pressed again for anything to happen.

Sequences are made up of Sequence Start, Sequence, and severeal sub Types of Sequence
* To start a sequence add a sequence start,
* Then add another setpoint thats also a sequence and give it a name
* now on your sequence start, in the sequence to section, type the name again, and set the Sequence type to Next press
    * this causes the setpoint to move to the typed one when the button on the typed one is pressed.
    * alternatively, you can use delay, which gives a timer before moving.
 * if you add another and sequence to it but set it to end, it will add another option,
 * If you select persist, the setpoint will not move from the previous target, THis is helpful for keeping mutliple mechs in sync,
 * End also allows for moving between setpoints again as sequences lock into that sequence until done.

[Further Reading](FurtherReading.md)
