﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using HarmonyLib;

namespace RepairStation
{
    [HarmonyPatch(typeof(ZNet), nameof(ZNet.OnNewConnection))]
    public static class RegisterAndCheckVersion
    {
        private static void Prefix(ZNetPeer peer, ref ZNet __instance)
        {
            // Register version check call
            RepairStationPlugin.RepairStationLogger.LogDebug("Registering version RPC handler");
            peer.m_rpc.Register($"{RepairStationPlugin.ModName}_VersionCheck", new Action<ZRpc, ZPackage>(RpcHandlers.RPC_AllManagersModTemplate_Version));

            // Make calls to check versions
            RepairStationPlugin.RepairStationLogger.LogDebug("Invoking version check");
            ZPackage zpackage = new();
            zpackage.Write(RepairStationPlugin.ModVersion);
            zpackage.Write(RpcHandlers.ComputeHashForMod().Replace("-", ""));
            peer.m_rpc.Invoke($"{RepairStationPlugin.ModName}_VersionCheck", zpackage);
        }
    }

    [HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_PeerInfo))]
    public static class VerifyClient
    {
        private static bool Prefix(ZRpc rpc, ZPackage pkg, ref ZNet __instance)
        {
            if (!__instance.IsServer() || RpcHandlers.ValidatedPeers.Contains(rpc)) return true;
            // Disconnect peer if they didn't send mod version at all
            RepairStationPlugin.RepairStationLogger.LogWarning($"Peer ({rpc.m_socket.GetHostName()}) never sent version or couldn't due to previous disconnect, disconnecting");
            rpc.Invoke("Error", 3);
            return false; // Prevent calling underlying method
        }

        private static void Postfix(ZNet __instance)
        {
            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), $"{RepairStationPlugin.ModName}RequestAdminSync",
                new ZPackage());
        }
    }

    [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.ShowConnectError))]
    public class ShowConnectionError
    {
        private static void Postfix(FejdStartup __instance)
        {
            if (__instance.m_connectionFailedPanel.activeSelf)
            {
                __instance.m_connectionFailedError.fontSizeMax = 25;
                __instance.m_connectionFailedError.fontSizeMin = 15;
                __instance.m_connectionFailedError.text += "\n" + RepairStationPlugin.ConnectionError;
            }
        }
    }

    [HarmonyPatch(typeof(ZNet), nameof(ZNet.Disconnect))]
    public static class RemoveDisconnectedPeerFromVerified
    {
        private static void Prefix(ZNetPeer peer, ref ZNet __instance)
        {
            if (!__instance.IsServer()) return;
            // Remove peer from validated list
            RepairStationPlugin.RepairStationLogger.LogInfo(
                $"Peer ({peer.m_rpc.m_socket.GetHostName()}) disconnected, removing from validated list");
            _ = RpcHandlers.ValidatedPeers.Remove(peer.m_rpc);
        }
    }

    public static class RpcHandlers
    {
        public static readonly List<ZRpc> ValidatedPeers = new();

        public static void RPC_AllManagersModTemplate_Version(ZRpc rpc, ZPackage pkg)
        {
            string? version = pkg.ReadString();
            string? hash = pkg.ReadString();

            var hashForAssembly = ComputeHashForMod().Replace("-", "");
            RepairStationPlugin.RepairStationLogger.LogInfo("Version check, local: " +
                                                            RepairStationPlugin.ModVersion +
                                                            ",  remote: " + version);
            if (hash != hashForAssembly || version != RepairStationPlugin.ModVersion)
            {
                RepairStationPlugin.ConnectionError = $"{RepairStationPlugin.ModName} Installed: {RepairStationPlugin.ModVersion} {hashForAssembly}\n Needed: {version} {hash}";
                if (!ZNet.instance.IsServer()) return;
                // Different versions - force disconnect client from server
                RepairStationPlugin.RepairStationLogger.LogWarning($"Peer ({rpc.m_socket.GetHostName()}) has incompatible version, disconnecting...");
                rpc.Invoke("Error", 3);
            }
            else
            {
                if (!ZNet.instance.IsServer())
                {
                    // Enable mod on client if versions match
                    RepairStationPlugin.RepairStationLogger.LogInfo(
                        "Received same version from server!");
                }
                else
                {
                    // Add client to validated list
                    RepairStationPlugin.RepairStationLogger.LogInfo(
                        $"Adding peer ({rpc.m_socket.GetHostName()}) to validated list");
                    ValidatedPeers.Add(rpc);
                }
            }
        }

        public static string ComputeHashForMod()
        {
            using SHA256 sha256Hash = SHA256.Create();
            // ComputeHash - returns byte array  
            byte[] bytes = sha256Hash.ComputeHash(File.ReadAllBytes(Assembly.GetExecutingAssembly().Location));
            // Convert byte array to a string   
            StringBuilder builder = new();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("X2"));
            }

            return builder.ToString();
        }
    }
}