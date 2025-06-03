//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TP.ConcurrentProgramming.Data
{
    internal class DataImplementation : DataAbstractAPI
    {
        #region ctor

        public DataImplementation() { }

        #endregion

        #region DataAbstractAPI

        public override async Task Start(int numberOfBalls, Action<IVector, double, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));

            await Task.Run(() =>
            {
                Random random = new Random();
                for (int i = 0; i < numberOfBalls; i++)
                {
                    Vector start = new(random.Next(100, 300), random.Next(100, 300));
                    Vector velocity = new((random.NextDouble() - 0.5) * 150, (random.NextDouble() - 0.5) * 150);
                    double mass = random.NextDouble() * 10 + 10;
                    double radius = random.NextDouble() * 10 + 10;

                    Ball ball = new(start, velocity, radius, mass);
                    BallsList.Add(ball);
                    upperLayerHandler(start, radius, ball);

                }
            });

            speedIncreaseTimer = new Timer(_ =>
            {
                lock (speedLock)
                {
                    foreach (var ball in BallsList)
                    {
                        ball.Velocity = new Vector(ball.Velocity.x * 1.01, ball.Velocity.y * 1.01);
                    }
                }
            }, null, 2000, 2000);
        }

        public override IEnumerable<IBall> GetBalls() => BallsList.ToArray();

        #endregion

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    stopThreads = true;
                    foreach (var thread in BallThreads.Values)
                        if (thread.IsAlive)
                            thread.Join();

                    BallThreads.Clear();
                    BallsList.Clear();
                    DiagnosticLogger.Stop();
                }
                Disposed = true;
            }
        }

        public override void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Fields

        private bool Disposed = false;
        private volatile bool stopThreads = false;
        private Timer? speedIncreaseTimer;
        private readonly object speedLock = new();
        private ConcurrentBag<Ball> BallsList = new();
        private ConcurrentDictionary<Ball, Thread> BallThreads = new();

        #endregion

        #region Test
#if DEBUG

        public void CheckBallsList(Action<IEnumerable<Ball>> callback) {
            callback?.Invoke(BallsList.ToArray());
        }

        public void CheckNumberOfBalls(Action<int> callback) {
            callback?.Invoke(BallsList.Count);
        }

        public void CheckObjectDisposed(Action<bool> callback) {
            callback?.Invoke(Disposed);
        }

#endif
        #endregion
    }
}


