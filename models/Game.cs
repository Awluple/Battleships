using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Bson;

namespace Battleships.Board
{

    public struct Message
    {

        public Message(string dataType, object data) {
            this.dataType = dataType;
            this.data = data;
        }

        public string dataType { get; set; }
        public object data { get; set; }
    }
    public struct GameJoinInfo
        {
        public GameJoinInfo(int id, string player)
        {
            this.id = id;
            this.player = player;
        }
        public int id { get; }
        public string player { get; }
        };

    public class Game
    {
        public (string playerOne, string playerTwo) players = (null, null);

        private static ClientWebSocket WsClient;

        public Game(string player) {
            players.playerOne = player;

            if(WsClient == null) {
                WsClient = new ClientWebSocket();
                WsClient.Options.AddSubProtocol("bson");
                this.Connect();
                this.Listener();
            }
            this.Send();
        }

        public async void CloseConnection() {
            await WsClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
        }

        private async void Connect() {
            await WsClient.ConnectAsync(new Uri("ws://127.0.0.1:7850/"), CancellationToken.None);
        }


        private async void Send() {
                    using (MemoryStream ms = new MemoryStream())
                    using (BsonDataWriter datawriter = new BsonDataWriter(ms))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        // serializer.Serialize(datawriter, new GameJoinInfo(1, "abc"));
                        await WsClient.SendAsync(new ArraySegment<byte>(ms.ToArray()), WebSocketMessageType.Binary, true, CancellationToken.None);
                    }
                }

        private async void Listener() {
            //  byte[] receiveBuffer = new byte[1024];

            while (true) {
                    ArraySegment<byte> receiveBuffer = new ArraySegment<byte>(new byte[1024]);
                    MemoryStream ms = new MemoryStream(receiveBuffer.Array);
                    await WsClient.ReceiveAsync(receiveBuffer, CancellationToken.None);
                    Message message;
                    try
                        {
                            using (BsonDataReader reader = new BsonDataReader(ms))
                            {
                                JsonSerializer serializer = new JsonSerializer();
                                message = serializer.Deserialize<Message>(reader);
                                JObject obj = (JObject)message.data;
                            }
                        }
                        catch (Newtonsoft.Json.JsonSerializationException ex)
                        {
                            
                        }
                }
        }
        
    }
}