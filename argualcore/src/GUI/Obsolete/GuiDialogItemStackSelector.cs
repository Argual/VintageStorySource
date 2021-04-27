using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace Argual.ArgualCore.GUI
{
    // Based on the recipe selector

    /// <summary>
    /// Use <see cref="DialogSelectCollectible"/> instead!
    /// </summary>
    [Obsolete]
    public class GuiDialogItemStackSelector : GuiDialogGeneric
    {
        int prevSlotOver = -1;
        List<SkillItem> skillItems;
        bool didSelect = false;
        Vintagestory.API.Common.Action<int> onSelectedItemStack;
        Vintagestory.API.Common.Action onCancelSelect;

        /// <summary>
        /// A dialog allowing to select an item stack from a list of item stacks.
        /// </summary>
        /// <param name="dialogTitle">Title of the dialog.</param>
        /// <param name="itemStacks">The item stacks the player can choose from.</param>
        /// <param name="onSelectedItemStack">The method to call upon selection.</param>
        /// <param name="onCancelSelect">The method to call upon dialog cancel.</param>
        /// <param name="capi">The client side API.</param>
        public GuiDialogItemStackSelector(string dialogTitle, ItemStack[] itemStacks, Vintagestory.API.Common.Action<int> onSelectedItemStack, Vintagestory.API.Common.Action onCancelSelect, ICoreClientAPI capi) : base(dialogTitle, capi)
        {
            this.onSelectedItemStack = onSelectedItemStack;
            this.onCancelSelect = onCancelSelect;

            skillItems = new List<SkillItem>();

            double size = GuiElementPassiveItemSlot.unscaledSlotSize + GuiElementItemSlotGrid.unscaledSlotPadding;

            for (int i = 0; i < itemStacks.Length; i++)
            {
                ItemStack stack = itemStacks[i];
                ItemSlot dummySlot = new DummySlot(stack);

                string key = GetDescKey(stack);
                string desc = Lang.GetMatching(key);
                if (desc == key) desc = "";

                skillItems.Add(GUITool.CreateSkillItemFromCollectible(capi, stack.Item, null, desc, ColorUtil.WhiteArgb));
            }

            SetupDialog();
        }


        string GetDescKey(ItemStack stack)
        {
            string domain = "";
            string codePath = "";
            if (stack.Collectible.Code != null)
            {
                domain = stack.Collectible.Code.Domain;
                codePath = stack.Collectible.Code.Path;
            }
            return domain + AssetLocation.LocationSeparator + "itemdesc-" + codePath;
        }

        void SetupDialog()
        {
            int cnt = Math.Max(1, skillItems.Count);

            int cols = Math.Min(cnt, 7);

            int rows = (int)Math.Ceiling(cnt / (float)cols);

            double size = GuiElementPassiveItemSlot.unscaledSlotSize + GuiElementItemSlotGrid.unscaledSlotPadding;
            double innerWidth = Math.Max(300, cols * size);
            ElementBounds skillGridBounds = ElementBounds.Fixed(0, 30, innerWidth, rows * size);

            ElementBounds textBounds = ElementBounds.Fixed(0, rows * size + 50, innerWidth, 33);

            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;


            SingleComposer =
                capi.Gui
                .CreateCompo("toolmodeselect", ElementStdBounds.AutosizedMainDialog)
                .AddShadedDialogBG(bgBounds, true)
                .AddDialogTitleBar(DialogTitle, OnTitleBarClose)
                .BeginChildElements(bgBounds)
                    .AddSkillItemGrid(skillItems, cols, rows, OnSlotClick, skillGridBounds, "skillitemgrid")
                    .AddDynamicText("", CairoFont.WhiteSmallishText(), EnumTextOrientation.Left, textBounds, "name")
                    .AddDynamicText("", CairoFont.WhiteDetailText(), EnumTextOrientation.Left, textBounds.BelowCopy(0, 10, 0, 0), "desc")
                .EndChildElements()
                .Compose()
            ;

            SingleComposer.GetSkillItemGrid("skillitemgrid").OnSlotOver = OnSlotOver;
        }

        private void OnSlotOver(int num)
        {
            if (num >= skillItems.Count) return;

            if (num != prevSlotOver)
            {
                prevSlotOver = num;
                SingleComposer.GetDynamicText("name").SetNewText(skillItems[num].Name);
                SingleComposer.GetDynamicText("desc").SetNewText(skillItems[num].Description);
            }
        }

        private void OnSlotClick(int num)
        {
            onSelectedItemStack(num);

            didSelect = true;

            TryClose();
        }

        /// <inheritdoc/>
        public override void OnGuiClosed()
        {
            base.OnGuiClosed();

            if (!didSelect)
            {
                onCancelSelect();
            }
        }


        private void OnTitleBarClose()
        {
            TryClose();
        }

        public override void Dispose()
        {
            base.Dispose();

            for (int i = 0; i < skillItems.Count; i++)
            {
                skillItems[i].Dispose();
            }
        }
    }
}
