using BepInEx;
﻿using BananaHook.HookAndPatch;
using Photon.Pun;

namespace BananaHook
{
    /* That's me! */
    [BepInPlugin("net.rusjj.gtlib.bananahook", "BananaHook Lib", "1.3.0")]

    public class BananaHook : BaseUnityPlugin
    {
        private static BananaHook m_hInstance;
        internal static void Log(string msg) => m_hInstance.Logger.LogMessage(msg);

        void Update()
        {
            if (PhotonNetwork.IsConnectedAndReady && !Patch.IsPatched())
            {
                m_hInstance = this;
                Patch.Apply();
                new HookAndPatch.EventListener();
        }
    }
}
