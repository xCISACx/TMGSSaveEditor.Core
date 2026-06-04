using System;
using System.Collections.Generic;
using System.Text;

namespace TMGSSaveEditor.Core
{
    public interface ISaveDataManager
    {
        // Must be able to load and return the save data object
        Object Load(string path);

        // Must be able to save the data object back to a path
        void Save(string path, Object data);

        // Must provide the specific character names for the UI dropdowns
        string[] ApproachCharacterNames { get; }
        string[] FriendCharacterNames { get; }
        string[] AdvCharacterNames { get; }
        string[] ParameterNames { get; }
        string[] ClothingNames { get; }

        enum CycleCountType;

        enum PlayerFlag;

        enum CharFlag;

        enum CharCounter;

        enum DateTopicBoy;

        enum EndingId;

        enum OneYearCommandType;

        enum ShopId;

        enum DateContent;

        string[] FashionKindId { get; }
        string[] AccessoryKindId { get; }

    }
}
