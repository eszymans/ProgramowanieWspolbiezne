using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace TP.ConcurrentProgramming.Data
{
    internal class DataImplementation : DataAbstractAPI
    {
        #region ctor

        public DataImplementation()
        {
            CollisionThread = new Thread(CollisionLoop);
            CollisionThread.Start();
        }

        #endregion ctor

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
                    Vector startingPosition = new(random.Next(100, 300), random.Next(100, 300));
                    Vector initialVelocity = new((random.NextDouble() - 0.5) * 150, (random.NextDouble() - 0.5) * 150);
                    double mass = random.NextDouble() * 10 + 10;
                    double radius = random.NextDouble() * 10 + 10;
                    Ball newBall = new(startingPosition, initialVelocity, radius, mass);
                    upperLayerHandler(startingPosition, radius, newBall);
                    BallsList.Add(newBall);

                    Thread ballThread = new Thread(() => BallThreadLoop(newBall));
                    BallThreads[newBall] = ballThread;
                    ballThread.Start();
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
                    stopThreads = true;

                    foreach (var thread in BallThreads.Values)
                    {
                        if (thread.IsAlive)
                            thread.Join();
                    }

                    if (CollisionThread.IsAlive)
                        CollisionThread.Join();

                    BallThreads.Clear();
                    BallsList.Clear();
                }

                Disposed = true;
            }
            else
            {
                throw new ObjectDisposedException(nameof(DataImplementation));
            }
        }

        public override void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable

        #region Private fields

        private bool Disposed = false;
        private volatile bool stopThreads = false;

        private ConcurrentBag<Ball> BallsList = new();
        private ConcurrentDictionary<Ball, Thread> BallThreads = new();
        private readonly object ballsLock = new();
        private readonly Thread CollisionThread;

        #endregion

        #region Movement

        private void BallThreadLoop(Ball ball)
        {
            DateTime lastUpdate = DateTime.Now;

            while (!stopThreads)
            {
                DateTime now = DateTime.Now;
                double deltaTime = (now - lastUpdate).TotalSeconds;
                lastUpdate = now;

                MoveBall(ball, deltaTime);

                Thread.Sleep(10); // ok. 60 FPS
            }
        }

        private void MoveBall(Ball ball, double deltaTime)
        {
            Vector scaledDelta = new Vector(
                ball.Velocity.x * deltaTime,
                ball.Velocity.y * deltaTime
            );

                ball.Move(scaledDelta);
        }

        #endregion

        #region Collision handling

        private void CollisionLoop()
        {
            while (!stopThreads)
            {
                HandleBallCollisions();
                Thread.Sleep(15);
            }
        }

        private void HandleBallCollisions()
        {
            Ball[] balls = BallsList.ToArray();

            for (int i = 0; i < balls.Length; i++)
            {
                Ball b1 = balls[i];
                for (int j = i + 1; j < balls.Length; j++)
                {
                    Ball b2 = balls[j];

                    // Aby uniknąć deadlocka — zawsze blokuj w tej samej kolejności
                    Ball first = b1.GetHashCode() < b2.GetHashCode() ? b1 : b2;
                    Ball second = b1.GetHashCode() < b2.GetHashCode() ? b2 : b1;

                    lock (first)
                        lock (second)
                        {
                            Vector pos1 = b1.Position;
                            Vector pos2 = b2.Position;

                            double dx = pos2.x - pos1.x;
                            double dy = pos2.y - pos1.y;
                            double distance = Math.Sqrt(dx * dx + dy * dy);
                            double minDistance = b1.Radius + b2.Radius;

                            if (distance < minDistance && distance > 0)
                            {
                                // Normalna kolizji
                                double nx = dx / distance;
                                double ny = dy / distance;

                                // Separacja pozycji
                                double overlap = minDistance - distance + 0.01;
                                double totalMass = b1.Mass + b2.Mass;

                                double correctionB1 = (b2.Mass / totalMass) * overlap;
                                double correctionB2 = (b1.Mass / totalMass) * overlap;

                                b1.Position = new Vector(pos1.x - nx * correctionB1, pos1.y - ny * correctionB1);
                                b2.Position = new Vector(pos2.x + nx * correctionB2, pos2.y + ny * correctionB2);

                                // Prędkości przed kolizją
                                IVector v1 = b1.Velocity;
                                IVector v2 = b2.Velocity;

                                // Składowe wzdłuż wektora normalnego
                                double v1n = v1.x * nx + v1.y * ny;
                                double v2n = v2.x * nx + v2.y * ny;

                                // Jeśli już się oddalają, pomiń
                                if (v1n - v2n <= 0)
                                    continue;

                                // Całkowicie sprężyste zderzenie: nowa składowa wzdłuż normalnej
                                double m1 = b1.Mass;
                                double m2 = b2.Mass;

                                double newV1n = (v1n * (m1 - m2) + 2 * m2 * v2n) / (m1 + m2);
                                double newV2n = (v2n * (m2 - m1) + 2 * m1 * v1n) / (m1 + m2);

                                // Składowe styczne pozostają bez zmian
                                double v1t_x = v1.x - v1n * nx;
                                double v1t_y = v1.y - v1n * ny;
                                double v2t_x = v2.x - v2n * nx;
                                double v2t_y = v2.y - v2n * ny;

                                // Nowe prędkości
                                b1.Velocity = new Vector(v1t_x + newV1n * nx, v1t_y + newV1n * ny);
                                b2.Velocity = new Vector(v2t_x + newV2n * nx, v2t_y + newV2n * ny);
                            }
                        }
                }
            }
        }

        #endregion

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

        #endregion
    }
}
