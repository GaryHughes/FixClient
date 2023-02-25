/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: Field.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using System.Globalization;
using System.Linq;
using System.Text;
using static Fix.Dictionary;

namespace Fix;

public class Field : ICloneable
{
    public static readonly byte Separator = Convert.ToByte('\x01');
    public static readonly byte ValueSeparator = Convert.ToByte('=');

    public const string DateFormat = "yyyyMMdd";
    public const string TimestampFormatShort = "yyyyMMdd-HH:mm:ss";
    public const string TimestampFormatLong = "yyyyMMdd-HH:mm:ss.fff";

    public static string TimestampString(string timestampFormat = TimestampFormatLong) => DateTime.UtcNow.ToString(timestampFormat);

    public static bool IsTimestampFormatStringValid(string format)
    {
        try
        {
            var result = DateTime.ParseExact(DateTime.Now.ToString(format, CultureInfo.InvariantCulture),
                                             format,
                                             CultureInfo.InvariantCulture,
                                             DateTimeStyles.NoCurrentDateDefault);
            return result != default;
        }
        catch
        {
            return false;
        }
    }

    public static readonly Field Invalid = new(0, string.Empty);

    public Field(int tag, string value)
    {
        Tag = tag;
        Value = value;
    }

    public Field(int tag, int value)
    {
        Tag = tag;
        Value = value.ToString();
    }

    public Field(int tag, long value)
    {
        Tag = tag;
        Value = value.ToString();
    }

    public Field(int tag, decimal value)
    {
        Tag = tag;
        Value = value.ToString();
    }

    public Field(int tag, bool value)
    {
        Tag = tag;
        Value = value ? "Y" : "N";
    }

    public Field(string tag, string value)
    {
        if (!int.TryParse(tag, out var intTag))
        {
            throw new ArgumentException($"Invalid FIX tag '{tag}' is not an integer");
        }

        Tag = intTag;
        Value = value;
    }

    public Field(MessageField definition)
    {
        Tag = definition.Tag;
    }

    public Field(VersionField definition)
    {
        Tag = definition.Tag;
    }

    public Field(VersionField definition, string value)
    : this(definition.Tag, value)
    {
    }

    public Field(VersionField definition, bool value)
        : this(definition.Tag, value)
    {
    }

    public Field(VersionField definition, int value)
    : this(definition.Tag, value.ToString())
    {
    }

    public Field(VersionField definition, decimal value)
    : this(definition.Tag, value.ToString())
    {
    }

    public Field(FieldValue value)
    : this(value.Tag, value.Value)
    {
    }

    public int Tag { get; }
    public string Value { get; set; } = string.Empty;
    public bool Data { get; set; }

    public static explicit operator bool(Field field)
    {
        if (field.Value == "N" || field.Value == "n" || field.Value == "false" || field.Value == "0")
            return false;
        if (field.Value == "Y" || field.Value == "y" || field.Value == "true" || field.Value == "1")
            return true;
        throw new Exception($"Field does not contain a valid boolean value {field}");
    }

    public static explicit operator long?(Field field)
    {
        if (!long.TryParse(field.Value, out var result))
        {
            return null;
        }

        return result;
    }

    public static explicit operator string(Field field)
    {
        return field.Value;
    }

    public static explicit operator decimal?(Field field)
    {
        if (!decimal.TryParse(field.Value, out decimal result))
        {
            return null;
        }

        return result;
    }

    public static explicit operator DateTime?(Field field)
    {
        string[] formats = { TimestampFormatLong, TimestampFormatShort, DateFormat };

        if (!DateTime.TryParseExact(field.Value,
                                    formats,
                                    CultureInfo.InvariantCulture,
                                    DateTimeStyles.None,
                                    out var result))
        {
            return null;
        }

        return result;
    }

    public static explicit operator FieldValue(Field field)
    {
        return new FieldValue(field.Tag, "", field.Value, "", Pedigree.Empty);
    }

    public static bool operator ==(Field left, FieldValue? right)
    {
        return (left, right) switch
        {
            (null, null) => true,
            (Field, null) => false,
            (null, FieldValue) => false,
            (Field field, FieldValue value) => field.Tag == value.Tag && field.Value == value.Value
        };
    }

    public static bool operator !=(Field left, FieldValue? right) => !(left == right);

    public override bool Equals(object? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(other, this))
        {
            return true;
        }

        if (other is Field field)
        {
            return Tag == field.Tag && Value == field.Value;
        }

        if (other is FieldValue fieldValue)
        {
            return Tag == fieldValue.Tag && Value == fieldValue.Value;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public int ComputeCheckSum()
    {
        int checksum = Encoding.ASCII.GetBytes(Tag + "=").Select(b => (int)b).Sum();

        if (!string.IsNullOrEmpty(Value))
        {
            if (Data)
            {
                byte[] bytes = Convert.FromBase64String(Value);
                checksum += bytes.Select(b => (sbyte)b).Select(i => (int)i).Sum();
            }
            else
            {
                checksum += Encoding.ASCII.GetBytes(Value).Select(b => (int)b).Sum();
            }
        }

        checksum += Encoding.ASCII.GetBytes("\x01").Select(b => (int)b).Sum();

        return checksum;
    }

    public int ComputeBodyLength()
    {
        int bodyLength = Encoding.ASCII.GetBytes(string.Format("{0}=", Tag)).Length;

        if (!string.IsNullOrEmpty(Value))
        {
            if (Data)
            {
                byte[] bytes = Convert.FromBase64String(Value);
                bodyLength += bytes.Length;
            }
            else
            {
                bodyLength += Value.Length;
            }
        }

        bodyLength += 1;

        return bodyLength;
    }

    public FieldDescription Describe(Dictionary.Message? messageDefinition)
    {
        return Describe(messageDefinition, Tag, Value);
    }

    public static FieldDescription Describe(Dictionary.Message? messageDefinition, int tag, string value)
    {
        static VersionField? FindGlobalDefinition(int tag)
        {
            foreach (var version in Versions.Reverse())
            {
                if (version.Fields.TryGetValue(tag, out var definition) && definition.IsValid)
                {
                    return definition;
                }
            }

            return null;
        }

        var definition = messageDefinition?.Fields.Where(f => f.Tag == tag).FirstOrDefault();

        if (definition == null)
        {
            // This field is not defined in this message so just fall back to the global field definitions.
            // This means we won't have values for required and depth which are message specific.
            string description = string.Empty;

            if (FindGlobalDefinition(tag) is VersionField globalDefinition)
            {
                if (value is string fieldValue)
                {
                    description = DescribeVersionFieldValue(globalDefinition, fieldValue);
                }

                globalDefinition.Values.TryGetValue(value, out var valueDefinition);

                return new FieldDescription(tag, value, globalDefinition.Name, description, false, -1, globalDefinition.DataType, globalDefinition.Pedigree, valueDefinition);
            }
        }

        if (definition is MessageField fieldDefinition)
        {
            if (FindGlobalDefinition(tag) is VersionField globalDefinition)
            {
                globalDefinition.Values.TryGetValue(value, out var valueDefinition);

                return new FieldDescription(tag,
                                            value,
                                            fieldDefinition.Name,
                                            fieldDefinition.Description,
                                            fieldDefinition.Required,
                                            fieldDefinition.Depth,
                                            globalDefinition.DataType,
                                            globalDefinition.Pedigree,
                                            valueDefinition);
            }
        }

        static string DescribeVersionFieldValue(VersionField fieldDefinition, string fieldValue)
        {
            if (fieldDefinition.Values.TryGetValue(fieldValue, out var valueDefinition))
            {
                return valueDefinition.Name;
            }

            return string.Empty;
        }

        return new FieldDescription(tag, value, string.Empty, string.Empty, false, -1, string.Empty, Pedigree.Empty, null);
    }

    #region Object

    public override string ToString() => $"{Tag}={Value}";

    #endregion

    public object Clone()
    {
        return new Field(Tag, Value);
    }
}

