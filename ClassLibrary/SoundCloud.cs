using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using ClassLibrary.API;

namespace ClassLibrary
{
    public class SoundCloud
    {
        public static string ClientId { get; } = "bb45a30915dd5b2e04cf203b0f257c09";
        public ApiProxy ApiProxy { get; set; }

        public SoundCloud()
        {
            ApiProxy = new ApiProxy();
        }
    }
}
