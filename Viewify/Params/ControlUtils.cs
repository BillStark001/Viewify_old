using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Viewify.Base;
using Viewify;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.IO;
using System.Windows.Media;

namespace Viewify.Params
{
    public class ControlRecord
    {
        public FrameworkElement Element { get; }
        public Func<object?> Getter { get; }
        public Action<object?> Setter { get; }

        public ControlRecord(FrameworkElement e, Func<object?> g, Action<object?> s)
        {
            Element = e;
            Getter = g;
            Setter = s;
        }
    }

    public static partial class ControlUtils
    {

        public static ControlRecord MakeCheckBox(this VarRecord vRec)
        {
            var elem = new CheckBox()
            {
                Height = 20,
                Content = vRec.TryLocalizeDescription(),
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            return new(
                elem,
                () => elem.IsChecked ?? false,
                (x) => elem.IsChecked = ValueUtils.ParseBoolean(x)
            );
        }

        public static ControlRecord MakeTextBox(this VarRecord vRec)
        {
            var elem = new TextBox();
            return new(
                elem,
                () => elem.Text,
                (x) => elem.Text = ValueUtils.ParseString(x) ?? ""
            );
        }


        public static ControlRecord MakeDecimalBox(this VarRecord vRec)
        {
            var withBar = vRec.ControlType == ControlType.ScrollBar;
            var isInteger = vRec.ParameterType == ParameterType.Int;
            var def = vRec.DefaultNumber;
            decimal defdefv = def != null ? def.Value.Item1 : 0;
            FrameworkElement textInput;
            ScrollBar? sb = null;
            FrameworkElement ret;
            // initialize input box control
            var realInput = new NumericUpDown()
            {
                MinWidth = 120,
                MaxWidth = 200,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = double.NaN,
            };
            if (def != null)
            {
                realInput.Maximum = (double)def.Value.Item3;
                realInput.Minimum = (double)def.Value.Item2;
                realInput.Value = (double)def.Value.Item1;
            }
            else
                realInput.Value = 0;
            if (isInteger)
            {
                realInput.MaxReservedDigit = 0;
                realInput.RoundDisplay = true;
                realInput.FillDisplay = false;
            }
            else
            {
                realInput.MaxReservedDigit = 12;
                realInput.RoundDisplay = true;
                realInput.FillDisplay = false;
            }
            textInput = realInput;

            if (withBar && def != null)
            {
                var (defv, min, max) = def.Value;
                sb = new ScrollBar()
                {
                    Orientation = Orientation.Horizontal,
                    // HorizontalAlignment = HorizontalAlignment.Stretch,
                    Value = (double)defv,
                    Minimum = (double)min,
                    Maximum = (double)max,
                    MinWidth = 60,
                    Height = 22,
                };
                if (isInteger)
                {
                    realInput.ValueChanged += (_, __) => { sb.Value = realInput.RoundedIntValue; };
                    sb.ValueChanged += (_, __) => { realInput.Value = (int)sb.Value; };
                }
                else
                {
                    realInput.ValueChanged += (_, __) => { sb.Value = realInput.Value; };
                    sb.ValueChanged += (_, __) => { realInput.Value = (double)sb.Value; };
                }
                var rets = new DockPanel()
                {
                    // Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Width = double.NaN,
                };
                rets.Children.Add(textInput);
                textInput.SetValue(DockPanel.DockProperty, Dock.Left);
                rets.Children.Add(sb);
                sb.SetValue(DockPanel.DockProperty, Dock.Left);
                ret = rets;
            }
            else
            {
                ret = textInput;
            }
            Func<object?> g;
            Action<object?> s;
            if (isInteger)
            {
                g = () => realInput.RoundedIntValue;
                s = (x) => { var xi = ValueUtils.ParseInt(x); realInput.Value = xi; if (sb != null) sb.Value = xi; };
            }
            else
            {
                g = () => realInput.Value;
                s = (x) => { var xd = ValueUtils.ParseDouble(x); realInput.Value = xd; if (sb != null) sb.Value = xd; };
            }
            return new(ret, g, s);
        }

        // controls
        public delegate bool TryGetValue<T>(string key, out T? value);
        public static Button MakeButton(this VarRecord vRec, TryGetValue<Action> getAction)
        {
            if (vRec.CommandName == null)
                throw new InvalidDataException("A 'cmd' string field indicating the command of a Button type is required.");

            var retbutt = new Button()
            {
                Content = vRec.TryLocalizeDescription(),
                Height = 25,
                Padding = new Thickness(5, 2, 5, 2),
                HorizontalAlignment = HorizontalAlignment.Left,
            };
            retbutt.Click += (_, __) =>
            {
                if (getAction(vRec.CommandName, out var act)
                && act != null)
                    act();
            };
            return retbutt;
        }
    }
}
