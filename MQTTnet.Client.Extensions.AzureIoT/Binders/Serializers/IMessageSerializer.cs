namespace MQTTnet.Client.Extensions.AzureIoT.Binders.Serializer
{
    public interface IMessageSerializer
    {
        string ContentType { get; }
        string ContentEncoding { get; }
        byte[] ToBytes<T>(T payload);
        T FromBytes<T>(byte[] payload);
    }

}