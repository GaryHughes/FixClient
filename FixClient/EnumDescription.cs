/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: EnumDescription.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using System.Collections.Generic;
using static Fix.Dictionary;

namespace FixClient;

public record EnumDescription(string Name, string Value, string Description)
{
    public override string ToString() => Name;
}

public class EnumDescriptionCollection : List<EnumDescription>
{
    public EnumDescriptionCollection(VersionField field)
    {
        if (field.Values.Count == 0)
        {
            throw new ArgumentException(string.Format("Field type must be enumerated! you passed {0}", field.Name));
        }

        foreach (var item in field.Values)
        {
            Add(new EnumDescription(item.Value.Name, item.Value.Value, item.Value.Description));
        }
    }
}

