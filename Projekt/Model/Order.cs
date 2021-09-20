
using System;

namespace Projekt
{
    public class Order
    {
        public int OrderId { get; set; }
        public int UserId {  get; set; }
        public int DishId {  get; set; }
        public int Amount {  get; set; }
        public DateTime Date {  get; set; }
    }
}
