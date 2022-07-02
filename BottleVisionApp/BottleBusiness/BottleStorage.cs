using BottleVisionApp.BottleBusiness;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BottleBusiness.BottleBusiness
{
    /// <summary>
    /// The Bottle Storage uses a Map to store information about the amount of bottle
    /// </summary>
    class BottleStorage
    {
        private readonly Dictionary<BottleType, int> bottlesInStorage = new Dictionary<BottleType, int>();

        public BottleStorage(List<int> amountOfEachBottle)
        {
            foreach (BottleType bottleType in (BottleType[])Enum.GetValues(typeof(BottleType)))
            {
                bottlesInStorage.Add(bottleType, amountOfEachBottle[(int)bottleType]);
            }
        }

        /// <summary>
        /// Gives information if bottles are needed
        /// </summary>
        /// <returns>true if yes, false else</returns>
        public bool NeedBottles()
        {
            return bottlesInStorage.Any(x => x.Value == 0);
        }

        /// <summary>
        /// Gets the amount of bottles of the given bottle type in the storage
        /// </summary>
        /// <param name="bottleType"></param>
        /// <returns></returns>
        public int GetAmount(BottleType bottleType)
        {
            return bottlesInStorage[bottleType];
        }

        /// <summary>
        /// Increases the number of bottles in the storage 
        /// by the given amount of the given bottle type.
        /// </summary>
        /// <param name="bottleType"></param>
        /// <param name="amount"></param>
        public void Increase(BottleType bottleType, int amount)
        {
            bottlesInStorage[bottleType] += amount;
        }

        /// <summary>
        /// Decreases the number of bottles in the storage 
        /// by the given amount of the given bottle type.
        /// Sets it 0 if warehouse somehow gets < 0 
        /// </summary>
        /// <param name="bottleType"></param>
        /// <param name="amount"></param>
        public void Decrease(BottleType bottleType, int amount)
        {
            bottlesInStorage[bottleType] -= amount;
            if (bottlesInStorage[bottleType] < 0)
                bottlesInStorage[bottleType] = 0;
        }

        /// <summary>
        /// Checks if the number of detected bottles are 
        /// the same as the number of bottles in the shop.
        /// </summary>
        /// <param name="detectedNumberOfBottles"></param>
        /// <returns>True if same, False else</returns>
        public bool IsSame(List<int> detectedNumberOfBottles)
        {
            return bottlesInStorage.Keys.ToList()
                    .All(bottleType => bottlesInStorage[bottleType] == detectedNumberOfBottles[(int)bottleType]);
        }
    }
}
