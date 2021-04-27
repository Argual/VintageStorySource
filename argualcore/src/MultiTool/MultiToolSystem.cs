using ProtoBuf;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace Argual.ArgualCore.MultiTool
{
    public class MultiToolSystem
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
            /// The index of the tool in the registry of multitools.
            /// </summary>
            public int toolIndex;
        }

        #endregion

        #region Fields

        private readonly List<AssetLocation> toolCodes = new List<AssetLocation>();
        private readonly List<ItemStack> toolItemStacks = new List<ItemStack>();
        private readonly ICoreAPI api;

        /// <summary>
        /// The keys of the attributes which should be kept when switching the multitool <see cref="ItemStack"/>.
        /// </summary>
        protected readonly List<string> toolAttributesToKeepOnSwitch = new List<string>();

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
        public Action<string> LogDebugAction { get; set; }

        /// <summary>
        /// Action called when logging a warning message.
        /// </summary>
        public Action<string> LogWarningAction { get; set; }

        /// <summary>
        /// The channel used to transfer messages about multitool switches between client and server.
        /// </summary>
        public INetworkChannel NetworkChannel { get; private set; }

        /// <summary>
        /// The name of the channel used to transfer messages about multitool switches between client and server.
        /// </summary>
        public virtual string ToolSwitchChannelName { get; private set; }

        /// <summary>
        /// The hotkey code of the hotkey used to switch between multitools.
        /// </summary>
        public virtual string ToolSwitchHotKeyCode { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a multitool system.
        /// </summary>
        /// <param name="api">The game api.</param>
        /// <param name="networkChannelName">Name of the channel used to transfer messages about multitool switches.</param>
        /// <param name="toolSwitchHotKeyCode">The code of the hotkey to register for multitool switches.</param>
        /// <param name="logDebugAction">This action will be called with debug messages.</param>
        /// <param name="logWarningAction">This action will be called with warning messages.</param>
        public MultiToolSystem(ICoreAPI api, string networkChannelName, string toolSwitchHotKeyCode, Action<string> logDebugAction=null, Action<string> logWarningAction = null)
        {
            this.api = api;
            LogDebugAction = logDebugAction;
            LogWarningAction = logWarningAction;

            ToolSwitchChannelName = networkChannelName;
            ToolSwitchHotKeyCode = toolSwitchHotKeyCode;

            // Register hotkey.
            if (api is ICoreClientAPI)
            {
                var capi = api as ICoreClientAPI;

                capi.Input.RegisterHotKey(
                    hotkeyCode: ToolSwitchHotKeyCode,
                    name: Lang.Get(LangKey.SwitchMultiToolHotkey),
                    key: GlKeys.Z,
                    type: HotkeyType.GUIOrOtherControls,
                    shiftPressed: true);
            }

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
        /// <param name="attributesToKeepOnSwitch">The keys of attributes which should be copied from the old <see cref="ItemStack"/> to the new one when switching multitools.</param>
        /// <returns>Whether or not the process was succesful.</returns>
        public bool RegisterMultiTool(string domain, string path, params string[] attributesToKeepOnSwitch)
        {
            return RegisterMultiTool(new AssetLocation(domain, path), attributesToKeepOnSwitch);
        }

        /// <summary>
        /// Registers a multitool.
        /// </summary>
        /// <param name="attributesToKeepOnSwitch">The keys of attributes which should be copied from the old <see cref="ItemStack"/> to the new one when switching multitools.</param>
        /// <returns>Whether or not the process was succesful.</returns>
        public bool RegisterMultiTool(AssetLocation assetLocation, params string[] attributesToKeepOnSwitch)
        {
            if (!assetLocation.Valid)
            {
                LogWarningAction?.Invoke($"Could not register multitool! {nameof(AssetLocation)} is invalid!");
                return false;
            }

            if (toolCodes.Any(c => c.Path == assetLocation.Path && c.Domain == assetLocation.Domain))
            {
                LogWarningAction?.Invoke($"Could not register multitool with code '{assetLocation}', because one with the same code is already registered!");
                return false;
            }

            toolCodes.Add(assetLocation);

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
        /// The <see cref="ItemStack"/>s of the registered <see cref="MultiTool.ItemMultiTool"/>s.
        /// </summary>
        public List<ItemStack> GetMultiToolItemStacks()
        {
            while (toolItemStacks.Count < toolCodes.Count)
            {
                var tool = api.World.GetItem(toolCodes[toolItemStacks.Count]);
                toolItemStacks.Add(new ItemStack(tool));
            }

            return toolItemStacks;
        }

        #endregion
    }

}
