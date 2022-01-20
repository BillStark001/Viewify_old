using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Viewify.Controls;
using Viewify.Logic;

namespace Viewify
{
    /// <summary>
    /// ConfigPanel.xaml 的交互逻辑
    /// </summary>
    public partial class ConfigPanel : UserControl
    {
        // json property
        public static readonly DependencyProperty InputJsonProperty = DependencyProperty.Register(
                "InputJson", typeof(string), typeof(ConfigPanel),
                new((d, e) => {
                    var thisVar = d as ConfigPanel;
                    // var tjson = thisVar != null ? thisVar.InputJson : null;
                    if (thisVar != null)
                        thisVar.Refresh();
                })
            );
        public string? InputJson
        {
            get { return (string?) GetValue(InputJsonProperty); }
            set { SetValue(InputJsonProperty, value); }
        }

        // record related

        public void Refresh()
        {
            try
            {
                RootRecord = VarRecordUtils.Deserialize(InputJson ?? "");
                Trace.WriteLine("RootRecord loaded");
                BaseControl.Children.Clear();
                if (RootRecord != null)
                {
                    _dataExchange.Clear();
                    _ctrlMapping.Clear();
                    BaseControl.Children.Add(ParseRecord(RootRecord));
                }
                else
                {
                    BaseControl.Children.Add(new TextBlock()
                    {
                        Text = "A VarRecord of value null is assigned. "
                    });
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine("An error occured while refreshing the panel:");
                Trace.WriteLine(e);
                BaseControl.Children.Clear();
                BaseControl.Children.Add(new TextBlock()
                {
                    Text = "An error occured while refreshing the panel:"
                });
                BaseControl.Children.Add(new TextBlock()
                {
                    Text = e.ToString()
                });
            }
        }

        private VarRecord? _rootRecord;
        public VarRecord? RootRecord
        {
            get { return _rootRecord; }
            set {
                _rootRecord = value;
            }
        }

        private readonly Dictionary<int, (Func<object?>, Action<object?>)> _dataExchange = new(); // getter, setter
        private readonly Dictionary<string, Action> _oprs = new();
        private readonly Dictionary<string, int> _ctrlMapping = new();

        public UIElement ParseRecord(VarRecord rc, string path = "")
        {
            var rpath = path + "." + rc.Name;

            if (_dataExchange.TryGetValue(rc.Id, out var rid))
                throw new InvalidDataException($"Repeated ID: #${rc.Id} at ${rpath}");
            else if (_ctrlMapping.TryGetValue(rpath, out var _))
                throw new InvalidDataException($"Repeated name: ${rpath} at #${rc.Id}");

            _ctrlMapping[rpath] = rc.Id;
            switch (rc.ParameterType)
            {
                case ParameterType.Bool:
                    var elembl = new CheckBox();
                    Func<object?> gbl = () => elembl.IsChecked ?? false;
                    Action<object?> sbl = (x) => elembl.IsChecked = (x as bool?) ?? false;
                    _dataExchange[rc.Id] = (gbl, sbl);
                    return elembl;

                case ParameterType.String:
                    var elemstr = new TextBox();
                    Func<object?> gstr = () => elemstr.Text;
                    Action<object?> sstr = (x) => elemstr.Text = x != null ? (x as string ?? x.ToString()) ?? "" : "";
                    _dataExchange[rc.Id] = (gstr, sstr);
                    return elemstr;

                case ParameterType.HStack:
                case ParameterType.VStack:
                    var elemhs = new StackPanel();
                    elemhs.Orientation = rc.ParameterType == ParameterType.HStack ? Orientation.Horizontal : Orientation.Vertical;
                    if (rc.SubControls != null)
                        foreach (var sub in rc.SubControls)
                            elemhs.Children.Add(ParseRecord(sub, rpath));
                    return elemhs;

                case ParameterType.Group:
                case ParameterType.CollapsibleGroup:
                    var elemg = ControlUtils.MakeGroup(rc, sr => ParseRecord(sr, rpath), rc.ParameterType == ParameterType.CollapsibleGroup, rc.ControlType != ControlType.NoMargin);
                    return elemg;

                default:
                    throw new NotImplementedException();
            }
            
        }

        public ConfigPanel()
        {
            InitializeComponent();
        }
    }
}
