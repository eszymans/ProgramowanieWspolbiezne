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

            await layerBellow.Start(numberOfBalls, (start, radius, dataBall) =>
            {
                var pos = new Position(start.x, start.y);
                var logicBall = new Ball(dataBall, radius);
                logicBalls.Add(logicBall);
                upperLayerHandler(pos, radius, logicBall);
            });

            collisionThread = new Thread(CollisionLoop);
            collisionThread.Start();
        }

        public override void Dispose()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));

            stopCollisionThread = true;
            collisionThread?.Join();
            layerBellow.Dispose();
            Disposed = true;
        }

        #region Collision Logic

        private void CollisionLoop()
        {
            while (!stopCollisionThread)
            {
                var balls = logicBalls.ToArray(); // używamy tylko warstwy logiki

                for (int i = 0; i < balls.Length; i++)
                {
                    for (int j = i + 1; j < balls.Length; j++)
                    {
                        IBall b1 = balls[i];
                        IBall b2 = balls[j];
                        HandleCollision(b1, b2);
                    }
                }

                Thread.Sleep(15);
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

                if (dist >= minDist || dist == 0) return;

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
        private volatile bool stopCollisionThread = false;
        private Thread? collisionThread;
        private readonly UnderneathLayerAPI layerBellow;
        private List<IBall> logicBalls = new();

        #endregion
    }
}
