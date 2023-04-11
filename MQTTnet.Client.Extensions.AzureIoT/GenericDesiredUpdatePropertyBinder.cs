using MQTTnet.Client;
using System;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace MQTTnet.Client.Extensions.AzureIoT
{
    public class GenericDesiredUpdatePropertyBinder
    {
        private readonly IMqttClient connection;
        public Func<JsonNode, GenericPropertyAck> OnProperty_Updated = null;
        public GenericDesiredUpdatePropertyBinder(IMqttClient c, UpdateTwinBinder<object> updTwinBinder)
        {
            connection = c;
            _ = connection.SubscribeAsync("$iothub/twin/PATCH/properties/desired/#");
            connection.ApplicationMessageReceivedAsync += async m =>
             {
                 var topic = m.ApplicationMessage.Topic;
                 if (topic.StartsWith("$iothub/twin/PATCH/properties/desired"))
                 {
                     string msg = Encoding.UTF8.GetString(m.ApplicationMessage.Payload);
                     JsonNode desired = JsonNode.Parse(msg);

                     if (desired != null)
                     {
                         if (OnProperty_Updated != null)
                         {
                             var ack = OnProperty_Updated(desired);
                             if (ack != null)
                             {
                                 _ = updTwinBinder.InvokeAsync(connection.Options.ClientId, ack.BuildAck());
                             }
                         }
                     }
                 }
                 await Task.Yield();
             };
        }
    }
}
