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
    public class BannedPlayers
    {
        // Variables
        private Menu menu;

        /// <summary>
        /// Struct used to store bans.
        /// </summary>
        public struct BanRecord
        {
            public string playerName;
            public List<string> identifiers;
            public DateTime bannedUntil;
            public string banReason;
            public string bannedBy;
        }

        BanRecord currentRecord = new BanRecord();

        public List<BanRecord> banlist = new List<BanRecord>();

        Menu bannedPlayer = new Menu("封鎖玩家", "封鎖理由: ");

        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            menu = new Menu(Game.Player.Name, "封鎖玩家管理");

            menu.InstructionalButtons.Add(Control.Jump, "篩選選項");
            menu.ButtonPressHandlers.Add(new Menu.ButtonPressHandler(Control.Jump, Menu.ControlPressCheckType.JUST_RELEASED, new Action<Menu, Control>(async (a, b) =>
            {
                if (banlist.Count > 1)
                {
                    string filterText = await GetUserInput("篩選列表（按用戶名排序）（將此保留為空以重置過濾器！）");
                    if (string.IsNullOrEmpty(filterText))
                    {
                        Subtitle.Custom("篩選列表已清除.");
                        menu.ResetFilter();
                        UpdateBans();
                    }
                    else
                    {
                        menu.FilterMenuItems(item => item.ItemData is BanRecord br && br.playerName.ToLower().Contains(filterText.ToLower()));
                        Subtitle.Custom("用戶名過濾已套用.");
                    }
                }
                else
                {
                    Notify.Error("要使用過濾功能，至少需要兩位以上的封鎖玩家");
                }

                Log($"Button pressed: {a} {b}");
            }), true));

            bannedPlayer.AddMenuItem(new MenuItem("玩家名字"));
            bannedPlayer.AddMenuItem(new MenuItem("封鎖人"));
            bannedPlayer.AddMenuItem(new MenuItem("解封時間"));
            bannedPlayer.AddMenuItem(new MenuItem("玩家識別碼"));
            bannedPlayer.AddMenuItem(new MenuItem("封鎖理由"));
            bannedPlayer.AddMenuItem(new MenuItem("~r~解封", "~r~警告，禁止玩家無法撤消。 在它們重新加入服務器之前，您將無法再次禁止它們。 您確定要取消該此玩家的封鎖嗎？ 〜s〜提示：如果被禁的玩家在封鎖日期結束後他們仍可以進入伺服器."));

            // should be enough for now to cover all possible identifiers.
            List<string> colors = new List<string>() { "~r~", "~g~", "~b~", "~o~", "~y~", "~p~", "~s~", "~t~", };

            bannedPlayer.OnMenuClose += (sender) =>
            {
                BaseScript.TriggerServerEvent("vMenu:RequestBanList", Game.Player.Handle);
                bannedPlayer.GetMenuItems()[5].Label = "";
                UpdateBans();
            };

            bannedPlayer.OnIndexChange += (sender, oldItem, newItem, oldIndex, newIndex) =>
            {
                bannedPlayer.GetMenuItems()[5].Label = "";
            };

            bannedPlayer.OnItemSelect += (sender, item, index) =>
            {
                if (index == 5 && IsAllowed(Permission.OPUnban))
                {
                    if (item.Label == "您確定嗎?")
                    {
                        if (banlist.Contains(currentRecord))
                        {
                            UnbanPlayer(banlist.IndexOf(currentRecord));
                            bannedPlayer.GetMenuItems()[5].Label = "";
                            bannedPlayer.GoBack();
                        }
                        else
                        {
                            Notify.Error("您設法以某種方式單擊了取消禁止按鈕，但是您顯然正在查看的禁止記錄甚至不存在...");
                        }
                    }
                    else
                    {
                        item.Label = "您確定嗎?";
                    }
                }
                else
                {
                    bannedPlayer.GetMenuItems()[5].Label = "";
                }

            };

            menu.OnItemSelect += (sender, item, index) =>
            {
                //if (index < banlist.Count)
                //{
                currentRecord = item.ItemData;

                bannedPlayer.MenuSubtitle = "封鎖理由: ~y~" + currentRecord.playerName;
                var nameItem = bannedPlayer.GetMenuItems()[0];
                var bannedByItem = bannedPlayer.GetMenuItems()[1];
                var bannedUntilItem = bannedPlayer.GetMenuItems()[2];
                var playerIdentifiersItem = bannedPlayer.GetMenuItems()[3];
                var banReasonItem = bannedPlayer.GetMenuItems()[4];
                nameItem.Label = currentRecord.playerName;
                nameItem.Description = "玩家名字: ~y~" + currentRecord.playerName;
                bannedByItem.Label = currentRecord.bannedBy;
                bannedByItem.Description = "被 ~y~" + currentRecord.bannedBy + "封鎖";
                if (currentRecord.bannedUntil.Date.Year == 3000)
                    bannedUntilItem.Label = "永遠";
                else
                    bannedUntilItem.Label = currentRecord.bannedUntil.Date.ToString();
                bannedUntilItem.Description = "這個玩家將再: " + currentRecord.bannedUntil.Date.ToString() + "後解鎖";
                playerIdentifiersItem.Description = "";

                int i = 0;
                foreach (string id in currentRecord.identifiers)
                {
                    // only (admins) people that can unban players are allowed to view IP's.
                    // this is just a slight 'safety' feature in case someone who doesn't know what they're doing
                    // gave builtin.everyone access to view the banlist.
                    if (id.StartsWith("ip:") && !IsAllowed(Permission.OPUnban))
                    {
                        playerIdentifiersItem.Description += $"{colors[i]}ip: (hidden) ";
                    }
                    else
                    {
                        playerIdentifiersItem.Description += $"{colors[i]}{id.Replace(":", ": ")} ";
                    }
                    i++;
                }
                banReasonItem.Description = "封鎖理由: " + currentRecord.banReason;

                var unbanPlayerBtn = bannedPlayer.GetMenuItems()[5];
                unbanPlayerBtn.Label = "";
                if (!IsAllowed(Permission.OPUnban))
                {
                    unbanPlayerBtn.Enabled = false;
                    unbanPlayerBtn.Description = "您不能取消玩家封鎖。 您只能查看其封鎖記錄.";
                    unbanPlayerBtn.LeftIcon = MenuItem.Icon.LOCK;
                }

                bannedPlayer.RefreshIndex();
                //}
            };
            MenuController.AddMenu(bannedPlayer);

        }

        /// <summary>
        /// Updates the ban list menu.
        /// </summary>
        public void UpdateBans()
        {
            menu.ResetFilter();
            menu.ClearMenuItems();

            foreach (BanRecord ban in banlist)
            {
                MenuItem recordBtn = new MenuItem(ban.playerName, $"~y~{ban.playerName}~s~ 被 ~y~{ban.bannedBy}~s~ 封鎖直到 ~y~{ban.bannedUntil}~s~ 封鎖理由是 ~y~{ban.banReason}~s~.")
                {
                    Label = "→→→",
                    ItemData = ban
                };
                menu.AddMenuItem(recordBtn);
                MenuController.BindMenuItem(menu, bannedPlayer, recordBtn);
            }
            menu.RefreshIndex();
        }

        /// <summary>
        /// Updates the list of ban records.
        /// </summary>
        /// <param name="banJsonString"></param>
        public void UpdateBanList(string banJsonString)
        {
            banlist.Clear();
            dynamic obj = JsonConvert.DeserializeObject(banJsonString);
            foreach (dynamic br in obj)
            {
                BanRecord b = JsonToBanRecord(br);
                banlist.Add(b);
            }
            UpdateBans();
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

        /// <summary>
        /// Sends an event to the server requesting the player to be unbanned.
        /// We'll just assume that worked fine, so remove the item from our local list, we'll re-sync once the menu is re-opened.
        /// </summary>
        /// <param name="index"></param>
        private void UnbanPlayer(int index)
        {
            BanRecord record = banlist[index];
            banlist.Remove(record);
            BaseScript.TriggerServerEvent("vMenu:RequestPlayerUnban", JsonConvert.SerializeObject(record));
        }

        /// <summary>
        /// Converts the ban record (json object) into a BanRecord struct.
        /// </summary>
        /// <param name="banRecordJsonObject"></param>
        /// <returns></returns>
        public static BanRecord JsonToBanRecord(dynamic banRecordJsonObject)
        {
            var newBr = new BanRecord();
            foreach (Newtonsoft.Json.Linq.JProperty brValue in banRecordJsonObject)
            {
                string key = brValue.Name.ToString();
                var value = brValue.Value;
                if (key == "playerName")
                {
                    newBr.playerName = value.ToString();
                }
                else if (key == "identifiers")
                {
                    var tmpList = new List<string>();
                    foreach (string identifier in value)
                    {
                        tmpList.Add(identifier);
                    }
                    newBr.identifiers = tmpList;
                }
                else if (key == "bannedUntil")
                {
                    newBr.bannedUntil = DateTime.Parse(value.ToString());
                }
                else if (key == "banReason")
                {
                    newBr.banReason = value.ToString();
                }
                else if (key == "bannedBy")
                {
                    newBr.bannedBy = value.ToString();
                }
            }
            return newBr;
        }
    }
}
