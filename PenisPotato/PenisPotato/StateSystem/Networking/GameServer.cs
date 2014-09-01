using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Lidgren.Network;
using PenisPotato.Networking;

namespace PenisPotato.StateSystem.Networking
{
    class GameServer
    {
        NetPeerConfiguration config;
        Thread runServer;
        bool inLobby = true;


	    // create and start server
        public NetServer server;
        NetOutgoingMessage outmsg;

        List<Player.NetworkPlayer> networkPlayers;
        List<Units.ServerCombat> ongoingFights;

        bool _isRunning;

        public GameServer()
        {
            ongoingFights = new List<Units.ServerCombat>();
        }

        public void Run()
        {
            runServer = new Thread(RunServer);
            runServer.Start();
        }

        public void RunServer()
        {
            networkPlayers = new List<Player.NetworkPlayer>();

            config = new NetPeerConfiguration("game");
			config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
			config.Port = 14242;

            server = new NetServer(config);
			server.Start();
            _isRunning = true;

            // schedule initial sending of position updates
			double nextSendUpdates = NetTime.Now;

			// run until escape is pressed
			while (_isRunning)
			{
				NetIncomingMessage msg;
				while ((msg = server.ReadMessage()) != null)
				{
					switch (msg.MessageType)
					{
						case NetIncomingMessageType.DiscoveryRequest:
							// Server received a discovery request from a client; send a discovery response (with no extra data attached)
							server.SendDiscoveryResponse(null, msg.SenderEndPoint);
							break;
                        case NetIncomingMessageType.ConnectionApproval:
                            // Read the first byte of the packet
                            // ( Enums can be casted to bytes, so it be used to make bytes human readable )
                            if (msg.ReadByte() == (byte)Player.PacketType.LOGIN)
                            {
                                Console.WriteLine("Incoming LOGIN");

                                // Approve client's connection
                                msg.SenderConnection.Approve();
                                networkPlayers.Add(new Player.NetworkPlayer(msg.ReadString(), msg.SenderEndPoint.Address.ToString(), msg.ReadInt64(), Color.Green));
                                networkPlayers[networkPlayers.Count - 1].InitGamePlayer(false, null);

                                outmsg = server.CreateMessage();
                                outmsg.Write((byte)Player.PacketType.LOBBYSTATE);
                                outmsg.Write(networkPlayers.Count);

                                foreach (Player.NetworkPlayer nP in networkPlayers)
                                {
                                    outmsg.Write(nP.playerName);
                                    outmsg.Write(nP.ipAddress);
                                    outmsg.Write(nP.uniqueIdentifer);
                                    outmsg.Write(nP.playerColor.ToVector3());
                                }
                            }
                            break;
						case NetIncomingMessageType.VerboseDebugMessage:
						case NetIncomingMessageType.DebugMessage:
						case NetIncomingMessageType.WarningMessage:
						case NetIncomingMessageType.ErrorMessage:
							// Just print diagnostic messages to console
							Console.WriteLine(msg.ReadString());
							break;
						case NetIncomingMessageType.StatusChanged:
							NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
                            if (status == NetConnectionStatus.Connected)
                            {

                                // Send message/packet to all connections, in reliable order
                                // Reliably means that each packet arrives in same order they were sent. Its slower than unreliable, but easiest to understand
                                server.SendToAll(outmsg, NetDeliveryMethod.ReliableOrdered);
                            }
                            else if (status == NetConnectionStatus.Disconnected)
                            {
                                Console.WriteLine("b");
                            }
							break;
                        case NetIncomingMessageType.Data:
                            byte packetType = msg.ReadByte();
                            // The client sent input to the server
                            if ((packetType == (byte)Player.PacketType.READY))
                            {
                                long id = msg.ReadInt64();
                                networkPlayers.Find(nP => nP.uniqueIdentifer == id).IsReady = !networkPlayers.Find(nP => nP.uniqueIdentifer == id).IsReady;

                                outmsg = server.CreateMessage();
                                outmsg.Write((byte)Player.PacketType.READY);
                                outmsg.Write(id);
                                server.SendToAll(outmsg, NetDeliveryMethod.ReliableOrdered);
                            }
                            else if (packetType == (byte)Player.PacketType.STRUCTURE_ADD)
                            {
                                long id = msg.ReadInt64();
                                Player.NetworkPlayer nPlayer = networkPlayers.Find(nP => nP.uniqueIdentifer == id);
                                //Read struct type, ownerIndex, position, player info
                                nPlayer.playerStructures.Add(DetermineStructureType(msg.ReadByte(), msg.ReadInt32(), msg.ReadVector2(), nPlayer));

                                outmsg = server.CreateMessage();
                                outmsg.Write((byte)Player.PacketType.STRUCTURE_ADD);
                                outmsg.Write(id);
                                outmsg.Write(nPlayer.playerStructures[nPlayer.playerStructures.Count - 1].pieceType);
                                outmsg.Write(nPlayer.playerStructures[nPlayer.playerStructures.Count - 1].settlementOwnerIndex);
                                outmsg.Write(nPlayer.playerStructures[nPlayer.playerStructures.Count - 1].piecePosition);
                                server.SendToAll(outmsg, NetDeliveryMethod.ReliableOrdered);
                            }
                            else if (packetType == (byte)Player.PacketType.SETTLEMENT_ADD)
                            {
                                long id = msg.ReadInt64();
                                Player.NetworkPlayer nPlayer = networkPlayers.Find(nP => nP.uniqueIdentifer == id);
                                Structures.Civil.Settlement newStruct = DetermineStructureType(msg.ReadByte(), -1, msg.ReadVector2(), nPlayer) as Structures.Civil.Settlement;
                                nPlayer.playerSettlements.Add(newStruct);
                                nPlayer.playerStructures.Add(newStruct);

                                outmsg = server.CreateMessage();
                                outmsg.Write((byte)Player.PacketType.SETTLEMENT_ADD);
                                outmsg.Write(id);
                                outmsg.Write(newStruct.piecePosition);
                                server.SendToAll(outmsg, NetDeliveryMethod.ReliableOrdered);
                            }
                            else if (packetType == (byte)Player.PacketType.SETTLEMENT_UPDATE)
                            {
                                short lenOfTransmission = msg.ReadInt16();
                                long defenderId = msg.ReadInt64();
                                long attackerId = msg.ReadInt64();
                                Vector2 settlementPosition = msg.ReadVector2();

                                if (lenOfTransmission <= 4)
                                {
                                    outmsg = server.CreateMessage();
                                    outmsg.Write((byte)Player.PacketType.SETTLEMENT_UPDATE);
                                    outmsg.Write(lenOfTransmission);
                                    outmsg.Write(defenderId);
                                    outmsg.Write(attackerId);
                                    outmsg.Write(settlementPosition);
                                }
                                else if (lenOfTransmission <= 6)
                                {
                                    short percConquered = msg.ReadInt16();

                                    if (percConquered >= 100)
                                    {
                                        Player.NetworkPlayer defender = networkPlayers.Find(nP => nP.uniqueIdentifer.Equals(defenderId));
                                        Player.NetworkPlayer attacker = networkPlayers.Find(nP => nP.uniqueIdentifer.Equals(attackerId));
                                        Structures.Civil.Settlement changingSettlement = defender.playerSettlements.Find(pS => pS.piecePosition.Equals(settlementPosition));
                                        changingSettlement.ChangeOwnership(attacker, defender);
                                    }

                                    outmsg = server.CreateMessage();
                                    outmsg.Write((byte)Player.PacketType.SETTLEMENT_UPDATE);
                                    outmsg.Write(lenOfTransmission);
                                    outmsg.Write(defenderId);
                                    outmsg.Write(attackerId);
                                    outmsg.Write(settlementPosition);
                                    outmsg.Write(percConquered);
                                }
                                server.SendToAll(outmsg, NetDeliveryMethod.ReliableOrdered);
                            }
                            else if (packetType == (byte)Player.PacketType.STRUCTURE_UPDATE)
                            {
                                long defenderId = msg.ReadInt64();
                                long attackerId = msg.ReadInt64();
                                Vector2 settlementPosition = msg.ReadVector2();
                                short conqIndex = msg.ReadInt16();
                                short percConquered = msg.ReadInt16();

                                outmsg = server.CreateMessage();
                                outmsg.Write((byte)Player.PacketType.STRUCTURE_UPDATE);
                                outmsg.Write(defenderId);
                                outmsg.Write(attackerId);
                                outmsg.Write(settlementPosition);
                                outmsg.Write(conqIndex);
                                outmsg.Write(percConquered);
                                server.SendToAll(outmsg, NetDeliveryMethod.ReliableOrdered);
                            }
                            else if (packetType == (byte)Player.PacketType.UNIT_ADD)
                            {
                                long id = msg.ReadInt64();
                                Player.NetworkPlayer nPlayer = networkPlayers.Find(nP => nP.uniqueIdentifer == id);
                                nPlayer.playerUnits.Add(DetermineUnitType(msg.ReadByte(), msg.ReadInt32(), msg.ReadVector2(), nPlayer));

                                outmsg = server.CreateMessage();
                                outmsg.Write((byte)Player.PacketType.UNIT_ADD);
                                outmsg.Write(id);
                                outmsg.Write(nPlayer.playerUnits[nPlayer.playerUnits.Count - 1].unitType);
                                outmsg.Write(nPlayer.playerUnits[nPlayer.playerUnits.Count - 1].numUnits);
                                outmsg.Write(nPlayer.playerUnits[nPlayer.playerUnits.Count - 1].piecePosition);
                                server.SendToAll(outmsg, NetDeliveryMethod.ReliableOrdered);
                            }
                            else if (packetType == (byte)Player.PacketType.UNIT_UPDATE)
                            {
                                long id = msg.ReadInt64();
                                Player.NetworkPlayer nPlayer = networkPlayers.Find(nP => nP.uniqueIdentifer == id);

                                int index = msg.ReadInt32();
                                nPlayer.playerUnits[index].numUnits = msg.ReadInt32();
                                nPlayer.playerUnits[index].piecePosition = msg.ReadVector2();

                                outmsg = server.CreateMessage();
                                outmsg.Write((byte)Player.PacketType.UNIT_UPDATE);
                                outmsg.Write(id);
                                outmsg.Write(index);
                                outmsg.Write(nPlayer.playerUnits[index].numUnits);
                                outmsg.Write(nPlayer.playerUnits[index].piecePosition);
                                server.SendToAll(outmsg, NetDeliveryMethod.ReliableOrdered);
                            }
                            else if (packetType == (byte)Player.PacketType.START_COMBAT)
                            {
                                long nPiD = msg.ReadInt64();
                                long nP2iD = msg.ReadInt64();

                                Player.NetworkPlayer nPlayer = networkPlayers.Find(nP => nP.uniqueIdentifer == nPiD);
                                Player.NetworkPlayer nPlayer2 = networkPlayers.Find(nP => nP.uniqueIdentifer == nP2iD);

                                int combatId = msg.ReadInt32();
                                int attInd = msg.ReadInt32();
                                int defInd = msg.ReadInt32();

                                //Get all participating parties
                                /*int amountOfUnitsnP1 = msg.ReadInt32();
                                int[] indicesnP1 = new int[amountOfUnitsnP1];
                                for (int x = 0; x < amountOfUnitsnP1; x++)
                                    indicesnP1[x] = msg.ReadInt32();
                                int amountOfUnitsnP2 = msg.ReadInt32();
                                int[] indicesnP2 = new int[amountOfUnitsnP2];
                                for (int x = 0; x < amountOfUnitsnP2; x++)
                                    indicesnP2[x] = msg.ReadInt32();*/

                                ongoingFights.Add(new Units.ServerCombat(nPlayer.playerUnits[attInd], nPlayer2.playerUnits[defInd],
                                    nPlayer.uniqueIdentifer, nPlayer2.uniqueIdentifer));

                                outmsg = server.CreateMessage();
                                outmsg.Write((byte)Player.PacketType.START_COMBAT);
                                outmsg.Write(nPiD);
                                outmsg.Write(nP2iD);
                                outmsg.Write(combatId);
                                outmsg.Write(ongoingFights.Count - 1);
                                outmsg.Write(nPlayer.playerUnits.IndexOf(nPlayer.playerUnits.Find(pU => pU.Equals(ongoingFights[ongoingFights.Count - 1].attacker[0]))));
                                outmsg.Write(nPlayer2.playerUnits.IndexOf(nPlayer2.playerUnits.Find(pU => pU.Equals(ongoingFights[ongoingFights.Count - 1].defender[0]))));
                                server.SendToAll(outmsg, NetDeliveryMethod.ReliableOrdered);
                            }
                            else if (packetType == (byte)Player.PacketType.UPDATE_COMBAT)
                            {
                                long nPiD = msg.ReadInt64();
                                long nP2iD = msg.ReadInt64();

                                Player.NetworkPlayer nPlayer = networkPlayers.Find(nP => nP.uniqueIdentifer == nPiD);
                                Player.NetworkPlayer nPlayer2 = networkPlayers.Find(nP => nP.uniqueIdentifer == nP2iD);
                                int fightIndex = msg.ReadInt32();

                                //Get all participating parties
                                int amountOfUnitsnP1 = msg.ReadInt32();
                                for (int x = 0; x < amountOfUnitsnP1; x++)
                                    ongoingFights[fightIndex].AddUnit(nPlayer.playerUnits[msg.ReadInt32()], true);

                                int amountOfUnitsnP2 = msg.ReadInt32();
                                for (int x = 0; x < amountOfUnitsnP2; x++)
                                    ongoingFights[fightIndex].AddUnit(nPlayer2.playerUnits[msg.ReadInt32()], false);

                                int unitToUpdate = ongoingFights[fightIndex].Update();
                                if (unitToUpdate > -1)
                                {
                                    outmsg = server.CreateMessage();
                                    outmsg.Write((byte)Player.PacketType.UPDATE_COMBAT);
                                    if (ongoingFights[fightIndex].lastWin)
                                    {
                                        outmsg.Write(nPlayer.uniqueIdentifer);
                                        unitToUpdate = nPlayer2.playerUnits.IndexOf(ongoingFights[fightIndex].defender[unitToUpdate]);
                                    }
                                    else
                                    {
                                        outmsg.Write(nPlayer2.uniqueIdentifer);
                                        unitToUpdate = nPlayer.playerUnits.IndexOf(ongoingFights[fightIndex].attacker[unitToUpdate]);
                                    }
                                    outmsg.Write(unitToUpdate);
                                    outmsg.Write(fightIndex);
                                    server.SendToAll(outmsg, NetDeliveryMethod.ReliableOrdered);
                                }
                            }
                            else if (packetType == (byte)Player.PacketType.DELETE_COMBAT)
                            {
                                long npid = msg.ReadInt64();
                                int fightindex = msg.ReadInt32();
                                ongoingFights.RemoveAt(fightindex);
                                outmsg = server.CreateMessage();
                                outmsg.Write((byte)Player.PacketType.DELETE_COMBAT);
                                outmsg.Write(npid);
                                outmsg.Write(fightindex);
                                server.SendToAll(outmsg, NetDeliveryMethod.ReliableOrdered);
                            }
                            break;
					}

					//
					// send position updates 30 times per second
					//
					double now = NetTime.Now;
					if (!inLobby && now > nextSendUpdates)
					{
						// Yes, it's time to send position updates

						// for each player...
						foreach (NetConnection player in server.Connections)
						{
							// ... send information about every other player (actually including self)
							foreach (NetConnection otherPlayer in server.Connections)
							{
								// send position update about 'otherPlayer' to 'player'
								NetOutgoingMessage om = server.CreateMessage();

								// write who this position is for
								om.Write(otherPlayer.RemoteUniqueIdentifier);

								if (otherPlayer.Tag == null)
									otherPlayer.Tag = new int[2];

								int[] pos = otherPlayer.Tag as int[];
								om.Write(pos[0]);
								om.Write(pos[1]);

								// send message
								server.SendMessage(om, player, NetDeliveryMethod.ReliableOrdered);
							}
						}

						// schedule next update
						nextSendUpdates += (1.0 / 30.0);
					}
				}

				// sleep to allow other processes to run smoothly
				Thread.Sleep(1);
			}

			server.Shutdown("app exiting");
		}

        private Units.Unit DetermineUnitType(byte type, int numUnits, Vector2 piecePosition, Player.NetworkPlayer nP)
        {
            switch (type)
            {
                case (byte)Units.UnitType.Dictator:
                    return new Units.Misc.Dictator(piecePosition, nP.playerColor, null);
                case (byte)Units.UnitType.Infantry:
                    return new Units.Infantry(piecePosition, nP.playerColor, numUnits, null);
                case (byte)Units.UnitType.Tank:
                    return new Units.Tank(piecePosition, nP.playerColor, numUnits, null);
                case (byte)Units.UnitType.Jet:
                    return new Units.Jet(piecePosition, nP.playerColor, numUnits, null);
                default:
                    return null;
            }
        }

        private Structures.Structure DetermineStructureType(byte type, int owner, Vector2 piecePosition, Player.NetworkPlayer nP)
        {
            switch (type)
            {
                case (byte)Structures.PieceTypes.Settlement:
                    return new Structures.Civil.Settlement(piecePosition, nP.playerColor, null);
                case (byte)Structures.PieceTypes.Factory:
                    return new Structures.Economy.Factory(piecePosition, nP.playerColor, null, owner);
                case (byte)Structures.PieceTypes.Market:
                    return new Structures.Economy.Market(piecePosition, nP.playerColor, null, owner);
                case (byte)Structures.PieceTypes.Exporter:
                    return new Structures.Economy.Exporter(piecePosition, nP.playerColor, null, owner);
                case (byte)Structures.PieceTypes.Barracks:
                    return new Structures.Military.Barracks(piecePosition, nP.playerColor, null, owner);
                case (byte)Structures.PieceTypes.TankDepot:
                    return new Structures.Military.TankDepot(piecePosition, nP.playerColor, null, owner);
                case (byte)Structures.PieceTypes.AirBase:
                    return new Structures.Military.AirBase(piecePosition, nP.playerColor, null, owner);
                case (byte)Structures.PieceTypes.LabourCamp:
                    return new Structures.Manipulation.LabourCamp(piecePosition, nP.playerColor, null, owner);
                case (byte)Structures.PieceTypes.MilitaryContractor:
                    return new Structures.Manipulation.MilitaryContractor(piecePosition, nP.playerColor, null, owner);
                case (byte)Structures.PieceTypes.Propaganda:
                    return new Structures.Manipulation.Propaganda(piecePosition, nP.playerColor, null, owner);
                default:
                    return new Structures.Civil.TownCentre(piecePosition, nP.playerColor, null);
            }
        }

        public void Stop()
        {
            _isRunning = false;
        }

        public Boolean IsRunning()
        {
            return _isRunning;
        }
    }
}
