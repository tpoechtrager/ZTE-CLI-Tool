namespace ZTE_Cli_Tool.TrafficThroughput;

public class ThroughputCalculator
{
  private Int64 _lastValue = 0;
  private DateTime _lastUpdate = DateTime.MinValue;

  public double Throughput = 0;
  public Int64 Updates = 0;

  public void Update(Int64 now)
  {
    if (Updates > 0) {
      TimeSpan timeDifference = DateTime.Now - _lastUpdate;
      Throughput = (now - _lastValue) / (timeDifference.TotalMilliseconds / 1000.0);
    }

    _lastValue = now;
    _lastUpdate = DateTime.Now;

    Updates++;
  }

  public void Update(string nowStr)
  {
    Update(Tools.ParseInt64(nowStr, -1));
  }
}
