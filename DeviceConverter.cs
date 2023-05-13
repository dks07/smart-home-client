using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHomeClient
{
  public class DeviceConverter : JsonConverter
  {
    public override bool CanConvert(Type objectType)
    {
      return typeof(Device).IsAssignableFrom(objectType);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      JObject jsonObject = JObject.Load(reader);
      JToken typeToken = jsonObject.GetValue("Type", StringComparison.OrdinalIgnoreCase);
      string typeName = "SmartHomeClient." + typeToken.ToString() + "Device";
      Type type = Type.GetType(typeName);
      var abc = typeof(LightDevice).ToString();
      if (type == null)
      {
        throw new ArgumentException($"Invalid type name '{typeName}'.");
      }

      object target = Activator.CreateInstance(type);
      serializer.Populate(jsonObject.CreateReader(), target);
      return target;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      throw new NotImplementedException();
    }
  }

}
