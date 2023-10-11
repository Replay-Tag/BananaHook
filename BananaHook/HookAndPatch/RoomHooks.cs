using BananaHook.HookAndPatch;
using BananaHook.Utils;
using GorillaLocomotion;
using HarmonyLib;
using Photon.Pun;
using System;
using System.Threading;

namespace BananaHook.Patches
{
    /* Utilla's thing */
    [HarmonyPatch(typeof(GorillaNetworking.PhotonNetworkController))]
    [HarmonyPatch("OnJoinedRoom", MethodType.Normal)]
    internal class OnRoomJoined
    {
        private static bool m_bIsPrivateLobby = false;
        private static void Postfix()
        {
            // That's because player's VRRigs are not still initialized :(
            new Thread(Postfix_Delayed).Start();
        }
        private static void Postfix_Delayed()
        {
            Thread.Sleep(100);

            object obj;
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("gameMode", out obj))
            {
                string gm = obj.ToString();

                if (gm.Contains("forest")) Room.m_eTriggeredMap = eJoinedMap.Forest;
                else if (gm.Contains("cave")) Room.m_eTriggeredMap = eJoinedMap.Cave;
                else if (gm.Contains("canyon")) Room.m_eTriggeredMap = eJoinedMap.Canyon;
                else if (gm.Contains("city")) Room.m_eTriggeredMap = eJoinedMap.GorillaShop;
                else if (gm.Contains("mountain")) Room.m_eTriggeredMap = eJoinedMap.Mountain;
                else Room.m_eTriggeredMap = eJoinedMap.Unknown;

                Room.m_bModdedLobby = gm.Contains("MODDED_");

                    if (gm.Contains("CASUAL")) Room.m_eCurrentGamemode = eRoomGamemode.Casual;
                else if (gm.Contains("INFECTION")) Room.m_eCurrentGamemode = eRoomGamemode.Infection;
                else if (gm.Contains("HUNT")) Room.m_eCurrentGamemode = eRoomGamemode.Hunt;
                else Room.m_eCurrentGamemode = eRoomGamemode.Custom;

                if (gm.Contains("DEFAULT")) Room.m_eCurrentLobbyMode = eRoomQueue.Default;
                else if (gm.Contains("COMPETITIVE")) Room.m_eCurrentLobbyMode = eRoomQueue.Competitive;
                else Room.m_eCurrentLobbyMode = eRoomQueue.Custom;
            }
            else
            {
                Room.m_eCurrentLobbyMode = eRoomQueue.Custom;
                Room.m_eCurrentGamemode = eRoomGamemode.Custom;
            }

            Room.m_hCurrentIt = Players.GetFirstGuyInfected(); // No other way to know
            Room.m_bIsTagging = Room.IsTagging();
            Room.m_szRoomCode = null;
            Room.CheckForTheGameEndPre();
            if (PhotonNetwork.InRoom)
            {
                var currentRoom = PhotonNetwork.NetworkingClient.CurrentRoom;
                m_bIsPrivateLobby = !currentRoom.IsVisible || currentRoom.CustomProperties.ContainsKey("Description");
                Room.m_szRoomCode = PhotonNetwork.CurrentRoom.Name;
            }
            if (Events.OnRoomJoined != null)
            {
                RoomJoinedArgs args = new RoomJoinedArgs();
                args.isPrivate = m_bIsPrivateLobby;
                args.roomCode = Room.m_szRoomCode;
                object[] obja = { null, args };
                foreach (var del in Events.OnRoomJoined.GetInvocationList())
                {
                    try
                    {
                        del.DynamicInvoke(obja);
                    }
                    catch (Exception e) { BananaHook.Log("OnRoomJoined Exception: " + e.Message + "\n" + e.StackTrace); }
                }
            }
            Room.CheckForTheGameEndPost();
        }
    }

    [HarmonyPatch(typeof(PhotonNetwork))]
    [HarmonyPatch("Disconnect", MethodType.Normal)]
    internal class OnRoomDisconnected
    {
        private static void Prefix()
        {
            Room.m_bIsGameEnded = false;
            Room.m_szRoomCode = null;
            if (!PhotonNetwork.InRoom) return;
            object[] obj = { null, null };
            foreach (var del in Events.OnRoomDisconnected.GetInvocationList())
            {
                try
                {
                    del.DynamicInvoke(obj);
                }
                catch (Exception e) { BananaHook.Log("OnRoomDisconnected Exception: " + e.Message + "\n" + e.StackTrace); }
            }
        }
    }

    [HarmonyPatch(typeof(PhotonHandler))]
    [HarmonyPatch("OnPlayerEnteredRoom", MethodType.Normal)]
    internal class OnPlayerConnected
    {
        private static void Postfix(Photon.Realtime.Player newPlayer)
        {
            try
            {
                if (!PhotonNetwork.InRoom) return;
                if (Room.m_eCurrentLobbyMode == eRoomQueue.Default || Room.m_eCurrentLobbyMode == eRoomQueue.Competitive)
                {
                    bool isTaggingNow = Room.IsTagging();
                    if (Room.m_bIsTagging != isTaggingNow && Events.OnTagOrInfectChange != null)
                    {
                        IsTagOrInfectArgs args = new IsTagOrInfectArgs();
                        args.isTagging = isTaggingNow;
                        object[] obj = { null, args };
                        foreach (var del in Events.OnTagOrInfectChange.GetInvocationList())
                        {
                            try
                            {
                                del.DynamicInvoke(obj);
                            }
                            catch (Exception e) { BananaHook.Log("OnTagOrInfectChange Exception: " + e.Message + "\n" + e.StackTrace); }
                        }
                    }
                    Room.m_bIsTagging = isTaggingNow;
                }
                if (Events.OnPlayerConnected != null)
                {
                    PlayerDisConnectedArgs args = new PlayerDisConnectedArgs();
                    args.player = newPlayer;
                    object[] obj = { null, args };
                    foreach (var del in Events.OnPlayerConnected.GetInvocationList())
                    {
                        try
                        {
                            del.DynamicInvoke(obj);
                        }
                        catch (Exception e) { BananaHook.Log("OnPlayerConnected Exception: " + e.Message + "\n" + e.StackTrace); }
                    }
                }
            }
            catch (Exception e) { BananaHook.Log("OnPlayerEnteredRoom Exception: " + e.Message + "\n" + e.StackTrace); }
        }
    }

    [HarmonyPatch(typeof(VRRig))]
    [HarmonyPatch("PlayTagSound", MethodType.Normal)]
    internal class OnPlayedTagSound
    {
        private static void Postfix(VRRig __instance, int soundIndex, float soundVolume)
        {
            // We dont need to listen for sounds if we're sending them.
            if (Photon.Pun.PhotonNetwork.IsMasterClient) return;

            // Can be risky. What if "2" become changed?
            // Also it's really useful since there's a glitch
            // where someone is NOT tagged, but the game ended.
            switch (soundIndex)
            {
                case 0: // Player Tagged (or infected, or hunted)
                    if (Room.m_eCurrentGamemode == eRoomGamemode.Hunt)
                    {
                        if (Room.m_bIsGameEnded)
                        {
                            Room.m_bIsGameEnded = false;
                            Room.m_hCurrentIt = Players.GetTargetOf(Photon.Pun.PhotonNetwork.LocalPlayer);
                            OnRoundStartArgs args = new OnRoundStartArgs();
                            args.player = null;
                            object[] obj = { null, args };
                            foreach (var del in Events.OnRoundStart.GetInvocationList())
                            {
                                try
                                {
                                    del.DynamicInvoke(obj);
                                }
                                catch (Exception e) { BananaHook.Log("OnRoundStart (PlayTagSound HUNT) Exception: " + e.Message + "\n" + e.StackTrace); }
                            }
                        }
                        else
                        {
                            OnPlayerTaggedByPlayerHook.OnEvent(Players.GetTargetOfWho(Players.FindPlayerOfVRRig(__instance)), Players.FindPlayerOfVRRig(__instance));
                        }
                    }
                    else if (Room.m_eCurrentGamemode != eRoomGamemode.Casual && Room.m_bIsGameEnded && !Room.m_bIsTagging)
                    {
                        Room.m_bIsGameEnded = false;
                        Room.m_hCurrentIt = Players.FindPlayerOfVRRig(__instance);
                        OnRoundStartArgs args = new OnRoundStartArgs();
                        args.player = Room.m_hCurrentIt;
                        object[] obj = { null, args };
                        foreach (var del in Events.OnRoundStart.GetInvocationList())
                        {
                            try
                            {
                                del.DynamicInvoke(obj);
                            }
                            catch (Exception e) { BananaHook.Log("OnRoundStart (PlayTagSound) Exception: " + e.Message + "\n" + e.StackTrace); }
                        }
                    }
                    break;

                //case 1: break; // Player joined? Tag/Untag?

                case 2: // End of Infection Game, Hunt Game
                    Room.m_bIsGameEnded = true;
                    object[] obja = { null, null };
                    foreach (var del in Events.OnRoundEndPre.GetInvocationList())
                    {
                        try
                        {
                            del.DynamicInvoke(obja);
                        }
                        catch (Exception e) { BananaHook.Log("OnRoundEndPre Exception: " + e.Message + "\n" + e.StackTrace); }
                    }
                    foreach (var del in Events.OnRoundEndPost.GetInvocationList())
                    {
                        try
                        {
                            del.DynamicInvoke(obja);
                        }
                        catch (Exception e) { BananaHook.Log("OnRoundEndPost Exception: " + e.Message + "\n" + e.StackTrace); }
                    }
                    break;

                //case 3: break; // Flag taken?

                case 5: // Hunted gorillas slowdown
                    {

                        break;
                    }
            }
        }
    }

    [HarmonyPatch(typeof(PhotonHandler))]
    [HarmonyPatch("OnPlayerLeftRoom", MethodType.Normal)]
    internal class OnPlayerDisconnected
    {
        private static void Prefix(Photon.Realtime.Player otherPlayer)
        {
            if (Events.OnPlayerDisconnectedPre != null)
            {
                PlayerDisConnectedArgs args = new PlayerDisConnectedArgs();
                args.player = otherPlayer;
                object[] obja = { null, args };
                foreach (var del in Events.OnPlayerDisconnectedPre.GetInvocationList())
                {
                    try
                    {
                        del.DynamicInvoke(obja);
                    }
                    catch (Exception e) { BananaHook.Log("OnPlayerDisconnectedPre Exception: " + e.Message + "\n" + e.StackTrace); }
                }
            }
        }
        private static void Postfix(Photon.Realtime.Player otherPlayer)
        {
            if (Events.OnPlayerDisconnectedPost != null)
            {
                PlayerDisConnectedArgs args = new PlayerDisConnectedArgs();
                args.player = otherPlayer;
                object[] obja = { null, args };
                foreach (var del in Events.OnPlayerDisconnectedPost.GetInvocationList())
                {
                    try
                    {
                        del.DynamicInvoke(obja);
                    }
                    catch (Exception e) { BananaHook.Log("OnPlayerDisconnectedPost Exception: " + e.Message + "\n" + e.StackTrace); }
                }
            }
            if (Room.m_eCurrentGamemode != eRoomGamemode.Casual)
            {
                bool isTaggingNow = Room.IsTagging();
                if (Room.m_bIsTagging != isTaggingNow && Events.OnTagOrInfectChange != null)
                {
                    IsTagOrInfectArgs args = new IsTagOrInfectArgs();
                    args.isTagging = isTaggingNow;
                    object[] obja = { null, args };
                    foreach (var del in Events.OnTagOrInfectChange.GetInvocationList())
                    {
                        try
                        {
                            del.DynamicInvoke(obja);
                        }
                        catch (Exception e) { BananaHook.Log("OnTagOrInfectChange Exception: " + e.Message + "\n" + e.StackTrace); }
                    }
                }
                Room.m_bIsTagging = isTaggingNow;
            }
        }
    }
}
