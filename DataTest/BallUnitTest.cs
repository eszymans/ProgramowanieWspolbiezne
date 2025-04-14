//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.Data.Test
{
  [TestClass]
  public class BallUnitTest
  {
    [TestMethod]
    public void ConstructorTestMethod()
    {
      Vector testinVector = new Vector(0.0, 0.0);
      Ball newInstance = new(testinVector, testinVector);
    }

    [TestMethod]
    public void MoveTestMethod()
    {
      Vector initialPosition = new(10.0, 10.0);
      Ball newInstance = new(initialPosition, new Vector(0.0, 0.0));
      IVector curentPosition = new Vector(0.0, 0.0);
      int numberOfCallBackCalled = 0;
      newInstance.NewPositionNotification += (sender, position) => { Assert.IsNotNull(sender); curentPosition = position; numberOfCallBackCalled++; };
      newInstance.Move(new Vector(0.0, 0.0));
      Assert.AreEqual<int>(1, numberOfCallBackCalled);
      Assert.AreEqual<IVector>(initialPosition, curentPosition);
    }

        [TestMethod]
        public void MoveTestMethodWithDelta_HandlesEdgeBounce()
        {
            Vector initialPosition = new Vector(374, 392);
            Vector initialVelocity = new Vector(5.0, 5.0);
            Ball ball = new Ball(initialPosition, initialVelocity);

            IVector curentPosition = new Vector(0.0, 0.0);
            int numberOfCallBackCalled = 0;

            ball.NewPositionNotification += (sender, position) =>
            {
                Assert.IsNotNull(sender);
                curentPosition = position;
                numberOfCallBackCalled++;
            };
            ball.Move(initialVelocity);

            Assert.AreEqual(1, numberOfCallBackCalled);
            Assert.AreEqual(369, curentPosition.x);
            Assert.AreEqual(387, curentPosition.y);

            ball.Move(new Vector(-6.0, -6.0));

            Assert.AreEqual(363, curentPosition.x);
            Assert.AreEqual(381, curentPosition.y);
        }
    }
}