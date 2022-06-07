using System;
using System.IO;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Diagnostics;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Bson;

using BattleshipsShared.Communication;
using BattleshipsShared.Models;

namespace Battleships.Board
{

    public class WebSocketContextEventArgs : System.EventArgs {
        public readonly Message message;


        private Message GetData(byte[] receiveBuffer) {
            MemoryStream ms = new MemoryStream(receiveBuffer);
            try
                {
                    using (BsonDataReader reader = new BsonDataReader(ms))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        return serializer.Deserialize<Message>(reader);
                    }
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return new Message(RequestType.Error, null);
                }
        }

        public WebSocketContextEventArgs (byte[] receiveBuffer) {
            this.message = this.GetData(receiveBuffer);
        }
    }


    public class Game
    {
        public static string opponent = null;
        public int gameId;

        private static ClientWebSocket WsClient;

        public static event EventHandler<WebSocketContextEventArgs> WebSocketMessage;

        public Game(int gameId) {
            this.gameId = gameId;
            if(WsClient == null) {
                Connect();
            }
        }

        protected virtual void OnWebSocketMessage (WebSocketContextEventArgs e) {
            WebSocketMessage?.Invoke(this, e);
        }

        public async void CloseConnection() {
            if(WsClient.State == WebSocketState.Open){
                await WsClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            }
        }

        private async void Connect() {
            WsClient = new ClientWebSocket();
            WsClient.Options.AddSubProtocol("bson");
            WsClient.Options.SetRequestHeader("player", Settings.userId.ToString());
            await WsClient.ConnectAsync(new Uri("ws://" + Settings.serverUri), CancellationToken.None);
            Listener();
        }


        private async void Send(Message message) {
                    using (MemoryStream ms = new MemoryStream())
                    using (BsonDataWriter datawriter = new BsonDataWriter(ms))
                    {
                        try
                        {
                            JsonSerializer serializer = new JsonSerializer();
                            serializer.Serialize(datawriter, message);
                            await WsClient.SendAsync(new ArraySegment<byte>(ms.ToArray()), WebSocketMessageType.Binary, true, CancellationToken.None);
                        }
                        catch (System.Exception ex)
                        {
                             // TODO
                        }
                        finally
                        {
                            
                        }
                    }
                }

        private async void Listener() {

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
                                this.OnWebSocketMessage(new WebSocketContextEventArgs(receiveBuffer.Array));
                            }
                        }
                        catch (Newtonsoft.Json.JsonSerializationException ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
        }
        
        public void JoinGame() {
            Send(new Message(RequestType.JoinGame, new Dictionary<string, object> {{"gameJoinInfo", new GameJoinInfo(gameId, Settings.userId.ToString())}}));
        }

        public void SendBoard(GameBoard board) {

            Send(new Message(RequestType.SetBoard, new Dictionary<string, object> {{"userBoard", new UserBoard(board.SerializeBoard(), gameId)}}));
        }
    }
}