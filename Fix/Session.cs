/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: Session.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
global using System;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Reflection;
using System.Timers;
using static Fix.Dictionary;

namespace Fix;

public enum Behaviour
{
    Initiator,
    Acceptor
}

public enum State
{
    Disconnected,
    Connected,
    LoggingOn,
    Resending,
    LoggedOn,
    Resetting
};

[JsonObject(MemberSerialization.OptIn)]
public partial class Session : ICloneable
{
    protected const string CategorySession = "Session";
    protected const string CategoryCommon = "Common Message Generation";
    protected const string CategoryNetwork = "Network";
    State _state = State.Disconnected;
    Timer? _heartbeatTimer;
    Timer? _testRequestTimer;
    bool _logonReceived;
    string _outgoingTimestampFormat = Field.TimestampFormatLong;

    #region Events

    public class MessageEvent : EventArgs
    {
        public MessageEvent(Message message)
        {
            Message = (Message)message.Clone();
        }

        public Message Message { get; }
    }

    public class LogEvent : EventArgs
    {
        public LogEvent(string format, params object[] args)
        {
            TimeStamp = DateTime.Now;
            Message = args.Length == 0 ? format : string.Format(format, args);
        }

        public DateTime TimeStamp { get; }
        public string Message { get; }
    }

    public class StateEvent : EventArgs
    {
        public StateEvent(State state, Field? sessionStatus)
        {
            State = state;
            SessionStatus = sessionStatus;
        }

        public State State { get; }
        public Field? SessionStatus { get; }
    }

    public delegate void MessageDelegate(object sender, MessageEvent e);

    public delegate void LogDelegate(object sender, LogEvent e);

    public delegate void StateDelegate(object sender, StateEvent e);

    public event MessageDelegate? MessageSending = null;
    public event MessageDelegate? MessageSent = null;
    public event MessageDelegate? MessageReceived = null;

    public event LogDelegate? Information = null;
    public event LogDelegate? Warning = null;
    public event LogDelegate? Error = null;

    public event StateDelegate? StateChanged = null;

    protected virtual void OnMessageSending(Message message)
    {
        MessageSending?.Invoke(this, new MessageEvent(message));
    }

    protected virtual void OnMessageSent(Message message)
    {
        MessageSent?.Invoke(this, new MessageEvent(message));
    }

    protected virtual void OnMessageReceived(Message message)
    {
        MessageReceived?.Invoke(this, new MessageEvent(message));
    }

    protected virtual void OnInformation(string format, params object[] args)
    {
        Information?.Invoke(this, new LogEvent(format, args));
    }

    protected virtual void OnWarning(string format, params object[] args)
    {
        Warning?.Invoke(this, new LogEvent(format, args));
    }

    protected virtual void OnError(string format, params object[] args)
    {
        Error?.Invoke(this, new LogEvent(format, args));
    }

    protected virtual void OnStateChanged(State state, Field? status)
    {
        StateChanged?.Invoke(this, new StateEvent(state, status));
    }

    #endregion

    public Session()
    {
        BeginString = Versions.FIXT_1_1;
        DefaultApplVerId = Versions.FIX_5_0SP2;
        HeartBtInt = 30;
        AutoSendingTime = true;
        OutgoingSeqNum = 1;
        IncomingSeqNum = 1;
        OutgoingTimestampFormat = Field.TimestampFormatLong;
        FragmentMessages = true;
        TestRequestDelay = 2;
        ValidateDataFields = true;
        ValidateMessages = true;
        Messages = new MessageCollection();
    }

    public Session(Session session)
    {
        OrderBehaviour = session.OrderBehaviour;
        BeginString = session.BeginString;
        DefaultApplVerId = session.DefaultApplVerId;
        LogonBehaviour = session.LogonBehaviour;
        TestRequestDelay = session.TestRequestDelay;
        SenderCompId = session.SenderCompId;
        TargetCompId = session.TargetCompId;
        HeartBtInt = session.HeartBtInt;
        OutgoingTimestampFormat = session.OutgoingTimestampFormat;
        IncomingSeqNum = session.IncomingSeqNum;
        OutgoingSeqNum = session.OutgoingSeqNum;
        BrokenNewSeqNo = session.BrokenNewSeqNo;
        NextExpectedMsgSeqNum = session.NextExpectedMsgSeqNum;
        TestRequestId = session.TestRequestId;
        ExpectedTestRequestId = session.ExpectedTestRequestId;
        FragmentMessages = session.FragmentMessages;
        AutoSendingTime = session.AutoSendingTime;
        ValidateDataFields = session.ValidateDataFields;
        ValidateMessages = session.ValidateMessages;
        Messages = new MessageCollection();
    }

    [DisplayName("Order Flow")]
    [JsonProperty]
    public Behaviour OrderBehaviour { get; set; }

    [Category(CategorySession)]
    [ReadOnly(false)]
    [TypeConverter(typeof(Dictionary.BeginStringTypeConverter))]
    [JsonProperty]
    public Dictionary.Version BeginString { get; set; }

    [Category(CategorySession)]
    [DisplayName("Default Application Version")]
    [ReadOnly(false)]
    [TypeConverter(typeof(Dictionary.ApplVerIdTypeConverter))]
    [JsonProperty]
    public Dictionary.Version DefaultApplVerId { get; set; }

    [Category(CategorySession)]
    [DisplayName("Logon")]
    [JsonProperty]
    public Behaviour LogonBehaviour { get; set; }

    [Category(CategorySession)]
    [DisplayName("TestRequest Delay")]
    [JsonProperty]
    public int TestRequestDelay { get; set; }

    [Category(CategorySession)]
    [DisplayName("SenderCompID")]
    [JsonProperty]
    public string SenderCompId { get; set; } = string.Empty;

    [Category(CategorySession)]
    [DisplayName("TargetCompID")]
    [JsonProperty]
    public string TargetCompId { get; set; } = string.Empty;

    [Category(CategorySession)]
    [DisplayName("Heartbeat Interval")]
    [JsonProperty]
    public int HeartBtInt { get; set; }

    [Category(CategoryCommon)]
    [DisplayName("Outgoing Timestamp Format")]
    [JsonProperty]
    public string OutgoingTimestampFormat
    {
        get => _outgoingTimestampFormat;
        set
        {
            if (!Field.IsTimestampFormatStringValid(value))
            {
                throw new Exception($"'{value}' is not a valid DateTime format string");
            }
            _outgoingTimestampFormat = value;
        }
    }

    [Category(CategoryCommon)]
    [DisplayName("Incoming MsgSegNum")]
    [JsonProperty]
    public int IncomingSeqNum { get; set; }

    [Browsable(false)]
    int IncomingResentSeqNum { get; set; }

    [Browsable(false)]
    int IncomingTargetSeqNum { get; set; }

    [Category(CategoryCommon)]
    [DisplayName("Outgoing MsgSegNum")]
    [JsonProperty]
    public int OutgoingSeqNum { get; set; }

    [Category(CategorySession)]
    [DisplayName("Broken NewSeqNo")]
    [JsonProperty]
    public bool BrokenNewSeqNo { get; set; }

    [Category(CategorySession)]
    [DisplayName("NextExpectedMsgSeqNum")]
    [JsonProperty]
    public bool NextExpectedMsgSeqNum { get; set; }

    [Category(CategoryCommon)]
    [DisplayName("Test Request ID")]
    [JsonProperty]
    public int TestRequestId { get; set; }

    [Category(CategorySession)]
    [DisplayName("Validate Data Fields")]
    [JsonProperty]
    public bool ValidateDataFields { get; set; }

    [Browsable(false)]
    public string? ExpectedTestRequestId { get; set; }

    [Category(CategoryNetwork)]
    [DisplayName("Fragment Messages")]
    [JsonProperty]
    public bool FragmentMessages { get; set; }

    [Browsable(false)]
    [JsonProperty]
    public bool AutoSendingTime { get; set; }

    [Browsable(false)]
    public MessageCollection Messages { get; }

    [Browsable(false)]
    [JsonProperty]
    public bool ValidateMessages { get; set; }

    public int AllocateOutgoingSeqNum()
    {
        return OutgoingSeqNum++;
    }

    public int AllocateTestRequestId()
    {
        return TestRequestId++;
    }

    public virtual void ResetMessages()
    {
        Messages.Clear();
    }

    public virtual void Reset()
    {
        OutgoingSeqNum = 1;
        IncomingSeqNum = 1;
        IncomingResentSeqNum = 0;
        ResetMessages();
    }

    [Browsable(false)]
    public Dictionary.Version Version
    {
        get
        {
            if (BeginString.BeginString == "FIXT.1.1")
                return DefaultApplVerId;
            return BeginString;
        }
    }

    public MessageField? FieldDefinition(Message message, Field field)
    {
        Dictionary.Message? exemplar = Version.Messages[message.MsgType];
        if (exemplar is null)
            return null;
        _ = exemplar.Fields.TryGetValue(field.Tag, out var definition);
        return definition;
    }

    Message ConstructMessage(Dictionary.Message definition)
    {
        Message template = MessageForTemplate(definition);

        var message = new Message
        {
            Definition = definition,
            MsgType = definition.MsgType
        };

        foreach (Field field in template.Fields)
        {
            if (!string.IsNullOrEmpty(field.Value))
                message.Fields.Set(field);
        }

        return message;
    }

    public virtual Message MessageForTemplate(Dictionary.Message definition)
    {
        var message = new Message(definition);
        //
        // Insert place holders for the standard header fields. Order is important so insert them
        // now and they can be overridden as required later on.
        //
        if (definition.Fields.TryGetValue(FIX_5_0SP2.Fields.BeginString.Tag, out var field))
        {
            message.Fields.Add(new Field(field.Tag, string.Empty));
        }

        if (definition.Fields.TryGetValue(FIX_5_0SP2.Fields.BodyLength.Tag, out field))
        {
            message.Fields.Add(new Field(field.Tag, string.Empty));
        }

        if (definition.Fields.TryGetValue(FIX_5_0SP2.Fields.MsgType.Tag, out field))
        {
            message.Fields.Add(new Field(field.Tag, string.Empty));
        }

        if (BeginString.BeginString == "FIXT.1.1" &&
            message.MsgType != FIX_5_0SP2.Messages.Logon.MsgType)
        {
            if (definition.Fields.TryGetValue(FIX_5_0SP2.Fields.ApplVerID.Tag, out field))
            {
                message.Fields.Add(new Field(field.Tag, string.Empty));
            }

            if (definition.Fields.TryGetValue(FIX_5_0SP2.Fields.ApplExtID.Tag, out field))
            {
                message.Fields.Add(new Field(field.Tag, string.Empty));
            }

            if (definition.Fields.TryGetValue(FIX_5_0SP2.Fields.CstmApplVerID.Tag, out field))
            {
                message.Fields.Add(new Field(field.Tag, string.Empty));
            }
        }

        if (definition.Fields.TryGetValue(FIX_5_0SP2.Fields.SenderCompID.Tag, out field))
        {
            message.Fields.Add(new Field(field.Tag, string.Empty));
        }

        if (definition.Fields.TryGetValue(FIX_5_0SP2.Fields.TargetCompID.Tag, out field))
        {
            message.Fields.Add(new Field(field.Tag, string.Empty));
        }

        if (definition.Fields.TryGetValue(FIX_5_0SP2.Fields.MsgSeqNum.Tag, out field))
        {
            message.Fields.Add(new Field(field.Tag, string.Empty));
        }

        if (definition.Fields.TryGetValue(FIX_5_0SP2.Fields.SendingTime.Tag, out field))
        {
            message.Fields.Add(new Field(field.Tag, string.Empty));
        }

        message.MsgType = definition.MsgType;
        message.Definition = definition;

        return message;
    }

    void Logon()
    {
        if (State == State.LoggingOn)
            throw new InvalidOperationException("Logon has already been initiated");

        if (State != State.Connected)
            throw new InvalidOperationException($"Logon cannot be initiated in the current state {State}");

        State = State.LoggingOn;

        if (LogonBehaviour != Behaviour.Initiator)
            return;

        Message logon = ConstructMessage(FIX_5_0SP2.Messages.Logon);

        logon.Fields.Set(FIX_5_0SP2.Fields.BeginString.Tag, BeginString.BeginString);

        logon.Fields.Set(FIX_5_0SP2.Fields.SenderCompID.Tag, SenderCompId);
        logon.Fields.Set(FIX_5_0SP2.Fields.TargetCompID.Tag, TargetCompId);
        logon.Fields.Set(FIX_5_0SP2.EncryptMethod.None);
        logon.Fields.Set(FIX_5_0SP2.Fields.HeartBtInt.Tag, HeartBtInt);

        if (NextExpectedMsgSeqNum)
        {
            logon.Fields.Set(FIX_5_0SP2.Fields.NextExpectedMsgSeqNum, IncomingSeqNum);
        }

        if (BeginString.BeginString == "FIXT.1.1")
        {
            logon.Fields.Set(FIX_5_0SP2.Fields.DefaultApplVerID.Tag, DefaultApplVerId.ApplVerID);
        }

        if (EncryptedPasswordMethod.HasValue)
        {
            logon.Fields.Set(FIX_5_0SP2.Fields.EncryptedPasswordMethod, EncryptedPasswordMethod.Value);
        }

        if (!string.IsNullOrEmpty(EncryptedPassword))
        {
            logon.Fields.Set(FIX_5_0SP2.Fields.EncryptedPassword, EncryptedPassword);
        }

        if (!string.IsNullOrEmpty(EncryptedNewPassword))
        {
            logon.Fields.Set(FIX_5_0SP2.Fields.EncryptedNewPassword, EncryptedNewPassword);
        }

        if (SessionStatus is not null)
        {
            logon.Fields.Set(FIX_5_0SP2.Fields.SessionStatus, SessionStatus.Value);
        }

        Send(logon);
    }

    void Receive(Message message)
    {
        message.Incoming = true;

        if (!ValidateMessages)
        {
            OnMessageReceived(message);
            return;
        }

        try
        {
            if (!ValidateCheckSum(message))
                return;

            if (!ValidateBodyLength(message))
                return;

            if (!ValidateBeginString(message))
                return;

            if (!ValidateCompIds(message))
                return;
        }
        finally
        {
            OnMessageReceived(message);
        }

        if (!ValidateFirstMessage(message))
            return;

        bool duplicate = message.PossDupFlag;

        if (message.MsgType == FIX_5_0SP2.Messages.SequenceReset.MsgType)
        {
            ProcessSequenceReset(message);
            return;
        }

        if (message.MsgType == FIX_5_0SP2.Messages.Logon.MsgType)
        {
            if (!duplicate)
            {
                if (!ProcessLogon(message))
                    return;
            }
        }

        if (State == State.Resending)
        {
            if (duplicate)
            {
                if (message.MsgSeqNum != IncomingResentSeqNum)
                {
                    OnError($"Fatal MsgSeqNum error during resend, received MsgSeqNum = {message.MsgSeqNum} when expecting {IncomingResentSeqNum} MsgType = {message.MsgType} disconnecting");
                    Close();
                    return;
                }

                IncomingResentSeqNum = message.MsgSeqNum + 1;
            }
        }
        else
        {
            if (!duplicate)
            {
                if (message.MsgType == FIX_5_0SP2.Messages.Logout.MsgType)
                {
                    ProcessLogout(message);
                    return;
                }

                if (!ValidateSequenceNumbers(message))
                    return;
            }
        }

        if (!duplicate)
        {
            IncomingSeqNum = message.MsgSeqNum + 1;
        }

        if (message.MsgType == FIX_5_0SP2.Messages.Heartbeat.MsgType)
        {
            ProcessHeartbeat(message);
        }
        else if (message.MsgType == FIX_5_0SP2.Messages.TestRequest.MsgType)
        {
            ProcessTestRequest(message);
        }
        else if (message.MsgType == FIX_5_0SP2.Messages.ResendRequest.MsgType)
        {
            ProcessResendRequest(message);
        }
    }

    bool ValidateFirstMessage(Message message)
    {
        if (LogonBehaviour == Behaviour.Initiator)
            return true;

        if (_logonReceived)
            return true;

        if (message.MsgType == FIX_5_0SP2.Messages.Logon.MsgType)
            return true;

        // TODO - Can we do this better?
        if (message.MsgType == FIX_5_0SP2.Messages.Reject.MsgType ||
            message.MsgType == FIX_5_0SP2.Messages.Logout.MsgType)
        {
            return true;
        }

        const string text = "First message is not a Logon";
        OnError(text);
        SendReject(message, text);
        SendLogout(text);
        return false;
    }

    void SendReject(Message message, string text)
    {
        var reject = new Message { MsgType = FIX_5_0SP2.Messages.Reject.MsgType };
        reject.Fields.Set(FIX_5_0SP2.Fields.RefSeqNum, message.MsgSeqNum);
        reject.Fields.Set(FIX_5_0SP2.Fields.Text, text);
        Send(reject);
    }

    void SendLogout(string text)
    {
        var logout = new Message { MsgType = FIX_5_0SP2.Messages.Logout.MsgType };
        logout.Fields.Set(FIX_5_0SP2.Fields.Text, text);
        Send(logout);

        if (State == State.LoggedOn)
        {
            StopDefibrillator();
            State = State.Connected;
        }

        Close();
    }

    bool ValidateBeginString(Message message)
    {
        if (BeginString == null)
        {
            // This is the first message we have received from the other end of the connection and we are
            // letting them specify the FIX version to use.
            if (Versions[message.BeginString] is not Dictionary.Version version)
            {
                OnError($"Unknown FIX version '{message.BeginString}' specified in BeginString - disconnecting");
                Close();
                return false;
            }

            BeginString = version;
            // TODO - validate DefaultApplVerId
        }
        else
        {
            if (message.BeginString != BeginString.BeginString)
            {
                OnError($"Invalid BeginString, received {message.BeginString} when expecting {BeginString.BeginString}");
                Close();
                return false;
            }
        }

        return true;
    }

    void RequestResend(int received)
    {
        int beginSeqNo = IncomingSeqNum;

        OnInformation($"Recoverable message sequence error, expected {beginSeqNo} received {received} - initiating recovery");

        int endSeqNo = received;
        //
        // FIX 4.0 and FIX 4.1 used EndSeqNo 99999 to represent send me everthing
        // after BeginSeqNo, FIX 4.2 and greater use EndSeqNo 0 for the same purpose.
        //
        /*
	        if (endSeqNo == 0 &&
                (BeginString.BeginString == Dictionary.Versions.FIX_4_0.BeginString ||
                BeginString.BeginString == Dictionary.Versions.FIX_4_1.BeginString))
        {
		    endSeqNo = 99999;
	    }
        */

        ExpectResend(beginSeqNo, endSeqNo);

        OnInformation($"Requesting resend, BeginSeqNo {beginSeqNo} EndSeqNo {endSeqNo}");

        Message resendRequest = ConstructMessage(FIX_5_0SP2.Messages.ResendRequest);
        resendRequest.Fields.Set(FIX_5_0SP2.Fields.BeginSeqNo, beginSeqNo);
        resendRequest.Fields.Set(FIX_5_0SP2.Fields.EndSeqNo, endSeqNo);
        Send(resendRequest);
    }

    void ExpectResend(int beginSeqNo, int endSeqNo)
    {
        State = State.Resending;
        IncomingResentSeqNum = beginSeqNo;
        IncomingTargetSeqNum = endSeqNo;
    }

    bool ValidateSequenceNumbers(Message message)
    {
        if (message.MsgSeqNum > IncomingSeqNum)
        {
            if (NextExpectedMsgSeqNum)
            {
                // If we are using NextExpectedMsgSeqNum the other end will automatically initiate a resend
                OnInformation($"Expecting automatic resend from MsgSeqNum {IncomingSeqNum} to MsgSeqNum {message.MsgSeqNum}");
                State = State.Resending;
                ExpectResend(IncomingSeqNum, message.MsgSeqNum);
                return true;
            }
            RequestResend(message.MsgSeqNum);
            return false;
        }

        if (message.MsgSeqNum < IncomingSeqNum)
        {
            string text = $"MsgSeqNum too low, expecting {IncomingSeqNum} but received {message.MsgSeqNum}";
            OnError(text);
            SendLogout(text);
            return false;
        }

        if (message.MsgType == FIX_5_0SP2.Messages.Reject.MsgType ||
            message.MsgType == FIX_5_0SP2.Messages.Logout.MsgType)
        {
            return true;
        }

        if (State == State.LoggingOn || State == State.Resending)
        {
            OnInformation($"Received expected MsgSeqNum = {message.MsgSeqNum} MsgType = {message.MsgType} without PossDup='Y' during {(State == State.LoggingOn ? "logon" : "resend")} - resuming normal processing");
            State = State.LoggedOn;
            StartDefibrillator();
        }

        return true;
    }

    bool ValidateCompIds(Message message)
    {
        if (string.IsNullOrEmpty(SenderCompId))
        {
            // This is the first message we have received from the other end of the connection and we aren't
            // expecting a particular session.
            SenderCompId = message.TargetCompID;
            TargetCompId = message.SenderCompID;
        }
        else
        {
            string? error = null;

            if (message.TargetCompID != SenderCompId)
            {
                error = $"Received TargetCompID '{message.TargetCompID}' when expecting '{SenderCompId}'";
            }
            else if (message.SenderCompID != TargetCompId)
            {
                error = $"Received SenderCompID '{message.SenderCompID}' when expecting '{TargetCompId}'";
            }

            if (!string.IsNullOrEmpty(error))
            {
                OnError(error);
                SendReject(message, error);
                Close();
                return false;
            }
        }

        return true;
    }

    bool ValidateCheckSum(Message message)
    {
        if (message.Fields.Find(FIX_5_0SP2.Fields.CheckSum.Tag) is not Field checkSum)
        {
            OnError($"Received message without a checksum {message}");
            return false;
        }

        string computedChecksum = Message.ComputeCheckSum(message);

        if (checkSum.Value != computedChecksum)
        {
            OnError($"Received message with invalid checksum, expected {computedChecksum} received {checkSum.Value}");
            return false;
        }

        return true;
    }

    bool ValidateBodyLength(Message message)
    {
        if (message.Fields.Find(FIX_5_0SP2.Fields.BodyLength.Tag) is not Field bodyLength)
        {
            OnError($"Received message without a bodylength {message}");
            return false;
        }

        string computedBodyLength = Message.ComputeBodyLength(message);

        if (!AreNumericallyEquivalent(bodyLength.Value, computedBodyLength))
        {
            OnError($"Received message with an invalid bodylength, expected {computedBodyLength} received {bodyLength.Value}");
            return false;
        }

        return true;
    }

    static bool AreNumericallyEquivalent(string s1, string s2)
    {
        try
        {
            return Convert.ToInt32(s1) == Convert.ToInt32(s2);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    void ProcessSequenceReset(Message reset)
    {
        int newSeqNo = reset.NewSeqNo;
        bool gapFill = reset.GapFillFlag;

        OnInformation($"SeqenceReset {(gapFill ? "GapFill" : "Reset")} received, NewSeqNo = {newSeqNo}");

        if (newSeqNo > IncomingSeqNum)
        {
            if (BrokenNewSeqNo)
            {
                //
                // The GATE 4.2 and 5.x CIs incorrectly sets NewSeqNo to one less than it should be.
                //
                OnInformation("Handling NewSeqNo 1 less than it should be");
                IncomingSeqNum = newSeqNo + 1;
                IncomingResentSeqNum = newSeqNo + 1;
            }
            else
            {
                IncomingSeqNum = newSeqNo;
                IncomingResentSeqNum = newSeqNo;
            }

            OnInformation($"Incoming sequence number reset to {IncomingSeqNum}");
        }
        else
        {
            if (BrokenNewSeqNo)
            {
                IncomingResentSeqNum = newSeqNo + 1;
            }
            else
            {
                IncomingResentSeqNum = newSeqNo;
            }
        }

        if (State == State.Resending && IncomingResentSeqNum >= IncomingTargetSeqNum)
        {
            OnInformation("Resend complete");
            State = State.LoggedOn;
            StartDefibrillator();
            SendTestRequest();
        }
    }

    void ProcessResendRequest(Message request)
    {
        _testRequestTimer?.Dispose();
        _testRequestTimer = null;

        int beginSeqNo = request.BeginSeqNo;
        int endSeqNo = request.EndSeqNo;

        PerformResend(beginSeqNo, endSeqNo);
    }

    void PerformResend(int beginSeqNo, int endSeqNo)
    {
        OnInformation($"Performing resend from BeginSeqNo = {beginSeqNo} to EndSeqNo {endSeqNo}");

        if (BeginString.BeginString == "FIX.4.0" || BeginString.BeginString == "FIX.4.1")
        {
            //
            // FIX 4.0 and FIX 4.1 used EndSeqNo 99999 to represent send me everthing
            // after BeginSeqNo, FIX 4.2 and greater use EndSeqNo 0 for the same purpose.
            //
            if (endSeqNo == 9999)
                endSeqNo = 0;
        }
        //
        // When we send a GapFill the MsgSeqNum of the message must be the MsgSeqNum of the
        // first message being GapFilled.
        //
        bool gapFill = true;
        int gapFillStart = beginSeqNo;


        var localMessages = (MessageCollection)Messages.Clone();

        lock (localMessages)
        {
            foreach (Message message in localMessages)
            {
                if (message.Incoming)
                    continue;

                if (message.PossDupFlag)
                    continue;

                if (message.MsgSeqNum < beginSeqNo)
                    continue;

                if (endSeqNo != 0 && message.MsgSeqNum > endSeqNo)
                    break;

                if (message.Administrative)
                {
                    if (gapFill == false)
                    {
                        gapFillStart = message.MsgSeqNum;
                        gapFill = true;
                    }
                    continue;
                }

                if (gapFill)
                {
                    if (message.MsgSeqNum > beginSeqNo)
                    {
                        var reset = new Message { MsgType = FIX_5_0SP2.Messages.SequenceReset.MsgType };
                        reset.Fields.Set(FIX_5_0SP2.Fields.MsgSeqNum, gapFillStart);
                        reset.Fields.Set(FIX_5_0SP2.Fields.GapFillFlag, true);
                        reset.Fields.Set(FIX_5_0SP2.Fields.PossDupFlag, true);
                        reset.Fields.Set(FIX_5_0SP2.Fields.NewSeqNo, message.MsgSeqNum);
                        Send(reset, false);
                    }
                    gapFill = false;
                }

                var duplicate = (Message)message.Clone();

                if (message.SendingTime is string sendingTime)
                {
                    // TODO - throw?
                    duplicate.Fields.Set(FIX_5_0SP2.Fields.OrigSendingTime, message.SendingTime);
                }

                duplicate.Fields.Set(FIX_5_0SP2.Fields.PossDupFlag, true);
                Send(duplicate, false);
            }
        }
        //
        // If there were no non-admin messages at the end of the sequence make sure we send a GapFill
        // and set the NewSeqNo correctly.
        //
        if (gapFill)
        {
            var reset = new Message { MsgType = FIX_5_0SP2.Messages.SequenceReset.MsgType };
            reset.Fields.Set(FIX_5_0SP2.Fields.MsgSeqNum, gapFillStart);
            reset.Fields.Set(FIX_5_0SP2.Fields.GapFillFlag, true);
            reset.Fields.Set(FIX_5_0SP2.Fields.PossDupFlag, true);
            reset.Fields.Set(FIX_5_0SP2.Fields.NewSeqNo, OutgoingSeqNum);
            Send(reset, false);
        }
    }

    void SendTestRequest()
    {
        var message = new Message { MsgType = FIX_5_0SP2.Messages.TestRequest.MsgType };
        ExpectedTestRequestId = AllocateTestRequestId().ToString();
        message.Fields.Set(FIX_5_0SP2.Fields.TestReqID, ExpectedTestRequestId);
        Send(message);
    }

    void ProcessHeartbeat(Message message)
    {
        if (State != State.LoggingOn && State != State.Resending)
            return;

        if (message.Fields.Find(FIX_5_0SP2.Fields.TestReqID) is Field testReqId)
        {
            if (ExpectedTestRequestId == null)
            {
                OnWarning("Received a Heartbeat with a TestReqID - we weren't expecting one");
                return;
            }

            if (testReqId.Value != ExpectedTestRequestId)
            {
                var text = $"received heartbeat with incorrect TestReqID during {(State == State.LoggingOn ? "logon" : "resend")} - disconnecting";
                OnError(text);
                Close();
                return;
            }
        }

        State = State.LoggedOn;
        StartDefibrillator();
    }

    void StartDefibrillator()
    {
        _testRequestTimer?.Dispose();
        _testRequestTimer = null;

        StopDefibrillator();

        if (HeartBtInt == 0)
            return;

        _heartbeatTimer = new Timer((HeartBtInt - 1) * 1000)
        {
            Interval = HeartBtInt * 1000,
            AutoReset = true
        };
        _heartbeatTimer.Elapsed += (sender, args) => Defibrillate();
        _heartbeatTimer.Start();
    }

    void Defibrillate()
    {
        if (State == State.LoggedOn)
        {
            Message heartbeat = ConstructMessage(FIX_5_0SP2.Messages.Heartbeat);
            Send(heartbeat);
        }
    }

    void StopDefibrillator()
    {
        _heartbeatTimer?.Dispose();
        _heartbeatTimer = null;
    }

    bool ExtractHeartBtInt(Message message)
    {
        if (message.Fields.Find(FIX_5_0SP2.Fields.HeartBtInt) is not Field heartBtInt)
        {
            const string text = "Logon message does not contain a HeartBtInt";
            OnError(text);
            SendReject(message, text);
            SendLogout(text);
            return false;
        }

        if (!int.TryParse(heartBtInt.Value, out int value))
        {
            var text = $"{heartBtInt.Value} is not a valid numeric HeartBtInt";
            OnError(text);
            SendReject(message, text);
            SendLogout(text);
            return false;
        }

        HeartBtInt = value;

        return true;
    }

    void ProcessLogout(Message message)
    {
        if (message.SessionStatus is Field sessionStatus)
        {
            SessionStatus = sessionStatus;
        }

        Close();
    }

    bool ProcessLogon(Message message)
    {
        if (State != State.LoggingOn && !message.ResetSeqNumFlag)
            return false;

        _logonReceived = true;

        if (!ExtractHeartBtInt(message))
            return false;

        if (NextExpectedMsgSeqNum)
        {
            if (message.Fields.Find(FIX_5_0SP2.Fields.NextExpectedMsgSeqNum) is not Field field)
            {
                const string text = "Logon does not contain NextExpectedMsgSeqNum";
                OnError(text);
                SendLogout(text);
                Close();
                return false;
            }

            if (!int.TryParse(field.Value, out int nextExpected))
            {
                var text = $"{field.Value} is not a valid value for NextExpectedMsgSeqNum";
                OnError(text);
                SendLogout(text);
                return false;
            }

            if (nextExpected > OutgoingSeqNum)
            {
                var text = $"NextExpectedMsgSeqNum too high, expecting {OutgoingSeqNum} but received {nextExpected}";
                OnError(text);
                SendLogout(text);
                return false;
            }

            int outgoing = OutgoingSeqNum;

            if (LogonBehaviour == Behaviour.Acceptor)
            {
                SendLogon(false);
            }

            SessionStatus = message.SessionStatus;

            if (nextExpected < outgoing)
            {
                var text = $"NextExpectedMsgSeqNum too low, expecting {outgoing} but received {nextExpected} - performing resend";
                OnWarning(text);
                State = State.Resending;
                PerformResend(nextExpected, outgoing);
                return false;
            }

            return true;
        }

        SessionStatus = message.SessionStatus;

        if (message.ResetSeqNumFlag)
        {
            if (message.MsgSeqNum != 1)
            {
                OnError("Invalid logon message, the ResetSeqNumFlag is set and the MsgSeqNum is not 1");
                return false;
            }

            if (State == State.Resetting)
            {
                State = State.LoggedOn;
                return true;
            }

            OnInformation("Logon message received with ResetSeqNumFlag - resetting sequence numbers");
            Reset();
        }
        else if (message.MsgSeqNum < IncomingSeqNum)
        {
            var text = $"MsgSeqNum too low, expecting {IncomingSeqNum} but received {message.MsgSeqNum}";
            OnError(text);
            SendLogout(text);
            return false;
        }

        if (LogonBehaviour == Behaviour.Acceptor || message.ResetSeqNumFlag)
        {
            SendLogon(message.ResetSeqNumFlag);
        }

        if (!NextExpectedMsgSeqNum && !message.ResetSeqNumFlag)
        {
            if (TestRequestDelay > 0)
            {
                _testRequestTimer = new Timer
                {
                    Interval = TestRequestDelay * 1000,
                    AutoReset = false
                };
                _testRequestTimer.Elapsed += (sender, args) => SendTestRequest();
                _testRequestTimer.Start();
            }
            else
            {
                SendTestRequest();
            }
        }

        return true;
    }

    void SendLogon(bool resetSeqNumFlag)
    {
        Message logon = ConstructMessage(FIX_5_0SP2.Messages.Logon);

        logon.Fields.Set(FIX_5_0SP2.EncryptMethod.None);
        logon.Fields.Set(FIX_5_0SP2.Fields.HeartBtInt, HeartBtInt);

        if (BeginString.BeginString == "FIXT.1.1")
        {
            logon.Fields.Set(FIX_5_0SP2.Fields.DefaultApplVerID.Tag, DefaultApplVerId.ApplVerID);
        }

        if (resetSeqNumFlag)
        {
            logon.Fields.Set(FIX_5_0SP2.Fields.ResetSeqNumFlag, "Y");
        }

        if (NextExpectedMsgSeqNum)
        {
            logon.Fields.Set(FIX_5_0SP2.Fields.NextExpectedMsgSeqNum, IncomingSeqNum + 1);
        }

        Send(logon);
    }

    void ProcessTestRequest(Message message)
    {
        Message heartbeat = ConstructMessage(FIX_5_0SP2.Messages.Heartbeat);

        if (message.Fields.Find(FIX_5_0SP2.Fields.TestReqID) is not Field testReqId)
        {
            // TODO - error
            return;
        }

        heartbeat.Fields.Set(testReqId);

        Send(heartbeat);
    }

    [Browsable(false)]
    public State State
    {
        get { return _state; }
        private set
        {
            if (_state != value)
            {
                OnInformation($"Transiton from state {_state} to state {value}");
                _state = value;
                OnStateChanged(_state, SessionStatus);
            }
        }
    }

    [Browsable(false)]
    public int? EncryptedPasswordMethod { get; set; }

    [Browsable(false)]
    public string? EncryptedPassword { get; set; }

    [Browsable(false)]
    public string? EncryptedNewPassword { get; set; }

    [Browsable(false)]
    public Field? SessionStatus { get; set; }

    public virtual void UpdateReadonlyAttributes()
    {
    }

    protected void SetReadOnly(string name, bool value)
    {
        if (TypeDescriptor.GetProperties(GetType())[name] is PropertyDescriptor descriptor)
        {
            if (descriptor.Attributes[typeof(ReadOnlyAttribute)] is ReadOnlyAttribute attribute)
            {
                if (attribute.GetType().GetField("isReadOnly", BindingFlags.NonPublic | BindingFlags.Instance) is FieldInfo field)
                {
                    field.SetValue(attribute, value);
                }
            }
        }
    }

    #region ICloneable

    public virtual object Clone()
    {
        return MemberwiseClone();
    }

    #endregion

}

