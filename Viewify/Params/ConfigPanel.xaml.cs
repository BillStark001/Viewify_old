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
using Viewify.Base;

namespace Viewify.Params
{
    /// <summary>
    /// ConfigPanel.xaml 的交互逻辑
    /// </summary>
    public partial class ConfigPanel : UserControl
    {
        // json property
        public static readonly DependencyProperty InputJsonProperty = DependencyProperty.Register(
                "InputJson", typeof(string), typeof(ConfigPanel),
                new((d, e) =>
                {
                    var thisVar = d as ConfigPanel;
                    if (thisVar != null)
                        thisVar.Refresh();
                })
            );

        public static readonly DependencyProperty DisplayErrorProperty = DependencyProperty.Register(
                "DisplayError", typeof(bool), typeof(ConfigPanel),
                new((d, e) =>
                {
                    // do nothing
                })
            );

        /// <summary>
        /// requires manual refresh
        /// </summary>
        public string? InputJson
        {
            get { return (string?)GetValue(InputJsonProperty); }
            set { SetValue(InputJsonProperty, value); }
        }
        public bool DisplayError
        {
            get { var r = (bool?)GetValue(DisplayErrorProperty); return r != null ? r.Value : false; }
            set { SetValue(DisplayErrorProperty, value); }
        }


        // TODO!!!!!!!!
        public Dictionary<string, Style> StyleStore = new();

        // record related

        void DisplayErrorMessage(string str)
        {
            BaseControl.Children.Clear();
            var rtb = new RichTextBox
            {
                IsReadOnly = true,
            };
            rtb.AppendText(str);
            BaseControl.Children.Add(rtb);
        }

        public Exception? LastException { get; private set; } = null;
        public bool HasFunctionalUI { get; private set; } = false;

        public void RefreshWiththrow()
        {
            Refresh();
            if (!HasFunctionalUI)
                throw LastException ?? new InvalidOperationException("UI refreshing failed with unknown reason.");
        }

        public void Refresh(VarRecord? record = null)
        {
            HasFunctionalUI = false;
            try
            {
                RootRecord = record ?? VarRecordUtils.Deserialize(InputJson ?? "");
                Trace.WriteLine("RootRecord loaded");
                BaseControl.Children.Clear();
                if (RootRecord != null)
                {
                    _dataExchange.Clear();
                    _enableMapping.Clear();
                    _dataTypes.Clear();

                    _ctrlMapping.Clear();
                    _reverseCtrlMapping.Clear();
                    _lazyRegistry.Clear();
                    _lazyRegistry2.Clear();
                    
                    _enumContainer.Clear();

                    BaseControl.Children.Add(ParseRecord(RootRecord));
                    LastException = null;
                    HasFunctionalUI = true;

                    while (_lazyRegistry.Count > 0)
                    {
                        var (vRec, path, ctrl) = _lazyRegistry.Dequeue();
                        int id = 0;
                        while (_reverseCtrlMapping.ContainsKey(id))
                            id = Random.Shared.Next(0x7fffffff);
                        TryRegisterNewData(id, path, vRec.ParameterType, ctrl);
                    }
                    while (_lazyRegistry2.Count > 0)
                    {
                        var (vRec, path, ctrl) = _lazyRegistry.Dequeue();
                        int id = 0;
                        while (_reverseCtrlMapping.ContainsKey(id))
                            id = Random.Shared.Next(0x7fffffff);
                        TryRegisterNewData(id, path, vRec.ParameterType, ctrl);
                    }
                }
                else
                {
                    var errTxt = "A VarRecord of value null is assigned.".TryLocalize();
                    if (DisplayError)
                        DisplayErrorMessage(errTxt);
                    LastException = new InvalidOperationException(errTxt);
                }
            }
            catch (Exception e)
            {
                LastException = e;
                var errTxt = "An error occured while refreshing the panel:".TryLocalize();
                Trace.WriteLine(e);
                if (DisplayError)
                    DisplayErrorMessage(errTxt + "\n" + e.ToString());
            }
        }

        public VarRecord? RootRecord
        {
            get; private set;
        }

        private readonly Dictionary<int, (Func<object?>, Action<object?>)> _dataExchange = new(); // getter, setter
        private readonly Dictionary<int, (Func<bool>, Action<bool>)> _enableMapping = new();
        private readonly Dictionary<int, ParameterType> _dataTypes = new();

        private readonly Dictionary<string, int> _ctrlMapping = new();
        private readonly Dictionary<int, string> _reverseCtrlMapping = new();
        private readonly Queue<(VarRecord, string, FrameworkElement)> _lazyRegistry = new();
        private readonly Queue<(VarRecord, string, ControlRecord)> _lazyRegistry2 = new();

        private readonly Dictionary<string, Action> _oprs = new();

        private readonly Dictionary<string, IEnumerable<EnumRecord>> _enumFetcher = new();
        private readonly Dictionary<string, (VarRecord, DockPanel)> _enumContainer = new();


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
                    case ParameterType.EnumBool:
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

        // enable/disable related

        public bool IsControlEnabled(int id)
        {
            if (_enableMapping.TryGetValue(id, out var v))
            {
                return v.Item1();
            }
            return false;
        }

        public void SetControlEnablement(int id, bool value)
        {
            if (_enableMapping.TryGetValue(id, out var v))
            {
                v.Item2(value);
            }
        }

        public bool IsControlEnabled(string path)
        {
            return _ctrlMapping.TryGetValue(path, out var v) && IsControlEnabled(v);
        }

        public void SetControlEnablement(string path, bool value)
        {
            if (_ctrlMapping.TryGetValue(path, out var v))
                SetControlEnablement(v, value);
        }

        public Dictionary<int, bool> GetEnablements()
        {
            Dictionary<int, bool> ret = new();
            foreach (var (k, (g, s)) in _enableMapping)
                ret[k] = g();
            return ret;
        }

        public void SetEnablements(Dictionary<int, bool> enbIn, bool enbDefault = true)
        {
            foreach (var (k, (g, s)) in _enableMapping)
            {
                if (enbIn.TryGetValue(k, out var v))
                    s(v);
                else
                    s(enbDefault);
            }
        }

        // operation register related
        public void RegisterCommand(string name, Action cmd)
        {
            _oprs[name] = cmd;
        }

        public void RegisterEnumVar(
            string name, 
            IEnumerable<EnumRecord>? val, 
            string? rpath = null, 
            bool ignoreCheck = true)
        {
            if (val != null)
                _enumFetcher[name] = val;
            if (_enumContainer.TryGetValue(name, out var wrapped))
            {
                var vRec = wrapped.Item1;
                var container = wrapped.Item2;
                var isRadio = vRec.ControlType == ControlType.Radio;
                container.Children.Clear();
                var rec = ControlUtils.MakeEnum(isRadio, _enumFetcher.GetValueOrDefault(name, new List<EnumRecord>()));
                TryRegisterNewData(vRec, rpath, new ControlRecord(container, rec.Getter, rec.Setter), ignoreCheck);
                container.Children.Add(rec.Element);
            }
            // else do nothing
        }

        // record related

        public void TryRegisterNewData(int id, string? rpath, ParameterType type, FrameworkElement element, bool ignoreCheck = false)
        {


            // check conflict
            if (ignoreCheck) { /* do nothing */ }
            else if (_dataExchange.ContainsKey(id))
                throw new InvalidDataException($"Repeated ID: #{id} at {type}${rpath} & {_dataTypes[id]}${(_reverseCtrlMapping.TryGetValue(id, out var n) ? n : "[Anonymous]")}");
            else if (!string.IsNullOrWhiteSpace(rpath) && _ctrlMapping.TryGetValue(rpath, out var rid))
                throw new InvalidDataException($"Repeated name: {type}${rpath} at #{id} & #{rid}");

            _dataTypes[id] = type;
            if (!string.IsNullOrWhiteSpace(rpath))
            {
                _ctrlMapping[rpath] = id;
                _reverseCtrlMapping[id] = rpath;
            }
            _enableMapping[id] = (
                () => element.IsEnabled,
                (x) => element.IsEnabled = x
            );
        }

        public void TryRegisterNewData(int id, string? rpath, ParameterType type, ControlRecord crec, bool ignoreCheck = false)
        {
            TryRegisterNewData(id, rpath, type, crec.Element, ignoreCheck);
            _dataExchange[id] = (crec.Getter, crec.Setter);
        }

        public void TryRegisterNewData(VarRecord vRec, string rpath, FrameworkElement element, bool ignoreCheck = false)
        {
            if (vRec.IsIdEmpty && vRec.IsNameEmpty)
            { }
            else if (vRec.IsIdEmpty)
                _lazyRegistry.Enqueue((vRec, rpath, element));
            else
                TryRegisterNewData(vRec.Id!.Value, rpath, vRec.ParameterType, element, ignoreCheck);
        }

        public void TryRegisterNewData(VarRecord vRec, string rpath, ControlRecord crec, bool ignoreCheck = false)
        {
            if (vRec.IsIdEmpty && vRec.IsNameEmpty)
            { }
            else if (vRec.IsIdEmpty)
                _lazyRegistry2.Enqueue((vRec, rpath, crec));
            else
                TryRegisterNewData(vRec.Id!.Value, rpath, vRec.ParameterType, crec, ignoreCheck);
        }

        public FrameworkElement ParseRecord(VarRecord vRec, string inPath = "")
        {
            var currentPath = vRec.GetProperPath(inPath);
            Trace.WriteLine(currentPath);

            FrameworkElement? ctrl = null;
            ControlRecord? cRec = null;
            bool doRegisterControl = true;
            bool doRegisterData = false;

            switch (vRec.ParameterType)
            {
                case ParameterType.Bool:
                    doRegisterData = true;
                    cRec = vRec.MakeCheckBox();
                    break;

                case ParameterType.String:
                    doRegisterData = true;
                    cRec = vRec.MakeTextBox();
                    break;

                case ParameterType.Int:
                case ParameterType.Decimal:
                    doRegisterData = true;
                    cRec = vRec.MakeDecimalBox();
                    break;

                // enum
                case ParameterType.Enum:
                case ParameterType.EnumBool:
                    cRec = vRec.MakeEnum();
                    if (vRec.ParameterType == ParameterType.EnumBool && vRec.EnumValues != null)
                    {
                        foreach (var ev in vRec.EnumValues)
                        {

                            TryRegisterNewData(
                                ev.Id, 
                                ev.GetProperPath(inPath),
                                vRec.ParameterType,
                                new ControlRecord(
                                    cRec.Element,
                                    () =>ValueUtils.ParseInt(cRec.Getter()) == ev.Id,
                                    (x) =>
                                    {
                                        if (ValueUtils.ParseBoolean(x))
                                            cRec.Setter(ev.Id);
                                        else
                                            cRec.Setter(null);
                                    }
                                )
                            );
                        }
                    }
                    else
                    {
                        doRegisterData = true;
                    }
                    break;

                case ParameterType.EnumVar:
                    var container = new DockPanel();
                    if (vRec.CommandName == null)
                        throw new InvalidDataException("A 'cmd' string field indicating the related definition of an EnumVar type is required.");
                    _enumContainer[vRec.CommandName] = (vRec, container);
                    RegisterEnumVar(vRec.CommandName, null, currentPath, false);
                    ctrl = container;
                    doRegisterControl = false;
                    break;


                // control type

                case ParameterType.Button:
                    ctrl = vRec.MakeButton(_oprs.TryGetValue);
                    break;


                case ParameterType.TextLabel:
                case ParameterType.TextField:
                    var rettxt = new TextBlock()
                    {
                        Text = vRec.TryLocalizeDescription(),
                    };
                    ctrl = rettxt;
                    if (vRec.ControlType == ControlType.WithEditor)
                    {
                        doRegisterData = true;
                        cRec = new(rettxt,
                            () => rettxt.Text, 
                            (x) => rettxt.Text = ValueUtils.ParseString(x) ?? ""
                        );
                    }
                    break;


                // layout type

                case ParameterType.HStack:
                case ParameterType.VStack:
                    var stack = new StackPanel();
                    stack.Orientation = 
                        vRec.ParameterType == ParameterType.HStack ? 
                        Orientation.Horizontal : 
                        Orientation.Vertical;

                    if (vRec.SubControls != null)
                        foreach (var sub in vRec.SubControls)
                            stack.Children.Add(ParseRecord(sub, currentPath));

                    if (vRec.ControlType == ControlType.WithMargin)
                        ctrl = new GroupBox()
                        {
                            Content = stack,
                            Header = vRec.TryLocalizeDisplayName2(),
                        };
                    else
                        ctrl = stack;
                    break;

                case ParameterType.Separator:
                    ctrl = new Separator();
                    break;

                case ParameterType.Group:
                case ParameterType.CollapsibleGroup:
                    ctrl = ControlUtils.MakeGroup(
                        vRec, sr => 
                        ParseRecord(sr, currentPath), 
                        vRec.ParameterType == ParameterType.CollapsibleGroup, 
                        vRec.ControlType != ControlType.NoMargin);
                    break;

                default:
                    throw new NotImplementedException();
            }

            ctrl = ctrl ?? cRec?.Element;

            if (ctrl != null && !string.IsNullOrWhiteSpace(vRec.StyleStr))
            {
                if (StyleStore.TryGetValue(vRec.StyleStr, out var style))
                    ctrl.Style = style;
            }

            if (ctrl == null)
                throw new InvalidOperationException("no control returned");
            if (doRegisterData && cRec == null)
                throw new InvalidOperationException("no data registry assigned");

            if (doRegisterData)
                TryRegisterNewData(vRec, currentPath, cRec!);
            else if (doRegisterControl)
            {
                TryRegisterNewData(vRec, currentPath, ctrl);
            }
                

            return ctrl;

        }

        public ConfigPanel()
        {
            InitializeComponent();
        }
    }
}
