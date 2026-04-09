using HarmonyLib;
using Il2Cpp;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Character
{
    [RegisterTypeInIl2Cpp]
    public class Character_Blessings : MonoBehaviour
    {
        public static Character_Blessings instance { get; private set; }
        public Character_Blessings(System.IntPtr ptr) : base(ptr) { }

        private readonly int base_id = 34;
        private readonly int base_container = 33;
        private InventoryBlessingSlotUI selected_active_slot = null;
        private InventoryBlessingSlotUI selected_discovered_slot = null;

        void Awake()
        {
            instance = this;
        }
        void Update()
        {
            if (IsBlessingOpen())
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    if (temp_selected_active_slot != null) { selected_active_slot = temp_selected_active_slot; }
                    else if ((temp_selected_discovered_slot != null) && (selected_active_slot != null))
                    {
                        selected_discovered_slot = temp_selected_discovered_slot;
                        int blessing_id = selected_discovered_slot.referenceBlessingID;
                        selected_discovered_slot = null;
                        ItemDataUnpacked item = CreateBlessing(blessing_id);
                        if ((timeline_id > -1) && (IsBlessingDiscovered(blessing_id)) && (!Refs_Manager.player_data_tracker.IsNullOrDestroyed()) && (item != null)) //&& (!active_blessing_slot.lockedSlot.gameObject.active)
                        {
                            bool found = false;
                            ushort container_id = (ushort)(timeline_id + base_container);
                            foreach (Il2CppLE.Data.ItemLocationPair item_pair in Refs_Manager.player_data_tracker.charData.SavedItems)
                            {
                                if (item_pair.ContainerID == container_id)
                                {
                                    if (item_pair.Data.Count > 7)
                                    {
                                        if (item_pair.Data[1] == 34)
                                        {
                                            item_pair.Data[2] = (byte)blessing_id;
                                            item_pair.Data[5] = item.implicitRolls[0];
                                            item_pair.Data[6] = item.implicitRolls[1];
                                            item_pair.Data[7] = item.implicitRolls[2];
                                            found = true;
                                            break;
                                        }
                                        else { Main.logger_instance?.Msg("Not a Blessing"); }
                                        break;
                                    }
                                }
                            }
                            if (!found) { Refs_Manager.player_data_tracker.charData.SavedItems.Add(CreateBlessingData(item, container_id)); }
                            Refs_Manager.player_data_tracker.charData.SaveData();
                        }
                        if (selected_active_slot.lockedSlot.gameObject.active) { selected_active_slot.lockedSlot.gameObject.active = false; }
                        OneSlotItemContainer one_slot_container = selected_active_slot.blessingUIContainer.container.TryCast<OneSlotItemContainer>();
                        if (!one_slot_container.IsNullOrDestroyed())
                        {
                            one_slot_container.Clear();
                            one_slot_container.TryAddItem(item, 1, Context.DEFAULT);
                        }
                        selected_active_slot.blessingUIContainer.forceUpdate = true;
                    }
                }
            }
            else
            {
                timeline_id = -1;
                selected_active_slot = null;
                selected_discovered_slot = null;
            }
        }
        ItemDataUnpacked CreateBlessing(int blessing_id)
        {
            ItemDataUnpacked item = new ItemDataUnpacked
            {
                LvlReq = 0,
                classReq = ItemList.ClassRequirement.Any,
                itemType = (byte)base_id,
                subType = (ushort)blessing_id,
                rarity = (byte)0,
                sockets = (byte)0,
                uniqueID = (ushort)0
            };
            item.implicitRolls = new byte[] { 255, 255, 255 };
            item.RefreshIDAndValues();

            return item;
        }
        Il2CppLE.Data.ItemLocationPair CreateBlessingData(ItemDataUnpacked item, ushort container_id)
        {
            Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte> Data = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte>(11);
            Data[0] = 2;
            Data[1] = item.itemType;
            Data[2] = (byte)item.subType;
            Data[3] = 0;
            Data[4] = 0;
            Data[5] = item.implicitRolls[0];
            Data[6] = item.implicitRolls[1];
            Data[7] = item.implicitRolls[2];
            Data[8] = 0;
            Data[9] = 0;
            Data[10] = 0;

            Il2CppLE.Data.ItemLocationPair new_blessing = new Il2CppLE.Data.ItemLocationPair
            {
                ContainerID = container_id,
                Data = Data,
                FormatVersion = 2,
                InventoryPosition = new Il2CppLE.Data.ItemInventoryPosition(0, 0),
                Quantity = 1,
                TabID = 0
            };

            return new_blessing;
        }
        public static Il2CppLE.Data.BlessingData CreateBlessingDataForSave(ushort subtype)
        {
            Il2CppLE.Data.BlessingData result = new Il2CppLE.Data.BlessingData
            {
                SubtypeId = subtype,
                ImplicitRollByte0 = 255,
                ImplicitRollByte1 = 255,
                ImplicitRollByte2 = 255
            };

            return result;
        }

        bool IsBlessingDiscovered(int id)
        {
            bool result = false;
            if (!Refs_Manager.player_data_tracker.IsNullOrDestroyed())
            {
                foreach (int discovered_id in Refs_Manager.player_data_tracker.charData.BlessingsDiscovered)
                {
                    if (id == discovered_id) { result = true; break; }
                }
            }

            return result;
        }

        private static int timeline_id = -1;
        private static bool adding_blessings = false;
        private static InventoryBlessingSlotUI temp_selected_active_slot = null;
        private static InventoryBlessingSlotUI temp_selected_discovered_slot = null;

        private static bool CanRun()
        {
            if ((Scenes.IsGameScene()) && (!Save_Manager.instance.IsNullOrDestroyed()) && (IsBlessingOpen()))
            {
                if (!Save_Manager.instance.data.IsNullOrDestroyed())
                {
                    return Save_Manager.instance.data.Character.Cheats.Enable_CanChooseBlessing;
                }
                else { return false; }
            }
            else { return false; }
        }
        public static bool IsBlessingOpen()
        {
            bool result = false;
            if (!Refs_Manager.BlessingsPanel.IsNullOrDestroyed()) //Don't use .net6 nullable here
            {
                result = Refs_Manager.BlessingsPanel.active;
            }
            return result;
        }
        public static void DiscoverAllBlessings()
        {
            if (!adding_blessings)
            {
                adding_blessings = true;

                Hud_Manager.Hud_Base.Resume_Click(); //Close Hud

                //Unlock all timelines
                if (!ItemContainersManager.Instance.IsNullOrDestroyed())
                {
                    System.Collections.Generic.List<TimelineID> timelines_id = new System.Collections.Generic.List<TimelineID>();
                    timelines_id.Add(TimelineID.UndeadAbom);
                    timelines_id.Add(TimelineID.OsprixWithLance);
                    timelines_id.Add(TimelineID.VoidRahyeh);
                    timelines_id.Add(TimelineID.FrostLich);
                    timelines_id.Add(TimelineID.Lagon);
                    timelines_id.Add(TimelineID.UndeadVsVoid);
                    timelines_id.Add(TimelineID.Dragons);
                    timelines_id.Add(TimelineID.Gaspar);
                    timelines_id.Add(TimelineID.Heorot);
                    timelines_id.Add(TimelineID.Volcano);

                    if (!BlessingRewardPanelManager.instance.IsNullOrDestroyed())
                    {
                        GameObject ui = BlessingRewardPanelManager.instance.gameObject;
                        ui.active = true;
                        foreach (TimelineID t_id in timelines_id)
                        {
                            ItemContainersManager.Instance.populateBlessingOptions(t_id, 0, 3, 2);
                            BlessingRewardPanelManager.instance._selectedOption = 1;
                            BlessingRewardPanelManager.instance.ConfirmSelection();
                        }
                        ui.active = false;
                    }
                    else { Main.logger_instance?.Error("BlessingRewardPanelManager.instance is null"); }
                }

                //Add all blessings
                if (!Refs_Manager.item_list.IsNullOrDestroyed())
                {
                    int base_id = 34;
                    int index = 0;
                    bool found = false;
                    foreach (ItemList.BaseEquipmentItem n_item in Refs_Manager.item_list.EquippableItems)
                    {
                        if (n_item.baseTypeID == base_id) { found = true; break; }
                        index++;
                    }
                    if ((found) && (!Refs_Manager.player_data_tracker.IsNullOrDestroyed()))
                    {
                        // Clear existing blssings to replace them
                        Refs_Manager.player_data_tracker.charData.BlessingsDiscovered.Clear();
                        Refs_Manager.player_data_tracker.charData.OpenBlessings.Clear();

                        List<int> blessingSubtypes = new();

                        foreach (ItemList.EquipmentItem item in Refs_Manager.item_list.EquippableItems[index].subItems)
                        {
                            // Collect all blessing subtypes
                            blessingSubtypes.Add(item.subTypeID);

                            // Add blessings
                            Refs_Manager.player_data_tracker.charData.BlessingsDiscovered.Add(item.subTypeID);
                            Refs_Manager.player_data_tracker.charData.OpenBlessings.Add(CreateBlessingDataForSave(System.Convert.ToUInt16(item.subTypeID)));
                        }

                        // Save blessings
                        Refs_Manager.player_data_tracker.charData.ReplaceBlessingsDiscovered(blessingSubtypes);
                        Refs_Manager.player_data_tracker.charData.SaveUnlockedBlessings(Refs_Manager.player_data_tracker.charData.GetOpenBlessingsAsItems());
                        Refs_Manager.player_data_tracker.charData.SaveData();
                    }
                    else { Main.logger_instance?.Error("Blessings not found in itemlist"); }
                }
                else { Main.logger_instance?.Error("ItemList is null"); }

                adding_blessings = false;
            }
        }

        [HarmonyPatch(typeof(InventoryPanelUI), "SelectTimelineForBlessingDisplay")]
        public class InventoryPanelUI_SelectTimelineForBlessingDisplay
        {
            [HarmonyPrefix]
            static void Postfix(int __0)
            {
                timeline_id = -1;
                if (CanRun())
                {
                    timeline_id = __0;
                }
            }
        }

        [HarmonyPatch(typeof(InventoryBlessingSlotUI), "UnityEngine_EventSystems_IPointerEnterHandler_OnPointerEnter")]
        public class InventoryBlessingSlotUI_UnityEngine_EventSystems_IPointerEnterHandler_OnPointerEnter
        {
            [HarmonyPrefix]
            static void Postfix(ref InventoryBlessingSlotUI __instance)
            {
                temp_selected_discovered_slot = null;
                temp_selected_active_slot = null;
                if (CanRun())
                {
                    string slot_name = __instance.gameObject.name;
                    if (slot_name.Contains("BlessingInventoryDisplayButton"))
                    {
                        temp_selected_discovered_slot = __instance;
                    }
                    else if ((slot_name.Contains("Blessing")) && (!slot_name.Contains("Inventory")))
                    {
                        temp_selected_active_slot = __instance;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(InventoryBlessingSlotUI), "UnityEngine_EventSystems_IPointerExitHandler_OnPointerExit")]
        public class InventoryBlessingSlotUI_UnityEngine_EventSystems_IPointerExitHandler_OnPointerExit
        {
            [HarmonyPrefix]
            static void Postfix()
            {
                temp_selected_discovered_slot = null;
                temp_selected_active_slot = null;
            }
        }
    }
}
