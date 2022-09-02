namespace Src2Neo.Neo4j
{
    /// <summary>
    /// This class contains string values that the <see cref="NeoWriter"/> uses to generate the Cypher querries.
    /// </summary>
    public static class CypherSettings
    {
        // Namespace
        public const string NamespaceTypeTag = "Namespace";
        public const string NamespaceNameTag = "Name";
        public const string NamespaceContainsClassTag = "CONTAINS";
        public const string NamespaceContainsInterfaceTag = "CONTAINS";
        public const string NamespaceContainsEnumTag = "CONTAINS";
        // Class
        public const string ClassTypeTag = "Class";
        public const string ClassNameTag = "Name";
        public const string ClassContainsFunctionTag = "CONTAINS";
        public const string ClassContainsObjectTag = "CONTAINS";
        public const string ClassImportsNamespaceTag = "IMPORTS";
        public const string ClassImplementsInterfaceTag = "IMPLEMENTS";
        public const string ClassInheritsFromClassTag = "EXTENDS";
        // Interface
        public const string InterfaceTypeTag = "Interface";
        public const string InterfaceNameTag = "Name";
        public const string InterfaceContainsFunctionTag = "CONTAINS";
        public const string InterfaceContainsObjectTag = "CONTAINS";
        public const string InterfaceImplementsInterfaceTag = "IMPLEMENTS";
        // Enum
        public const string EnumTypeTag = "Enum";
        public const string EnumNameTag = "Name";
        public const string EnumContainsMemberTag = "CONTAINS";
        // Method
        public const string FunctionTypeTag = "Method";
        public const string FunctionNameTag = "Name";
        public const string FunctionContainsObjectTag = "CONTAINS";
        public const string FunctionCallsFunctionTag = "CALLS";
        // Object
        public const string ObjectTypeTag = "Object";
        public const string ObjectNameTag = "Name";
        public const string ObjectTypeOfTag = "TYPEOF";
    }
}
