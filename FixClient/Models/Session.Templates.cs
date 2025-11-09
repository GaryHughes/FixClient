using System.Collections.Generic;
using System.Linq;

namespace FixClient.Models;

public partial class Session
{
    readonly Dictionary<string, Fix.Message> _messageTemplates = new();

    public override Fix.Message MessageForTemplate(Fix.Dictionary.Message templateMessage)
    {
        if (_messageTemplates.TryGetValue(templateMessage.MsgType, out var message))
        {
            return message;
        }
        
        message = base.MessageForTemplate(templateMessage);

        foreach (var templateField in templateMessage.Fields)
        {
            message.Fields.Set(new Fix.Field(templateField));
        }

        message.MsgType = templateMessage.MsgType;
        message.Definition = templateMessage;
        _messageTemplates[message.MsgType] = message;

        return message;
    }

    public void ResetTemplateMessage(string msgType)
    {
        if (!_messageTemplates.TryGetValue(msgType, out var template))
        {
            return;
        }
        
        if (template.Definition is not Fix.Dictionary.Message definition)
        {
            return;
        }

        template.Fields.Clear();

        foreach (var templateField in definition.Fields)
        {
            template.Fields.Set(new Fix.Field(templateField));
        }

        var exemplar = base.MessageForTemplate(definition);

        foreach (var field in exemplar.Fields)
        {
            template.Fields.Set(new Fix.Field(field.Tag, field.Value));
        }

        template.MsgType = msgType;
        template.Definition = definition;
    }

    public void ResetMessageTemplates()
    {
        var existing = _messageTemplates.ToDictionary(item => item.Key, item => item.Value);
        
        _messageTemplates.Clear();
        
        foreach (var previous in existing.Select(item => item.Value))
        {
            if (previous.Definition == null)
            {
                continue;
            }
            
            var template = MessageForTemplate(previous.Definition);
            
            foreach (var field in previous.Fields)
            {
                if (!string.IsNullOrEmpty(field.Value))
                {
                    template.Fields.Set(field);
                }
            }
        }
    }
}