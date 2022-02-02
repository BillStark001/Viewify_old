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
using Viewify;
using Viewify.Logic;

namespace Viewify.Controls
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
                    _reverseCtrlMapping.Clear();
                    _dataTypes.Clear();
                    _enumContainer.Clear();
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
        private readonly Dictionary<int, ParameterType> _dataTypes = new();
        private readonly Dictionary<string, Action> _oprs = new();
        private readonly Dictionary<string, int> _ctrlMapping = new();
        private readonly Dictionary<int, string> _reverseCtrlMapping = new();
        private readonly Dictionary<string, IEnumerable<EnumValue>> _enumFetcher = new();
        private readonly Dictionary<string, (VarRecord, DockPanel)> _enumContainer = new();

        public void RegisterCommand(string name, Action cmd)
        {
            _oprs[name] = cmd;
        }

        public void RegisterEnumVar(string name, IEnumerable<EnumValue>? val)
        {
            if (val != null)
                _enumFetcher[name] = val;
            // TODO
            if (_enumContainer.TryGetValue(name, out var wrapped))
            {
                var rc = wrapped.Item1;
                var panel = wrapped.Item2;
                var isRadio = rc.ControlType == ControlType.Radio;
                panel.Children.Clear();
                var (reten, gen, sen) = ControlUtils.MakeEnum(isRadio, _enumFetcher.GetValueOrDefault(name, new List<EnumValue>()));
                _dataExchange[rc.Id] = (gen, sen);
                panel.Children.Add(reten);
            }
            // else do nothing?
        }

        // values

        public object? GetValue(int id)
        {
            if (_dataExchange.TryGetValue(id, out var rid))
                return rid.Item1();
            return null;
        }

        public object? GetValue(string path)
        {
            return _ctrlMapping.TryGetValue(path, out var id) ? GetValue(id) : null;
        }

        public void SetValue(int id, object? val)
        {
            if (_dataExchange.TryGetValue(id, out var rid))
                rid.Item2(val);
        }

        public void SetValue(string path, object? val)
        {
            if (_ctrlMapping.TryGetValue(path, out var id))
                SetValue(id, val);
        }

        public Dictionary<int, ValueRecord> GetValues()
        {
            Dictionary<int, ValueRecord> ans = new();
            foreach (var (k, (g, s)) in _dataExchange)
            {
                var ansk = g();
                var ptype = _dataTypes[k];
                switch (ptype)
                {
                    case ParameterType.Bool:
                        ans[k] = new() { Bool = ValueUtils.ParseBoolean(ansk) };
                        break;
                    case ParameterType.String:
                    case ParameterType.Regex:
                        ans[k] = new() { String = ValueUtils.ParseString(ansk) };
                        break;
                    case ParameterType.Int:
                    case ParameterType.Enum:
                    case ParameterType.EnumVar:
                        ans[k] = new() { Int = ValueUtils.ParseInt(ansk) };
                        break;
                    case ParameterType.Decimal:
                        ans[k] = new() { Double = ValueUtils.ParseDouble(ansk) };
                        break;
                    case ParameterType.TextField:
                    case ParameterType.TextLabel:
                        // do nothing
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            return ans;
        }

        public void SetValues(Dictionary<int, ValueRecord> vs)
        {
            foreach (var (k, (g, s)) in _dataExchange)
            {
                vs.TryGetValue(k, out var v);
                s(v);
            }
        }

        // record related

        public UIElement ParseRecord(VarRecord rc, string path = "")
        {
            var rpath = path + "." + rc.Name;

            if (_dataExchange.TryGetValue(rc.Id, out var rid))
                throw new InvalidDataException($"Repeated ID: #{rc.Id} at {rc.ParameterType}${rpath} & {_dataTypes[rc.Id]}${_reverseCtrlMapping[rc.Id]}");
            else if (_ctrlMapping.TryGetValue(rpath, out var _))
                throw new InvalidDataException($"Repeated name: {rc.ParameterType}${rpath} at #{rc.Id}");

            _dataTypes[rc.Id] = rc.ParameterType;
            _reverseCtrlMapping[rc.Id] = rpath;
            if (!string.IsNullOrEmpty(rc.Name))
            {
                _ctrlMapping[rpath] = rc.Id;
                
            }
                
            switch (rc.ParameterType)
            {
                case ParameterType.Bool:
                    var elembl = new CheckBox()
                    {
                        Height = 20,
                        Content = rc.Description ?? rc.DisplayName ?? rc.Name ?? "",
                        VerticalAlignment = VerticalAlignment.Stretch, 
                    };
                    Func<object?> gbl = () => elembl.IsChecked ?? false;
                    Action<object?> sbl = (x) => elembl.IsChecked = ValueUtils.ParseBoolean(x);
                    _dataExchange[rc.Id] = (gbl, sbl);
                    return elembl;

                case ParameterType.String:
                    var elemstr = new TextBox();
                    Func<object?> gstr = () => elemstr.Text;
                    Action<object?> sstr = (x) => elemstr.Text = ValueUtils.ParseString(x) ?? "";
                    _dataExchange[rc.Id] = (gstr, sstr);
                    return elemstr;

                case ParameterType.Int:
                case ParameterType.Decimal:
                    var (retid, gid, sid) = ControlUtils.MakeDecimalBox(rc);
                    _dataExchange[rc.Id] = (gid, sid);
                    return retid;

                // enum

                case ParameterType.Enum:
                    var (reten, gen, sen) = ControlUtils.MakeEnum(rc);
                    _dataExchange[rc.Id] = (gen, sen);
                    return reten;

                case ParameterType.EnumVar:
                    var retenv = new DockPanel();
                    if (rc.CommandName == null)
                        throw new InvalidDataException("A 'cmd' string field indicating the related definition of an EnumVar type is required.");
                    _enumContainer[rc.CommandName] = (rc, retenv);
                    RegisterEnumVar(rc.CommandName, null);
                    return retenv;
                // control type

                case ParameterType.Button:
                    if (rc.CommandName == null)
                        throw new InvalidDataException("A 'cmd' string field indicating the command of a Button type is required.");
                    var retbutt = new Button()
                    {
                        Content = rc.Description ?? rc.DisplayName ?? rc.Name ?? "",
                        Height = 25, 
                        HorizontalAlignment = HorizontalAlignment.Left, 
                    };
                    retbutt.Click += (_, __) =>
                    {
                        if (rc.CommandName != null && _oprs.TryGetValue(rc.CommandName, out var act))
                            act();
                    };
                    return retbutt;


                case ParameterType.TextLabel:
                case ParameterType.TextField:
                    var rettxt = new TextBlock()
                    {
                        Text = rc.DefaultString ?? rc.Description ?? rc.DisplayName ?? rc.Name ?? "",
                    };
                    if (rc.ControlType == ControlType.WithEditor)
                    {
                        Func<object?> gtxt = () => rettxt.Text;
                        Action<object?> stxt = (x) => rettxt.Text = ValueUtils.ParseString(x) ?? "";
                        _dataExchange[rc.Id] = (gtxt, stxt);
                    }
                    return rettxt;


                // layout type

                case ParameterType.HStack:
                case ParameterType.VStack:
                    var elemhs = new StackPanel();
                    elemhs.Orientation = rc.ParameterType == ParameterType.HStack ? Orientation.Horizontal : Orientation.Vertical;
                    if (rc.SubControls != null)
                        foreach (var sub in rc.SubControls)
                            elemhs.Children.Add(ParseRecord(sub, rpath));
                    if (rc.ControlType == ControlType.WithMargin)
                    {
                        return new GroupBox()
                        {
                            Content = elemhs, 
                            Header = rc.DisplayName, 
                        };
                    }
                    return elemhs;

                case ParameterType.Separator:
                    var elemsp = new Separator();
                    return elemsp;

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
