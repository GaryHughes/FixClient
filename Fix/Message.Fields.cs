using static Fix.Dictionary;

namespace Fix;

public class MissingFieldException : Exception
{
    public MissingFieldException(Dictionary.VersionField field)
    : base($"Message does not contain a {field.Name} field")
    {
    }
}

public partial class Message
{
    public string MsgType
    {
        get
        {
            if (Definition != null)
            {
                return Definition.MsgType;
            }

            if (Fields.Find(FIX_5_0SP2.Fields.MsgType) is Field field)
            {
                return @field.Value;
            }

            throw new MissingFieldException(FIX_5_0SP2.Fields.MsgType);
        }
        set
        {
            Fields.Set(FIX_5_0SP2.Fields.MsgType.Tag, value);
            Definition = null;
        }
    }

    public string BeginString
    {
        get
        {
            if (Fields.Find(FIX_5_0SP2.Fields.BeginString) is Field field)
            {
                return @field.Value;
            }

            throw new MissingFieldException(FIX_5_0SP2.Fields.BeginString);
        }
    }

    public int BodyLength
    {
        get
        {
            if (Fields.Find(FIX_5_0SP2.Fields.BodyLength) is Field field)
            {
                return Convert.ToInt32(@field.Value);
            }

            throw new MissingFieldException(FIX_5_0SP2.Fields.BodyLength);
        }
    }

    public int MsgSeqNum
    {
        get
        {
            if (Fields.Find(FIX_5_0SP2.Fields.MsgSeqNum) is Field field)
            {
                return Convert.ToInt32(@field.Value);
            }

            throw new MissingFieldException(FIX_5_0SP2.Fields.MsgSeqNum);
        }
    }

    public string? CheckSum => Fields.Find(FIX_5_0SP2.Fields.CheckSum)?.Value;

    public string SenderCompID
    {
        get
        {
            return Fields.Find(FIX_5_0SP2.Fields.SenderCompID)?.Value ?? string.Empty;
        }
        set
        {
            Fields.Set(FIX_5_0SP2.Fields.SenderCompID, value);
        }
    }

    public string TargetCompID
    {
        get
        {
            return Fields.Find(FIX_5_0SP2.Fields.TargetCompID)?.Value ?? string.Empty;
        }
        set
        {
            Fields.Set(FIX_5_0SP2.Fields.TargetCompID, value);
        }
    }

    public string? SendingTime => Fields.Find(FIX_5_0SP2.Fields.SendingTime)?.Value;

    public int BeginSeqNo
    {
        get
        {
            if (Fields.Find(FIX_5_0SP2.Fields.BeginSeqNo) is Field field)
            {
                if (!int.TryParse(@field.Value, out int value))
                {
                    throw new Exception($"Message contains an invalid {@field}");
                }

                return value;
            }

            throw new MissingFieldException(FIX_5_0SP2.Fields.BeginSeqNo);
        }
    }

    public int EndSeqNo
    {
        get
        {
            if (Fields.Find(FIX_5_0SP2.Fields.EndSeqNo) is Field field)
            {
                if (!int.TryParse(@field.Value, out int value))
                {
                    throw new Exception($"Message contains an invalid {@field}");
                }

                return value;
            }

            throw new MissingFieldException(FIX_5_0SP2.Fields.EndSeqNo);
        }
    }

    public bool GapFillFlag
    {
        get
        {
            if (Fields.Find(FIX_5_0SP2.Fields.GapFillFlag) is Field field)
            {
                return (bool)@field;
            }

            return false;
        }
    }

    public bool ResetSeqNumFlag
    {
        get
        {
            if (Fields.Find(FIX_5_0SP2.Fields.ResetSeqNumFlag) is Field field)
            {
                return (bool)@field;
            }

            return false;
        }
    }

    public int NewSeqNo
    {
        get
        {
            if (Fields.Find(FIX_5_0SP2.Fields.NewSeqNo) is Field field)
            {
                if (!int.TryParse(@field.Value, out int value))
                {
                    throw new Exception($"Message contains an invalid {@field}");
                }

                return value;
            }

            throw new MissingFieldException(FIX_5_0SP2.Fields.NewSeqNo);
        }
    }

    public bool PossDupFlag
    {
        get
        {
            if (Fields.Find(FIX_5_0SP2.Fields.PossDupFlag) is Field field)
            {
                return (bool)@field;
            }

            return false;
        }
    }

    public Field? OrdStatus => Fields.Find(FIX_5_0SP2.Fields.OrdStatus);

    public Field? SessionStatus => Fields.Find(FIX_5_0SP2.Fields.SessionStatus);

    public string? ClOrdID => Fields.Find(FIX_5_0SP2.Fields.ClOrdID)?.Value;

    public string? OrigClOrdID => Fields.Find(FIX_5_0SP2.Fields.OrigClOrdID)?.Value;

}

