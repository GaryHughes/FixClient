/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: DictionaryPanel.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Threading;

namespace FixClient;

public partial class DictionaryPanel : FixClientPanel
{
    readonly WebBrowser _browser;

    readonly ToolStripButton _backButton;
    readonly ToolStripButton _forwardButton;

    public DictionaryPanel()
    {
        InitializeComponent();

        _browser = new WebBrowser { Dock = DockStyle.Fill };
        _browser.ScriptErrorsSuppressed = true;
        _browser.Navigate("https://www.onixs.biz/fix-dictionary.html");

        ContentPanel.Controls.Add(_browser);

        #region ToolStrip
        var toolStrip = new ToolStrip
        {
            GripStyle = ToolStripGripStyle.Hidden,
            BackColor = LookAndFeel.Color.ToolStrip,
            Renderer = new ToolStripRenderer()
        };
        TopToolStripPanel.Join(toolStrip);

        _backButton = new ToolStripButton
        {
            ToolTipText = "Back",
            Image = Properties.Resources.Back,
            ImageTransparentColor = Color.Magenta
        };
        _backButton.Click += (sender, ev) => _browser.GoBack();
        toolStrip.Items.Add(_backButton);

        _forwardButton = new ToolStripButton
        {
            ToolTipText = "Forward",
            Image = Properties.Resources.Forward,
            ImageTransparentColor = Color.Magenta
        };
        _forwardButton.Click += (sender, ev) => _browser.GoForward();
        toolStrip.Items.Add(_forwardButton);
        #endregion
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Fix.Message Message
    {
        set
        {
            //
            //"http://onixs.biz/fixdictionary/4.2/msgType_D_68.html"
            //
            string numeric;

            if (int.TryParse(value.MsgType, out int result))
            {
                numeric = value.MsgType;
            }
            else
            {
                var builder = new StringBuilder();

                foreach (char c in value.MsgType)
                {
                    builder.Append(Convert.ToInt16(c));
                }

                numeric = builder.ToString();
            }

            var uri = new Uri(string.Format("http://onixs.biz/fix-dictionary/{0}/msgType_{1}_{2}.html",
                                            value.BeginString.Substring(4, 3),
                                            value.MsgType,
                                            numeric));

            ThreadPool.QueueUserWorkItem(delegate
                                                {
                                                    try
                                                    {
                                                        _browser.Navigate(uri);
                                                    }
                                                    catch (Exception)
                                                    {
                                                        //DisplayError(this, new ErrorEventArgs { Message = ex.Message });
                                                    }
                                                });
        }
    }
}
