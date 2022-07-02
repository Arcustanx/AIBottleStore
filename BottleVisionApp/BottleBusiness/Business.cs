using BottleVisionApp.BottleBusiness;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BottleBusiness.BottleBusiness
{
    /// <summary>
    /// This is the Business. It has a shop, a warehouse and a budget (earned money).
    /// The number of sold bottles are used for each step of calculating the budget.
    /// The business is updated, according to the detected changes by the camera.
    /// </summary>
    public class Business
    {
        private readonly Warehouse warehouse;
        private readonly Shop shop;
        private readonly BottleCatalog bottleCatalog = new BottleCatalog();
        private readonly Dictionary<BottleType, int> soldBottles = new Dictionary<BottleType, int>();
        private readonly Dictionary<BottleType, int> overallSold = new Dictionary<BottleType, int>();
        private double budget = 0.00;


        /// <summary>
        /// Initialize Business which has a Shop and a Warehouse
        /// </summary>
        /// <param name="initShop"></param>
        /// <param name="initWarehouse"></param>
        public Business(List<int> initShop, List<int> initWarehouse)
        {
            shop = new Shop(initShop);
            warehouse = new Warehouse(initWarehouse);
            bottleCatalog.GetListOfBottles().ForEach(bottleType => soldBottles.Add(bottleType, 0));
            bottleCatalog.GetListOfBottles().ForEach(bottleType => overallSold.Add(bottleType, 0));
        }

        // PUBLIC METHODS

        /// <summary>
        /// Get the Catalog
        /// </summary>
        /// <returns>List of all Bottles in the Catalog</returns>
        public List<BottleType> GetBottleList()
        {
            return bottleCatalog.GetListOfBottles();
        }

        /// <summary>
        /// Gets the overall sold number of bottles of a given bottle type since the business started
        /// </summary>
        /// <param name="bottleType"></param>
        /// <returns></returns>
        public string GetOverallSoldBottles(BottleType bottleType)
        {
            return overallSold[bottleType].ToString();
        }

        /// <summary>
        /// Checks if the detected amount of bottles is actual the same as in the shop
        /// </summary>
        /// <param name="detectedNumberOfBottles"></param>
        /// <returns>True if amount is same, else false</returns>
        public bool IsSameAmount(List<int> detectedNumberOfBottles)
        {
            return shop.IsSame(detectedNumberOfBottles);
        }

        /// <summary>
        /// Updates number of bottles in all storages: shop & warehouse
        /// </summary>
        /// <param name="detectedNumberOfBottles"></param>
        public void UpdateStorages(List<int> detectedNumberOfBottles)
        {
            bottleCatalog.GetListOfBottles().ForEach(bottleType =>
                Update(bottleType, detectedNumberOfBottles[(int)bottleType]));
        }

        /// <summary>
        /// Checks if any bottles were sold
        /// </summary>
        /// <returns>True if yes, else False</returns>
        public bool GetIsAnyBottleSold()
        {
            return soldBottles.Values.Any(x => x != 0);
        }

        /// <summary>
        /// Checks if the warehouse needs to be filled up with bottles
        /// </summary>
        /// <returns>True, if the number of bottles of one type is 0, else False</returns>
        public bool NeedBottlesInWarehouse()
        {
            return warehouse.NeedBottles();
        }

        /// <summary>
        /// Checks if the shop needs to be filled up with bottles
        /// </summary>
        /// <returns>True, if the number of bottles of one type is 0, else False</returns>
        public bool NeedBottlesInShop()
        {
            return shop.NeedBottles();
        }

        /// <summary>
        /// Shows how many bottles of a given type were sold together with its price
        /// </summary>
        /// <param name="bottleType"></param>
        /// <returns>number of sold bottles times the price</returns>
        public string GetBottlePrice(BottleType bottleType)
        {
            return String.Format("{0:.00}", GetPriceOf(bottleType) / 100.0);
        }

        /// <summary>
        /// Get the amount of money earned.
        /// </summary>
        /// <returns>Earned money</returns>
        public string GetBudgetMsg()
        {
            return String.Format("{0:.00}", budget) + " Euro";
        }

        /// <summary>
        /// Sets number of sold bottles to 0
        /// </summary>
        public void ResetSoldBottles()
        {
            bottleCatalog.GetListOfBottles().ForEach(bottleType => soldBottles[bottleType] = 0);
        }

        /// <summary>
        /// Get Amount of bottles in the shop of the given Bottle Type
        /// </summary>
        /// <param name="bottleType"></param>
        /// <returns>amount as int</returns>
        public int GetAmountInShop(BottleType bottleType)
        {
            return shop.GetAmount(bottleType);
        }

        /// <summary>
        /// Get Amount of bottles in the warehouse of the given Bottle Type
        /// </summary>
        /// <param name="bottleType"></param>
        /// <returns>amount as int</returns>
        public int GetAmountInWarehouse(BottleType bottleType)
        {
            return warehouse.GetAmount(bottleType);
        }




        // PRIVATE METHODS

        /// <summary>
        /// Updates the number of bottles according to detection.
        /// If the amount detected is smaller than it was before the bottles were sold.
        /// If the amount detected is higher than it was before the bottle came from the warehouse.
        /// </summary>
        /// <param name="bottleType"></param>
        /// <param name="detectedNumOfBottles"></param>
        private void Update(BottleType bottleType, int detectedNumOfBottles)
        {
            int actualAmount = shop.GetAmount(bottleType);
            int difference = Math.Abs(actualAmount - detectedNumOfBottles);

            if (actualAmount > detectedNumOfBottles)
            {
                Sold(bottleType, difference);
            }
            else if (actualAmount < detectedNumOfBottles)
            {
                Refilled(bottleType, difference);
            }
        }

        /// <summary>
        /// Get the Price of given Bottle
        /// </summary>
        /// <param name="bottleType"></param>
        /// <returns></returns>
        private int GetPriceOf(BottleType bottleType)
        {
            return bottleCatalog.GetPrice(bottleType);
        }

        /// <summary>
        /// Adds an amount of bottles to the shop and
        /// Subtracts the same amount from the warehouse.
        /// </summary>
        /// <param name="bottleType"></param>
        /// <param name="amount"></param>
        private void Refilled(BottleType bottleType, int amount)
        {
            shop.Increase(bottleType, amount);
            warehouse.Decrease(bottleType, amount);
        }

        /// <summary>
        /// Subtracts the amount of bottles sold from the shop,
        /// Calculates the earned money from selling and
        /// Updates the number of sold bottles.
        /// </summary>
        /// <param name="bottleType"></param>
        /// <param name="amount"></param>
        private void Sold(BottleType bottleType, int amount)
        {
            shop.Decrease(bottleType, amount);
            budget += GetPriceOf(bottleType) / 100.0 * amount;
            UpdateSoldBottles(bottleType, amount);
        }

        /// <summary>
        /// Updates the amount of sold bottles.
        /// </summary>
        /// <param name="bottleType"></param>
        /// <param name="amount"></param>
        private void UpdateSoldBottles(BottleType bottleType, int amount)
        {
            soldBottles[bottleType] = amount;
            overallSold[bottleType] += amount;
        }
    }
}