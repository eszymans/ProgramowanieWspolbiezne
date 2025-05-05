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
using System.Diagnostics;

namespace TP.ConcurrentProgramming.Data
{
  internal class DataImplementation : DataAbstractAPI
  {
    #region ctor

    public DataImplementation()
    {
      MoveTimer = new Timer(Move, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(30));
    }

    #endregion ctor

    #region DataAbstractAPI

    public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(DataImplementation));
      if (upperLayerHandler == null)
        throw new ArgumentNullException(nameof(upperLayerHandler));
      Random random = new Random();
      for (int i = 0; i < numberOfBalls; i++)
      {
        Vector startingPosition = new(random.Next(100, 400 - 100), random.Next(100, 400 - 100));
        Vector initialVelocity = new((random.NextDouble() - 0.5) * 50, (random.NextDouble() - 0.5) * 50);
        Ball newBall = new(startingPosition, initialVelocity);
        upperLayerHandler(startingPosition, newBall);
        BallsList.Add(newBall);
      }
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
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }

    #endregion IDisposable

    #region private

    //private bool disposedValue;
        private bool Disposed = false;
        private DateTime lastUpdateTime = DateTime.Now;

        private readonly Timer MoveTimer;
        private Random RandomGenerator = new();
        private List<Ball> BallsList = [];
        private void Move(object? state)
        {
            DateTime now = DateTime.Now;
            double deltaTime = (now - lastUpdateTime).TotalSeconds;
            lastUpdateTime = now;

            foreach (Ball ball in BallsList)
            {
                Vector scaledDelta = new Vector(
                    ball.Velocity.x * deltaTime,
                    ball.Velocity.y * deltaTime
                );

                ball.Move(scaledDelta);
            }

            HandleBallCollisions();
        }

        private void HandleBallCollisions() 
        {
            for (int i = 0; i < BallsList.Count; i++) 
            {
                for (int j = i + 1; j < BallsList.Count; j++)
                {
                    Ball b1 = BallsList[i];
                    Ball b2 = BallsList[j];

                    Vector pos1 = b1.Position;
                    Vector pos2 = b2.Position;

                    double dx = pos2.x - pos1.x;
                    double dy = pos2.y - pos1.y;
                    double distance = Math.Sqrt(dx * dx + dy * dy);
                    double minDistance = 2 * b1.Radius;

                    if (distance < minDistance && distance > 0) 
                    {
                        double nx = dx / distance;
                        double ny = dy / distance;

                        double vx1 = b1.Velocity.x;
                        double vy1 = b1.Velocity.y;
                        double vx2 = b2.Velocity.x;
                        double vy2 = b2.Velocity.y;

                        double p = 2 * (vx1 * nx + vy1 * ny - vx2 * nx - vy2 * ny) /
                                   (b1.Mass + b2.Mass);

                        b1.Velocity = new Vector(
                            vx1 - p * b2.Mass * nx,
                            vy1 - p * b2.Mass * ny
                        );

                        b2.Velocity = new Vector(
                            vx2 + p * b1.Mass * nx,
                            vy2 + p * b1.Mass * ny
                        );

                        double overlap = minDistance - distance;
                        Vector correction = new Vector(nx * overlap, ny * overlap);


                        b1.Position = new Vector(b1.Position.x - correction.x, b1.Position.y - correction.y);
                        b2.Position = new Vector(b2.Position.x + correction.x, b2.Position.y + correction.y);
                    }

                }
            }
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