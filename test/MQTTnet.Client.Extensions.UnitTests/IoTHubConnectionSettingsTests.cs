using MQTTnet.Client.Extensions.AzureIoT.Auth;
using System;
using Xunit;

namespace MQTTnet.Client.Extensions.UnitTests
{
    public class IoTHubConnectionSettingsTests
    {
        [Fact]
        public void DefaultValues()
        {
            var dcs = new IoTHubConnectionSettings();
            Assert.Equal(60, dcs.SasMinutes);
            Assert.Equal(60, dcs.KeepAliveInSeconds);
            Assert.Equal(8883, dcs.TcpPort);
            Assert.False(dcs.DisableCrl);
            Assert.True(dcs.UseTls);
            Assert.Equal("TcpPort=8883;MqttVersion=5", dcs.ToString());
        }


        //[Fact]
        //public void InferClientIdFromUserName()
        //{
        //    Assert.Equal("user", new ConnectionSettings { UserName = "user" }.ClientId);
        //    Assert.Equal("client", new ConnectionSettings { UserName = "user", ClientId = "client" }.ClientId);
        //}

        //[Fact]
        //public void InferClientIdFromCert()
        //{
        //    Assert.Equal("onething", new ConnectionSettings { X509Key = "onething.pem|onething.key" }.ClientId);
        //    Assert.Equal("client", new ConnectionSettings { X509Key = "onething.pem|onething.key",  ClientId = "client" }.ClientId);
        //}


        [Fact]
        public void ParseConnectionString()
        {
            string cs = "HostName=<hubname>.azure-devices.net;DeviceId=<deviceId>;SharedAccessKey=<SasKey>";
            IoTHubConnectionSettings dcs = IoTHubConnectionSettings.FromConnectionString(cs);
            Assert.Equal("<hubname>.azure-devices.net", dcs.HostName);
            Assert.Equal("<deviceId>", dcs.DeviceId);
            Assert.Equal("<SasKey>", dcs.SharedAccessKey);
            Assert.Empty(dcs.ClientId!);
        }

        [Fact]
        public void InvalidValuesDontUseDefaults()
        {
            string cs = "HostName=<hubname>.azure-devices.net;DeviceId=<deviceId>;SharedAccessKey=<SasKey>;MaxRetries=-2;SasMinutes=aa;RetryInterval=4.3";
            IoTHubConnectionSettings dcs = new(cs);
            Assert.Equal("<hubname>.azure-devices.net", dcs.HostName);
            Assert.Equal("<deviceId>", dcs.DeviceId);
            Assert.Equal("<SasKey>", dcs.SharedAccessKey);
            Assert.Equal(60, dcs.SasMinutes);
        }


        [Fact]
        public void ParseConnectionStringWithDefaultValues()
        {
            string cs = "HostName=<hubname>.azure-devices.net;DeviceId=<deviceId>;ModuleId=<moduleId>;SharedAccessKey=<SasKey>";
            IoTHubConnectionSettings dcs = IoTHubConnectionSettings.FromConnectionString(cs);
            Assert.Equal("<hubname>.azure-devices.net", dcs.HostName);
            Assert.Equal("<deviceId>", dcs.DeviceId);
            Assert.Equal("<moduleId>", dcs.ModuleId);
            Assert.Equal("<SasKey>", dcs.SharedAccessKey);
            Assert.Equal(60, dcs.SasMinutes);
            Assert.Equal(60, dcs.KeepAliveInSeconds);
            Assert.Equal(8883, dcs.TcpPort);
            Assert.Empty(dcs.ClientId!);
            Assert.True(dcs.UseTls);
            Assert.False(dcs.DisableCrl);
        }

        [Fact]
        public void ParseConnectionStringWithAllValues()
        {
            string cs = "HostName=<hubname>.azure-devices.net;DeviceId=<deviceId>;ClientId=<ClientId>;ModuleId=<moduleId>;SharedAccessKey=<SasKey>;SasMinutes=2;TcpPort=1234;UseTls=false;CaFile=<path>;DisableCrl=true;UserName=<usr>;Password=<pwd>;MqttVersion=3";
            IoTHubConnectionSettings dcs = IoTHubConnectionSettings.FromConnectionString(cs);
            Assert.Equal("<hubname>.azure-devices.net", dcs.HostName);
            Assert.Equal("<deviceId>", dcs.DeviceId);
            Assert.Equal("<moduleId>", dcs.ModuleId);
            Assert.Equal("<SasKey>", dcs.SharedAccessKey);
            Assert.Equal("<ClientId>", dcs.ClientId);
            Assert.Equal("<usr>", dcs.UserName);
            Assert.Equal("<pwd>", dcs.Password);
            Assert.Equal(2, dcs.SasMinutes);
            Assert.Equal(1234, dcs.TcpPort);
            Assert.False(dcs.UseTls);
            Assert.Equal("<path>", dcs.CaFile);
            Assert.True(dcs.DisableCrl);
            Assert.Equal(3, dcs.MqttVersion);
        }

        [Fact]
        public void ToStringReturnConnectionString()
        {
            IoTHubConnectionSettings dcs = new()
            {
                HostName = "h",
                DeviceId = "d",
                SharedAccessKey = "sas",
                ModelId = "dtmi",
                MqttVersion = 3
            };
            string expected = "HostName=h;TcpPort=8883;DeviceId=d;SharedAccessKey=***;ModelId=dtmi;MqttVersion=3";
            Assert.Equal(expected, dcs.ToString());
        }

        [Fact]
        public void ToStringReturnConnectionStringWithModule()
        {
            IoTHubConnectionSettings dcs = new()
            {
                HostName = "h",
                DeviceId = "d",
                ModuleId = "m",
                SharedAccessKey = "sas"
            };
            string expected = "HostName=h;TcpPort=8883;DeviceId=d;ModuleId=m;SharedAccessKey=***;MqttVersion=5";
            Assert.Equal(expected, dcs.ToString());
        }

        [Fact]
        public void InvalidMqttVersionThrowsException()
        {
            string cs = "HostName=<hubname>.azure-devices.net;DeviceId=<deviceId>;SharedAccessKey=<SasKey>;MaxRetries=-2;SasMinutes=aa;MqttVersion=4";
            try
            {
                IoTHubConnectionSettings dcs = new(cs);
                Assert.Fail("should throw");
            }
            catch (FormatException ex)
            {
                Assert.Equal("Invalid ConnectionSettings: Mqtt Version 4 not supported, should be '3' or '5'", ex.Message);

            }
        }

        [Fact]
        public void HostNameIsMandatory()
        {
            string cs = "DeviceId=<deviceId>;SharedAccessKey=<SasKey>";
            try
            {
                IoTHubConnectionSettings dcs = new(cs);
                Assert.Fail("should throw");
            }
            catch (FormatException ex)
            {
                Assert.Equal("Invalid ConnectionSettings: HostName is mandatory", ex.Message);
            }
        }

        [Fact]
        public void EmptyStringTrhows()
        {
            try
            {
                var cs = new ConnectionSettings("");
                Assert.Fail("should throw");
            }
            catch (FormatException ex)
            {
                Assert.Equal("Invalid ConnectionSettings: HostName is mandatory", ex.Message);
            }
        }

        [Fact]
        public void HubConnectionWithKeyFiles()
        {
            try
            {
                var cs = new IoTHubConnectionSettings("HostName=<hubname>.azure-devices.net;DeviceId=<deviceId>");
                Assert.Fail("should throw");
            }
            catch (FormatException ex)
            {
                Assert.Equal("Invalid ConnectionSettings: SharedAccessKey or X509Key must be set", ex.Message);
            }
        }
    }
}
