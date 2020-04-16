using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loader
{
    public static class ObjLoader
    {
        public static IObjHandle CreateHandle()
        {
            var handle = new ObjHandle();

            return handle;
        }
    }
}
