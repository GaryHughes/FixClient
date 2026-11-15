namespace Fix;

public struct MessageDescription
{
    public Dictionary.Version Version { get; set; }
    public Dictionary.Message? Definition { get; set; }
    public DateTime? SendingTime { get; set; }
    public string MsgType { get; set; }
    public string? MsgTypeDescription { get; set; }
    public IEnumerable<FieldDescription> Fields { get; set; }
}
