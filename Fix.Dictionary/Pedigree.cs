namespace Fix;

public static partial class Dictionary
{

    public record Pedigree
    {
        public string? Added { get; init; }
        public string? AddedEP { get; init; }
        public string? Updated { get; init; }
        public string? UpdatedEP { get; init; }
        public string? Deprecated { get; init; }
        public string? DeprecatedEP { get; init; }

        public static Pedigree Empty { get; } = new Pedigree();

        public override string ToString()
        {
            var items = new List<String>();

            if (Added is string added)
            {
                items.Add($"Added = {added}");
            }

            if (AddedEP is string addedEP)
            {
                items.Add($"AddedEP = {addedEP}");
            }

            if (Updated is string updated)
            {
                items.Add($"Updated = {updated}");
            }

            if (UpdatedEP is string updatedEP)
            {
                items.Add($"UpdatedEP = {updatedEP}");
            }

            if (Deprecated is string deprecated)
            {
                items.Add($"Deprecated = {deprecated}");
            }

            if (DeprecatedEP is string deprecatedEP)
            {
                items.Add($"DeprecatedEP = {deprecatedEP}");
            }

            if (items.Count == 0)
            {
                return string.Empty;
            }

            return string.Join(", ", items);
        }
    }
}
