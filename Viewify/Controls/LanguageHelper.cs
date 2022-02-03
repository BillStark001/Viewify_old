using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Viewify.Logic;

namespace Viewify.Controls
{
    public static class LanguageHelper
    {
        /// <summary>
        /// Returns a localized string if the relevant resource is found, otherwise itself.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string TryLocalize(this string str)
        {
            var loc = Application.Current.TryFindResource(str);
            if (loc == null)
                return str;
            else
                return loc.ToString() ?? str;
        }

        /// <summary>
        /// Returns a localized string if the relevant resource is found, otherwise itself.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="succeeded"></param>
        /// <returns></returns>
        public static string TryLocalize(this string str, out bool succeeded)
        {
            var loc = Application.Current.TryFindResource(str);
            succeeded = false;
            if (loc == null)
                return str;
            else
            {
                var ret = loc.ToString();
                if (ret != null)
                    succeeded = true;
                return ret ?? str;
            }
        }

        public static string TryLocalizeDescription(this VarRecord rc)
        {
            return (rc.Description ?? rc.DisplayName ?? rc.Name ?? $"#{rc.Id}").TryLocalize();
        }

        public static string TryLocalizeDescription2(this VarRecord rc)
        {
            return (rc.Description ?? rc.DisplayName ?? rc.Name ?? "").TryLocalize();
        }
        public static string TryLocalizeDescription(this EnumValue ev)
        {
            return (ev.Description ?? ev.StringKey ?? $"#{ev.Id}").TryLocalize();
        }

        public static string TryLocalizeDescription2(this EnumValue ev)
        {
            return (ev.Description ?? ev.StringKey ?? "").TryLocalize();
        }

        public static string TryLocalizeDisplayName(this VarRecord rc)
        {
            return (rc.DisplayName ?? rc.Name ?? $"#{rc.Id}").TryLocalize();
        }

        public static string TryLocalizeDisplayName2(this VarRecord rc)
        {
            return (rc.DisplayName ?? rc.Name ?? "").TryLocalize();
        }
    }
}
