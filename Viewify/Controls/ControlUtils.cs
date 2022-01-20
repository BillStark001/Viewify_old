using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Viewify.Logic;
using Viewify;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit;

namespace Viewify.Controls
{
    public static class ControlUtils
    {
        
        public static (UIElement, Func<object?>, Action<object?>) MakeDecimalBox(VarRecord rec, uint numDigit = 0)
        {
            var withBar = rec.ControlType == ControlType.ScrollBar;
            var isInteger = rec.ParameterType == ParameterType.Int;
            var def = rec.DefaultNumber;
            decimal defdefv = def != null ? def.Value.Item1 : 0;
            UIElement textInput;
            ScrollBar? sb = null;
            UIElement ret;
            if (isInteger)
            {
                var iud = new IntegerUpDown()
                {
                    Width = 120,
                };
                if (def != null)
                {
                    iud.Maximum = (int)def.Value.Item3;
                    iud.Minimum = (int)def.Value.Item2;
                    iud.Value = (int)def.Value.Item1;
                }
                else
                    iud.Value = 0;
                textInput = iud;
            }
            else
            {
                var nud = new DoubleUpDown()
                {
                    Width = 120,
                };
                if (def != null)
                {
                    nud.Maximum = (double)def.Value.Item3;
                    nud.Minimum = (double)def.Value.Item2;
                    nud.Value = (double)def.Value.Item1;
                }
                else
                    nud.Value = 0;
                textInput = nud;
            }
            if (withBar && def != null)
            {
                var (defv, min, max) = def.Value;
                sb = new ScrollBar()
                {
                    Orientation = Orientation.Horizontal,
                    Value = (double)defv,
                    Minimum = (double)min,
                    Maximum = (double)max,
                    MinWidth = 100,
                };
                if (textInput is IntegerUpDown iud)
                {
                    iud.ValueChanged += (_, __) => { sb.Value = iud.Value ?? (double)defv; };
                    sb.ValueChanged += (_, __) => { iud.Value = (int)sb.Value; };
                }
                else if (textInput is DoubleUpDown nud)
                {
                    nud.ValueChanged += (_, __) => { sb.Value = nud.Value ?? (double)defv; };
                    sb.ValueChanged += (_, __) => { nud.Value = (double)sb.Value; };
                }
                var rets = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Width = double.NaN,
                };
                rets.Children.Add(textInput);
                rets.Children.Add(sb);
                ret = rets;
            }
            else
            {
                ret = textInput;
            }
            Func<object?> g;
            Action<object?> s;
            if (textInput is IntegerUpDown iud2)
            {
                g = () => iud2.Value ?? (int)defdefv;
                s = (x) => { var xi = x as int?; if (xi != null) { iud2.Value = xi; if (sb != null) sb.Value = (int)xi; } };
            }
            else
            {
                var nud2 = (DoubleUpDown)textInput;
                g = () => nud2.Value ?? (double)defdefv;
                s = (x) => { var xi = x as double?; if (xi != null) { nud2.Value = xi; if (sb != null) sb.Value = (double)xi; } };
            }
            return (ret, g, s);
        }

        private static void Sb_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            throw new NotImplementedException();
        }

        public static UIElement MakeGroup(VarRecord rec, Func<VarRecord, UIElement> makeSubRec, bool collapsible, bool withMargin)
        {
            var innerGroup = new UniformGrid();
            innerGroup.Columns = 2;
            
            if (rec.SubControls != null)
                foreach (var subRec in rec.SubControls)
                {
                    innerGroup.Children.Add(new Label() { Content = subRec.DisplayName ?? subRec.Name });
                    innerGroup.Children.Add(makeSubRec(subRec));
                }
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
                        Header = new Label() { Content = rec.DisplayName ?? rec.Name },
                    };
                    return g;
                }
            }
            else
            {
                var ex = new Expander()
                {
                    Header = new Label() { Content = rec.DisplayName ?? rec.Name },
                    Content = innerGroup,
                };
                return ex;
            }
        }

    }
}
