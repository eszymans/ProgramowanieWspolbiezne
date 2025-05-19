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
  public class DataAbstractAPIUnitTest
  {
    [TestMethod]
    public async Task ConstructorTestTestMethod()
    {
      DataAbstractAPI instance1 = DataAbstractAPI.GetDataLayer();
      DataAbstractAPI instance2 = DataAbstractAPI.GetDataLayer();
      Assert.AreSame<DataAbstractAPI>(instance1, instance2);
      instance1.Dispose();
      await Assert.ThrowsExceptionAsync<ObjectDisposedException>(async () => 
      {
          await instance2.Start(1, (pos, r, ball) => { });
      }
      );
    }
  }
}