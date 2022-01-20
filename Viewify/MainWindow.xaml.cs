﻿using System;
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
                    },
                }
            };
            ThePanel.InputJson = VarRecordUtils.Serialize(c);
        }
    }
}
