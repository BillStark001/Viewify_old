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
    public static partial class ControlUtils
    {


        public static FrameworkElement MakeGroup(this VarRecord rec, Func<VarRecord, FrameworkElement> makeSubRec, bool collapsible, bool withMargin)
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

        public static ControlRecord MakeEnum(this VarRecord rec)
        {
            var isVar = rec.ParameterType == ParameterType.EnumVar;
            var isRadio = rec.ControlType == ControlType.Radio;
            List<EnumRecord>? ev = isVar ? null : rec.EnumValues;
            return MakeEnum(isRadio, ev);
        }

        public static ControlRecord MakeEnum(bool isRadio, IEnumerable<EnumRecord>? enumValuesIn)
        {
            List<EnumRecord> enumValues = enumValuesIn != null ? new(enumValuesIn) : new();
            if (enumValues.Count == 0)
                enumValues.Add(new(0, ""));

            FrameworkElement ret;
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
            return new(ret, getterFunc, setterFunc);
        }
    }
}
