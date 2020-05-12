using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenuAPI;
using Newtonsoft.Json;
using CitizenFX.Core;
using static CitizenFX.Core.UI.Screen;
using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.PermissionsManager;

namespace vMenuClient
{
    public class TimeOptions
    {
        // Variables
        private Menu menu;
        public MenuItem freezeTimeToggle;

        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            // Create the menu.
            menu = new Menu(Game.Player.Name, "時間選項");

            // Create all menu items.
            freezeTimeToggle = new MenuItem("凍結/解凍 時間", "啟用/禁用 凍結時間");
            MenuItem earlymorning = new MenuItem("清晨6點", "將時間設定為清晨 06:00.")
            {
                Label = "06:00"
            };
            MenuItem morning = new MenuItem("早晨", "將時間設定為早上 09:00.")
            {
                Label = "09:00"
            };
            MenuItem noon = new MenuItem("中午", "將時間設定為中午 12:00.")
            {
                Label = "12:00"
            };
            MenuItem earlyafternoon = new MenuItem("下午", "將時間設定為下午 15:00.")
            {
                Label = "15:00"
            };
            MenuItem afternoon = new MenuItem("晚上", "將時間設定為晚上 18:00.")
            {
                Label = "18:00"
            };
            MenuItem evening = new MenuItem("晚間", "將時間設定為晚間 21:00.")
            {
                Label = "21:00"
            };
            MenuItem midnight = new MenuItem("午夜", "將時間設定為午夜 00:00.")
            {
                Label = "00:00"
            };
            MenuItem night = new MenuItem("凌晨", "將時間設定為凌晨 03:00.")
            {
                Label = "03:00"
            };

            List<string> hours = new List<string>() { "00", "01", "02", "03", "04", "05", "06", "07", "08", "09" };
            List<string> minutes = new List<string>() { "00", "01", "02", "03", "04", "05", "06", "07", "08", "09" };
            for (var i = 10; i < 60; i++)
            {
                if (i < 24)
                {
                    hours.Add(i.ToString());
                }
                minutes.Add(i.ToString());
            }
            MenuListItem manualHour = new MenuListItem("自訂小時", hours, 0);
            MenuListItem manualMinute = new MenuListItem("自訂分鐘", minutes, 0);

            // Add all menu items to the menu.
            if (IsAllowed(Permission.TOFreezeTime))
            {
                menu.AddMenuItem(freezeTimeToggle);
            }
            if (IsAllowed(Permission.TOSetTime))
            {
                menu.AddMenuItem(earlymorning);
                menu.AddMenuItem(morning);
                menu.AddMenuItem(noon);
                menu.AddMenuItem(earlyafternoon);
                menu.AddMenuItem(afternoon);
                menu.AddMenuItem(evening);
                menu.AddMenuItem(midnight);
                menu.AddMenuItem(night);
                menu.AddMenuItem(manualHour);
                menu.AddMenuItem(manualMinute);
            }

            // Handle button presses.
            menu.OnItemSelect += (sender, item, index) =>
            {
                // If it's the freeze time button.
                if (item == freezeTimeToggle)
                {
                    Subtitle.Info($"目前時間已經 {(EventManager.freezeTime ? "~y~繼續" : "~o~凍結")}~s~.", prefix: "Info:");
                    UpdateServerTime(EventManager.currentHours, EventManager.currentMinutes, !EventManager.freezeTime);
                }
                else
                {
                    // Set the time using the index and some math :)
                    // eg: index = 3 (12:00) ---> 3 * 3 (=9) + 3 [= 12] ---> 12:00
                    // eg: index = 8 (03:00) ---> 8 * 3 (=24) + 3 (=27, >23 so 27-24) [=3] ---> 03:00
                    var newHour = 0;
                    if (IsAllowed(Permission.TOFreezeTime))
                    {
                        newHour = (((index * 3) + 3 < 23) ? (index * 3) + 3 : ((index * 3) + 3) - 24);
                    }
                    else
                    {
                        newHour = ((((index + 1) * 3) + 3 < 23) ? ((index + 1) * 3) + 3 : (((index + 1) * 3) + 3) - 24);
                    }

                    var newMinute = 0;
                    Subtitle.Info($"目前時間已經設定為 ~y~{(newHour < 10 ? $"0{newHour}" : newHour.ToString())}~s~:~y~" +
                        $"{(newMinute < 10 ? $"0{newMinute}" : newMinute.ToString())}~s~.", prefix: "Info:");
                    UpdateServerTime(newHour, newMinute, EventManager.freezeTime);
                }

            };

            menu.OnListItemSelect += (sender, item, listIndex, itemIndex) =>
            {
                int newHour = EventManager.currentHours;
                int newMinute = EventManager.currentMinutes;
                if (item == manualHour)
                {
                    newHour = item.ListIndex;
                }
                else if (item == manualMinute)
                {
                    newMinute = item.ListIndex;
                }
                Subtitle.Info($"目前時間已經設定為 ~y~{(newHour < 10 ? $"0{newHour}" : newHour.ToString())}~s~:~y~" +
                        $"{(newMinute < 10 ? $"0{newMinute}" : newMinute.ToString())}~s~.", prefix: "Info:");
                UpdateServerTime(newHour, newMinute, EventManager.freezeTime);
            };
        }

        /// <summary>
        /// Create the menu if it doesn't exist, and then returns it.
        /// </summary>
        /// <returns>The Menu</returns>
        public Menu GetMenu()
        {
            if (menu == null)
            {
                CreateMenu();
            }
            return menu;
        }
    }
}
