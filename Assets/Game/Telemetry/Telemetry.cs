using System.Collections.Generic;
namespace Game.Telemetry {
  public interface ITelemetry {
    void Track(string evt, Dictionary<string,object> data=null);
  }
  public sealed class NullTelemetry : ITelemetry {
    public void Track(string evt, Dictionary<string,object> data=null){}
  }
}
