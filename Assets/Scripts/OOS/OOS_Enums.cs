namespace OOS
{
    public enum Language
    {
        Csharp,
        Java,
        Cplusplus,
        Undefined // This is used, when no language was detected.
    }

    public enum AccessModifier
    {
        Public, // C#, Java, C++
        Private, // C#, Java, C++
        Protected, // C#, Java, C++
        PrivateProtected, // C#
        Internal // C#
    }
}
