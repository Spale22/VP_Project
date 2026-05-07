using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public enum WindRotationDirection
    {
        [EnumMember]
        Clockwise,
        [EnumMember]
        CounterClockwise
    }

    [DataContract]
    public enum LateralDirection
    {
        [EnumMember]
        Left,
        [EnumMember]
        Right
    }

    [DataContract]
    public enum DeviationType
    {
        [EnumMember]
        Below,
        [EnumMember]
        Above
    }

    [DataContract]
    public enum SessionStatus
    {
        [EnumMember]
        IN_PROGRESS,
        [EnumMember]
        COMPLETED
    }

    [DataContract]
    public enum AckStatus
    {
        [EnumMember]
        ACK,
        [EnumMember]
        NACK
    }

    [DataContract]
    public enum WarningType
    {
        [EnumMember]
        WindDirectionShift,
        [EnumMember]
        OutOfBand,
        [EnumMember]
        LateralAsymmetry
    }
}
