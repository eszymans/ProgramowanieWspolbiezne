private class BusinessLogicIBallFixture : BusinessLogic.IBall
{
    public double Radius { get; set; }
    double BusinessLogic.IBall.Mass { get; set; } // Explicit interface implementation
    public event EventHandler<IPosition>? NewPositionNotification;

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
