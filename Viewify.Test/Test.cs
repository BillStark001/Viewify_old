using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Viewify.Logic;
using Newtonsoft.Json;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Drawing;

namespace Viewify
{
    class Test
    {
        static void Main(string[] args)
        {
            Trace.WriteLine("114514");
            Style rowStyle = new Style(typeof(DataGridRow));

            Setter setter = new Setter(DataGridRow.BackgroundProperty, Brushes.Red);

            rowStyle.Setters.Add(setter);

            var j = JsonConvert.SerializeObject(setter, Formatting.Indented);
            Trace.WriteLine(j);
            var ij = JsonConvert.DeserializeObject<Setter>(j);
        }
    }
}
