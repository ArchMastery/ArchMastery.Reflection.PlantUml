using System;

namespace CSharpToPlantUML
{
    [Flags]
    public enum Layers
    {
        None = 0,
        Type = 1,
        Public = 2,
        NonPublic = 4,
        Members = Type + Public + NonPublic,      // 7
        TypeEnd = Members + 8,
        InnerObjects = 16,
        Relationships = 32,
        Inheritance = 64,
        Notes = 128,
        All = Members + Relationships + Inheritance + Notes
    }
}