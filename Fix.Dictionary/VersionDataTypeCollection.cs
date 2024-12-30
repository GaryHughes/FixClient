using System.Collections.Generic;
using System.Linq;

namespace Fix;

public static partial class Dictionary
{
    public class VersionDataTypeCollection : IEnumerable<DataType>
    {
        public VersionDataTypeCollection(params DataType[] dataTypes)
        {
            _dataTypes = dataTypes;
        }

        public bool TryGetValue(string name, out DataType? type)
        {
            type = (from item in DataTypes where item.Name == name select item).FirstOrDefault();
            return type != null;
        }

        // Call this Count because there is FIX data type called Length that will live in the same scope
        // in derived classes. 
        public virtual int Count => DataTypes.Length;

        public virtual IEnumerator<DataType> GetEnumerator()
        {
            foreach (var dataType in DataTypes)
            {
                yield return dataType;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        public DataType[] DataTypes
        {
            get
            {
                return _dataTypes;
            }
            protected set
            {
                _dataTypes = value;
            }
        }

        protected DataType[] _dataTypes = new DataType[] { };
    }
}
