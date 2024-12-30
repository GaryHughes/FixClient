using System.Collections.Generic;
using System.Linq;

namespace Fix;

public static partial class Dictionary
{
    public class VersionCollection : IEnumerable<Version>
    {
        public readonly Version FIX_4_0 = new Version("FIX.4.0", Fix.Dictionary.FIX_4_0.DataTypes, Fix.Dictionary.FIX_4_0.Fields, Fix.Dictionary.FIX_4_0.Messages);
        public readonly Version FIX_4_2 = new Version("FIX.4.2", Fix.Dictionary.FIX_4_2.DataTypes, Fix.Dictionary.FIX_4_2.Fields, Fix.Dictionary.FIX_4_2.Messages);
        public readonly Version FIX_4_4 = new Version("FIX.4.4", Fix.Dictionary.FIX_4_4.DataTypes, Fix.Dictionary.FIX_4_4.Fields, Fix.Dictionary.FIX_4_4.Messages);
        public readonly Version FIX_5_0SP2 = new Version("FIX.5.0SP2", Fix.Dictionary.FIX_5_0SP2.DataTypes, Fix.Dictionary.FIX_5_0SP2.Fields, Fix.Dictionary.FIX_5_0SP2.Messages);
        // There isn't a separate transport orchestra and generating one from the repository doesn't work
        // due to errors in the repository so just cheat. It's only the session messages so this gives us
        // what we need.
        public readonly Version FIXT_1_1 = new Version("FIXT.1.1", Fix.Dictionary.FIX_5_0SP2.DataTypes, Fix.Dictionary.FIX_5_0SP2.Fields, Fix.Dictionary.FIX_5_0SP2.Messages);

        public VersionCollection()
        {
            Default = FIX_5_0SP2;

            _versions = new[] {
                FIX_4_0,
                FIX_4_2,
                FIX_4_4,
                FIX_5_0SP2,
                FIXT_1_1
            };
        }


        public Version? this[string beginString]
        {
            get
            {
                return (from version in _versions
                        where version.BeginString == beginString
                        select version).FirstOrDefault();
            }
        }

        public Version Default { get; }

        public int Count => _versions.Length;

        public virtual IEnumerator<Version> GetEnumerator()
        {
            foreach (var version in _versions)
            {
                yield return version;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        Version[] _versions;
    }

    public static VersionCollection Versions { get; } = new VersionCollection();
}
