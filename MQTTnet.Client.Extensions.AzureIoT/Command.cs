using MQTTnet.Client;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace MQTTnet.Client.Extensions.AzureIoT
{
    public class Command
    {
        private readonly IMqttClient connection;
        public Func<CommandRequest, Task<CommandResponse>> OnCmdDelegate { get; set; }

        public Command(IMqttClient c)
        {
            connection = c;
            _ = connection.SubscribeAsync("$iothub/methods/POST/#");
            connection.ApplicationMessageReceivedAsync += async m =>
            {
                var topic = m.ApplicationMessage.Topic;
                if (topic.StartsWith($"$iothub/methods/POST/"))
                {
                    var segments = topic.Split('/');
                    var cmdName = segments[3];
                    string msg = m.ApplicationMessage.ConvertPayloadToString();
                    CommandRequest req = new CommandRequest()
                    {
                        CommandName = cmdName,
                        CommandPayload = msg
                    };
                    if (OnCmdDelegate != null && req != null)
                    {
                        var tp = TopicParser.ParseTopic(topic);
                        CommandResponse response = await OnCmdDelegate.Invoke(req);
                        _ = connection.PublishStringAsync($"$iothub/methods/res/{response.Status}/?$rid={tp.Rid}", JsonSerializer.Serialize(response.ReponsePayload));
                    }
                }
                await Task.Yield();
            };
        }
    }
}
