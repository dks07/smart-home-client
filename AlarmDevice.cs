namespace SmartHomeClient;

public class AlarmDevice : Device
{
  public bool IsTriggered { get; set; }
  public float SmokeLevel { get; set; }
}