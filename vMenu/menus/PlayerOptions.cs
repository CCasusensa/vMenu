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
    public class PlayerOptions
    {
        // Menu variable, will be defined in CreateMenu()
        private Menu menu;

        // Public variables (getters only), return the private variables.
        public bool PlayerGodMode { get; private set; } = UserDefaults.PlayerGodMode;
        public bool PlayerInvisible { get; private set; } = false;
        public bool PlayerStamina { get; private set; } = UserDefaults.UnlimitedStamina;
        public bool PlayerFastRun { get; private set; } = UserDefaults.FastRun;
        public bool PlayerFastSwim { get; private set; } = UserDefaults.FastSwim;
        public bool PlayerSuperJump { get; private set; } = UserDefaults.SuperJump;
        public bool PlayerNoRagdoll { get; private set; } = UserDefaults.NoRagdoll;
        public bool PlayerNeverWanted { get; private set; } = UserDefaults.NeverWanted;
        public bool PlayerIsIgnored { get; private set; } = UserDefaults.EveryoneIgnorePlayer;
        public bool PlayerStayInVehicle { get; private set; } = UserDefaults.PlayerStayInVehicle;
        public bool PlayerFrozen { get; private set; } = false;
        private Menu CustomDrivingStyleMenu = new Menu("駕駛風格", "自訂駕駛風格");

        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            #region create menu and menu items
            // Create the menu.
            menu = new Menu(Game.Player.Name, "玩家選單");

            // Create all checkboxes.
            MenuCheckboxItem playerGodModeCheckbox = new MenuCheckboxItem("無敵", "使您變無敵.", PlayerGodMode);
            MenuCheckboxItem invisibleCheckbox = new MenuCheckboxItem("隱身", "使您隱身，其他人看不見.", PlayerInvisible);
            MenuCheckboxItem unlimitedStaminaCheckbox = new MenuCheckboxItem("無限體力", "使您可以永遠的跑步，而不會疲勞造成減速.", PlayerStamina);
            MenuCheckboxItem fastRunCheckbox = new MenuCheckboxItem("快速跑步", "獲得 ~g~超級~s~ 的力量快速游泳!", PlayerFastRun);
            SetRunSprintMultiplierForPlayer(Game.Player.Handle, (PlayerFastRun && IsAllowed(Permission.POFastRun) ? 1.49f : 1f));
            MenuCheckboxItem fastSwimCheckbox = new MenuCheckboxItem("快速游泳", "獲得 ~g~超級 2.0~s~ 的游泳速度!", PlayerFastSwim);
            SetSwimMultiplierForPlayer(Game.Player.Handle, (PlayerFastSwim && IsAllowed(Permission.POFastSwim) ? 1.49f : 1f));
            MenuCheckboxItem superJumpCheckbox = new MenuCheckboxItem("超級跳躍", "獲得 ~g~超級 3.0~s~ 的力量跳躍!", PlayerSuperJump);
            MenuCheckboxItem noRagdollCheckbox = new MenuCheckboxItem("停止摔車", "發生車禍時不會再摔車.", PlayerNoRagdoll);
            MenuCheckboxItem neverWantedCheckbox = new MenuCheckboxItem("永不通輯", "永遠都不會有通緝.", PlayerNeverWanted);
            MenuCheckboxItem everyoneIgnoresPlayerCheckbox = new MenuCheckboxItem("無視所有玩家", "所有人遠離此玩家.", PlayerIsIgnored);
            MenuCheckboxItem playerStayInVehicleCheckbox = new MenuCheckboxItem("待在車內", "啟用功能後，如果NPC對您發火，無法將您拖出車外.", PlayerStayInVehicle);
            MenuCheckboxItem playerFrozenCheckbox = new MenuCheckboxItem("凍結玩家", "把該玩家定住在原地.", PlayerFrozen);

            // Wanted level options
            List<string> wantedLevelList = new List<string> { "0", "1", "2", "3", "4", "5" };
            MenuListItem setWantedLevel = new MenuListItem("設置通緝等級", wantedLevelList, GetPlayerWantedLevel(Game.Player.Handle), "選擇一個值來設置目前通緝等輯");
            MenuListItem setArmorItem = new MenuListItem("設置護甲", new List<string> { "無護甲", GetLabelText("WT_BA_0"), GetLabelText("WT_BA_1"), GetLabelText("WT_BA_2"), GetLabelText("WT_BA_3"), GetLabelText("WT_BA_4"), }, 0, "Set the armor level/type for your player.");

            MenuItem healPlayerBtn = new MenuItem("治癒玩家", "給予玩家最大生命值.");
            MenuItem cleanPlayerBtn = new MenuItem("清潔衣服", "清潔衣服.");
            MenuItem dryPlayerBtn = new MenuItem("烘乾衣服", "烘乾衣服.");
            MenuItem wetPlayerBtn = new MenuItem("撥濕衣服", "撥濕衣服.");
            MenuItem suicidePlayerBtn = new MenuItem("~r~自殺", "服用藥物殺死自己，或者如過手上有手槍將會開槍自盡.");

            Menu vehicleAutoPilot = new Menu("自動駕駛", "管理載具自動駕駛選項.");

            MenuController.AddSubmenu(menu, vehicleAutoPilot);

            MenuItem vehicleAutoPilotBtn = new MenuItem("自動駕駛選單", "管理載具自動駕駛選項.")
            {
                Label = "→→→"
            };

            List<string> drivingStyles = new List<string>() { "正常", "衝刺", "避開高速公路", "倒車", "自訂" };
            MenuListItem drivingStyle = new MenuListItem("駕駛風格", drivingStyles, 0, "將用於'駕駛' 的駕駛方式設置為'導航點'和'隨機駕駛'");

            // Scenarios (list can be found in the PedScenarios class)
            MenuListItem playerScenarios = new MenuListItem("玩家場景", PedScenarios.Scenarios, 0, "選擇一個場景，然後按Enter鍵以啟動它。 選擇另一個方案將覆蓋當前方案。 如果您已經在玩家選定的場景，再次選擇它將會停止該場景.");
            MenuItem stopScenario = new MenuItem("強制停止場景", "這將迫使正在播放的場景立即停止，而無需等待其完成“停止”動畫.");
            #endregion

            #region add items to menu based on permissions
            // Add all checkboxes to the menu. (keeping permissions in mind)
            if (IsAllowed(Permission.POGod))
            {
                menu.AddMenuItem(playerGodModeCheckbox);
            }
            if (IsAllowed(Permission.POInvisible))
            {
                menu.AddMenuItem(invisibleCheckbox);
            }
            if (IsAllowed(Permission.POUnlimitedStamina))
            {
                menu.AddMenuItem(unlimitedStaminaCheckbox);
            }
            if (IsAllowed(Permission.POFastRun))
            {
                menu.AddMenuItem(fastRunCheckbox);
            }
            if (IsAllowed(Permission.POFastSwim))
            {
                menu.AddMenuItem(fastSwimCheckbox);
            }
            if (IsAllowed(Permission.POSuperjump))
            {
                menu.AddMenuItem(superJumpCheckbox);
            }
            if (IsAllowed(Permission.PONoRagdoll))
            {
                menu.AddMenuItem(noRagdollCheckbox);
            }
            if (IsAllowed(Permission.PONeverWanted))
            {
                menu.AddMenuItem(neverWantedCheckbox);
            }
            if (IsAllowed(Permission.POSetWanted))
            {
                menu.AddMenuItem(setWantedLevel);
            }
            if (IsAllowed(Permission.POIgnored))
            {
                menu.AddMenuItem(everyoneIgnoresPlayerCheckbox);
            }
            if (IsAllowed(Permission.POStayInVehicle))
            {
                menu.AddMenuItem(playerStayInVehicleCheckbox);
            }
            if (IsAllowed(Permission.POMaxHealth))
            {
                menu.AddMenuItem(healPlayerBtn);
            }
            if (IsAllowed(Permission.POMaxArmor))
            {
                menu.AddMenuItem(setArmorItem);
            }
            if (IsAllowed(Permission.POCleanPlayer))
            {
                menu.AddMenuItem(cleanPlayerBtn);
            }
            if (IsAllowed(Permission.PODryPlayer))
            {
                menu.AddMenuItem(dryPlayerBtn);
            }
            if (IsAllowed(Permission.POWetPlayer))
            {
                menu.AddMenuItem(wetPlayerBtn);
            }

            menu.AddMenuItem(suicidePlayerBtn);

            if (IsAllowed(Permission.POVehicleAutoPilotMenu))
            {
                menu.AddMenuItem(vehicleAutoPilotBtn);
                MenuController.BindMenuItem(menu, vehicleAutoPilot, vehicleAutoPilotBtn);

                vehicleAutoPilot.AddMenuItem(drivingStyle);

                MenuItem startDrivingWaypoint = new MenuItem("開至導航點", "開到地圖上的標記點.");
                MenuItem startDrivingRandomly = new MenuItem("隨機行駛", "在地圖上隨便亂開.");
                MenuItem stopDriving = new MenuItem("停止行駛", "會先找到一個停靠點在停止行使.");
                MenuItem forceStopDriving = new MenuItem("強制停止行駛", "立即停在駕駛");
                MenuItem customDrivingStyle = new MenuItem("自訂駕駛風格", "選擇自訂駕駛風格.") { Label = "→→→" };
                MenuController.AddSubmenu(vehicleAutoPilot, CustomDrivingStyleMenu);
                vehicleAutoPilot.AddMenuItem(customDrivingStyle);
                MenuController.BindMenuItem(vehicleAutoPilot, CustomDrivingStyleMenu, customDrivingStyle);
                Dictionary<int, string> knownNames = new Dictionary<int, string>()
                {
                    { 0, "在車輛前停車" },
                    { 1, "在人之前停車" },
                    { 2, "避開車輛" },
                    { 3, "避開空車" },
                    { 4, "避開人" },
                    { 5, "避開物體" },

                    { 7, "在紅綠燈處停下" },
                    { 8, "使用紅綠燈" },
                    { 9, "允行開錯路" },
                    { 10, "倒檔" },

                    { 18, "使用最短程的捷徑" },

                    { 22, "無視道路" },

                    { 24, "無視所有捷徑" },

                    { 29, "避免行駛高速公路 (如果可能的話)" },
                };
                for (var i = 0; i < 31; i++)
                {
                    string name = "~r~未知選項";
                    if (knownNames.ContainsKey(i))
                    {
                        name = knownNames[i];
                    }
                    MenuCheckboxItem checkbox = new MenuCheckboxItem(name, "開關駕駛風格選項", false);
                    CustomDrivingStyleMenu.AddMenuItem(checkbox);
                }
                CustomDrivingStyleMenu.OnCheckboxChange += (sender, item, index, _checked) =>
                {
                    int style = GetStyleFromIndex(drivingStyle.ListIndex);
                    CustomDrivingStyleMenu.MenuSubtitle = $"自訂駕駛風格: {style}";
                    if (drivingStyle.ListIndex == 4)
                    {
                        Notify.Custom("駕駛風格已更新");
                        SetDriveTaskDrivingStyle(Game.PlayerPed.Handle, style);
                    }
                    else
                    {
                        Notify.Custom("駕駛風格未更新，因為您沒有在上一個選單中啟用自定義駕駛風格.");
                    }
                };

                vehicleAutoPilot.AddMenuItem(startDrivingWaypoint);
                vehicleAutoPilot.AddMenuItem(startDrivingRandomly);
                vehicleAutoPilot.AddMenuItem(stopDriving);
                vehicleAutoPilot.AddMenuItem(forceStopDriving);

                vehicleAutoPilot.RefreshIndex();

                vehicleAutoPilot.OnItemSelect += async (sender, item, index) =>
                {
                    if (Game.PlayerPed.IsInVehicle() && item != stopDriving && item != forceStopDriving)
                    {
                        if (Game.PlayerPed.CurrentVehicle != null && Game.PlayerPed.CurrentVehicle.Exists() && !Game.PlayerPed.CurrentVehicle.IsDead && Game.PlayerPed.CurrentVehicle.IsDriveable)
                        {
                            if (Game.PlayerPed.CurrentVehicle.Driver == Game.PlayerPed)
                            {
                                if (item == startDrivingWaypoint)
                                {
                                    if (IsWaypointActive())
                                    {
                                        int style = GetStyleFromIndex(drivingStyle.ListIndex);
                                        DriveToWp(style);
                                        Notify.Info("您的司機現在正在為您駕駛車輛。 您可以隨時按“停止駕駛”按鈕取消。 車輛到達目的地後將停止.");
                                    }
                                    else
                                    {
                                        Notify.Error("您需要先上車!");
                                    }

                                }
                                else if (item == startDrivingRandomly)
                                {
                                    int style = GetStyleFromIndex(drivingStyle.ListIndex);
                                    DriveWander(style);
                                    Notify.Info("您的司機現在正在為您駕駛車輛。 您可以隨時按“停止駕駛”按鈕取消.");
                                }
                            }
                            else
                            {
                                Notify.Error("您必須是這輛車的駕駛員!");
                            }
                        }
                        else
                        {
                            Notify.Error("您的車輛損壞或不存在!");
                        }
                    }
                    else if (item != stopDriving && item != forceStopDriving)
                    {
                        Notify.Error("您需要先上車!");
                    }
                    if (item == stopDriving)
                    {
                        if (Game.PlayerPed.IsInVehicle())
                        {
                            Vehicle veh = GetVehicle();
                            if (veh != null && veh.Exists() && !veh.IsDead)
                            {
                                Vector3 outPos = new Vector3();
                                if (GetNthClosestVehicleNode(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, 3, ref outPos, 0, 0, 0))
                                {
                                    Notify.Info("司機將找到合適的停車位，然後停止駕駛。 請耐心等待.");
                                    ClearPedTasks(Game.PlayerPed.Handle);
                                    TaskVehiclePark(Game.PlayerPed.Handle, veh.Handle, outPos.X, outPos.Y, outPos.Z, Game.PlayerPed.Heading, 3, 60f, true);
                                    while (Game.PlayerPed.Position.DistanceToSquared2D(outPos) > 3f)
                                    {
                                        await BaseScript.Delay(0);
                                    }
                                    SetVehicleHalt(veh.Handle, 3f, 0, false);
                                    ClearPedTasks(Game.PlayerPed.Handle);
                                    Notify.Info("您的司機已經停止駕駛.");
                                }
                            }
                        }
                        else
                        {
                            ClearPedTasks(Game.PlayerPed.Handle);
                            Notify.Alert("您的司機不在任何車輛上.");
                        }
                    }
                    else if (item == forceStopDriving)
                    {
                        ClearPedTasks(Game.PlayerPed.Handle);
                        Notify.Info("駕駛任務已取消.");
                    }
                };

                vehicleAutoPilot.OnListItemSelect += (sender, item, listIndex, itemIndex) =>
                {
                    if (item == drivingStyle)
                    {
                        int style = GetStyleFromIndex(listIndex);
                        SetDriveTaskDrivingStyle(Game.PlayerPed.Handle, style);
                        Notify.Info($"駕駛風格現在設置為： ~r~{drivingStyles[listIndex]}~s~.");
                    }
                };
            }

            if (IsAllowed(Permission.POFreeze))
            {
                menu.AddMenuItem(playerFrozenCheckbox);
            }
            if (IsAllowed(Permission.POScenarios))
            {
                menu.AddMenuItem(playerScenarios);
                menu.AddMenuItem(stopScenario);
            }
            #endregion

            #region handle all events
            // Checkbox changes.
            menu.OnCheckboxChange += (sender, item, itemIndex, _checked) =>
            {
                // God Mode toggled.
                if (item == playerGodModeCheckbox)
                {
                    PlayerGodMode = _checked;
                }
                // Invisibility toggled.
                else if (item == invisibleCheckbox)
                {
                    PlayerInvisible = _checked;
                    SetEntityVisible(Game.PlayerPed.Handle, !PlayerInvisible, false);
                }
                // Unlimited Stamina toggled.
                else if (item == unlimitedStaminaCheckbox)
                {
                    PlayerStamina = _checked;
                    StatSetInt((uint)GetHashKey("MP0_STAMINA"), _checked ? 100 : 0, true);
                }
                // Fast run toggled.
                else if (item == fastRunCheckbox)
                {
                    PlayerFastRun = _checked;
                    SetRunSprintMultiplierForPlayer(Game.Player.Handle, (_checked ? 1.49f : 1f));
                }
                // Fast swim toggled.
                else if (item == fastSwimCheckbox)
                {
                    PlayerFastSwim = _checked;
                    SetSwimMultiplierForPlayer(Game.Player.Handle, (_checked ? 1.49f : 1f));
                }
                // Super jump toggled.
                else if (item == superJumpCheckbox)
                {
                    PlayerSuperJump = _checked;
                }
                // No ragdoll toggled.
                else if (item == noRagdollCheckbox)
                {
                    PlayerNoRagdoll = _checked;
                }
                // Never wanted toggled.
                else if (item == neverWantedCheckbox)
                {
                    PlayerNeverWanted = _checked;
                    if (!_checked)
                    {
                        SetMaxWantedLevel(5);
                    }
                    else
                    {
                        SetMaxWantedLevel(0);
                    }
                }
                // Everyone ignores player toggled.
                else if (item == everyoneIgnoresPlayerCheckbox)
                {
                    PlayerIsIgnored = _checked;

                    // Manage player is ignored by everyone.
                    SetEveryoneIgnorePlayer(Game.Player.Handle, PlayerIsIgnored);
                    SetPoliceIgnorePlayer(Game.Player.Handle, PlayerIsIgnored);
                    SetPlayerCanBeHassledByGangs(Game.Player.Handle, !PlayerIsIgnored);
                }
                else if (item == playerStayInVehicleCheckbox)
                {
                    PlayerStayInVehicle = _checked;
                }
                // Freeze player toggled.
                else if (item == playerFrozenCheckbox)
                {
                    PlayerFrozen = _checked;

                    if (!MainMenu.NoClipEnabled)
                    {
                        FreezeEntityPosition(Game.PlayerPed.Handle, PlayerFrozen);
                    }
                    else if (!MainMenu.NoClipEnabled)
                    {
                        FreezeEntityPosition(Game.PlayerPed.Handle, PlayerFrozen);
                    }
                }
            };

            // List selections
            menu.OnListItemSelect += (sender, listItem, listIndex, itemIndex) =>
            {
                // Set wanted Level
                if (listItem == setWantedLevel)
                {
                    SetPlayerWantedLevel(Game.Player.Handle, listIndex, false);
                    SetPlayerWantedLevelNow(Game.Player.Handle, false);
                }
                // Player Scenarios 
                else if (listItem == playerScenarios)
                {
                    PlayScenario(PedScenarios.ScenarioNames[PedScenarios.Scenarios[listIndex]]);
                }
                else if (listItem == setArmorItem)
                {
                    Game.PlayerPed.Armor = (listItem.ListIndex) * 20;
                }
            };

            // button presses
            menu.OnItemSelect += (sender, item, index) =>
            {
                // Force Stop Scenario button
                if (item == stopScenario)
                {
                    // Play a new scenario named "forcestop" (this scenario doesn't exist, but the "Play" function checks
                    // for the string "forcestop", if that's provided as th scenario name then it will forcefully clear the player task.
                    PlayScenario("forcestop");
                }
                else if (item == healPlayerBtn)
                {
                    Game.PlayerPed.Health = Game.PlayerPed.MaxHealth;
                    Notify.Success("已經治療.");
                }
                else if (item == cleanPlayerBtn)
                {
                    Game.PlayerPed.ClearBloodDamage();
                    Notify.Success("衣服已經洗乾淨了.");
                }
                else if (item == dryPlayerBtn)
                {
                    Game.PlayerPed.WetnessHeight = 0f;
                    Notify.Success("衣服已經烘乾了.");
                }
                else if (item == wetPlayerBtn)
                {
                    Game.PlayerPed.WetnessHeight = 2f;
                    Notify.Success("衣服現在濕了.");
                }
                else if (item == suicidePlayerBtn)
                {
                    CommitSuicide();
                }
            };
            #endregion

        }

        private int GetCustomDrivingStyle()
        {
            var items = CustomDrivingStyleMenu.GetMenuItems();
            var flags = new int[items.Count];
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item is MenuCheckboxItem checkbox)
                {
                    flags[i] = checkbox.Checked ? 1 : 0;
                }
            }
            string binaryString = "";
            var reverseFlags = flags.Reverse();
            foreach (int i in reverseFlags)
            {
                binaryString += i;
            }
            var binaryNumber = Convert.ToUInt32(binaryString, 2);
            return (int)binaryNumber;
        }

        private int GetStyleFromIndex(int index)
        {
            int style;
            switch (index)
            {
                case 0:
                    style = 443; // normal
                    break;
                case 1:
                    style = 575; // rushed
                    break;
                case 2:
                    style = 536871355; // Avoid highways
                    break;
                case 3:
                    style = 1467; // Go in reverse
                    break;
                case 4:
                    style = GetCustomDrivingStyle(); // custom driving style;
                    break;
                default:
                    style = 0; // no style (impossible, but oh well)
                    break;
            }
            return style;
        }

        /// <summary>
        /// Checks if the menu exists, if not then it creates it first.
        /// Then returns the menu.
        /// </summary>
        /// <returns>The Player Options Menu</returns>
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
