using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SmartHomeClient;

public abstract class Device
{
  public string DeviceId { get; set; }
  public string Name { get; set; }
  [JsonConverter(typeof(StringEnumConverter))]
  public DeviceType Type { get; set; }
  public string Location { get; set; }
  public string Description { get; set; }
  [JsonConverter(typeof(StringEnumConverter))]
  public DeviceStatus Status { get; set; }
  public bool IsOn { get; set; }
  public int Wattage { get; set; }
}