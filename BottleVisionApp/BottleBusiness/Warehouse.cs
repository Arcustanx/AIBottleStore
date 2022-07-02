using System.Collections.Generic;

namespace BottleBusiness.BottleBusiness
{
    /// <summary>
    /// The Warehouse holds bottles which are transported to the shop.
    /// </summary>
    class Warehouse : BottleStorage
    {
        public Warehouse(List<int> amountOfEachCan) :
            base(amountOfEachCan)
        {
        }
    }
}
