using System;
using System.Threading.Tasks;
using Akka.Configuration;
using Akka.Persistence.BenchmarkHarness;

namespace Akka.Persistence.MemoryJournalBenchmark
{
    class Program
    {
        private static Config Config = @"
            
        ";

        public const int WriteCount = 1000;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Type 1 for write benchmark, 2 for recovery benchmark");
            var choice = Console.ReadLine();
            int benchmarkKind = 1;
            switch (choice)
            {
                case "2":
                    benchmarkKind = 2;
                    break;
            }

            if (benchmarkKind == 1)
                await WriteBenchmark();
        }

        static async Task WriteBenchmark()
        {
            for (var i = 1; i <= 5; i++)
            {
                var harness = new WriteBenchmarkHarness(Config, i * 10, WriteCount);
                Console.WriteLine("Run {0}" + Environment.NewLine + "Actors: {1} \t Msgs:{2} \t TotalMsg: {3}", i, harness.NumPersistentActors, harness.WriteCount, harness.TotalWriteOps);
                var completed = await harness.Run();
                Console.WriteLine("Completed in {0} - {1} msg/s", completed, harness.TotalWriteOps/completed.TotalSeconds);
                harness.Dispose();
            }
        }
    }
}
