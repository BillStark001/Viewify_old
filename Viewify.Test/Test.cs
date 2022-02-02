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
                }
            };
            string cis = VarRecordUtils.Serialize(c);
            VarRecord ci = VarRecordUtils.Deserialize(cis)!;
            Trace.WriteLine(cis);
            Trace.WriteLine(VarRecordUtils.Serialize(ci));
        }
    }
}
