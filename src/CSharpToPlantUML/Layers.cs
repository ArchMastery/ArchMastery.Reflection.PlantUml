using System;

namespace CSharpToPlantUML
{
    [Flags]
    public enum Layers
    {
        Type, Public, Protected, Private, Relationships, Inheritance, Notes, All
    }
}