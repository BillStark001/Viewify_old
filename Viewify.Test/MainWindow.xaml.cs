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
                DisplayName = "Mod Build",
                ParameterType = ParameterType.VStack,
                ControlType = ControlType.WithMargin,
                SubControls = new()
                {
                    new()
                    {
                        Id = 1,
                        Name = "modToBuild",
                        ParameterType = ParameterType.EnumVar,
                        CommandName = "getModToBuild", 
                    },
                    new()
                    {
                        Name = "options", 
                        DisplayName = "Options",
                        ParameterType = ParameterType.VStack,
                        ControlType = ControlType.WithMargin,
                        SubControls = new()
                        {
                            new() { Id = 16, Name = "clearModCache", ParameterType = ParameterType.Bool, Description = "01. Clear Mod Cache", },
                            new() { Id = 17, Name = "clearCache", ParameterType = ParameterType.Bool, Description = "02. Clear Cache", },

                            new() { Id = 18, Name = "buildAptUi", ParameterType = ParameterType.Bool, Description = "03. Build Apt UI File", },
                            new()
                            {
                                ParameterType = ParameterType.EnumBool, 
                                ControlType = ControlType.Radio, 
                                EnumValues = new()
                                {
                                    new(-1, "doNothing", "Do Nothing"), 
                                    new(19, "buildGlobalData", "04. Build Global Data"),
                                    new(20, "buildEssentialData", "05. Build Essential Data"),
                                    new(21, "mergeAssets", "06. Merge Assets"),
                                }
                            }, 
                            /*
                            new() { Id = 19, Name = "buildGlobalData", ParameterType = ParameterType.Bool, Description = "04. Clear Mod Cache", },
                            new() { Id = 20, Name = "buildEssentialData", ParameterType = ParameterType.Bool, Description = "05. Clear Mod Cache", },
                            new() { Id = 21, Name = "mergeAssets", ParameterType = ParameterType.Bool, Description = "06. Clear Mod Cache", },
                            */
                            new() { Id = 22, Name = "fixNeutralAssets", ParameterType = ParameterType.Bool, Description = "07. Clear Mod Cache", },

                            new() { Id = 23, Name = "copyAdditionalFiles", ParameterType = ParameterType.Bool, Description = "08. Clear Mod Cache", },
                            new() { Id = 24, Name = "buildBig", ParameterType = ParameterType.Bool, Description = "09. Clear Mod Cache", },
                            new() { Id = 25, Name = "buildSkudef", ParameterType = ParameterType.Bool, Description = "10. Clear Mod Cache", },

                            new() { Id = 26, Name = "buildFullScreenIni", ParameterType = ParameterType.Bool, Description = "11. Clear Mod Cache", },
                            new() { Id = 27, Name = "buildWindowModeIni", ParameterType = ParameterType.Bool, Description = "12. Clear Mod Cache", },

                            new()
                            {
                                DisplayName = "Mod Version: ",
                                ParameterType = ParameterType.TextLabel,
                            },
                            new()
                            {
                                Id = 2,
                                Name = "modVersion",
                                ParameterType = ParameterType.String,
                            },
                            new()
                            {
                                DisplayName = "Skudef Name: ",
                                ParameterType = ParameterType.TextLabel,
                            },
                            new()
                            {
                                Id = 3,
                                Name = "skudefName",
                                ParameterType = ParameterType.String,
                            },
                        }, 
                    }, 

                    new()
                    {
                        ParameterType = ParameterType.TextLabel, 
                        Description = "test1\ntest2\ntest3\ntest4"
                    }, 

                    new()
                    {
                        Name = "buildMod", 
                        Id = 8, 
                        ParameterType = ParameterType.Button, 
                        DisplayName = "Build Mod", 
                        CommandName = "buildMod", 
                    }, 
                }
            };
            var cs = VarRecordUtils.Serialize(c);
            Trace.WriteLine(cs);
            var c2 = VarRecordUtils.Deserialize(cs);
            ThePanel.InputJson = cs;
            ThePanel.RegisterEnumVar("getModToBuild", new List<EnumValue>()
            {
                new(114, "114"), 
                new(514, "514"), 
                new(1919, "1919"), 
                new(810, "810")
            });
            ThePanel.RegisterCommand("buildMod", () =>
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
