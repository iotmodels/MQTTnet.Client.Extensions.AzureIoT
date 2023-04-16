namespace MQTTnet.Client.Extensions.AzureIoT
{
    public interface IMessageSerializer
    {
        byte[] ToBytes<T>(T payload);
        bool TryReadFromBytes<T>(byte[] payload, out T result);
    }

}