//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.Data
{
  internal class Ball : IBall
  {
        internal double Radius { get; } = 10.0;
        internal double Mass { get; } = 1.0;

    #region ctor

    internal Ball(Vector initialPosition, Vector initialVelocity)
    {
      Position = initialPosition;
      Velocity = initialVelocity;
    }

    #endregion ctor

    #region IBall

    public event EventHandler<IVector>? NewPositionNotification;

    public IVector Velocity { get; set; }

    #endregion IBall

    #region private

    protected internal Vector Position;

    private void RaiseNewPositionChangeNotification()
    {
      NewPositionNotification?.Invoke(this, Position);
    }

        internal void Move(Vector delta)
        {
            double newX = Position.x + delta.x;
            double newY = Position.y + delta.y;

            if (newX > 396 - Radius * 2 || newX < 0)
            {
                delta = new Vector(-delta.x, delta.y);
                Velocity = new Vector(-Velocity.x, Velocity.y);
            }

            if (newY > 416 - Radius * 2 || newY < 0)
            {
                delta = new Vector(delta.x, -delta.y);
                Velocity = new Vector(Velocity.x, -Velocity.y);
            }

            Position = new Vector(Position.x + delta.x, Position.y + delta.y);
            RaiseNewPositionChangeNotification();
        }

    #endregion private
}
}