using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Lidgren.Network;
using PenisPotato.Networking;

namespace PenisPotato.Player
{
    public class NetworkPlayer : Player
    {
        public List<NetworkPlayer> peers;
        public long uniqueIdentifer;
        public String ipAddress;
        public Boolean IsReady;
        public Boolean IsHost;

        public NetClient client;
        public Queue<Structures.Structure> structuresToSend;

        // Create new outgoing message
        NetOutgoingMessage outmsg;

        public NetworkPlayer(bool isHost, string ipToConnectTo)
        {
            IsHost = isHost;

            NetPeerConfiguration config = new NetPeerConfiguration("game");
            config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            config.EnableMessageType(NetIncomingMessageType.Data);

            client = new NetClient(config);
            client.Start();

            peers = new List<NetworkPlayer>();

            if (isHost)
                client.DiscoverLocalPeers(14242);
            else
            {
                if (ipToConnectTo.Equals(""))
                    client.DiscoverLocalPeers(14242);
                else
                    client.DiscoverKnownPeer(new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ipToConnectTo), 14242));
            }
        }

        public NetworkPlayer(string name, string ipaddress, long identifier, Color color)
        {
            playerName = name;
            ipAddress = ipaddress;
            this.uniqueIdentifer = identifier;
            playerColor = color;

            peers = new List<NetworkPlayer>();
        }

        public void InitGamePlayer(bool isMain)
        {
            if (isMain)
                structuresToSend = new Queue<Structures.Structure>();
            else
            {
                playerStructures = new List<Structures.Structure>();
                playerSettlements = new List<Structures.Civil.Settlement>();
                playerUnits = new List<Units.Unit>();
            }
        }

        public override void Update(GameTime gameTime)
        {
            NetIncomingMessage msg;

            while ((msg = client.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.DiscoveryResponse:
                        // just connect to first server discovered
                        outmsg = client.CreateMessage();
                        outmsg.Write((byte)PacketType.LOGIN);
                        outmsg.Write("Poopface");
                        outmsg.Write(client.UniqueIdentifier);
                        uniqueIdentifer = client.UniqueIdentifier;
                        ipAddress = msg.SenderEndPoint.Address.ToString();
                        client.Connect(msg.SenderEndPoint.Address.ToString(), 14242, outmsg);
                        break;
                    case NetIncomingMessageType.Data:
                        byte packetType = msg.ReadByte();
                        if (packetType == (byte)PacketType.LOBBYSTATE)
                        {
                            peers.Clear();

                            int count = msg.ReadInt32();

                            // Iterate all players
                            for (int i = 0; i < count; i++)
                            {
                                // Create new character to hold the data
                                NetworkPlayer nP = new NetworkPlayer(msg.ReadString(), msg.ReadString(), msg.ReadInt64(), new Color(msg.ReadVector3()));

                                peers.Add(nP);
                            }
                        }
                        else if (packetType == (byte)PacketType.READY)
                        {
                            long id = msg.ReadInt64();
                            peers.Find(nP => nP.uniqueIdentifer == id).IsReady = !peers.Find(nP => nP.uniqueIdentifer == id).IsReady;
                        }
                        else if (packetType == (byte)PacketType.STRUCTURE)
                        {
                            long id = msg.ReadInt64();
                            if (id != this.client.UniqueIdentifier)
                            {
                                NetworkPlayer nPlayer = peers.Find(nP => nP.uniqueIdentifer == id);
                                nPlayer.playerStructures.Add(DetermineStructureType(msg.ReadByte(), msg.ReadVector2(), nPlayer));
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            if (structuresToSend != null && structuresToSend.Count > 0)
            {
                NetOutgoingMessage outmsg = client.CreateMessage();
                Structures.Structure building = structuresToSend.Dequeue();
                outmsg.Write((byte)PacketType.STRUCTURE);
                outmsg.Write(client.UniqueIdentifier);
                outmsg.Write(building.pieceType);
                outmsg.Write(building.piecePosition);
                client.SendMessage(outmsg, NetDeliveryMethod.ReliableOrdered);
            }
        }

        private Structures.Structure DetermineStructureType(byte type, Vector2 piecePosition, NetworkPlayer nP)
        {
            switch (type)
            {
                case (byte)Structures.PieceTypes.Settlement:
                    return new Structures.Civil.Settlement(piecePosition, nP.playerColor, masterState.ScreenManager.buildItems[(int)StateSystem.BuildItems.settlement].menuItem);
                case (byte)Structures.PieceTypes.Factory:
                    return new Structures.Economy.Factory(piecePosition, nP.playerColor, masterState.ScreenManager.buildItems[(int)StateSystem.BuildItems.factory].menuItem);
                case (byte)Structures.PieceTypes.Market:
                    return new Structures.Economy.Market(piecePosition, nP.playerColor, masterState.ScreenManager.buildItems[(int)StateSystem.BuildItems.market].menuItem);
                case (byte)Structures.PieceTypes.Exporter:
                    return new Structures.Economy.Exporter(piecePosition, nP.playerColor, masterState.ScreenManager.buildItems[(int)StateSystem.BuildItems.exporter].menuItem);
                case (byte)Structures.PieceTypes.Barracks:
                    return new Structures.Military.Barracks(piecePosition, nP.playerColor, masterState.ScreenManager.buildItems[(int)StateSystem.BuildItems.barracks].menuItem);
                case (byte)Structures.PieceTypes.TankDepot:
                    return new Structures.Military.TankDepot(piecePosition, nP.playerColor, masterState.ScreenManager.buildItems[(int)StateSystem.BuildItems.tankDepot].menuItem);
                case (byte)Structures.PieceTypes.AirBase:
                    return new Structures.Military.AirBase(piecePosition, nP.playerColor, masterState.ScreenManager.buildItems[(int)StateSystem.BuildItems.airfield].menuItem);
                case (byte)Structures.PieceTypes.LabourCamp:
                    return new Structures.Manipulation.LabourCamp(piecePosition, nP.playerColor, masterState.ScreenManager.buildItems[(int)StateSystem.BuildItems.labourCamp].menuItem);
                case (byte)Structures.PieceTypes.MilitaryContractor:
                    return new Structures.Manipulation.MilitaryContractor(piecePosition, nP.playerColor, masterState.ScreenManager.buildItems[(int)StateSystem.BuildItems.contractor].menuItem);
                case (byte)Structures.PieceTypes.Propaganda:
                    return new Structures.Manipulation.Propaganda(piecePosition, nP.playerColor, masterState.ScreenManager.buildItems[(int)StateSystem.BuildItems.propaganda].menuItem);
                default:
                    return new Structures.Civil.TownCentre(piecePosition, nP.playerColor, masterState.ScreenManager.buildItems[(int)StateSystem.BuildItems.settlement].menuItem);
            }
        }
    }

    public enum PacketType
    {
        LOGIN,
        LOBBYSTATE,
        READY,
        COLORCHANGE,
        STRUCTURE,
        UNIT
    }
}
