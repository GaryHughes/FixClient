using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PropertyModels.ComponentModel;

namespace FixClient.Models;

public class MessageProperties(Fix.Dictionary.Message? message)
{
    const string MessageCategory = "Message";

    [Category(MessageCategory)]
    public string? Name => message?.Name;

    [Category(MessageCategory)]
    public string? MsgType => message?.MsgType;

    [Category(MessageCategory)]
    public Fix.Dictionary.Pedigree? Pedigree => message?.Pedigree;
    
    [Category(MessageCategory)]
    [MultilineText]
    public string? Description => message?.Description;
}

public class FieldProperties(Fix.FieldDescription? description)
{
    const string FieldCategory = "Field";

    [Category(FieldCategory)]
    public string? Name => description?.Name;

    [Category(FieldCategory)]
    public int? Tag => description?.Tag;

    [Category(FieldCategory)]
    public string? DataType => description?.DataType;

    [Category(FieldCategory)]
    public int? Depth => description?.Depth;

    [Category(FieldCategory)]
    public bool? Required => description?.Required;

    [Category(FieldCategory)]
    public Fix.Dictionary.Pedigree? Pedigree => description?.Pedigree;
    
    [Category(FieldCategory)]
    [MultilineText]
    public string? Description => description?.Description;
}

public class ValueProperties(Fix.Dictionary.FieldValue? value)
{
    const string ValueCategory = "Value";

    [Category(ValueCategory)]
    public string? Name { get; } = value?.Name;

    [Category(ValueCategory)]
    public string? Value { get; } = value?.Value;

    [Category(ValueCategory)]
    public Fix.Dictionary.Pedigree? Pedigree { get; } = value?.Pedigree;
    
    [Category(ValueCategory)]
    [MultilineText]
    public string? Description => value?.Description;
}

public partial class DataDictionaryInspectorProperties : ObservableObject
{
    [ObservableProperty]
    private MessageProperties _message = new MessageProperties(null);
    
    [ObservableProperty]
    private FieldProperties _field = new FieldProperties(null);

    [ObservableProperty] 
    private ValueProperties _value = new ValueProperties(null);
    
    public void ClearMessageProperties()
    {
        ClearFieldProperties();
        Message = new MessageProperties(null);
    }

    public void ClearFieldProperties()
    {
        ClearValueProperties();
        Field = new FieldProperties(null);
    }

    public void ClearValueProperties()
    {
        Value = new ValueProperties(null);
    }
}