using System.Net;

namespace FixClient.Models;

public partial class Session
{
    public IPEndPoint EndPoint()
    {
        if (Behaviour == Fix.Behaviour.Initiator)
        {
            return new IPEndPoint(Fix.Network.GetAddress(Host), Port);
        }

        return new IPEndPoint(0, Port);
    }

    public IPEndPoint BindEndPoint()
    {
        IPAddress? address = Fix.Network.GetLocalAddress(BindHost);
        IPEndPoint endPoint = address != null ? new IPEndPoint(address, BindPort) : new IPEndPoint(0, BindPort);
        return endPoint;
    }
}