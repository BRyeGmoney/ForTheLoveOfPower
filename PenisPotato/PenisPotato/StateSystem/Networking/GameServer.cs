﻿using System;
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
        public StateManager stateManager;

        List<Player.NetworkPlayer> networkPlayers;

        bool _isRunning;

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
                                networkPlayers.Add(new Player.NetworkPlayer(msg.ReadString(), msg.SenderEndPoint.Address.ToString(), msg.ReadInt64(), Color.AliceBlue));
                                networkPlayers[networkPlayers.Count - 1].InitGamePlayer(false);

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
                            else if (packetType == (byte)Player.PacketType.STRUCTURE)
                            {
                                long id = msg.ReadInt64();
                                Player.NetworkPlayer nPlayer = networkPlayers.Find(nP => nP.uniqueIdentifer == id);
                                nPlayer.playerStructures.Add(DetermineStructureType(msg.ReadByte(), msg.ReadVector2(), nPlayer));

                                outmsg = server.CreateMessage();
                                outmsg.Write((byte)Player.PacketType.STRUCTURE);
                                outmsg.Write(id);
                                outmsg.Write(nPlayer.playerStructures[nPlayer.playerStructures.Count - 1].pieceType);
                                outmsg.Write(nPlayer.playerStructures[nPlayer.playerStructures.Count - 1].piecePosition);
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

        private Structures.Structure DetermineStructureType(byte type, Vector2 piecePosition, Player.NetworkPlayer nP)
        {
            switch (type)
            {
                case (byte)Structures.PieceTypes.Settlement:
                    return new Structures.Civil.Settlement(piecePosition, nP.playerColor, nP.masterState.ScreenManager.buildItems[(int)StateSystem.BuildItems.settlement].menuItem);
                case (byte)Structures.PieceTypes.Factory:
                    return new Structures.Economy.Factory(piecePosition, nP.playerColor, nP.masterState.ScreenManager.buildItems[(int)StateSystem.BuildItems.factory].menuItem);
                case (byte)Structures.PieceTypes.Market:
                    return new Structures.Economy.Market(piecePosition, nP.playerColor, nP.masterState.ScreenManager.buildItems[(int)StateSystem.BuildItems.market].menuItem);
                case (byte)Structures.PieceTypes.Exporter:
                    return new Structures.Economy.Exporter(piecePosition, nP.playerColor, nP.masterState.ScreenManager.buildItems[(int)StateSystem.BuildItems.exporter].menuItem);
                case (byte)Structures.PieceTypes.Barracks:
                    return new Structures.Military.Barracks(piecePosition, nP.playerColor, nP.masterState.ScreenManager.buildItems[(int)StateSystem.BuildItems.barracks].menuItem);
                case (byte)Structures.PieceTypes.TankDepot:
                    return new Structures.Military.TankDepot(piecePosition, nP.playerColor, nP.masterState.ScreenManager.buildItems[(int)StateSystem.BuildItems.tankDepot].menuItem);
                case (byte)Structures.PieceTypes.AirBase:
                    return new Structures.Military.AirBase(piecePosition, nP.playerColor, nP.masterState.ScreenManager.buildItems[(int)StateSystem.BuildItems.airfield].menuItem);
                case (byte)Structures.PieceTypes.LabourCamp:
                    return new Structures.Manipulation.LabourCamp(piecePosition, nP.playerColor, nP.masterState.ScreenManager.buildItems[(int)StateSystem.BuildItems.labourCamp].menuItem);
                case (byte)Structures.PieceTypes.MilitaryContractor:
                    return new Structures.Manipulation.MilitaryContractor(piecePosition, nP.playerColor, nP.masterState.ScreenManager.buildItems[(int)StateSystem.BuildItems.contractor].menuItem);
                case (byte)Structures.PieceTypes.Propaganda:
                    return new Structures.Manipulation.Propaganda(piecePosition, nP.playerColor, nP.masterState.ScreenManager.buildItems[(int)StateSystem.BuildItems.propaganda].menuItem);
                default:
                    return new Structures.Civil.TownCentre(piecePosition, nP.playerColor, nP.masterState.ScreenManager.buildItems[(int)StateSystem.BuildItems.settlement].menuItem);
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
