using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace Argual.ArgualCore.MultiTool
{
    public class MultiToolSystem : IDisposable
    {
        #region Subclasses

        /// <summary>
        /// Represents information about a multitool switch.
        /// </summary>
        [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
        public struct ToolSwitchMessage
        {
            /// <summary>
            /// The Id of the inventory the tool is in.
            /// </summary>
            public string inventoryId;

            /// <summary>
            /// The slot Id of the slot the tool is in.
            /// </summary>
            public int slotId;

            /// <summary>
            /// The family of the tool to switch to.
            /// </summary>
            public string toolFamily;
        }

        #endregion

        #region Fields

        private readonly Dictionary<string, List<AssetLocation>> toolCodes = new Dictionary<string, List<AssetLocation>>();
        private readonly Dictionary<string, List<SkillItem>> toolSkillItems = new Dictionary<string, List<SkillItem>>();
        private readonly ICoreAPI api;

        /// <summary>
        /// The key to the attribute tree with information about the multitool.
        /// </summary>
        public static readonly string multiToolAttributeTreeKeyFamilyInfo = ArgualCoreMod.Domain + AssetLocation.LocationSeparator + "multitoolfamilyinfo";

        public static readonly string multiToolAttributeKeyCurrentFamily = "currentfamily";

        /// <summary>
        /// The code of the hotkey used to trigger a multitool switch.
        /// </summary>
        public static readonly string multiToolSwitchHotKeyCode = ArgualCoreMod.Domain + AssetLocation.LocationSeparator + "toolswitch";

        /// <summary>
        /// The keys of the attributes which should be kept when switching the multitool <see cref="ItemStack"/>.
        /// </summary>
        protected readonly List<string> toolAttributesToKeepOnSwitch = new List<string>() { multiToolAttributeTreeKeyFamilyInfo };
        private bool disposedValue;

        #endregion

        #region Properties

        /// <summary>
        /// The keys of the attributes which should be kept when switching the multitool <see cref="ItemStack"/>.
        /// </summary>
        public IEnumerable<string> ToolAttributesToKeepOnSwitch
        {
            get
            {
                foreach (var key in toolAttributesToKeepOnSwitch)
                {
                    yield return key;
                }
            }
        }

        /// <summary>
        /// Action called when logging a debug message.
        /// </summary>
        public Vintagestory.API.Common.Action<string> LogDebugAction { get; set; }

        /// <summary>
        /// Action called when logging a warning message.
        /// </summary>
        public Vintagestory.API.Common.Action<string> LogWarningAction { get; set; }

        /// <summary>
        /// The channel used to transfer messages about multitool switches between client and server.
        /// </summary>
        public INetworkChannel NetworkChannel { get; private set; }

        /// <summary>
        /// The name of the channel used to transfer messages about multitool switches between client and server.
        /// </summary>
        public virtual string ToolSwitchChannelName { get; private set; }

        /// <summary>
        /// The asset locations of the registered tools grouped by tool family.
        /// </summary>
        public Dictionary<string, List<AssetLocation>> ToolCodes { get => toolCodes; }

        /// <summary>
        /// The registered tool families.
        /// </summary>
        public IEnumerable<string> ToolFamilies
        {
            get
            {
                foreach (var key in ToolCodes.Keys)
                {
                    yield return key;
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a multitool system.
        /// </summary>
        /// <param name="api">The game api.</param>
        /// <param name="networkChannelName">Name of the channel used to transfer messages about multitool switches.</param>
        /// <param name="logDebugAction">This action will be called with debug messages.</param>
        /// <param name="logWarningAction">This action will be called with warning messages.</param>
        public MultiToolSystem(ICoreAPI api, string networkChannelName, Vintagestory.API.Common.Action<string> logDebugAction =null, Vintagestory.API.Common.Action<string> logWarningAction = null)
        {
            this.api = api;
            LogDebugAction = logDebugAction;
            LogWarningAction = logWarningAction;

            ToolSwitchChannelName = networkChannelName;

            // Register channel.
            NetworkChannel = api.Network.RegisterChannel(ToolSwitchChannelName);
            NetworkChannel.RegisterMessageType(typeof(ToolSwitchMessage));
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Adds an attribute key to the list of keys of attributes to be kept on multitool switch.
        /// </summary>
        public void AddWatchedAttributeKey(string key)
        {
            if (toolAttributesToKeepOnSwitch.Contains(key))
            {
                LogWarningAction($"Attribute with key '{key}' is already on the list of watched attributes! Make sure it is not conflicting with another mod's key!");
                return;
            }
            LogDebugAction?.Invoke($"Registered watched attribute key: '{key}'.");
            toolAttributesToKeepOnSwitch.Add(key);
        }

        /// <summary>
        /// Removes an attribute key from the list of keys of attributes to be kept on multitool switch.
        /// </summary>
        public bool RemoveWatchedAttributeKey(string key)
        {
            if (toolAttributesToKeepOnSwitch.Remove(key))
            {
                LogDebugAction?.Invoke($"Removed attribute key '{key}' from list of watched attributes.");
                return true;
            }
            else
            {
                LogWarningAction?.Invoke($"Could not remove attribute key '{key}'. It was not on the list of watched attributes.");
                return false;
            }
        }

        /// <summary>
        /// Registers a multitool.
        /// </summary>
        /// <param name="assetLocation">The asset location of the multitool to be registered.</param>
        /// <param name="attributesToKeepOnSwitch">The keys of attributes which should be copied from the old <see cref="ItemStack"/> to the new one when switching multitools.</param>
        /// <returns>Whether or not the process was succesful.</returns>
        public bool RegisterMultiTool(AssetLocation assetLocation, params string[] attributesToKeepOnSwitch)
        {
            return RegisterMultiTool(assetLocation.ToString(), assetLocation, attributesToKeepOnSwitch);
        }

        /// <summary>
        /// Registers a multitool.
        /// </summary>
        /// <param name="toolFamily">Multitools can be grouped by tool family. This allows a tool to be upgraded and/or customized without bloat in the multitool selection menu.</param>
        /// <param name="assetLocation">The asset location of the multitool to be registered.</param>
        /// <param name="attributesToKeepOnSwitch">The keys of attributes which should be copied from the old <see cref="ItemStack"/> to the new one when switching multitools.</param>
        /// <returns>Whether or not the process was succesful.</returns>
        public bool RegisterMultiTool(string toolFamily, AssetLocation assetLocation, params string[] attributesToKeepOnSwitch)
        {
            if (!assetLocation.Valid)
            {
                LogWarningAction?.Invoke($"Could not register multitool! {nameof(AssetLocation)} is invalid!");
                return false;
            }

            if (toolCodes.Values.Any(l=>l.Any(c => c.Path == assetLocation.Path && c.Domain == assetLocation.Domain)))
            {
                LogWarningAction?.Invoke($"Could not register multitool with code '{assetLocation}', because one with the same code is already registered!");
                return false;
            }

            if (!toolCodes.ContainsKey(toolFamily))
            {
                toolCodes[toolFamily] = new List<AssetLocation>();
            }

            toolCodes[toolFamily].Add(assetLocation);

            LogDebugAction?.Invoke($"Registered multitool: {assetLocation}.");

            foreach (var key in attributesToKeepOnSwitch)
            {
                if (!toolAttributesToKeepOnSwitch.Contains(key))
                {
                    AddWatchedAttributeKey(key);
                }
            }

            return true;
        }

        /// <summary>
        /// Tries to find the family of the given tool and returns whether or not it was found.
        /// </summary>
        public bool TryFindMultiToolFamily(AssetLocation toolLocation, out string family)
        {
            family = default;

            foreach (var key in ToolCodes.Keys)
            {
                foreach (var code in ToolCodes[key])
                {
                    if (code.ToString() == toolLocation.ToString())
                    {
                        family = key;
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion

        #region Obsolete

        /// <summary>
        /// Registers a multitool.
        /// <para>
        /// This is obsolete, and will register every multitool into its own individual tool family.
        /// <br>Use <see cref="RegisterMultiTool(string, AssetLocation, string[])"/> instead!</br>
        /// </para>
        /// </summary>
        /// <param name="attributesToKeepOnSwitch">The keys of attributes which should be copied from the old <see cref="ItemStack"/> to the new one when switching multitools.</param>
        /// <returns>Whether or not the process was succesful.</returns>
        [System.Obsolete]
        public bool RegisterMultiTool(string domain, string path, params string[] attributesToKeepOnSwitch)
        {
            return RegisterMultiTool(domain+AssetLocation.LocationSeparator+path, new AssetLocation(domain, path), attributesToKeepOnSwitch);
        }

        /// <summary>
        /// The <see cref="ItemStack"/>s of the registered <see cref="MultiTool.ItemMultiTool"/>s.
        /// </summary>
        [System.Obsolete("", true)]
        public List<ItemStack> GetMultiToolItemStacks()
        {
            throw new System.NotImplementedException();
        }


        #endregion

        #region Disposing

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var l in toolSkillItems.Values)
                    {
                        for (int i = 0; i < l.Count; i++)
                        {
                            l[i].Dispose();
                        }
                        l.Clear();
                    }
                    toolSkillItems.Clear();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~MultiToolSystem()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }

}
