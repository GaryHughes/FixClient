/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: Message.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Fix.Dictionary;

namespace Fix;

public enum MessageStatus
{
    None,
    Info,
    Warn,
    Error
}

public partial class Message : ICloneable
{
    public Message()
    {
        //
        // Insert place holders for the standard header fields. Order is important so insert them
        // now and they can be overridden as required later on.
        //
        Fields = new FieldCollection
        {
            new Field(FIX_5_0SP2.Fields.BeginString),
            new Field(FIX_5_0SP2.Fields.BodyLength),
            new Field(FIX_5_0SP2.Fields.MsgType),
            new Field(FIX_5_0SP2.Fields.SenderCompID),
            new Field(FIX_5_0SP2.Fields.TargetCompID),
            new Field(FIX_5_0SP2.Fields.MsgSeqNum),
            new Field(FIX_5_0SP2.Fields.SendingTime)
        };
    }

    public Message(Dictionary.Message definition)
    {
        Fields = new FieldCollection();
        Definition = definition;
    }

    public Message(string data)
    : this(Encoding.ASCII.GetBytes(data))
    {
    }

    public Message(byte[] data)
    {
        Fields = new FieldCollection();
        using MemoryStream stream = new(data);
        using Reader reader = new(stream);
        reader.Read(this);
    }

    public Message(string[,] data)
    {
        Fields = new FieldCollection(data);
    }

    public Message(IEnumerable<Field> source)
    : this()
    {
        foreach (Field field in source)
        {
            Fields.Set(field);
        }
    }

    public Message Trim()
    {
        var message = new Message();
        message.Fields.Clear();

        foreach (Field field in Fields)
        {
            if (field.Value != null && field.Value.Trim().Length > 0)
                message.Fields.Add(field);
        }

        return message;
    }

    public static async Task<Message?> Parse(string text)
    {
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(text));

        await foreach (var message in Parser.Parse(stream))
        {
            return message;
        }

        return null;
    }

    public MessageStatus Status { get; set; }
    public string? StatusMessage { get; set; }

    public Dictionary.Message? Definition { get; set; }

    public FieldCollection Fields { get; }

    // TODO - This should probably be in the definition and it should come from the repo data, is it actually in the repo?
    public bool Administrative
    {
        get
        {
            if (MsgType == FIX_5_0SP2.Messages.Heartbeat.MsgType ||
                MsgType == FIX_5_0SP2.Messages.Logon.MsgType ||
                MsgType == FIX_5_0SP2.Messages.Logout.MsgType ||
                MsgType == FIX_5_0SP2.Messages.Reject.MsgType ||
                MsgType == FIX_5_0SP2.Messages.ResendRequest.MsgType ||
                MsgType == FIX_5_0SP2.Messages.SequenceReset.MsgType ||
                MsgType == FIX_5_0SP2.Messages.TestRequest.MsgType)
            {
                return true;
            }

            return false;
        }
    }

    public string ComputeCheckSum()
    {
        return ComputeCheckSum(this);
    }

    public static string ComputeCheckSum(Message message)
    {
        //
        // The checksum is computed by summing all bytes with the message (except those of the
        // checksum field '10=nnn\001' itself) modulo 256.
        //
        int value = message.Fields.Where(field => field.Tag != FIX_5_0SP2.Fields.CheckSum.Tag).Sum(field => field.ComputeCheckSum()) % 256;
        return value.ToString("D3");
    }

    public string ComputeBodyLength()
    {
        return ComputeBodyLength(this);
    }

    public static string ComputeBodyLength(Message message)
    {
        int bodyLength = 0;
        bool passedBodyLength = false;

        foreach (Field field in message.Fields)
        {
            if (field.Tag == FIX_5_0SP2.Fields.CheckSum.Tag)
                continue;

            if (field.Tag == FIX_5_0SP2.Fields.BodyLength.Tag)
            {
                passedBodyLength = true;
                continue;
            }

            if (!passedBodyLength)
                continue;

            bodyLength += field.ComputeBodyLength();
        }

        return bodyLength.ToString();
    }

    public bool Incoming { get; set; }

    #region Object

    public MessageDescription Describe(Dictionary.Version? version = null)
    {
        if (version == null)
        {
            if (Fields.TryGetValue(FIX_5_0SP2.Fields.BeginString, out var beginString))
            {
                version = Dictionary.Versions[beginString.Value];
            }

            if (version == null)
            {
                version = Versions.Default;
            }
        }

        var messageDefinition = version.Messages[MsgType];

        var description = new MessageDescription
        {
            Version = version,
            Definition = messageDefinition,
            MsgType = MsgType,
            MsgTypeDescription = version.Messages[MsgType]?.Name,
            Fields = from field in Fields select field.Describe(messageDefinition)
        };

        if (Fields.TryGetValue(FIX_5_0SP2.Fields.SendingTime, out var sendingTime))
        {
            try
            {
                description.SendingTime = (DateTime?)sendingTime;
            }
            catch
            {
            }
        }

        return description;
    }

    public string PrettyPrint()
    {
        var stream = new MemoryStream();
        PrettyPrint(stream);
        return Encoding.UTF8.GetString(stream.GetBuffer());
    }

    public void PrettyPrint(Stream stream)
    {
        using var writer = new StreamWriter(stream, Encoding.ASCII, 4096, true);
        var description = Describe();

        writer.WriteLine(description.MsgTypeDescription + " (" + (Incoming ? "incoming" : "outgoing") + ")\r\n{");

        int widestName = 0;

        foreach (var field in description.Fields)
        {
            if (field.Name?.Length > widestName)
            {
                widestName = field.Name.Length;
            }
        }

        foreach (var field in description.Fields)
        {
            writer.WriteLine("    {0} {1} - {2}{3}",
                                (field.Name ?? "").PadLeft(widestName),
                                string.Format("({0})", field.Tag).PadLeft(6),
                                field.Value,
                                field.ValueDefinition switch
                                {
                                    FieldValue value => " - " + value.Name,
                                    _ => string.Empty
                                });
        }

        writer.WriteLine("}");
    }

    public override string ToString() => PrettyPrint();


    #endregion

    #region ICloneable

    public object Clone()
    {
        var clone = new Message();
        clone.Fields.Clear();
        foreach (Field field in Fields)
        {
            clone.Fields.Add((Field)field.Clone());
        }
        clone.Definition = Definition;
        clone.Incoming = Incoming;
        clone.Status = Status;
        clone.StatusMessage = StatusMessage;
        return clone;
    }

    #endregion
}

