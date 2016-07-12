using System;
using System.Threading;
using Akka.Actor;

namespace TopShelfNode3
{
    class Worker
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.FullName);
        private static readonly ManualResetEvent asTerminatedEvent = new ManualResetEvent(false);
        private ActorSystem actorSystem;

        public void Start()
        {
            this.actorSystem = ActorSystem.Create("sample");
        }

        public void Stop()
        {
            log.Info("Shutting down");
            var cluster = Akka.Cluster.Cluster.Get(actorSystem);
            cluster.RegisterOnMemberRemoved(() => MemberRemoved(actorSystem));
            cluster.Leave(cluster.SelfAddress);

            asTerminatedEvent.WaitOne();
            log.Info("Actor system terminated, exiting");
        }

        private async void MemberRemoved(ActorSystem actorSystem)
        {
            await actorSystem.Terminate();
            asTerminatedEvent.Set();
        }

    }
}
