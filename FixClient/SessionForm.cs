using System.ComponentModel;

namespace FixClient;

partial class SessionForm : Form
{
    readonly CustomPropertyGrid _propertyGrid;
    Session? _session;

    public SessionForm()
    {
        InitializeComponent();
        _propertyGrid = new CustomPropertyGrid
        {
            Dock = DockStyle.Fill,
            ToolbarVisible = false,
            HelpVisible = false,
            PropertySort = PropertySort.Categorized
        };
        // Read only state depends on other property values (e.g. BeginString), refresh so it is re-evaluated.
        _propertyGrid.PropertyValueChanged += (o, args) => _propertyGrid.Refresh();
        _gridPlaceHolder.Controls.Add(_propertyGrid);
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public bool Readonly
    {
        get { return _propertyGrid.Enabled; }
        set
        {
            _propertyGrid.Enabled = !value;
            OK.Enabled = !value;
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Session? Session
    {
        get { return _session; }
        set
        {
            _session = value;
            _propertyGrid.SelectedObject = value;
        }
    }
}
