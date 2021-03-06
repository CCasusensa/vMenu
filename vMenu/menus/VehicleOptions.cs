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
    public class VehicleOptions
    {
        #region Variables
        // Menu variable, will be defined in CreateMenu()
        private Menu menu;

        // Submenus
        public Menu VehicleModMenu { get; private set; }
        public Menu VehicleDoorsMenu { get; private set; }
        public Menu VehicleWindowsMenu { get; private set; }
        public Menu VehicleComponentsMenu { get; private set; }
        public Menu VehicleLiveriesMenu { get; private set; }
        public Menu VehicleColorsMenu { get; private set; }
        public Menu DeleteConfirmMenu { get; private set; }
        public Menu VehicleUnderglowMenu { get; private set; }

        // Public variables (getters only), return the private variables.
        public bool VehicleGodMode { get; private set; } = UserDefaults.VehicleGodMode;
        public bool VehicleGodInvincible { get; private set; } = UserDefaults.VehicleGodInvincible;
        public bool VehicleGodEngine { get; private set; } = UserDefaults.VehicleGodEngine;
        public bool VehicleGodVisual { get; private set; } = UserDefaults.VehicleGodVisual;
        public bool VehicleGodStrongWheels { get; private set; } = UserDefaults.VehicleGodStrongWheels;
        public bool VehicleGodRamp { get; private set; } = UserDefaults.VehicleGodRamp;
        public bool VehicleGodAutoRepair { get; private set; } = UserDefaults.VehicleGodAutoRepair;

        public bool VehicleNeverDirty { get; private set; } = UserDefaults.VehicleNeverDirty;
        public bool VehicleEngineAlwaysOn { get; private set; } = UserDefaults.VehicleEngineAlwaysOn;
        public bool VehicleNoSiren { get; private set; } = UserDefaults.VehicleNoSiren;
        public bool VehicleNoBikeHelemet { get; private set; } = UserDefaults.VehicleNoBikeHelmet;
        public bool FlashHighbeamsOnHonk { get; private set; } = UserDefaults.VehicleHighbeamsOnHonk;
        public bool DisablePlaneTurbulence { get; private set; } = UserDefaults.VehicleDisablePlaneTurbulence;
        public bool VehicleBikeSeatbelt { get; private set; } = UserDefaults.VehicleBikeSeatbelt;
        public bool VehicleInfiniteFuel { get; private set; } = false;
        public bool VehicleShowHealth { get; private set; } = false;
        public bool VehicleFrozen { get; private set; } = false;
        public bool VehicleTorqueMultiplier { get; private set; } = false;
        public bool VehiclePowerMultiplier { get; private set; } = false;
        public float VehicleTorqueMultiplierAmount { get; private set; } = 2f;
        public float VehiclePowerMultiplierAmount { get; private set; } = 2f;

        private Dictionary<MenuItem, int> vehicleExtras = new Dictionary<MenuItem, int>();
        #endregion

        #region CreateMenu()
        /// <summary>
        /// Create menu creates the vehicle options menu.
        /// </summary>
        private void CreateMenu()
        {
            // Create the menu.
            menu = new Menu(Game.Player.Name, "載具選單");

            #region menu items variables
            // vehicle god mode menu
            Menu vehGodMenu = new Menu("載具無敵", "載具無敵選單");
            MenuItem vehGodMenuBtn = new MenuItem("無敵選單", "啟用或禁用特定的損壞類型.") { Label = "→→→" };
            MenuController.AddSubmenu(menu, vehGodMenu);

            // Create Checkboxes.
		    MenuCheckboxItem vehicleGod = new MenuCheckboxItem("載具無敵模式", "使您的載具不受任何損壞。請注意，您需要進入下面的無敵選項，以選擇要禁用的損害類型.", VehicleGodMode);
	    	MenuCheckboxItem vehicleNeverDirty = new MenuCheckboxItem("保持載具清潔", "如果載具的灰塵水平超過0，這將不斷清潔您的汽車。請注意，這只會清潔~o~灰塵~s~或~o~污垢~s~。這不能清潔泥土，雪或其他損壞貼花。修理您的車輛以將其卸下.", VehicleNeverDirty);
	    	MenuCheckboxItem vehicleBikeSeatbelt = new MenuCheckboxItem("自行車防摔", "防止您從自行車上撞下來.", VehicleBikeSeatbelt);
	    	MenuCheckboxItem vehicleEngineAO = new MenuCheckboxItem("引擎常開", "退出載具時，保持載具引擎運轉.", VehicleEngineAlwaysOn);
	    	MenuCheckboxItem vehicleNoTurbulence = new MenuCheckboxItem("飛機湍流", "禁用所有飛機的湍流。注意僅適用於飛機。不支持直升機和其他飛行器.", DisablePlaneTurbulence);
	    	MenuCheckboxItem vehicleNoSiren = new MenuCheckboxItem("禁用警笛", "禁用載具的警報器。僅在您的載具上有警笛時才起作用.", VehicleNoSiren);
	    	MenuCheckboxItem vehicleNoBikeHelmet = new MenuCheckboxItem("停用自行車頭盔", "騎自行車或四輪摩托時不再自動裝備頭盔.", VehicleNoBikeHelemet);
	    	MenuCheckboxItem vehicleFreeze = new MenuCheckboxItem("凍結載具", "凍結載具的位置.", VehicleFrozen);
	    	MenuCheckboxItem torqueEnabled = new MenuCheckboxItem("啟用扭矩倍增器", "啟用從以下列表中選擇的扭矩倍增器.", VehicleTorqueMultiplier);
	    	MenuCheckboxItem powerEnabled = new MenuCheckboxItem("啟用功率倍增器", "啟用從以下列表中選擇的功率倍增器.", VehiclePowerMultiplier);
	    	MenuCheckboxItem highbeamsOnHonk = new MenuCheckboxItem("鳴笛閃光遠光", "鳴喇叭時，打開載具上的遠光燈。在白天關閉燈光時不起作用.", FlashHighbeamsOnHonk);
	    	MenuCheckboxItem showHealth = new MenuCheckboxItem("顯示車載具久度", "顯示載具耐久度.", VehicleShowHealth);
	    	MenuCheckboxItem infiniteFuel = new MenuCheckboxItem("無限燃料", "啟用或禁用此載具的無限燃料，僅在安裝了FRFuel的情況下有效.", VehicleInfiniteFuel);

            // Create buttons.
		    MenuItem fixVehicle = new MenuItem("維修載具", "修理載具上出現的任何視覺和物理損壞.");
		    MenuItem cleanVehicle = new MenuItem("洗車", "清洗您的載具.");
		    MenuItem toggleEngine = new MenuItem("切換引擎開/關", "打開/關閉引擎.");
		    MenuItem setLicensePlateText = new MenuItem("設置車牌文字", "輸入載具的自定義車牌.");
		    MenuItem modMenuBtn = new MenuItem("載具定義選項", "這裡調整和自定義您的載具.")
            {
                Label = "→→→"
            };
            MenuItem doorsMenuBtn = new MenuItem("載具門", "在這裡打開，關閉，拆除和恢復載具門.")
            {
                Label = "→→→"
            };
            MenuItem windowsMenuBtn = new MenuItem("載具窗戶", "上下搖動窗戶或在此處刪除/恢復載具窗戶.")
            {
                Label = "→→→"
            };
            MenuItem componentsMenuBtn = new MenuItem("載具附件", "添加/刪除車輛部件/附件.")
            {
                Label = "→→→"
            };
            MenuItem liveriesMenuBtn = new MenuItem("載具配件", "用奇特的配件來裝飾您的載具!")
            {
                Label = "→→→"
            };
            MenuItem colorsMenuBtn = new MenuItem("載具顏色", "給載具添加一些 ~g~ 令人陶醉的 ~s~ 顏色，為您的汽車增添風格！")
            {
                Label = "→→→"
            };
            MenuItem underglowMenuBtn = new MenuItem("載具霓虹燈套件", "讓您的載具熠熠生輝，充滿霓虹色彩！")
            {
                Label = "→→→"
            };
            MenuItem vehicleInvisible = new MenuItem("切換載具可見性", "使您的車輛可見/不可見。 ~r~您若離開車輛將再次顯示，否則您將無法重新進入.");
            MenuItem flipVehicle = new MenuItem("車輪設定", "將您當前的載具設置在所有四個車輪上.");
            MenuItem vehicleAlarm = new MenuItem("切換車輛警報", "啟動/停止車輛警報.");
            MenuItem cycleSeats = new MenuItem("選擇汽車座椅", "選擇可用的汽車座椅.");
            List<string> lights = new List<string>()
            {
			    "警示燈",
			    "左轉燈",
		    	"右轉燈",
                "室內燈",
                //"Taxi Light", // this doesn't seem to work no matter what.
                "直升機聚光燈",
            };
            MenuListItem vehicleLights = new MenuListItem("載具燈", lights, 0, "打開/關閉載具燈");


            var tiresList = new List<string>() { "所有輪胎", "輪胎 #1", "輪胎 #2", "輪胎 #3", "輪胎 #4", "輪胎 #5", "輪胎 #6", "輪胎 #7", "輪胎 #8" };
            MenuListItem vehicleTiresList = new MenuListItem("修理/銷毀輪胎", tiresList, 0, "修理或銷毀特定的汽車輪胎，或一次修復所有輪胎。請注意，並非所有指令都對所有車輛都有效.");

            MenuItem deleteBtn = new MenuItem("~r~刪除車輛", "刪除您的車輛，這個~r~無法撤消~s~！")
            {
                LeftIcon = MenuItem.Icon.WARNING,
                Label = "→→→"
            };
            MenuItem deleteNoBtn = new MenuItem("不，取消", "不，請勿刪除我的車輛並返回！");
            MenuItem deleteYesBtn = new MenuItem("~r~是的，刪除", "是的，我確定，請刪除我的車輛，我知道這無法撤消.")
            {
                LeftIcon = MenuItem.Icon.WARNING
            };

            // Create lists.
            var dirtlevel = new List<string> { "沒有污垢", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15" };
            MenuListItem setDirtLevel = new MenuListItem("設定汙垢等級", dirtlevel, 0, "選擇您的車輛上應該看到多少污垢，按~r~enter~s~ “應用所選級別");
            var licensePlates = new List<string> { GetLabelText("CMOD_PLA_0"), GetLabelText("CMOD_PLA_1"), GetLabelText("CMOD_PLA_2"), GetLabelText("CMOD_PLA_3"),
                GetLabelText("CMOD_PLA_4"), "North Yankton" };
            MenuListItem setLicensePlateType = new MenuListItem("車牌類型", licensePlates, 0, "選擇車牌類型在您的載具上按~r~輸入~s~進行申請");
            var torqueMultiplierList = new List<string> { "x2", "x4", "x8", "x16", "x32", "x64", "x128", "x256", "x512", "x1024" };
            MenuListItem torqueMultiplier = new MenuListItem("設置引擎扭矩倍增器", torqueMultiplierList, 0, "設置引擎扭矩倍增器.");
            var powerMultiplierList = new List<string> { "x2", "x4", "x8", "x16", "x32", "x64", "x128", "x256", "x512", "x1024" };
            MenuListItem powerMultiplier = new MenuListItem("設定引擎功率倍增器", powerMultiplierList, 0, "設定引擎功率倍增器.");
            List<string> speedLimiterOptions = new List<string>() { "設定", "重設", "自定義速度限制" };
            MenuListItem speedLimiter = new MenuListItem("限速", speedLimiterOptions, 0, "將車輛的最高速度設置為~y~當前速度~s~。若重置車輛最大速度，則恢復為預設值。此選項僅影響您當前的車輛.");
            #endregion

            #region Submenus
            // Submenu's
            VehicleModMenu = new Menu("模組選單", "載具模組");
            VehicleModMenu.InstructionalButtons.Add(Control.Jump, "開關載具門");
            VehicleModMenu.ButtonPressHandlers.Add(new Menu.ButtonPressHandler(Control.Jump, Menu.ControlPressCheckType.JUST_PRESSED, new Action<Menu, Control>((m, c) =>
            {
                Vehicle veh = GetVehicle();
                if (veh != null && veh.Exists() && !veh.IsDead && veh.Driver == Game.PlayerPed)
                {
                    var open = GetVehicleDoorAngleRatio(veh.Handle, 0) < 0.1f;
                    if (open)
                    {
                        for (var i = 0; i < 8; i++)
                        {
                            SetVehicleDoorOpen(veh.Handle, i, false, false);
                        }
                    }
                    else
                    {
                        SetVehicleDoorsShut(veh.Handle, false);
                    }
                }
            }), false));
            VehicleDoorsMenu = new Menu("載具門", "載具門設定");
            VehicleWindowsMenu = new Menu("載去窗戶", "載具窗戶設定");
            VehicleComponentsMenu = new Menu("載具附加", "載具附加件/組件");
            VehicleLiveriesMenu = new Menu("載具配件", "載具配件");
            VehicleColorsMenu = new Menu("載具顏色", "載具顏色");
            DeleteConfirmMenu = new Menu("確認動作", "您確定刪除該載具?");
            VehicleUnderglowMenu = new Menu("霓載具虹燈套件", "載具霓虹燈下發光選項");

            MenuController.AddSubmenu(menu, VehicleModMenu);
            MenuController.AddSubmenu(menu, VehicleDoorsMenu);
            MenuController.AddSubmenu(menu, VehicleWindowsMenu);
            MenuController.AddSubmenu(menu, VehicleComponentsMenu);
            MenuController.AddSubmenu(menu, VehicleLiveriesMenu);
            MenuController.AddSubmenu(menu, VehicleColorsMenu);
            MenuController.AddSubmenu(menu, DeleteConfirmMenu);
            MenuController.AddSubmenu(menu, VehicleUnderglowMenu);
            #endregion

            #region Add items to the menu.
            // Add everything to the menu. (based on permissions)
            if (IsAllowed(Permission.VOGod)) // GOD MODE
            {
                menu.AddMenuItem(vehicleGod);
                menu.AddMenuItem(vehGodMenuBtn);
                MenuController.BindMenuItem(menu, vehGodMenu, vehGodMenuBtn);

                MenuCheckboxItem godInvincible = new MenuCheckboxItem("無敵", "使載具立於無敵。不會受到任何傷害，包括火災破壞，爆炸破壞，碰撞破壞等.", VehicleGodInvincible);
                MenuCheckboxItem godEngine = new MenuCheckboxItem("引擎損壞", "使您的引擎免受損壞.", VehicleGodEngine);
                MenuCheckboxItem godVisual = new MenuCheckboxItem("視覺傷害", "這樣可以防止划痕和其他損壞貼紙貼在您的車輛上。 它不能防止（身體）變形損壞.", VehicleGodVisual);
                MenuCheckboxItem godStrongWheels = new MenuCheckboxItem("強力車輪", "防止車輪變形並減少操縱。 這不會使輪胎防彈.", VehicleGodStrongWheels);
                MenuCheckboxItem godRamp = new MenuCheckboxItem("斜坡傷害", "坡道越野車等殘疾人車輛在使用坡道時不會受到損壞.", VehicleGodRamp);
                MenuCheckboxItem godAutoRepair = new MenuCheckboxItem("~r~自動修復", "有任何類型的損壞時，自動修復您的車輛。 建議關閉此功能以防止出現故障.", VehicleGodAutoRepair);

                vehGodMenu.AddMenuItem(godInvincible);
                vehGodMenu.AddMenuItem(godEngine);
                vehGodMenu.AddMenuItem(godVisual);
                vehGodMenu.AddMenuItem(godStrongWheels);
                vehGodMenu.AddMenuItem(godRamp);
                vehGodMenu.AddMenuItem(godAutoRepair);

                vehGodMenu.OnCheckboxChange += (sender, item, index, _checked) =>
                {
                    if (item == godInvincible)
                    {
                        VehicleGodInvincible = _checked;
                    }
                    else if (item == godEngine)
                    {
                        VehicleGodEngine = _checked;
                    }
                    else if (item == godVisual)
                    {
                        VehicleGodVisual = _checked;
                    }
                    else if (item == godStrongWheels)
                    {
                        VehicleGodStrongWheels = _checked;
                    }
                    else if (item == godRamp)
                    {
                        VehicleGodRamp = _checked;
                    }
                    else if (item == godAutoRepair)
                    {
                        VehicleGodAutoRepair = _checked;
                    }
                };

            }
            if (IsAllowed(Permission.VORepair)) // REPAIR VEHICLE
            {
                menu.AddMenuItem(fixVehicle);
            }
            if (IsAllowed(Permission.VOKeepClean))
            {
                menu.AddMenuItem(vehicleNeverDirty);
            }
            if (IsAllowed(Permission.VOWash))
            {
                menu.AddMenuItem(cleanVehicle); // CLEAN VEHICLE
                menu.AddMenuItem(setDirtLevel); // SET DIRT LEVEL
            }
            if (IsAllowed(Permission.VOEngine)) // TOGGLE ENGINE ON/OFF
            {
                menu.AddMenuItem(toggleEngine);
            }
            if (IsAllowed(Permission.VOBikeSeatbelt))
            {
                menu.AddMenuItem(vehicleBikeSeatbelt);
            }
            if (IsAllowed(Permission.VOSpeedLimiter)) // SPEED LIMITER
            {
                menu.AddMenuItem(speedLimiter);
            }
            if (IsAllowed(Permission.VOChangePlate))
            {
                menu.AddMenuItem(setLicensePlateText); // SET LICENSE PLATE TEXT
                menu.AddMenuItem(setLicensePlateType); // SET LICENSE PLATE TYPE
            }
            if (IsAllowed(Permission.VOMod)) // MOD MENU
            {
                menu.AddMenuItem(modMenuBtn);
            }
            if (IsAllowed(Permission.VOColors)) // COLORS MENU
            {
                menu.AddMenuItem(colorsMenuBtn);
            }
            if (IsAllowed(Permission.VOUnderglow)) // UNDERGLOW EFFECTS
            {
                menu.AddMenuItem(underglowMenuBtn);
                MenuController.BindMenuItem(menu, VehicleUnderglowMenu, underglowMenuBtn);
            }
            if (IsAllowed(Permission.VOLiveries)) // LIVERIES MENU
            {
                menu.AddMenuItem(liveriesMenuBtn);
            }
            if (IsAllowed(Permission.VOComponents)) // COMPONENTS MENU
            {
                menu.AddMenuItem(componentsMenuBtn);
            }
            if (IsAllowed(Permission.VODoors)) // DOORS MENU
            {
                menu.AddMenuItem(doorsMenuBtn);
            }
            if (IsAllowed(Permission.VOWindows)) // WINDOWS MENU
            {
                menu.AddMenuItem(windowsMenuBtn);
            }
            if (IsAllowed(Permission.VOTorqueMultiplier))
            {
                menu.AddMenuItem(torqueEnabled); // TORQUE ENABLED
                menu.AddMenuItem(torqueMultiplier); // TORQUE LIST
            }
            if (IsAllowed(Permission.VOPowerMultiplier))
            {
                menu.AddMenuItem(powerEnabled); // POWER ENABLED
                menu.AddMenuItem(powerMultiplier); // POWER LIST
            }
            if (IsAllowed(Permission.VODisableTurbulence))
            {
                menu.AddMenuItem(vehicleNoTurbulence);
            }
            if (IsAllowed(Permission.VOFlip)) // FLIP VEHICLE
            {
                menu.AddMenuItem(flipVehicle);
            }
            if (IsAllowed(Permission.VOAlarm)) // TOGGLE VEHICLE ALARM
            {
                menu.AddMenuItem(vehicleAlarm);
            }
            if (IsAllowed(Permission.VOCycleSeats)) // CYCLE THROUGH VEHICLE SEATS
            {
                menu.AddMenuItem(cycleSeats);
            }
            if (IsAllowed(Permission.VOLights)) // VEHICLE LIGHTS LIST
            {
                menu.AddMenuItem(vehicleLights);
            }
            if (IsAllowed(Permission.VOFixOrDestroyTires))
            {
                menu.AddMenuItem(vehicleTiresList);
                //menu.AddMenuItem(destroyTireList);
            }
            if (IsAllowed(Permission.VOFreeze)) // FREEZE VEHICLE
            {
                menu.AddMenuItem(vehicleFreeze);
            }
            if (IsAllowed(Permission.VOInvisible)) // MAKE VEHICLE INVISIBLE
            {
                menu.AddMenuItem(vehicleInvisible);
            }
            if (IsAllowed(Permission.VOEngineAlwaysOn)) // LEAVE ENGINE RUNNING
            {
                menu.AddMenuItem(vehicleEngineAO);
            }
            if (IsAllowed(Permission.VOInfiniteFuel)) // INFINITE FUEL
            {
                menu.AddMenuItem(infiniteFuel);
            }
            // always allowed
            menu.AddMenuItem(showHealth); // SHOW VEHICLE HEALTH

            if (IsAllowed(Permission.VONoSiren) && !vMenuShared.ConfigManager.GetSettingsBool(vMenuShared.ConfigManager.Setting.vmenu_use_els_compatibility_mode)) // DISABLE SIREN
            {
                menu.AddMenuItem(vehicleNoSiren);
            }
            if (IsAllowed(Permission.VONoHelmet)) // DISABLE BIKE HELMET
            {
                menu.AddMenuItem(vehicleNoBikeHelmet);
            }
            if (IsAllowed(Permission.VOFlashHighbeamsOnHonk)) // FLASH HIGHBEAMS ON HONK
            {
                menu.AddMenuItem(highbeamsOnHonk);
            }

            if (IsAllowed(Permission.VODelete)) // DELETE VEHICLE
            {
                menu.AddMenuItem(deleteBtn);
            }
            #endregion

            #region delete vehicle handle stuff
            DeleteConfirmMenu.AddMenuItem(deleteNoBtn);
            DeleteConfirmMenu.AddMenuItem(deleteYesBtn);
            DeleteConfirmMenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == deleteNoBtn)
                {
                    DeleteConfirmMenu.GoBack();
                }
                else
                {
                    Vehicle veh = GetVehicle();
                    if (veh != null && veh.Exists() && GetVehicle().Driver == Game.PlayerPed)
                    {
                        SetVehicleHasBeenOwnedByPlayer(veh.Handle, false);
                        SetEntityAsMissionEntity(veh.Handle, false, false);
                        veh.Delete();
                    }
                    else
                    {
                        if (!Game.PlayerPed.IsInVehicle())
                        {
                            Notify.Alert(CommonErrors.NoVehicle);
                        }
                        else
                        {
                            Notify.Alert("如果要刪除載具，您需要坐在駕駛員座位上。");
                        }

                    }
                    DeleteConfirmMenu.GoBack();
                    menu.GoBack();
                }
            };
            #endregion

            #region Bind Submenus to their buttons.
            MenuController.BindMenuItem(menu, VehicleModMenu, modMenuBtn);
            MenuController.BindMenuItem(menu, VehicleDoorsMenu, doorsMenuBtn);
            MenuController.BindMenuItem(menu, VehicleWindowsMenu, windowsMenuBtn);
            MenuController.BindMenuItem(menu, VehicleComponentsMenu, componentsMenuBtn);
            MenuController.BindMenuItem(menu, VehicleLiveriesMenu, liveriesMenuBtn);
            MenuController.BindMenuItem(menu, VehicleColorsMenu, colorsMenuBtn);
            MenuController.BindMenuItem(menu, DeleteConfirmMenu, deleteBtn);
            #endregion

            #region Handle button presses
            // Manage button presses.
            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == deleteBtn) // reset the index so that "no" / "cancel" will always be selected by default.
                {
                    DeleteConfirmMenu.RefreshIndex();
                }
                // If the player is actually in a vehicle, continue.
                if (GetVehicle() != null && GetVehicle().Exists())
                {
                    // Create a vehicle object.
                    Vehicle vehicle = GetVehicle();

                    // Check if the player is the driver of the vehicle, if so, continue.
                    if (vehicle.GetPedOnSeat(VehicleSeat.Driver) == new Ped(Game.PlayerPed.Handle))
                    {
                        // Repair vehicle.
                        if (item == fixVehicle)
                        {
                            vehicle.Repair();
                        }
                        // Clean vehicle.
                        else if (item == cleanVehicle)
                        {
                            vehicle.Wash();
                        }
                        // Flip vehicle.
                        else if (item == flipVehicle)
                        {
                            SetVehicleOnGroundProperly(vehicle.Handle);
                        }
                        // Toggle alarm.
                        else if (item == vehicleAlarm)
                        {
                            ToggleVehicleAlarm(vehicle);
                        }
                        // Toggle engine
                        else if (item == toggleEngine)
                        {
                            SetVehicleEngineOn(vehicle.Handle, !vehicle.IsEngineRunning, false, true);
                        }
                        // Set license plate text
                        else if (item == setLicensePlateText)
                        {
                            SetLicensePlateCustomText();
                        }
                        else if (item == vehicleInvisible) // Make vehicle invisible.
                        {
                            if (vehicle.IsVisible)
                            {
                                // Check the visibility of all peds inside before setting the vehicle as invisible.
                                Dictionary<Ped, bool> visiblePeds = new Dictionary<Ped, bool>();
                                foreach (Ped p in vehicle.Occupants)
                                {
                                    visiblePeds.Add(p, p.IsVisible);
                                }

                                // Set the vehicle invisible or invincivble.
                                vehicle.IsVisible = !vehicle.IsVisible;

                                // Restore visibility for each ped.
                                foreach (var pe in visiblePeds)
                                {
                                    pe.Key.IsVisible = pe.Value;
                                }
                            }
                            else
                            {
                                // Set the vehicle invisible or invincivble.
                                vehicle.IsVisible = !vehicle.IsVisible;
                            }
                        }
                    }

                    // If the player is not the driver seat and a button other than the option below (cycle seats) was pressed, notify them.
                    else if (item != cycleSeats)
                    {
                        Notify.Error("您必須是載具的駕駛員才能訪問此選項！", true, false);
                    }

                    // Cycle vehicle seats
                    if (item == cycleSeats)
                    {
                        CycleThroughSeats();
                    }
                }
            };
            #endregion

            #region Handle checkbox changes.
            menu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                // Create a vehicle object.
                Vehicle vehicle = GetVehicle();

                if (item == vehicleGod) // God Mode Toggled
                {
                    VehicleGodMode = _checked;
                }
                //else if (item == vehicleSpecialGod) // special god mode
                //{
                //    VehicleSpecialGodMode = _checked;
                //}
                else if (item == vehicleFreeze) // Freeze Vehicle Toggled
                {
                    VehicleFrozen = _checked;
                    if (!_checked)
                    {
                        if (vehicle != null && vehicle.Exists())
                        {
                            FreezeEntityPosition(vehicle.Handle, false);
                        }
                    }
                }
                else if (item == torqueEnabled) // Enable Torque Multiplier Toggled
                {
                    VehicleTorqueMultiplier = _checked;
                }
                else if (item == powerEnabled) // Enable Power Multiplier Toggled
                {
                    VehiclePowerMultiplier = _checked;
                    if (_checked)
                    {
                        if (vehicle != null && vehicle.Exists())
                            SetVehicleEnginePowerMultiplier(vehicle.Handle, VehiclePowerMultiplierAmount);
                    }
                    else
                    {
                        if (vehicle != null && vehicle.Exists())
                            SetVehicleEnginePowerMultiplier(vehicle.Handle, 1f);
                    }
                }
                else if (item == vehicleEngineAO) // Leave Engine Running (vehicle always on) Toggled
                {
                    VehicleEngineAlwaysOn = _checked;
                }
                else if (item == showHealth) // show vehicle health on screen.
                {
                    VehicleShowHealth = _checked;
                }
                else if (item == vehicleNoSiren) // Disable Siren Toggled
                {
                    VehicleNoSiren = _checked;
                    if (vehicle != null && vehicle.Exists())
                        vehicle.IsSirenSilent = _checked;
                }
                else if (item == vehicleNoBikeHelmet) // No Helemet Toggled
                {
                    VehicleNoBikeHelemet = _checked;
                }
                else if (item == highbeamsOnHonk)
                {
                    FlashHighbeamsOnHonk = _checked;
                }
                else if (item == vehicleNoTurbulence)
                {
                    DisablePlaneTurbulence = _checked;
                    if (vehicle != null && vehicle.Exists() && vehicle.Model.IsPlane)
                    {
                        if (MainMenu.VehicleOptionsMenu.DisablePlaneTurbulence)
                        {
                            SetPlaneTurbulenceMultiplier(vehicle.Handle, 0f);
                        }
                        else
                        {
                            SetPlaneTurbulenceMultiplier(vehicle.Handle, 1.0f);
                        }
                    }
                }
                else if (item == vehicleNeverDirty)
                {
                    VehicleNeverDirty = _checked;
                }
                else if (item == vehicleBikeSeatbelt)
                {
                    VehicleBikeSeatbelt = _checked;
                }
                else if (item == infiniteFuel)
                {
                    VehicleInfiniteFuel = _checked;
                }
            };
            #endregion

            #region Handle List Changes.
            // Handle list changes.
            menu.OnListIndexChange += (sender, item, oldIndex, newIndex, itemIndex) =>
            {
                if (GetVehicle() != null && GetVehicle().Exists())
                {
                    Vehicle veh = GetVehicle();
                    // If the torque multiplier changed. Change the torque multiplier to the new value.
                    if (item == torqueMultiplier)
                    {
                        // Get the selected value and remove the "x" in the string with nothing.
                        var value = torqueMultiplierList[newIndex].ToString().Replace("x", "");
                        // Convert the value to a float and set it as a public variable.
                        VehicleTorqueMultiplierAmount = float.Parse(value);
                    }
                    // If the power multiplier is changed. Change the power multiplier to the new value.
                    else if (item == powerMultiplier)
                    {
                        // Get the selected value. Remove the "x" from the string.
                        var value = powerMultiplierList[newIndex].ToString().Replace("x", "");
                        // Conver the string into a float and set it to be the value of the public variable.
                        VehiclePowerMultiplierAmount = float.Parse(value);
                        if (VehiclePowerMultiplier)
                        {
                            SetVehicleEnginePowerMultiplier(veh.Handle, VehiclePowerMultiplierAmount);
                        }
                    }
                    else if (item == setLicensePlateType)
                    {
                        // Set the license plate style.
                        switch (newIndex)
                        {
                            case 0:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.BlueOnWhite1;
                                break;
                            case 1:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.BlueOnWhite2;
                                break;
                            case 2:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.BlueOnWhite3;
                                break;
                            case 3:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.YellowOnBlue;
                                break;
                            case 4:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.YellowOnBlack;
                                break;
                            case 5:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.NorthYankton;
                                break;
                            default:
                                break;
                        }
                    }
                }
            };
            #endregion

            #region Handle List Items Selected
            menu.OnListItemSelect += async (sender, item, listIndex, itemIndex) =>
            {
                // Set dirt level
                if (item == setDirtLevel)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        GetVehicle().DirtLevel = float.Parse(listIndex.ToString());
                    }
                    else
                    {
                        Notify.Error(CommonErrors.NoVehicle);
                    }
                }
                // Toggle vehicle lights
                else if (item == vehicleLights)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        Vehicle veh = GetVehicle();
                        // We need to do % 4 because this seems to be some sort of flags system. For a taxi, this function returns 65, 66, etc.
                        // So % 4 takes care of that.
                        var state = GetVehicleIndicatorLights(veh.Handle) % 4; // 0 = none, 1 = left, 2 = right, 3 = both

                        if (listIndex == 0) // Hazard lights
                        {
                            if (state != 3) // either all lights are off, or one of the two (left/right) is off.
                            {
                                SetVehicleIndicatorLights(veh.Handle, 1, true); // left on
                                SetVehicleIndicatorLights(veh.Handle, 0, true); // right on
                            }
                            else // both are on.
                            {
                                SetVehicleIndicatorLights(veh.Handle, 1, false); // left off
                                SetVehicleIndicatorLights(veh.Handle, 0, false); // right off
                            }
                        }
                        else if (listIndex == 1) // left indicator
                        {
                            if (state != 1) // Left indicator is (only) off
                            {
                                SetVehicleIndicatorLights(veh.Handle, 1, true); // left on
                                SetVehicleIndicatorLights(veh.Handle, 0, false); // right off
                            }
                            else
                            {
                                SetVehicleIndicatorLights(veh.Handle, 1, false); // left off
                                SetVehicleIndicatorLights(veh.Handle, 0, false); // right off
                            }
                        }
                        else if (listIndex == 2) // right indicator
                        {
                            if (state != 2) // Right indicator (only) is off
                            {
                                SetVehicleIndicatorLights(veh.Handle, 1, false); // left off
                                SetVehicleIndicatorLights(veh.Handle, 0, true); // right on
                            }
                            else
                            {
                                SetVehicleIndicatorLights(veh.Handle, 1, false); // left off
                                SetVehicleIndicatorLights(veh.Handle, 0, false); // right off
                            }
                        }
                        else if (listIndex == 3) // Interior lights
                        {
                            SetVehicleInteriorlight(veh.Handle, !IsVehicleInteriorLightOn(veh.Handle));
                            //CommonFunctions.Log("Something cool here.");
                        }
                        //else if (listIndex == 4) // taxi light
                        //{
                        //    veh.IsTaxiLightOn = !veh.IsTaxiLightOn;
                        //    //    SetTaxiLights(veh, true);
                        //    //    SetTaxiLights(veh, false);
                        //    //    //CommonFunctions.Log(IsTaxiLightOn(veh).ToString());
                        //    //    //SetTaxiLights(veh, true);
                        //    //    //CommonFunctions.Log(IsTaxiLightOn(veh).ToString());
                        //    //    //SetTaxiLights(veh, false);
                        //    //    //SetTaxiLights(veh, !IsTaxiLightOn(veh));
                        //    //    CommonFunctions.Log
                        //}
                        else if (listIndex == 4) // helicopter spotlight
                        {
                            SetVehicleSearchlight(veh.Handle, !IsVehicleSearchlightOn(veh.Handle), true);
                        }
                    }
                    else
                    {
                        Notify.Error(CommonErrors.NoVehicle);
                    }
                }
                // Speed Limiter
                else if (item == speedLimiter)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        Vehicle vehicle = GetVehicle();

                        if (vehicle != null && vehicle.Exists())
                        {
                            if (listIndex == 0) // Set
                            {
                                SetEntityMaxSpeed(vehicle.Handle, 500.01f);
                                SetEntityMaxSpeed(vehicle.Handle, vehicle.Speed);

                                if (ShouldUseMetricMeasurements()) // kph
                                {
                                    Notify.Info($"載具速度現在限制為 ~b~{Math.Round(vehicle.Speed * 3.6f, 1)} KPH~s~.");
                                }
                                else // mph
                                {
                                    Notify.Info($"載具速度現在限制為~b~{Math.Round(vehicle.Speed * 2.237f, 1)} MPH~s~.");
                                }

                            }
                            else if (listIndex == 1) // Reset
                            {
                                SetEntityMaxSpeed(vehicle.Handle, 500.01f); // Default max speed seemingly for all vehicles.
                                Notify.Info("載具現在不再限制速度.");
                            }
                            else if (listIndex == 2) // custom speed
                            {
                                string inputSpeed = await GetUserInput("Enter a speed (in meters/sec)", "20.0", 5);
                                if (!string.IsNullOrEmpty(inputSpeed))
                                {
                                    if (float.TryParse(inputSpeed, out float outFloat))
                                    {
                                        //vehicle.MaxSpeed = outFloat;
                                        SetEntityMaxSpeed(vehicle.Handle, 500.01f);
                                        await BaseScript.Delay(0);
                                        SetEntityMaxSpeed(vehicle.Handle, outFloat + 0.01f);
                                        if (ShouldUseMetricMeasurements()) // kph
                                        {
                                            Notify.Info($"載具速度現在限制為 ~b~{Math.Round(outFloat * 3.6f, 1)} KPH~s~.");
                                        }
                                        else // mph
                                        {
                                            Notify.Info($"載具速度現在限制為 ~b~{Math.Round(outFloat * 2.237f, 1)} MPH~s~.");
                                        }
                                    }
                                    else if (int.TryParse(inputSpeed, out int outInt))
                                    {
                                        SetEntityMaxSpeed(vehicle.Handle, 500.01f);
                                        await BaseScript.Delay(0);
                                        SetEntityMaxSpeed(vehicle.Handle, outInt + 0.01f);
                                        if (ShouldUseMetricMeasurements()) // kph
                                        {
                                            Notify.Info($"載具速度現在限制為 ~b~{Math.Round((float)outInt * 3.6f, 1)} KPH~s~.");
                                        }
                                        else // mph
                                        {
                                            Notify.Info($"載具速度現在限制為 ~b~{Math.Round((float)outInt * 2.237f, 1)} MPH~s~.");
                                        }
                                    }
                                    else
                                    {
                                        Notify.Error("這不是有效數字。 請以米/秒為單位輸入有效速度.");
                                    }
                                }
                                else
                                {
                                    Notify.Error(CommonErrors.InvalidInput);
                                }
                            }
                        }
                    }
                }
                else if (item == vehicleTiresList)
                {
                    //bool fix = item == vehicleTiresList;

                    var veh = GetVehicle();
                    if (veh != null && veh.Exists())
                    {
                        if (Game.PlayerPed == veh.Driver)
                        {
                            if (listIndex == 0)
                            {
                                if (IsVehicleTyreBurst(veh.Handle, 0, false))
                                {
                                    for (var i = 0; i < 8; i++)
                                    {
                                        SetVehicleTyreFixed(veh.Handle, i);
                                    }
                                    Notify.Success("所有車輛輪胎均已修復.");
                                }
                                else
                                {
                                    for (var i = 0; i < 8; i++)
                                    {
                                        SetVehicleTyreBurst(veh.Handle, i, false, 1f);
                                    }
                                    Notify.Success("所有汽車輪胎均已銷毀.");
                                }
                            }
                            else
                            {
                                int index = listIndex - 1;
                                if (IsVehicleTyreBurst(veh.Handle, index, false))
                                {
                                    SetVehicleTyreFixed(veh.Handle, index);
                                    Notify.Success($"載具輪胎 #{listIndex} 已修復.");
                                }
                                else
                                {
                                    SetVehicleTyreBurst(veh.Handle, index, false, 1f);
                                    Notify.Success($"載具輪胎 #{listIndex} 已摧毀.");
                                }
                            }
                        }
                        else
                        {
                            Notify.Error(CommonErrors.NeedToBeTheDriver);
                        }
                    }
                    else
                    {
                        Notify.Error(CommonErrors.NoVehicle);
                    }
                }
            };
            #endregion

            #region Vehicle Colors Submenu Stuff
            // primary menu
            Menu primaryColorsMenu = new Menu("載具顏色", "原色");
            MenuController.AddSubmenu(VehicleColorsMenu, primaryColorsMenu);

            MenuItem primaryColorsBtn = new MenuItem("原色") { Label = "→→→" };
            VehicleColorsMenu.AddMenuItem(primaryColorsBtn);
            MenuController.BindMenuItem(VehicleColorsMenu, primaryColorsMenu, primaryColorsBtn);

            // secondary menu
            Menu secondaryColorsMenu = new Menu("載具顏色", "副顏色");
            MenuController.AddSubmenu(VehicleColorsMenu, secondaryColorsMenu);

            MenuItem secondaryColorsBtn = new MenuItem("副顏色") { Label = "→→→" };
            VehicleColorsMenu.AddMenuItem(secondaryColorsBtn);
            MenuController.BindMenuItem(VehicleColorsMenu, secondaryColorsMenu, secondaryColorsBtn);

            // color lists
            List<string> classic = new List<string>();
            List<string> matte = new List<string>();
            List<string> metals = new List<string>();
            List<string> util = new List<string>();
            List<string> worn = new List<string>();
            List<string> wheelColors = new List<string>() { "默認合金" };

            // Just quick and dirty solution to put this in a new enclosed section so that we can still use 'i' as a counter in the other code parts.
            {
                int i = 0;
                foreach (var vc in VehicleData.ClassicColors)
                {
                    classic.Add($"{GetLabelText(vc.label)} ({i + 1}/{VehicleData.ClassicColors.Count})");
                    i++;
                }

                i = 0;
                foreach (var vc in VehicleData.MatteColors)
                {
                    matte.Add($"{GetLabelText(vc.label)} ({i + 1}/{VehicleData.MatteColors.Count})");
                    i++;
                }

                i = 0;
                foreach (var vc in VehicleData.MetalColors)
                {
                    metals.Add($"{GetLabelText(vc.label)} ({i + 1}/{VehicleData.MetalColors.Count})");
                    i++;
                }

                i = 0;
                foreach (var vc in VehicleData.UtilColors)
                {
                    util.Add($"{GetLabelText(vc.label)} ({i + 1}/{VehicleData.UtilColors.Count})");
                    i++;
                }

                i = 0;
                foreach (var vc in VehicleData.WornColors)
                {
                    worn.Add($"{GetLabelText(vc.label)} ({i + 1}/{VehicleData.WornColors.Count})");
                    i++;
                }

                wheelColors.AddRange(classic);
            }

            MenuListItem wheelColorsList = new MenuListItem("輪胎顏色", wheelColors, 0);
            MenuListItem dashColorList = new MenuListItem("儀表板顏色", classic, 0);
            MenuListItem intColorList = new MenuListItem("內部 / 修剪顏色", classic, 0);
            MenuSliderItem vehicleEnveffScale = new MenuSliderItem("車輛塗裝", "這僅適用於某些車輛，例如besra。它褪色某些塗料層。", 0, 20, 10, true);

            MenuItem chrome = new MenuItem("鉻和金");
            VehicleColorsMenu.AddMenuItem(chrome);
            VehicleColorsMenu.AddMenuItem(vehicleEnveffScale);

            VehicleColorsMenu.OnItemSelect += (sender, item, index) =>
            {
                Vehicle veh = GetVehicle();
                if (veh != null && veh.Exists() && !veh.IsDead && veh.Driver == Game.PlayerPed)
                {
                    if (item == chrome)
                    {
                        SetVehicleColours(veh.Handle, 120, 120); // chrome is index 120
                    }
                }
                else
                {
                    Notify.Error("您必須是要載具的駕駛才能對此進行更改。");
                }
            };
            VehicleColorsMenu.OnSliderPositionChange += (m, sliderItem, oldPosition, newPosition, itemIndex) =>
            {
                Vehicle veh = GetVehicle();
                if (veh != null && veh.Driver == Game.PlayerPed && !veh.IsDead)
                {
                    if (sliderItem == vehicleEnveffScale)
                    {
                        SetVehicleEnveffScale(veh.Handle, newPosition / 20f);
                    }
                }
                else
                {
                    Notify.Error("您必須是要載具的駕駛才能對此進行更改。");
                }
            };

            VehicleColorsMenu.AddMenuItem(dashColorList);
            VehicleColorsMenu.AddMenuItem(intColorList);
            VehicleColorsMenu.AddMenuItem(wheelColorsList);

            VehicleColorsMenu.OnListIndexChange += HandleListIndexChanges;

            void HandleListIndexChanges(Menu sender, MenuListItem listItem, int oldIndex, int newIndex, int itemIndex)
            {
                Vehicle veh = GetVehicle();
                if (veh != null && veh.Exists() && !veh.IsDead && veh.Driver == Game.PlayerPed)
                {
                    int primaryColor = 0;
                    int secondaryColor = 0;
                    int pearlColor = 0;
                    int wheelColor = 0;
                    int dashColor = 0;
                    int intColor = 0;

                    GetVehicleColours(veh.Handle, ref primaryColor, ref secondaryColor);
                    GetVehicleExtraColours(veh.Handle, ref pearlColor, ref wheelColor);
                    GetVehicleDashboardColour(veh.Handle, ref dashColor);
                    GetVehicleInteriorColour(veh.Handle, ref intColor);

                    if (sender == primaryColorsMenu)
                    {
                        if (itemIndex == 1)
                            pearlColor = VehicleData.ClassicColors[newIndex].id;
                        else
                            pearlColor = 0;

                        switch (itemIndex)
                        {
                            case 0:
                            case 1:
                                primaryColor = VehicleData.ClassicColors[newIndex].id;
                                break;
                            case 2:
                                primaryColor = VehicleData.MatteColors[newIndex].id;
                                break;
                            case 3:
                                primaryColor = VehicleData.MetalColors[newIndex].id;
                                break;
                            case 4:
                                primaryColor = VehicleData.UtilColors[newIndex].id;
                                break;
                            case 5:
                                primaryColor = VehicleData.WornColors[newIndex].id;
                                break;
                        }
                        SetVehicleColours(veh.Handle, primaryColor, secondaryColor);
                    }
                    else if (sender == secondaryColorsMenu)
                    {
                        switch (itemIndex)
                        {
                            case 0:
                                pearlColor = VehicleData.ClassicColors[newIndex].id;
                                break;
                            case 1:
                            case 2:
                                secondaryColor = VehicleData.ClassicColors[newIndex].id;
                                break;
                            case 3:
                                secondaryColor = VehicleData.MatteColors[newIndex].id;
                                break;
                            case 4:
                                secondaryColor = VehicleData.MetalColors[newIndex].id;
                                break;
                            case 5:
                                secondaryColor = VehicleData.UtilColors[newIndex].id;
                                break;
                            case 6:
                                secondaryColor = VehicleData.WornColors[newIndex].id;
                                break;
                        }
                        SetVehicleColours(veh.Handle, primaryColor, secondaryColor);
                    }
                    else if (sender == VehicleColorsMenu)
                    {
                        if (listItem == wheelColorsList)
                        {
                            if (newIndex == 0)
                            {
                                wheelColor = 156; // default alloy color.
                            }
                            else
                            {
                                wheelColor = VehicleData.ClassicColors[newIndex - 1].id;
                            }
                        }
                        else if (listItem == dashColorList)
                        {
                            dashColor = VehicleData.ClassicColors[newIndex].id;
                            // sadly these native names are mixed up :/ but ofc it's impossible to fix due to backwards compatibility.
                            // this should actually be called SetVehicleDashboardColour
                            SetVehicleInteriorColour(veh.Handle, dashColor);
                        }
                        else if (listItem == intColorList)
                        {
                            intColor = VehicleData.ClassicColors[newIndex].id;
                            // sadly these native names are mixed up :/ but ofc it's impossible to fix due to backwards compatibility.
                            // this should actually be called SetVehicleInteriorColour
                            SetVehicleDashboardColour(veh.Handle, intColor);
                        }
                    }

                    SetVehicleExtraColours(veh.Handle, pearlColor, wheelColor);
                }
                else
                {
                    Notify.Error("您需要成為載具的駕駛員才能更改載具的顏色.");
                }
            }


            for (int i = 0; i < 2; i++)
            {
                var pearlescentList = new MenuListItem("珠光", classic, 0);
                var classicList = new MenuListItem("經典", classic, 0);
                var metallicList = new MenuListItem("金屬", classic, 0);
                var matteList = new MenuListItem("消光", matte, 0);
                var metalList = new MenuListItem("合金", metals, 0);
                var utilList = new MenuListItem("Util", util, 0);
                var wornList = new MenuListItem("破舊", worn, 0);

                if (i == 0)
                {
                    primaryColorsMenu.AddMenuItem(classicList);
                    primaryColorsMenu.AddMenuItem(metallicList);
                    primaryColorsMenu.AddMenuItem(matteList);
                    primaryColorsMenu.AddMenuItem(metalList);
                    primaryColorsMenu.AddMenuItem(utilList);
                    primaryColorsMenu.AddMenuItem(wornList);

                    primaryColorsMenu.OnListIndexChange += HandleListIndexChanges;
                }
                else
                {
                    secondaryColorsMenu.AddMenuItem(pearlescentList);
                    secondaryColorsMenu.AddMenuItem(classicList);
                    secondaryColorsMenu.AddMenuItem(metallicList);
                    secondaryColorsMenu.AddMenuItem(matteList);
                    secondaryColorsMenu.AddMenuItem(metalList);
                    secondaryColorsMenu.AddMenuItem(utilList);
                    secondaryColorsMenu.AddMenuItem(wornList);

                    secondaryColorsMenu.OnListIndexChange += HandleListIndexChanges;
                }
            }
            #endregion

            #region Vehicle Doors Submenu Stuff
            MenuItem openAll = new MenuItem("開啟所有車門", "開啟所有車門.");
            MenuItem closeAll = new MenuItem("關閉所有車門", "關閉所有車門.");
            MenuItem LF = new MenuItem("右前門", "開啟/關閉 右前門.");
            MenuItem RF = new MenuItem("左前門", "開啟/關閉 左前門.");
            MenuItem LR = new MenuItem("左後門", "開啟/關閉 右後門.");
            MenuItem RR = new MenuItem("右後門", "開啟/關閉 左後門.");
            MenuItem HD = new MenuItem("引擎蓋", "開啟/關閉 引擎蓋.");
            MenuItem TR = new MenuItem("後車箱", "開啟/關閉 後車箱.");
            MenuItem E1 = new MenuItem("附加 1", "開啟/關閉 附加 (#1). 請注意，大多數的載具上都此功能");
            MenuItem E2 = new MenuItem("附加 2", "開啟/關閉 附加 (#2). 請注意，大多數的載具上都此功能.");
            MenuItem BB = new MenuItem("炸彈艙", "開啟/關閉 炸彈艙。僅能在某些飛機上使用");
            var doors = new List<string>() { "Front Left", "Front Right", "Rear Left", "Rear Right", "Hood", "Trunk", "Extra 1", "Extra 2" };
            MenuListItem removeDoorList = new MenuListItem("移除門", doors, 0, "完成卸下特定的載具門.");
            MenuCheckboxItem deleteDoors = new MenuCheckboxItem("刪除已拆除的門", "啟用後上述列表的門將被刪除。如果是禁用的話門只會掉在地上.", false);

            VehicleDoorsMenu.AddMenuItem(LF);
            VehicleDoorsMenu.AddMenuItem(RF);
            VehicleDoorsMenu.AddMenuItem(LR);
            VehicleDoorsMenu.AddMenuItem(RR);
            VehicleDoorsMenu.AddMenuItem(HD);
            VehicleDoorsMenu.AddMenuItem(TR);
            VehicleDoorsMenu.AddMenuItem(E1);
            VehicleDoorsMenu.AddMenuItem(E2);
            VehicleDoorsMenu.AddMenuItem(BB);
            VehicleDoorsMenu.AddMenuItem(openAll);
            VehicleDoorsMenu.AddMenuItem(closeAll);
            VehicleDoorsMenu.AddMenuItem(removeDoorList);
            VehicleDoorsMenu.AddMenuItem(deleteDoors);

            VehicleDoorsMenu.OnListItemSelect += (sender, item, index, itemIndex) =>
            {
                Vehicle veh = GetVehicle();
                if (veh != null && veh.Exists())
                {
                    if (veh.Driver == Game.PlayerPed)
                    {
                        if (item == removeDoorList)
                        {
                            SetVehicleDoorBroken(veh.Handle, index, deleteDoors.Checked);
                        }
                    }
                    else
                    {
                        Notify.Error(CommonErrors.NeedToBeTheDriver);
                    }
                }
                else
                {
                    Notify.Error(CommonErrors.NoVehicle);
                }
            };

            // Handle button presses.
            VehicleDoorsMenu.OnItemSelect += (sender, item, index) =>
            {
                // Get the vehicle.
                Vehicle veh = GetVehicle();
                // If the player is in a vehicle, it's not dead and the player is the driver, continue.
                if (veh != null && veh.Exists() && !veh.IsDead && veh.Driver == Game.PlayerPed)
                {
                    // If button 0-5 are pressed, then open/close that specific index/door.
                    if (index < 8)
                    {
                        // If the door is open.
                        bool open = GetVehicleDoorAngleRatio(veh.Handle, index) > 0.1f ? true : false;

                        if (open)
                        {
                            // Close the door.
                            SetVehicleDoorShut(veh.Handle, index, false);
                        }
                        else
                        {
                            // Open the door.
                            SetVehicleDoorOpen(veh.Handle, index, false, false);
                        }
                    }
                    // If the index >= 8, and the button is "openAll": open all doors.
                    else if (item == openAll)
                    {
                        // Loop through all doors and open them.
                        for (var door = 0; door < 8; door++)
                        {
                            SetVehicleDoorOpen(veh.Handle, door, false, false);
                        }
                        if (veh.HasBombBay) veh.OpenBombBay();
                    }
                    // If the index >= 8, and the button is "closeAll": close all doors.
                    else if (item == closeAll)
                    {
                        // Close all doors.
                        SetVehicleDoorsShut(veh.Handle, false);
                        if (veh.HasBombBay) veh.CloseBombBay();
                    }
                    // If bomb bay doors button is pressed and the vehicle has bomb bay doors.
                    else if (item == BB && veh.HasBombBay)
                    {
                        bool bombBayOpen = AreBombBayDoorsOpen(veh.Handle);
                        // If open, close them.
                        if (bombBayOpen)
                            veh.CloseBombBay();
                        // Otherwise, open them.
                        else
                            veh.OpenBombBay();
                    }
                }
                else
                {
                    Notify.Alert(CommonErrors.NoVehicle, placeholderValue: "打開/關閉載具");
                }
            };

            #endregion

            #region Vehicle Windows Submenu Stuff
            MenuItem fwu = new MenuItem("~y~↑~s~ 搖上前車窗", "搖上後車窗.");
            MenuItem fwd = new MenuItem("~o~↓~s~ 搖下前車窗", "搖下後車窗.");
            MenuItem rwu = new MenuItem("~y~↑~s~ 搖上後車窗", "搖上後車窗.");
            MenuItem rwd = new MenuItem("~o~↓~s~ 搖下後車窗", "搖下後車窗.");
            VehicleWindowsMenu.AddMenuItem(fwu);
            VehicleWindowsMenu.AddMenuItem(fwd);
            VehicleWindowsMenu.AddMenuItem(rwu);
            VehicleWindowsMenu.AddMenuItem(rwd);
            VehicleWindowsMenu.OnItemSelect += (sender, item, index) =>
            {
                Vehicle veh = GetVehicle();
                if (veh != null && veh.Exists() && !veh.IsDead)
                {
                    if (item == fwu)
                    {
                        RollUpWindow(veh.Handle, 0);
                        RollUpWindow(veh.Handle, 1);
                    }
                    else if (item == fwd)
                    {
                        RollDownWindow(veh.Handle, 0);
                        RollDownWindow(veh.Handle, 1);
                    }
                    else if (item == rwu)
                    {
                        RollUpWindow(veh.Handle, 2);
                        RollUpWindow(veh.Handle, 3);
                    }
                    else if (item == rwd)
                    {
                        RollDownWindow(veh.Handle, 2);
                        RollDownWindow(veh.Handle, 3);
                    }
                }
            };
            #endregion

            #region Vehicle Liveries Submenu Stuff
            menu.OnItemSelect += (sender, item, idex) =>
            {
                // If the liverys menu button is selected.
                if (item == liveriesMenuBtn)
                {
                    // Get the player's vehicle.
                    Vehicle veh = GetVehicle();
                    // If it exists, isn't dead and the player is in the drivers seat continue.
                    if (veh != null && veh.Exists() && !veh.IsDead)
                    {
                        if (veh.Driver == Game.PlayerPed)
                        {
                            VehicleLiveriesMenu.ClearMenuItems();
                            SetVehicleModKit(veh.Handle, 0);
                            var liveryCount = GetVehicleLiveryCount(veh.Handle);

                            if (liveryCount > 0)
                            {
                                var liveryList = new List<string>();
                                for (var i = 0; i < liveryCount; i++)
                                {
                                    var livery = GetLiveryName(veh.Handle, i);
                                    livery = GetLabelText(livery) != "NULL" ? GetLabelText(livery) : $"Livery #{i}";
                                    liveryList.Add(livery);
                                }
                                MenuListItem liveryListItem = new MenuListItem("塗裝", liveryList, GetVehicleLivery(veh.Handle), "選擇載具的塗裝");
                                VehicleLiveriesMenu.AddMenuItem(liveryListItem);
                                VehicleLiveriesMenu.OnListIndexChange += (_menu, listItem, oldIndex, newIndex, itemIndex) =>
                                {
                                    if (listItem == liveryListItem)
                                    {
                                        veh = GetVehicle();
                                        SetVehicleLivery(veh.Handle, newIndex);
                                    }
                                };
                                VehicleLiveriesMenu.RefreshIndex();
                                //VehicleLiveriesMenu.UpdateScaleform();
                            }
                            else
                            {
                                Notify.Error("這輛載具沒有任何的塗裝.");
                                VehicleLiveriesMenu.CloseMenu();
                                menu.OpenMenu();
                                MenuItem backBtn = new MenuItem("沒有可用的塗裝 :(", "點擊我返回.")
                                {
                                    Label = "Go Back"
                                };
                                VehicleLiveriesMenu.AddMenuItem(backBtn);
                                VehicleLiveriesMenu.OnItemSelect += (sender2, item2, index2) =>
                                {
                                    if (item2 == backBtn)
                                    {
                                        VehicleLiveriesMenu.GoBack();
                                    }
                                };

                                VehicleLiveriesMenu.RefreshIndex();
                                //VehicleLiveriesMenu.UpdateScaleform();
                            }
                        }
                        else
                        {
                            Notify.Error("您必須是載具駕駛員才能訪問此選單.");
                        }
                    }
                    else
                    {
                        Notify.Error("您必須是載具駕駛員才能訪問此選單.");
                    }
                }
            };
            #endregion

            #region Vehicle Mod Submenu Stuff
            menu.OnItemSelect += (sender, item, index) =>
            {
                // When the mod submenu is openend, reset all items in there.
                if (item == modMenuBtn)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        UpdateMods();
                    }
                    else
                    {
                        VehicleModMenu.CloseMenu();
                        menu.OpenMenu();
                    }

                }
            };
            #endregion

            #region Vehicle Components Submenu
            // when the components menu is opened.
            menu.OnItemSelect += (sender, item, index) =>
            {
                // If the components menu is opened.
                if (item == componentsMenuBtn)
                {
                    // Empty the menu in case there were leftover buttons from another vehicle.
                    if (VehicleComponentsMenu.Size > 0)
                    {
                        VehicleComponentsMenu.ClearMenuItems();
                        vehicleExtras.Clear();
                        VehicleComponentsMenu.RefreshIndex();
                        //VehicleComponentsMenu.UpdateScaleform();
                    }

                    // Get the vehicle.
                    Vehicle veh = GetVehicle();

                    // Check if the vehicle exists, it's actually a vehicle, it's not dead/broken and the player is in the drivers seat.
                    if (veh != null && veh.Exists() && !veh.IsDead && veh.Driver == Game.PlayerPed)
                    {
                        //List<int> extraIds = new List<int>();
                        // Loop through all possible extra ID's (AFAIK: 0-14).
                        for (var extra = 0; extra < 14; extra++)
                        {
                            // If this extra exists...
                            if (veh.ExtraExists(extra))
                            {
                                // Add it's ID to the list.
                                //extraIds.Add(extra);

                                // Create a checkbox for it.
                                MenuCheckboxItem extraCheckbox = new MenuCheckboxItem($"Extra #{extra}", extra.ToString(), veh.IsExtraOn(extra));
                                // Add the checkbox to the menu.
                                VehicleComponentsMenu.AddMenuItem(extraCheckbox);

                                // Add it's ID to the dictionary.
                                vehicleExtras[extraCheckbox] = extra;
                            }
                        }



                        if (vehicleExtras.Count > 0)
                        {
                            MenuItem backBtn = new MenuItem("返回", "返回載具選項菜單.");
                            VehicleComponentsMenu.AddMenuItem(backBtn);
                            VehicleComponentsMenu.OnItemSelect += (sender3, item3, index3) =>
                            {
                                VehicleComponentsMenu.GoBack();
                            };
                        }
                        else
                        {
                            MenuItem backBtn = new MenuItem("沒有可用的額外功能 :(", "返回載具選項菜單.")
                            {
                                Label = "Go Back"
                            };
                            VehicleComponentsMenu.AddMenuItem(backBtn);
                            VehicleComponentsMenu.OnItemSelect += (sender3, item3, index3) =>
                            {
                                VehicleComponentsMenu.GoBack();
                            };
                        }
                        // And update the submenu to prevent weird glitches.
                        VehicleComponentsMenu.RefreshIndex();
                        //VehicleComponentsMenu.UpdateScaleform();

                    }
                }
            };
            // when a checkbox in the components menu changes
            VehicleComponentsMenu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                // When a checkbox is checked/unchecked, get the selected checkbox item index and use that to get the component ID from the list.
                // Then toggle that extra.
                if (vehicleExtras.TryGetValue(item, out int extra))
                {
                    Vehicle veh = GetVehicle();
                    veh.ToggleExtra(extra, _checked);
                }
            };
            #endregion

            #region Underglow Submenu
            MenuCheckboxItem underglowFront = new MenuCheckboxItem("啟用前燈", "啟用或禁用載具前燈。 注意並非所有載具都有燈.", false);
            MenuCheckboxItem underglowBack = new MenuCheckboxItem("啟用尾燈", "啟用或禁用載具尾燈。 注意並非所有載具都有燈.", false);
            MenuCheckboxItem underglowLeft = new MenuCheckboxItem("啟用左燈", "啟用或禁用載具左燈。 注意並非所有載具都有燈.", false);
            MenuCheckboxItem underglowRight = new MenuCheckboxItem("啟用右燈", "啟用或禁用載具右燈。 注意並非所有載具都有燈.", false);
            var underglowColorsList = new List<string>();
            for (int i = 0; i < 13; i++)
            {
                underglowColorsList.Add(GetLabelText($"CMOD_NEONCOL_{i}"));
            }
            MenuListItem underglowColor = new MenuListItem(GetLabelText("CMOD_NEON_1"), underglowColorsList, 0, "Select the color of the neon underglow.");

            VehicleUnderglowMenu.AddMenuItem(underglowFront);
            VehicleUnderglowMenu.AddMenuItem(underglowBack);
            VehicleUnderglowMenu.AddMenuItem(underglowLeft);
            VehicleUnderglowMenu.AddMenuItem(underglowRight);

            VehicleUnderglowMenu.AddMenuItem(underglowColor);

            menu.OnItemSelect += (sender, item, index) =>
            {
                #region reset checkboxes state when opening the menu.
                if (item == underglowMenuBtn)
                {
                    Vehicle veh = GetVehicle();
                    if (veh != null)
                    {
                        if (veh.Mods.HasNeonLights)
                        {
                            underglowFront.Checked = veh.Mods.HasNeonLight(VehicleNeonLight.Front) && veh.Mods.IsNeonLightsOn(VehicleNeonLight.Front);
                            underglowBack.Checked = veh.Mods.HasNeonLight(VehicleNeonLight.Back) && veh.Mods.IsNeonLightsOn(VehicleNeonLight.Back);
                            underglowLeft.Checked = veh.Mods.HasNeonLight(VehicleNeonLight.Left) && veh.Mods.IsNeonLightsOn(VehicleNeonLight.Left);
                            underglowRight.Checked = veh.Mods.HasNeonLight(VehicleNeonLight.Right) && veh.Mods.IsNeonLightsOn(VehicleNeonLight.Right);

                            underglowFront.Enabled = true;
                            underglowBack.Enabled = true;
                            underglowLeft.Enabled = true;
                            underglowRight.Enabled = true;

                            underglowFront.LeftIcon = MenuItem.Icon.NONE;
                            underglowBack.LeftIcon = MenuItem.Icon.NONE;
                            underglowLeft.LeftIcon = MenuItem.Icon.NONE;
                            underglowRight.LeftIcon = MenuItem.Icon.NONE;
                        }
                        else
                        {
                            underglowFront.Checked = false;
                            underglowBack.Checked = false;
                            underglowLeft.Checked = false;
                            underglowRight.Checked = false;

                            underglowFront.Enabled = false;
                            underglowBack.Enabled = false;
                            underglowLeft.Enabled = false;
                            underglowRight.Enabled = false;

                            underglowFront.LeftIcon = MenuItem.Icon.LOCK;
                            underglowBack.LeftIcon = MenuItem.Icon.LOCK;
                            underglowLeft.LeftIcon = MenuItem.Icon.LOCK;
                            underglowRight.LeftIcon = MenuItem.Icon.LOCK;
                        }
                    }
                    else
                    {
                        underglowFront.Checked = false;
                        underglowBack.Checked = false;
                        underglowLeft.Checked = false;
                        underglowRight.Checked = false;

                        underglowFront.Enabled = false;
                        underglowBack.Enabled = false;
                        underglowLeft.Enabled = false;
                        underglowRight.Enabled = false;

                        underglowFront.LeftIcon = MenuItem.Icon.LOCK;
                        underglowBack.LeftIcon = MenuItem.Icon.LOCK;
                        underglowLeft.LeftIcon = MenuItem.Icon.LOCK;
                        underglowRight.LeftIcon = MenuItem.Icon.LOCK;
                    }

                    underglowColor.ListIndex = GetIndexFromColor();
                }
                #endregion
            };
            // handle item selections
            VehicleUnderglowMenu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (Game.PlayerPed.IsInVehicle())
                {
                    Vehicle veh = GetVehicle();
                    if (veh.Mods.HasNeonLights)
                    {
                        veh.Mods.NeonLightsColor = GetColorFromIndex(underglowColor.ListIndex);
                        if (item == underglowLeft)
                        {
                            veh.Mods.SetNeonLightsOn(VehicleNeonLight.Left, veh.Mods.HasNeonLight(VehicleNeonLight.Left) && _checked);
                        }
                        else if (item == underglowRight)
                        {
                            veh.Mods.SetNeonLightsOn(VehicleNeonLight.Right, veh.Mods.HasNeonLight(VehicleNeonLight.Right) && _checked);
                        }
                        else if (item == underglowBack)
                        {
                            veh.Mods.SetNeonLightsOn(VehicleNeonLight.Back, veh.Mods.HasNeonLight(VehicleNeonLight.Back) && _checked);
                        }
                        else if (item == underglowFront)
                        {
                            veh.Mods.SetNeonLightsOn(VehicleNeonLight.Front, veh.Mods.HasNeonLight(VehicleNeonLight.Front) && _checked);
                        }
                    }
                }
            };

            VehicleUnderglowMenu.OnListIndexChange += (sender, item, oldIndex, newIndex, itemIndex) =>
            {
                if (item == underglowColor)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        Vehicle veh = GetVehicle();
                        if (veh.Mods.HasNeonLights)
                        {
                            veh.Mods.NeonLightsColor = GetColorFromIndex(newIndex);
                        }
                    }
                }
            };
            #endregion

            #region Handle menu-opening refreshing license plate
            menu.OnMenuOpen += (sender) =>
            {
                menu.GetMenuItems().ForEach((item) =>
                {
                    var veh = GetVehicle(true);

                    if (item == setLicensePlateType && item is MenuListItem listItem && veh != null && veh.Exists())
                    {
                        // Set the license plate style.
                        switch (veh.Mods.LicensePlateStyle)
                        {
                            case LicensePlateStyle.BlueOnWhite1:
                                listItem.ListIndex = 0;
                                break;
                            case LicensePlateStyle.BlueOnWhite2:
                                listItem.ListIndex = 1;
                                break;
                            case LicensePlateStyle.BlueOnWhite3:
                                listItem.ListIndex = 2;
                                break;
                            case LicensePlateStyle.YellowOnBlue:
                                listItem.ListIndex = 3;
                                break;
                            case LicensePlateStyle.YellowOnBlack:
                                listItem.ListIndex = 4;
                                break;
                            case LicensePlateStyle.NorthYankton:
                                listItem.ListIndex = 5;
                                break;
                            default:
                                break;
                        }
                    }
                });
            };
            #endregion

        }
        #endregion

        /// <summary>
        /// Public get method for the menu. Checks if the menu exists, if not create the menu first.
        /// </summary>
        /// <returns>Returns the Vehicle Options menu.</returns>
        public Menu GetMenu()
        {
            // If menu doesn't exist. Create one.
            if (menu == null)
            {
                CreateMenu();
            }
            // Return the menu.
            return menu;
        }

        #region Update Vehicle Mods Menu
        /// <summary>
        /// Refreshes the mods page. The selectedIndex allows you to go straight to a specific index after refreshing the menu.
        /// This is used because when the wheel type is changed, the menu is refreshed to update the available wheels list.
        /// </summary>
        /// <param name="selectedIndex">Pass this if you want to go straight to a specific mod/index.</param>
        public void UpdateMods(int selectedIndex = 0)
        {
            // If there are items, remove all of them.
            if (VehicleModMenu.Size > 0)
            {
                if (selectedIndex != 0)
                {
                    VehicleModMenu.ClearMenuItems(true);
                }
                else
                {
                    VehicleModMenu.ClearMenuItems(false);
                }

            }

            // Get the vehicle.
            Vehicle veh = GetVehicle();

            // Check if the vehicle exists, is still drivable/alive and it's actually a vehicle.
            if (veh != null && veh.Exists() && !veh.IsDead)
            {
                #region initial setup & dynamic vehicle mods setup
                // Set the modkit so we can modify the car.
                SetVehicleModKit(veh.Handle, 0);

                // Get all mods available on this vehicle.
                VehicleMod[] mods = veh.Mods.GetAllMods();

                // Loop through all the mods.
                foreach (var mod in mods)
                {
                    veh = GetVehicle();

                    // Get the proper localized mod type (suspension, armor, etc) name.
                    var typeName = mod.LocalizedModTypeName;

                    // Create a list to all available upgrades for this modtype.
                    var modlist = new List<string>();

                    // Get the current item index ({current}/{max upgrades})
                    var currentItem = $"[1/{ mod.ModCount + 1}]";

                    // Add the stock value for this mod.
                    var name = $"stock {typeName} {currentItem}";
                    modlist.Add(name);

                    // Loop through all available upgrades for this specific mod type.
                    for (var x = 0; x < mod.ModCount; x++)
                    {
                        // Create the item index.
                        currentItem = $"[{2 + x}/{ mod.ModCount + 1}]";

                        // Create the name (again, converting to proper case), then add the name.
                        name = mod.GetLocalizedModName(x) != "" ? $"{ToProperString(mod.GetLocalizedModName(x))} {currentItem}" : $"{typeName} #{x} {currentItem}";
                        modlist.Add(name);
                    }

                    // Create the MenuListItem for this mod type.
                    var currIndex = GetVehicleMod(veh.Handle, (int)mod.ModType) + 1;
                    MenuListItem modTypeListItem = new MenuListItem(typeName, modlist, currIndex, $"選擇一個 ~y~{typeName}~s~ 升級, 它將會自動套用到您的載具上.");

                    // Add the list item to the menu.
                    VehicleModMenu.AddMenuItem(modTypeListItem);
                }
                #endregion

                #region more variables and setup
                veh = GetVehicle();
                // Create the wheel types list & listitem and add it to the menu.
                List<string> wheelTypes = new List<string>() { "Sports", "Muscle", "Lowrider", "SUV", "Offroad", "Tuner", "Bike Wheels", "High End", "Benny's (1)", "Benny's (2)" };
                MenuListItem vehicleWheelType = new MenuListItem("輪胎型態", wheelTypes, MathUtil.Clamp(GetVehicleWheelType(veh.Handle), 0, 9), $"為您的載具選擇 ~y~輪胎~s~");
                if (!veh.Model.IsBoat && !veh.Model.IsHelicopter && !veh.Model.IsPlane && !veh.Model.IsBicycle && !veh.Model.IsTrain)
                {
                    VehicleModMenu.AddMenuItem(vehicleWheelType);
                }

                // Create the checkboxes for some options.
                MenuCheckboxItem toggleCustomWheels = new MenuCheckboxItem("自定義輪胎", "可以對您的載具管理 ~y~自定義輪胎~s~.", GetVehicleModVariation(veh.Handle, 23));
                MenuCheckboxItem xenonHeadlights = new MenuCheckboxItem("氙氣大燈", "開關 ~b~氙氣大燈 ~s~.", IsToggleModOn(veh.Handle, 22));
                MenuCheckboxItem turbo = new MenuCheckboxItem("渦輪", "開關 ~y~渦輪~s~.", IsToggleModOn(veh.Handle, 18));
                MenuCheckboxItem bulletProofTires = new MenuCheckboxItem("防彈輪胎", "開關 ~y~防彈輪胎~s~.", !GetVehicleTyresCanBurst(veh.Handle));

                // Add the checkboxes to the menu.
                VehicleModMenu.AddMenuItem(toggleCustomWheels);
                VehicleModMenu.AddMenuItem(xenonHeadlights);
                int currentHeadlightColor = _GetHeadlightsColorFromVehicle(veh);
                if (currentHeadlightColor < 0 || currentHeadlightColor > 12)
                {
                    currentHeadlightColor = 13;
                }
                MenuListItem headlightColor = new MenuListItem("大燈顏色", new List<string>() { "White", "Blue", "Electric Blue", "Mint Green", "Lime Green", "Yellow", "Golden Shower", "Orange", "Red", "Pony Pink", "Hot Pink", "Purple", "Blacklight", "Default Xenon" }, currentHeadlightColor, "《俠盜列車手5》更新中的新功能：彩色大燈。 請注意，您必須先啟用氙氣大燈.");
                VehicleModMenu.AddMenuItem(headlightColor);
                VehicleModMenu.AddMenuItem(turbo);
                VehicleModMenu.AddMenuItem(bulletProofTires);
                // Create a list of tire smoke options.
                List<string> tireSmokes = new List<string>() { "Red", "Orange", "Yellow", "Gold", "Light Green", "Dark Green", "Light Blue", "Dark Blue", "Purple", "Pink", "Black" };
                Dictionary<string, int[]> tireSmokeColors = new Dictionary<string, int[]>()
                {
                    ["Red"] = new int[] { 244, 65, 65 },
                    ["Orange"] = new int[] { 244, 167, 66 },
                    ["Yellow"] = new int[] { 244, 217, 65 },
                    ["Gold"] = new int[] { 181, 120, 0 },
                    ["Light Green"] = new int[] { 158, 255, 84 },
                    ["Dark Green"] = new int[] { 44, 94, 5 },
                    ["Light Blue"] = new int[] { 65, 211, 244 },
                    ["Dark Blue"] = new int[] { 24, 54, 163 },
                    ["Purple"] = new int[] { 108, 24, 192 },
                    ["Pink"] = new int[] { 192, 24, 172 },
                    ["Black"] = new int[] { 1, 1, 1 }
                };
                int smoker = 0, smokeg = 0, smokeb = 0;
                GetVehicleTyreSmokeColor(veh.Handle, ref smoker, ref smokeg, ref smokeb);
                var item = tireSmokeColors.ToList().Find((f) => { return (f.Value[0] == smoker && f.Value[1] == smokeg && f.Value[2] == smokeb); });
                int index = tireSmokeColors.ToList().IndexOf(item);
                if (index < 0)
                {
                    index = 0;
                }

                MenuListItem tireSmoke = new MenuListItem("燒胎煙霧顏色", tireSmokes, index, $"對載具選擇一個 ~y~燒胎顏色~s~.");
                VehicleModMenu.AddMenuItem(tireSmoke);

                // Create the checkbox to enable/disable the tiresmoke.
                MenuCheckboxItem tireSmokeEnabled = new MenuCheckboxItem("燒胎煙霧", "對載具 開關 ~y~燒胎煙霧~s~. ~h~~r~注意:~s~ 禁用輪胎煙霧時，您需要四處行駛，以免產生問題.", IsToggleModOn(veh.Handle, 20));
                VehicleModMenu.AddMenuItem(tireSmokeEnabled);

                // Create list for window tint
                List<string> windowTints = new List<string>() { "Stock [1/7]", "None [2/7]", "Limo [3/7]", "Light Smoke [4/7]", "Dark Smoke [5/7]", "Pure Black [6/7]", "Green [7/7]" };
                var currentTint = GetVehicleWindowTint(veh.Handle);
                if (currentTint == -1)
                {
                    currentTint = 4; // stock
                }

                // Convert window tint to the correct index of the list above.
                switch (currentTint)
                {
                    case 0:
                        currentTint = 1; // None
                        break;
                    case 1:
                        currentTint = 5; // Pure Black
                        break;
                    case 2:
                        currentTint = 4; // Dark Smoke
                        break;
                    case 3:
                        currentTint = 3; // Light Smoke
                        break;
                    case 4:
                        currentTint = 0; // Stock
                        break;
                    case 5:
                        currentTint = 2; // Limo
                        break;
                    case 6:
                        currentTint = 6; // Green
                        break;
                    default:
                        break;
                }

                MenuListItem windowTint = new MenuListItem("窗口色調", windowTints, currentTint, "給窗戶塗上顏色");
                VehicleModMenu.AddMenuItem(windowTint);

                #endregion

                #region Checkbox Changes
                // Handle checkbox changes.
                VehicleModMenu.OnCheckboxChange += (sender2, item2, index2, _checked) =>
                {
                    veh = GetVehicle();

                    // Xenon Headlights
                    if (item2 == xenonHeadlights)
                    {
                        ToggleVehicleMod(veh.Handle, 22, _checked);
                    }
                    // Turbo
                    else if (item2 == turbo)
                    {
                        ToggleVehicleMod(veh.Handle, 18, _checked);
                    }
                    // Bullet Proof Tires
                    else if (item2 == bulletProofTires)
                    {
                        SetVehicleTyresCanBurst(veh.Handle, !_checked);
                    }
                    // Custom Wheels
                    else if (item2 == toggleCustomWheels)
                    {
                        SetVehicleMod(veh.Handle, 23, GetVehicleMod(veh.Handle, 23), !GetVehicleModVariation(veh.Handle, 23));

                        // If the player is on a motorcycle, also change the back wheels.
                        if (IsThisModelABike((uint)GetEntityModel(veh.Handle)))
                        {
                            SetVehicleMod(veh.Handle, 24, GetVehicleMod(veh.Handle, 24), GetVehicleModVariation(veh.Handle, 23));
                        }
                    }
                    // Toggle Tire Smoke
                    else if (item2 == tireSmokeEnabled)
                    {
                        // If it should be enabled:
                        if (_checked)
                        {
                            // Enable it.
                            ToggleVehicleMod(veh.Handle, 20, true);
                            // Get the selected color values.
                            var r = tireSmokeColors[tireSmokes[tireSmoke.ListIndex]][0];
                            var g = tireSmokeColors[tireSmokes[tireSmoke.ListIndex]][1];
                            var b = tireSmokeColors[tireSmokes[tireSmoke.ListIndex]][2];
                            // Set the color.
                            SetVehicleTyreSmokeColor(veh.Handle, r, g, b);
                        }
                        // If it should be disabled:
                        else
                        {
                            // Set the smoke to white.
                            SetVehicleTyreSmokeColor(veh.Handle, 255, 255, 255);
                            // Disable it.
                            ToggleVehicleMod(veh.Handle, 20, false);
                            // Remove the mod.
                            RemoveVehicleMod(veh.Handle, 20);
                        }
                    }
                };
                #endregion

                #region List Changes
                // Handle list selections
                VehicleModMenu.OnListIndexChange += (sender2, item2, oldIndex, newIndex, itemIndex) =>
                {
                    // Get the vehicle and set the mod kit.
                    veh = GetVehicle();
                    SetVehicleModKit(veh.Handle, 0);

                    #region handle the dynamic (vehicle-specific) mods
                    // If the affected list is actually a "dynamically" generated list, continue. If it was one of the manual options, go to else.
                    if (itemIndex < sender2.Size - 9)
                    {
                        // Get all mods available on this vehicle.
                        mods = veh.Mods.GetAllMods();

                        var dict = new Dictionary<int, int>();
                        var x = 0;

                        foreach (var mod in mods)
                        {
                            dict.Add(x, (int)mod.ModType);
                            x++;
                        }

                        int modType = dict[itemIndex];
                        int selectedUpgrade = item2.ListIndex - 1;
                        bool customWheels = GetVehicleModVariation(veh.Handle, 23);

                        SetVehicleMod(veh.Handle, modType, selectedUpgrade, customWheels);
                    }
                    #endregion
                    // If it was not one of the lists above, then it was one of the manual lists/options selected, 
                    // either: vehicle Wheel Type, tire smoke color, or window tint:
                    #region Handle the items available on all vehicles.
                    // Wheel types
                    else if (item2 == vehicleWheelType)
                    {
                        // 6 should be used for bikes only.
                        if ((newIndex == 6 && veh.Model.IsBike) || (newIndex != 6 && !veh.Model.IsBike))
                        {
                            // Set the wheel type
                            SetVehicleWheelType(veh.Handle, newIndex);

                            bool customWheels = GetVehicleModVariation(veh.Handle, 23);

                            // Reset the wheel mod index for front wheels
                            SetVehicleMod(veh.Handle, 23, -1, customWheels);

                            // If the model is a bike, do the same thing for the rear wheels.
                            if (veh.Model.IsBike)
                            {
                                SetVehicleMod(veh.Handle, 24, -1, customWheels);
                            }

                            // Refresh the menu with the item index so that the view doesn't change
                            UpdateMods(selectedIndex: itemIndex);
                        }
                        else
                        {
                            // Go past the index if it's not a bike.
                            if (!veh.Model.IsBike)
                            {
                                if (newIndex > oldIndex)
                                {
                                    item2.ListIndex++;
                                }
                                else
                                {
                                    item2.ListIndex--;
                                }
                            }
                            // Reset the index to 6 if it is a bike
                            else
                            {
                                item2.ListIndex = 6;
                            }
                        }
                    }
                    // Tire smoke
                    else if (item2 == tireSmoke)
                    {
                        // Get the selected color values.
                        var r = tireSmokeColors[tireSmokes[newIndex]][0];
                        var g = tireSmokeColors[tireSmokes[newIndex]][1];
                        var b = tireSmokeColors[tireSmokes[newIndex]][2];

                        // Set the color.
                        SetVehicleTyreSmokeColor(veh.Handle, r, g, b);
                    }
                    // Window Tint
                    else if (item2 == windowTint)
                    {
                        // Stock = 4,
                        // None = 0,
                        // Limo = 5,
                        // LightSmoke = 3,
                        // DarkSmoke = 2,
                        // PureBlack = 1,
                        // Green = 6,

                        switch (newIndex)
                        {
                            case 1:
                                SetVehicleWindowTint(veh.Handle, 0); // None
                                break;
                            case 2:
                                SetVehicleWindowTint(veh.Handle, 5); // Limo
                                break;
                            case 3:
                                SetVehicleWindowTint(veh.Handle, 3); // Light Smoke
                                break;
                            case 4:
                                SetVehicleWindowTint(veh.Handle, 2); // Dark Smoke
                                break;
                            case 5:
                                SetVehicleWindowTint(veh.Handle, 1); // Pure Black
                                break;
                            case 6:
                                SetVehicleWindowTint(veh.Handle, 6); // Green
                                break;
                            case 0:
                            default:
                                SetVehicleWindowTint(veh.Handle, 4); // Stock
                                break;
                        }
                    }
                    else if (item2 == headlightColor)
                    {
                        if (newIndex == 13) // default
                        {
                            _SetHeadlightsColorOnVehicle(veh, 255);
                        }
                        else if (newIndex > -1 && newIndex < 13)
                        {
                            _SetHeadlightsColorOnVehicle(veh, newIndex);
                        }
                    }
                    #endregion
                };

                #endregion
            }
            // Refresh Index and update the scaleform to prevent weird broken menus.
            if (selectedIndex == 0)
            {
                VehicleModMenu.RefreshIndex();
            }

            //VehicleModMenu.UpdateScaleform();

            // Set the selected index to the provided index (0 by default)
            // Used for example, when the wheelstype is changed, the menu is refreshed and we want to set the
            // selected item back to the "wheelsType" list so the user doesn't have to scroll down each time they
            // change the wheels type.
            //VehicleModMenu.CurrentIndex = selectedIndex;
        }

        internal static void _SetHeadlightsColorOnVehicle(Vehicle veh, int newIndex)
        {
            if (veh != null && veh.Exists() && veh.Driver == Game.PlayerPed)
            {
                if (newIndex > -1 && newIndex < 13)
                {
                    SetVehicleHeadlightsColour(veh.Handle, newIndex);
                }
                else
                {
                    SetVehicleHeadlightsColour(veh.Handle, newIndex);
                }
            }
        }

        internal static int _GetHeadlightsColorFromVehicle(Vehicle vehicle)
        {
            if (vehicle != null && vehicle.Exists())
            {
                if (IsToggleModOn(vehicle.Handle, 22))
                {
                    int val = GetVehicleHeadlightsColour(vehicle.Handle);
                    if (val > -1 && val < 13)
                    {
                        return val;
                    }
                    return -1;
                }
            }
            return -1;
        }
        #endregion

        #region GetColorFromIndex function (underglow)

        private readonly List<int[]> _VehicleNeonLightColors = new List<int[]>()
        {
            { new int[3] { 255, 255, 255 } },   // White
            { new int[3] { 2, 21, 255 } },      // Blue
            { new int[3] { 3, 83, 255 } },      // Electric blue
            { new int[3] { 0, 255, 140 } },     // Mint Green
            { new int[3] { 94, 255, 1 } },      // Lime Green
            { new int[3] { 255, 255, 0 } },     // Yellow
            { new int[3] { 255, 150, 5 } },     // Golden Shower
            { new int[3] { 255, 62, 0 } },      // Orange
            { new int[3] { 255, 0, 0 } },       // Red
            { new int[3] { 255, 50, 100 } },    // Pony Pink
            { new int[3] { 255, 5, 190 } },     // Hot Pink
            { new int[3] { 35, 1, 255 } },      // Purple
            { new int[3] { 15, 3, 255 } },      // Blacklight
        };

        /// <summary>
        /// Converts a list index to a <see cref="System.Drawing.Color"/> struct.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private System.Drawing.Color GetColorFromIndex(int index)
        {
            if (index >= 0 && index < 13)
            {
                return System.Drawing.Color.FromArgb(_VehicleNeonLightColors[index][0], _VehicleNeonLightColors[index][1], _VehicleNeonLightColors[index][2]);
            }
            return System.Drawing.Color.FromArgb(255, 255, 255);
        }

        /// <summary>
        /// Returns the color index that is applied on the current vehicle. 
        /// If a color is active on the vehicle which is not in the list, it'll return the default index 0 (white).
        /// </summary>
        /// <returns></returns>
        private int GetIndexFromColor()
        {
            Vehicle veh = GetVehicle();

            if (veh == null || !veh.Exists() || !veh.Mods.HasNeonLights)
            {
                return 0;
            }

            int r = 255, g = 255, b = 255;

            GetVehicleNeonLightsColour(veh.Handle, ref r, ref g, ref b);

            if (r == 255 && g == 0 && b == 255) // default return value when the vehicle has no neon kit selected.
            {
                return 0;
            }

            if (_VehicleNeonLightColors.Any(a => { return a[0] == r && a[1] == g && a[2] == b; }))
            {
                return _VehicleNeonLightColors.FindIndex(a => { return a[0] == r && a[1] == g && a[2] == b; });
            }

            return 0;
        }
        #endregion
    }
}
