/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: FieldCollection.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using static Fix.Dictionary;

namespace Fix;

public class FieldCollection : IEnumerable<Field>
{
    //
    // We need a much smarter data structure here but this will do to get things working and
    // define the interface
    //
    readonly List<Field> _fields = new();

    public FieldCollection()
    {
    }

    public FieldCollection(string[,] data)
    {
        for (int index = 0; index < data.Length / 2; ++index)
        {
            if (!int.TryParse(data[index, 0], out int tag))
            {
                throw new ArgumentException($"Non numeric tag '{data[index, 0]}={data[index, 1]}'");
            }

            Add(new Field(tag, data[index, 1]));
        }
    }

    public void Set(Field value)
    {
        foreach (Field field in _fields)
        {
            if (field.Tag == value.Tag)
            {
                field.Value = value.Value;
                return;
            }
        }
        Add(value);
    }

    public void Set(int tag, string value) => Set(new Field(tag, value));
    public void Set(int tag, int value) => Set(new Field(tag, value));
    public void Set(int tag, long value) => Set(new Field(tag, value));
    public void Set(int tag, decimal value) => Set(new Field(tag, value));
    public void Set(int tag, bool value) => Set(new Field(tag, value));

    public void Set(FieldValue value) => Set(new Field(value.Tag, value.Value));
    public void Set(VersionField field, string value) => Set(new Field(field, value));
    public void Set(VersionField field, int value) => Set(new Field(field, value));
    public void Set(VersionField field, long value) => Set(new Field(field, value));
    public void Set(VersionField field, decimal value) => Set(new Field(field, value));
    public void Set(VersionField field, bool value) => Set(new Field(field, value));
    public void Set(VersionField field, Field value) => Set(new Field(field.Tag, value.Value));



    public void Add(int tag, string value) => Add(new Field(tag, value));
    public void Add(int tag, int value) => Add(new Field(tag, value));
    public void Add(int tag, long value) => Add(new Field(tag, value));
    public void Add(int tag, decimal value) => Add(new Field(tag, value));
    public void Add(int tag, bool value) => Add(new Field(tag, value));


    public void Add(Field field)
    {
        _fields.Add(field);
    }

    public void Insert(int index, Field value)
    {
        _fields.Insert(index, value);
    }

    public int Count => _fields.Count;

    public bool TryGetValue(VersionField definition, out Field result)
    {
        return TryGetValue(definition.Tag, out result);
    }

    public bool TryGetValue(int tag, out Field result)
    {
        if (_fields is null)
        {
            result = Field.Invalid;
            return false;
        }

        var found = (from field in _fields
                     where field.Tag == tag
                     select field).FirstOrDefault();

        if (found is null)
        {
            result = Field.Invalid;
            return false;
        }

        result = found;

        return true;
    }
    public Field? TryGetValue(Dictionary.VersionField definition)

    {
        return TryGetValue(definition.Tag);
    }

    public Field? TryGetValue(int tag)
    {
        if (TryGetValue(tag, out var field))
        {
            return field;
        }

        return null;
    }


    public bool Contains(VersionField field)
    {
        var result = Find(field);
        if (result is null)
        {
            return false;
        }
        return true;
    }

    public Field? Find(int tag)
    {
        return (from Field field in _fields
                where field.Tag == tag
                select field).FirstOrDefault();
    }

    public Field? Find(VersionField definition) => Find(definition.Tag);

    public Field? FindFrom(int tag, int index)
    {
        int temp = index;
        return FindFrom(tag, ref temp);
    }

    public Field? FindFrom(int tag, ref int index)
    {
        if (index < 0 || index >= _fields.Count)
            throw new IndexOutOfRangeException();

        for (; index < _fields.Count; ++index)
        {
            Field field = _fields[index];
            if (field.Tag == tag)
                return field;
        }
        index = -1;
        return null;
    }

    public Field? FindFrom(Dictionary.VersionField definition, int index)
    {
        int temp = index;
        return FindFrom(definition, ref temp);
    }

    public Field? FindFrom(Dictionary.VersionField definition, ref int index) => FindFrom(definition.Tag, ref index);

    public Field this[int index]
    {
        get { return _fields[index]; }
    }

    public void Clear()
    {
        _fields.Clear();
    }

    public void Repeat(int index, int count)
    {
        _fields.InsertRange(index + count, CloneRange(index, count));
    }

    public void Remove(int index, int count)
    {
        _fields.RemoveRange(index, count);
    }

    public void Remove(Dictionary.VersionField definition)
    {
        Remove(definition.Tag);
    }

    public void Remove(int tag)
    {
        if (_fields.Find(f => f.Tag == tag) is Field existing)
        {
            _fields.Remove(existing);
        }
    }

    IEnumerable<Field> CloneRange(int index, int count)
    {
        return from Field field in _fields.GetRange(index, count) select (Field)field.Clone();
    }

    #region IEnumerable<Field>

    public IEnumerator<Field> GetEnumerator()
    {
        return _fields.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion
}

