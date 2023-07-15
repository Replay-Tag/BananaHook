using Photon.Pun;
using Photon.Realtime;
using System;

namespace BananaHook.Utils
{
    public enum eRoomQueue : byte
    {
        Custom = 0,
        Default = 1,
        Competitive = 2,
    };
    public enum eRoomGamemode : byte
    {
        Custom = 0,
        Casual = 1,
        Infection = 2,
        Hunt = 3,
    };
    public enum eJoinedMap : byte
    {
        Unknown = 0,
        GorillaShop = 1,
        Forest = 2,
        Cave = 3,
        Canyon = 4,
        Mountain = 5,
        SkyJungle = 6,
        Basement = 7,
        Beachcove = 8
    };
    public class Room
    {
        public static bool m_bModdedLobby { get; internal set; } = false;
        public static eRoomQueue m_eCurrentLobbyMode { get; internal set; } = eRoomQueue.Custom;
        public static eRoomGamemode m_eCurrentGamemode { get; internal set; } = eRoomGamemode.Custom;
        public static eJoinedMap m_eTriggeredMap { get; internal set; } = eJoinedMap.Unknown;
        public static string m_szRoomCode { get; internal set; } = null; // Just a room code
        public static bool m_bIsGameEnded { get; internal set; } = false; // True - the game is in process of ending
        public static bool m_bIsTagging { get; internal set; } = true; // True - Rock-Monk, False - Something else!
        public static bool m_bIsHunting { get; internal set; } = false; // True - Water-Monk, False - Something else!
        public static Player m_hCurrentIt { get; internal set; } = null;

        public static bool IsInRoom() => PhotonNetwork.InRoom;
        public static string GetRoomCode() => PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.Name : null;
        public static int GetMaxPlayers() => PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.MaxPlayers : 1;
        public static int GetPlayers() => PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.PlayerCount : 1;
        public static bool IsTagging()
        {
            return (PhotonNetwork.InRoom && (m_eCurrentLobbyMode == eRoomQueue.Default || m_eCurrentLobbyMode == eRoomQueue.Competitive) && Photon.Pun.PhotonNetwork.PlayerList.Length < 4);
        }
        public static bool IsInfecting()
        {
            return !IsTagging();
        }
        public static bool IsHunting()
        {
            return (PhotonNetwork.InRoom && m_eCurrentGamemode == eRoomGamemode.Hunt);
        }

        internal static int m_nTagged = 0, m_nTotal = 0;
        /* Should work. Be aware of glitch "the game is ended but 1 guy is untagged" */
        internal static void CheckForTheGameEndPre()
        {
            if (m_bIsTagging || (m_nTagged = Players.CountInfectedPlayers()) < 4) return;
            if ((m_nTotal = Players.CountValidPlayers()) > 0 && m_nTagged == m_nTotal)
            {
                m_bIsGameEnded = true;
                try
                { 
                    Events.OnRoundEndPre?.Invoke(null, null);
                }
                catch (Exception e) { BananaHook.Log("OnRoundEndPre Exception: " + e.Message + "\n" + e.StackTrace); }
            }
        }
        internal static void CheckForTheGameEndPost()
        {
            if (m_bIsTagging || (m_nTagged = Players.CountInfectedPlayers()) < 4) return;
            if (m_bIsGameEnded && m_nTotal > 0 && m_nTagged == m_nTotal)
            {
                try
                { 
                    Events.OnRoundEndPost?.Invoke(null, null);
                }
                catch (Exception e) { BananaHook.Log("OnRoundEndPost Exception: " + e.Message + "\n" + e.StackTrace); }
            }
        }
    }
}
