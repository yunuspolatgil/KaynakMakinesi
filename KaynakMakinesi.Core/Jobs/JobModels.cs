using System;

namespace KaynakMakinesi.Core.Jobs
{
    public enum JobState { Pending, InProgress, Done, Failed }

    public sealed class JobItem
    {
        public long Id { get; set; }
        public string Type { get; set; }              // örn: "WriteRegister"
        public string PayloadJson { get; set; }       // parametreler
        public JobState State { get; set; }
        public int Step { get; set; }                 // state-machine için
        public int RetryCount { get; set; }
        public string LastError { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public interface IJobRepository
    {
        void EnsureSchema();

        void MarkStaleInProgressAsPending(TimeSpan olderThan);
        JobItem DequeueNextPending();
        void Update(JobItem job);
        void Enqueue(JobItem job);
    }
}