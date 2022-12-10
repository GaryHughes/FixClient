using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Media;

namespace FixClient;

public partial class ParserPanel : FixClientPanel
{
    readonly ToolStripCheckBox _showAdminMessageCheckBox;

    readonly TextBlock _textBlock = new TextBlock()
    {
        FontFamily = new System.Windows.Media.FontFamily("Consolas"),
        Background = System.Windows.Media.Brushes.White,
        Padding = new System.Windows.Thickness(10)
        
    };

    string? _filename;

    public ParserPanel()
    {
        InitializeComponent();

        #region ToolStrip
        var loadButton = new ToolStripButton(Properties.Resources.Open)
        {
            ImageTransparentColor = System.Drawing.Color.Magenta,
            ToolTipText = "Load a Glue/Gate/FIX Router/Desk Server log file"
        };
        loadButton.Click += async (sender, ev) => await LoadClientMessagesButtonClick(sender, ev);

        _showAdminMessageCheckBox = new ToolStripCheckBox();
        _showAdminMessageCheckBox.CheckChanged += async (sender, ev) => await ShowAdminMessageCheckBoxCheckChanged(sender, ev);

        var toolStrip = new ToolStrip(new ToolStripItem[]
        {
            loadButton,
            new ToolStripLabel("Show Administrative Messages"),
            _showAdminMessageCheckBox,
        })
        {
            GripStyle = ToolStripGripStyle.Hidden,
            BackColor = LookAndFeel.Color.ToolStrip,
            Renderer = new ToolStripRenderer()
        };

        #endregion

        var host = new ElementHost()
        {
            Dock = DockStyle.Fill,
            Child = new ScrollViewer() { Content = _textBlock }
        };

        TopToolStripPanel.Join(toolStrip);
        ContentPanel.Controls.Add(host);
    }

    async Task ShowAdminMessageCheckBoxCheckChanged(object? sender, EventArgs e)
    {
        if (_filename is string filename)
        {
            await LoadFile(filename);
        }
    }

    async Task LoadClientMessagesButtonClick(object? sender, EventArgs e)
    {
        using OpenFileDialog dlg = new();

        if (dlg.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        await LoadFile(dlg.FileName);
    }

    async Task LoadFile(string filename)
    { 
        Cursor? original = Cursor.Current;
        Cursor.Current = Cursors.WaitCursor;

        try
        {
            _textBlock.Text = string.Empty;
             
            var orderBook = new Fix.OrderBook();

            var url = new Uri($"file://{Path.GetFullPath(filename)}");

            await foreach (var message in Fix.Parser.Parse(url))
            {
                if (!_showAdminMessageCheckBox.Checked && message.Administrative)
                {
                    continue;
                }

                _textBlock.Text += message.PrettyPrint() + "\r\n";

                if (orderBook.Process(message) == Fix.OrderBookMessageEffect.Modified)
                {
                    var report = new Fix.OrderReport(orderBook)
                    {
                        SeparatorCharacter = '\u2500'
                    };
                    using var stream = new MemoryStream();
                    report.Print(stream);
                    stream.Flush();
                    _textBlock.Text += Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Position) + "\r\n";
                }
            }

            _filename = filename;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this,
                            ex.Message,
                            Application.ProductName,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
        }
        finally
        {
            Cursor.Current = original;
        }
    }
}
