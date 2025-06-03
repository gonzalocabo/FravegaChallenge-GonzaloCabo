using System.Xml;

namespace FravegaChallenge.API.Endpoints.V1.Descriptions;

public static class OrdersDescriptions
{
    public const string CREATE_ORDER_DESCRIPTION = @"Creates a new order in the system. 
- `externalReferenceId` must be unique per channel.
- `totalValue` must match the sum of all product prices multiplied by quantity.";

    public const string REGISTER_EVENT_DESCRIPTION = @"Registers a new event in the system.<br>
If the event was already been registered, it will be ignored.<br>
- `id` must be unique.";

    public const string GET_ORDER_DESCRIPTION = @"Retrieves the complete details of an order, including all available translations.<br><br>
This endpoint returns the full information for the specified order, such as customer data,
product list, status, and any translated fields where applicable.<br><br>
The response includes translated names when available in property ``{property_name}_translate``, e.g., ``channel_translate``.<br><br>
Events will be returned if available ";

    public const string SEARCH_ORDERS_DESCRIPTION = @"Searches for orders using the specified optional parameters.<br><br>
You can filter orders by any of the following criteria:<br>
<ul>
<li><b>orderId</b> – Unique identifier of the order.</li>
<li><b>documentNumber</b> – Customer's document number.</li>
<li><b>status</b> – Current status of the order.</li>
<li><b>createdOnFrom</b> – Start date/time for the search (Argentina time).</li>
<li><b>createdOnTo</b> – End date/time for the search (Argentina time).</li>
</ul>
All parameters are optional. If no filters are provided, all orders will be returned.<br><br>
**Example usage:**<br>
`GET /orders/search?status=Created&createdOnFrom=2024-05-01T00:00:00-03:00&createdOnTo=2024-05-31T23:59:59-03:00`";
}
