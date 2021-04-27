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
                (api as ICoreClientAPI).Input.SetHotKeyHandler(MultiToolSystem.ToolSwitchHotKeyCode, HandleToolSwitchHotkey);
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

        #endregion

        #region Protected methods

        /// <summary>
        /// Called on the server side after a player switches their multitool.
        /// <para>
        /// <br>Do not forget to call the base method!</br>
        /// <br>Use an if statement checking <see cref="RegistryObject.Class"/> if this overload should run only when the multitool is of this type!</br>
        /// </para>
        /// </summary>
        /// <param name="player">The player with the multitool.</param>
        /// <param name="slot">The slot the multitool is in.</param>
        protected virtual void OnMultiToolSwitchComplete(IServerPlayer player, ItemSlot slot){}


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

            ArgualCoreMod modSystem = api.ModLoader.GetModSystem<ArgualCoreMod>();
            List<ItemStack> stacks = modSystem.MultiToolSystem.GetMultiToolItemStacks();

            dialog = new GuiDialogItemStackSelector(
                Lang.Get(LangKey.SwitchMultiToolDialogTitle),
                stacks.ToArray(),
                (selectedIndex) => SendToolSwitchMessageToServer(slot, stacks, selectedIndex),
                () => { return; },
                capi
            );

            dialog.TryOpen();
        }

        /// <summary>
        /// Called on the client side.
        /// </summary>
        void SendToolSwitchMessageToServer(ItemSlot slot, List<ItemStack> stacks, int index)
        {
            if (slot.Itemstack != null && slot.Itemstack.Item is ItemMultiTool)
            {
                Item item = stacks[index].Item;
                if (slot.Itemstack.Item.Code.Path != item.Code.Path || slot.Itemstack.Item.Code.Domain != item.Code.Domain)
                {
                    var packet = new MultiToolSystem.ToolSwitchMessage()
                    {
                        inventoryId = slot.Inventory.InventoryID,
                        slotId = slot.Inventory.GetSlotId(slot),
                        toolIndex = index
                    };

                    (MultiToolSystem.NetworkChannel as IClientNetworkChannel).SendPacket(packet);
                }
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
                    SwitchToolItemInSlot(sender, slot, msg.toolIndex, PlaySwitchSound);
                }
            }
        }

        /// <summary>
        /// Called on the server side when a multitool needs to be switched.
        /// </summary>
        void SwitchToolItemInSlot(IServerPlayer player, ItemSlot slot, int newToolIndex, bool playSound)
        {
            var coreMod = api.ModLoader.GetModSystem<ArgualCoreMod>();
            var stacks = coreMod.MultiToolSystem.GetMultiToolItemStacks();
            var tool = stacks.ElementAtOrDefault(newToolIndex);
            if (tool != null && tool.Item.Code.ToString() != slot.Itemstack.Item?.Code.ToString())
            {
                ItemStack newItemStack = tool.Clone();

                var attributesToKeep = coreMod.MultiToolSystem.ToolAttributesToKeepOnSwitch;
                foreach (var key in attributesToKeep)
                {
                    if (slot.Itemstack.Attributes.HasAttribute(key))
                    {
                        newItemStack.Attributes[key] = slot.Itemstack.Attributes[key];
                    }
                }

                slot.Itemstack = newItemStack;
                slot.MarkDirty();

                if (playSound)
                {
                    api.World.PlaySoundFor(SwitchSound, player, true);
                }

                OnMultiToolSwitchComplete(player, slot);
            }
        }

        #endregion

    }
}
