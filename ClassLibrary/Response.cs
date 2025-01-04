using System.Collections.Generic;

namespace ClassLibrary
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
        public DataVersion data = null;

    }


}
