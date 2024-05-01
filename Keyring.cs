using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using JetBrains.Annotations;
using AzuExtendedPlayerInventory;

namespace Stollie.Keychain
{
    [BepInPlugin(PluginId, "Keyring", "1.0.0")]
    [BepInDependency("Azumatt.AzuExtendedPlayerInventory", BepInDependency.DependencyFlags.HardDependency)]
    public class MyMod : BaseUnityPlugin
    {
        public const string PluginId = "stollie.mods.keyring";
        private Harmony _harmony;
        private static MyMod _instance;
        private static ConfigEntry<bool> _loggingEnabled;
        private const string CryptKeySlotName = "Crypt";
        private const string CryptKeyPrefabName = "CryptKey";

        [UsedImplicitly]
        private void Awake()
        {
            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PluginId);
            _instance = this;

            _loggingEnabled = Config.Bind("Logging", "Logging Enabled", true, "Enable logging");

            API.OnHudAwake += (hud) => {  };
            API.SlotAdded += (slotName) => {  };

            bool isAPILoaded = API.IsLoaded();
            if (API.IsLoaded())
            {
                // Add a custom slot for the Crypt Key item
                int desiredCrpytKeyPosition = SetSlotPositionWithGapAfterExistingSlots(0);
                bool cryptKeySlotAddedSuccess = API.AddSlot(CryptKeySlotName, GetCryptKeyItem, CanPlaceCryptKey, desiredCrpytKeyPosition);
                if (!cryptKeySlotAddedSuccess)
                {

                    LogError("Failed to add custom slot for Crypt Key item.");
                }
                else
                    Log($"Successfully added Crypt Key slot to Slot# {desiredCrpytKeyPosition}");

            }
        }

        // Function to find the position for the new slot
        private int SetSlotPositionWithGapAfterExistingSlots(int slotGap)
        {
            // Get all current slots
            SlotInfo slots = API.GetSlots();
            Log($"Current Slot count is {slots.SlotNames.Length}");
            return slots.SlotNames.Length + slotGap;
        }

        // Function to get the Crypt Key item from the player's inventory
        private ItemDrop.ItemData GetCryptKeyItem(Player player)
        {
            // Check player's inventory for Crypt Key item
            foreach (ItemDrop.ItemData item in player.GetInventory().GetAllItems())
            {
                if (item.m_dropPrefab.name == CryptKeyPrefabName)
                {
                    return item;
                }
            }
            return null; // Crypt Key not found in inventory
        }

        // Function to check if the Crypt Key item can be placed in the custom slot
        private bool CanPlaceCryptKey(ItemDrop.ItemData item)
        {
            if (item.m_dropPrefab.name != CryptKeyPrefabName)
                LogWarning("Tried to place non Crypt Key item in slot");
            else
                Log("Placed Crypt Key in slot");

            return item.m_dropPrefab.name == CryptKeyPrefabName; // Only allow Crypt Key item to be placed
        }

        #region Logging
        public static void Log(string message)
        {
            if (_loggingEnabled.Value)
                _instance.Logger.LogInfo($"KEYCHAIN LOG: {message}");
        }

        public static void LogWarning(string message)
        {
            if (_loggingEnabled.Value)
                _instance.Logger.LogWarning($"KEYCHAIN LOG: {message}");
        }

        public static void LogError(string message)
        {
            if (_loggingEnabled.Value)
                _instance.Logger.LogError($"KEYCHAIN LOG: {message}");
        }
        #endregion

        [UsedImplicitly]
        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
            _instance = null;
        }
    }
}



