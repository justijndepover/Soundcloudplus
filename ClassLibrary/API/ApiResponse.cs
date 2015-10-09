﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ClassLibrary.API
{
    public class ApiResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public dynamic Data { get; set; }

    }
}
