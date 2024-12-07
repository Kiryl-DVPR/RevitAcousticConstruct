using ClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcoustiCUtils.Library
{
    public class Response<T>
    {
        public int code = 0;
        //public string error = null;
        public List<T> data = null;
        
    }
    public class ResponseVersion
    {
        public int code = 0;
        //public string error = null;
        public dataVersion data = null;

    }


}
