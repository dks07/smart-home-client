using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using Newtonsoft.Json;
using System.IO;
using System.IO.Pipes;

namespace SmartHomeClient
{
  class Program
  {
    static async Task Main(string[] args)
    {

      ServiceCollection serviceCollection = new ServiceCollection();

      // Build configuration
      IConfigurationRoot configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
        .AddJsonFile("appsettings.json", false).AddEnvironmentVariables()
        .Build();

      // Add access to generic IConfigurationRoot
      serviceCollection.AddSingleton<IConfiguration>(configuration);
      var mqttHost = configuration["Settings:MqttHost"];
      var mqttPort = int.Parse(configuration["Settings:MqttPort"]);
      var devicesFilePath = configuration["Settings:DevicePath"];
      var mqttClient = new MqttFactory().CreateMqttClient();

      var options = new MqttClientOptionsBuilder()
        .WithTcpServer(mqttHost, mqttPort)
        .Build();

      await mqttClient.ConnectAsync(options);
      //string currentDirectory = Directory.GetCurrentDirectory();
      //string devicesFilePath = Path.Combine(currentDirectory, "devices.json");

      // Create a new instance of the file system watcher
      //FileSystemWatcher watcher = new FileSystemWatcher();

      //// Set the path to watch for changes
      //watcher.Path = Path.GetDirectoryName(devicesFilePath);

      //// Set the file name to watch for changes
      //watcher.Filter = Path.GetFileName(devicesFilePath);
      //Console.WriteLine($"File Path {watcher.Path}");
      //Console.WriteLine($"File Filter {watcher.Filter}");
      //watcher.Changed += async (sender, e) =>
      //{
      //  await OnChangedAsync(e.FullPath, mqttClient);
      //};
      //watcher.Created += async (sender, e) =>
      //{
      //  await OnChangedAsync(e.FullPath, mqttClient);
      //};


      // Enable the file system watcher
      //watcher.EnableRaisingEvents = true;
      var lastWriteTime = File.GetLastWriteTimeUtc(devicesFilePath);

      await OnChangedAsync(devicesFilePath, mqttClient);
      while (true)
      {
        var newWriteTime = File.GetLastWriteTimeUtc(devicesFilePath);
        if (newWriteTime != lastWriteTime)
        {
          Console.WriteLine("File has been modified!");
          await OnChangedAsync(devicesFilePath, mqttClient);
          lastWriteTime = newWriteTime;
        }
        
        Thread.Sleep(TimeSpan.FromSeconds(3));
      }
      Console.ReadLine();
    }

    private static bool enableRaisingEvents = true;
    // This method is called when the devices JSON file is changed
    private static async Task OnChangedAsync(string path, IMqttClient mqttClient)
    {
      try
      {
        if (enableRaisingEvents)
        {
          enableRaisingEvents = false;
          // Read the devices from the JSON file
          var devices = await ReadDevicesFromFile(path);
          if (devices != null)
            foreach (var device in devices)
            {
              var message = new MqttApplicationMessageBuilder()
                .WithTopic($"smart-home/devices/{device.DeviceId}")
                .WithPayload(JsonConvert.SerializeObject(device))
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                .WithRetainFlag()
                .Build();

              await mqttClient.PublishAsync(message);
            }
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error processing file: {ex.Message}");
      }
      finally
      {
        enableRaisingEvents = true;
      }
    }

    // This method reads the devices data from the JSON file
    private static async Task<List<Device>?> ReadDevicesFromFile(string path)
    {
      if (File.Exists(path))
      {
        using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
          if (fs.CanRead)
          {
            using (StreamReader sr = new StreamReader(fs))
            {
              string json = await sr.ReadToEndAsync();
              // Deserialize JSON and process data here

              List<Device>? devices = JsonConvert.DeserializeObject<List<Device>>(json, new JsonSerializerSettings
              {
                TypeNameHandling = TypeNameHandling.Auto,
                Converters = { new DeviceConverter() }
              });
              return devices;
            }
          }
          else
          {
            Console.WriteLine($"File is not readable at path {path}");
          }
        }
      }
      else
      {
        Console.WriteLine($"File does not exist at path {path}");
      }

      return null;
    }
  }
}
