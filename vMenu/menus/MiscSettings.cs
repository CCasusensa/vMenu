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
    public class MiscSettings
    {
        // Variables
        private Menu menu;
        private Menu teleportOptionsMenu;
        private Menu developerToolsMenu;

        public bool ShowSpeedoKmh { get; private set; } = UserDefaults.MiscSpeedKmh;
        public bool ShowSpeedoMph { get; private set; } = UserDefaults.MiscSpeedMph;
        public bool ShowCoordinates { get; private set; } = false;
        public bool HideHud { get; private set; } = false;
        public bool HideRadar { get; private set; } = false;
        public bool ShowLocation { get; private set; } = UserDefaults.MiscShowLocation;
        public bool DeathNotifications { get; private set; } = UserDefaults.MiscDeathNotifications;
        public bool JoinQuitNotifications { get; private set; } = UserDefaults.MiscJoinQuitNotifications;
        public bool LockCameraX { get; private set; } = false;
        public bool LockCameraY { get; private set; } = false;
        public bool ShowLocationBlips { get; private set; } = UserDefaults.MiscLocationBlips;
        public bool ShowPlayerBlips { get; private set; } = UserDefaults.MiscShowPlayerBlips;
        public bool MiscShowOverheadNames { get; private set; } = UserDefaults.MiscShowOverheadNames;
        public bool ShowVehicleModelDimensions { get; private set; } = false;
        public bool ShowPedModelDimensions { get; private set; } = false;
        public bool ShowPropModelDimensions { get; private set; } = false;
        public bool ShowEntityHandles { get; private set; } = false;
        public bool ShowEntityModels { get; private set; } = false;
        public bool MiscRespawnDefaultCharacter { get; private set; } = UserDefaults.MiscRespawnDefaultCharacter;
        public bool RestorePlayerAppearance { get; private set; } = UserDefaults.MiscRestorePlayerAppearance;
        public bool RestorePlayerWeapons { get; private set; } = UserDefaults.MiscRestorePlayerWeapons;
        public bool DrawTimeOnScreen { get; internal set; } = UserDefaults.MiscShowTime;
        public bool MiscRightAlignMenu { get; private set; } = UserDefaults.MiscRightAlignMenu;
        public bool MiscDisablePrivateMessages { get; private set; } = UserDefaults.MiscDisablePrivateMessages;
        public bool MiscDisableControllerSupport { get; private set; } = UserDefaults.MiscDisableControllerSupport;

        internal bool TimecycleEnabled { get; private set; } = false;
        internal int LastTimeCycleModifierIndex { get; private set; } = UserDefaults.MiscLastTimeCycleModifierIndex;
        internal int LastTimeCycleModifierStrength { get; private set; } = UserDefaults.MiscLastTimeCycleModifierStrength;


        // keybind states
        public bool KbTpToWaypoint { get; private set; } = UserDefaults.KbTpToWaypoint;
        public int KbTpToWaypointKey { get; } = vMenuShared.ConfigManager.GetSettingsInt(vMenuShared.ConfigManager.Setting.vmenu_teleport_to_wp_keybind_key) != -1
            ? vMenuShared.ConfigManager.GetSettingsInt(vMenuShared.ConfigManager.Setting.vmenu_teleport_to_wp_keybind_key)
            : 168; // 168 (F7 by default)
        public bool KbDriftMode { get; private set; } = UserDefaults.KbDriftMode;
        public bool KbRecordKeys { get; private set; } = UserDefaults.KbRecordKeys;
        public bool KbRadarKeys { get; private set; } = UserDefaults.KbRadarKeys;
        public bool KbPointKeys { get; private set; } = UserDefaults.KbPointKeys;

        internal static List<vMenuShared.ConfigManager.TeleportLocation> TpLocations = new List<vMenuShared.ConfigManager.TeleportLocation>();

        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            MenuController.MenuAlignment = MiscRightAlignMenu ? MenuController.MenuAlignmentOption.Right : MenuController.MenuAlignmentOption.Left;
            if (MenuController.MenuAlignment != (MiscRightAlignMenu ? MenuController.MenuAlignmentOption.Right : MenuController.MenuAlignmentOption.Left))
            {
                Notify.Error(CommonErrors.RightAlignedNotSupported);

                // (re)set the default to left just in case so they don't get this error again in the future.
                MenuController.MenuAlignment = MenuController.MenuAlignmentOption.Left;
                MiscRightAlignMenu = false;
                UserDefaults.MiscRightAlignMenu = false;
            }

            // Create the menu.
            menu = new Menu(Game.Player.Name, "其它設定選單");
            teleportOptionsMenu = new Menu(Game.Player.Name, "瞬移設定");
            developerToolsMenu = new Menu(Game.Player.Name, "開發者工具");

            // teleport menu
            Menu teleportMenu = new Menu(Game.Player.Name, "傳送地點");
            MenuItem teleportMenuBtn = new MenuItem("傳送地點", "傳送到伺服器添加的預配位置");
            MenuController.AddSubmenu(menu, teleportMenu);
            MenuController.BindMenuItem(menu, teleportMenu, teleportMenuBtn);

            // keybind settings menu
            Menu keybindMenu = new Menu(Game.Player.Name, "按鍵設定");
            MenuItem keybindMenuBtn = new MenuItem("按鍵設定", "開啟或關閉鍵盤綁定");
            MenuController.AddSubmenu(menu, keybindMenu);
            MenuController.BindMenuItem(menu, keybindMenu, keybindMenuBtn);

            // keybind settings menu items
            MenuCheckboxItem kbTpToWaypoint = new MenuCheckboxItem("傳送到導航點", "", KbTpToWaypoint);
            MenuCheckboxItem kbDriftMode = new MenuCheckboxItem("飄移模式", "按住鍵盤上的左移或者搖趕上的X鍵，能使車輛甩尾.", KbDriftMode);
            MenuCheckboxItem kbRecordKeys = new MenuCheckboxItem("錄影控制", "開啟或關閉用鍵盤和搖桿控制 (Rockstar 編輯器) 熱鍵.", KbRecordKeys);
            MenuCheckboxItem kbRadarKeys = new MenuCheckboxItem("小地圖控件", "按鍵盤上的z，控制器上的向下箭頭鍵在擴展雷達和普通雷達之間切換.", KbRadarKeys);
            MenuCheckboxItem kbPointKeysCheckbox = new MenuCheckboxItem("手指點控件", "", KbPointKeys);
            MenuItem backBtn = new MenuItem("返回");

            // Create the menu items.
            MenuCheckboxItem rightAlignMenu = new MenuCheckboxItem("像右對齊選單", "", MiscRightAlignMenu);
            MenuCheckboxItem disablePms = new MenuCheckboxItem("禁用私人訊息", "阻止其它人通過 '線上模式'選單像您發送私人訊息.", MiscDisablePrivateMessages);
            MenuCheckboxItem disableControllerKey = new MenuCheckboxItem("禁止控制器選單", "這將禁用控制器選單切換鍵不會禁用導航按鈕.", MiscDisableControllerSupport);
            MenuCheckboxItem speedKmh = new MenuCheckboxItem("顯示儀表板KM/H", "視窗上顯示一個計速器，以KM/h為單位顯示速度", ShowSpeedoKmh);
            MenuCheckboxItem speedMph = new MenuCheckboxItem("顯示速度MPH", "視窗上顯示一個計速器，以MPH為單位顯示速度", ShowSpeedoMph);
            MenuCheckboxItem coords = new MenuCheckboxItem("顯示坐標", "視窗頂部顯示當前座標.", ShowCoordinates);
            MenuCheckboxItem hideRadar = new MenuCheckboxItem("隱藏雷達", "隱藏雷達/小地圖", HideRadar);
            MenuCheckboxItem hideHud = new MenuCheckboxItem("隱藏 Hud", "隱藏所有HUD元素.", HideHud);
            MenuCheckboxItem showLocation = new MenuCheckboxItem("位置顯示", "顯示您當前的位置和方向，以最近的交叉路口類似PLD. ~r~警告: 可能會降低FPS", ShowLocation) { LeftIcon = MenuItem.Icon.WARNING };
            MenuCheckboxItem drawTime = new MenuCheckboxItem("顯示時間", "在視窗上顯示當前遊戲時間.", DrawTimeOnScreen);
            MenuItem saveSettings = new MenuItem("保存個人設置", "保存當前設置。 所有保存都在客戶端完成，如果重新安裝Windows，則會丟失設置。 使用vMenu在所有服務器之間共享設置.")
            {
                RightIcon = MenuItem.Icon.TICK
            };
            MenuCheckboxItem joinQuitNotifs = new MenuCheckboxItem("加入/離開通知", "有人加入或者離開伺服器都會收到通知.", JoinQuitNotifications);
            MenuCheckboxItem deathNotifs = new MenuCheckboxItem("死亡通知", "有人死亡或者被殺都會收到通知.", DeathNotifications);
            MenuCheckboxItem nightVision = new MenuCheckboxItem("開關夜視鏡", "開啟或關閉夜視鏡功能.", false);
            MenuCheckboxItem thermalVision = new MenuCheckboxItem("開關熱感應", "開啟或關閉熱感應功能.", false);
            MenuCheckboxItem vehModelDimensions = new MenuCheckboxItem("顯示載具尺寸", "當前靠近您的每輛載具都會有模型輪廓", ShowVehicleModelDimensions);
            MenuCheckboxItem propModelDimensions = new MenuCheckboxItem("顯示道具尺寸", "當前靠近您的每個武器都會有道具輪廓", ShowPropModelDimensions);
            MenuCheckboxItem pedModelDimensions = new MenuCheckboxItem("顯示人物尺寸", "當前靠近您的每個人都會有人物輪廓.", ShowPedModelDimensions);
            MenuCheckboxItem showEntityHandles = new MenuCheckboxItem("顯示實體句炳", "繪製所有關閉實體的句炳 (必須啟用上面的功能，該功能才會有用).", ShowEntityHandles);
            MenuCheckboxItem showEntityModels = new MenuCheckboxItem("顯示實體模型", "繪製所有閉合實體的模型(必須啟用上面的功能，該功能才會有用).", ShowEntityModels);
            MenuSliderItem dimensionsDistanceSlider = new MenuSliderItem("顯示尺寸半徑", "顯示實體模型/手柄/尺寸繪製範圍.", 0, 20, 20, false);

            MenuItem clearArea = new MenuItem("清空地區", "清除你周圍區域(100米)所有物件除了人");
            MenuCheckboxItem lockCamX = new MenuCheckboxItem("鎖定相機水平翻轉", "鎖定相機水平翻轉.", false);
            MenuCheckboxItem lockCamY = new MenuCheckboxItem("鎖定相機垂直翻轉", "鎖定相機垂直翻轉.", false);


            Menu connectionSubmenu = new Menu(Game.Player.Name, "連線選項");
            MenuItem connectionSubmenuBtn = new MenuItem("連線選項", "伺服器連接/遊戲退出選項");

            MenuItem quitSession = new MenuItem("退出連線", "");
            MenuItem rejoinSession = new MenuItem("重新加入連線", "");
            MenuItem quitGame = new MenuItem("退出遊戲", "五秒後退出遊戲");
            MenuItem disconnectFromServer = new MenuItem("與伺服器段開連接", "");
            connectionSubmenu.AddMenuItem(quitSession);
            connectionSubmenu.AddMenuItem(rejoinSession);
            connectionSubmenu.AddMenuItem(quitGame);
            connectionSubmenu.AddMenuItem(disconnectFromServer);

            MenuCheckboxItem enableTimeCycle = new MenuCheckboxItem("Enable Timecycle Modifier", "Enable or disable the timecycle modifier from the list below.", TimecycleEnabled);
            List<string> timeCycleModifiersListData = TimeCycles.Timecycles.ToList();
            for (var i = 0; i < timeCycleModifiersListData.Count; i++)
            {
                timeCycleModifiersListData[i] += $" ({i + 1}/{timeCycleModifiersListData.Count})";
            }
            MenuListItem timeCycles = new MenuListItem("TM", timeCycleModifiersListData, MathUtil.Clamp(LastTimeCycleModifierIndex, 0, Math.Max(0, timeCycleModifiersListData.Count - 1)), "選擇一個倒數計時啟用上面的複選框.");
            MenuSliderItem timeCycleIntensity = new MenuSliderItem("時間週期修改器強度", "設置時間週期修改器強度.", 0, 20, LastTimeCycleModifierStrength, true);

            MenuCheckboxItem locationBlips = new MenuCheckboxItem("位置提示", "在地圖上顯示位置提示.", ShowLocationBlips);
            MenuCheckboxItem playerBlips = new MenuCheckboxItem("顯示玩家提示", "在地圖上顯示玩家提示.", ShowPlayerBlips);
            MenuCheckboxItem playerNames = new MenuCheckboxItem("顯示玩家名字", "開啟或關閉玩家名字", MiscShowOverheadNames);
            MenuCheckboxItem respawnDefaultCharacter = new MenuCheckboxItem("重新生成為預設創建", "", MiscRespawnDefaultCharacter);
            MenuCheckboxItem restorePlayerAppearance = new MenuCheckboxItem("恢復玩家外觀", "死亡重生時恢復玩家的皮膚，重新加速伺服器時將不會恢復您以前的造型.", RestorePlayerAppearance);
            MenuCheckboxItem restorePlayerWeapons = new MenuCheckboxItem("恢復玩家武器", "死亡重生時恢復玩家的武器，重新加速伺服器時將不會恢復您以前的武器.", RestorePlayerWeapons);

            MenuController.AddSubmenu(menu, connectionSubmenu);
            MenuController.BindMenuItem(menu, connectionSubmenu, connectionSubmenuBtn);

            keybindMenu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == kbTpToWaypoint)
                {
                    KbTpToWaypoint = _checked;
                }
                else if (item == kbDriftMode)
                {
                    KbDriftMode = _checked;
                }
                else if (item == kbRecordKeys)
                {
                    KbRecordKeys = _checked;
                }
                else if (item == kbRadarKeys)
                {
                    KbRadarKeys = _checked;
                }
                else if (item == kbPointKeysCheckbox)
                {
                    KbPointKeys = _checked;
                }
            };
            keybindMenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == backBtn)
                {
                    keybindMenu.GoBack();
                }
            };

            connectionSubmenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == quitGame)
                {
                    QuitGame();
                }
                else if (item == quitSession)
                {
                    if (NetworkIsSessionActive())
                    {
                        if (NetworkIsHost())
                        {
                            Notify.Error("抱歉，您不能在主持人後退出戰局。 這將阻止其他玩家加入/停留在服務器上。");
                        }
                        else
                        {
                            QuitSession();
                        }
                    }
                    else
                    {
                        Notify.Error("您目前沒有參加任何戰局.");
                    }
                }
                else if (item == rejoinSession)
                {
                    if (NetworkIsSessionActive())
                    {
                        Notify.Error("您已經連接到戰局.");
                    }
                    else
                    {
                        Notify.Info("嘗試重新加入戰局.");
                        NetworkSessionHost(-1, 32, false);
                    }
                }
                else if (item == disconnectFromServer)
                {

                    RegisterCommand("disconnect", new Action<dynamic, dynamic, dynamic>((a, b, c) => { }), false);
                    ExecuteCommand("disconnect");
                }
            };

            // Teleportation options
            if (IsAllowed(Permission.MSTeleportToWp) || IsAllowed(Permission.MSTeleportLocations) || IsAllowed(Permission.MSTeleportToCoord))
            {
                MenuItem teleportOptionsMenuBtn = new MenuItem("傳送選項", "各種傳送選擇.") { Label = "→→→" };
                menu.AddMenuItem(teleportOptionsMenuBtn);
                MenuController.BindMenuItem(menu, teleportOptionsMenu, teleportOptionsMenuBtn);

                MenuItem tptowp = new MenuItem("傳送到導航點", "傳送到地圖上的導航點");
                MenuItem tpToCoord = new MenuItem("傳送到座標", "輸入 x, y, z 座標 您將被傳誦到該位置");
                MenuItem saveLocationBtn = new MenuItem("保存傳送位置", "將您當前的位置添加倒傳送位置選單中，並且保存到伺服器中");
                teleportOptionsMenu.OnItemSelect += async (sender, item, index) =>
                {
                    // Teleport to waypoint.
                    if (item == tptowp)
                    {
                        TeleportToWp();
                    }
                    else if (item == tpToCoord)
                    {
                        string x = await GetUserInput("輸入 X 座標.");
                        if (string.IsNullOrEmpty(x))
                        {
                            Notify.Error(CommonErrors.InvalidInput);
                            return;
                        }
                        string y = await GetUserInput("輸入 Y 座標.");
                        if (string.IsNullOrEmpty(y))
                        {
                            Notify.Error(CommonErrors.InvalidInput);
                            return;
                        }
                        string z = await GetUserInput("輸入 Z 座標.");
                        if (string.IsNullOrEmpty(z))
                        {
                            Notify.Error(CommonErrors.InvalidInput);
                            return;
                        }

                        float posX = 0f;
                        float posY = 0f;
                        float posZ = 0f;

                        if (!float.TryParse(x, out posX))
                        {
                            if (int.TryParse(x, out int intX))
                            {
                                posX = (float)intX;
                            }
                            else
                            {
                                Notify.Error("您輸入的不是一個有效的 X 座標.");
                                return;
                            }
                        }
                        if (!float.TryParse(y, out posY))
                        {
                            if (int.TryParse(y, out int intY))
                            {
                                posY = (float)intY;
                            }
                            else
                            {
                                Notify.Error("您輸入的不是一個有效的 Y 座標.");
                                return;
                            }
                        }
                        if (!float.TryParse(z, out posZ))
                        {
                            if (int.TryParse(z, out int intZ))
                            {
                                posZ = (float)intZ;
                            }
                            else
                            {
                                Notify.Error("您輸入的不是一個有效的 Z 座標.");
                                return;
                            }
                        }

                        await TeleportToCoords(new Vector3(posX, posY, posZ), true);
                    }
                    else if (item == saveLocationBtn)
                    {
                        SavePlayerLocationToLocationsFile();
                    }
                };

                if (IsAllowed(Permission.MSTeleportToWp))
                {
                    teleportOptionsMenu.AddMenuItem(tptowp);
                    keybindMenu.AddMenuItem(kbTpToWaypoint);
                }
                if (IsAllowed(Permission.MSTeleportToCoord))
                {
                    teleportOptionsMenu.AddMenuItem(tpToCoord);
                }
                if (IsAllowed(Permission.MSTeleportLocations))
                {
                    teleportOptionsMenu.AddMenuItem(teleportMenuBtn);

                    MenuController.AddSubmenu(teleportOptionsMenu, teleportMenu);
                    MenuController.BindMenuItem(teleportOptionsMenu, teleportMenu, teleportMenuBtn);
                    teleportMenuBtn.Label = "→→→";

                    teleportMenu.OnMenuOpen += (sender) =>
                    {
                        if (teleportMenu.Size != TpLocations.Count())
                        {
                            teleportMenu.ClearMenuItems();
                            foreach (var location in TpLocations)
                            {
                                var x = Math.Round(location.coordinates.X, 2);
                                var y = Math.Round(location.coordinates.Y, 2);
                                var z = Math.Round(location.coordinates.Z, 2);
                                var heading = Math.Round(location.heading, 2);
                                MenuItem tpBtn = new MenuItem(location.name, $"已經傳送到 ~y~{location.name}~n~~s~x: ~y~{x}~n~~s~y: ~y~{y}~n~~s~z: ~y~{z}~n~~s~heading: ~y~{heading}") { ItemData = location };
                                teleportMenu.AddMenuItem(tpBtn);
                            }
                        }
                    };

                    teleportMenu.OnItemSelect += async (sender, item, index) =>
                    {
                        if (item.ItemData is vMenuShared.ConfigManager.TeleportLocation tl)
                        {
                            await TeleportToCoords(tl.coordinates, true);
                            SetEntityHeading(Game.PlayerPed.Handle, tl.heading);
                            SetGameplayCamRelativeHeading(0f);
                        }
                    };

                    if (IsAllowed(Permission.MSTeleportSaveLocation))
                    {
                        teleportOptionsMenu.AddMenuItem(saveLocationBtn);
                    }
                }

            }

            #region dev tools menu

            MenuItem devToolsBtn = new MenuItem("開發者選項", "各種開發/調試工具.") { Label = "→→→" };
            menu.AddMenuItem(devToolsBtn);
            MenuController.AddSubmenu(menu, developerToolsMenu);
            MenuController.BindMenuItem(menu, developerToolsMenu, devToolsBtn);

            // clear area and coordinates
            if (IsAllowed(Permission.MSClearArea))
            {
                developerToolsMenu.AddMenuItem(clearArea);
            }
            if (IsAllowed(Permission.MSShowCoordinates))
            {
                developerToolsMenu.AddMenuItem(coords);
            }

            // model outlines
            if (!vMenuShared.ConfigManager.GetSettingsBool(vMenuShared.ConfigManager.Setting.vmenu_disable_entity_outlines_tool))
            {
                developerToolsMenu.AddMenuItem(vehModelDimensions);
                developerToolsMenu.AddMenuItem(propModelDimensions);
                developerToolsMenu.AddMenuItem(pedModelDimensions);
                developerToolsMenu.AddMenuItem(showEntityHandles);
                developerToolsMenu.AddMenuItem(showEntityModels);
                developerToolsMenu.AddMenuItem(dimensionsDistanceSlider);
            }


            // timecycle modifiers
            developerToolsMenu.AddMenuItem(timeCycles);
            developerToolsMenu.AddMenuItem(enableTimeCycle);
            developerToolsMenu.AddMenuItem(timeCycleIntensity);

            developerToolsMenu.OnSliderPositionChange += (sender, item, oldPos, newPos, itemIndex) =>
            {
                if (item == timeCycleIntensity)
                {
                    ClearTimecycleModifier();
                    if (TimecycleEnabled)
                    {
                        SetTimecycleModifier(TimeCycles.Timecycles[timeCycles.ListIndex]);
                        float intensity = ((float)newPos / 20f);
                        SetTimecycleModifierStrength(intensity);
                    }
                    UserDefaults.MiscLastTimeCycleModifierIndex = timeCycles.ListIndex;
                    UserDefaults.MiscLastTimeCycleModifierStrength = timeCycleIntensity.Position;
                }
                else if (item == dimensionsDistanceSlider)
                {
                    FunctionsController.entityRange = ((float)newPos / 20f) * 2000f; // max radius = 2000f;
                }
            };

            developerToolsMenu.OnListIndexChange += (sender, item, oldIndex, newIndex, itemIndex) =>
            {
                if (item == timeCycles)
                {
                    ClearTimecycleModifier();
                    if (TimecycleEnabled)
                    {
                        SetTimecycleModifier(TimeCycles.Timecycles[timeCycles.ListIndex]);
                        float intensity = ((float)timeCycleIntensity.Position / 20f);
                        SetTimecycleModifierStrength(intensity);
                    }
                    UserDefaults.MiscLastTimeCycleModifierIndex = timeCycles.ListIndex;
                    UserDefaults.MiscLastTimeCycleModifierStrength = timeCycleIntensity.Position;
                }
            };

            developerToolsMenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == clearArea)
                {
                    var pos = Game.PlayerPed.Position;
                    BaseScript.TriggerServerEvent("vMenu:ClearArea", pos.X, pos.Y, pos.Z);
                }
            };

            developerToolsMenu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == vehModelDimensions)
                {
                    ShowVehicleModelDimensions = _checked;
                }
                else if (item == propModelDimensions)
                {
                    ShowPropModelDimensions = _checked;
                }
                else if (item == pedModelDimensions)
                {
                    ShowPedModelDimensions = _checked;
                }
                else if (item == showEntityHandles)
                {
                    ShowEntityHandles = _checked;
                }
                else if (item == showEntityModels)
                {
                    ShowEntityModels = _checked;
                }
                else if (item == enableTimeCycle)
                {
                    TimecycleEnabled = _checked;
                    ClearTimecycleModifier();
                    if (TimecycleEnabled)
                    {
                        SetTimecycleModifier(TimeCycles.Timecycles[timeCycles.ListIndex]);
                        float intensity = ((float)timeCycleIntensity.Position / 20f);
                        SetTimecycleModifierStrength(intensity);
                    }
                }
                else if (item == coords)
                {
                    ShowCoordinates = _checked;
                }
            };

            #endregion


            // Keybind options
            if (IsAllowed(Permission.MSDriftMode))
            {
                keybindMenu.AddMenuItem(kbDriftMode);
            }
            // always allowed keybind menu options
            keybindMenu.AddMenuItem(kbRecordKeys);
            keybindMenu.AddMenuItem(kbRadarKeys);
            keybindMenu.AddMenuItem(kbPointKeysCheckbox);
            keybindMenu.AddMenuItem(backBtn);

            // Always allowed
            menu.AddMenuItem(rightAlignMenu);
            menu.AddMenuItem(disablePms);
            menu.AddMenuItem(disableControllerKey);
            menu.AddMenuItem(speedKmh);
            menu.AddMenuItem(speedMph);
            menu.AddMenuItem(keybindMenuBtn);
            keybindMenuBtn.Label = "→→→";
            if (IsAllowed(Permission.MSConnectionMenu))
            {
                menu.AddMenuItem(connectionSubmenuBtn);
                connectionSubmenuBtn.Label = "→→→";
            }
            if (IsAllowed(Permission.MSShowLocation))
            {
                menu.AddMenuItem(showLocation);
            }
            menu.AddMenuItem(drawTime); // always allowed
            if (IsAllowed(Permission.MSJoinQuitNotifs))
            {
                menu.AddMenuItem(deathNotifs);
            }
            if (IsAllowed(Permission.MSDeathNotifs))
            {
                menu.AddMenuItem(joinQuitNotifs);
            }
            if (IsAllowed(Permission.MSNightVision))
            {
                menu.AddMenuItem(nightVision);
            }
            if (IsAllowed(Permission.MSThermalVision))
            {
                menu.AddMenuItem(thermalVision);
            }
            if (IsAllowed(Permission.MSLocationBlips))
            {
                menu.AddMenuItem(locationBlips);
                ToggleBlips(ShowLocationBlips);
            }
            if (IsAllowed(Permission.MSPlayerBlips))
            {
                menu.AddMenuItem(playerBlips);
            }
            if (IsAllowed(Permission.MSOverheadNames))
            {
                menu.AddMenuItem(playerNames);
            }
            // always allowed, it just won't do anything if the server owner disabled the feature, but players can still toggle it.
            menu.AddMenuItem(respawnDefaultCharacter);
            if (IsAllowed(Permission.MSRestoreAppearance))
            {
                menu.AddMenuItem(restorePlayerAppearance);
            }
            if (IsAllowed(Permission.MSRestoreWeapons))
            {
                menu.AddMenuItem(restorePlayerWeapons);
            }

            // Always allowed
            menu.AddMenuItem(hideRadar);
            menu.AddMenuItem(hideHud);
            menu.AddMenuItem(lockCamX);
            menu.AddMenuItem(lockCamY);
            menu.AddMenuItem(saveSettings);

            // Handle checkbox changes.
            menu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == rightAlignMenu)
                {

                    MenuController.MenuAlignment = _checked ? MenuController.MenuAlignmentOption.Right : MenuController.MenuAlignmentOption.Left;
                    MiscRightAlignMenu = _checked;
                    UserDefaults.MiscRightAlignMenu = MiscRightAlignMenu;

                    if (MenuController.MenuAlignment != (_checked ? MenuController.MenuAlignmentOption.Right : MenuController.MenuAlignmentOption.Left))
                    {
                        Notify.Error(CommonErrors.RightAlignedNotSupported);
                        // (re)set the default to left just in case so they don't get this error again in the future.
                        MenuController.MenuAlignment = MenuController.MenuAlignmentOption.Left;
                        MiscRightAlignMenu = false;
                        UserDefaults.MiscRightAlignMenu = false;
                    }

                }
                else if (item == disablePms)
                {
                    MiscDisablePrivateMessages = _checked;
                }
                else if (item == disableControllerKey)
                {
                    MiscDisableControllerSupport = _checked;
                    MenuController.EnableMenuToggleKeyOnController = !_checked;
                }
                else if (item == speedKmh)
                {
                    ShowSpeedoKmh = _checked;
                }
                else if (item == speedMph)
                {
                    ShowSpeedoMph = _checked;
                }
                else if (item == hideHud)
                {
                    HideHud = _checked;
                    DisplayHud(!_checked);
                }
                else if (item == hideRadar)
                {
                    HideRadar = _checked;
                    if (!_checked)
                    {
                        DisplayRadar(true);
                    }
                }
                else if (item == showLocation)
                {
                    ShowLocation = _checked;
                }
                else if (item == drawTime)
                {
                    DrawTimeOnScreen = _checked;
                }
                else if (item == deathNotifs)
                {
                    DeathNotifications = _checked;
                }
                else if (item == joinQuitNotifs)
                {
                    JoinQuitNotifications = _checked;
                }
                else if (item == nightVision)
                {
                    SetNightvision(_checked);
                }
                else if (item == thermalVision)
                {
                    SetSeethrough(_checked);
                }
                else if (item == lockCamX)
                {
                    LockCameraX = _checked;
                }
                else if (item == lockCamY)
                {
                    LockCameraY = _checked;
                }
                else if (item == locationBlips)
                {
                    ToggleBlips(_checked);
                    ShowLocationBlips = _checked;
                }
                else if (item == playerBlips)
                {
                    ShowPlayerBlips = _checked;
                }
                else if (item == playerNames)
                {
                    MiscShowOverheadNames = _checked;
                }
                else if (item == respawnDefaultCharacter)
                {
                    MiscRespawnDefaultCharacter = _checked;
                }
                else if (item == restorePlayerAppearance)
                {
                    RestorePlayerAppearance = _checked;
                }
                else if (item == restorePlayerWeapons)
                {
                    RestorePlayerWeapons = _checked;
                }

            };

            // Handle button presses.
            menu.OnItemSelect += (sender, item, index) =>
            {
                // save settings
                if (item == saveSettings)
                {
                    UserDefaults.SaveSettings();
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

        private struct Blip
        {
            public readonly Vector3 Location;
            public readonly int Sprite;
            public readonly string Name;
            public readonly int Color;
            public readonly int blipID;

            public Blip(Vector3 Location, int Sprite, string Name, int Color, int blipID)
            {
                this.Location = Location;
                this.Sprite = Sprite;
                this.Name = Name;
                this.Color = Color;
                this.blipID = blipID;
            }
        }

        private List<Blip> blips = new List<Blip>();

        /// <summary>
        /// Toggles blips on/off.
        /// </summary>
        /// <param name="enable"></param>
        private void ToggleBlips(bool enable)
        {
            if (enable)
            {
                try
                {
                    foreach (var bl in vMenuShared.ConfigManager.GetLocationBlipsData())
                    {
                        int blipID = AddBlipForCoord(bl.coordinates.X, bl.coordinates.Y, bl.coordinates.Z);
                        SetBlipSprite(blipID, bl.spriteID);
                        BeginTextCommandSetBlipName("STRING");
                        AddTextComponentSubstringPlayerName(bl.name);
                        EndTextCommandSetBlipName(blipID);
                        SetBlipColour(blipID, bl.color);
                        SetBlipAsShortRange(blipID, true);

                        Blip b = new Blip(bl.coordinates, bl.spriteID, bl.name, bl.color, blipID);
                        blips.Add(b);
                    }
                }
                catch (JsonReaderException ex)
                {
                    Debug.Write($"\n\n[vMenu] 加載locations.json文件時發生錯誤。 請與服務器服主聯繫以解決此問題。請提供以下錯誤詳細信息:\n{ex.Message}.\n\n\n");
                }
            }
            else
            {
                if (blips.Count > 0)
                {
                    foreach (Blip blip in blips)
                    {
                        int id = blip.blipID;
                        if (DoesBlipExist(id))
                        {
                            RemoveBlip(ref id);
                        }
                    }
                }
                blips.Clear();
            }
        }

    }
}
