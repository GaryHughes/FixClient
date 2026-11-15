/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: Network.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using System.Net;
using System.Net.Sockets;

namespace Fix;

public static class Network
{
    public static IPAddress GetAddress(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new Exception("Host has not been specified");

        IPAddress[] hostAddresses = Dns.GetHostAddresses(name);

        foreach (IPAddress address in hostAddresses)
        {
            if (address.AddressFamily != AddressFamily.InterNetwork)
            {
                continue;
            }

            return address;
        }

        throw new Exception($"Unable to retrieve address for the host {name}");
    }

    public static IPAddress? GetLocalAddress(string name)
    {
        name = name.Trim();

        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        try
        {
            IPAddress[] hostAddresses = Dns.GetHostAddresses(name);
            IPAddress[] localAddresses = Dns.GetHostAddresses(Dns.GetHostName());

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
        }
        catch (Exception)
        {
        }

        throw new Exception($"{name} is not a valid name or address for the local machine");
    }
}

