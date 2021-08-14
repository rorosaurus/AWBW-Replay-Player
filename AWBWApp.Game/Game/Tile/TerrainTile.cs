﻿using System.Collections.Generic;
using AWBWApp.Game.Game.Logic;
using Newtonsoft.Json;

namespace AWBWApp.Game.Game.Tile
{
    public class TerrainTile
    {
        [JsonProperty]
        public int AWBWId { get; set; }

        [JsonProperty]
        public int BaseDefence { get; set; }

        [JsonProperty]
        public int SightDistanceIncrease { get; set; }

        [JsonProperty]
        public int LimitFogOfWarSightDistance { get; set; } = -1;

        [JsonProperty]
        public Dictionary<MovementType, int> MovementCostsPerType { get; set; }

        [JsonProperty]
        public string BaseTexture { get; set; }

        [JsonProperty]
        public string FogOfWarTexture { get; set; }
    }
}
