using System;

namespace BananaHook
{
    public class RoomJoinedArgs : EventArgs
    {
        public bool     isPrivate { get; internal set; }
        public string   roomCode { get; internal set; }
    }
    public class PlayerDisConnectedArgs : EventArgs
    {
        public Photon.Realtime.Player player { get; internal set; }
    }
    public class PlayerTaggedPlayerArgs : EventArgs
    {
        public Photon.Realtime.Player tagger { get; internal set; }
        public Photon.Realtime.Player victim { get; internal set; }
        public bool                   isTagging { get; internal set; }
    }
    public class PlayerNicknameArgs : EventArgs
    {
        public string oldNickName { get; internal set; }
        public string newNickName { get; internal set; }
    }
    public class OnRoundStartArgs : EventArgs
    {
        public Photon.Realtime.Player player { get; internal set; }
    }
    public class IsTagOrInfectArgs : EventArgs
    {
        public bool isTagging { get; internal set; }
    }
    public class Events
    {
        public static EventHandler<RoomJoinedArgs>          OnRoomJoined;
        // public static EventHandler<RoomJoinedArgs>          OnRoomJoinedPost;
        public static EventHandler                          OnRoomDisconnected;
        public static EventHandler<PlayerDisConnectedArgs>  OnPlayerConnected;
        public static EventHandler<PlayerDisConnectedArgs>  OnPlayerDisconnectedPre;
        public static EventHandler<PlayerDisConnectedArgs>  OnPlayerDisconnectedPost;
        public static EventHandler<PlayerTaggedPlayerArgs>  OnPlayerTagPlayer;
        public static EventHandler<PlayerTaggedPlayerArgs>  OnLocalPlayerTag;
        public static EventHandler<PlayerNicknameArgs>      OnLocalNicknameChange;
        public static EventHandler                          OnRoundEndPre;
        public static EventHandler                          OnRoundEndPost;
        public static EventHandler<OnRoundStartArgs>        OnRoundStart;
        public static EventHandler<IsTagOrInfectArgs>       OnTagOrInfectChange;
    }
}
