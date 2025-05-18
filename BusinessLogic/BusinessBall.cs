//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.BusinessLogic
// klasa pełna adapterów między Data a BusinessLogic
{
  public class Vector : IVector
  {
    public double x { get; init; }
    public double y { get; init; }

    public Vector(double XComponent, double YComponent)
    {
      x = XComponent;
      y = YComponent;
    }

    public static Vector operator +(Vector a, Vector b)
    {
      return new Vector(a.x + b.x, a.y + b.y);
    }
  }
  internal class Ball : IBall
  {
    private readonly Data.IBall dataBall;

    public Ball(Data.IBall ball, double radius)
    {
      dataBall = ball;
      dataBall.NewPositionNotification += RaisePositionChangeEvent;
      Radius = radius;
    }

    public double Radius { get; set; }

    public Vector Position
    {
      get => new Vector(dataBall.Position.x, dataBall.Position.y);
      set => dataBall.Position = new Vector(value.x, value.y);
    }

    public Vector Velocity
    {
      get => new Vector(dataBall.Velocity.x, dataBall.Velocity.y);
      set => dataBall.Velocity = new Vector(value.x, value.y);
    }

    public double Mass
    {
      get => dataBall.Mass;
      set => dataBall.Mass = value;
    }

    public event EventHandler<IPosition>? NewPositionNotification;

    private void RaisePositionChangeEvent(object? sender, Data.IVector e)
    {
      NewPositionNotification?.Invoke(this, new Position(e.x, e.y));
    }
  }

}