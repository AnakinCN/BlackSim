using BlackSim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Tests;

/// <summary>
/// tests on the simulation core
/// </summary>
public class CoreTests : INumberStory
{
    /// <summary>
    /// the Number variable for testing simulation core
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// Future Event List tests
    /// </summary>
    [Fact]
    public void FELTest()
    {
        Number = 0;
        Simulation sim = new Simulation(accelerationRate: 16);
        sim.AddFutureEvent(TimeSpan.FromSeconds(1), new PlusEvent(this));
        Assert.Single(sim.FEL);
        sim.Run();
        Assert.Equal(1, Number);
        Assert.Empty(sim.FEL);
        Assert.InRange(sim.SimulationTime, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1) + TimeSpan.FromMilliseconds(20));      //if CPU too busy, this may fail

        sim.AddFutureEvent(TimeSpan.FromSeconds(2), new PlusEvent(this));
        Assert.Single(sim.FEL);
        sim.Run();
        Assert.Equal(2, Number);
        Assert.Empty(sim.FEL);
        Assert.InRange(sim.SimulationTime, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2) + TimeSpan.FromMilliseconds(20));   //if CPU too busy, this may fail

        sim.AddFutureEvent(TimeSpan.FromSeconds(3), new MinusEvent(this));
        Assert.Single(sim.FEL);
        sim.Run();
        Assert.Equal(1, Number);
        Assert.Empty(sim.FEL);
        Assert.InRange(sim.SimulationTime, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3) + TimeSpan.FromMilliseconds(20));   //if CPU too busy, this may fail
    }

    /// <summary>
    /// to test if two IEvent can be arranged at the same time in the FEL, which is not allowed
    /// </summary>
    [Fact]
    public void SameSimulationTimeTest()
    {
        Number = 0;
        Simulation sim = new Simulation(accelerationRate: 16);

        //add two IEvent with the simulation time
        TimeSpan time1 = sim.AddFutureEvent(TimeSpan.FromSeconds(1), new PlusEvent(this));
        TimeSpan time2 = sim.AddFutureEvent(TimeSpan.FromSeconds(1), new MinusEvent(this));
        sim.Run();
        Assert.Equal(TimeSpan.FromSeconds(1), time1);
        Assert.NotEqual(TimeSpan.FromSeconds(1), time2);    //the second IEvent shall be slightly postponed
        Assert.Equal(0, Number);
        Assert.Empty(sim.FEL);
    }
}
