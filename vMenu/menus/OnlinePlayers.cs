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
    public class OnlinePlayers
    {
        public List<int> PlayersWaypointList = new List<int>();

        // Menu variable, will be defined in CreateMenu()
        private Menu menu;

        Menu playerMenu = new Menu("線上玩家", "玩家:");
        Player currentPlayer = new Player(Game.Player.Handle);


        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            // Create the menu.
            menu = new Menu(Game.Player.Name, "線上玩家") { };
            menu.CounterPreText = "玩家: ";

            MenuController.AddSubmenu(menu, playerMenu);

            MenuItem sendMessage = new MenuItem("發送私人訊息", "發送私人訊息給這個玩家. ~r~注意: 其他管理員可能可以看到所有訊息.");
            MenuItem teleport = new MenuItem("傳送到該玩家身邊", "傳送到該玩家身邊");
            MenuItem teleportVeh = new MenuItem("搭著載具傳送到玩家身旁", "連同載具一起傳送過去.");
            MenuItem summon = new MenuItem("召喚玩家", "把該玩家拉過來你身邊.");
            MenuItem toggleGPS = new MenuItem("切換GPS", "啟用或禁用切換GPS 可以開關該玩家的GPS路線。");
            MenuItem spectate = new MenuItem("觀察玩家", "觀察該玩家。再按一次選項可以停止觀看.");
            MenuItem printIdentifiers = new MenuItem("顯示識別碼", "使用該功能，會將該玩家的識別碼顯示再您的F8控制台上，且儲存到伺服器log檔案中.");
            MenuItem kill = new MenuItem("~r~殺死玩家", "K殺死該玩家，請注意他們會收到一條通知，說您殺死了他們。 且該功能還會將記錄在管理員使用指令的log中。");
            MenuItem kick = new MenuItem("~r~踢出玩家", "把該玩家踢出伺服器.");
            MenuItem ban = new MenuItem("~r~永久封鎖玩家", "對該玩家永久封鎖. 您可以再點擊一次來對該玩家的封鎖天數、原因進行修改");
            MenuItem tempban = new MenuItem("~r~暫時封鎖玩家", "對該玩家暫時封鎖(最多30天). 您可以再點擊一次來對該玩家的封鎖天數、原因進行修改");

            // always allowed
            playerMenu.AddMenuItem(sendMessage);
            // permissions specific
            if (IsAllowed(Permission.OPTeleport))
            {
                playerMenu.AddMenuItem(teleport);
                playerMenu.AddMenuItem(teleportVeh);
            }
            if (IsAllowed(Permission.OPSummon))
            {
                playerMenu.AddMenuItem(summon);
            }
            if (IsAllowed(Permission.OPSpectate))
            {
                playerMenu.AddMenuItem(spectate);
            }
            if (IsAllowed(Permission.OPWaypoint))
            {
                playerMenu.AddMenuItem(toggleGPS);
            }
            if (IsAllowed(Permission.OPIdentifiers))
            {
                playerMenu.AddMenuItem(printIdentifiers);
            }
            if (IsAllowed(Permission.OPKill))
            {
                playerMenu.AddMenuItem(kill);
            }
            if (IsAllowed(Permission.OPKick))
            {
                playerMenu.AddMenuItem(kick);
            }
            if (IsAllowed(Permission.OPTempBan))
            {
                playerMenu.AddMenuItem(tempban);
            }
            if (IsAllowed(Permission.OPPermBan))
            {
                playerMenu.AddMenuItem(ban);
                ban.LeftIcon = MenuItem.Icon.WARNING;
            }

            playerMenu.OnMenuClose += (sender) =>
            {
                playerMenu.RefreshIndex();
                ban.Label = "";
            };

            playerMenu.OnIndexChange += (sender, oldItem, newItem, oldIndex, newIndex) =>
            {
                ban.Label = "";
            };

            // handle button presses for the specific player's menu.
            playerMenu.OnItemSelect += async (sender, item, index) =>
            {
                // send message
                if (item == sendMessage)
                {
                    if (MainMenu.MiscSettingsMenu != null && !MainMenu.MiscSettingsMenu.MiscDisablePrivateMessages)
                    {
                        string message = await GetUserInput($"發送私人訊息給 {currentPlayer.Name}", 200);
                        if (string.IsNullOrEmpty(message))
                        {
                            Notify.Error(CommonErrors.InvalidInput);
                        }
                        else
                        {
                            TriggerServerEvent("vMenu:SendMessageToPlayer", currentPlayer.ServerId, message);
                            PrivateMessage(currentPlayer.ServerId.ToString(), message, true);
                        }
                    }
                    else
                    {
                        Notify.Error("如果您自己禁用了私人消息，則無法發送私人消息。 在“其他設置”選單中啟用它們，然後重試.");
                    }



                }
                // teleport (in vehicle) button
                else if (item == teleport || item == teleportVeh)
                {
                    if (Game.Player.Handle != currentPlayer.Handle)
                        TeleportToPlayer(currentPlayer.Handle, item == teleportVeh); // teleport to the player. optionally in the player's vehicle if that button was pressed.
                    else
                        Notify.Error("您不能傳送到自己旁邊!");
                }
                // summon button
                else if (item == summon)
                {
                    if (Game.Player.Handle != currentPlayer.Handle)
                        SummonPlayer(currentPlayer);
                    else
                        Notify.Error("您不能召喚您自己");
                }
                // spectating
                else if (item == spectate)
                {
                    SpectatePlayer(currentPlayer);
                }
                // kill button
                else if (item == kill)
                {
                    KillPlayer(currentPlayer);
                }
                // manage the gps route being clicked.
                else if (item == toggleGPS)
                {
                    bool selectedPedRouteAlreadyActive = false;
                    if (PlayersWaypointList.Count > 0)
                    {
                        if (PlayersWaypointList.Contains(currentPlayer.Handle))
                        {
                            selectedPedRouteAlreadyActive = true;
                        }
                        foreach (int playerId in PlayersWaypointList)
                        {
                            int playerPed = GetPlayerPed(playerId);
                            if (DoesEntityExist(playerPed) && DoesBlipExist(GetBlipFromEntity(playerPed)))
                            {
                                int oldBlip = GetBlipFromEntity(playerPed);
                                SetBlipRoute(oldBlip, false);
                                RemoveBlip(ref oldBlip);
                                Notify.Custom($"~g~GPS路線到 ~s~{GetSafePlayerName(currentPlayer.Name)}~g~ 現在是非激活狀態.");
                            }
                        }
                        PlayersWaypointList.Clear();
                    }

                    if (!selectedPedRouteAlreadyActive)
                    {
                        if (currentPlayer.Handle != Game.Player.Handle)
                        {
                            int ped = GetPlayerPed(currentPlayer.Handle);
                            int blip = GetBlipFromEntity(ped);
                            if (DoesBlipExist(blip))
                            {
                                SetBlipColour(blip, 58);
                                SetBlipRouteColour(blip, 58);
                                SetBlipRoute(blip, true);
                            }
                            else
                            {
                                blip = AddBlipForEntity(ped);
                                SetBlipColour(blip, 58);
                                SetBlipRouteColour(blip, 58);
                                SetBlipRoute(blip, true);
                            }
                            PlayersWaypointList.Add(currentPlayer.Handle);
                            Notify.Custom($"~g~GSP路線 ~s~{GetSafePlayerName(currentPlayer.Name)}~g~ 現在是激活狀態, 按下 ~s~GPS路線~g~ 可以用來關閉GPS路線.");
                        }
                        else
                        {
                            Notify.Error("您不能對自己使用該功能.");
                        }
                    }
                }
                else if (item == printIdentifiers)
                {
                    Func<string, string> CallbackFunction = (data) =>
                    {
                        Debug.WriteLine(data);
                        string ids = "~s~";
                        foreach (string s in JsonConvert.DeserializeObject<string[]>(data))
                        {
                            ids += "~n~" + s;
                        }
                        Notify.Custom($"~y~{GetSafePlayerName(currentPlayer.Name)}~g~'s 識別碼: {ids}", false);
                        return data;
                    };
                    BaseScript.TriggerServerEvent("vMenu:GetPlayerIdentifiers", currentPlayer.ServerId, CallbackFunction);
                }
                // kick button
                else if (item == kick)
                {
                    if (currentPlayer.Handle != Game.Player.Handle)
                        KickPlayer(currentPlayer, true);
                    else
                        Notify.Error("您不能踢你除你自己!");
                }
                // temp ban
                else if (item == tempban)
                {
                    BanPlayer(currentPlayer, false);
                }
                // perm ban
                else if (item == ban)
                {
                    if (ban.Label == "您確定嗎?")
                    {
                        ban.Label = "";
                        UpdatePlayerlist();
                        playerMenu.GoBack();
                        BanPlayer(currentPlayer, true);
                    }
                    else
                    {
                        ban.Label = "您確定嗎?";
                    }
                }
            };

            // handle button presses in the player list.
            menu.OnItemSelect += (sender, item, index) =>
                {
                    if (MainMenu.PlayersList.ToList().Any(p => p.ServerId.ToString() == item.Label.Replace(" →→→", "").Replace("Server #", "")))
                    {
                        currentPlayer = MainMenu.PlayersList.ToList().Find(p => p.ServerId.ToString() == item.Label.Replace(" →→→", "").Replace("Server #", ""));
                        playerMenu.MenuSubtitle = $"~s~玩家: ~y~{GetSafePlayerName(currentPlayer.Name)}";
                        playerMenu.CounterPreText = $"[伺服器 ID: ~y~{currentPlayer.ServerId}~s~] ";
                    }
                    else
                    {
                        playerMenu.GoBack();
                    }
                };
        }

        /// <summary>
        /// Updates the player items.
        /// </summary>
        public void UpdatePlayerlist()
        {
            menu.ClearMenuItems();

            foreach (Player p in MainMenu.PlayersList)
            {
                MenuItem pItem = new MenuItem($"{GetSafePlayerName(p.Name)}", $"點擊來查看可對該玩家的選項. 伺服器 ID: {p.ServerId}. 本地 ID: {p.Handle}.")
                {
                    Label = $"Server #{p.ServerId} →→→"
                };
                menu.AddMenuItem(pItem);
                MenuController.BindMenuItem(menu, playerMenu, pItem);
            }

            menu.RefreshIndex();
            //menu.UpdateScaleform();
            playerMenu.RefreshIndex();
            //playerMenu.UpdateScaleform();
        }

        /// <summary>
        /// Checks if the menu exists, if not then it creates it first.
        /// Then returns the menu.
        /// </summary>
        /// <returns>The Online Players Menu</returns>
        public Menu GetMenu()
        {
            if (menu == null)
            {
                CreateMenu();
                return menu;
            }
            else
            {
                UpdatePlayerlist();
                return menu;
            }
        }
    }
}
