﻿using System;
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
    public static class ControlUtils
    {

        public static string ReserveDigit(this double value, int digit = 3, bool round = false, bool fill = false)
        {
            if (digit == 0)
                return (round ? (long)(value > 0 ? value + 0.5 : value - 0.5) : (long)value).ToString();
            else if (digit > 0)
            {
                double delta = round ? 0 : 0.5 * Math.Pow(10, -digit);
                var ret = Math.Round((decimal)(value - delta), digit, MidpointRounding.AwayFromZero);
                return fill ? ret.ToString($"n{digit}") : ret.ToString();
            }
            else
            {
                var ndigit = -digit;
                long exp = (long)Math.Pow(10, ndigit);
                long i = value >= 0 ? (long)value : (long)-value;
                var div = (i / exp) * exp;
                var mod = i % exp;
                if (!round || mod < exp / 2)
                    return (value >= 0 ? div : -div).ToString();
                else
                    return (value >= 0 ? div + exp : -div - exp).ToString();
            }
        }

        public static string PathCombine(string parent, string? child)
        {
            if (parent == null)
                parent = string.Empty;
            if (child == null)
                child = string.Empty;

            if (parent.Contains(' ') || child.Contains(' '))
                throw new InvalidOperationException("Spaces are not allowed in a path.");

            if (string.IsNullOrWhiteSpace(parent))
                return child;
            if (string.IsNullOrWhiteSpace(child))
                child = "#EMPTY";
            return parent + '.' + child;
        }

        public static (UIElement, Func<object?>, Action<object?>) MakeDecimalBox(VarRecord rec, uint numDigit = 0)
        {
            var withBar = rec.ControlType == ControlType.ScrollBar;
            var isInteger = rec.ParameterType == ParameterType.Int;
            var def = rec.DefaultNumber;
            decimal defdefv = def != null ? def.Value.Item1 : 0;
            UIElement textInput;
            ScrollBar? sb = null;
            UIElement ret;
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
            return (ret, g, s);
        }


        public static UIElement MakeGroup(VarRecord rec, Func<VarRecord, UIElement> makeSubRec, bool collapsible, bool withMargin)
        {
            var innerGroup = new Grid();
            
            int r = 0;
            if (rec.SubControls != null)
                foreach (var subRec in rec.SubControls)
                {
                    if (subRec.ParameterType == ParameterType.Separator || 
                        subRec.ControlType == ControlType.IgnoreFieldInGroup)
                    {
                        var c1 = makeSubRec(subRec);
                        innerGroup.Children.Add(c1);
                        c1.SetValue(Grid.RowProperty, r);
                        c1.SetValue(Grid.ColumnProperty, 0);
                        c1.SetValue(Grid.ColumnSpanProperty, 2);
                    }
                    else
                    {
                        var c0 = new Label() { Content = subRec.TryLocalizeDisplayName() };
                        var c1 = makeSubRec(subRec);
                        innerGroup.Children.Add(c0);
                        innerGroup.Children.Add(c1);
                        c0.SetValue(Grid.RowProperty, r);
                        c0.SetValue(Grid.ColumnProperty, 0);
                        c1.SetValue(Grid.RowProperty, r);
                        c1.SetValue(Grid.ColumnProperty, 1);
                    }
                    ++r;
                    innerGroup.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto, MinHeight = 10 });
                }

            innerGroup.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            innerGroup.ColumnDefinitions.Add(new ColumnDefinition() { MinWidth = 150 });

            if (!collapsible)
            {
                if (!withMargin)
                    return innerGroup;
                else
                {
                    var g = new GroupBox()
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        Content = innerGroup,
                        Header = new Label() { Content = rec.TryLocalizeDisplayName2() },
                    };
                    return g;
                }
            }
            else
            {
                var ex = new Expander()
                {
                    Header = new Label() { Content = rec.TryLocalizeDisplayName2() },
                    Content = innerGroup,
                };
                return ex;
            }
        }

        public static (UIElement, Func<object?>, Action<object?>) MakeEnum(VarRecord rec)
        {
            var isVar = rec.ParameterType == ParameterType.EnumVar;
            var isRadio = rec.ControlType == ControlType.Radio;
            List<EnumValue>? ev = isVar ? null : rec.EnumValues;
            return MakeEnum(isRadio, ev);
        }

        public static (UIElement, Func<object?>, Action<object?>) MakeEnum(bool isRadio, IEnumerable<EnumValue>? enumValuesIn)
        {
            List<EnumValue> enumValues = enumValuesIn != null ? new(enumValuesIn) : new();
            if (enumValues.Count == 0)
                enumValues.Add(new(0, ""));

            UIElement ret;
            int currentId = enumValues[0].Id;
            Func<object?> getterFunc = () => currentId;
            Action<object?> setterFunc;

            if (isRadio)
            {
                StackPanel st = new();
                
                Dictionary<int, RadioButton> dr = new();
                foreach (var eobj in enumValues)
                {
                    var robj = new RadioButton()
                    {
                        Content = eobj.TryLocalizeDescription(),
                        Height = 20,
                    };
                    if (dr.TryGetValue(eobj.Id, out var _))
                        throw new InvalidDataException($"Repeated Enum ID: #{eobj.Id}");
                    dr[eobj.Id] = robj;
                    st.Children.Add(robj);
                    robj.Checked += (_, __) => currentId = eobj.Id;
                }
                setterFunc = (x) =>
                {
                    int xv = ValueUtils.ParseInt(x, currentId);
                    dr[currentId].IsChecked = false;
                    currentId = xv;
                    if (dr.TryGetValue(xv, out var dxv))
                        dxv.IsChecked = true;
                };
                ret = st;
            }
            else
            {
                ComboBox cb = new();
                Dictionary<int, ComboBoxItem> dr = new();
                foreach (var eobj in enumValues)
                {
                    var cobj = new ComboBoxItem()
                    {
                        Content = eobj.TryLocalizeDescription(),
                    };
                    if (dr.TryGetValue(eobj.Id, out var _))
                        throw new InvalidDataException($"Repeated Enum ID: #{eobj.Id}");
                    dr[eobj.Id] = cobj;
                    cb.Items.Add(cobj);
                    cobj.Selected += (_, __) => currentId = eobj.Id;
                }
                setterFunc = (x) =>
                {
                    int? xv = ValueUtils.ParseInt(x, currentId);
                    currentId = xv.Value;
                    if (dr.TryGetValue(xv.Value, out var dxv))
                        cb.SelectedItem = dxv;
                    else
                        cb.SelectedItem = null;
                };
                ret = cb;
            }
            setterFunc(currentId);
            return (ret, getterFunc, setterFunc);
        }

    }
}