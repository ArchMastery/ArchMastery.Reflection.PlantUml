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
        Relationships = 16,
        Inheritance = 32,
        Notes = 64,
        Members = Type + Public + NonPublic + Inheritance,
        All = Type + Public + NonPublic + Relationships + Inheritance + Notes
    }
}