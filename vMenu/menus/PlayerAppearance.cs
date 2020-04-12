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
    public class PlayerAppearance
    {
        private Menu menu;

        private Menu pedCustomizationMenu;
        private Menu savedPedsMenu;
        private Menu spawnPedsMenu;
        private Menu addonPedsMenu;
        private Menu mainPedsMenu = new Menu("主要人物", "產生外觀d");
        private Menu animalsPedsMenu = new Menu("動物", "產生外觀");
        private Menu malePedsMenu = new Menu("男性", "產生外觀");
        private Menu femalePedsMenu = new Menu("女性", "產生外觀");
        private Menu otherPedsMenu = new Menu("其他", "產生外觀");

        public static Dictionary<string, uint> AddonPeds;

        public static int ClothingAnimationType { get; set; } = UserDefaults.PAClothingAnimationType;

        private Dictionary<MenuListItem, int> drawablesMenuListItems = new Dictionary<MenuListItem, int>();
        private Dictionary<MenuListItem, int> propsMenuListItems = new Dictionary<MenuListItem, int>();

        #region create the menu
        /// <summary>
        /// Creates the menu(s).
        /// </summary>
        private void CreateMenu()
        {
            // Create the menu.
            menu = new Menu(Game.Player.Name, "玩家外觀");
            savedPedsMenu = new Menu(Game.Player.Name, "保存外觀");
            pedCustomizationMenu = new Menu(Game.Player.Name, "自定義保存外觀");
            spawnPedsMenu = new Menu(Game.Player.Name, "產生外觀");
            addonPedsMenu = new Menu(Game.Player.Name, "添加外觀");


            // Add the (submenus) to the menu pool.
            MenuController.AddSubmenu(menu, pedCustomizationMenu);
            MenuController.AddSubmenu(menu, savedPedsMenu);
            MenuController.AddSubmenu(menu, spawnPedsMenu);
            MenuController.AddSubmenu(spawnPedsMenu, addonPedsMenu);
            MenuController.AddSubmenu(spawnPedsMenu, mainPedsMenu);
            MenuController.AddSubmenu(spawnPedsMenu, animalsPedsMenu);
            MenuController.AddSubmenu(spawnPedsMenu, malePedsMenu);
            MenuController.AddSubmenu(spawnPedsMenu, femalePedsMenu);
            MenuController.AddSubmenu(spawnPedsMenu, otherPedsMenu);

            // Create the menu items.
            MenuItem pedCustomization = new MenuItem("自訂模型外觀", "修改模型外觀的外觀.") { Label = "→→→" };
            MenuItem saveCurrentPed = new MenuItem("儲存目前模型外觀", "保存當前的模型。請注意，對於男/女模型，這不會保存大多數自定義設置.");
            MenuItem savedPedsBtn = new MenuItem("儲存模型外觀", "編輯，重命名，複製，產生或刪除已保存的模型.") { Label = "→→→" };
            MenuItem spawnPedsBtn = new MenuItem("產生模型外觀", "通過從列表中選擇一個或從列表中選擇一個插件來更改模型.") { Label = "→→→" };


            MenuItem spawnByNameBtn = new MenuItem("產出模型名稱", "手動輸入模型外觀即可生成.");
            MenuItem addonPedsBtn = new MenuItem("添加模型外觀", "從插件模型列表中生成外觀.") { Label = "→→→" };
            MenuItem mainPedsBtn = new MenuItem("主要模型外觀", "從主要玩家列表中選擇一個新模型.") { Label = "→→→" };
            MenuItem animalPedsBtn = new MenuItem("動物模型外觀", "成為動物. ~r~請注意，如果您死為動物，這可能會使您自己的遊戲或其他玩家的遊戲崩潰，Godmode無法阻止這種情況.") { Label = "→→→" };
            MenuItem malePedsBtn = new MenuItem("男性模型外觀", "選擇男性模型外觀.") { Label = "→→→" };
            MenuItem femalePedsBtn = new MenuItem("女性模型外觀", "選擇女性模型外觀.") { Label = "→→→" };
            MenuItem otherPedsBtn = new MenuItem("其他模型外觀", "選擇其他模型外觀.") { Label = "→→→" };

            List<string> walkstyles = new List<string>() { "Normal", "Injured", "Tough Guy", "Femme", "Gangster", "Posh", "Sexy", "Business", "Drunk", "Hipster" };
            MenuListItem walkingStyle = new MenuListItem("走路風格", walkstyles, 0, "更改當前模型的步行方式 每次更改模型或加載已保存的模型外觀時，都需要重新應用此功能");

            List<string> clothingGlowAnimations = new List<string>() { "On", "Off", "Fade", "Flash" };
            MenuListItem clothingGlowType = new MenuListItem("發光的服裝風格", clothingGlowAnimations, ClothingAnimationType, "設置在玩家的發光衣服上使用的樣式.");

            // Add items to the menu.
            menu.AddMenuItem(pedCustomization);
            menu.AddMenuItem(saveCurrentPed);
            menu.AddMenuItem(savedPedsBtn);
            menu.AddMenuItem(spawnPedsBtn);

            menu.AddMenuItem(walkingStyle);
            menu.AddMenuItem(clothingGlowType);

            if (IsAllowed(Permission.PACustomize))
            {
                MenuController.BindMenuItem(menu, pedCustomizationMenu, pedCustomization);
            }
            else
            {
                menu.RemoveMenuItem(pedCustomization);
            }

            // always allowed
            MenuController.BindMenuItem(menu, savedPedsMenu, savedPedsBtn);
            MenuController.BindMenuItem(menu, spawnPedsMenu, spawnPedsBtn);

            Menu selectedSavedPedMenu = new Menu("儲存模型外觀", "重新命名");
            MenuController.AddSubmenu(savedPedsMenu, selectedSavedPedMenu);
            MenuItem spawnSavedPed = new MenuItem("產生已儲存的模型外觀", "產生已儲存的模型外觀.");
            MenuItem cloneSavedPed = new MenuItem("複製已儲存的模型外觀", "複製已儲存的模型外觀.");
            MenuItem renameSavedPed = new MenuItem("重新命名已儲存的模型外觀", "重新命名已儲存的模型外觀.") { LeftIcon = MenuItem.Icon.WARNING };
            MenuItem replaceSavedPed = new MenuItem("~r~取代已儲存的模型外觀", "取代原模型外觀 請注意，此操作無法撤消！") { LeftIcon = MenuItem.Icon.WARNING };
            MenuItem deleteSavedPed = new MenuItem("~r~刪除已儲存的模型外觀", "刪除此模型外觀 請注意，此操作無法撤消！") { LeftIcon = MenuItem.Icon.WARNING };

            if (!IsAllowed(Permission.PASpawnSaved))
            {
                spawnSavedPed.Enabled = false;
                spawnSavedPed.RightIcon = MenuItem.Icon.LOCK;
                spawnSavedPed.Description = "您沒有權限產生已儲存的模型外觀.";
            }

            selectedSavedPedMenu.AddMenuItem(spawnSavedPed);
            selectedSavedPedMenu.AddMenuItem(cloneSavedPed);
            selectedSavedPedMenu.AddMenuItem(renameSavedPed);
            selectedSavedPedMenu.AddMenuItem(replaceSavedPed);
            selectedSavedPedMenu.AddMenuItem(deleteSavedPed);

            KeyValuePair<string, PedInfo> savedPed = new KeyValuePair<string, PedInfo>();

            selectedSavedPedMenu.OnItemSelect += async (sender, item, index) =>
            {
                if (item == spawnSavedPed)
                {
                    await SetPlayerSkin(savedPed.Value.model, savedPed.Value, true);
                }
                else if (item == cloneSavedPed)
                {
                    string name = await GetUserInput($"輸入一個要複製的名字 ({savedPed.Key.Substring(4)})", savedPed.Key.Substring(4), 30);
                    if (string.IsNullOrEmpty(name))
                    {
                        Notify.Error(CommonErrors.InvalidSaveName);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(GetResourceKvpString($"ped_{name}")))
                        {
                            Notify.Error(CommonErrors.SaveNameAlreadyExists);
                        }
                        else
                        {
                            if (StorageManager.SavePedInfo("ped_" + name, savedPed.Value, false))
                            {
                                Notify.Success($"保存模型外觀已成功複製。 複製名稱: ~g~<{name}~s~.");
                            }
                            else
                            {
                                Notify.Error(CommonErrors.UnknownError, placeholderValue: " 無法保存您複製的模型外觀。 別擔心，您的原始模型外觀沒有受到損壞.");
                            }
                        }
                    }
                }
                else if (item == renameSavedPed)
                {
                    string name = await GetUserInput($"輸入一個新名字: {savedPed.Key.Substring(4)}", savedPed.Key.Substring(4), 30);
                    if (string.IsNullOrEmpty(name))
                    {
                        Notify.Error(CommonErrors.InvalidSaveName);
                    }
                    else
                    {
                        if ("ped_" + name == savedPed.Key)
                        {
                            Notify.Error("您需要選擇其他名稱，不能使用與現有模型外觀相同的名稱.");
                            return;
                        }
                        if (StorageManager.SavePedInfo("ped_" + name, savedPed.Value, false))
                        {
                            Notify.Success($"保存模型外觀成功 新的模型外觀名稱: ~g~<{name}~s~.");
                            DeleteResourceKvp(savedPed.Key);
                            selectedSavedPedMenu.MenuSubtitle = name;
                            savedPed = new KeyValuePair<string, PedInfo>("ped_" + name, savedPed.Value);
                        }
                        else
                        {
                            Notify.Error(CommonErrors.SaveNameAlreadyExists);
                        }
                    }
                }
                else if (item == replaceSavedPed)
                {
                    if (item.Label == "您確定嗎?")
                    {
                        item.Label = "";
                        bool success = await SavePed(savedPed.Key.Substring(4), overrideExistingPed: true);
                        if (!success)
                        {
                            Notify.Error(CommonErrors.UnknownError, placeholderValue: " 無法保存您覆蓋的模型外觀。 別擔心，您的原始模型外觀沒有受到損壞.");
                        }
                        else
                        {
                            Notify.Success("您已經成功覆蓋魔形外觀.");
                            savedPed = new KeyValuePair<string, PedInfo>(savedPed.Key, StorageManager.GetSavedPedInfo(savedPed.Key));
                        }
                    }
                    else
                    {
                        item.Label = "您確定嗎?";
                    }
                }
                else if (item == deleteSavedPed)
                {
                    if (item.Label == "您確定嗎?")
                    {
                        DeleteResourceKvp(savedPed.Key);
                        Notify.Success("您已經成功刪除模型外觀");
                        selectedSavedPedMenu.GoBack();
                    }
                    else
                    {
                        item.Label = "您確定嗎?";
                    }
                }
            };

            void ResetSavedPedsMenu(bool refreshIndex)
            {
                foreach (var item in selectedSavedPedMenu.GetMenuItems())
                {
                    item.Label = "";
                }
                if (refreshIndex)
                {
                    selectedSavedPedMenu.RefreshIndex();
                }
            }

            selectedSavedPedMenu.OnIndexChange += (menu, newItem, oldItem, oldIndex, newIndex) => ResetSavedPedsMenu(false);
            selectedSavedPedMenu.OnMenuOpen += (menu) => ResetSavedPedsMenu(true);


            void UpdateSavedPedsMenu()
            {
                int size = savedPedsMenu.Size;

                Dictionary<string, PedInfo> savedPeds = StorageManager.GetSavedPeds();

                foreach (var ped in savedPeds)
                {
                    if (size < 1 || !savedPedsMenu.GetMenuItems().Any(e => ped.Key == e.ItemData.Key))
                    {
                        MenuItem btn = new MenuItem(ped.Key.Substring(4), "點擊管理此已保存的模型外觀.") { Label = "→→→", ItemData = ped };
                        savedPedsMenu.AddMenuItem(btn);
                        MenuController.BindMenuItem(savedPedsMenu, selectedSavedPedMenu, btn);
                    }
                }

                if (savedPedsMenu.Size > 0)
                {
                    foreach (var d in savedPedsMenu.GetMenuItems())
                    {
                        if (!savedPeds.ContainsKey(d.ItemData.Key))
                        {
                            savedPedsMenu.RemoveMenuItem(d);
                        }
                        else
                        {
                            // Make sure the saved ped data is actually correct and up to date for this item.
                            var p = savedPeds.First(e => e.Key == d.ItemData.Key);
                            if (!string.IsNullOrEmpty(p.Key))
                            {
                                d.ItemData = p;
                            }
                        }
                    }
                }

                if (savedPedsMenu.Size > 0)
                {
                    savedPedsMenu.SortMenuItems((a, b) => a.Text.ToLower().CompareTo(b.Text.ToLower()));
                }

                // refresh index only if the size of the menu has changed.
                if (size != savedPedsMenu.Size)
                {
                    savedPedsMenu.RefreshIndex();
                }
            }

            savedPedsMenu.OnMenuOpen += (_) =>
            {
                UpdateSavedPedsMenu();
            };

            savedPedsMenu.OnItemSelect += (_, item, __) =>
            {
                savedPed = item.ItemData;
                selectedSavedPedMenu.MenuSubtitle = item.Text;
            };

            if (AddonPeds != null && AddonPeds.Count > 0 && IsAllowed(Permission.PAAddonPeds))
            {
                spawnPedsMenu.AddMenuItem(addonPedsBtn);
                MenuController.BindMenuItem(spawnPedsMenu, addonPedsMenu, addonPedsBtn);

                var addons = AddonPeds.ToList();

                addons.Sort((a, b) => a.Key.ToLower().CompareTo(b.Key.ToLower()));

                foreach (var ped in addons)
                {
                    string name = GetLabelText(ped.Key);
                    if (string.IsNullOrEmpty(name) || name == "NULL")
                    {
                        name = ped.Key;
                    }

                    MenuItem pedBtn = new MenuItem(ped.Key, "點擊來召喚這個模型外觀.") { Label = $"({name})" };

                    if (!IsModelInCdimage(ped.Value) || !IsModelAPed(ped.Value))
                    {
                        pedBtn.Enabled = false;
                        pedBtn.LeftIcon = MenuItem.Icon.LOCK;
                        pedBtn.Description = "這個模型外觀不是 (correctly) 資料. 如果您是服務器所有者，請確保ped名稱和型號有效!";
                    }

                    addonPedsMenu.AddMenuItem(pedBtn);
                }

                addonPedsMenu.OnItemSelect += async (sender, item, index) =>
                {
                    await SetPlayerSkin((uint)GetHashKey(item.Text), new PedInfo() { version = -1 }, true);
                };
            }

            if (IsAllowed(Permission.PASpawnNew))
            {
                spawnPedsMenu.AddMenuItem(spawnByNameBtn);
                spawnPedsMenu.AddMenuItem(mainPedsBtn);
                spawnPedsMenu.AddMenuItem(animalPedsBtn);
                spawnPedsMenu.AddMenuItem(malePedsBtn);
                spawnPedsMenu.AddMenuItem(femalePedsBtn);
                spawnPedsMenu.AddMenuItem(otherPedsBtn);

                MenuController.BindMenuItem(spawnPedsMenu, mainPedsMenu, mainPedsBtn);
                if (vMenuShared.ConfigManager.GetSettingsBool(vMenuShared.ConfigManager.Setting.vmenu_enable_animals_spawn_menu))
                {
                    MenuController.BindMenuItem(spawnPedsMenu, animalsPedsMenu, animalPedsBtn);
                }
                else
                {
                    animalPedsBtn.Enabled = false;
                    animalPedsBtn.Description = "伺服器服主禁用了此功能，這可能是有充分原因的，因為動物經常會導致遊戲崩潰.";
                    animalPedsBtn.LeftIcon = MenuItem.Icon.LOCK;
                }

                MenuController.BindMenuItem(spawnPedsMenu, malePedsMenu, malePedsBtn);
                MenuController.BindMenuItem(spawnPedsMenu, femalePedsMenu, femalePedsBtn);
                MenuController.BindMenuItem(spawnPedsMenu, otherPedsMenu, otherPedsBtn);

                foreach (var animal in animalModels)
                {
                    MenuItem animalBtn = new MenuItem(animal.Key, "點擊來召喚這個動物模型外觀.") { Label = $"({animal.Value})" };
                    animalsPedsMenu.AddMenuItem(animalBtn);
                }

                foreach (var ped in mainModels)
                {
                    MenuItem pedBtn = new MenuItem(ped.Key, "點擊來召喚這個模型外觀.") { Label = $"({ped.Value})" };
                    mainPedsMenu.AddMenuItem(pedBtn);
                }

                foreach (var ped in maleModels)
                {
                    MenuItem pedBtn = new MenuItem(ped.Key, "點擊來召喚這個模型外觀.") { Label = $"({ped.Value})" };
                    malePedsMenu.AddMenuItem(pedBtn);
                }

                foreach (var ped in femaleModels)
                {
                    MenuItem pedBtn = new MenuItem(ped.Key, "點擊來召喚這個模型外觀.") { Label = $"({ped.Value})" };
                    femalePedsMenu.AddMenuItem(pedBtn);
                }

                foreach (var ped in otherPeds)
                {
                    MenuItem pedBtn = new MenuItem(ped.Key, "點擊來召喚這個模型外觀.") { Label = $"({ped.Value})" };
                    otherPedsMenu.AddMenuItem(pedBtn);
                }

                async void FilterMenu(Menu m, Control c)
                {
                    string input = await GetUserInput("按造模型外觀型號名稱過濾，將該字段留空以重置過濾器");
                    if (!string.IsNullOrEmpty(input))
                    {
                        m.FilterMenuItems((mb) => mb.Label.ToLower().Contains(input.ToLower()) || mb.Text.ToLower().Contains(input.ToLower()));
                        Subtitle.Custom("過濾已經套用");
                    }
                    else
                    {
                        m.ResetFilter();
                        Subtitle.Custom("過濾已經清除.");
                    }
                }

                void ResetMenuFilter(Menu m)
                {
                    m.ResetFilter();
                }

                otherPedsMenu.OnMenuClose += ResetMenuFilter;
                malePedsMenu.OnMenuClose += ResetMenuFilter;
                femalePedsMenu.OnMenuClose += ResetMenuFilter;

                otherPedsMenu.InstructionalButtons.Add(Control.Jump, "Filter List");
                otherPedsMenu.ButtonPressHandlers.Add(new Menu.ButtonPressHandler(Control.Jump, Menu.ControlPressCheckType.JUST_RELEASED, new Action<Menu, Control>(FilterMenu), true));

                malePedsMenu.InstructionalButtons.Add(Control.Jump, "Filter List");
                malePedsMenu.ButtonPressHandlers.Add(new Menu.ButtonPressHandler(Control.Jump, Menu.ControlPressCheckType.JUST_RELEASED, new Action<Menu, Control>(FilterMenu), true));

                femalePedsMenu.InstructionalButtons.Add(Control.Jump, "Filter List");
                femalePedsMenu.ButtonPressHandlers.Add(new Menu.ButtonPressHandler(Control.Jump, Menu.ControlPressCheckType.JUST_RELEASED, new Action<Menu, Control>(FilterMenu), true));


                async void SpawnPed(Menu m, MenuItem item, int index)
                {

                    uint model = (uint)GetHashKey(item.Text);
                    if (m == animalsPedsMenu && !Game.PlayerPed.IsInWater)
                    {
                        switch (item.Text)
                        {
                            case "a_c_dolphin":
                            case "a_c_fish":
                            case "a_c_humpback":
                            case "a_c_killerwhale":
                            case "a_c_sharkhammer":
                            case "a_c_sharktiger":
                                Notify.Error("該動物只能在水中時召喚，否則會立即死亡.");
                                return;
                            default: break;
                        }
                    }

                    if (IsModelInCdimage(model))
                    {
                        // for animals we need to remove all weapons, this is because animals have their own weapons which you can't normally get and/or select in the weapon wheel.
                        // so we clear the weapons to force that specific weapon to be equipped.
                        if (m == animalsPedsMenu)
                        {
                            Game.PlayerPed.Weapons.RemoveAll();
                            await SetPlayerSkin(model, new PedInfo() { version = -1 }, false);
                            await Delay(1000);
                            SetPedComponentVariation(Game.PlayerPed.Handle, 0, 0, 0, 0);
                            await Delay(1000);
                            SetPedComponentVariation(Game.PlayerPed.Handle, 0, 0, 1, 0);
                            await Delay(1000);
                            SetPedDefaultComponentVariation(Game.PlayerPed.Handle);
                        }
                        else
                        {
                            await SetPlayerSkin(model, new PedInfo() { version = -1 }, true);
                        }
                    }
                    else
                    {
                        Notify.Error(CommonErrors.InvalidModel);
                    }
                }

                mainPedsMenu.OnItemSelect += SpawnPed;
                malePedsMenu.OnItemSelect += SpawnPed;
                femalePedsMenu.OnItemSelect += SpawnPed;
                animalsPedsMenu.OnItemSelect += SpawnPed;
                otherPedsMenu.OnItemSelect += SpawnPed;

                spawnPedsMenu.OnItemSelect += async (sender, item, index) =>
                {
                    if (item == spawnByNameBtn)
                    {
                        string model = await GetUserInput("模型外觀名稱", 30);
                        if (!string.IsNullOrEmpty(model))
                        {
                            await SetPlayerSkin(model, new PedInfo() { version = -1 }, true);
                        }
                        else
                        {
                            Notify.Error(CommonErrors.InvalidInput);
                        }
                    }
                };
            }


            // Handle list selections.
            menu.OnListItemSelect += (sender, item, listIndex, itemIndex) =>
            {
                if (item == walkingStyle)
                {
                    //if (MainMenu.DebugMode) Subtitle.Custom("Ped is: " + IsPedMale(Game.PlayerPed.Handle));
                    SetWalkingStyle(walkstyles[listIndex].ToString());
                }
                if (item == clothingGlowType)
                {
                    ClothingAnimationType = item.ListIndex;
                }
            };

            // Handle button presses.
            menu.OnItemSelect += async (sender, item, index) =>
            {
                if (item == pedCustomization)
                {
                    RefreshCustomizationMenu();
                }
                else if (item == saveCurrentPed)
                {
                    if (await SavePed())
                    {
                        Notify.Success("成功儲存新的模型外觀.");
                    }
                    else
                    {
                        Notify.Error("無法保存新的模型外觀，可能已經存在?");
                    }
                }
            };


            #region ped drawable list changes
            // Manage list changes.
            pedCustomizationMenu.OnListIndexChange += (sender, item, oldListIndex, newListIndex, itemIndex) =>
            {
                if (drawablesMenuListItems.ContainsKey(item))
                {
                    int drawableID = drawablesMenuListItems[item];
                    SetPedComponentVariation(Game.PlayerPed.Handle, drawableID, newListIndex, 0, 0);
                }
                else if (propsMenuListItems.ContainsKey(item))
                {
                    int propID = propsMenuListItems[item];
                    if (newListIndex == 0)
                    {
                        SetPedPropIndex(Game.PlayerPed.Handle, propID, newListIndex - 1, 0, false);
                        ClearPedProp(Game.PlayerPed.Handle, propID);
                    }
                    else
                    {
                        SetPedPropIndex(Game.PlayerPed.Handle, propID, newListIndex - 1, 0, true);
                    }
                    if (propID == 0)
                    {
                        int component = GetPedPropIndex(Game.PlayerPed.Handle, 0);      // helmet index
                        int texture = GetPedPropTextureIndex(Game.PlayerPed.Handle, 0); // texture
                        int compHash = GetHashNameForProp(Game.PlayerPed.Handle, 0, component, texture); // prop combination hash
                        if (N_0xd40aac51e8e4c663((uint)compHash) > 0) // helmet has visor. 
                        {
                            if (!IsHelpMessageBeingDisplayed())
                            {
                                BeginTextCommandDisplayHelp("TWOSTRINGS");
                                AddTextComponentSubstringPlayerName("按住 ~INPUT_SWITCH_VISOR~ 打開或關閉頭盔遮陽板");
                                AddTextComponentSubstringPlayerName("步行或騎摩托車且vMenu關閉時.");
                                EndTextCommandDisplayHelp(0, false, true, 6000);
                            }
                        }
                    }

                }
            };

            // Manage list selections.
            pedCustomizationMenu.OnListItemSelect += (sender, item, listIndex, itemIndex) =>
            {
                if (drawablesMenuListItems.ContainsKey(item)) // drawable
                {
                    int currentDrawableID = drawablesMenuListItems[item];
                    int currentTextureIndex = GetPedTextureVariation(Game.PlayerPed.Handle, currentDrawableID);
                    int maxDrawableTextures = GetNumberOfPedTextureVariations(Game.PlayerPed.Handle, currentDrawableID, listIndex) - 1;

                    if (currentTextureIndex == -1)
                        currentTextureIndex = 0;

                    int newTexture = currentTextureIndex < maxDrawableTextures ? currentTextureIndex + 1 : 0;

                    SetPedComponentVariation(Game.PlayerPed.Handle, currentDrawableID, listIndex, newTexture, 0);
                }
                else if (propsMenuListItems.ContainsKey(item)) // prop
                {
                    int currentPropIndex = propsMenuListItems[item];
                    int currentPropVariationIndex = GetPedPropIndex(Game.PlayerPed.Handle, currentPropIndex);
                    int currentPropTextureVariation = GetPedPropTextureIndex(Game.PlayerPed.Handle, currentPropIndex);
                    int maxPropTextureVariations = GetNumberOfPedPropTextureVariations(Game.PlayerPed.Handle, currentPropIndex, currentPropVariationIndex) - 1;

                    int newPropTextureVariationIndex = currentPropTextureVariation < maxPropTextureVariations ? currentPropTextureVariation + 1 : 0;
                    SetPedPropIndex(Game.PlayerPed.Handle, currentPropIndex, currentPropVariationIndex, newPropTextureVariationIndex, true);
                }
            };
            #endregion

        }


        #endregion

        #region get the menu
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
        #endregion

        #region Ped Customization Menu
        ///// <summary>
        ///// Refresh/create the ped customization menu.
        ///// </summary>
        private void RefreshCustomizationMenu()
        {
            drawablesMenuListItems.Clear();
            propsMenuListItems.Clear();
            pedCustomizationMenu.ClearMenuItems();

            #region Ped Drawables
            for (int drawable = 0; drawable < 12; drawable++)
            {
                int currentDrawable = GetPedDrawableVariation(Game.PlayerPed.Handle, drawable);
                int maxVariations = GetNumberOfPedDrawableVariations(Game.PlayerPed.Handle, drawable);
                int maxTextures = GetNumberOfPedTextureVariations(Game.PlayerPed.Handle, drawable, currentDrawable);

                if (maxVariations > 0)
                {
                    List<string> drawableTexturesList = new List<string>();

                    for (int i = 0; i < maxVariations; i++)
                    {
                        drawableTexturesList.Add($"Drawable #{i + 1} (of {maxVariations})");
                    }

                    MenuListItem drawableTextures = new MenuListItem($"{textureNames[drawable]}", drawableTexturesList, currentDrawable, $"使用按鍵 ← & → 來選擇一個 ~o~{textureNames[drawable]} 變異~s~, 按下 ~r~enter~s~ 循環瀏覽可用的紋理.");
                    drawablesMenuListItems.Add(drawableTextures, drawable);
                    pedCustomizationMenu.AddMenuItem(drawableTextures);
                }
            }
            #endregion

            #region Ped Props
            for (int tmpProp = 0; tmpProp < 5; tmpProp++)
            {
                int realProp = tmpProp > 2 ? tmpProp + 3 : tmpProp;

                int currentProp = GetPedPropIndex(Game.PlayerPed.Handle, realProp);
                int maxPropVariations = GetNumberOfPedPropDrawableVariations(Game.PlayerPed.Handle, realProp);

                if (maxPropVariations > 0)
                {
                    List<string> propTexturesList = new List<string>();

                    propTexturesList.Add($"Prop #1 (of {maxPropVariations + 1})");
                    for (int i = 0; i < maxPropVariations; i++)
                    {
                        propTexturesList.Add($"Prop #{i + 2} (of {maxPropVariations + 1})");
                    }


                    MenuListItem propTextures = new MenuListItem($"{propNames[tmpProp]}", propTexturesList, currentProp + 1, $"使用 ← & → 來選擇一個 ~o~{propNames[tmpProp]} 變異~s~, 按下 ~r~enter~s~ 循環瀏覽可用的紋理.");
                    propsMenuListItems.Add(propTextures, realProp);
                    pedCustomizationMenu.AddMenuItem(propTextures);

                }
            }
            pedCustomizationMenu.RefreshIndex();
            #endregion
        }

        #region Textures & Props
        private readonly List<string> textureNames = new List<string>()
        {
            "臉",
            "面具 / 鬍子",
            "髮型 / 髮色",
            "手部 / 服裝",
            "腿 / 褲子",
            "背包 / 降落傘",
            "鞋子",
            "頸部 / 圍巾",
            "襯衫 / 附件",
            "防彈衣 / 附件 2",
            "徽章 / 標誌",
            "襯衫覆蓋 / 外套",
        };

        private readonly List<string> propNames = new List<string>()
        {
            "帽子 / 頭盔", // id 0
            "眼鏡", // id 1
            "雜項", // id 2
            "手錶", // id 6
            "手鍊", // id 7
        };
        #endregion
        #endregion


        #region saved peds menus
        ///// <summary>
        ///// Refresh the spawn saved peds menu.
        ///// </summary>
        //private void RefreshSpawnSavedPedMenu()
        //{
        //    spawnSavedPedMenu.ClearMenuItems();
        //    int findHandle = StartFindKvp("ped_");
        //    List<string> savesFound = new List<string>();
        //    var i = 0;
        //    while (true)
        //    {
        //        i++;
        //        var saveName = FindKvp(findHandle);
        //        if (saveName != null && saveName != "" && saveName != "NULL")
        //        {
        //            // It's already the new format, so add it.
        //            savesFound.Add(saveName);
        //        }
        //        else
        //        {
        //            break;
        //        }
        //    }

        //    var items = new List<string>();
        //    foreach (var savename in savesFound)
        //    {
        //        if (savename.Length > 4)
        //        {
        //            var title = savename.Substring(4);
        //            if (!items.Contains(title))
        //            {
        //                MenuItem savedPedBtn = new MenuItem(title, "Spawn this saved ped.");
        //                spawnSavedPedMenu.AddMenuItem(savedPedBtn);
        //                items.Add(title);
        //            }
        //        }
        //    }

        //    // Sort the menu items (case IN-sensitive) by name.
        //    spawnSavedPedMenu.SortMenuItems((pair1, pair2) => pair1.Text.ToString().ToLower().CompareTo(pair2.Text.ToString().ToLower()));

        //    spawnSavedPedMenu.RefreshIndex();
        //    //spawnSavedPedMenu.UpdateScaleform();
        //}

        ///// <summary>
        ///// Refresh the delete saved peds menu.
        ///// </summary>
        //private void RefreshDeleteSavedPedMenu()
        //{
        //    deleteSavedPedMenu.ClearMenuItems();
        //    int findHandle = StartFindKvp("ped_");
        //    List<string> savesFound = new List<string>();
        //    while (true)
        //    {
        //        var saveName = FindKvp(findHandle);
        //        if (saveName != null && saveName != "" && saveName != "NULL")
        //        {
        //            savesFound.Add(saveName);
        //        }
        //        else
        //        {
        //            break;
        //        }
        //    }
        //    foreach (var savename in savesFound)
        //    {
        //        MenuItem deleteSavedPed = new MenuItem(savename.Substring(4), "~r~Delete ~s~this saved ped, this action can ~r~NOT~s~ be undone!")
        //        {
        //            LeftIcon = MenuItem.Icon.WARNING
        //        };
        //        deleteSavedPedMenu.AddMenuItem(deleteSavedPed);
        //    }

        //    // Sort the menu items (case IN-sensitive) by name.
        //    deleteSavedPedMenu.SortMenuItems((pair1, pair2) => pair1.Text.ToString().ToLower().CompareTo(pair2.Text.ToString().ToLower()));

        //    deleteSavedPedMenu.OnItemSelect += (sender, item, idex) =>
        //    {
        //        var name = item.Text.ToString();
        //        StorageManager.DeleteSavedStorageItem("ped_" + name);
        //        Notify.Success("Saved ped deleted.");
        //        deleteSavedPedMenu.GoBack();
        //    };

        //    deleteSavedPedMenu.RefreshIndex();
        //    //deleteSavedPedMenu.UpdateScaleform();
        //}
        #endregion

        //private List<string> stuff = new List<string>()
        //    {
        //        "csb_abigail",
        //    "csb_anita",
        //    "csb_anton",
        //    "csb_ballasog",
        //    "csb_bride",
        //    "csb_burgerdrug",
        //    "csb_car3guy1",
        //    "csb_car3guy2",
        //    "csb_chef",
        //    "csb_chin_goon",
        //    "csb_cletus",
        //    "csb_cop",
        //    "csb_customer",
        //    "csb_denise_friend",
        //    "csb_fos_rep",
        //    "csb_groom",
        //    "csb_grove_str_dlr",
        //    "csb_g",
        //    "csb_hao",
        //    "csb_hugh",
        //    "csb_imran",
        //    "csb_janitor",
        //    "csb_maude",
        //    "csb_mweather",
        //    "csb_ortega",
        //    "csb_oscar",
        //    "csb_porndudes",
        //    "csb_prologuedriver",
        //    "csb_prolsec",
        //    "csb_ramp_gang",
        //    "csb_ramp_hic",
        //    "csb_ramp_hipster",
        //    "csb_ramp_marine",
        //    "csb_ramp_mex",
        //    "csb_reporter",
        //    "csb_roccopelosi",
        //    "csb_screen_writer",
        //    "csb_stripper_01",
        //    "csb_stripper_02",
        //    "csb_tonya",
        //    "csb_trafficwarden",
        //    "g_f_y_ballas_01",
        //    "g_f_y_families_01",
        //    "g_f_y_lost_01",
        //    "g_f_y_vagos_01",
        //    "g_m_m_armboss_01",
        //    "g_m_m_armgoon_01",
        //    "g_m_m_armlieut_01",
        //    "g_m_m_chemwork_01",
        //    "g_m_m_chiboss_01",
        //    "g_m_m_chicold_01",
        //    "g_m_m_chigoon_01",
        //    "g_m_m_chigoon_02",
        //    "g_m_m_korboss_01",
        //    "g_m_m_mexboss_01",
        //    "g_m_m_mexboss_02",
        //    "g_m_y_armgoon_02",
        //    "g_m_y_azteca_01",
        //    "g_m_y_ballaeast_01",
        //    "g_m_y_ballaorig_01",
        //    "g_m_y_ballasout_01",
        //    "g_m_y_famca_01",
        //    "g_m_y_famdnf_01",
        //    "g_m_y_famfor_01",
        //    "g_m_y_korean_01",
        //    "g_m_y_korean_02",
        //    "g_m_y_korlieut_01",
        //    "g_m_y_lost_01",
        //    "g_m_y_lost_02",
        //    "g_m_y_lost_03",
        //    "g_m_y_mexgang_01",
        //    "g_m_y_mexgoon_01",
        //    "g_m_y_mexgoon_02",
        //    "g_m_y_mexgoon_03",
        //    "g_m_y_pologoon_01",
        //    "g_m_y_pologoon_02",
        //    "g_m_y_salvaboss_01",
        //    "g_m_y_salvagoon_01",
        //    "g_m_y_salvagoon_02",
        //    "g_m_y_salvagoon_03",
        //    "g_m_y_strpunk_01",
        //    "g_m_y_strpunk_02",
        //    "hc_driver",
        //    "hc_gunman",
        //    "hc_hacker",
        //    "ig_abigail",
        //    "ig_amandatownley",
        //    "ig_andreas",
        //    "ig_ashley",
        //    "ig_ballasog",
        //    "ig_bankman",
        //    "ig_barry",
        //    "ig_bestmen",
        //    "ig_beverly",
        //    "ig_brad",
        //    "ig_bride",
        //    "ig_car3guy1",
        //    "ig_car3guy2",
        //    "ig_casey",
        //    "ig_chef",
        //    "ig_chengsr",
        //    "ig_chrisformage",
        //    "ig_claypain",
        //    "ig_clay",
        //    "ig_cletus",
        //    "ig_dale",
        //    "ig_davenorton",
        //    "ig_denise",
        //    "ig_devin",
        //    "ig_dom",
        //    "ig_dreyfuss",
        //    "ig_drfriedlander",
        //    "ig_fabien",
        //    "ig_fbisuit_01",
        //    "ig_floyd",
        //    "ig_groom",
        //    "ig_hao",
        //    "ig_hunter",
        //    "ig_janet",
        //    "ig_jay_norris",
        //    "ig_jewelass",
        //    "ig_jimmyboston",
        //    "ig_jimmydisanto",
        //    "ig_joeminuteman",
        //    "ig_johnnyklebitz",
        //    "ig_josef",
        //    "ig_josh",
        //    "ig_kerrymcintosh",
        //    "ig_lamardavis",
        //    "ig_lazlow",
        //    "ig_lestercrest",
        //    "ig_lifeinvad_01",
        //    "ig_lifeinvad_02",
        //    "ig_magenta",
        //    "ig_manuel",
        //    "ig_marnie",
        //    "ig_maryann",
        //    "ig_maude",
        //    "ig_michelle",
        //    "ig_milton",
        //    "ig_molly",
        //    "ig_mrk",
        //    "ig_mrsphillips",
        //    "ig_mrs_thornhill",
        //    "ig_natalia",
        //    "ig_nervousron",
        //    "ig_nigel",
        //    "ig_old_man1a",
        //    "ig_old_man2",
        //    "ig_omega",
        //    "ig_oneil",
        //    "ig_orleans",
        //    "ig_ortega",
        //    "ig_paper",
        //    "ig_patricia",
        //    "ig_priest",
        //    "ig_prolsec_02",
        //    "ig_ramp_gang",
        //    "ig_ramp_hic",
        //    "ig_ramp_hipster",
        //    "ig_ramp_mex",
        //    "ig_roccopelosi",
        //    "ig_russiandrunk",
        //    "ig_screen_writer",
        //    "ig_siemonyetarian",
        //    "ig_solomon",
        //    "ig_stevehains",
        //    "ig_stretch",
        //    "ig_talina",
        //    "ig_tanisha",
        //    "ig_taocheng",
        //    "ig_taostranslator",
        //    "ig_tenniscoach",
        //    "ig_terry",
        //    "ig_tomepsilon",
        //    "ig_tonya",
        //    "ig_tracydisanto",
        //    "ig_trafficwarden",
        //    "ig_tylerdix",
        //    "ig_wade",
        //    "ig_zimbor",
        //    "mp_f_deadhooker",
        //    "mp_f_misty_01",
        //    "mp_f_stripperlite",
        //    "mp_g_m_pros_01",
        //    "mp_m_claude_01",
        //    "mp_m_exarmy_01",
        //    "mp_m_famdd_01",
        //    "mp_m_fibsec_01",
        //    "mp_m_marston_01",
        //    "mp_m_niko_01",
        //    "mp_m_shopkeep_01",
        //    "mp_s_m_armoured_01",
        //    "player_one",
        //    "player_two",
        //    "player_zero",
        //    "s_f_m_fembarber",
        //    "s_f_m_maid_01",
        //    "s_f_m_shop_high",
        //    "s_f_m_sweatshop_01",
        //    "s_f_y_airhostess_01",
        //    "s_f_y_bartender_01",
        //    "s_f_y_baywatch_01",
        //    "s_f_y_cop_01",
        //    "s_f_y_factory_01",
        //    "s_f_y_hooker_01",
        //    "s_f_y_hooker_02",
        //    "s_f_y_hooker_03",
        //    "s_f_y_migrant_01",
        //    "s_f_y_movprem_01",
        //    "s_f_y_ranger_01",
        //    "s_f_y_scrubs_01",
        //    "s_f_y_sheriff_01",
        //    "s_f_y_shop_low",
        //    "s_f_y_shop_mid",
        //    "s_f_y_stripperlite",
        //    "s_f_y_stripper_01",
        //    "s_f_y_stripper_02",
        //    "s_f_y_sweatshop_01",
        //    "s_m_m_ammucountry",
        //    "s_m_m_armoured_01",
        //    "s_m_m_armoured_02",
        //    "s_m_m_autoshop_01",
        //    "s_m_m_autoshop_02",
        //    "s_m_m_bouncer_01",
        //    "s_m_m_chemsec_01",
        //    "s_m_m_ciasec_01",
        //    "s_m_m_cntrybar_01",
        //    "s_m_m_dockwork_01",
        //    "s_m_m_doctor_01",
        //    "s_m_m_fiboffice_01",
        //    "s_m_m_fiboffice_02",
        //    "s_m_m_gaffer_01",
        //    "s_m_m_gardener_01",
        //    "s_m_m_gentransport",
        //    "s_m_m_hairdress_01",
        //    "s_m_m_highsec_01",
        //    "s_m_m_highsec_02",
        //    "s_m_m_janitor",
        //    "s_m_m_lathandy_01",
        //    "s_m_m_lifeinvad_01",
        //    "s_m_m_linecook",
        //    "s_m_m_lsmetro_01",
        //    "s_m_m_mariachi_01",
        //    "s_m_m_marine_01",
        //    "s_m_m_marine_02",
        //    "s_m_m_migrant_01",
        //    "s_m_m_movalien_01",
        //    "s_m_m_movprem_01",
        //    "s_m_m_movspace_01",
        //    "s_m_m_paramedic_01",
        //    "s_m_m_pilot_01",
        //    "s_m_m_pilot_02",
        //    "s_m_m_postal_01",
        //    "s_m_m_postal_02",
        //    "s_m_m_prisguard_01",
        //    "s_m_m_scientist_01",
        //    "s_m_m_security_01",
        //    "s_m_m_snowcop_01",
        //    "s_m_m_strperf_01",
        //    "s_m_m_strpreach_01",
        //    "s_m_m_strvend_01",
        //    "s_m_m_trucker_01",
        //    "s_m_m_ups_01",
        //    "s_m_m_ups_02",
        //    "s_m_o_busker_01",
        //    "s_m_y_airworker",
        //    "s_m_y_ammucity_01",
        //    "s_m_y_armymech_01",
        //    "s_m_y_autopsy_01",
        //    "s_m_y_barman_01",
        //    "s_m_y_baywatch_01",
        //    "s_m_y_blackops_01",
        //    "s_m_y_blackops_02",
        //    "s_m_y_busboy_01",
        //    "s_m_y_chef_01",
        //    "s_m_y_clown_01",
        //    "s_m_y_construct_01",
        //    "s_m_y_construct_02",
        //    "s_m_y_cop_01",
        //    "s_m_y_dealer_01",
        //    "s_m_y_devinsec_01",
        //    "s_m_y_dockwork_01",
        //    "s_m_y_doorman_01",
        //    "s_m_y_dwservice_01",
        //    "s_m_y_dwservice_02",
        //    "s_m_y_factory_01",
        //    "s_m_y_fireman_01",
        //    "s_m_y_garbage",
        //    "s_m_y_grip_01",
        //    "s_m_y_hwaycop_01",
        //    "s_m_y_marine_01",
        //    "s_m_y_marine_02",
        //    "s_m_y_marine_03",
        //    "s_m_y_mime",
        //    "s_m_y_pestcont_01",
        //    "s_m_y_pilot_01",
        //    "s_m_y_prismuscl_01",
        //    "s_m_y_prisoner_01",
        //    "s_m_y_ranger_01",
        //    "s_m_y_robber_01",
        //    "s_m_y_sheriff_01",
        //    "s_m_y_shop_mask",
        //    "s_m_y_strvend_01",
        //    "s_m_y_swat_01",
        //    "s_m_y_uscg_01",
        //    "s_m_y_valet_01",
        //    "s_m_y_waiter_01",
        //    "s_m_y_winclean_01",
        //    "s_m_y_xmech_01",
        //    "s_m_y_xmech_02",
        //    "u_f_m_corpse_01",
        //    "u_f_m_miranda",
        //    "u_f_m_promourn_01",
        //    "u_f_o_moviestar",
        //    "u_f_o_prolhost_01",
        //    "u_f_y_bikerchic",
        //    "u_f_y_comjane",
        //    "u_f_y_corpse_01",
        //    "u_f_y_corpse_02",
        //    "u_f_y_hotposh_01",
        //    "u_f_y_jewelass_01",
        //    "u_f_y_mistress",
        //    "u_f_y_poppymich",
        //    "u_f_y_princess",
        //    "u_f_y_spyactress",
        //    "u_m_m_aldinapoli",
        //    "u_m_m_bankman",
        //    "u_m_m_bikehire_01",
        //    "u_m_m_fibarchitect",
        //    "u_m_m_filmdirector",
        //    "u_m_m_glenstank_01",
        //    "u_m_m_griff_01",
        //    "u_m_m_jesus_01",
        //    "u_m_m_jewelsec_01",
        //    "u_m_m_jewelthief",
        //    "u_m_m_markfost",
        //    "u_m_m_partytarget",
        //    "u_m_m_prolsec_01",
        //    "u_m_m_promourn_01",
        //    "u_m_m_rivalpap",
        //    "u_m_m_spyactor",
        //    "u_m_m_willyfist",
        //    "u_m_o_finguru_01",
        //    "u_m_o_taphillbilly",
        //    "u_m_o_tramp_01",
        //    "u_m_y_abner",
        //    "u_m_y_antonb",
        //    "u_m_y_babyd",
        //    "u_m_y_baygor",
        //    "u_m_y_burgerdrug_01",
        //    "u_m_y_chip",
        //    "u_m_y_cyclist_01",
        //    "u_m_y_fibmugger_01",
        //    "u_m_y_guido_01",
        //    "u_m_y_gunvend_01",
        //    "u_m_y_hippie_01",
        //    "u_m_y_imporage",
        //    "u_m_y_justin",
        //    "u_m_y_mani",
        //    "u_m_y_militarybum",
        //    "u_m_y_paparazzi",
        //    "u_m_y_party_01",
        //    "u_m_y_pogo_01",
        //    "u_m_y_prisoner_01",
        //    "u_m_y_proldriver_01",
        //    "u_m_y_rsranger_01",
        //    "u_m_y_sbike",
        //    "u_m_y_staggrm_01",
        //    "u_m_y_tattoo_01",
        //    "u_m_y_zombie_01"
        //    };

        #region Model Names
        private Dictionary<string, string> mainModels = new Dictionary<string, string>()
        {
            ["player_one"] = "Franklin",
            ["player_two"] = "Trevor",
            ["player_zero"] = "Michael",
            ["mp_f_freemode_01"] = "FreemodeFemale01",
            ["mp_m_freemode_01"] = "FreemodeMale01"
        };
        private Dictionary<string, string> animalModels = new Dictionary<string, string>()
        {
            ["a_c_boar"] = "Boar",
            ["a_c_cat_01"] = "Cat",
            ["a_c_chickenhawk"] = "ChickenHawk",
            ["a_c_chimp"] = "Chimp",
            ["a_c_chop"] = "Chop",
            ["a_c_cormorant"] = "Cormorant",
            ["a_c_cow"] = "Cow",
            ["a_c_coyote"] = "Coyote",
            ["a_c_crow"] = "Crow",
            ["a_c_deer"] = "Deer",
            ["a_c_dolphin"] = "Dolphin",
            ["a_c_fish"] = "Fish",
            ["a_c_hen"] = "Hen",
            ["a_c_humpback"] = "Humpback",
            ["a_c_husky"] = "Husky",
            ["a_c_killerwhale"] = "KillerWhale",
            ["a_c_mtlion"] = "MountainLion",
            ["a_c_pig"] = "Pig",
            ["a_c_pigeon"] = "Pigeon",
            ["a_c_poodle"] = "Poodle",
            ["a_c_pug"] = "Pug",
            ["a_c_rabbit_01"] = "Rabbit",
            ["a_c_rat"] = "Rat",
            ["a_c_retriever"] = "Retriever",
            ["a_c_rhesus"] = "Rhesus",
            ["a_c_rottweiler"] = "Rottweiler",
            ["a_c_seagull"] = "Seagull",
            ["a_c_sharkhammer"] = "HammerShark",
            ["a_c_sharktiger"] = "TigerShark",
            ["a_c_shepherd"] = "Shepherd",
            ["a_c_westy"] = "Westy"
        };
        private Dictionary<string, string> maleModels = new Dictionary<string, string>()
        {
            ["a_m_m_acult_01"] = "Acult01AMM",
            ["a_m_m_afriamer_01"] = "AfriAmer01AMM",
            ["a_m_m_beach_01"] = "Beach01AMM",
            ["a_m_m_beach_02"] = "Beach02AMM",
            ["a_m_m_bevhills_01"] = "Bevhills01AMM",
            ["a_m_m_bevhills_02"] = "Bevhills02AMM",
            ["a_m_m_business_01"] = "Business01AMM",
            ["a_m_m_eastsa_01"] = "Eastsa01AMM",
            ["a_m_m_eastsa_02"] = "Eastsa02AMM",
            ["a_m_m_farmer_01"] = "Farmer01AMM",
            ["a_m_m_fatlatin_01"] = "Fatlatin01AMM",
            ["a_m_m_genfat_01"] = "Genfat01AMM",
            ["a_m_m_genfat_02"] = "Genfat02AMM",
            ["a_m_m_golfer_01"] = "Golfer01AMM",
            ["a_m_m_hasjew_01"] = "Hasjew01AMM",
            ["a_m_m_hillbilly_01"] = "Hillbilly01AMM",
            ["a_m_m_hillbilly_02"] = "Hillbilly02AMM",
            ["a_m_m_indian_01"] = "Indian01AMM",
            ["a_m_m_ktown_01"] = "Ktown01AMM",
            ["a_m_m_malibu_01"] = "Malibu01AMM",
            ["a_m_m_mexcntry_01"] = "MexCntry01AMM",
            ["a_m_m_mexlabor_01"] = "MexLabor01AMM",
            ["a_m_m_og_boss_01"] = "OgBoss01AMM",
            ["a_m_m_paparazzi_01"] = "Paparazzi01AMM",
            ["a_m_m_polynesian_01"] = "Polynesian01AMM",
            ["a_m_m_prolhost_01"] = "PrologueHostage01AMM",
            ["a_m_m_rurmeth_01"] = "Rurmeth01AMM",
            ["a_m_m_salton_01"] = "Salton01AMM",
            ["a_m_m_salton_02"] = "Salton02AMM",
            ["a_m_m_salton_03"] = "Salton03AMM",
            ["a_m_m_salton_04"] = "Salton04AMM",
            ["a_m_m_skater_01"] = "Skater01AMM",
            ["a_m_m_skidrow_01"] = "Skidrow01AMM",
            ["a_m_m_socenlat_01"] = "Socenlat01AMM",
            ["a_m_m_soucent_01"] = "Soucent01AMM",
            ["a_m_m_soucent_02"] = "Soucent02AMM",
            ["a_m_m_soucent_03"] = "Soucent03AMM",
            ["a_m_m_soucent_04"] = "Soucent04AMM",
            ["a_m_m_stlat_02"] = "Stlat02AMM",
            ["a_m_m_tennis_01"] = "Tennis01AMM",
            ["a_m_m_tourist_01"] = "Tourist01AMM",
            ["a_m_m_trampbeac_01"] = "TrampBeac01AMM",
            ["a_m_m_tramp_01"] = "Tramp01AMM",
            ["a_m_m_tranvest_01"] = "Tranvest01AMM",
            ["a_m_m_tranvest_02"] = "Tranvest02AMM",
            ["a_m_o_acult_01"] = "Acult01AMO",
            ["a_m_o_acult_02"] = "Acult02AMO",
            ["a_m_o_beach_01"] = "Beach01AMO",
            ["a_m_o_genstreet_01"] = "Genstreet01AMO",
            ["a_m_o_ktown_01"] = "Ktown01AMO",
            ["a_m_o_salton_01"] = "Salton01AMO",
            ["a_m_o_soucent_01"] = "Soucent01AMO",
            ["a_m_o_soucent_02"] = "Soucent02AMO",
            ["a_m_o_soucent_03"] = "Soucent03AMO",
            ["a_m_o_tramp_01"] = "Tramp01AMO",
            ["a_m_y_acult_01"] = "Acult01AMY",
            ["a_m_y_acult_02"] = "Acult02AMY",
            ["a_m_y_beachvesp_01"] = "Beachvesp01AMY",
            ["a_m_y_beachvesp_02"] = "Beachvesp02AMY",
            ["a_m_y_beach_01"] = "Beach01AMY",
            ["a_m_y_beach_02"] = "Beach02AMY",
            ["a_m_y_beach_03"] = "Beach03AMY",
            ["a_m_y_bevhills_01"] = "Bevhills01AMY",
            ["a_m_y_bevhills_02"] = "Bevhills02AMY",
            ["a_m_y_breakdance_01"] = "Breakdance01AMY",
            ["a_m_y_busicas_01"] = "Busicas01AMY",
            ["a_m_y_business_01"] = "Business01AMY",
            ["a_m_y_business_02"] = "Business02AMY",
            ["a_m_y_business_03"] = "Business03AMY",
            ["a_m_y_cyclist_01"] = "Cyclist01AMY",
            ["a_m_y_dhill_01"] = "Dhill01AMY",
            ["a_m_y_downtown_01"] = "Downtown01AMY",
            ["a_m_y_eastsa_01"] = "Eastsa01AMY",
            ["a_m_y_eastsa_02"] = "Eastsa02AMY",
            ["a_m_y_epsilon_01"] = "Epsilon01AMY",
            ["a_m_y_epsilon_02"] = "Epsilon02AMY",
            ["a_m_y_gay_01"] = "Gay01AMY",
            ["a_m_y_gay_02"] = "Gay02AMY",
            ["a_m_y_genstreet_01"] = "Genstreet01AMY",
            ["a_m_y_genstreet_02"] = "Genstreet02AMY",
            ["a_m_y_golfer_01"] = "Golfer01AMY",
            ["a_m_y_hasjew_01"] = "Hasjew01AMY",
            ["a_m_y_hiker_01"] = "Hiker01AMY",
            ["a_m_y_hippy_01"] = "Hippy01AMY",
            ["a_m_y_hipster_01"] = "Hipster01AMY",
            ["a_m_y_hipster_02"] = "Hipster02AMY",
            ["a_m_y_hipster_03"] = "Hipster03AMY",
            ["a_m_y_indian_01"] = "Indian01AMY",
            ["a_m_y_jetski_01"] = "Jetski01AMY",
            ["a_m_y_juggalo_01"] = "Juggalo01AMY",
            ["a_m_y_ktown_01"] = "Ktown01AMY",
            ["a_m_y_ktown_02"] = "Ktown02AMY",
            ["a_m_y_latino_01"] = "Latino01AMY",
            ["a_m_y_methhead_01"] = "Methhead01AMY",
            ["a_m_y_mexthug_01"] = "MexThug01AMY",
            ["a_m_y_motox_01"] = "Motox01AMY",
            ["a_m_y_motox_02"] = "Motox02AMY",
            ["a_m_y_musclbeac_01"] = "Musclbeac01AMY",
            ["a_m_y_musclbeac_02"] = "Musclbeac02AMY",
            ["a_m_y_polynesian_01"] = "Polynesian01AMY",
            ["a_m_y_roadcyc_01"] = "Roadcyc01AMY",
            ["a_m_y_runner_01"] = "Runner01AMY",
            ["a_m_y_runner_02"] = "Runner02AMY",
            ["a_m_y_salton_01"] = "Salton01AMY",
            ["a_m_y_skater_01"] = "Skater01AMY",
            ["a_m_y_skater_02"] = "Skater02AMY",
            ["a_m_y_soucent_01"] = "Soucent01AMY",
            ["a_m_y_soucent_02"] = "Soucent02AMY",
            ["a_m_y_soucent_03"] = "Soucent03AMY",
            ["a_m_y_soucent_04"] = "Soucent04AMY",
            ["a_m_y_stbla_01"] = "Stbla01AMY",
            ["a_m_y_stbla_02"] = "Stbla02AMY",
            ["a_m_y_stlat_01"] = "Stlat01AMY",
            ["a_m_y_stwhi_01"] = "Stwhi01AMY",
            ["a_m_y_stwhi_02"] = "Stwhi02AMY",
            ["a_m_y_sunbathe_01"] = "Sunbathe01AMY",
            ["a_m_y_surfer_01"] = "Surfer01AMY",
            ["a_m_y_vindouche_01"] = "Vindouche01AMY",
            ["a_m_y_vinewood_01"] = "Vinewood01AMY",
            ["a_m_y_vinewood_02"] = "Vinewood02AMY",
            ["a_m_y_vinewood_03"] = "Vinewood03AMY",
            ["a_m_y_vinewood_04"] = "Vinewood04AMY",
            ["a_m_y_yoga_01"] = "Yoga01AMY"
        };
        private Dictionary<string, string> femaleModels = new Dictionary<string, string>()
        {
            ["a_f_m_beach_01"] = "Beach01AFM",
            ["a_f_m_bevhills_01"] = "Bevhills01AFM",
            ["a_f_m_bevhills_02"] = "Bevhills02AFM",
            ["a_f_m_bodybuild_01"] = "Bodybuild01AFM",
            ["a_f_m_business_02"] = "Business02AFM",
            ["a_f_m_downtown_01"] = "Downtown01AFM",
            ["a_f_m_eastsa_01"] = "Eastsa01AFM",
            ["a_f_m_eastsa_02"] = "Eastsa02AFM",
            ["a_f_m_fatbla_01"] = "FatBla01AFM",
            ["a_f_m_fatcult_01"] = "FatCult01AFM",
            ["a_f_m_fatwhite_01"] = "FatWhite01AFM",
            ["a_f_m_ktown_01"] = "Ktown01AFM",
            ["a_f_m_ktown_02"] = "Ktown02AFM",
            ["a_f_m_prolhost_01"] = "PrologueHostage01AFM",
            ["a_f_m_salton_01"] = "Salton01AFM",
            ["a_f_m_skidrow_01"] = "Skidrow01AFM",
            ["a_f_m_soucentmc_01"] = "Soucentmc01AFM",
            ["a_f_m_soucent_01"] = "Soucent01AFM",
            ["a_f_m_soucent_02"] = "Soucent02AFM",
            ["a_f_m_tourist_01"] = "Tourist01AFM",
            ["a_f_m_trampbeac_01"] = "TrampBeac01AFM",
            ["a_f_m_tramp_01"] = "Tramp01AFM",
            ["a_f_o_genstreet_01"] = "Genstreet01AFO",
            ["a_f_o_indian_01"] = "Indian01AFO",
            ["a_f_o_ktown_01"] = "Ktown01AFO",
            ["a_f_o_salton_01"] = "Salton01AFO",
            ["a_f_o_soucent_01"] = "Soucent01AFO",
            ["a_f_o_soucent_02"] = "Soucent02AFO",
            ["a_f_y_beach_01"] = "Beach01AFY",
            ["a_f_y_bevhills_01"] = "Bevhills01AFY",
            ["a_f_y_bevhills_02"] = "Bevhills02AFY",
            ["a_f_y_bevhills_03"] = "Bevhills03AFY",
            ["a_f_y_bevhills_04"] = "Bevhills04AFY",
            ["a_f_y_business_01"] = "Business01AFY",
            ["a_f_y_business_02"] = "Business02AFY",
            ["a_f_y_business_03"] = "Business03AFY",
            ["a_f_y_business_04"] = "Business04AFY",
            ["a_f_y_eastsa_01"] = "Eastsa01AFY",
            ["a_f_y_eastsa_02"] = "Eastsa02AFY",
            ["a_f_y_eastsa_03"] = "Eastsa03AFY",
            ["a_f_y_epsilon_01"] = "Epsilon01AFY",
            ["a_f_y_fitness_01"] = "Fitness01AFY",
            ["a_f_y_fitness_02"] = "Fitness02AFY",
            ["a_f_y_genhot_01"] = "Genhot01AFY",
            ["a_f_y_golfer_01"] = "Golfer01AFY",
            ["a_f_y_hiker_01"] = "Hiker01AFY",
            ["a_f_y_hippie_01"] = "Hippie01AFY",
            ["a_f_y_hipster_01"] = "Hipster01AFY",
            ["a_f_y_hipster_02"] = "Hipster02AFY",
            ["a_f_y_hipster_03"] = "Hipster03AFY",
            ["a_f_y_hipster_04"] = "Hipster04AFY",
            ["a_f_y_indian_01"] = "Indian01AFY",
            ["a_f_y_juggalo_01"] = "Juggalo01AFY",
            ["a_f_y_runner_01"] = "Runner01AFY",
            ["a_f_y_rurmeth_01"] = "Rurmeth01AFY",
            ["a_f_y_scdressy_01"] = "Scdressy01AFY",
            ["a_f_y_skater_01"] = "Skater01AFY",
            ["a_f_y_soucent_01"] = "Soucent01AFY",
            ["a_f_y_soucent_02"] = "Soucent02AFY",
            ["a_f_y_soucent_03"] = "Soucent03AFY",
            ["a_f_y_tennis_01"] = "Tennis01AFY",
            ["a_f_y_topless_01"] = "Topless01AFY",
            ["a_f_y_tourist_01"] = "Tourist01AFY",
            ["a_f_y_tourist_02"] = "Tourist02AFY",
            ["a_f_y_vinewood_01"] = "Vinewood01AFY",
            ["a_f_y_vinewood_02"] = "Vinewood02AFY",
            ["a_f_y_vinewood_03"] = "Vinewood03AFY",
            ["a_f_y_vinewood_04"] = "Vinewood04AFY",
            ["a_f_y_yoga_01"] = "Yoga01AFY"
        };
        private Dictionary<string, string> otherPeds = new Dictionary<string, string>()
        {
            ["csb_abigail"] = "AbigailCutscene",
            ["csb_anita"] = "AnitaCutscene",
            ["csb_anton"] = "AntonCutscene",
            ["csb_ballasog"] = "BallasogCutscene",
            ["csb_bride"] = "BrideCutscene",
            ["csb_burgerdrug"] = "BurgerDrugCutscene",
            ["csb_car3guy1"] = "Car3Guy1Cutscene",
            ["csb_car3guy2"] = "Car3Guy2Cutscene",
            ["csb_chef"] = "ChefCutscene",
            ["csb_chin_goon"] = "ChinGoonCutscene",
            ["csb_cletus"] = "CletusCutscene",
            ["csb_cop"] = "CopCutscene",
            ["csb_customer"] = "CustomerCutscene",
            ["csb_denise_friend"] = "DeniseFriendCutscene",
            ["csb_fos_rep"] = "FosRepCutscene",
            ["csb_groom"] = "GroomCutscene",
            ["csb_grove_str_dlr"] = "GroveStrDlrCutscene",
            ["csb_g"] = "GCutscene",
            ["csb_hao"] = "HaoCutscene",
            ["csb_hugh"] = "HughCutscene",
            ["csb_imran"] = "ImranCutscene",
            ["csb_janitor"] = "JanitorCutscene",
            ["csb_maude"] = "MaudeCutscene",
            ["csb_mweather"] = "MerryWeatherCutscene",
            ["csb_ortega"] = "OrtegaCutscene",
            ["csb_oscar"] = "OscarCutscene",
            ["csb_porndudes"] = "PornDudesCutscene",
            ["csb_prologuedriver"] = "PrologueDriverCutscene",
            ["csb_prolsec"] = "PrologueSec01Cutscene",
            ["csb_ramp_gang"] = "RampGangCutscene",
            ["csb_ramp_hic"] = "RampHicCutscene",
            ["csb_ramp_hipster"] = "RampHipsterCutscene",
            ["csb_ramp_marine"] = "RampMarineCutscene",
            ["csb_ramp_mex"] = "RampMexCutscene",
            ["csb_reporter"] = "ReporterCutscene",
            ["csb_roccopelosi"] = "RoccoPelosiCutscene",
            ["csb_screen_writer"] = "ScreenWriterCutscene",
            ["csb_stripper_01"] = "Stripper01Cutscene",
            ["csb_stripper_02"] = "Stripper02Cutscene",
            ["csb_tonya"] = "TonyaCutscene",
            ["csb_trafficwarden"] = "TrafficWardenCutscene",
            ["g_f_y_ballas_01"] = "Ballas01GFY",
            ["g_f_y_families_01"] = "Families01GFY",
            ["g_f_y_lost_01"] = "Lost01GFY",
            ["g_f_y_vagos_01"] = "Vagos01GFY",
            ["g_m_m_armboss_01"] = "ArmBoss01GMM",
            ["g_m_m_armgoon_01"] = "ArmGoon01GMM",
            ["g_m_m_armlieut_01"] = "ArmLieut01GMM",
            ["g_m_m_chemwork_01"] = "ChemWork01GMM",
            ["g_m_m_chiboss_01"] = "ChiBoss01GMM",
            ["g_m_m_chicold_01"] = "ChiCold01GMM",
            ["g_m_m_chigoon_01"] = "ChiGoon01GMM",
            ["g_m_m_chigoon_02"] = "ChiGoon02GMM",
            ["g_m_m_korboss_01"] = "KorBoss01GMM",
            ["g_m_m_mexboss_01"] = "MexBoss01GMM",
            ["g_m_m_mexboss_02"] = "MexBoss02GMM",
            ["g_m_y_armgoon_02"] = "ArmGoon02GMY",
            ["g_m_y_azteca_01"] = "Azteca01GMY",
            ["g_m_y_ballaeast_01"] = "BallaEast01GMY",
            ["g_m_y_ballaorig_01"] = "BallaOrig01GMY",
            ["g_m_y_ballasout_01"] = "BallaSout01GMY",
            ["g_m_y_famca_01"] = "Famca01GMY",
            ["g_m_y_famdnf_01"] = "Famdnf01GMY",
            ["g_m_y_famfor_01"] = "Famfor01GMY",
            ["g_m_y_korean_01"] = "Korean01GMY",
            ["g_m_y_korean_02"] = "Korean02GMY",
            ["g_m_y_korlieut_01"] = "KorLieut01GMY",
            ["g_m_y_lost_01"] = "Lost01GMY",
            ["g_m_y_lost_02"] = "Lost02GMY",
            ["g_m_y_lost_03"] = "Lost03GMY",
            ["g_m_y_mexgang_01"] = "MexGang01GMY",
            ["g_m_y_mexgoon_01"] = "MexGoon01GMY",
            ["g_m_y_mexgoon_02"] = "MexGoon02GMY",
            ["g_m_y_mexgoon_03"] = "MexGoon03GMY",
            ["g_m_y_pologoon_01"] = "PoloGoon01GMY",
            ["g_m_y_pologoon_02"] = "PoloGoon02GMY",
            ["g_m_y_salvaboss_01"] = "SalvaBoss01GMY",
            ["g_m_y_salvagoon_01"] = "SalvaGoon01GMY",
            ["g_m_y_salvagoon_02"] = "SalvaGoon02GMY",
            ["g_m_y_salvagoon_03"] = "SalvaGoon03GMY",
            ["g_m_y_strpunk_01"] = "StrPunk01GMY",
            ["g_m_y_strpunk_02"] = "StrPunk02GMY",
            ["hc_driver"] = "PestContDriver",
            ["hc_gunman"] = "PestContGunman",
            ["hc_hacker"] = "Hacker",
            ["ig_abigail"] = "Abigail",
            ["ig_amandatownley"] = "AmandaTownley",
            ["ig_andreas"] = "Andreas",
            ["ig_ashley"] = "Ashley",
            ["ig_ballasog"] = "Ballasog",
            ["ig_bankman"] = "Bankman",
            ["ig_barry"] = "Barry",
            ["ig_bestmen"] = "Bestmen",
            ["ig_beverly"] = "Beverly",
            ["ig_brad"] = "Brad",
            ["ig_bride"] = "Bride",
            ["ig_car3guy1"] = "Car3Guy1",
            ["ig_car3guy2"] = "Car3Guy2",
            ["ig_casey"] = "Casey",
            ["ig_chef"] = "Chef",
            ["ig_chengsr"] = "WeiCheng",
            ["ig_chrisformage"] = "CrisFormage",
            ["ig_claypain"] = "Claypain",
            ["ig_clay"] = "Clay",
            ["ig_cletus"] = "Cletus",
            ["ig_dale"] = "Dale",
            ["ig_davenorton"] = "DaveNorton",
            ["ig_denise"] = "Denise",
            ["ig_devin"] = "Devin",
            ["ig_dom"] = "Dom",
            ["ig_dreyfuss"] = "Dreyfuss",
            ["ig_drfriedlander"] = "DrFriedlander",
            ["ig_fabien"] = "Fabien",
            ["ig_fbisuit_01"] = "FbiSuit01",
            ["ig_floyd"] = "Floyd",
            ["ig_groom"] = "Groom",
            ["ig_hao"] = "Hao",
            ["ig_hunter"] = "Hunter",
            ["ig_janet"] = "Janet",
            ["ig_jay_norris"] = "JayNorris",
            ["ig_jewelass"] = "Jewelass",
            ["ig_jimmyboston"] = "JimmyBoston",
            ["ig_jimmydisanto"] = "JimmyDisanto",
            ["ig_joeminuteman"] = "JoeMinuteman",
            ["ig_johnnyklebitz"] = "JohnnyKlebitz",
            ["ig_josef"] = "Josef",
            ["ig_josh"] = "Josh",
            ["ig_kerrymcintosh"] = "KerryMcintosh",
            ["ig_lamardavis"] = "LamarDavis",
            ["ig_lazlow"] = "Lazlow",
            ["ig_lestercrest"] = "LesterCrest",
            ["ig_lifeinvad_01"] = "Lifeinvad01",
            ["ig_lifeinvad_02"] = "Lifeinvad02",
            ["ig_magenta"] = "Magenta",
            ["ig_manuel"] = "Manuel",
            ["ig_marnie"] = "Marnie",
            ["ig_maryann"] = "MaryAnn",
            ["ig_maude"] = "Maude",
            ["ig_michelle"] = "Michelle",
            ["ig_milton"] = "Milton",
            ["ig_molly"] = "Molly",
            ["ig_mrk"] = "MrK",
            ["ig_mrsphillips"] = "MrsPhillips",
            ["ig_mrs_thornhill"] = "MrsThornhill",
            ["ig_natalia"] = "Natalia",
            ["ig_nervousron"] = "NervousRon",
            ["ig_nigel"] = "Nigel",
            ["ig_old_man1a"] = "OldMan1a",
            ["ig_old_man2"] = "OldMan2",
            ["ig_omega"] = "Omega",
            ["ig_oneil"] = "ONeil",
            ["ig_orleans"] = "Orleans",
            ["ig_ortega"] = "Ortega",
            ["ig_paper"] = "Paper",
            ["ig_patricia"] = "Patricia",
            ["ig_priest"] = "Priest",
            ["ig_prolsec_02"] = "PrologueSec02",
            ["ig_ramp_gang"] = "RampGang",
            ["ig_ramp_hic"] = "RampHic",
            ["ig_ramp_hipster"] = "RampHipster",
            ["ig_ramp_mex"] = "RampMex",
            ["ig_roccopelosi"] = "RoccoPelosi",
            ["ig_russiandrunk"] = "RussianDrunk",
            ["ig_screen_writer"] = "ScreenWriter",
            ["ig_siemonyetarian"] = "SiemonYetarian",
            ["ig_solomon"] = "Solomon",
            ["ig_stevehains"] = "SteveHains",
            ["ig_stretch"] = "Stretch",
            ["ig_talina"] = "Talina",
            ["ig_tanisha"] = "Tanisha",
            ["ig_taocheng"] = "TaoCheng",
            ["ig_taostranslator"] = "TaosTranslator",
            ["ig_tenniscoach"] = "TennisCoach",
            ["ig_terry"] = "Terry",
            ["ig_tomepsilon"] = "TomEpsilon",
            ["ig_tonya"] = "Tonya",
            ["ig_tracydisanto"] = "TracyDisanto",
            ["ig_trafficwarden"] = "TrafficWarden",
            ["ig_tylerdix"] = "TylerDixon",
            ["ig_wade"] = "Wade",
            ["ig_zimbor"] = "Zimbor",
            ["mp_f_deadhooker"] = "DeadHooker",
            ["mp_f_misty_01"] = "Misty01",
            ["mp_f_stripperlite"] = "StripperLite",
            ["mp_g_m_pros_01"] = "MPros01",
            ["mp_m_claude_01"] = "Claude01",
            ["mp_m_exarmy_01"] = "ExArmy01",
            ["mp_m_famdd_01"] = "Famdd01",
            ["mp_m_fibsec_01"] = "FibSec01",
            ["mp_m_marston_01"] = "Marston01",
            ["mp_m_niko_01"] = "Niko01",
            ["mp_m_shopkeep_01"] = "ShopKeep01",
            ["mp_s_m_armoured_01"] = "Armoured01",
            ["s_f_m_fembarber"] = "FemBarberSFM",
            ["s_f_m_maid_01"] = "Maid01SFM",
            ["s_f_m_shop_high"] = "ShopHighSFM",
            ["s_f_m_sweatshop_01"] = "Sweatshop01SFM",
            ["s_f_y_airhostess_01"] = "Airhostess01SFY",
            ["s_f_y_bartender_01"] = "Bartender01SFY",
            ["s_f_y_baywatch_01"] = "Baywatch01SFY",
            ["s_f_y_cop_01"] = "Cop01SFY",
            ["s_f_y_factory_01"] = "Factory01SFY",
            ["s_f_y_hooker_01"] = "Hooker01SFY",
            ["s_f_y_hooker_02"] = "Hooker02SFY",
            ["s_f_y_hooker_03"] = "Hooker03SFY",
            ["s_f_y_migrant_01"] = "Migrant01SFY",
            ["s_f_y_movprem_01"] = "MovPrem01SFY",
            ["s_f_y_ranger_01"] = "Ranger01SFY",
            ["s_f_y_scrubs_01"] = "Scrubs01SFY",
            ["s_f_y_sheriff_01"] = "Sheriff01SFY",
            ["s_f_y_shop_low"] = "ShopLowSFY",
            ["s_f_y_shop_mid"] = "ShopMidSFY",
            ["s_f_y_stripperlite"] = "StripperLiteSFY",
            ["s_f_y_stripper_01"] = "Stripper01SFY",
            ["s_f_y_stripper_02"] = "Stripper02SFY",
            ["s_f_y_sweatshop_01"] = "Sweatshop01SFY",
            ["s_m_m_ammucountry"] = "AmmuCountrySMM",
            ["s_m_m_armoured_01"] = "Armoured01SMM",
            ["s_m_m_armoured_02"] = "Armoured02SMM",
            ["s_m_m_autoshop_01"] = "Autoshop01SMM",
            ["s_m_m_autoshop_02"] = "Autoshop02SMM",
            ["s_m_m_bouncer_01"] = "Bouncer01SMM",
            ["s_m_m_chemsec_01"] = "ChemSec01SMM",
            ["s_m_m_ciasec_01"] = "CiaSec01SMM",
            ["s_m_m_cntrybar_01"] = "Cntrybar01SMM",
            ["s_m_m_dockwork_01"] = "Dockwork01SMM",
            ["s_m_m_doctor_01"] = "Doctor01SMM",
            ["s_m_m_fiboffice_01"] = "FibOffice01SMM",
            ["s_m_m_fiboffice_02"] = "FibOffice02SMM",
            ["s_m_m_gaffer_01"] = "Gaffer01SMM",
            ["s_m_m_gardener_01"] = "Gardener01SMM",
            ["s_m_m_gentransport"] = "GentransportSMM",
            ["s_m_m_hairdress_01"] = "Hairdress01SMM",
            ["s_m_m_highsec_01"] = "Highsec01SMM",
            ["s_m_m_highsec_02"] = "Highsec02SMM",
            ["s_m_m_janitor"] = "JanitorSMM",
            ["s_m_m_lathandy_01"] = "Lathandy01SMM",
            ["s_m_m_lifeinvad_01"] = "Lifeinvad01SMM",
            ["s_m_m_linecook"] = "LinecookSMM",
            ["s_m_m_lsmetro_01"] = "Lsmetro01SMM",
            ["s_m_m_mariachi_01"] = "Mariachi01SMM",
            ["s_m_m_marine_01"] = "Marine01SMM",
            ["s_m_m_marine_02"] = "Marine02SMM",
            ["s_m_m_migrant_01"] = "Migrant01SMM",
            ["s_m_m_movalien_01"] = "MovAlien01",
            ["s_m_m_movprem_01"] = "Movprem01SMM",
            ["s_m_m_movspace_01"] = "Movspace01SMM",
            ["s_m_m_paramedic_01"] = "Paramedic01SMM",
            ["s_m_m_pilot_01"] = "Pilot01SMM",
            ["s_m_m_pilot_02"] = "Pilot02SMM",
            ["s_m_m_postal_01"] = "Postal01SMM",
            ["s_m_m_postal_02"] = "Postal02SMM",
            ["s_m_m_prisguard_01"] = "Prisguard01SMM",
            ["s_m_m_scientist_01"] = "Scientist01SMM",
            ["s_m_m_security_01"] = "Security01SMM",
            ["s_m_m_snowcop_01"] = "Snowcop01SMM",
            ["s_m_m_strperf_01"] = "Strperf01SMM",
            ["s_m_m_strpreach_01"] = "Strpreach01SMM",
            ["s_m_m_strvend_01"] = "Strvend01SMM",
            ["s_m_m_trucker_01"] = "Trucker01SMM",
            ["s_m_m_ups_01"] = "Ups01SMM",
            ["s_m_m_ups_02"] = "Ups02SMM",
            ["s_m_o_busker_01"] = "Busker01SMO",
            ["s_m_y_airworker"] = "AirworkerSMY",
            ["s_m_y_ammucity_01"] = "Ammucity01SMY",
            ["s_m_y_armymech_01"] = "Armymech01SMY",
            ["s_m_y_autopsy_01"] = "Autopsy01SMY",
            ["s_m_y_barman_01"] = "Barman01SMY",
            ["s_m_y_baywatch_01"] = "Baywatch01SMY",
            ["s_m_y_blackops_01"] = "Blackops01SMY",
            ["s_m_y_blackops_02"] = "Blackops02SMY",
            ["s_m_y_busboy_01"] = "Busboy01SMY",
            ["s_m_y_chef_01"] = "Chef01SMY",
            ["s_m_y_clown_01"] = "Clown01SMY",
            ["s_m_y_construct_01"] = "Construct01SMY",
            ["s_m_y_construct_02"] = "Construct02SMY",
            ["s_m_y_cop_01"] = "Cop01SMY",
            ["s_m_y_dealer_01"] = "Dealer01SMY",
            ["s_m_y_devinsec_01"] = "Devinsec01SMY",
            ["s_m_y_dockwork_01"] = "Dockwork01SMY",
            ["s_m_y_doorman_01"] = "Doorman01SMY",
            ["s_m_y_dwservice_01"] = "DwService01SMY",
            ["s_m_y_dwservice_02"] = "DwService02SMY",
            ["s_m_y_factory_01"] = "Factory01SMY",
            ["s_m_y_fireman_01"] = "Fireman01SMY",
            ["s_m_y_garbage"] = "GarbageSMY",
            ["s_m_y_grip_01"] = "Grip01SMY",
            ["s_m_y_hwaycop_01"] = "Hwaycop01SMY",
            ["s_m_y_marine_01"] = "Marine01SMY",
            ["s_m_y_marine_02"] = "Marine02SMY",
            ["s_m_y_marine_03"] = "Marine03SMY",
            ["s_m_y_mime"] = "MimeSMY",
            ["s_m_y_pestcont_01"] = "PestCont01SMY",
            ["s_m_y_pilot_01"] = "Pilot01SMY",
            ["s_m_y_prismuscl_01"] = "PrisMuscl01SMY",
            ["s_m_y_prisoner_01"] = "Prisoner01SMY",
            ["s_m_y_ranger_01"] = "Ranger01SMY",
            ["s_m_y_robber_01"] = "Robber01SMY",
            ["s_m_y_sheriff_01"] = "Sheriff01SMY",
            ["s_m_y_shop_mask"] = "ShopMaskSMY",
            ["s_m_y_strvend_01"] = "Strvend01SMY",
            ["s_m_y_swat_01"] = "Swat01SMY",
            ["s_m_y_uscg_01"] = "Uscg01SMY",
            ["s_m_y_valet_01"] = "Valet01SMY",
            ["s_m_y_waiter_01"] = "Waiter01SMY",
            ["s_m_y_winclean_01"] = "WinClean01SMY",
            ["s_m_y_xmech_01"] = "Xmech01SMY",
            ["s_m_y_xmech_02"] = "Xmech02SMY",
            ["u_f_m_corpse_01"] = "Corpse01",
            ["u_f_m_miranda"] = "Miranda",
            ["u_f_m_promourn_01"] = "PrologueMournFemale01",
            ["u_f_o_moviestar"] = "MovieStar",
            ["u_f_o_prolhost_01"] = "PrologueHostage01",
            ["u_f_y_bikerchic"] = "BikerChic",
            ["u_f_y_comjane"] = "ComJane",
            ["u_f_y_corpse_02"] = "Corpse02",
            ["u_f_y_hotposh_01"] = "Hotposh01",
            ["u_f_y_jewelass_01"] = "Jewelass01",
            ["u_f_y_mistress"] = "Mistress",
            ["u_f_y_poppymich"] = "Poppymich",
            ["u_f_y_princess"] = "Princess",
            ["u_f_y_spyactress"] = "SpyActress",
            ["u_m_m_aldinapoli"] = "AlDiNapoli",
            ["u_m_m_bankman"] = "Bankman01",
            ["u_m_m_bikehire_01"] = "BikeHire01",
            ["u_m_m_fibarchitect"] = "FibArchitect",
            ["u_m_m_filmdirector"] = "FilmDirector",
            ["u_m_m_glenstank_01"] = "Glenstank01",
            ["u_m_m_griff_01"] = "Griff01",
            ["u_m_m_jesus_01"] = "Jesus01",
            ["u_m_m_jewelsec_01"] = "JewelSec01",
            ["u_m_m_jewelthief"] = "JewelThief",
            ["u_m_m_markfost"] = "Markfost",
            ["u_m_m_partytarget"] = "PartyTarget",
            ["u_m_m_prolsec_01"] = "PrologueSec01",
            ["u_m_m_promourn_01"] = "PrologueMournMale01",
            ["u_m_m_rivalpap"] = "RivalPaparazzi",
            ["u_m_m_spyactor"] = "SpyActor",
            ["u_m_m_willyfist"] = "WillyFist",
            ["u_m_o_finguru_01"] = "Finguru01",
            ["u_m_o_taphillbilly"] = "Taphillbilly",
            ["u_m_o_tramp_01"] = "Tramp01",
            ["u_m_y_abner"] = "Abner",
            ["u_m_y_antonb"] = "Antonb",
            ["u_m_y_babyd"] = "Babyd",
            ["u_m_y_baygor"] = "Baygor",
            ["u_m_y_burgerdrug_01"] = "BurgerDrug",
            ["u_m_y_chip"] = "Chip",
            ["u_m_y_cyclist_01"] = "Cyclist01",
            ["u_m_y_fibmugger_01"] = "FibMugger01",
            ["u_m_y_guido_01"] = "Guido01",
            ["u_m_y_gunvend_01"] = "GunVend01",
            ["u_m_y_hippie_01"] = "Hippie01",
            ["u_m_y_imporage"] = "Imporage",
            ["u_m_y_justin"] = "Justin",
            ["u_m_y_mani"] = "Mani",
            ["u_m_y_militarybum"] = "MilitaryBum",
            ["u_m_y_paparazzi"] = "Paparazzi",
            ["u_m_y_party_01"] = "Party01",
            ["u_m_y_pogo_01"] = "Pogo01",
            ["u_m_y_prisoner_01"] = "Prisoner01",
            ["u_m_y_proldriver_01"] = "PrologueDriver",
            ["u_m_y_rsranger_01"] = "RsRanger01AMO",
            ["u_m_y_sbike"] = "SbikeAMO",
            ["u_m_y_staggrm_01"] = "Staggrm01AMO",
            ["u_m_y_tattoo_01"] = "Tattoo01AMO",
            ["u_m_y_zombie_01"] = "Zombie01",
        };


        #endregion

    }
}
