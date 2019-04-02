using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;

namespace Akka.Persistence.BenchmarkHarness
{
    public class RecoveryBenchmarkHarness : IDisposable
    {
        public RecoveryBenchmarkHarness(Config config, int numPersistentActors, int writeCount)
        {
            NumPersistentActors = numPersistentActors;
            WriteCount = writeCount;
            Sys = ActorSystem.Create("Sys", config);
            TestActorProps = Props.Create(() => new PersistentBenchmarkActor());
            Actors = new List<IActorRef>();
        }

        public ActorSystem Sys { get; }

        public Props TestActorProps { get; }

        public int NumPersistentActors { get; }

        public int WriteCount { get; }

        public int TotalWriteOps => WriteCount * NumPersistentActors;

        public int ObservedWrites { get; private set; }

        public List<IActorRef> Actors { get; }

        public async Task<TimeSpan> Warmup()
        {
            var start = DateTime.UtcNow;
            var tasks = new List<Task>();
            for (var i = 0; i < NumPersistentActors; i++)
            {
                var actor = Sys.ActorOf(TestActorProps, i.ToString());
                Actors.Add(actor);
                for (var j = 0; j < WriteCount; j++)
                    actor.Tell(j);
                tasks.Add(actor.Ask<int>(PersistentBenchmarkActor.GetCount.Instance));
            }

            await Task.WhenAll(tasks);
            var stop = DateTime.UtcNow;
            await Task.WhenAll(Actors.Select(x => x.GracefulStop(TimeSpan.FromSeconds(10))));
            return stop - start;
        }

        public async Task<TimeSpan> Run()
        {
            var start = DateTime.UtcNow;
            var tasks = new List<Task<int>>();
            foreach(var a in Actors)
            {
                var actor = Sys.ActorOf(TestActorProps, a.Path.Name);
                tasks.Add(actor.Ask<int>(PersistentBenchmarkActor.GetCount.Instance));
            }

            await Task.WhenAll(tasks);
            var stop = DateTime.UtcNow;
            ObservedWrites = tasks.Sum(x => x.Result);
            return stop - start;
        }

        public void Dispose()
        {
            Sys?.Dispose();
        }
    }
}