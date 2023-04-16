using System;

namespace MQTTnet.Client.Extensions.AzureIoT
{
    public enum ConnectionStatus
    {
        
        Disconnected,
        Connected,
        DisconnectedRetrying,
        Closed
    }
    public enum ConnectionStatusChangeReason
    {
        ClientClosed,
        ConnectionOk,
        CommunicationError,
        RetryExpired,
        BadCredential,
        DeviceDisabled
    }

    public static class EnumExtensions
    {
        public static ConnectionStatus ToConnectionStatus(this MqttClientConnectResultCode reason)
        {
            ConnectionStatus result;
            switch (reason)
            {
                case MqttClientConnectResultCode.Success:
                    result = ConnectionStatus.Connected;
                    break;
                default:
                    result = ConnectionStatus.Disconnected;
                    break;
            };
            return result;
        }
        public static ConnectionStatusChangeReason ToConnectionStatusChangeReason(this MqttClientConnectResultCode reason)
        {
            ConnectionStatusChangeReason result;
            switch (reason)
            {
                case MqttClientConnectResultCode.NotAuthorized:
                case MqttClientConnectResultCode.BadAuthenticationMethod:
                    result = ConnectionStatusChangeReason.BadCredential;
                    break;
                case MqttClientConnectResultCode.ServerUnavailable:
                    result = ConnectionStatusChangeReason.DeviceDisabled;
                    break;
                case MqttClientConnectResultCode.Success:
                    result = ConnectionStatusChangeReason.ConnectionOk;
                    break;
                default:
                    result = ConnectionStatusChangeReason.CommunicationError;
                    break;
            };
            return result;
        }
    }


    public class ConnectionStatusInfo
    {
        public ConnectionStatus Status { get; set;  }
        public ConnectionStatusChangeReason ChangeReason { get; set; }
        public DateTimeOffset StatusLastChangedOnUtc { get; set; }
    }
}
