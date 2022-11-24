namespace SilverSoft.Utils
{
    public class HandlerRouteAttribute:Attribute
    {
        public HandlerRouteAttribute() { }

        public HandlerRouteAttribute(string route)
        {
            Route = route;
        }

        public string Route { get; set; }
    }
}
