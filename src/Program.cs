using System;
using System.Collections.Generic;
using System.Text;

namespace DmpasComSingleton
{
    class Program
    {
        static void Main(string[] args)
        {
            SingleComServer.Instance.Run();
        }
    }
}
