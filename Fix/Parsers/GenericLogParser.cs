namespace Fix.Parsers;

public class GenericLogParser : LogParser
{
    protected override async Task<Message?> ParseMessage(TextReader reader)
    {
        bool incoming = false;
        string body = string.Empty;

        for (; ; )
        {
            var line = await reader.ReadLineAsync().ConfigureAwait(false);

            if (line is null)
            {
                return null;
            }

            var position = line.IndexOf("8=FIX");

            if (position < 0)
            {
                if (body.Length == 0)
                {
                    continue;
                }

                // This is a hack to support Atlas logs that can be broken over multiple lines.
                position = line.IndexOf(" IN ");

                if (position < 0)
                {
                    position = line.IndexOf(" OUT ");
                }

                if (position < 0)
                {
                    continue;
                }
            }

            var direction = line.Substring(0, position);

            if (direction.Contains("IN") || direction.Contains("Incoming") || direction.Contains("Receiving"))
            {
                incoming = true;
            }

            body += line.Substring(position);

            if (body.Contains("10="))
            {
                return new Message(body)
                {
                    Incoming = incoming
                };
            }
        }
    }
}

