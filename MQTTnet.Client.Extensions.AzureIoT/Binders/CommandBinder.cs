using MQTTnet.Client.Extensions.AzureIoT.Binders.Serializer;
using System;
using System.Threading.Tasks;

namespace MQTTnet.Client.Extensions.AzureIoT.Binders
{
    public class CommandBinder
    {
        private readonly IMqttClient connection;
        public Func<CommandRequest, Task<CommandResponse>> OnCmdDelegate { get; set; }

        public CommandBinder(IMqttClient c)
        {
            IMessageSerializer serializer = new Utf8JsonSerializer();
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
                        CommandResponse response = await OnCmdDelegate.Invoke(req);
                        var tp = TopicParser.ParseTopic(topic);
                        await connection.PublishBinaryAsync($"$iothub/methods/res/{response.Status}/?$rid={tp.Rid}", serializer.ToBytes(response.ReponsePayload));
                    }
                }
            };
        }
    }
}
