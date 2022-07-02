using System.Collections.Generic;

namespace BottleBusiness.BottleBusiness
{
    /// <summary>
    /// The Shop is the place where the camera detects the changes.
    /// Through these changes updates will happen.
    /// </summary>
    class Shop : BottleStorage
    {
        public Shop(List<int> amountOfEachCan) :
            base(amountOfEachCan)
        {
        }
    }
}
