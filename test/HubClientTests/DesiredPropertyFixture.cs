using MQTTnet.Client.Extensions.AzureIoT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace HubClientTests
{
    public class DesiredPropertyFixture
    {
        [Fact]
        public void LoadInt()
        {
            string desiredJson = """
                {
                    "aa" : 32,
                    "$version" : 5
                }
                """;
            JsonNode? node = JsonNode.Parse(desiredJson);
                

            var dp = new DesiredProperties(node);

            Assert.Equal(32, dp["aa"]);
            Assert.Equal(5, dp.Version);
        }

        [Fact]
        public void LoadDouble()
        {
            string desiredJson = """
                {
                    "aa" : 3.2,
                    "$version" : 5
                }
                """;
            JsonNode? node = JsonNode.Parse(desiredJson);


            var dp = new DesiredProperties(node);

            Assert.Equal(3.2, dp["aa"]);
            Assert.Equal(5, dp.Version);
        }


        [Fact]
        public void LoadLong()
        {
            string desiredJson = """
                {
                    "loong" : 9223372036854775807,
                    "$version" : 5
                }
                """;
            JsonNode? node = JsonNode.Parse(desiredJson);


            var dp = new DesiredProperties(node);

            Assert.Equal(9223372036854775807, dp["loong"]);
            Assert.Equal(5, dp.Version);
        }

        [Fact]
        public void LoadString()
        {
            string desiredJson = """
                {
                    "name" : "mr me",
                    "$version" : 5
                }
                """;
            JsonNode? node = JsonNode.Parse(desiredJson);


            var dp = new DesiredProperties(node);

            Assert.Equal("mr me", dp["name"]);
            Assert.Equal(5, dp.Version);
        }
    }
}
