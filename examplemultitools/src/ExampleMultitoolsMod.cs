using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace Argual.ExampleMultitools
{
    public class ExampleMultitoolsMod : ModSystem
    {
        public const string Domain = "examplemultitools";

        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            ArgualCore.ArgualCoreMod argCoreMod = api.ModLoader.GetModSystem<ArgualCore.ArgualCoreMod>();

            // Register items ad per usual.
            api.RegisterItemClass("examplemultitool", typeof(ArgualCore.MultiTool.ItemMultiTool));
            api.RegisterItemClass("advancedexamplemultitool", typeof(AdvancedMultitool));

            // Register multitools to the multitool system (in the same family).
            argCoreMod.MultiToolSystem.RegisterMultiTool(Domain + ":example-toolFamily", new AssetLocation(Domain, "examplemultitool"));
            argCoreMod.MultiToolSystem.RegisterMultiTool(Domain + ":example-toolFamily", new AssetLocation(Domain, "advancedexamplemultitool"));

            // Add watched attribute so it will be copied over on multitool switch / upgrade.
            argCoreMod.MultiToolSystem.AddWatchedAttributeKey(AdvancedMultitool.advancedSwitchAttributeKey);

        }

    }
}
