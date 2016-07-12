using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace TopShelfNode3
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<Worker>(s =>
                {
                    s.ConstructUsing(name => new Worker());
                    s.WhenStarted(w => w.Start());
                    s.WhenStopped(w => w.Stop());
                });
                x.RunAsLocalSystem();
                x.SetDescription("Sample Akka.NET worker topshelf hosted");
                x.SetDisplayName("TopShelfNode3");
                x.SetServiceName("TopShelfNode3");
            });
        }
    }
}
