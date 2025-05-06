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
        #region ctor

        internal Ball(Vector initialPosition, Vector initialVelocity, double radius, double mass)
        {
            Position = initialPosition;
            Velocity = initialVelocity;
            Radius = radius;
            Mass = mass;
        }

        #endregion ctor

        #region IBall

        public double Radius { get; set; }
        public double Mass { get; set; }
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

            if (newX > 392 - Radius * 2 || newX < 0)
            {
                delta = new Vector(-delta.x, delta.y);
                Velocity = new Vector(-Velocity.x, Velocity.y);
            }

            if (newY > 412 - Radius * 2 || newY < 0)
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
