using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Viewify.Logic;
using Newtonsoft.Json;

namespace Viewify
{
    class Test
    {
        static void Main(string[] args)
        {
            Trace.WriteLine("114514");

            var c = new VarRecord()
            {
                Name = "root",
                ParameterType = ParameterType.Group,
                ControlType = ControlType.NoMargin,
                SubControls = new()
                {
                    new()
                    {
                        Id = 1, 
                        Name = "test1", 
                        ParameterType = ParameterType.String, 
                    }, 
                    new()
                    {
                        Id = -1,
                        Name = "test2", 
                        ParameterType = ParameterType.Bool,
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
                        Id = 2222,
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
                        Id = 222,
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
                        Id = 232,
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
                        new()
                        {
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
            string cis = VarRecordUtils.Serialize(c);
            VarRecord ci = VarRecordUtils.Deserialize(cis)!;
            Trace.WriteLine(cis);
            Trace.WriteLine(VarRecordUtils.Serialize(ci));
        }
    }
}
