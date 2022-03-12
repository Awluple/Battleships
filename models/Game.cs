using System;
using System.IO;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Bson;

using BattleshipsShared.Communication;
using BattleshipsShared.Models;

namespace Battleships.Board
{
    public class Game
    {
        public (string playerOne, string playerTwo) players = (null, null);

        private static ClientWebSocket WsClient;

        public Game(string player) {
            players.playerOne = player;

            if(WsClient == null) {
                WsClient = new ClientWebSocket();
                WsClient.Options.AddSubProtocol("bson");
                Connect();
                Listener();
            }
            Send();
        }

        public static async void CloseConnection() {
            if(WsClient.State == WebSocketState.Open){
                await WsClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            }
        }

        private static async void Connect() {
            await WsClient.ConnectAsync(new Uri("ws://127.0.0.1:7850/"), CancellationToken.None);
        }


        private static async void Send() {
                    using (MemoryStream ms = new MemoryStream())
                    using (BsonDataWriter datawriter = new BsonDataWriter(ms))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Serialize(datawriter, new Message(RequestType.CreateGame, new Dictionary<string, object> {{"gameJoinInfo", new GameJoinInfo(1, "abc")}})); // TEMP
                        await WsClient.SendAsync(new ArraySegment<byte>(ms.ToArray()), WebSocketMessageType.Binary, true, CancellationToken.None);
                    }
                }

        private static async void Listener() {
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
                            Console.WriteLine(ex);
                        }
                }
        }
        
    }
}