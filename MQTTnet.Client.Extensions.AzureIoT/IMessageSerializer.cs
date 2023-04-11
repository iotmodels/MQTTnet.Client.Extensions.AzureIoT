﻿namespace MQTTnet.Client.Extensions.AzureIoT
{
    public interface IMessageSerializer
    {
        byte[] ToBytes<T>(T payload, string name = "");
        bool TryReadFromBytes<T>(byte[] payload, string name, out T result);
    }

}