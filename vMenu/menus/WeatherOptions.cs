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
    public class WeatherOptions
    {
        // Variables
        private Menu menu;
        public static Dictionary<uint, MenuItem> weatherHashMenuIndex = new Dictionary<uint, MenuItem>();
        public MenuCheckboxItem dynamicWeatherEnabled;
        public MenuCheckboxItem blackout;

        private void CreateMenu()
        {
            // Create the menu.
            menu = new Menu(Game.Player.Name, "天氣選項");

            dynamicWeatherEnabled = new MenuCheckboxItem("開關動態天氣", "啟用或禁用動態天氣變化.", EventManager.dynamicWeather);
            blackout = new MenuCheckboxItem("開關燈光", "這會禁用或啟用地圖上的所有燈光.", EventManager.blackoutMode);
            MenuItem extrasunny = new MenuItem("大太陽", "將天氣設置為 ~y~更加陽光明媚~s~!");
            MenuItem clear = new MenuItem("明確", "將天氣設置為 ~y~明確~s~!");
            MenuItem neutral = new MenuItem("普通天氣", "將天氣設置為 ~y~普通天氣~s~!");
            MenuItem smog = new MenuItem("煙霧", "將天氣設置為 ~y~煙霧~s~!");
            MenuItem foggy = new MenuItem("霧霾", "將天氣設置為 ~y~霧霾~s~!");
            MenuItem clouds = new MenuItem("多雲的", "將天氣設置為  ~y~多雲的~s~!");
            MenuItem overcast = new MenuItem("灰濛蒙", "將天氣設置為  ~y~灰濛蒙~s~!");
            MenuItem clearing = new MenuItem("樹林中中氣息", "將天氣設置為 ~y~樹林中中氣息~s~!");
            MenuItem rain = new MenuItem("多雨", "將天氣設置為~y~多雨~s~!");
            MenuItem thunder = new MenuItem("雷電雨", "將天氣設置為 ~y~雷電雨~s~!");
            MenuItem blizzard = new MenuItem("暴風雪", "將天氣設置為 ~y~暴風雪~s~!");
            MenuItem snow = new MenuItem("雪", "將天氣設置為 ~y~雪~s~!");
            MenuItem snowlight = new MenuItem("小雪", "將天氣設置為 ~y~小雪~s~!");
            MenuItem xmas = new MenuItem("聖誕節的雪", "將天氣設置為 ~y~聖誕節的雪~s~!");
            MenuItem halloween = new MenuItem("萬聖節", "將天氣設置為 ~y~萬聖節~s~!");
            MenuItem removeclouds = new MenuItem("移除所有的雲", "移除天空中的所有的雲!");
            MenuItem randomizeclouds = new MenuItem("隨機產生雲", "向天空隨機新增雲!");

            var indexOffset = 2;
            if (IsAllowed(Permission.WODynamic))
            {
                menu.AddMenuItem(dynamicWeatherEnabled);
                indexOffset--;
            }
            if (IsAllowed(Permission.WOBlackout))
            {
                menu.AddMenuItem(blackout);
                indexOffset--;
            }
            if (IsAllowed(Permission.WOSetWeather))
            {
                weatherHashMenuIndex.Add((uint)GetHashKey("EXTRASUNNY"), extrasunny);
                weatherHashMenuIndex.Add((uint)GetHashKey("CLEAR"), clear);
                weatherHashMenuIndex.Add((uint)GetHashKey("NEUTRAL"), neutral);
                weatherHashMenuIndex.Add((uint)GetHashKey("SMOG"), smog);
                weatherHashMenuIndex.Add((uint)GetHashKey("FOGGY"), foggy);
                weatherHashMenuIndex.Add((uint)GetHashKey("CLOUDS"), clouds);
                weatherHashMenuIndex.Add((uint)GetHashKey("OVERCAST"), overcast);
                weatherHashMenuIndex.Add((uint)GetHashKey("CLEARING"), clearing);
                weatherHashMenuIndex.Add((uint)GetHashKey("RAIN"), rain);
                weatherHashMenuIndex.Add((uint)GetHashKey("THUNDER"), thunder);
                weatherHashMenuIndex.Add((uint)GetHashKey("BLIZZARD"), blizzard);
                weatherHashMenuIndex.Add((uint)GetHashKey("SNOW"), snow);
                weatherHashMenuIndex.Add((uint)GetHashKey("SNOWLIGHT"), snowlight);
                weatherHashMenuIndex.Add((uint)GetHashKey("XMAS"), xmas);
                weatherHashMenuIndex.Add((uint)GetHashKey("HALLOWEEN"), halloween);

                menu.AddMenuItem(extrasunny);
                menu.AddMenuItem(clear);
                menu.AddMenuItem(neutral);
                menu.AddMenuItem(smog);
                menu.AddMenuItem(foggy);
                menu.AddMenuItem(clouds);
                menu.AddMenuItem(overcast);
                menu.AddMenuItem(clearing);
                menu.AddMenuItem(rain);
                menu.AddMenuItem(thunder);
                menu.AddMenuItem(blizzard);
                menu.AddMenuItem(snow);
                menu.AddMenuItem(snowlight);
                menu.AddMenuItem(xmas);
                menu.AddMenuItem(halloween);
            }
            if (IsAllowed(Permission.WORandomizeClouds))
            {
                menu.AddMenuItem(removeclouds);
            }

            if (IsAllowed(Permission.WORemoveClouds))
            {
                menu.AddMenuItem(randomizeclouds);
            }

            List<string> weatherTypes = new List<string>()
            {
                "EXTRASUNNY",
                "CLEAR",
                "NEUTRAL",
                "SMOG",
                "FOGGY",
                "CLOUDS",
                "OVERCAST",
                "CLEARING",
                "RAIN",
                "THUNDER",
                "BLIZZARD",
                "SNOW",
                "SNOWLIGHT",
                "XMAS",
                "HALLOWEEN"
            };

            menu.OnItemSelect += (sender, item, index2) =>
            {
                var index = index2 + indexOffset;
                // A weather type is selected.
                if (index >= 2 && index <= 16)
                {
                    Notify.Custom($"天氣將再30秒後更改為 ~y~{weatherTypes[index - 2]}~s~");
                    UpdateServerWeather(weatherTypes[index - 2], EventManager.blackoutMode, EventManager.dynamicWeather);
                }
                if (item == removeclouds)
                {
                    ModifyClouds(true);
                }
                else if (item == randomizeclouds)
                {
                    ModifyClouds(false);
                }
            };

            menu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == dynamicWeatherEnabled)
                {
                    EventManager.dynamicWeather = _checked;
                    Notify.Custom($"現在天氣動態變化 {(_checked ? "~g~開啟" : "~r~關閉")}~s~.");
                    UpdateServerWeather(EventManager.currentWeatherType, EventManager.blackoutMode, _checked);
                }
                else if (item == blackout)
                {
                    EventManager.blackoutMode = _checked;
                    Notify.Custom($"現在是停電模式 {(_checked ? "~g~開啟" : "~r~關閉")}~s~.");
                    UpdateServerWeather(EventManager.currentWeatherType, _checked, EventManager.dynamicWeather);
                }
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
