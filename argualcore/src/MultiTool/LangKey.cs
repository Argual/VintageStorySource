using Argual.ArgualCore.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Argual.ArgualCore.MultiTool
{
    /// <summary>
    /// 
    /// </summary>
    public static class LangKey
    {

        public const string MultiToolDefault = "multitooldefault";
        public static string MultiToolDefaultItem { get => LangTool.GetItemKey(ArgualCoreMod.Domain, MultiToolDefault); }
        public static string MultiToolDefaultItemDesc { get => LangTool.GetItemDescKey(ArgualCoreMod.Domain, MultiToolDefault); }

        public const string SwitchMultiTool = "switchmultitool";
        public static string SwitchMultiToolHotkey { get => LangTool.GetKeyWithDomain(ArgualCoreMod.Domain, LangTool.Combine("hotkey", SwitchMultiTool)); }
        public static string SwitchMultiToolHeldHelp { get => LangTool.GetHeldHelpKey(ArgualCoreMod.Domain, SwitchMultiTool); }
        public static string SwitchMultiToolDialogTitle { get => LangTool.GetDialogTitleKey(ArgualCoreMod.Domain, SwitchMultiTool); }

    }
}
