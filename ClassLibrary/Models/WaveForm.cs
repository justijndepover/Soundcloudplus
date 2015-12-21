using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Models
{
    public class WaveForm
    {
        public int width { get; set; }
        public int height { get; set; }
        public int[] samples { get; set; }
    }
}
