using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;

namespace Akka.Persistence.BenchmarkHarness
{
    public class WriteBenchmarkHarness : IDisposable
    {
        public WriteBenchmarkHarness(Config config, int numPersistentActors, int writeCount)
        {
            NumPersistentActors = numPersistentActors;
            WriteCount = writeCount;
            Sys = ActorSystem.Create("Sys", config);
            TestActorProps = Props.Create(() => new PersistentBenchmarkActor());
        }

        public ActorSystem Sys { get; }

        public Props TestActorProps { get; }

        public int NumPersistentActors { get; }

        public int WriteCount { get; }

        public int TotalWriteOps => WriteCount * NumPersistentActors;

        public async Task<TimeSpan> Run()
        {
            var start = DateTime.UtcNow;
            var tasks = new List<Task>();
            for (var i = 0; i < NumPersistentActors; i++)
            {
                var actor = Sys.ActorOf(TestActorProps);
                for(var j = 0; j < WriteCount; j++)
                    actor.Tell(j);
                tasks.Add(actor.Ask<int>(PersistentBenchmarkActor.GetCount.Instance));
            }

            await Task.WhenAll(tasks);
            var stop = DateTime.UtcNow;
            return stop - start;
        }

        public void Dispose()
        {
            Sys?.Dispose();
        }
    }
}