/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: ConnectForm.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace FixClient;

public partial class ConnectForm : Form
{
    Timer? _timer;
    readonly IPEndPoint _endPoint;
    readonly IPEndPoint _bindEndPoint;
    readonly Fix.Behaviour _behaviour;
    TcpClient? _tcpClient;
    TcpSocketListener? _tcpListener;

    public ConnectForm(IPEndPoint bindEndPoint, IPEndPoint endPoint, Fix.Behaviour behaviour)
    {
        InitializeComponent();
        _endPoint = endPoint;
        _bindEndPoint = bindEndPoint;
        _behaviour = behaviour;
        Load += ConnectFormLoad;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Stream? Stream { get; private set; }

    void ConnectFormLoad(object? sender, EventArgs e)
    {
        _timer = new Timer { Interval = 100 };
        _timer.Tick += TimerTick;
        _timer.Start();
        progressBar.Enabled = true;

        if (_behaviour == Fix.Behaviour.Initiator)
        {
            _tcpClient = _bindEndPoint == null ? new TcpClient() : new TcpClient(_bindEndPoint);
            _tcpClient.BeginConnect(_endPoint.Address, _endPoint.Port, result =>
            {
                try
                {
                    _tcpClient.EndConnect(result);
                    Stream = _tcpClient.GetStream();
                    DialogResult = DialogResult.OK;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(null, ex.Message, "Fix Initiator", MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                    DialogResult = DialogResult.Cancel;
                }
                BeginInvoke(new MethodInvoker(Close), new object[] { this, EventArgs.Empty });
            }, null);
        }
        else
        {
            _tcpListener = new TcpSocketListener(_endPoint);
            _tcpListener.Start();
            _tcpListener.BeginAcceptSocket(result =>
            {
                try
                {
                    Stream = new NetworkStream(_tcpListener.EndAcceptSocket(result), true);
                    DialogResult = DialogResult.OK;
                }
                catch (ObjectDisposedException)
                {
                    //
                    // This is expected if the socket is closed.
                    //
                }
                catch (Exception ex)
                {
                    MessageBox.Show(null, ex.Message, "Fix Initiator", MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                    DialogResult = DialogResult.Cancel;
                }
                _tcpListener.Stop();
            }, null);
        }
    }

    void TimerTick(object? sender, EventArgs e)
    {
        if (progressBar.Value == progressBar.Maximum)
        {
            progressBar.Value = progressBar.Minimum;
            return;
        }

        progressBar.PerformStep();
    }

    void CancelButtonClick(object sender, EventArgs e)
    {
        _tcpClient?.Close();
        _tcpListener?.Stop();
        Close();
    }
}
