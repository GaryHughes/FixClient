using System.Buffers;
using System.Globalization;
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

    public static string TimestampString(string timestampFormat = TimestampFormatLong) => DateTime.UtcNow.ToString(timestampFormat, CultureInfo.InvariantCulture);

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
        Value = value.ToString(CultureInfo.InvariantCulture);
    }

    public Field(int tag, long value)
    {
        Tag = tag;
        Value = value.ToString(CultureInfo.InvariantCulture);
    }

    public Field(int tag, decimal value)
    {
        Tag = tag;
        Value = value.ToString(CultureInfo.InvariantCulture);
    }

    public Field(int tag, bool value)
    {
        Tag = tag;
        Value = value ? "Y" : "N";
    }

    public Field(string tag, string value)
    {
        if (!int.TryParse(tag, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intTag))
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
    : this(definition.Tag, value.ToString(CultureInfo.InvariantCulture))
    {
    }

    public Field(VersionField definition, decimal value)
    : this(definition.Tag, value.ToString(CultureInfo.InvariantCulture))
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
        if (!long.TryParse(field.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
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
        if (!decimal.TryParse(field.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal result))
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

    // Must agree with Equals, which compares Tag and Value. FieldValue hashes the same way, so a
    // Field and an equal FieldValue also have the same hash code.
    public override int GetHashCode() => HashCode.Combine(Tag, Value);

    //
    // These are called for every field of every message sent and received so they work on stack
    // or pooled buffers rather than allocating strings and byte arrays for each field.
    //
    public int ComputeCheckSum()
    {
        Span<byte> tag = stackalloc byte[MaxTagBytes];
        int tagLength = FormatTag(tag);

        int checksum = ValueSeparator + Separator;

        foreach (byte b in tag[..tagLength])
        {
            checksum += b;
        }

        if (!string.IsNullOrEmpty(Value))
        {
            checksum += ProcessValueBytes(sum: true);
        }

        return checksum;
    }

    public int ComputeBodyLength()
    {
        Span<byte> tag = stackalloc byte[MaxTagBytes];

        // The tag, '=' and the trailing SOH.
        int bodyLength = FormatTag(tag) + 2;

        if (!string.IsNullOrEmpty(Value))
        {
            bodyLength += Data ? ProcessValueBytes(sum: false) : Value.Length;
        }

        return bodyLength;
    }

    const int MaxTagBytes = 11;
    const int StackBufferSize = 256;

    int FormatTag(Span<byte> destination)
    {
        Tag.TryFormat(destination, out int written, default, CultureInfo.InvariantCulture);
        return written;
    }

    // Returns the sum or the count of the bytes the value is sent as: the decoded bytes for a data
    // field and the Latin-1 encoding of the text otherwise.
    int ProcessValueBytes(bool sum)
    {
        int maximum = Value.Length + 1;
        byte[]? rented = null;
        Span<byte> buffer = maximum <= StackBufferSize
            ? stackalloc byte[StackBufferSize]
            : (rented = ArrayPool<byte>.Shared.Rent(maximum));

        try
        {
            int count;

            if (Data)
            {
                if (!Convert.TryFromBase64String(Value, buffer, out count))
                {
                    // Not valid base64; let FromBase64String throw the same FormatException as before.
                    count = Convert.FromBase64String(Value).Length;
                }
            }
            else
            {
                count = Encoding.Latin1.GetBytes(Value, buffer);
            }

            if (!sum)
            {
                return count;
            }

            int total = 0;

            foreach (byte b in buffer[..count])
            {
                total += b;
            }

            return total;
        }
        finally
        {
            if (rented is not null)
            {
                ArrayPool<byte>.Shared.Return(rented);
            }
        }
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
        return new Field(Tag, Value) { Data = Data };
    }
}

