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
    public class PersonalVehicle
    {
        // Variables
        private Menu menu;
        public bool EnableVehicleBlip { get; private set; } = UserDefaults.PVEnableVehicleBlip;

        // Empty constructor
        public PersonalVehicle() { }

        public Vehicle CurrentPersonalVehicle { get; internal set; } = null;

        public Menu VehicleDoorsMenu { get; internal set; } = null;


        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            // Menu
            menu = new Menu(GetSafePlayerName(Game.Player.Name), "個人載具選項");

            // menu items
            MenuItem setVehice = new MenuItem("設定載具", "將您當前的載具設置為您的個人載具。 如果您已經有個人交通工具，那麼它將覆蓋您的選擇.") { Label = "目前載具: 無" };
            MenuItem toggleEngine = new MenuItem("開關引擎", "即使您不在載具內，也可以打開或關閉引擎。 如果其他人當前正在使用您的載具，這將不起作用.");
            MenuListItem toggleLights = new MenuListItem("開關載具燈", new List<string>() { "強制開啟", "強制關閉", "重置" }, 0, "這將啟用或禁用您的載具前燈(但是必須先發動載具才能使用此功能).");
            MenuItem kickAllPassengers = new MenuItem("踢出乘客", "把所有乘車踢出載具外.");
            //MenuItem
            MenuItem lockDoors = new MenuItem("鎖門", "鎖載具門，即使有鎖門，在載具裡面的人依然還是能夠離開車輛.");
            MenuItem unlockDoors = new MenuItem("解鎖門", "解鎖載具的門.");
            MenuItem doorsMenuBtn = new MenuItem("載具門", "編輯載具門")
            {
                Label = "→→→"
            };
            MenuItem soundHorn = new MenuItem("喇叭聲音", "發出載具的喇叭聲音");
            MenuItem toggleAlarm = new MenuItem("開關警報器", "開關載具警報聲音。 這不會設置警報。 它僅發出當前警報聲音的狀態.");
            MenuCheckboxItem enableBlip = new MenuCheckboxItem("個人載具光點", "開啟或關閉 個人載具標記光點.", EnableVehicleBlip) { Style = MenuCheckboxItem.CheckboxStyle.Cross };
            MenuCheckboxItem exclusiveDriver = new MenuCheckboxItem("私人載具", "如果開啟的話其他玩家不能開您的載具.", false) { Style = MenuCheckboxItem.CheckboxStyle.Cross };
            //submenu
            VehicleDoorsMenu = new Menu("載具門", "載具門管理");
            MenuController.AddSubmenu(menu, VehicleDoorsMenu);
            MenuController.BindMenuItem(menu, VehicleDoorsMenu, doorsMenuBtn);

            // This is always allowed if this submenu is created/allowed.
            menu.AddMenuItem(setVehice);

            // Add conditional features.

            // Toggle engine.
            if (IsAllowed(Permission.PVToggleEngine))
            {
                menu.AddMenuItem(toggleEngine);
            }

            // Toggle lights
            if (IsAllowed(Permission.PVToggleLights))
            {
                menu.AddMenuItem(toggleLights);
            }

            // Kick vehicle passengers
            if (IsAllowed(Permission.PVKickPassengers))
            {
                menu.AddMenuItem(kickAllPassengers);
            }

            // Lock and unlock vehicle doors
            if (IsAllowed(Permission.PVLockDoors))
            {
                menu.AddMenuItem(lockDoors);
                menu.AddMenuItem(unlockDoors);
            }

            if(IsAllowed(Permission.PVDoors))
            {
                menu.AddMenuItem(doorsMenuBtn);
            }

            // Sound horn
            if (IsAllowed(Permission.PVSoundHorn))
            {
                menu.AddMenuItem(soundHorn);
            }

            // Toggle alarm sound
            if (IsAllowed(Permission.PVToggleAlarm))
            {
                menu.AddMenuItem(toggleAlarm);
            }

            // Enable blip for personal vehicle
            if (IsAllowed(Permission.PVAddBlip))
            {
                menu.AddMenuItem(enableBlip);
            }

            if (IsAllowed(Permission.PVExclusiveDriver))
            {
                menu.AddMenuItem(exclusiveDriver);
            }


            // Handle list presses
            menu.OnListItemSelect += (sender, item, itemIndex, index) =>
            {
                var veh = CurrentPersonalVehicle;
                if (veh != null && veh.Exists())
                {
                    if (!NetworkHasControlOfEntity(CurrentPersonalVehicle.Handle))
                    {
                        if (!NetworkRequestControlOfEntity(CurrentPersonalVehicle.Handle))
                        {
                            Notify.Error("您目前無法控制這輛載具。可能有其他玩家控制您的載具，請重試.");
                            return;
                        }
                    }

                    if (item == toggleLights)
                    {
                        PressKeyFob(CurrentPersonalVehicle);
                        if (itemIndex == 0)
                        {
                            SetVehicleLights(CurrentPersonalVehicle.Handle, 3);
                        }
                        else if (itemIndex == 1)
                        {
                            SetVehicleLights(CurrentPersonalVehicle.Handle, 1);
                        }
                        else
                        {
                            SetVehicleLights(CurrentPersonalVehicle.Handle, 0);
                        }
                    }
                }
                else
                {
                    Notify.Error("您尚未選擇個人載具，或者您的載具已被刪除。 在使用這些選項之前，請先設置個人交通工具.");
                }
            };

            // Handle checkbox changes
            menu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == enableBlip)
                {
                    EnableVehicleBlip = _checked;
                    if (EnableVehicleBlip)
                    {
                        if (CurrentPersonalVehicle != null && CurrentPersonalVehicle.Exists())
                        {
                            if (CurrentPersonalVehicle.AttachedBlip == null || !CurrentPersonalVehicle.AttachedBlip.Exists())
                            {
                                CurrentPersonalVehicle.AttachBlip();
                            }
                            CurrentPersonalVehicle.AttachedBlip.Sprite = BlipSprite.PersonalVehicleCar;
                            CurrentPersonalVehicle.AttachedBlip.Name = "私人載具";
                        }
                        else
                        {
                            Notify.Error("您尚未選擇個人載具，或者您的載具已被刪除。 在使用這些選項之前，請先設置個人交通工具.");
                        }

                    }
                    else
                    {
                        if (CurrentPersonalVehicle != null && CurrentPersonalVehicle.Exists() && CurrentPersonalVehicle.AttachedBlip != null && CurrentPersonalVehicle.AttachedBlip.Exists())
                        {
                            CurrentPersonalVehicle.AttachedBlip.Delete();
                        }
                    }
                }
                else if (item == exclusiveDriver)
                {
                    if (CurrentPersonalVehicle != null && CurrentPersonalVehicle.Exists())
                    {
                        if (NetworkRequestControlOfEntity(CurrentPersonalVehicle.Handle))
                        {
                            if (_checked)
                            {
                                SetVehicleExclusiveDriver(CurrentPersonalVehicle.Handle, Game.PlayerPed.Handle);
                                SetVehicleExclusiveDriver_2(CurrentPersonalVehicle.Handle, Game.PlayerPed.Handle, 1);
                            }
                            else
                            {
                                SetVehicleExclusiveDriver(CurrentPersonalVehicle.Handle, 0);
                                SetVehicleExclusiveDriver_2(CurrentPersonalVehicle.Handle, 0, 1);
                            }
                        }
                        else
                        {
                            item.Checked = !_checked;
                            Notify.Error("您目前無法控制這輛載具。可能有其他玩家控制您的載具，請重試.");
                        }
                    }
                }
            };

            // Handle button presses.
            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == setVehice)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        var veh = GetVehicle();
                        if (veh != null && veh.Exists())
                        {
                            if (Game.PlayerPed == veh.Driver)
                            {
                                CurrentPersonalVehicle = veh;
                                veh.PreviouslyOwnedByPlayer = true;
                                veh.IsPersistent = true;
                                if (EnableVehicleBlip && IsAllowed(Permission.PVAddBlip))
                                {
                                    if (veh.AttachedBlip == null || !veh.AttachedBlip.Exists())
                                    {
                                        veh.AttachBlip();
                                    }
                                    veh.AttachedBlip.Sprite = BlipSprite.PersonalVehicleCar;
                                    veh.AttachedBlip.Name = "私人載具";
                                }
                                var name = GetLabelText(veh.DisplayName);
                                if (string.IsNullOrEmpty(name) || name.ToLower() == "null")
                                {
                                    name = veh.DisplayName;
                                }
                                item.Label = $"目前載具: {name}";
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
                    else
                    {
                        Notify.Error(CommonErrors.NoVehicle);
                    }
                }
                else if (CurrentPersonalVehicle != null && CurrentPersonalVehicle.Exists())
                {
                    if (item == kickAllPassengers)
                    {
                        if (CurrentPersonalVehicle.Occupants.Count() > 0 && CurrentPersonalVehicle.Occupants.Any(p => p != Game.PlayerPed))
                        {
                            var netId = VehToNet(CurrentPersonalVehicle.Handle);
                            TriggerServerEvent("vMenu:GetOutOfCar", netId, Game.Player.ServerId);
                        }
                        else
                        {
                            Notify.Info("您的載具目前沒有乘客無法使用該功能.");
                        }
                    }
                    else
                    {
                        if (!NetworkHasControlOfEntity(CurrentPersonalVehicle.Handle))
                        {
                            if (!NetworkRequestControlOfEntity(CurrentPersonalVehicle.Handle))
                            {
                                Notify.Error("您目前無法控制這輛載具。可能有其他玩家控制您的載具，請重試.");
                                return;
                            }
                        }

                        if (item == toggleEngine)
                        {
                            PressKeyFob(CurrentPersonalVehicle);
                            SetVehicleEngineOn(CurrentPersonalVehicle.Handle, !CurrentPersonalVehicle.IsEngineRunning, true, true);
                        }

                        else if (item == lockDoors || item == unlockDoors)
                        {
                            PressKeyFob(CurrentPersonalVehicle);
                            bool _lock = item == lockDoors;
                            LockOrUnlockDoors(CurrentPersonalVehicle, _lock);
                        }

                        else if (item == soundHorn)
                        {
                            PressKeyFob(CurrentPersonalVehicle);
                            SoundHorn(CurrentPersonalVehicle);
                        }

                        else if (item == toggleAlarm)
                        {
                            PressKeyFob(CurrentPersonalVehicle);
                            ToggleVehicleAlarm(CurrentPersonalVehicle);
                        }
                    }
                }
                else
                {
                    Notify.Error("您尚未選擇個人載具，或者您的載具已被刪除。 在使用這些選項之前，請先設置個人交通工具.");
                }
            };

            #region Doors submenu 
            MenuItem openAll = new MenuItem("打開所有載具門", "打開所有載具門.");
            MenuItem closeAll = new MenuItem("關閉所有載具門", "關閉所有載具門.");
            MenuItem LF = new MenuItem("左前門", "開啟/關閉 左前門.");
            MenuItem RF = new MenuItem("右前門", "開啟/關閉 右前門.");
            MenuItem LR = new MenuItem("左後門", "開啟/關閉 左後門.");
            MenuItem RR = new MenuItem("右後門", "開啟/關閉 右後門.");
            MenuItem HD = new MenuItem("引擎蓋", "開啟/關閉 引擎蓋.");
            MenuItem TR = new MenuItem("後車廂", "開啟/關閉 後車廂.");
            MenuItem E1 = new MenuItem("附加 1", "開啟/關閉 附加 (#1). 請注意:大多數的載具沒有此功能.");
            MenuItem E2 = new MenuItem("附加 2", "開啟/關閉 附加 (#2). 請注意:大多數的載具沒有此功能.");
            MenuItem BB = new MenuItem("炸彈艙", "開啟/關閉 炸彈艙。僅能在某些飛機上使用.");
            var doors = new List<string>() { "Front Left", "Front Right", "Rear Left", "Rear Right", "Hood", "Trunk", "Extra 1", "Extra 2", "Bomb Bay" };
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
                Vehicle veh = CurrentPersonalVehicle;
                if(veh != null && veh.Exists())
                {
                    if (!NetworkHasControlOfEntity(CurrentPersonalVehicle.Handle))
                    {
                        if (!NetworkRequestControlOfEntity(CurrentPersonalVehicle.Handle))
                        {
                            Notify.Error("您目前無法控制這輛載具。可能有其他玩家控制您的載具，請重試.");
                            return;
                        }
                    }

                    if(item == removeDoorList)
                    {
                        PressKeyFob(veh);
                        SetVehicleDoorBroken(veh.Handle, index, deleteDoors.Checked);
                    }
                }
            };

            VehicleDoorsMenu.OnItemSelect += (sender, item, index) =>
            {
                Vehicle veh = CurrentPersonalVehicle;
                if(veh != null && veh.Exists() && !veh.IsDead)
                {
                    if (!NetworkHasControlOfEntity(CurrentPersonalVehicle.Handle))
                    {
                        if (!NetworkRequestControlOfEntity(CurrentPersonalVehicle.Handle))
                        {
                            Notify.Error("您目前無法控制這輛載具。可能有其他玩家控制您的載具，請重試.");
                            return;
                        }
                    }

                    if (index < 8)
                    {
                        bool open = GetVehicleDoorAngleRatio(veh.Handle, index) > 0.1f;
                        PressKeyFob(veh);
                        if(open)
                        {
                            SetVehicleDoorShut(veh.Handle, index, false);
                        } else
                        {
                            SetVehicleDoorOpen(veh.Handle, index, false, false);
                        }
                    } else if(item == openAll)
                    {
                        PressKeyFob(veh);
                        for(var door = 0; door < 8; door++)
                        {
                            SetVehicleDoorOpen(veh.Handle, door, false, false);
                        }
                    } else if(item == closeAll)
                    {
                        PressKeyFob(veh);
                        for(var door = 0; door < 8; door++)
                        {
                            SetVehicleDoorShut(veh.Handle, door, false);
                        }
                    } else if(item == BB && veh.HasBombBay)
                    {
                        PressKeyFob(veh);
                        bool bombBayOpen = AreBombBayDoorsOpen(veh.Handle);
                        if(bombBayOpen)
                        {
                            veh.CloseBombBay();
                        } else
                        {
                            veh.OpenBombBay();
                        }
                    } else
                    {
                        Notify.Error("您尚未選擇個人載具，或者您的載具已被刪除。 在使用這些選項之前，請先設置個人交通工具.");
                    }
                }
            };
            #endregion
        }



        private async void SoundHorn(Vehicle veh)
        {
            if (veh != null && veh.Exists())
            {
                int timer = GetGameTimer();
                while (GetGameTimer() - timer < 1000)
                {
                    SoundVehicleHornThisFrame(veh.Handle);
                    await Delay(0);
                }
            }
        }

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
