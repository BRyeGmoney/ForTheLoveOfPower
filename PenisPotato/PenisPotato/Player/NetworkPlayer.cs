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
        public Queue<Units.Unit> unitsToSend;
        public Queue<Units.Unit> unitsToUpdate;
        public Queue<Units.Combat> ongoingFights;

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

        public void InitGamePlayer(bool isMain, StateSystem.Screens.GameplayScreen masterState)
        {
            if (isMain)
            {
                unitsToSend = new Queue<Units.Unit>();
                unitsToUpdate = new Queue<Units.Unit>();
                structuresToSend = new Queue<Structures.Structure>();
                ongoingFights = new Queue<Units.Combat>();
                this.masterState = masterState;
            }
            else
            {
                playerStructures = new List<Structures.Structure>();
                playerSettlements = new List<Structures.Civil.Settlement>();
                playerUnits = new List<Units.Unit>();
                //Combat List
                combat = new List<Units.Combat>();
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
                                if (nPlayer.client == null)
                                    nPlayer.playerStructures.Add(DetermineStructureType(msg.ReadByte(), msg.ReadVector2(), nPlayer));
                            }
                        }
                        else if (packetType == (byte)PacketType.UNIT_ADD)
                        {
                            long id = msg.ReadInt64();
                            if (id != this.client.UniqueIdentifier)
                            {
                                NetworkPlayer nPlayer = peers.Find(nP => nP.uniqueIdentifer == id);
                                if (nPlayer.client == null)
                                    nPlayer.playerUnits.Add(DetermineUnitType(msg.ReadByte(), msg.ReadInt32(), msg.ReadVector2(), nPlayer));
                            }
                        }
                        else if (packetType == (byte)PacketType.UNIT_UPDATE)
                        {
                            long id = msg.ReadInt64();
                            if (id != this.client.UniqueIdentifier)
                            {
                                NetworkPlayer  nPlayer = peers.Find(nP => nP.uniqueIdentifer == id);
                                if (nPlayer.client == null)
                                {
                                    int index = msg.ReadInt32();
                                    nPlayer.playerUnits[index].numUnits = msg.ReadInt32();
                                    nPlayer.playerUnits[index].piecePosition = msg.ReadVector2();
                                }
                            }
                        }
                        else if (packetType == (byte)PacketType.START_COMBAT)
                        {
                            NetworkPlayer nPlayer = peers.Find(nP => nP.uniqueIdentifer == msg.ReadInt64());
                            NetworkPlayer nPlayer2 = peers.Find(nP => nP.uniqueIdentifer == msg.ReadInt64());

                            int oldCombatId = msg.ReadInt32();

                            if (!nPlayer.uniqueIdentifer.Equals(this.uniqueIdentifer))
                            {
                                (this.combat.Find(fight => (fight as Units.SkeletonCombat).combatid.Equals(oldCombatId)) as Units.SkeletonCombat).combatid = msg.ReadInt32();
                            }
                            else
                            {
                                int newCombatid = msg.ReadInt32();
                                this.combat.Add(new Units.SkeletonCombat(nPlayer.playerUnits[msg.ReadInt32()], nPlayer2.playerUnits[msg.ReadInt32()], nPlayer.uniqueIdentifer, nPlayer2.uniqueIdentifer) { combatid = newCombatid });
                            }
                        }
                        else if (packetType == (byte)PacketType.UPDATE_COMBAT)
                        {

                        }
                        break;
                    default:
                        break;
                }
            }

            if (structuresToSend != null && structuresToSend.Count > 0)
            {
                outmsg = client.CreateMessage();
                Structures.Structure building = structuresToSend.Dequeue();
                outmsg.Write((byte)PacketType.STRUCTURE);
                outmsg.Write(client.UniqueIdentifier);
                outmsg.Write(building.pieceType);
                outmsg.Write(building.piecePosition);
                client.SendMessage(outmsg, NetDeliveryMethod.ReliableOrdered);
            }
            if (unitsToSend != null && unitsToSend.Count > 0)
            {
                outmsg = client.CreateMessage();
                Units.Unit unit = unitsToSend.Dequeue();
                outmsg.Write((byte)PacketType.UNIT_ADD);
                outmsg.Write(client.UniqueIdentifier);
                outmsg.Write(unit.unitType);
                outmsg.Write(unit.numUnits);
                outmsg.Write(unit.piecePosition);
                client.SendMessage(outmsg, NetDeliveryMethod.ReliableOrdered);
            }
            if (unitsToUpdate != null && unitsToUpdate.Count > 0)
            {
                outmsg = client.CreateMessage();
                Units.Unit unit = unitsToUpdate.Dequeue();

                outmsg.Write((byte)PacketType.UNIT_UPDATE);
                outmsg.Write(client.UniqueIdentifier);
                outmsg.Write(this.masterState.playerOne.playerUnits.IndexOf(unit));
                outmsg.Write(unit.numUnits);
                outmsg.Write(unit.piecePosition);
                client.SendMessage(outmsg, NetDeliveryMethod.ReliableOrdered);
            }
            if (ongoingFights != null && ongoingFights.Count > 0)
            {
                outmsg = client.CreateMessage();
                Units.SkeletonCombat fight = ongoingFights.Dequeue() as Units.SkeletonCombat;

                if (fight.combatid < 0)
                {
                    outmsg.Write((byte)PacketType.START_COMBAT);

                    outmsg.Write(fight.attackingNetworkID);
                    outmsg.Write(fight.defendingNetworkID);

                    outmsg.Write(fight.combatid);

                    outmsg.Write(peers.Find(nP => nP.uniqueIdentifer == fight.attackingNetworkID).playerUnits.IndexOf(fight.attacker[0]));
                    outmsg.Write(peers.Find(nP => nP.uniqueIdentifer == fight.defendingNetworkID).playerUnits.IndexOf(fight.defender[0]));
                }
                else //update
                {
                    outmsg = client.CreateMessage();
                    outmsg.Write((byte)PacketType.UPDATE_COMBAT);

                    outmsg.Write(fight.combatid);

                    outmsg.Write(fight.attackingNetworkID);
                    outmsg.Write(fight.defendingNetworkID);

                    List<int> units = fight.FindNeighboringEnemies(fight.attacker[0], true, peers);
                    outmsg.Write(units.Count);
                    units.ForEach(un => outmsg.Write(un));

                    units = fight.FindNeighboringEnemies(fight.defender[0], false, peers);
                    outmsg.Write(units.Count);
                    units.ForEach(un => outmsg.Write(un));
                }
            }

        }

        private Units.Unit DetermineUnitType(byte type, int numUnits, Vector2 piecePosition, NetworkPlayer nP)
        {
            switch (type)
            {
                case (byte)Units.UnitType.Dictator:
                    return new Units.Misc.Dictator(piecePosition, nP.playerColor, nP.ScreenManager.buildItems[(int)StateSystem.BuildItems.dictator].menuItem);
                case (byte)Units.UnitType.Infantry:
                    return new Units.Infantry(piecePosition, nP.playerColor, numUnits, nP.ScreenManager.buildItems[(int)StateSystem.BuildItems.infantry].menuItem);
                case (byte)Units.UnitType.Tank:
                    return new Units.Tank(piecePosition, nP.playerColor, numUnits, nP.ScreenManager.buildItems[(int)StateSystem.BuildItems.tank].menuItem);
                case (byte)Units.UnitType.Jet:
                    return new Units.Jet(piecePosition, nP.playerColor, numUnits, nP.ScreenManager.buildItems[(int)StateSystem.BuildItems.plane].menuItem);
                default:
                    return null;
            }
        }

        private Structures.Structure DetermineStructureType(byte type, Vector2 piecePosition, NetworkPlayer nP)
        {
            switch (type)
            {
                case (byte)Structures.PieceTypes.Settlement:
                    return new Structures.Civil.Settlement(piecePosition, nP.playerColor, nP.ScreenManager.buildItems[(int)StateSystem.BuildItems.settlement].menuItem);
                case (byte)Structures.PieceTypes.Factory:
                    return new Structures.Economy.Factory(piecePosition, nP.playerColor, nP.ScreenManager.buildItems[(int)StateSystem.BuildItems.factory].menuItem);
                case (byte)Structures.PieceTypes.Market:
                    return new Structures.Economy.Market(piecePosition, nP.playerColor, nP.ScreenManager.buildItems[(int)StateSystem.BuildItems.market].menuItem);
                case (byte)Structures.PieceTypes.Exporter:
                    return new Structures.Economy.Exporter(piecePosition, nP.playerColor, nP.ScreenManager.buildItems[(int)StateSystem.BuildItems.exporter].menuItem);
                case (byte)Structures.PieceTypes.Barracks:
                    return new Structures.Military.Barracks(piecePosition, nP.playerColor, nP.ScreenManager.buildItems[(int)StateSystem.BuildItems.barracks].menuItem);
                case (byte)Structures.PieceTypes.TankDepot:
                    return new Structures.Military.TankDepot(piecePosition, nP.playerColor, nP.ScreenManager.buildItems[(int)StateSystem.BuildItems.tankDepot].menuItem);
                case (byte)Structures.PieceTypes.AirBase:
                    return new Structures.Military.AirBase(piecePosition, nP.playerColor, nP.ScreenManager.buildItems[(int)StateSystem.BuildItems.airfield].menuItem);
                case (byte)Structures.PieceTypes.LabourCamp:
                    return new Structures.Manipulation.LabourCamp(piecePosition, nP.playerColor, nP.ScreenManager.buildItems[(int)StateSystem.BuildItems.labourCamp].menuItem);
                case (byte)Structures.PieceTypes.MilitaryContractor:
                    return new Structures.Manipulation.MilitaryContractor(piecePosition, nP.playerColor, nP.ScreenManager.buildItems[(int)StateSystem.BuildItems.contractor].menuItem);
                case (byte)Structures.PieceTypes.Propaganda:
                    return new Structures.Manipulation.Propaganda(piecePosition, nP.playerColor, nP.ScreenManager.buildItems[(int)StateSystem.BuildItems.propaganda].menuItem);
                default:
                    return new Structures.Civil.TownCentre(piecePosition, nP.playerColor, nP.ScreenManager.buildItems[(int)StateSystem.BuildItems.settlement].menuItem);
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
        UNIT_ADD,
        UNIT_UPDATE,
        START_COMBAT,
        UPDATE_COMBAT,
    }
}
