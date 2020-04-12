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
    public class VoiceChat
    {
        // Variables
        private Menu menu;
        public bool EnableVoicechat = UserDefaults.VoiceChatEnabled;
        public bool ShowCurrentSpeaker = UserDefaults.ShowCurrentSpeaker;
        public bool ShowVoiceStatus = UserDefaults.ShowVoiceStatus;
        public float currentProximity = UserDefaults.VoiceChatProximity;
        public List<string> channels = new List<string>()
        {
            "頻道 1 (預設)",
            "頻道 2",
            "頻道 3",
            "頻道 4",
        };
        public string currentChannel;
        private List<float> proximityRange = new List<float>()
        {
            5f, // 5m
            10f, // 10m
            15f, // 15m
            20f, // 20m
            100f, // 100m
            300f, // 300m
            1000f, // 1.000m
            2000f, // 2.000m
            0f, // global
        };


        private void CreateMenu()
        {
            currentChannel = channels[0];
            if (IsAllowed(Permission.VCStaffChannel))
            {
                channels.Add("Staff Channel");
            }

            // Create the menu.
            menu = new Menu(Game.Player.Name, "語音聊天設置");

            MenuCheckboxItem voiceChatEnabled = new MenuCheckboxItem("啟用語音聊天", "啟用/停用語音聊天.", EnableVoicechat);
            MenuCheckboxItem showCurrentSpeaker = new MenuCheckboxItem("顯示當前發言人", "顯示目前誰正在說話.", ShowCurrentSpeaker);
            MenuCheckboxItem showVoiceStatus = new MenuCheckboxItem("顯示麥克風狀態", "顯示麥克風目前是打開還是靜音.", ShowVoiceStatus);

            List<string> proximity = new List<string>()
            {
                "5 米",
                "10 米",
                "15 米",
                "20 米",
                "100 米",
                "300 米",
                "1 公里",
                "2 公里",
                "全域",
            };
            MenuListItem voiceChatProximity = new MenuListItem("語音聊天距離", proximity, proximityRange.IndexOf(currentProximity), "將語音聊天接收距離設置.");
            MenuListItem voiceChatChannel = new MenuListItem("語音聊天頻道", channels, channels.IndexOf(currentChannel), "設置語音聊天頻道.");

            if (IsAllowed(Permission.VCEnable))
            {
                menu.AddMenuItem(voiceChatEnabled);

                // Nested permissions because without voice chat enabled, you wouldn't be able to use these settings anyway.
                if (IsAllowed(Permission.VCShowSpeaker))
                {
                    menu.AddMenuItem(showCurrentSpeaker);
                }

                menu.AddMenuItem(voiceChatProximity);
                menu.AddMenuItem(voiceChatChannel);
                menu.AddMenuItem(showVoiceStatus);
            }

            menu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == voiceChatEnabled)
                {
                    EnableVoicechat = _checked;
                }
                else if (item == showCurrentSpeaker)
                {
                    ShowCurrentSpeaker = _checked;
                }
                else if (item == showVoiceStatus)
                {
                    ShowVoiceStatus = _checked;
                }
            };

            menu.OnListIndexChange += (sender, item, oldIndex, newIndex, itemIndex) =>
            {
                if (item == voiceChatProximity)
                {
                    currentProximity = proximityRange[newIndex];
                    Subtitle.Custom($"新的語音聊天接近度設置為： ~b~{proximity[newIndex]}~s~.");
                }
                else if (item == voiceChatChannel)
                {
                    currentChannel = channels[newIndex];
                    Subtitle.Custom($"新的語音聊天頻道設置為: ~b~{channels[newIndex]}~s~.");
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
