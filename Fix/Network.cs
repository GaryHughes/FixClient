using System.Net;
using System.Net.Sockets;

namespace Fix;

public static class Network
{
    public static IPAddress GetAddress(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Host has not been specified", nameof(name));

        IPAddress[] hostAddresses = Dns.GetHostAddresses(name);

        foreach (IPAddress address in hostAddresses)
        {
            if (address.AddressFamily != AddressFamily.InterNetwork)
            {
                continue;
            }

            return address;
        }

        throw new ArgumentException($"Unable to retrieve address for the host {name}", nameof(name));
    }

    public static IPAddress? GetLocalAddress(string name)
    {
        name = name.Trim();

        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        string message = $"{name} is not a valid name or address for the local machine";

        IPAddress[] hostAddresses;
        IPAddress[] localAddresses;

        try
        {
            hostAddresses = Dns.GetHostAddresses(name);
            localAddresses = Dns.GetHostAddresses(Dns.GetHostName());
        }
        catch (Exception ex) when (ex is SocketException or ArgumentException)
        {
            throw new ArgumentException(message, nameof(name), ex);
        }

        foreach (IPAddress address in hostAddresses)
        {
            if (address.AddressFamily != AddressFamily.InterNetwork)
            {
                continue;
            }

            if (IPAddress.IsLoopback(address))
            {
                return address;
            }

            if (localAddresses.Contains(address))
            {
                return address;
            }
        }

        throw new ArgumentException(message, nameof(name));
    }
}

