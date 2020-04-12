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
    public class SavedVehicles
    {
        // Variables
        private Menu menu;
        private Menu selectedVehicleMenu = new Menu("管理載具", "管理此保存的載具.");
        private Menu unavailableVehiclesMenu = new Menu("載具遺失", "無法保存的載具");
        private Dictionary<string, VehicleInfo> savedVehicles = new Dictionary<string, VehicleInfo>();
        private List<Menu> subMenus = new List<Menu>();
        private Dictionary<MenuItem, KeyValuePair<string, VehicleInfo>> svMenuItems = new Dictionary<MenuItem, KeyValuePair<string, VehicleInfo>>();
        private KeyValuePair<string, VehicleInfo> currentlySelectedVehicle = new KeyValuePair<string, VehicleInfo>();
        private int deleteButtonPressedCount = 0;


        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            string menuTitle = "儲存載具";
            #region Create menus and submenus
            // Create the menu.
            menu = new Menu(menuTitle, "管理已保存的載具");

            MenuItem saveVehicle = new MenuItem("保存當前載具", "保存您當前坐入之載具.");
            menu.AddMenuItem(saveVehicle);
            saveVehicle.LeftIcon = MenuItem.Icon.CAR;

            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == saveVehicle)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        SaveVehicle();
                    }
                    else
                    {
                        Notify.Error("您目前不在任何車輛中。 請先輸入車輛再嘗試保存.");
                    }
                }
            };

            for (int i = 0; i < 22; i++)
            {
                Menu categoryMenu = new Menu("儲存車輛", GetLabelText($"VEH_CLASS_{i}"));

                MenuItem categoryButton = new MenuItem(GetLabelText($"VEH_CLASS_{i}"), $"從中保存的所有車輛 {(GetLabelText($"VEH_CLASS_{i}"))} 類型.");
                subMenus.Add(categoryMenu);
                MenuController.AddSubmenu(menu, categoryMenu);
                menu.AddMenuItem(categoryButton);
                categoryButton.Label = "→→→";
                MenuController.BindMenuItem(menu, categoryMenu, categoryButton);

                categoryMenu.OnMenuClose += (sender) =>
                {
                    UpdateMenuAvailableCategories();
                };

                categoryMenu.OnItemSelect += (sender, item, index) =>
                {
                    UpdateSelectedVehicleMenu(item, sender);
                };
            }

            MenuItem unavailableModels = new MenuItem("車輛遺失", "在遊戲文件中找不到該模組。可能原因這是一個附加載具，並且伺服器當前未對其進行處理.")
            {
                Label = "→→→"
            };

            menu.AddMenuItem(unavailableModels);
            MenuController.BindMenuItem(menu, unavailableVehiclesMenu, unavailableModels);
            MenuController.AddSubmenu(menu, unavailableVehiclesMenu);


            MenuController.AddMenu(selectedVehicleMenu);
		    MenuItem spawnVehicle = new MenuItem("生成車輛", "生成此已保存的車輛.");
	    	MenuItem renameVehicle = new MenuItem("重命名車輛", "重命名您保存的車輛.");
	    	MenuItem replaceVehicle = new MenuItem("~r~更換車輛", "您保存的車輛將被您當前所坐的車輛代替。~r~警告：這無法撤消！");
	    	MenuItem deleteVehicle = new MenuItem("~r~刪除車輛", "~r~這將刪除您保存的車輛。警告：這不能撤消！");
            selectedVehicleMenu.AddMenuItem(spawnVehicle);
            selectedVehicleMenu.AddMenuItem(renameVehicle);
            selectedVehicleMenu.AddMenuItem(replaceVehicle);
            selectedVehicleMenu.AddMenuItem(deleteVehicle);

            selectedVehicleMenu.OnMenuOpen += (sender) =>
            {
                spawnVehicle.Label = "(" + GetDisplayNameFromVehicleModel(currentlySelectedVehicle.Value.model).ToLower() + ")";
            };

            selectedVehicleMenu.OnMenuClose += (sender) =>
            {
                selectedVehicleMenu.RefreshIndex();
                deleteButtonPressedCount = 0;
                deleteVehicle.Label = "";
            };

            selectedVehicleMenu.OnItemSelect += async (sender, item, index) =>
            {
                if (item == spawnVehicle)
                {
                    if (MainMenu.VehicleSpawnerMenu != null)
                    {
                        SpawnVehicle(currentlySelectedVehicle.Value.model, MainMenu.VehicleSpawnerMenu.SpawnInVehicle, MainMenu.VehicleSpawnerMenu.ReplaceVehicle, false, vehicleInfo: currentlySelectedVehicle.Value, saveName: currentlySelectedVehicle.Key.Substring(4));
                    }
                    else
                    {
                        SpawnVehicle(currentlySelectedVehicle.Value.model, true, true, false, vehicleInfo: currentlySelectedVehicle.Value, saveName: currentlySelectedVehicle.Key.Substring(4));
                    }
                }
                else if (item == renameVehicle)
                {
                    string newName = await GetUserInput(windowTitle: "輸入此車輛的新名稱.", maxInputLength: 30);
                    if (string.IsNullOrEmpty(newName))
                    {
                        Notify.Error(CommonErrors.InvalidInput);
                    }
                    else
                    {
                        if (StorageManager.SaveVehicleInfo("veh_" + newName, currentlySelectedVehicle.Value, false))
                        {
                            DeleteResourceKvp(currentlySelectedVehicle.Key);
                            while (!selectedVehicleMenu.Visible)
                            {
                                await BaseScript.Delay(0);
                            }
                            Notify.Success("您的車輛已成功重命名.");
                            UpdateMenuAvailableCategories();
                            selectedVehicleMenu.GoBack();
                            currentlySelectedVehicle = new KeyValuePair<string, VehicleInfo>(); // clear the old info
                        }
                        else
                        {
                            Notify.Error("此名稱已被使用或未知失敗。 如果您認為有問題，請與服務器所有者聯繫。");
                        }
                    }
                }
                else if (item == replaceVehicle)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        SaveVehicle(currentlySelectedVehicle.Key.Substring(4));
                        selectedVehicleMenu.GoBack();
                        Notify.Success("您保存的車輛已替換為當前車輛.");
                    }
                    else
                    {
                        Notify.Error("您需要先上車才能更換舊車.");
                    }
                }
                else if (item == deleteVehicle)
                {
                    if (deleteButtonPressedCount == 0)
                    {
                        deleteButtonPressedCount = 1;
                        item.Label = "再按一次確認.";
                        Notify.Alert("您確定要刪除這輛車嗎？ 再按一次按鈕確認.");
                    }
                    else
                    {
                        deleteButtonPressedCount = 0;
                        item.Label = "";
                        DeleteResourceKvp(currentlySelectedVehicle.Key);
                        UpdateMenuAvailableCategories();
                        selectedVehicleMenu.GoBack();
                        Notify.Success("您保存的車輛已被刪除.");
                    }
                }
                if (item != deleteVehicle) // if any other button is pressed, restore the delete vehicle button pressed count.
                {
                    deleteButtonPressedCount = 0;
                    deleteVehicle.Label = "";
                }
            };
            unavailableVehiclesMenu.InstructionalButtons.Add(Control.FrontendDelete, "刪除載具!");

            unavailableVehiclesMenu.ButtonPressHandlers.Add(new Menu.ButtonPressHandler(Control.FrontendDelete, Menu.ControlPressCheckType.JUST_RELEASED, new Action<Menu, Control>((m, c) =>
            {
                if (m.Size > 0)
                {
                    int index = m.CurrentIndex;
                    if (index < m.Size)
                    {
                        MenuItem item = m.GetMenuItems().Find(i => i.Index == index);
                        if (item != null && (item.ItemData is KeyValuePair<string, VehicleInfo> sd))
                        {
                            if (item.Label == "~r~您確定嗎?")
                            {
                                Log("Unavailable saved vehicle deleted, data: " + JsonConvert.SerializeObject(sd));
                                DeleteResourceKvp(sd.Key);
                                unavailableVehiclesMenu.GoBack();
                                UpdateMenuAvailableCategories();
                            }
                            else
                            {
                                item.Label = "~r~您確定嗎?";
                            }
                        }
                        else
                        {
                            Notify.Error("不知何故找不到這輛車.");
                        }
                    }
                    else
                    {
                        Notify.Error("您以某種方式設法觸發了一個不存在的菜單項的刪除...?");
                    }
                }
                else
                {
                    Notify.Error("當前沒有可刪除的車輛!");
                }
            }), true));

            void ResetAreYouSure()
            {
                foreach (var i in unavailableVehiclesMenu.GetMenuItems())
                {
                    if (i.ItemData is KeyValuePair<string, VehicleInfo> vd)
                    {
                        i.Label = $"({vd.Value.name})";
                    }
                }
            }
            unavailableVehiclesMenu.OnMenuClose += (sender) => ResetAreYouSure();
            unavailableVehiclesMenu.OnIndexChange += (sender, oldItem, newItem, oldIndex, newIndex) => ResetAreYouSure();

            #endregion
        }


        /// <summary>
        /// Updates the selected vehicle.
        /// </summary>
        /// <param name="selectedItem"></param>
        /// <returns>A bool, true if successfull, false if unsuccessfull</returns>
        private bool UpdateSelectedVehicleMenu(MenuItem selectedItem, Menu parentMenu = null)
        {
            if (!svMenuItems.ContainsKey(selectedItem))
            {
                Notify.Error("以某種非常奇怪的方式，您設法選擇了一個按鈕，該按鈕根據此列表不存在。 因此您的車輛無法裝載. :( 也許您的保存文件已損壞?");
                return false;
            }
            var vehInfo = svMenuItems[selectedItem];
            selectedVehicleMenu.MenuSubtitle = $"{vehInfo.Key.Substring(4)} ({vehInfo.Value.name})";
            currentlySelectedVehicle = vehInfo;
            MenuController.CloseAllMenus();
            selectedVehicleMenu.OpenMenu();
            if (parentMenu != null)
            {
                MenuController.AddSubmenu(parentMenu, selectedVehicleMenu);
            }
            return true;
        }


        /// <summary>
        /// Updates the available vehicle category list.
        /// </summary>
        public void UpdateMenuAvailableCategories()
        {
            savedVehicles = GetSavedVehicles();
            svMenuItems = new Dictionary<MenuItem, KeyValuePair<string, VehicleInfo>>();

            for (int i = 1; i < GetMenu().Size - 1; i++)
            {
                if (savedVehicles.Any(a => GetVehicleClassFromName(a.Value.model) == i - 1 && IsModelInCdimage(a.Value.model)))
                {
                    GetMenu().GetMenuItems()[i].RightIcon = MenuItem.Icon.NONE;
                    GetMenu().GetMenuItems()[i].Label = "→→→";
                    GetMenu().GetMenuItems()[i].Enabled = true;
                    GetMenu().GetMenuItems()[i].Description = $"從中保存的所有車輛 {GetMenu().GetMenuItems()[i].Text} 類別.";
                }
                else
                {
                    GetMenu().GetMenuItems()[i].Label = "";
                    GetMenu().GetMenuItems()[i].RightIcon = MenuItem.Icon.LOCK;
                    GetMenu().GetMenuItems()[i].Enabled = false;
                    GetMenu().GetMenuItems()[i].Description = $"您沒有任何屬於的已保存車輛 {GetMenu().GetMenuItems()[i].Text} 類別.";
                }
            }

            // Check if the items count will be changed. If there are less cars than there were before, one probably got deleted
            // so in that case we need to refresh the index of that menu just to be safe. If not, keep the index where it is for improved
            // usability of the menu.
            foreach (Menu m in subMenus)
            {
                int size = m.Size;
                int vclass = subMenus.IndexOf(m);

                int count = savedVehicles.Count(a => GetVehicleClassFromName(a.Value.model) == vclass);
                if (count < size)
                {
                    m.RefreshIndex();
                }
            }

            foreach (Menu m in subMenus)
            {
                // Clear items but don't reset the index because we can guarantee that the index won't be out of bounds.
                // this is the case because of the loop above where we reset the index if the items count changes.
                m.ClearMenuItems(true);
            }

            // Always clear this index because it's useless anyway and it's safer.
            unavailableVehiclesMenu.ClearMenuItems();

            foreach (var sv in savedVehicles)
            {
                if (IsModelInCdimage(sv.Value.model))
                {
                    int vclass = GetVehicleClassFromName(sv.Value.model);
                    Menu menu = subMenus[vclass];

                    MenuItem savedVehicleBtn = new MenuItem(sv.Key.Substring(4), $"管理此保存的車輛.")
                    {
                        Label = $"({sv.Value.name}) →→→"
                    };
                    menu.AddMenuItem(savedVehicleBtn);

                    svMenuItems.Add(savedVehicleBtn, sv);
                }
                else
                {
                    MenuItem missingVehItem = new MenuItem(sv.Key.Substring(4), "在遊戲文件中找不到該模組。可能原因這是一個附加載具，並且伺服器當前未對其進行處理.")
                    {
                        Label = "(" + sv.Value.name + ")",
                        Enabled = false,
                        LeftIcon = MenuItem.Icon.LOCK,
                        ItemData = sv
                    };
                    //SetResourceKvp(sv.Key + "_tmp_dupe", JsonConvert.SerializeObject(sv.Value));
                    unavailableVehiclesMenu.AddMenuItem(missingVehItem);
                }
            }
            foreach (Menu m in subMenus)
            {
                m.SortMenuItems((MenuItem A, MenuItem B) =>
                {
                    return A.Text.ToLower().CompareTo(B.Text.ToLower());
                });
            }
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
