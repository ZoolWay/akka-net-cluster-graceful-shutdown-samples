using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClusterWebNode2
{
    public class Startup
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.FullName);
        private ActorSystem actorSystem;
        private readonly ManualResetEvent asTerminatedEvent = new ManualResetEvent(false);

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime applicationLifetime)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();

            applicationLifetime.ApplicationStarted.Register(HandleStarted);
            applicationLifetime.ApplicationStopped.Register(HandleStopped);
        }

        private void HandleStarted()
        {
            log.Info("ASP.NET application started");
            var config = Akka.Configuration.ConfigurationFactory.ParseString(@"
                akka {
                    actor {
                        provider = ""Akka.Cluster.ClusterActorRefProvider, Akka.Cluster""
                        serializers {
                            wire = ""Akka.Serialization.WireSerializer, Akka.Serialization.Wire""
                        }
                        serialization - bindings {
                            ""System.Object"" = wire
                        }
                    }
                    remote {
                        helios.tcp {
                            transport -class = ""Akka.Remote.Transport.Helios.HeliosTcpTransport, Akka.Remote""
						    applied-adapters = []
                            transport-protocol = tcp
                            hostname = ""127.0.0.1""
                            port = 0
					    }
                    }
                    loggers = [""Akka.Logger.log4net.Log4NetLogger,Akka.Logger.log4net""]
                    cluster {
					    seed-nodes = [""akka.tcp://sample@127.0.0.1:7001""]
                        roles = [webnode2]
				    }
			    }
            ");
            this.actorSystem = ActorSystem.Create("sample", config);
            log.Debug("ActorSystem created");
        }

        private void HandleStopped()
        {
            log.Info("ASP.NET application stopped");
            if (this.actorSystem != null)
            {
                log.Debug("Leaving cluster");
                var cluster = Akka.Cluster.Cluster.Get(this.actorSystem);
                cluster.RegisterOnMemberRemoved(MemberRemoved);
                cluster.Leave(cluster.SelfAddress);
                this.asTerminatedEvent.WaitOne();
            }
        }

        private async void MemberRemoved()
        {
            log.Info("Shutting down actor system");
            await this.actorSystem.Terminate();
            this.asTerminatedEvent.Set();
        }

    }
}
