namespace Fix;

public abstract class LogParser
{
    public async IAsyncEnumerable<Message> Parse(Stream stream)
    {
        using var reader = new StreamReader(stream);

        while (true)
        {
            var message = await ParseMessage(reader).ConfigureAwait(false);

            if (message is null)
            {
                yield break;
            }

            yield return message;
        }
    }

    protected abstract Task<Message?> ParseMessage(TextReader reader);

}

