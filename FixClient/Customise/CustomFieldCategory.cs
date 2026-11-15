namespace FixClient;

class CustomFieldCategory
{
    readonly string _name;
    readonly List<CustomField> _fields = new();

    public CustomFieldCategory(string name)
    {
        _name = name;
    }

    public string Name
    {
        get { return _name; }
    }

    public List<CustomField> Fields
    {
        get { return _fields; }
    }

    public bool Add(CustomField field)
    {
        if (_fields.Contains(field))
            return false;

        _fields.Add(field);
        return true;
    }

    public override string ToString()
    {
        return Name;
    }
}

