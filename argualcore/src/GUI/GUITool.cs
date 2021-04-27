using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Argual.ArgualCore.Language;
using Vintagestory.API.MathTools;

namespace Argual.ArgualCore.GUI

{
    /// <summary>
    /// Provides a variety of functions to create and manipulate GUI elements.
    /// </summary>
    public static class GUITool
    {
        #region Skill Item Creation

        #region From ItemStack

        /// <summary>
        /// Creates a <see cref="SkillItem"/> using the provided parameters.
        /// </summary>
        /// <param name="capi">The client which will be using the skill item.</param>
        /// <param name="stack">The stack to use as template.</param>
        /// <returns></returns>
        public static SkillItem CreateSkillItemFromItemStack(ICoreClientAPI capi, ItemStack stack)
        {
            return CreateSkillItemFromItemStack(capi, stack, null, "", ColorUtil.WhiteArgb);
        }

        /// <summary>
        /// Creates a <see cref="SkillItem"/> using the provided parameters.
        /// </summary>
        /// <param name="capi">The client which will be using the skill item.</param>
        /// <param name="stack">The stack to use as template.</param>
        /// <param name="name">What to name the skill item. <para>If empty or null, the default name of the item will be used.</para></param>
        /// <returns></returns>
        public static SkillItem CreateSkillItemFromItemStack(ICoreClientAPI capi, ItemStack stack, string name)
        {
            return CreateSkillItemFromItemStack(capi, stack, name, "", ColorUtil.WhiteArgb);
        }

        /// <summary>
        /// Creates a <see cref="SkillItem"/> using the provided parameters.
        /// </summary>
        /// <param name="capi">The client which will be using the skill item.</param>
        /// <param name="stack">The stack to use as template.</param>
        /// <param name="name">What to name the skill item. <para>If empty or null, the default name of the item will be used.</para></param>
        /// <param name="description">The description of the skill item. <para>If null, an empty string will be used.</para></param>
        /// <returns></returns>
        public static SkillItem CreateSkillItemFromItemStack(ICoreClientAPI capi, ItemStack stack, string name, string description)
        {
            return CreateSkillItemFromItemStack(capi, stack, name, description, ColorUtil.WhiteArgb);
        }

        /// <summary>
        /// Creates a <see cref="SkillItem"/> using the provided parameters.
        /// </summary>
        /// <param name="capi">The client which will be using the skill item.</param>
        /// <param name="stack">The stack to use as template.</param>
        /// <param name="name">What to name the skill item. <para>If empty or null, the default name of the item will be used.</para></param>
        /// <param name="description">The description of the skill item. <para>If null, an empty string will be used.</para></param>
        /// <param name="color">The color used to colorize the skill item icon.</param>
        /// <returns></returns>
        public static SkillItem CreateSkillItemFromItemStack(ICoreClientAPI capi, ItemStack stack, string name, string description, int color)
        {

            ItemSlot dummySlot = new DummySlot(stack);
            double size = GuiElementPassiveItemSlot.unscaledSlotSize + GuiElementItemSlotGrid.unscaledSlotPadding;

            if (string.IsNullOrWhiteSpace(name))
            {
                name = stack.GetName();
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                description = "";
            }

            return new SkillItem()
            {
                Code = stack.Item.Code.Clone(),
                Name = name,
                Description = description,
                RenderHandler = (AssetLocation code, float dt, double posX, double posY) => {
                    // No idea why the weird offset and size multiplier
                    double scsize = GuiElement.scaled(size - 5);

                    capi.Render.RenderItemstackToGui(dummySlot, posX + scsize / 2, posY + scsize / 2, 100, (float)GuiElement.scaled(GuiElementPassiveItemSlot.unscaledItemSize), color);
                }
            };
        }


        #endregion

        #region From CollectibleObject

        /// <summary>
        /// Creates a <see cref="SkillItem"/> using the provided parameters.
        /// </summary>
        /// <param name="capi">The client which will be using the skill item.</param>
        /// <param name="collectible">The collectible to use as template.</param>
        /// <returns></returns>
        public static SkillItem CreateSkillItemFromCollectible(ICoreClientAPI capi, CollectibleObject collectible)
        {
            return CreateSkillItemFromCollectible(capi, collectible, null, "", ColorUtil.WhiteArgb);
        }

        /// <summary>
        /// Creates a <see cref="SkillItem"/> using the provided parameters.
        /// </summary>
        /// <param name="capi">The client which will be using the skill item.</param>
        /// <param name="collectible">The collectible to use as template.</param>
        /// <param name="name">What to name the skill item. <para>If empty or null, the default name of the item will be used.</para></param>
        /// <returns></returns>
        public static SkillItem CreateSkillItemFromCollectible(ICoreClientAPI capi, CollectibleObject collectible, string name)
        {
            return CreateSkillItemFromCollectible(capi, collectible, name, "", ColorUtil.WhiteArgb);
        }

        /// <summary>
        /// Creates a <see cref="SkillItem"/> using the provided parameters.
        /// </summary>
        /// <param name="capi">The client which will be using the skill item.</param>
        /// <param name="collectible">The collectible to use as template.</param>
        /// <param name="name">What to name the skill item. <para>If empty or null, the default name of the item will be used.</para></param>
        /// <param name="description">The description of the skill item. <para>If null, an empty string will be used.</para></param>
        /// <returns></returns>
        public static SkillItem CreateSkillItemFromCollectible(ICoreClientAPI capi, CollectibleObject collectible, string name, string description)
        {
            return CreateSkillItemFromCollectible(capi, collectible, name, description, ColorUtil.WhiteArgb);
        }

        /// <summary>
        /// Creates a <see cref="SkillItem"/> using the provided parameters.
        /// </summary>
        /// <param name="capi">The client which will be using the skill item.</param>
        /// <param name="collectible">The collectible to use as template.</param>
        /// <param name="name">What to name the skill item. <para>If empty or null, the default name of the item will be used.</para></param>
        /// <param name="description">The description of the skill item. <para>If null, an empty string will be used.</para></param>
        /// <param name="color">The color used to colorize the skill item icon.</param>
        /// <returns></returns>
        public static SkillItem CreateSkillItemFromCollectible(ICoreClientAPI capi, CollectibleObject collectible, string name, string description, int color)
        {
            return CreateSkillItemFromItemStack(capi, new ItemStack(collectible), name, description, color);
        }


        #endregion

        #endregion

    }
}
