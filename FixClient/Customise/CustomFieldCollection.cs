using System.Text;
using System.Xml;

namespace FixClient;

class CustomFieldCollection
{
    readonly Dictionary<string, CustomFieldCategory> _fields = new();

    public CustomFieldCollection(string xml)
    {
        Load(xml);
    }

    public void Load(string xml)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        using var reader = new XmlTextReader(stream);

        while (!reader.EOF)
        {
            reader.MoveToContent();

            if (reader.Name != "Field")
            {
                reader.Read();
                continue;
            }

            if (reader.GetAttribute("name") is not string name)
            {
                continue;
            }

            int id = Convert.ToInt32(reader.GetAttribute("id"));

            if (reader.GetAttribute("categories") is not string categoryAttribute)
            {
                continue;
            }

            string[] categories = categoryAttribute.Split(',');

            foreach (string c in categories)
            {
                string category = c.Trim();

                if (category == string.Empty)
                {
                    continue;
                }

                if (!_fields.ContainsKey(category))
                {
                    _fields[category] = new CustomFieldCategory(category);
                }

                _fields[category].Add(new CustomField { Tag = id, Name = name });
            }

            reader.Read();
        }
    }

    public Dictionary<string, CustomFieldCategory> Fields => _fields;

}

