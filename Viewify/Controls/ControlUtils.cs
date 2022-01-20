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

namespace Viewify.Controls
{
    public static class ControlUtils
    {

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
