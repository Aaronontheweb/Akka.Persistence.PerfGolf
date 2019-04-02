using Akka.Actor;

namespace Akka.Persistence.BenchmarkHarness
{
    /// <summary>
    ///     Persistent actor that is going to try to get its work done using the <see cref="FailingJournal" />
    /// </summary>
    public class PersistentBenchmarkActor : ReceivePersistentActor
    {
        private int _currentCount;

        public PersistentBenchmarkActor()
        {
            PersistenceId = Context.Self.Path.Name;

            Recover<int>(i =>
            {
                _currentCount += i;
            });

            Recover<SnapshotOffer>(o =>
            {
                if (o.Snapshot is int i)
                {
                    _currentCount = i;
                }
            });

            Command<int>(e =>
            {
                Persist(e, i =>
                {
                    _currentCount += i;
                });
            });

            Command<GetCount>(g => { Sender.Tell(_currentCount); });
        }

        public override string PersistenceId { get; }

        public class GetCount
        {
            public static readonly GetCount Instance = new GetCount();

            private GetCount()
            {
            }
        }
    }
}
