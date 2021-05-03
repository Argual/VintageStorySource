using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace Argual.ArgualCore.Language
{
    /// <summary>
    /// Contains various tools related to localization.
    /// </summary>
    public static class LangTool
    {
        #region Constants

        public const string LangSeparator = "-";
        public const string Item = "item";
        public const string Desc = "desc";
        public const string Block = "block";
        public const string HeldHelp = "heldhelp";

        public const string Dialog = "dialog";
        public const string Title = "title";

        #endregion

        #region Public static methods

        /// <summary>
        /// Returns the key prefixed with the domain.
        /// </summary>
        public static string GetKeyWithDomain(string domain, string key)
        {
            return domain + AssetLocation.LocationSeparator + key;
        }

        /// <summary>
        /// Returns the combination of the given parts separated by <see cref="LangSeparator"/>s.
        /// </summary>
        public static string Combine(params string[] parts)
        {
            string result = "";
            foreach (var part in parts)
            {
                if (!string.IsNullOrWhiteSpace(result))
                {
                    result += LangSeparator;
                }
                result += part;
            }
            return result;
        }

        public static string GetItemKey(string domain, string item)
        {
            return GetKeyWithDomain(domain, GetItemKey(item));
        }

        public static string GetItemKey(string item)
        {
            return Item + LangSeparator + item;
        }

        public static string GetItemDescKey(string domain, string item)
        {
            return GetKeyWithDomain(domain, GetItemDescKey(item));
        }

        public static string GetItemDescKey(string item)
        {
            return Combine(Item + Desc, item);
        }

        public static string GetBlockKey(string domain, string block)
        {
            return GetKeyWithDomain(domain, GetBlockKey(block));
        }

        public static string GetBlockKey(string block)
        {
            return Combine(Block, block);
        }

        public static string GetBlockDescKey(string domain, string block)
        {
            return GetKeyWithDomain(domain, GetBlockDescKey(block));
        }

        public static string GetBlockDescKey(string block)
        {
            return Combine(Block + Desc, block);
        }

        public static string GetHeldHelpKey(string domain, string collectible)
        {
            return GetKeyWithDomain(domain, GetHeldHelpKey(collectible));
        }

        public static string GetHeldHelpKey(string collectible)
        {
            return Combine(HeldHelp, collectible);
        }

        public static string GetDialogTitleKey(string domain, string title)
        {
            return GetKeyWithDomain(domain, GetDialogTitleKey(title));
        }

        public static string GetDialogTitleKey(string title)
        {
            return Combine(Dialog, Title, title);
        }

        #endregion

    }
}
