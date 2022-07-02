using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BottleVisionApp.BottleBusiness
{
    /// <summary>
    /// Defines the bottles and its prices
    /// </summary>
    public class BottleCatalog
    {
        private readonly Dictionary<BottleType, int> bottlePrices = new Dictionary<BottleType, int>();

        // add new bottles here to the catalog
        public BottleCatalog()
        {
            bottlePrices.Add(BottleType.Cola, 250);
            bottlePrices.Add(BottleType.Fanta, 200);
            bottlePrices.Add(BottleType.Water, 150);
        }

        /// <summary>
        /// Get the price of a bottle
        /// </summary>
        /// <param name="bottleType"></param>
        /// <returns></returns>
        public int GetPrice(BottleType bottleType)
        {
            return bottlePrices[bottleType];
        }

        /// <summary>
        /// Get a list of all available bottles
        /// </summary>
        /// <returns>list of bottles</returns>
        public List<BottleType> GetListOfBottles()
        {
            return bottlePrices.Keys.ToList();
        }
    }
}
