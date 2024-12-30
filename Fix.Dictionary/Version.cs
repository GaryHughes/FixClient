global using System;

namespace Fix;

public static partial class Dictionary
{
    public class Version
    {
        public Version(string beginString,
                        VersionDataTypeCollection dataTypes,
                        VersionFieldCollection fields,
                        VersionMessageCollection messages)
        {
            BeginString = beginString;
            DataTypes = dataTypes;
            Fields = fields;
            Messages = messages;
        }

        public string BeginString { get; }
        public VersionDataTypeCollection DataTypes { get; }
        public VersionFieldCollection Fields { get; }
        public VersionMessageCollection Messages { get; }

        public string ApplVerID
        {
            get
            {
                return "";
            }
        }

        public override string ToString() => BeginString;

    }
}
