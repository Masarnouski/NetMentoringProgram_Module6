using MyIoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestContainer
{
    class Program
    {
        static void Main(string[] args)
        {
            var obj = GetService<CustomerBLL>();
        }


        static T GetService<T>() {

            Container container = new Container(Assembly.LoadFrom("MyIoC.dll"));

            return container.CreateInstance<T>();
        }
    }
}