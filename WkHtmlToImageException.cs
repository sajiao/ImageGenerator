using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ImageGenerator
{
    public class WkHtmlToImageException : Exception
    {
        [CompilerGenerated]
        public int ErrorCode { get; set; }

        public WkHtmlToImageException(int errCode, string message) : base(message)
        {
            this.ErrorCode = errCode;
        }

       
    }
}
