using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Services
{
    public class Service
    {
        
    }
    public class Service : IService
    {
        public string Print()
        {
            return "OK";
        }
    }
}
