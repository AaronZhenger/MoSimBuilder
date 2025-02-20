namespace Util
{
    public enum ModuleType
    {
        invertedCorner,
        standardCorner,
        inverted,
        standard,
        lowProfile
    }

    public enum ControlType
    {
        Toggle,
        Hold,
        Sequence
    }

    public enum SequenceType
    {
        nextPress,
        delay
    }

    public enum TubeType
    {
        OneXTwoXEighth,
        TwoXTwoXEighth,
        OneXOneXEighth
    }

    public enum Units
    {
        Inch,
        Meter,
        Centimeter,
        Millimeter
    }
}