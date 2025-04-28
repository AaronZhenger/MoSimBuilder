namespace Util
{
    /// <summary>
    /// The Swerve Module style to use
    /// </summary>
    public enum ModuleType
    {
        invertedCorner,
        standardCorner,
        inverted,
        standard,
        lowProfile
    }

    /// <summary>
    /// The type of setpoint that is being used
    /// </summary>
    public enum ControlType
    {
        Toggle,
        Hold,
        Sequence
    }

    public enum elevatorType
    {
        Cascade,
        Continuous
    }

    /// <summary>
    /// The continuation requirement for the sequence type of setpoint
    /// </summary>
    public enum SequenceType
    {
        nextPress,
        delay
    }

    /// <summary>
    /// Tube sizing names.
    /// </summary>
    public enum TubeType
    {
        OneXTwoXEighth,
        TwoXTwoXEighth,
        OneXOneXEighth
    }

    /// <summary>
    /// Units that can be used to generate Parts.
    /// </summary>
    public enum Units
    {
        Inch,
        Meter,
        Centimeter,
        Millimeter
    }
}