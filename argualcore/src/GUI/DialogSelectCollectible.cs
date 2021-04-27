using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Argual.ArgualCore.GUI
{
    /// <summary>
    /// A simple dialog for selecting a collectible from a list of collectibles.
    /// </summary>
    public class DialogSelectCollectible : DialogSelectSkillItem
    {
        /// <summary>
        /// Initializes the selection dialog.
        /// </summary>
        /// <param name="dialogTitle">Title of the dialog.</param>
        /// <param name="collectibles">The collectibles the player can choose from.</param>
        /// <param name="onSelected">The method to call upon selection.</param>
        /// <param name="onCancelSelect">The method to call upon dialog cancel.</param>
        /// <param name="columnCount">The number of columns. This has no effect if there are fewer skill items than this amount.</param>
        /// <param name="showName">Whether or not to show skill item names below the selection grid.</param>
        /// <param name="showDescription">Whether or not to show skill item descriptions below the selection grid. <para>If there are no descriptions, this has no effect.</para></param>
        /// <param name="capi">The client side API.</param>
        public DialogSelectCollectible(string dialogTitle, IEnumerable<CollectibleObject> collectibles, Vintagestory.API.Common.Action<int> onSelected, Vintagestory.API.Common.Action onCancelSelect, ICoreClientAPI capi, int columnCount = 7, bool showName = true, bool showDescription = true) : this(dialogTitle, collectibles, (c)=>DefaultConvert(c, capi), onSelected, onCancelSelect, capi, columnCount, showName, showDescription) { }

        /// <summary>
        /// Initializes the selection dialog.
        /// </summary>
        /// <param name="dialogTitle">Title of the dialog.</param>
        /// <param name="collectibles">The collectibles the player can choose from.</param>
        /// <param name="converter">The function used to get the skill items from the collectibles.</param>
        /// <param name="onSelected">The method to call upon selection.</param>
        /// <param name="onCancelSelect">The method to call upon dialog cancel.</param>
        /// <param name="columnCount">The number of columns. This has no effect if there are fewer skill items than this amount.</param>
        /// <param name="showName">Whether or not to show skill item names below the selection grid.</param>
        /// <param name="showDescription">Whether or not to show skill item descriptions below the selection grid. <para>If there are no descriptions, this has no effect.</para></param>
        /// <param name="capi">The client side API.</param>
        public DialogSelectCollectible(string dialogTitle, IEnumerable<CollectibleObject> collectibles, Vintagestory.API.Common.Func<CollectibleObject, SkillItem> converter, Vintagestory.API.Common.Action<int> onSelected, Vintagestory.API.Common.Action onCancelSelect, ICoreClientAPI capi, int columnCount = 7, bool showName = true, bool showDescription = true) : base(dialogTitle, collectibles.Select(c=> converter?.Invoke(c)), onSelected, onCancelSelect, capi, columnCount, showName, showDescription) { }

        private static SkillItem DefaultConvert(CollectibleObject collectible, ICoreClientAPI capi)
        {
            return GUITool.CreateSkillItemFromCollectible(capi, collectible, null, null, ColorUtil.WhiteArgb);
        }

    }
}
