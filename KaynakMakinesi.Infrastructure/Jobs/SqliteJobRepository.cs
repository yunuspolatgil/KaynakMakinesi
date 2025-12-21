using System;
using Dapper;
using KaynakMakinesi.Core.Jobs;
using KaynakMakinesi.Infrastructure.Db;

namespace KaynakMakinesi.Infrastructure.Jobs
{
    public sealed class SqliteJobRepository : IJobRepository
    {
        private readonly SqliteDb _db;

        public SqliteJobRepository(SqliteDb db)
        {
            _db = db;
        }

        public void EnsureSchema()
        {
            using (var con = _db.Open())
            {
                con.Execute(@"
CREATE TABLE IF NOT EXISTS JobQueue(
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  Type TEXT NOT NULL,
  PayloadJson TEXT,
  State TEXT NOT NULL,
  Step INTEGER NOT NULL,
  RetryCount INTEGER NOT NULL,
  LastError TEXT,
  CreatedAt TEXT NOT NULL,
  UpdatedAt TEXT NOT NULL
);
CREATE INDEX IF NOT EXISTS IX_JobQueue_State ON JobQueue(State, UpdatedAt);
");
            }
        }

        public void MarkStaleInProgressAsPending(TimeSpan olderThan)
        {
            var threshold = DateTime.Now.Subtract(olderThan).ToString("O");
            using (var con = _db.Open())
            {
                con.Execute(@"
UPDATE JobQueue
SET State='Pending', UpdatedAt=@now
WHERE State='InProgress' AND UpdatedAt < @threshold;",
                new { now = DateTime.Now.ToString("O"), threshold });
            }
        }

        public JobItem DequeueNextPending()
        {
            using (var con = _db.Open())
            using (var tx = con.BeginTransaction())
            {
                var job = con.QueryFirstOrDefault<JobItem>(@"
SELECT Id, Type, PayloadJson, 
       CASE State 
         WHEN 'Pending' THEN 0
         WHEN 'InProgress' THEN 1
         WHEN 'Done' THEN 2
         ELSE 3
       END as State,
       Step, RetryCount, LastError,
       CreatedAt, UpdatedAt
FROM JobQueue
WHERE State='Pending'
ORDER BY Id
LIMIT 1;", transaction: tx);

                if (job == null)
                {
                    tx.Commit();
                    return null;
                }

                con.Execute("UPDATE JobQueue SET State='InProgress', UpdatedAt=@now WHERE Id=@id;",
                    new { now = DateTime.Now.ToString("O"), id = job.Id }, tx);

                job.State = JobState.InProgress;
                tx.Commit();
                return job;
            }
        }

        public void Update(JobItem job)
        {
            using (var con = _db.Open())
            {
                con.Execute(@"
UPDATE JobQueue
SET State=@State, Step=@Step, RetryCount=@RetryCount, LastError=@LastError, UpdatedAt=@UpdatedAt
WHERE Id=@Id;",
                new
                {
                    Id = job.Id,
                    State = job.State.ToString(),
                    job.Step,
                    job.RetryCount,
                    job.LastError,
                    UpdatedAt = DateTime.Now.ToString("O")
                });
            }
        }

        public void Enqueue(JobItem job)
        {
            job.CreatedAt = DateTime.Now;
            job.UpdatedAt = DateTime.Now;

            using (var con = _db.Open())
            {
                con.Execute(@"
INSERT INTO JobQueue(Type, PayloadJson, State, Step, RetryCount, LastError, CreatedAt, UpdatedAt)
VALUES(@Type, @PayloadJson, @State, @Step, @RetryCount, @LastError, @CreatedAt, @UpdatedAt);",
                new
                {
                    job.Type,
                    job.PayloadJson,
                    State = job.State.ToString(),
                    job.Step,
                    job.RetryCount,
                    job.LastError,
                    CreatedAt = job.CreatedAt.ToString("O"),
                    UpdatedAt = job.UpdatedAt.ToString("O")
                });
            }
        }
    }
}
