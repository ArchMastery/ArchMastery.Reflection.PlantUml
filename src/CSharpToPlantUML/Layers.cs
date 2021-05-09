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
        Members = Type | Public | NonPublic,      // 7
        TypeEnd = Members,
        InnerObjects = TypeEnd,
        Relationships = 16,
        Inheritance = 32,
        Notes = 64,
        All = Members | Relationships | Inheritance | Notes
    }
}