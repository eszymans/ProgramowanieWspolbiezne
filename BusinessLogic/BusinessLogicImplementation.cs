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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using UnderneathLayerAPI = TP.ConcurrentProgramming.Data.DataAbstractAPI;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class BusinessLogicImplementation : BusinessLogicAbstractAPI
    {
        public BusinessLogicImplementation() : this(null) { }

        internal BusinessLogicImplementation(UnderneathLayerAPI? underneathLayer)
        {
            layerBellow = underneathLayer ?? UnderneathLayerAPI.GetDataLayer();
        }

        public override async Task Start(int numberOfBalls, Action<IPosition, double, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));

            logicBalls = new List<IBall>();
            ballThreads.Clear();

           await layerBellow.Start(numberOfBalls, (start, radius, dataBall) =>
            {
                var pos = new Position(start.x, start.y);
                var logicBall = new Ball(dataBall, radius);
                logicBalls.Add(logicBall);
                // programowanie współbiezne : tworzenie wątków dla każdej kulki//
                var thread = new Thread(() => BallThreadLoop(logicBall));
                thread.Start();
                ballThreads.Add(thread);
                //-------------------------------------------------------------//
                upperLayerHandler?.Invoke(pos, radius, logicBall);
            });
        }

        private void BallThreadLoop(Ball ball)
        {
            DateTime lastUpdate = DateTime.Now;
            while (!Disposed)
            {
                // Programowanie czasu rzeczywistego //
                DateTime now = DateTime.Now;
                double deltaTime = (now - lastUpdate).TotalSeconds;
                lastUpdate = now;

                Vector delta = new(ball.Velocity.x * deltaTime, ball.Velocity.y * deltaTime); // wykorzystanie czasu rzeczywistego do obliczenia położenia
                ball.Move(delta);

                // ----------------------------------//
                CheckCollisionsForBall(ball);
                Thread.Sleep(10);
            }

        }

        public override void Dispose()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            layerBellow.Dispose();
            Disposed = true;
        }

        #region Collision Logic

        private bool FindingCollisionTime(IBall ball, IBall ball2, double deltaTime)
        {
            Vector dp = new Vector(ball2.Position.x - ball.Position.x, ball2.Position.y - ball.Position.y);
            Vector dv = new Vector(ball2.Velocity.x - ball.Velocity.x, ball2.Velocity.y - ball.Velocity.y);
            double a = dv.x * dv.x + dv.y * dv.y;
            double b = 2 * (dp.x * dv.x + dp.y * dv.y);
            double c = dp.x * dp.x + dp.y * dp.y - (ball.Radius + ball2.Radius) * (ball.Radius + ball2.Radius);

            double discriminant = b * b - 4 * a * c;
            if (discriminant < 0)
            {
                // No collision
                return;
            }
            double sqrtDiscriminant = Math.Sqrt(discriminant);
            double t1 = (-b - sqrtDiscriminant) / (2 * a);
            double t2 = (-b + sqrtDiscriminant) / (2 * a);
            if (t1 < 0 && t2 < 0)
            {
                // Both collision times are in the past
                return;
            }
            double collisionTime = Math.Min(t1 >= 0 ? t1 : double.MaxValue, t2 >= 0 ? t2 : double.MaxValue);
            if (collisionTime < double.MaxValue)
            {
                collisionTime = t * deltaTime;
                return true;
            }
            return;

        }

        private void CheckCollisionsForBall(IBall ball)
        {
            lock (_collisionLock)
            {
                foreach (var otherBall in logicBalls)
                {
                    if (otherBall != ball)
                    {
                        var dx = otherBall.Position.x - ball.Position.x;
                        var dy = otherBall.Position.y - ball.Position.y;
                        var dist = Math.Sqrt(dx * dx + dy * dy);
                        var minDist = ball.Radius + otherBall.Radius;
                        if (dist < minDist || dist == 0)
                        {
                            HandleCollision(ball, otherBall);
                        }
                    }
                }
            }
        }

        private void HandleCollision(IBall b1, IBall b2)
        {
            lock (b1)
                lock (b2)
                {
                var dx = b2.Position.x - b1.Position.x;
                var dy = b2.Position.y - b1.Position.y;
                var dist = Math.Sqrt(dx * dx + dy * dy);
                var minDist = b1.Radius + b2.Radius;

                var nx = dx / dist;
                var ny = dy / dist;
                var overlap = minDist - dist + 0.01;
                var totalMass = b1.Mass + b2.Mass;

                b1.Position = new Vector(
                    b1.Position.x - nx * (b2.Mass / totalMass) * overlap,
                    b1.Position.y - ny * (b2.Mass / totalMass) * overlap
                );

                b2.Position = new Vector(
                    b2.Position.x + nx * (b1.Mass / totalMass) * overlap,
                    b2.Position.y + ny * (b1.Mass / totalMass) * overlap
                );

                var v1n = b1.Velocity.x * nx + b1.Velocity.y * ny;
                var v2n = b2.Velocity.x * nx + b2.Velocity.y * ny;

                if (v1n - v2n <= 0) return;

                double m1 = b1.Mass, m2 = b2.Mass;
                double newV1n = (v1n * (m1 - m2) + 2 * m2 * v2n) / (m1 + m2);
                double newV2n = (v2n * (m2 - m1) + 2 * m1 * v1n) / (m1 + m2);

                double v1t_x = b1.Velocity.x - v1n * nx;
                double v1t_y = b1.Velocity.y - v1n * ny;
                double v2t_x = b2.Velocity.x - v2n * nx;
                double v2t_y = b2.Velocity.y - v2n * ny;

                b1.Velocity = new Vector(v1t_x + newV1n * nx, v1t_y + newV1n * ny);
                b2.Velocity = new Vector(v2t_x + newV2n * nx, v2t_y + newV2n * ny);
            }
        }


        #endregion

        #region Private Fields

        private bool Disposed = false;
        private readonly UnderneathLayerAPI layerBellow;
        private List<IBall> logicBalls = new();
        private readonly object _collisionLock = new object();
        private readonly List<Thread> ballThreads = new();

        #endregion

        #region Test
#if DEBUG

        public void CheckObjectDisposed(Action<bool> callback) {
            callback?.Invoke(Disposed);
        }
#endif
        #endregion
    }
}
