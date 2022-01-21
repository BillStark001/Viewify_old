using System;
using System.Collections.Generic;
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
using Viewify.Logic;
using Viewify.Controls;
using System.Diagnostics;

namespace Viewify
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var c = new VarRecord()
            {
                Name = "root",
                DisplayName = "Root",
                ParameterType = ParameterType.Group,
                // ControlType = ControlType.NoMargin,
                SubControls = new()
                {
                    new()
                    {
                        Id = 1,
                        Name = "test1",
                        DisplayName = "Test1 With A display Name",
                        ParameterType = ParameterType.String,
                    },
                    new()
                    {
                        Id = -1,
                        Name = "test2",
                        ParameterType = ParameterType.Bool,
                        Description = "This is a checkbox.",
                    },
                    new() { ParameterType = ParameterType.Separator, },
                    new()
                    {
                        Id = 114514,
                        Name = "dectest1",
                        ParameterType = ParameterType.Int,
                        DefaultNumber = (1000000, 114514, 1919810),
                        // ControlType = ControlType.ScrollBar, 
                    },
                    new()
                    {
                        Id = 2,
                        Name = "enumtest",
                        ParameterType = ParameterType.Enum,
                        EnumValues = new()
                        {
                            new(0, "num0"),
                            new(1, "num1"),
                        },
                    },
                    new()
                    {
                        Id = 22,
                        Name = "enumtest2",
                        ParameterType = ParameterType.Enum,
                        ControlType = ControlType.Radio, 
                        EnumValues = new()
                        {
                            new(0, "num0"),
                            new(1, "num1"),
                        },
                    },
                    new()
                    {
                        Id = 23, 
                        Name = "enumvartest", 
                        CommandName = "testEnumVar", 
                        ParameterType = ParameterType.EnumVar, 
                        ControlType = ControlType.Radio
                    }, 
                    new()
                    {
                        ParameterType = ParameterType.Button,
                        ControlType = ControlType.IgnoreFieldInGroup,
                        CommandName = "test1",
                        Description = "これはボタンです",
                    },
                    new() { ParameterType = ParameterType.Separator, },
                    new()
                    {
                        Name = "anotherRoot",
                        DisplayName = "Another Root",
                        ParameterType = ParameterType.CollapsibleGroup,
                        ControlType = ControlType.IgnoreFieldInGroup,
                        SubControls =
                        new() {
                            new()
                            {
                                Name = "f1", 
                                Id = 321412, 
                                ControlType = ControlType.IgnoreFieldInGroup,
                                ParameterType = ParameterType.TextField,
                                DefaultString = "This is a multi-line test string. \n Corona team garter belt stockings.",
                            }, 
                        }, 
                    }, 
                }
            };
            var cs = VarRecordUtils.Serialize(c);
            Trace.WriteLine(cs);
            var c2 = VarRecordUtils.Deserialize(cs);
            ThePanel.InputJson = cs;
            ThePanel.RegisterEnumVar("testEnumVar", new List<EnumValue>()
            {
                new(114, "114"), 
                new(514, "514"), 
                new(1919, "1919"), 
                new(810, "810")
            });
            ThePanel.RegisterCommand("test1", () =>
            {
                var a = ThePanel.GetValue(".root.test1");
                MessageBox.Show(a != null ? a.ToString() : "null");
                ThePanel.SetValue(".root.anotherRoot.f1", a);

                var vs = ThePanel.GetValues();
                Trace.WriteLine(ValueUtils.Serialize2(vs));
                ThePanel.SetValues(vs);
            });
        }
    }
}
