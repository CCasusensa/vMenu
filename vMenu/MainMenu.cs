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
using static vMenuShared.ConfigManager;
using static vMenuShared.PermissionsManager;

namespace vMenuClient
{
    public class MainMenu : BaseScript
    {
        #region Variables
        //public static MenuPool Mp { get; } = new MenuPool();

        private bool firstTick = true;
        public static bool PermissionsSetupComplete => ArePermissionsSetup;
        public static bool ConfigOptionsSetupComplete = false;

        public static Control MenuToggleKey { get { return MenuController.MenuToggleKey; } private set { MenuController.MenuToggleKey = value; } } // M by default (InteractionMenu)
        public static int NoClipKey { get; private set; } = 289; // F2 by default (ReplayStartStopRecordingSecondary)
        public static Menu Menu { get; private set; }
        public static Menu PlayerSubmenu { get; private set; }
        public static Menu VehicleSubmenu { get; private set; }
        public static Menu WorldSubmenu { get; private set; }

        public static PlayerOptions PlayerOptionsMenu { get; private set; }
        public static OnlinePlayers OnlinePlayersMenu { get; private set; }
        public static BannedPlayers BannedPlayersMenu { get; private set; }
        public static SavedVehicles SavedVehiclesMenu { get; private set; }
        public static PersonalVehicle PersonalVehicleMenu { get; private set; }
        public static VehicleOptions VehicleOptionsMenu { get; private set; }
        public static VehicleSpawner VehicleSpawnerMenu { get; private set; }
        public static PlayerAppearance PlayerAppearanceMenu { get; private set; }
        public static MpPedCustomization MpPedCustomizationMenu { get; private set; }
        public static TimeOptions TimeOptionsMenu { get; private set; }
        public static WeatherOptions WeatherOptionsMenu { get; private set; }
        public static WeaponOptions WeaponOptionsMenu { get; private set; }
        public static WeaponLoadouts WeaponLoadoutsMenu { get; private set; }
        public static Recording RecordingMenu { get; private set; }
        public static MiscSettings MiscSettingsMenu { get; private set; }
        public static VoiceChat VoiceChatSettingsMenu { get; private set; }
        public static About AboutMenu { get; private set; }
        public static bool NoClipEnabled { get { return NoClip.IsNoclipActive(); } set { NoClip.SetNoclipActive(value); } }
        public static PlayerList PlayersList;

        // Only used when debugging is enabled:
        //private BarTimerBar bt = new BarTimerBar("Opening Menu");

        public static bool DebugMode = GetResourceMetadata(GetCurrentResourceName(), "client_debug_mode", 0) == "true" ? true : false;
        public static bool EnableExperimentalFeatures = /*true;*/ (GetResourceMetadata(GetCurrentResourceName(), "experimental_features_enabled", 0) ?? "0") == "1";
        public static string Version { get { return GetResourceMetadata(GetCurrentResourceName(), "version", 0); } }

        public static bool DontOpenMenus { get { return MenuController.DontOpenAnyMenu; } set { MenuController.DontOpenAnyMenu = value; } }
        public static bool DisableControls { get { return MenuController.DisableMenuButtons; } set { MenuController.DisableMenuButtons = value; } }

        private const int currentCleanupVersion = 2;
        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainMenu()
        {
            PlayersList = Players;

            #region cleanup unused kvps
            int tmp_kvp_handle = StartFindKvp("");
            bool cleanupVersionChecked = false;
            List<string> tmp_kvp_names = new List<string>();
            while (true)
            {
                string k = FindKvp(tmp_kvp_handle);
                if (string.IsNullOrEmpty(k))
                {
                    break;
                }
                if (k == "vmenu_cleanup_version")
                {
                    if (GetResourceKvpInt("vmenu_cleanup_version") >= currentCleanupVersion)
                    {
                        cleanupVersionChecked = true;
                    }
                }
                tmp_kvp_names.Add(k);
            }
            EndFindKvp(tmp_kvp_handle);

            if (!cleanupVersionChecked)
            {
                SetResourceKvpInt("vmenu_cleanup_version", currentCleanupVersion);
                foreach (string kvp in tmp_kvp_names)
                {
                    if (currentCleanupVersion == 1 || currentCleanupVersion == 2)
                    {
                        if (!kvp.StartsWith("settings_") && !kvp.StartsWith("vmenu") && !kvp.StartsWith("veh_") && !kvp.StartsWith("ped_") && !kvp.StartsWith("mp_ped_"))
                        {
                            DeleteResourceKvp(kvp);
                            Debug.WriteLine($"[vMenu] [cleanup id: 1] Removed unused (old) KVP: {kvp}.");
                        }
                    }
                    if (currentCleanupVersion == 2)
                    {
                        if (kvp.StartsWith("mp_char"))
                        {
                            DeleteResourceKvp(kvp);
                            Debug.WriteLine($"[vMenu] [cleanup id: 2] Removed unused (old) KVP: {kvp}.");
                        }
                    }
                }
                Debug.WriteLine("[vMenu] Cleanup of old unused KVP items completed.");
            }
            #endregion

            if (EnableExperimentalFeatures || DebugMode)
            {
                RegisterCommand("testped", new Action<dynamic, List<dynamic>, string>((dynamic source, List<dynamic> args, string rawCommand) =>
                {
                    PedHeadBlendData data = Game.PlayerPed.GetHeadBlendData();
                    Debug.WriteLine(JsonConvert.SerializeObject(data, Formatting.Indented));
                }), false);

                RegisterCommand("tattoo", new Action<dynamic, List<dynamic>, string>((dynamic source, List<dynamic> args, string rawCommand) =>
                {
                    if (args != null && args[0] != null && args[1] != null)
                    {
                        Debug.WriteLine(args[0].ToString() + " " + args[1].ToString());
                        TattooCollectionData d = Game.GetTattooCollectionData(int.Parse(args[0].ToString()), int.Parse(args[1].ToString()));
                        Debug.WriteLine("check");
                        Debug.Write(JsonConvert.SerializeObject(d, Formatting.Indented) + "\n");
                    }
                }), false);
            }

            RegisterCommand("vmenuclient", new Action<dynamic, List<dynamic>, string>((dynamic source, List<dynamic> args, string rawCommand) =>
            {
                if (args != null)
                {
                    if (args.Count > 0)
                    {
                        if (args[0].ToString().ToLower() == "debug")
                        {
                            DebugMode = !DebugMode;
                            Notify.Custom($"調試模式: {DebugMode}.");
                            // Set discord rich precense once, allowing it to be overruled by other resources once those load.
                            if (DebugMode)
                            {
                                SetRichPresence($"Debugging vMenu {Version}!");
                            }
                            else
                            {
                                SetRichPresence($"Enjoying FiveM!");
                            }
                        }
                        else if (args[0].ToString().ToLower() == "gc")
                        {
                            GC.Collect();
                            Debug.Write("Cleared memory.\n");
                        }
                        else if (args[0].ToString().ToLower() == "dump")
                        {
                            Notify.Info("將向控制台進行完整的配置轉儲。 檢查日誌文件。 這可能會導致卡頓!");
                            Debug.WriteLine("\n\n\n########################### vMenu ###########################");
                            Debug.WriteLine($"Running vMenu Version: {Version}, Experimental features: {EnableExperimentalFeatures}, Debug mode: {DebugMode}.");
                            Debug.WriteLine("\nDumping a list of all KVPs:");
                            int handle = StartFindKvp("");
                            List<string> names = new List<string>();
                            while (true)
                            {
                                string k = FindKvp(handle);
                                if (string.IsNullOrEmpty(k))
                                {
                                    break;
                                }
                                //if (!k.StartsWith("settings_") && !k.StartsWith("vmenu") && !k.StartsWith("veh_") && !k.StartsWith("ped_") && !k.StartsWith("mp_ped_"))
                                //{
                                //    DeleteResourceKvp(k);
                                //}
                                names.Add(k);
                            }
                            EndFindKvp(handle);

                            Dictionary<string, dynamic> kvps = new Dictionary<string, dynamic>();
                            foreach (var kvp in names)
                            {
                                int type = 0; // 0 = string, 1 = float, 2 = int.
                                if (kvp.StartsWith("settings_"))
                                {
                                    if (kvp == "settings_voiceChatProximity") // float
                                    {
                                        type = 1;
                                    }
                                    else if (kvp == "settings_clothingAnimationType") // int
                                    {
                                        type = 2;
                                    }
                                    else if (kvp == "settings_miscLastTimeCycleModifierIndex") // int
                                    {
                                        type = 2;
                                    }
                                    else if (kvp == "settings_miscLastTimeCycleModifierStrength") // int
                                    {
                                        type = 2;
                                    }
                                }
                                else if (kvp == "vmenu_cleanup_version") // int
                                {
                                    type = 2;
                                }
                                switch (type)
                                {
                                    case 0:
                                        var s = GetResourceKvpString(kvp);
                                        if (s.StartsWith("{") || s.StartsWith("["))
                                        {
                                            kvps.Add(kvp, JsonConvert.DeserializeObject(s));
                                        }
                                        else
                                        {
                                            kvps.Add(kvp, GetResourceKvpString(kvp));
                                        }
                                        break;
                                    case 1:
                                        kvps.Add(kvp, GetResourceKvpFloat(kvp));
                                        break;
                                    case 2:
                                        kvps.Add(kvp, GetResourceKvpInt(kvp));
                                        break;
                                }
                            }
                            Debug.WriteLine(@JsonConvert.SerializeObject(kvps, Formatting.None) + "\n");

                            Debug.WriteLine("\n\nDumping a list of allowed permissions:");
                            Debug.WriteLine(@JsonConvert.SerializeObject(Permissions, Formatting.None));

                            Debug.WriteLine("\n\nDumping vmenu server configuration settings:");
                            var settings = new Dictionary<string, string>();
                            foreach (var a in Enum.GetValues(typeof(Setting)))
                            {
                                settings.Add(a.ToString(), GetSettingsString((Setting)a));
                            }
                            Debug.WriteLine(@JsonConvert.SerializeObject(settings, Formatting.None));
                            Debug.WriteLine("\nEnd of vMenu dump!");
                            Debug.WriteLine("\n########################### vMenu ###########################");
                        }
                    }
                    else
                    {
                        Notify.Custom($"vMenu is currently running version: {Version}.");
                    }
                }
            }), false);

            // Set discord rich precense once, allowing it to be overruled by other resources once those load.
            if (DebugMode)
            {
                SetRichPresence($"Debugging vMenu {Version}!");
            }

            if (GetCurrentResourceName() != "vMenu")
            {
                Exception InvalidNameException = new Exception("\r\n\r\n[vMenu] INSTALLATION ERROR!\r\nThe name of the resource is not valid. Please change the folder name from '" + GetCurrentResourceName() + "' to 'vMenu' (case sensitive) instead!\r\n\r\n\r\n");
                try
                {
                    throw InvalidNameException;
                }
                catch (Exception e)
                {
                    Log(e.Message);
                }
                TriggerEvent("chatMessage", "^3IMPORTANT: vMenu IS NOT SETUP CORRECTLY. PLEASE CHECK THE SERVER LOG FOR MORE INFO.");
                MenuController.MainMenu = null;
                MenuController.DontOpenAnyMenu = true;
                MenuController.DisableMenuButtons = true;
            }
            else
            {
                Tick += OnTick;
            }
            try
            {
                SetClockDate(DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year);
            }
            catch (InvalidTimeZoneException timeEx)
            {
                Debug.WriteLine($"[vMenu] [Error] Could not set the in-game day, month and year because of an invalid timezone(?).");
                Debug.WriteLine($"[vMenu] [Error] InvalidTimeZoneException: {timeEx.Message}");
                Debug.WriteLine($"[vMenu] [Error] vMenu will continue to work normally.");
            }
        }

        #region Set Permissions function
        /// <summary>
        /// Set the permissions for this client.
        /// </summary>
        /// <param name="dict"></param>
        public static void SetPermissions(string permissionsList)
        {
            vMenuShared.PermissionsManager.SetPermissions(permissionsList);

            VehicleSpawner.allowedCategories = new List<bool>()
            {
                IsAllowed(Permission.VSCompacts, checkAnyway: true),
                IsAllowed(Permission.VSSedans, checkAnyway: true),
                IsAllowed(Permission.VSSUVs, checkAnyway: true),
                IsAllowed(Permission.VSCoupes, checkAnyway: true),
                IsAllowed(Permission.VSMuscle, checkAnyway: true),
                IsAllowed(Permission.VSSportsClassic, checkAnyway: true),
                IsAllowed(Permission.VSSports, checkAnyway: true),
                IsAllowed(Permission.VSSuper, checkAnyway: true),
                IsAllowed(Permission.VSMotorcycles, checkAnyway: true),
                IsAllowed(Permission.VSOffRoad, checkAnyway: true),
                IsAllowed(Permission.VSIndustrial, checkAnyway: true),
                IsAllowed(Permission.VSUtility, checkAnyway: true),
                IsAllowed(Permission.VSVans, checkAnyway: true),
                IsAllowed(Permission.VSCycles, checkAnyway: true),
                IsAllowed(Permission.VSBoats, checkAnyway: true),
                IsAllowed(Permission.VSHelicopters, checkAnyway: true),
                IsAllowed(Permission.VSPlanes, checkAnyway: true),
                IsAllowed(Permission.VSService, checkAnyway: true),
                IsAllowed(Permission.VSEmergency, checkAnyway: true),
                IsAllowed(Permission.VSMilitary, checkAnyway: true),
                IsAllowed(Permission.VSCommercial, checkAnyway: true),
                IsAllowed(Permission.VSTrains, checkAnyway: true),
            };
            ArePermissionsSetup = true;

            TriggerServerEvent("vMenu:IsResourceUpToDate");
        }
        #endregion


        /// <summary>
        /// Main OnTick task runs every game tick and handles all the menu stuff.
        /// </summary>
        /// <returns></returns>
        private async Task OnTick()
        {
            #region FirstTick
            // Only run this the first tick.
            if (firstTick)
            {
                firstTick = false;
                switch (GetSettingsInt(Setting.vmenu_pvp_mode))
                {
                    case 1:
                        NetworkSetFriendlyFireOption(true);
                        SetCanAttackFriendly(Game.PlayerPed.Handle, true, false);
                        break;
                    case 2:
                        NetworkSetFriendlyFireOption(false);
                        SetCanAttackFriendly(Game.PlayerPed.Handle, false, false);
                        break;
                    case 0:
                    default:
                        break;
                }
                // Clear all previous pause menu info/brief messages on resource start.
                ClearBrief();

                // Request the permissions data from the server.
                TriggerServerEvent("vMenu:RequestPermissions");

                // Wait until the data is received and the player's name is loaded correctly.
                while (!ConfigOptionsSetupComplete || !PermissionsSetupComplete || Game.Player.Name == "**Invalid**" || Game.Player.Name == "** Invalid **")
                {
                    await Delay(0);
                }
                if ((IsAllowed(Permission.Staff) && GetSettingsBool(Setting.vmenu_menu_staff_only)) || GetSettingsBool(Setting.vmenu_menu_staff_only) == false)
                {
                    if (GetSettingsInt(Setting.vmenu_menu_toggle_key) != -1)
                    {
                        MenuToggleKey = (Control)GetSettingsInt(Setting.vmenu_menu_toggle_key);
                        //MenuToggleKey = GetSettingsInt(Setting.vmenu_menu_toggle_key);
                    }
                    if (GetSettingsInt(Setting.vmenu_noclip_toggle_key) != -1)
                    {
                        NoClipKey = GetSettingsInt(Setting.vmenu_noclip_toggle_key);
                    }

                    // Create the main menu.
                    Menu = new Menu(Game.Player.Name, "主選單");
                    PlayerSubmenu = new Menu(Game.Player.Name, "玩家相關選項");
                    VehicleSubmenu = new Menu(Game.Player.Name, "載具相關選項");
                    WorldSubmenu = new Menu(Game.Player.Name, "世界選項");

                    // Add the main menu to the menu pool.
                    MenuController.AddMenu(Menu);
                    MenuController.MainMenu = Menu;

                    MenuController.AddSubmenu(Menu, PlayerSubmenu);
                    MenuController.AddSubmenu(Menu, VehicleSubmenu);
                    MenuController.AddSubmenu(Menu, WorldSubmenu);

                    // Create all (sub)menus.
                    CreateSubmenus();
                }
                else
                {
                    MenuController.MainMenu = null;
                    MenuController.DisableMenuButtons = true;
                    MenuController.DontOpenAnyMenu = true;
                    MenuController.MenuToggleKey = (Control)(-1); // disables the menu toggle key
                }

                // Manage Stamina
                if (PlayerOptionsMenu != null && PlayerOptionsMenu.PlayerStamina && IsAllowed(Permission.POUnlimitedStamina))
                    StatSetInt((uint)GetHashKey("MP0_STAMINA"), 100, true);
                else
                    StatSetInt((uint)GetHashKey("MP0_STAMINA"), 0, true);

                // Manage other stats, in order of appearance in the pause menu (stats) page.
                StatSetInt((uint)GetHashKey("MP0_SHOOTING_ABILITY"), 100, true);        // Shooting
                StatSetInt((uint)GetHashKey("MP0_STRENGTH"), 100, true);                // Strength
                StatSetInt((uint)GetHashKey("MP0_STEALTH_ABILITY"), 100, true);         // Stealth
                StatSetInt((uint)GetHashKey("MP0_FLYING_ABILITY"), 100, true);          // Flying
                StatSetInt((uint)GetHashKey("MP0_WHEELIE_ABILITY"), 100, true);         // Driving
                StatSetInt((uint)GetHashKey("MP0_LUNG_CAPACITY"), 100, true);           // Lung Capacity
                StatSetFloat((uint)GetHashKey("MP0_PLAYER_MENTAL_STATE"), 0f, true);    // Mental State

            }
            #endregion


            // If the setup (permissions) is done, and it's not the first tick, then do this:
            if (ConfigOptionsSetupComplete && !firstTick)
            {
                #region Handle Opening/Closing of the menu.


                var tmpMenu = GetOpenMenu();
                if (MpPedCustomizationMenu != null)
                {
                    bool IsOpen()
                    {
                        return
                            MpPedCustomizationMenu.appearanceMenu.Visible ||
                            MpPedCustomizationMenu.faceShapeMenu.Visible ||
                            MpPedCustomizationMenu.createCharacterMenu.Visible ||
                            MpPedCustomizationMenu.inheritanceMenu.Visible ||
                            MpPedCustomizationMenu.propsMenu.Visible ||
                            MpPedCustomizationMenu.clothesMenu.Visible ||
                            MpPedCustomizationMenu.tattoosMenu.Visible;
                    }

                    if (IsOpen())
                    {
                        if (tmpMenu == MpPedCustomizationMenu.createCharacterMenu)
                        {
                            MpPedCustomization.DisableBackButton = true;
                        }
                        else
                        {
                            MpPedCustomization.DisableBackButton = false;
                        }
                        MpPedCustomization.DontCloseMenus = true;
                    }
                    else
                    {
                        MpPedCustomization.DisableBackButton = false;
                        MpPedCustomization.DontCloseMenus = false;
                    }
                }

                if (Game.IsDisabledControlJustReleased(0, Control.PhoneCancel) && MpPedCustomization.DisableBackButton)
                {
                    await Delay(0);
                    Notify.Alert("退出之前，您必須先保存人物模型，或點擊~r~退出而不保存的按鈕。");
                }

                if (Game.CurrentInputMode == InputMode.MouseAndKeyboard)
                {
                    if (Game.IsControlJustPressed(0, (Control)NoClipKey) && IsAllowed(Permission.NoClip) && UpdateOnscreenKeyboard() != 0)
                    {
                        if (Game.PlayerPed.IsInVehicle())
                        {
                            Vehicle veh = GetVehicle();
                            if (veh != null && veh.Exists() && veh.Driver == Game.PlayerPed)
                            {
                                NoClipEnabled = !NoClipEnabled;
                            }
                            else
                            {
                                NoClipEnabled = false;
                                Notify.Error("該載具不存在(以某種方式)，或者您需要成為該載具的駕駛員才能啟用noclip!");
                            }
                        }
                        else
                        {
                            NoClipEnabled = !NoClipEnabled;
                        }
                    }
                }

                #endregion

                // Menu toggle button.
                Game.DisableControlThisFrame(0, MenuToggleKey);
            }
        }

        #region Add Menu Function
        /// <summary>
        /// Add the menu to the menu pool and set it up correctly.
        /// Also add and bind the menu buttons.
        /// </summary>
        /// <param name="submenu"></param>
        /// <param name="menuButton"></param>
        private void AddMenu(Menu parentMenu, Menu submenu, MenuItem menuButton)
        {
            parentMenu.AddMenuItem(menuButton);
            MenuController.AddSubmenu(parentMenu, submenu);
            MenuController.BindMenuItem(parentMenu, submenu, menuButton);
            submenu.RefreshIndex();
        }
        #endregion

        #region Create Submenus
        /// <summary>
        /// Creates all the submenus depending on the permissions of the user.
        /// </summary>
        private void CreateSubmenus()
        {
            // Add the online players menu.
            if (IsAllowed(Permission.OPMenu))
            {
                OnlinePlayersMenu = new OnlinePlayers();
                Menu menu = OnlinePlayersMenu.GetMenu();
                MenuItem button = new MenuItem("線上玩家列表", "當前在伺服器的所有玩家")
                {
                    Label = "→→→"
                };
                AddMenu(Menu, menu, button);
                Menu.OnItemSelect += (sender, item, index) =>
                {
                    if (item == button)
                    {
                        OnlinePlayersMenu.UpdatePlayerlist();
                        menu.RefreshIndex();
                    }
                };
            }
            if (IsAllowed(Permission.OPUnban) || IsAllowed(Permission.OPViewBannedPlayers))
            {
                BannedPlayersMenu = new BannedPlayers();
                Menu menu = BannedPlayersMenu.GetMenu();
                MenuItem button = new MenuItem("封鎖玩家列表", "當前在伺服器被封鎖的玩家名單")
                {
                    Label = "→→→"
                };
                AddMenu(Menu, menu, button);
                Menu.OnItemSelect += (sender, item, index) =>
                {
                    if (item == button)
                    {
                        TriggerServerEvent("vMenu:RequestBanList", Game.Player.Handle);
                        menu.RefreshIndex();
                    }
                };
            }

            MenuItem playerSubmenuBtn = new MenuItem("玩家相關選項", "查看與玩家相關的功能") { Label = "→→→" };
            Menu.AddMenuItem(playerSubmenuBtn);

            // Add the player options menu.
            if (IsAllowed(Permission.POMenu))
            {
                PlayerOptionsMenu = new PlayerOptions();
                Menu menu = PlayerOptionsMenu.GetMenu();
                MenuItem button = new MenuItem("玩家選項", "可以查看常見的玩家功能.")
                {
                    Label = "→→→"
                };
                AddMenu(PlayerSubmenu, menu, button);
            }

            MenuItem vehicleSubmenuBtn = new MenuItem("載具相關選項", "打開此子選單以獲取與載具相關的子類別.") { Label = "→→→" };
            Menu.AddMenuItem(vehicleSubmenuBtn);
            // Add the vehicle options Menu.
            if (IsAllowed(Permission.VOMenu))
            {
                VehicleOptionsMenu = new VehicleOptions();
                Menu menu = VehicleOptionsMenu.GetMenu();
                MenuItem button = new MenuItem("載具相關選項", "您可以更改常用的載具選項，以及調整和样式化您的載具.")
                {
                    Label = "→→→"
                };
                AddMenu(VehicleSubmenu, menu, button);
            }

            // Add the vehicle spawner menu.
            if (IsAllowed(Permission.VSMenu))
            {
                VehicleSpawnerMenu = new VehicleSpawner();
                Menu menu = VehicleSpawnerMenu.GetMenu();
                MenuItem button = new MenuItem("載具召喚", "召喚出任何一台載具.")
                {
                    Label = "→→→"
                };
                AddMenu(VehicleSubmenu, menu, button);
            }

            // Add Saved Vehicles menu.
            if (IsAllowed(Permission.SVMenu))
            {
                SavedVehiclesMenu = new SavedVehicles();
                Menu menu = SavedVehiclesMenu.GetMenu();
                MenuItem button = new MenuItem("載具管理", "可以用來管你的載具.")
                {
                    Label = "→→→"
                };
                AddMenu(VehicleSubmenu, menu, button);
                VehicleSubmenu.OnItemSelect += (sender, item, index) =>
                {
                    if (item == button)
                    {
                        SavedVehiclesMenu.UpdateMenuAvailableCategories();
                    }
                };
            }

            // Add the Personal Vehicle menu.
            if (IsAllowed(Permission.PVMenu))
            {
                PersonalVehicleMenu = new PersonalVehicle();
                Menu menu = PersonalVehicleMenu.GetMenu();
                MenuItem button = new MenuItem("私人車輛", "將一輛車設置為私人車輛，不再室內時還可以對該車輛進行某些操作.")
                {
                    Label = "→→→"
                };
                AddMenu(VehicleSubmenu, menu, button);
            }

            // Add the player appearance menu.
            if (IsAllowed(Permission.PAMenu))
            {
                PlayerAppearanceMenu = new PlayerAppearance();
                Menu menu = PlayerAppearanceMenu.GetMenu();
                MenuItem button = new MenuItem("玩家容貌", "選擇一個PED模型，對其進行自定義，儲存加載角色.")
                {
                    Label = "→→→"
                };
                AddMenu(PlayerSubmenu, menu, button);

                MpPedCustomizationMenu = new MpPedCustomization();
                Menu menu2 = MpPedCustomizationMenu.GetMenu();
                MenuItem button2 = new MenuItem("人物外觀選項", "創建、編輯、保存和加載外觀。 ~r~注意，您只能保存在此子選單中創建的外觀。 vMenu無法檢測到在此子選單之外創建的外觀。完全是由於GTA限制.")
                {
                    Label = "→→→"
                };
                AddMenu(PlayerSubmenu, menu2, button2);
            }

            MenuItem worldSubmenuBtn = new MenuItem("世界相關選項", "打開與世界相關的子類別的子選單.") { Label = "→→→" };
            Menu.AddMenuItem(worldSubmenuBtn);

            // Add the time options menu.
            // check for 'not true' to make sure that it _ONLY_ gets disabled if the owner _REALLY_ wants it disabled, not if they accidentally spelled "false" wrong or whatever.
            if (IsAllowed(Permission.TOMenu) && GetSettingsBool(Setting.vmenu_enable_time_sync))
            {
                TimeOptionsMenu = new TimeOptions();
                Menu menu = TimeOptionsMenu.GetMenu();
                MenuItem button = new MenuItem("時間選項", "更改時間，並編輯其他與時間相關的選項.")
                {
                    Label = "→→→"
                };
                AddMenu(WorldSubmenu, menu, button);
            }

            // Add the weather options menu.
            // check for 'not true' to make sure that it _ONLY_ gets disabled if the owner _REALLY_ wants it disabled, not if they accidentally spelled "false" wrong or whatever.
            if (IsAllowed(Permission.WOMenu) && GetSettingsBool(Setting.vmenu_enable_weather_sync))
            {
                WeatherOptionsMenu = new WeatherOptions();
                Menu menu = WeatherOptionsMenu.GetMenu();
                MenuItem button = new MenuItem("天氣選項", "在此處更改所有與天氣相關的選項.")
                {
                    Label = "→→→"
                };
                AddMenu(WorldSubmenu, menu, button);
            }

            // Add the weapons menu.
            if (IsAllowed(Permission.WPMenu))
            {
                WeaponOptionsMenu = new WeaponOptions();
                Menu menu = WeaponOptionsMenu.GetMenu();
                MenuItem button = new MenuItem("武器選項", "添加/刪除武器，修改武器並設置彈藥選項.")
                {
                    Label = "→→→"
                };
                AddMenu(PlayerSubmenu, menu, button);
            }

            // Add Weapon Loadouts menu.
            if (IsAllowed(Permission.WLMenu))
            {
                WeaponLoadoutsMenu = new WeaponLoadouts();
                Menu menu = WeaponLoadoutsMenu.GetMenu();
                MenuItem button = new MenuItem("武器裝載量", "管理，並生成節省的武器裝載.")
                {
                    Label = "→→→"
                };
                AddMenu(PlayerSubmenu, menu, button);
            }

            if (IsAllowed(Permission.NoClip))
            {
                MenuItem toggleNoclip = new MenuItem("切換 NoClip", "切換 NoClip 啟用或停用.");
                PlayerSubmenu.AddMenuItem(toggleNoclip);
                PlayerSubmenu.OnItemSelect += (sender, item, index) =>
                {
                    if (item == toggleNoclip)
                    {
                        NoClipEnabled = !NoClipEnabled;
                    }
                };
            }

            // Add Voice Chat Menu.
            if (IsAllowed(Permission.VCMenu))
            {
                VoiceChatSettingsMenu = new VoiceChat();
                Menu menu = VoiceChatSettingsMenu.GetMenu();
                MenuItem button = new MenuItem("語音聊天設置", "在此處更改語音聊天選項.")
                {
                    Label = "→→→"
                };
                AddMenu(Menu, menu, button);
            }

            {
                RecordingMenu = new Recording();
                Menu menu = RecordingMenu.GetMenu();
                MenuItem button = new MenuItem("錄影選項", "遊戲中的錄製選項.")
                {
                    Label = "→→→"
                };
                AddMenu(Menu, menu, button);
            }

            // Add misc settings menu.
            {
                MiscSettingsMenu = new MiscSettings();
                Menu menu = MiscSettingsMenu.GetMenu();
                MenuItem button = new MenuItem("其他設定", "可以在此處配置其他vMenu選項/設置。您也可以在此選單中保存設置.")
                {
                    Label = "→→→"
                };
                AddMenu(Menu, menu, button);
            }

            // Add About Menu.
            AboutMenu = new About();
            Menu sub = AboutMenu.GetMenu();
            MenuItem btn = new MenuItem("關於vMenu", "介紹 vMenu.")
            {
                Label = "→→→"
            };
            AddMenu(Menu, sub, btn);

            // Refresh everything.
            MenuController.Menus.ForEach((m) => m.RefreshIndex());

            if (!GetSettingsBool(Setting.vmenu_use_permissions))
            {
                Notify.Alert("vMenu設置為忽略權限，將使用預設權限.");
            }

            if (PlayerSubmenu.Size > 0)
            {
                MenuController.BindMenuItem(Menu, PlayerSubmenu, playerSubmenuBtn);
            }
            else
            {
                Menu.RemoveMenuItem(playerSubmenuBtn);
            }

            if (VehicleSubmenu.Size > 0)
            {
                MenuController.BindMenuItem(Menu, VehicleSubmenu, vehicleSubmenuBtn);
            }
            else
            {
                Menu.RemoveMenuItem(vehicleSubmenuBtn);
            }

            if (WorldSubmenu.Size > 0)
            {
                MenuController.BindMenuItem(Menu, WorldSubmenu, worldSubmenuBtn);
            }
            else
            {
                Menu.RemoveMenuItem(worldSubmenuBtn);
            }

            if (MiscSettingsMenu != null)
            {
                MenuController.EnableMenuToggleKeyOnController = !MiscSettingsMenu.MiscDisableControllerSupport;
            }
        }
        #endregion
    }
}
