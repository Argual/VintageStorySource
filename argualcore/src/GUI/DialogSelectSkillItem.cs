using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace Argual.ArgualCore.GUI
{
    /// <summary>
    /// A simple dialog for selecting a skill item from a list of skill items.
    /// </summary>
    public class DialogSelectSkillItem : GuiDialogGeneric
    {
        /// <summary>
        /// The index of the last slot the player hovered over, or -1, if the player have not hovered over any slot yet.
        /// </summary>
        protected int prevSlotOver = -1;

        /// <summary>
        /// Whether or not the player made a selection.
        /// </summary>
        protected bool didSelect = false;

        /// <summary>
        /// Action to invoke when the player made a selection.
        /// </summary>
        protected Vintagestory.API.Common.Action<int> onSelected;

        /// <summary>
        /// Action to invoke when the player canceled a selection.
        /// </summary>
        protected Vintagestory.API.Common.Action onCancelSelect;

        /// <summary>
        /// The list of skill items.
        /// </summary>
        protected List<SkillItem> skillItems;

        /// <summary>
        /// The number of columns. If there are fewer columns than skill items, this value has no effect.
        /// </summary>
        protected int columnCount = 7;

        /// <summary>
        /// Whether or not there was any description given to any skill item initially.
        /// </summary>
        protected bool isThereAnyDescription = false;

        /// <summary>
        /// Whether or not to show skill item names below the selection grid.
        /// </summary>
        public bool ShowSkillItemName { get; protected set; } = true;

        /// <summary>
        /// Whether or not to show skill item descriptions below the selection grid.
        /// <para>If there are no descriptions, this has no effect.</para>
        /// </summary>
        public bool ShowSkillItemDescription { get; protected set; } = true;

        /// <summary>
        /// Initializes the selection dialog.
        /// </summary>
        /// <param name="dialogTitle">Title of the dialog.</param>
        /// <param name="skillItems">The skill items the player can choose from.</param>
        /// <param name="onSelected">The method to call upon selection.</param>
        /// <param name="onCancelSelect">The method to call upon dialog cancel.</param>
        /// <param name="columnCount">The number of columns. This has no effect if there are fewer skill items than this amount.</param>
        /// <param name="showName">Whether or not to show skill item names below the selection grid.</param>
        /// <param name="showDescription">Whether or not to show skill item descriptions below the selection grid. <para>If there are no descriptions, this has no effect.</para></param>
        /// <param name="capi">The client side API.</param>
        public DialogSelectSkillItem(string dialogTitle, IEnumerable<SkillItem> skillItems, Vintagestory.API.Common.Action<int> onSelected, Vintagestory.API.Common.Action onCancelSelect, ICoreClientAPI capi, int columnCount=7, bool showName=true, bool showDescription=true) : base(dialogTitle, capi)
        {
            this.onSelected = onSelected;
            this.onCancelSelect = onCancelSelect;

            this.skillItems = new List<SkillItem>(skillItems);
            this.columnCount = columnCount;

            isThereAnyDescription = skillItems.Any(i => !string.IsNullOrWhiteSpace(i.Description));

            ShowSkillItemName = showName;
            ShowSkillItemDescription = showDescription;

            SetupDialog();
        }

        /// <summary>
        /// Called when initializing the dialog.
        /// </summary>
        protected virtual void SetupDialog()
        {
            int cnt = Math.Max(1, skillItems.Count);

            int cols = Math.Min(cnt, columnCount);

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
                .AddDialogTitleBar(DialogTitle, OnTitleBarClose);

            SingleComposer.BeginChildElements(bgBounds);
            SingleComposer.AddSkillItemGrid(skillItems, cols, rows, OnSlotClick, skillGridBounds, "skillitemgrid");      

            if (ShowSkillItemName)
            {
                SingleComposer.AddDynamicText("", CairoFont.WhiteSmallishText(), EnumTextOrientation.Left, textBounds, "name");
            }

            if (ShowSkillItemDescription && isThereAnyDescription)
            {
                SingleComposer.AddDynamicText("", CairoFont.WhiteDetailText(), EnumTextOrientation.Left, textBounds.BelowCopy(0, 10, 0, 0), "desc");
            }

            SingleComposer.EndChildElements().Compose();

            SingleComposer.GetSkillItemGrid("skillitemgrid").OnSlotOver = OnSlotOver;
        }

        /// <summary>
        /// Called when the player hovers over a slot.
        /// </summary>
        /// <param name="num">The index of the hovered slot.</param>
        protected virtual void OnSlotOver(int num)
        {
            if (num >= skillItems.Count) return;

            if (num != prevSlotOver)
            {
                prevSlotOver = num;

                if (ShowSkillItemName)
                {
                    SingleComposer.GetDynamicText("name").SetNewText(skillItems[num].Name);
                }

                if (ShowSkillItemDescription && isThereAnyDescription)
                {
                    SingleComposer.GetDynamicText("desc").SetNewText(skillItems[num].Description);
                }
            }
        }

        /// <summary>
        /// Called when the player clicks a slot.
        /// </summary>
        /// <param name="num">The index of the clicked slot.</param>
        protected virtual void OnSlotClick(int num)
        {
            onSelected?.Invoke(num);

            didSelect = true;

            TryClose();
        }

        public override void OnGuiClosed()
        {
            base.OnGuiClosed();

            if (!didSelect)
            {
                onCancelSelect?.Invoke();
            }
        }

        /// <summary>
        /// Called when the player closes the title bar.
        /// </summary>
        protected virtual void OnTitleBarClose()
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
