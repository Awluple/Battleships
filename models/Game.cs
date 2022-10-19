using System;
using System.IO;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Bson;

using BattleshipsShared.Communication;
using BattleshipsShared.Models;

namespace Battleships.Board
{

    public class WebSocketContextEventArgs : System.EventArgs {
        public readonly Message message;

        /// <summary>Deserializes data comming from the server into Message object</summary>
        /// <param name="receiveBuffer">Received buffer from the listener</param>
        /// <returns>Deselialized Message object</returns>
        private Message GetData(byte[] receiveBuffer) {
            MemoryStream ms = new MemoryStream(receiveBuffer);
            try
                {
                    using (BsonDataReader reader = new BsonDataReader(ms))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        Message message = serializer.Deserialize<Message>(reader);
                        Debug.WriteLine($"Data recieved - {message.requestType}");
                        return message;
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

    public class WebSocketErrorContextEventArgs : System.EventArgs {

    }

    /// <summary>Holds a game sennsion, manages WebSocket connection with the server</summary>
    /// <param name="gameId">Id of a game to connect</param>
    public class Game
    {
        public static string opponent = null;
        public int gameId;

        private static ClientWebSocket WsClient;

        public static event EventHandler<WebSocketContextEventArgs> WebSocketMessage;
        public static event EventHandler<WebSocketErrorContextEventArgs> WebSocketError;

        public Game(int gameId) {
            this.gameId = gameId;
        }

        protected virtual void OnWebSocketMessage (WebSocketContextEventArgs e) {
            WebSocketMessage?.Invoke(this, e);
        }
        protected virtual void OnWebSocketError (WebSocketErrorContextEventArgs e) {
            WebSocketError?.Invoke(this, e);
        }

        /// <summary>Closes the WebSocket connection with the server if the connection exists</summary>
        public static async void CloseConnection() {
            if(WsClient != null && WsClient.State == WebSocketState.Open){
                WebSocketMessage = null;
                await WsClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                Debug.WriteLine("Websocket connection closed");
            }
        }
        /// <summary>Connect to the server using WebSocket</summary>
        public async Task Connect() {
            WsClient = new ClientWebSocket();
            
            WsClient.Options.AddSubProtocol("bson");
            WsClient.Options.SetRequestHeader("player", Settings.userId.ToString());
            WsClient.Options.SetRequestHeader("game", this.gameId.ToString());

            await WsClient.ConnectAsync(new Uri("ws://" + Settings.serverUri), CancellationToken.None);
            Debug.WriteLine("Connected to the Websocket Server");
            Listener(); // start listenig for upcoming messages
        }

        /// <summary>Manages sending messages to the server</summary>
        /// <param name="message">Message to be send to the server</param>
        private async void Send(Message message) {
                    using (MemoryStream ms = new MemoryStream())
                    using (BsonDataWriter datawriter = new BsonDataWriter(ms))
                    {
                        try
                        {
                            JsonSerializer serializer = new JsonSerializer();
                            serializer.Serialize(datawriter, message);
                            await WsClient.SendAsync(new ArraySegment<byte>(ms.ToArray()), WebSocketMessageType.Binary, true, CancellationToken.None);
                            Debug.WriteLine($"Data sent - {message.requestType}");
                        }
                        catch (System.Exception ex)
                        {
                            Debug.WriteLine(ex);
                        }
                    }
                }

        /// <summary>Listens to upcoming messenges from the server. Runs OnWebSocketMessage event when a messege is recived or OnWebSocketError if there was an error</summary>
        private async void Listener() {

            while (WsClient.State == WebSocketState.Open) {
                try
                {
                    ArraySegment<byte> receiveBuffer = new ArraySegment<byte>(new byte[1024]);
                    MemoryStream ms = new MemoryStream(receiveBuffer.Array);
                    await WsClient.ReceiveAsync(receiveBuffer, CancellationToken.None);
                    try
                        {
                            if(WsClient.State != WebSocketState.Open) return;
                            this.OnWebSocketMessage(new WebSocketContextEventArgs(receiveBuffer.Array));
                        }
                        catch (Newtonsoft.Json.JsonSerializationException ex)
                        {
                            Debug.WriteLine(ex);
                        }
                }catch (System.Net.WebSockets.WebSocketException ex)
                {
                    Debug.WriteLine(ex);
                    this.OnWebSocketError(new WebSocketErrorContextEventArgs());
                }
            }
        }
        
        public void JoinGame() {
            Send(new Message(RequestType.JoinGame, new Dictionary<string, object> {{"gameJoinInfo", new GameJoinInfo(gameId, Settings.userId.ToString())}}));
        }

        public void SendBoard(PlayerBoard board) {
            Send(new Message(RequestType.SetBoard, new Dictionary<string, object> {{"userBoard", new UserBoard(board.SerializeBoard())}}));
        }

        public void Shot(int row, int column) {
            Debug.WriteLine($"Sending shot data: Column: {column}, Row: {row}");
            Send(new Message(RequestType.PlayerShot, new Dictionary<string, object> {{"shot", new Shot(column, row)}}));
        }
        public void Rematch() {
            Send(new Message(RequestType.RematchProposition, new Dictionary<string, object> {}));
        }
    }
}