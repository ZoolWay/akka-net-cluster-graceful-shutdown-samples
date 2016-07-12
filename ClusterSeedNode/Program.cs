using System;
using System.Threading;
using Akka.Actor;

namespace ClusterSeedNode
{
    class Program
    {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.FullName);
        private static readonly ManualResetEvent quitEvent = new ManualResetEvent(false);
        private static readonly ManualResetEvent asTerminatedEvent = new ManualResetEvent(false);

        /// <summary>
        /// SEED NODE PROGRAM
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.CancelKeyPress += (sender, e) =>
            {
                quitEvent.Set();
                e.Cancel = true;
            };

            log.Debug("Creating actor system");
            ActorSystem actorSystem = ActorSystem.Create("sample");

            quitEvent.WaitOne();

            log.Info("Shutting down");
            var cluster = Akka.Cluster.Cluster.Get(actorSystem);
            cluster.RegisterOnMemberRemoved(() => MemberRemoved(actorSystem));
            cluster.Leave(cluster.SelfAddress);

            asTerminatedEvent.WaitOne();
            log.Info("Actor system terminated, exiting");
            if (System.Diagnostics.Debugger.IsAttached) Console.ReadLine();
        }

        private static async void MemberRemoved(ActorSystem actorSystem)
        {
            await actorSystem.Terminate();
            asTerminatedEvent.Set();
        }
    }
}
