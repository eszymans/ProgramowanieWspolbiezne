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
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace TP.ConcurrentProgramming.Data
{
    internal class DataImplementation : DataAbstractAPI
    {
        #region ctor

        public DataImplementation()
        {
            MoveTimer = new Timer(Move, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(15));
        }

        #endregion ctor

        #region DataAbstractAPI

        public override async Task Start(int numberOfBalls, Action<IVector, Double, IBall> upperLayerHandler)
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
                    Vector startingPosition = new(random.Next(100, 300), random.Next(100, 300));
                    Vector initialVelocity = new((random.NextDouble() - 0.5) * 75, (random.NextDouble() - 0.5) * 75);
                    double mass = random.NextDouble() * 10 + 10;
                    double radius = random.NextDouble() * 10 + 10;
                    Ball newBall = new(startingPosition, initialVelocity, radius, mass);
                    upperLayerHandler(startingPosition, radius, newBall);
                    BallsList.Add(newBall);
                }
            });
        }

        #endregion DataAbstractAPI

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    MoveTimer.Dispose();
                    BallsList.Clear();
                }
                Disposed = true;
            }
            else
                throw new ObjectDisposedException(nameof(DataImplementation));
        }

        public override void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable

        #region private

        private bool Disposed = false;
        private DateTime lastUpdateTime = DateTime.Now;

        private readonly Timer MoveTimer;
        private Random RandomGenerator = new();
        private ConcurrentBag<Ball> BallsList = new();
        private readonly object lockObject = new();

        private void MoveBall(Ball ball, double deltaTime)
        {
            Vector scaledDelta = new Vector(
                ball.Velocity.x * deltaTime,
                ball.Velocity.y * deltaTime
            );

            ball.Move(scaledDelta);
        }

        private void Move(object? state)
        {
            lock (lockObject)
            {
                DateTime now = DateTime.Now;
                double deltaTime = (now - lastUpdateTime).TotalSeconds;
                lastUpdateTime = now;

                var moveTasks = BallsList.Select(ball => Task.Run(() => MoveBall(ball, deltaTime))).ToArray();

                Task.WhenAll(moveTasks).Wait();

                HandleBallCollisions();
            }
        }

        private void HandleBallCollisions()
        {
            var balls = BallsList.ToArray();

            Parallel.For(0, balls.Length, i =>
            {
                Ball b1 = balls[i];
                for (int j = i + 1; j < balls.Length; j++)
                {
                    Ball b2 = balls[j];

                    Vector pos1 = b1.Position;
                    Vector pos2 = b2.Position;

                    double dx = pos2.x - pos1.x;
                    double dy = pos2.y - pos1.y;
                    double distance = Math.Sqrt(dx * dx + dy * dy);

                    double minDistance = b1.Radius + b2.Radius;

                    if (distance < minDistance && distance > 0)
                    {
                        double nx = dx / distance;
                        double ny = dy / distance;

                        IVector v1 = b1.Velocity;
                        IVector v2 = b2.Velocity;

                        double p = 2 * (v1.x * nx + v1.y * ny - v2.x * nx - v2.y * ny) / (b1.Mass + b2.Mass);

                        Vector newV1 = new Vector(
                            v1.x - p * b2.Mass * nx,
                            v1.y - p * b2.Mass * ny
                        );

                        Vector newV2 = new Vector(
                            v2.x + p * b1.Mass * nx,
                            v2.y + p * b1.Mass * ny
                        );

                        lock (b1) b1.Velocity = newV1;
                        lock (b2) b2.Velocity = newV2;

                        double overlap = minDistance - distance;
                        double totalMass = b1.Mass + b2.Mass;

                        double correctionB1Factor = b2.Mass / totalMass;
                        double correctionB2Factor = b1.Mass / totalMass;

                        Vector correction = new Vector(nx * overlap, ny * overlap);

                        lock (b1) b1.Position = new Vector(pos1.x - correction.x * correctionB1Factor, pos1.y - correction.y * correctionB1Factor);
                        lock (b2) b2.Position = new Vector(pos2.x + correction.x * correctionB2Factor, pos2.y + correction.y * correctionB2Factor);
                    }
                }
            });
        }

        #endregion private

        #region TestingInfrastructure

        [Conditional("DEBUG")]
        internal void CheckBallsList(Action<IEnumerable<IBall>> returnBallsList)
        {
            returnBallsList(BallsList);
        }

        [Conditional("DEBUG")]
        internal void CheckNumberOfBalls(Action<int> returnNumberOfBalls)
        {
            returnNumberOfBalls(BallsList.Count);
        }

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }

        #endregion TestingInfrastructure
    }
}
