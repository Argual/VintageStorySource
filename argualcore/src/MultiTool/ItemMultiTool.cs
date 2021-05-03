using Argual.ArgualCore.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace Argual.ArgualCore.MultiTool
{
    public class ItemMultiTool : Item
    {
        #region Fields

        private GuiDialog dialog;

        /// <summary>
        /// The multitool system this item is registered to.
        /// </summary>
        public virtual MultiToolSystem MultiToolSystem { get => api.ModLoader.GetModSystem<ArgualCoreMod>().MultiToolSystem; }

        /// <summary>
        /// The location of the sound to be played when switching tool.
        /// </summary>
        public virtual AssetLocation SwitchSound { get; set; } = new AssetLocation("game", "sounds/toggleswitch");

        /// <summary>
        /// Whether or not to play a sound when switching tool.
        /// </summary>
        public virtual bool PlaySwitchSound { get; set; } = true;

        #endregion

        #region Public methods

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            if (api is ICoreServerAPI)
            {
                (MultiToolSystem.NetworkChannel as IServerNetworkChannel).SetMessageHandler<MultiToolSystem.ToolSwitchMessage>(OnToolSwitchMessageReceivedFromClient);
            }
            else if (api is ICoreClientAPI)
            {
                (api as ICoreClientAPI).Input.SetHotKeyHandler(MultiToolSystem.multiToolSwitchHotKeyCode, HandleToolSwitchHotkey);
            }
        }

        public override void OnUnloaded(ICoreAPI api)
        {
            if (dialog != null)
            {
                dialog.TryClose();
                dialog.Dispose();
            }

            base.OnUnloaded(api);
        }

        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
        {
            ArgualCoreMod modSystem = api.ModLoader.GetModSystem<ArgualCoreMod>();
            return new WorldInteraction[] {
                new WorldInteraction()
                {
                    ActionLangCode = Lang.Get(LangKey.SwitchMultiToolHeldHelp, Lang.Get(LangKey.SwitchMultiToolHotkey)),
                    HotKeyCode = LangKey.SwitchMultiTool,
                    MouseButton = EnumMouseButton.None
                }
            }.Append(base.GetHeldInteractionHelp(inSlot));
        }

        /// <summary>
        /// <inheritdoc/>
        /// <para>Copies the watched attributes from multitool ingredients to the output multitool.</para>
        /// </summary>
        public override void OnCreatedByCrafting(ItemSlot[] allInputslots, ItemSlot outputSlot, GridRecipe byRecipe)
        {
            base.OnCreatedByCrafting(allInputslots, outputSlot, byRecipe);

            foreach (var inSlot in allInputslots)
            {
                if (!inSlot.Empty && inSlot.Itemstack.Item is ItemMultiTool)
                {
                    CopyAttributes(inSlot.Itemstack, outputSlot.Itemstack);
                }
            }

            if (MultiToolSystem.TryFindMultiToolFamily(outputSlot.Itemstack.Item.Code, out string family))
            {
                var tree = outputSlot.Itemstack.Attributes.GetOrAddTreeAttribute(MultiToolSystem.multiToolAttributeTreeKeyFamilyInfo);
                tree.SetString(MultiToolSystem.multiToolAttributeKeyCurrentFamily, family);
                tree.SetString(family, outputSlot.Itemstack.Item.Code.ToString());
            }
        }

        #endregion

        #region Protected methods
        
        /// <summary>
        /// Called server side after a succesful multitool switch.
        /// </summary>
        /// <param name="player">The player with the multitool.</param>
        /// <param name="slot">The slot with the multitool.</param>
        protected virtual void OnMultiToolSwitchComplete(IServerPlayer player, ItemSlot slot) { }

        #endregion

        #region Private methods

        bool HandleToolSwitchHotkey(KeyCombination keys)
        {
            var activeSlot = (api as ICoreClientAPI).World.Player.InventoryManager.ActiveHotbarSlot;
            if (activeSlot != null)
            {
                if (activeSlot.Itemstack != null && activeSlot.Itemstack.Item != null && activeSlot.Itemstack.Item is ItemMultiTool)
                {
                    OpenDialog(activeSlot);
                }
            }

            return true;
        }

        void OpenDialog(ItemSlot slot)
        {
            var capi = (ICoreClientAPI)api;

            if (dialog != null)
            {
                dialog.Dispose();
            }

            string dialogTitle = Lang.Get(LangKey.SwitchMultiToolDialogTitle);

            List<AssetLocation> toolAssets = new List<AssetLocation>();
            foreach (var family in MultiToolSystem.ToolCodes.Keys)
            {
                var tree = slot.Itemstack.Attributes.GetOrAddTreeAttribute(MultiToolSystem.multiToolAttributeTreeKeyFamilyInfo);
                var code = tree.GetString(family, MultiToolSystem.ToolCodes[family].First().ToString());
                toolAssets.Add(new AssetLocation(code));
            }

            var tools = toolAssets.Select(a => api.World.GetItem(a));

            dialog = new DialogSelectCollectible(
                dialogTitle,
                tools,
                collectible => {
                    string key = Language.LangTool.GetItemDescKey(collectible.Code.Domain, collectible.Code.Path);
                    string desc = Lang.Get(key);
                    if (key == desc)
                    {
                        desc = null;
                    }
                    return GUITool.CreateSkillItemFromCollectible(capi, collectible, null, desc);
                },
                (selectedIndex) => SendToolSwitchMessageToServer(slot, selectedIndex),
                () => { return; },
                capi,
                showDescription: false);

            dialog.TryOpen();
        }

        /// <summary>
        /// Called on the client side.
        /// </summary>
        void SendToolSwitchMessageToServer(ItemSlot slot, int index)
        {
            if (slot.Itemstack != null && slot.Itemstack.Item is ItemMultiTool)
            {
                var packet = new MultiToolSystem.ToolSwitchMessage()
                {
                    inventoryId = slot.Inventory.InventoryID,
                    slotId = slot.Inventory.GetSlotId(slot),
                    toolFamily = MultiToolSystem.ToolCodes.Keys.ToList()[index],
                };

                (MultiToolSystem.NetworkChannel as IClientNetworkChannel).SendPacket(packet);
            }
        }

        /// <summary>
        /// Called on the server side.
        /// </summary>
        void OnToolSwitchMessageReceivedFromClient(IServerPlayer sender, MultiToolSystem.ToolSwitchMessage msg)
        {
            var inventory = sender.InventoryManager.GetInventory(msg.inventoryId);
            if (inventory != null)
            {
                ItemSlot slot = inventory.ElementAtOrDefault(msg.slotId);
                if (slot != null)
                {
                    SwitchToolItemInSlot(sender, slot, msg.toolFamily, PlaySwitchSound);
                }
            }
        }

        /// <summary>
        /// Called on the server side when a multitool needs to be switched.
        /// </summary>
        void SwitchToolItemInSlot(IServerPlayer player, ItemSlot slot, string newToolFamily, bool playSound)
        {
            var famTree = slot.Itemstack.Attributes.GetOrAddTreeAttribute(MultiToolSystem.multiToolAttributeTreeKeyFamilyInfo);
            var newCode = famTree.GetString(newToolFamily, MultiToolSystem.ToolCodes[newToolFamily].First().ToString());

            var toolStack = new ItemStack(api.World.GetItem(new AssetLocation(newCode)));
            famTree.SetString(MultiToolSystem.multiToolAttributeKeyCurrentFamily, newCode);
            famTree.SetString(newToolFamily, newCode);

            if (toolStack != null && toolStack.Item.Code.ToString() != slot.Itemstack.Item?.Code.ToString())
            {
                ItemStack newItemStack = toolStack.Clone();

                CopyAttributes(slot.Itemstack, newItemStack);

                slot.Itemstack = newItemStack;
                slot.MarkDirty();

                if (playSound)
                {
                    api.World.PlaySoundFor(SwitchSound, player, true);
                }

                (slot.Itemstack.Item as ItemMultiTool).OnMultiToolSwitchComplete(player, slot);
            }
        }

        void CopyAttributes(ItemStack stackFrom, ItemStack stackTo)
        {
            var attributesToKeep = MultiToolSystem.ToolAttributesToKeepOnSwitch;
            foreach (var key in attributesToKeep)
            {
                if (stackFrom.Attributes.HasAttribute(key))
                {
                    stackTo.Attributes[key] = stackFrom.Attributes[key];
                }
            }
        }

        #endregion

    }
}
