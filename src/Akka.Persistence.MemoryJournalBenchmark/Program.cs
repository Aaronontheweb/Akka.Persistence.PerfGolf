using System;
using System.Threading.Tasks;
using Akka.Configuration;
using Akka.Persistence.BenchmarkHarness;
using Akka.Persistence.Journal;

namespace Akka.Persistence.MemoryJournalBenchmark
{
    class Program
    {
        public static readonly Config Config = @"akka.persistence.journal.plugin = ""akka.persistence.journal.shared""
                           akka.persistence.journal.shared.recovery-event-timeout = 2s
                           akka.persistence.journal.shared.class = """ +
                                               typeof(SharedMemoryJournal).AssemblyQualifiedName + "\"";

        public const int WriteCount = 10000;
        public const int RecoveryCount = 10000;

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
            else if (benchmarkKind == 2)
                await RecoveryBenchmark();
        }

        static async Task WriteBenchmark()
        {
            Console.WriteLine("Begin Write Benchmark");
            for (var i = 1; i <= 5; i++)
            {
                var harness = new WriteBenchmarkHarness(Config, i * 10, WriteCount);
                Console.WriteLine("Run {0}" + Environment.NewLine + "Actors: {1} \t Msgs:{2} \t TotalMsg: {3}", i, harness.NumPersistentActors, harness.WriteCount, harness.TotalWriteOps);
                var completed = await harness.Run();
                Console.WriteLine("Completed in {0} - {1} msg/s ({2} total)", completed, harness.ObservedWrites / completed.TotalSeconds, harness.ObservedWrites);
                harness.Dispose();
            }
            Console.WriteLine("End Write Benchmark");
        }

        static async Task RecoveryBenchmark()
        {
            Console.WriteLine("Begin Recovery Benchmark");
            for (var i = 1; i <= 5; i++)
            {
                var harness = new RecoveryBenchmarkHarness(Config, i * 10, RecoveryCount);
                Console.WriteLine("Warming up recovery...");
                await harness.Warmup();
                Console.WriteLine("Run {0}" + Environment.NewLine + "Actors: {1} \t Msgs:{2} \t TotalMsg: {3}", i, harness.NumPersistentActors, harness.WriteCount, harness.TotalWriteOps);
                var completed = await harness.Run();
                Console.WriteLine("Completed in {0} - {1} msg/s ({2} total)", completed, harness.ObservedWrites / completed.TotalSeconds, harness.ObservedWrites);
                harness.Dispose();
            }
            Console.WriteLine("End Recovery Benchmark");
        }
    }
}
