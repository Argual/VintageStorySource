using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace Argual.ExampleMultitools
{
    public class AdvancedMultitool : ArgualCore.MultiTool.ItemMultiTool
    {
        public static readonly string advancedSwitchAttributeKey = ExampleMultitoolsMod.Domain + AssetLocation.LocationSeparator + "switchCount";

        protected override void OnMultiToolSwitchComplete(IServerPlayer player, ItemSlot slot)
        {
            int switchCount = slot.Itemstack.Attributes.GetInt(advancedSwitchAttributeKey, 0);
            switchCount++;
            slot.Itemstack.Attributes.SetInt(advancedSwitchAttributeKey, switchCount);
            player.SendMessage(GlobalConstants.GeneralChatGroup, $"I have been switched to {switchCount} times!", EnumChatType.Notification);
        }

    }
}
