using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//
// This class produces simple text based tabular reports with column justification. Column headers can contain
// new lines. An optional footer row is also supported.
//
namespace Fix.Common;

public class Report
{
    public enum ColumnJustification
    {
        Left,
        Right
    }

    public class Column
    {
        public Column(string name, ColumnJustification justification = ColumnJustification.Left)
        {
            RawName = name;
            Name = RawName.Split('\n');
            Justification = justification;
        }

        public string RawName { get; }
        public string[] Name { get; }
        public ColumnJustification Justification { get; }
    };

    public Report()
    {
    }

    public Report(string title)
    {
        Title = title;
    }

    public string? Title { get; set; } = null;

    public char SeparatorCharacter { get; set; } = '-';

    public List<Column> Columns { get; } = new List<Column>();
    List<string[]> Rows { get; } = new List<string[]>();
    string?[]? Footer { get; set; } = null;


    public void AddColumn(string name, ColumnJustification justification = ColumnJustification.Left)
    {
        Columns.Add(new Column(name, justification));
    }

    public void AddRow(params object?[] values)
    {
        Rows.Add((from value in values select value?.ToString()).ToArray());
    }

    public void SetFooter(params object?[] values)
    {
        Footer = (from value in values select value?.ToString()).ToArray();
    }

    public override string ToString()
    {
        var builder = new StringBuilder();
        //
        // Calculate the column widths
        //
        var columnWidths = new int[Columns.Count];
        int columnHeaderRows = 0;

        for (int index = 0; index < Columns.Count; ++index)
        {
            var column = Columns[index];
            columnHeaderRows = Math.Max(columnHeaderRows, column.Name.Length);
            int size = column.Name.Max(name => name.Length);
            if (Rows.Any())
            {
                size = Math.Max(size, Rows.Max(row => (index >= row.Length || row[index] == null) ? 0 : row[index].Length));
            }
            if (index < Footer?.Length)
            {
                var footerValue = Footer[index];
                size = Math.Max(size, footerValue == null ? 0 : footerValue.Length);
            }
            columnWidths[index] = size;
        }
        //
        // Print the report title
        //
        int totalWidth = columnWidths.Sum() + ((columnWidths.Length - 1) * 2); // 2 space characters between each column

        if (Title is string title && title.Length > 0)
        {
            builder.AppendLine(title.PadLeft((totalWidth / 2) + (title.Length / 2)));
        }
        //
        // Print a separator between the title column headers and column headers
        //
        builder.AppendLine($"{SeparatorCharacter}".PadLeft(totalWidth, SeparatorCharacter));
        //
        // Print the column headers
        //
        for (int headerIndex = 0; headerIndex < columnHeaderRows; ++headerIndex)
        {
            for (int index = 0; index < Columns.Count; ++index)
            {
                var column = Columns[index];
                string value = "";
                if (column.Name.Length == columnHeaderRows)
                {
                    value = column.Name[headerIndex];
                }
                else
                {
                    int effectiveMax = columnHeaderRows - headerIndex;
                    if (column.Name.Length >= effectiveMax)
                    {
                        value = column.Name[headerIndex - (columnHeaderRows - column.Name.Length)];
                    }
                }

                if (column.Justification == ColumnJustification.Left)
                {
                    builder.Append(value.PadRight(columnWidths[index]));
                }
                else
                {
                    builder.Append(value.PadLeft(columnWidths[index]));
                }

                if (index < Columns.Count - 1)
                {
                    builder.Append("  ");
                }
            }
            builder.AppendLine();
        }
        //
        // Print a separator between the column headers and the data rows
        //
        builder.AppendLine($"{SeparatorCharacter}".PadLeft(totalWidth, SeparatorCharacter));
        //
        // Print the data rows        
        //
        foreach (var row in Rows)
        {
            for (int index = 0; index < Columns.Count; ++index)
            {
                var column = Columns[index];
                if (column.Justification == ColumnJustification.Left)
                {
                    builder.Append((row[index] ?? "").PadRight(columnWidths[index]));
                }
                else
                {
                    if (index < row.Length)
                    {
                        builder.Append((row[index] ?? "").PadLeft(columnWidths[index]));
                    }
                    else
                    {
                        builder.Append("".PadLeft(columnWidths[index]));
                    }
                }

                if (index < Columns.Count - 1)
                {
                    builder.Append("  ");
                }
            }
            builder.AppendLine();
        }
        //
        // Print a footer.
        //
        builder.Append($"{SeparatorCharacter}".PadLeft(totalWidth, SeparatorCharacter));

        if (Footer != null)
        {
            builder.AppendLine();
            for (int index = 0; index < Columns.Count; ++index)
            {
                var column = Columns[index];
                string value = "";
                if (index < Footer.Length)
                {
                    value = Footer[index] ?? "";
                }
                if (column.Justification == ColumnJustification.Left)
                {
                    builder.Append(value.PadRight(columnWidths[index]));
                }
                else
                {
                    builder.Append(value.PadLeft(columnWidths[index]));
                }

                if (index < Columns.Count - 1)
                {
                    builder.Append("  ");
                }
            }
        }

        return builder.ToString();
    }
}
