using System.Collections.Generic;
using Photon.Realtime;
using Photon.Pun;

namespace BananaHook.Utils
{
    public class Players
    {
        /* Valid players - completed a tutorial */
        public static int CountValidPlayers()
        {
            int nCount = 0;
            object bDidTutorial;
            foreach (var p in PhotonNetwork.PlayerList)
            {
                if(p.CustomProperties.TryGetValue("didTutorial", out bDidTutorial) && (bool)bDidTutorial) ++nCount;
            }
            return nCount;
        }
        public static VRRig GetVRRigOfPlayer(Player player)
        {
            VRRig vrrig;
            if (GorillaParent.instance.vrrigDict.TryGetValue(player, out vrrig)) return vrrig;
            if (GorillaGameManager.instance.playerVRRigDict.TryGetValue(player.ActorNumber, out vrrig)) return vrrig;
            foreach (VRRig vrrig2 in GorillaParent.instance.vrrigs)
            {
                if (vrrig2.myPlayer == player) return vrrig2;
            }
            return null;
        }
        public static Player FindPlayerOfVRRig(VRRig vrrig) => (vrrig != null  && vrrig.creator != null) ? vrrig.creator : (Player)null;
        public static Player GetTargetOf(Player p)
        {
            if (Room.m_eCurrentGamemode != eRoomGamemode.Hunt || p == null) return null;
            return GorillaGameManager.instance.GetComponent<GorillaHuntManager>().GetTargetOf(p);
        }
        public static Player GetTargetOfWho(Player testingPlayer)
        {
            if (Room.m_eCurrentGamemode != eRoomGamemode.Hunt || testingPlayer == null) return null;
            GorillaHuntManager man = GorillaGameManager.instance.GetComponent<GorillaHuntManager>();
            foreach (var p in PhotonNetwork.PlayerList)
            {
                if (man.GetTargetOf(p) == testingPlayer) return p;
            }
            return null;
        }
        // Reversed GorillaTagManager::CurrentInfectionPlayers() (before i downloaded a decompiler, lmao)
        // P.S. Returning a List instead of Array[]. We dont really need copying.
        public static List<Player> CurrentValidPlayers()
        {
            var pArray = new List<Player>();
            object bDidTutorial;
            foreach (var p in PhotonNetwork.PlayerList)
            {
                if(p.CustomProperties.TryGetValue("didTutorial", out bDidTutorial) && (bool)bDidTutorial) pArray.Add(p);
            }
            return pArray;
        }
        public static int CountInfectedPlayers()
        {
            int nCount = 0;
            VRRig prig;
            foreach (var p in PhotonNetwork.PlayerList)
            {
                prig = Players.GetVRRigOfPlayer(p);
                if (prig != null && prig.currentMatIndex != 0) ++nCount;
            }
            return nCount;
        }
        public static List<Player> CurrentInfectedPlayers()
        {
            var pArray = new List<Player>();
            VRRig prig;
            foreach (var p in PhotonNetwork.PlayerList)
            {
                prig = Players.GetVRRigOfPlayer(p);
                if (prig != null && prig.currentMatIndex != 0) pArray.Add(p);
            }
            return pArray;
        }
        /* Just a first guy in a loop that is infected */
        public static Player GetFirstGuyInfected()
        {
            VRRig prig;
            foreach (var p in PhotonNetwork.PlayerList)
            {
                prig = Players.GetVRRigOfPlayer(p);
                if (prig != null && prig.currentMatIndex != 0) return p;
            }
            return null;
        }
        public static bool IsInfected(Player playerToCheck)
        {
            VRRig prig = Players.GetVRRigOfPlayer(playerToCheck);
            return (prig != null && prig.currentMatIndex == 2);
        }
        public static bool IsTagger(Player playerToCheck)
        {
            VRRig prig = Players.GetVRRigOfPlayer(playerToCheck);
            return (prig != null && prig.currentMatIndex == 1);
        }
        public static bool IsHunted(Player playerToCheck)
        {
            VRRig prig = Players.GetVRRigOfPlayer(playerToCheck);
            return (prig != null && prig.currentMatIndex == 3);
        }
        public static Player GetCurrentIt()
        {
            if (!Room.IsInRoom()) return null;
            return Room.m_hCurrentIt;
        }
    }
}
