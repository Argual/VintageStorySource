using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace Argual.ArgualCore
{
    public partial class ArgualCoreMod : ModSystem
    {

        #region Constants

        public const string Domain = "argualcore";

        public const string FileNameConfig = Domain + ".cfg";

        #endregion

        #region Private fields

        ICoreAPI api;

        #endregion

        #region Properties

        public MultiTool.MultiToolSystem MultiToolSystem { get; private set; }

        #endregion

        #region Public methods

        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            this.api = api;

            SetupMultiToolSystem(this.api);
        }


        #endregion

        #region Private methods

        private void SetupMultiToolSystem(ICoreAPI api)
        {
            api.RegisterItemClass(MultiTool.LangKey.MultiToolDefault, typeof(MultiTool.ItemMultiTool));

            MultiToolSystem = new MultiTool.MultiToolSystem(api, Domain + AssetLocation.LocationSeparator + "toolswitchchannel",  LogDebug, LogWarning);

            AssetLocation multiToolDefault = new AssetLocation(Domain, MultiTool.LangKey.MultiToolDefault);

            MultiToolSystem.RegisterMultiTool(multiToolDefault.ToString(), multiToolDefault);

            
            // Register hotkey.
            if (api is ICoreClientAPI)
            {
                var capi = api as ICoreClientAPI;

                capi.Input.RegisterHotKey(
                    hotkeyCode: MultiTool.MultiToolSystem.multiToolSwitchHotKeyCode,
                    name: Lang.Get(MultiTool.LangKey.SwitchMultiToolHotkey),
                    key: GlKeys.Z,
                    type: HotkeyType.GUIOrOtherControls,
                    shiftPressed: true);
            }
        }

        private void LogDebug(string message)
        {
            api?.Logger.Debug($"[{Domain}] " + message);
        }

        private void LogWarning(string message)
        {
            api?.Logger.Warning($"[{Domain}] " + message);
        }

        #endregion

        #region Obsoletes

        /// <summary>
        /// Registers a multitool.
        /// </summary>
        /// <param name="domain"/>
        /// <param name="path"/>
        /// <param name="attributesToKeepOnSwitch">The keys of attributes which should be copied from the old <see cref="ItemStack"/> to the new one when switching multitools.</param>
        /// <returns>Whether or not the process was succesful.</returns>
        /// <remarks>
        /// This method is obsolete! Call <see cref="MultiTool.MultiToolSystem.RegisterMultiTool(string, string, string[])"/> instead from <see cref="MultiToolSystem"/>.
        /// </remarks>
        [Obsolete("Call "+nameof(MultiToolSystem)+"."+nameof(MultiTool.MultiToolSystem.RegisterMultiTool)+" instead.")]
        public bool RegisterMultiTool(string domain, string path, params string[] attributesToKeepOnSwitch)
        {
            return MultiToolSystem.RegisterMultiTool(domain, path, attributesToKeepOnSwitch);
        }

        /// <summary>
        /// Registers a multitool.
        /// </summary>
        /// <param name="assetLocation"/>
        /// <param name="attributesToKeepOnSwitch">The keys of attributes which should be copied from the old <see cref="ItemStack"/> to the new one when switching multitools.</param>
        /// <returns>Whether or not the process was succesful.</returns>
        /// <remarks>
        /// This method is obsolete! Call <see cref="MultiTool.MultiToolSystem.RegisterMultiTool(string, AssetLocation, string[])"/> instead from <see cref="MultiToolSystem"/>.
        /// </remarks>
        [Obsolete]
        public bool RegisterMultiTool(AssetLocation assetLocation, params string[] attributesToKeepOnSwitch)
        {
            return MultiToolSystem.RegisterMultiTool(assetLocation.ToString(), assetLocation, attributesToKeepOnSwitch);
        }

        /// <summary>
        /// The <see cref="ItemStack"/>s of the registered <see cref="MultiTool.ItemMultiTool"/>s.
        /// </summary>
        /// <remarks>
        /// This method is obsolete! Call <see cref="MultiTool.MultiToolSystem.GetMultiToolItemStacks"/> instead from <see cref="MultiToolSystem"/>.
        /// </remarks>
        [Obsolete]
        public List<ItemStack> GetMultiToolItemStacks()
        {
            return MultiToolSystem.GetMultiToolItemStacks();
        }

        #endregion

    }
}
