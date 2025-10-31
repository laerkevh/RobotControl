using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

// =========================
// 1) Simple data models
// =========================
class Item
{
    public string Name { get; set; } = string.Empty;
    public decimal PricePerUnit { get; set; }
    // 1=a, 2=b, 3=c — used to compute X position
    public uint InventoryLocation { get; set; }
}

class OrderLine
{
    public Item Item { get; set; } = null!;
    public int Quantity { get; set; }
}

class Order
{
    public List<OrderLine> OrderLines { get; set; } = new();
    public DateTime Time { get; set; }
}

// Very small order queue
class OrderBook
{
    private readonly Queue<Order> _orders = new();
    public void AddOrder(Order order) => _orders.Enqueue(order);
    public List<OrderLine> ProcessNextOrder() => _orders.Count > 0 ? _orders.Dequeue().OrderLines : new();
}

// =========================
// 2) Robot classes
// =========================
class Robot
{
    public const int UrscriptPort = 30002, DashboardPort = 29999;
    public string IpAddress = "localhost";

    public void SendString(int port, string message)
    {
        using var client = new TcpClient(IpAddress, port);
        using var stream = client.GetStream();
        stream.Write(Encoding.ASCII.GetBytes(message));
    }

    public void SendUrscript(string urscript)
    {
        SendString(DashboardPort, "brake release\n");
        SendString(UrscriptPort, urscript);
    }
}

class ItemSorterRobot : Robot
{
    // {0} will be replaced with the item's X coordinate in meters
    public const string UrscriptTemplate = @"

def move_item_to_shipment_box():
    SBOX_X = 0.3
    SBOX_Y = 0.3
    ITEM_X = {0}
    ITEM_Y = 0.1
    DOWN_Z = 0.1

    def moveto(x, y, z = 0.0):
        movej(p[x, y, z, 0, 0, 0], a=1.2, v=0.25)
        sleep(0.5)
    end

    # go to item box and dip down
    moveto(ITEM_X, ITEM_Y, 0.0)
    moveto(ITEM_X, ITEM_Y, DOWN_Z)
    sleep(0.5) # grasp assumed
    moveto(ITEM_X, ITEM_Y, 0.0)

    # go to shipment box S and dip down
    moveto(SBOX_X, SBOX_Y, 0.0)
    moveto(SBOX_X, SBOX_Y, DOWN_Z)
    sleep(0.5) # release assumed
    moveto(SBOX_X, SBOX_Y, 0.0)
end
";

    public void PickUp(uint inventoryLocation)
    {
        // 1,2,3 -> 0.1, 0.2, 0.3 meters
        double itemX = inventoryLocation * 0.1;
        string program = string.Format(CultureInfo.InvariantCulture, UrscriptTemplate,
            itemX.ToString("0.0###", CultureInfo.InvariantCulture));
        SendUrscript(program);
    }
}

// =========================
// 3) Tiny demo (console)
// =========================
class Program
{
    static OrderBook orderBook = new();
    static ItemSorterRobot robot = new();

    public static async Task Main()
    {
        SeedTestOrders();
        Console.WriteLine("Week07 – Item Sorter Robot (Student Simple Version)\n");
        Console.WriteLine("Press ENTER to process the NEXT order. Press Ctrl+C to exit.\n");

        while (true)
        {
            Console.Write("Ready> ");
            Console.ReadLine();
            await ProcessNextOrderAsync();
        }
    }

    static async Task ProcessNextOrderAsync()
    {
        var lines = orderBook.ProcessNextOrder();
        if (lines.Count == 0)
        {
            Console.WriteLine("No pending orders.\n");
            return;
        }

        foreach (var line in lines)
        {
            for (int i = 0; i < line.Quantity; i++)
            {
                Console.WriteLine($"Picking up {line.Item.Name} from box {line.Item.InventoryLocation}…");
                robot.PickUp(line.Item.InventoryLocation);
                await Task.Delay(9500); // wait for robot motion
            }
        }

        Console.WriteLine("Order complete. Conveyor puts a new empty box at S.\n");
    }

    static void SeedTestOrders()
    {
        var item1 = new Item { Name = "M3 screw", PricePerUnit = 1m, InventoryLocation = 1 };
        var item2 = new Item { Name = "M3 nut", PricePerUnit = 1.5m, InventoryLocation = 2 };
        var item3 = new Item { Name = "pen", PricePerUnit = 1m, InventoryLocation = 3 };

        var order1 = new Order
        {
            OrderLines = new()
            {
                new OrderLine { Item = item1, Quantity = 1 },
                new OrderLine { Item = item2, Quantity = 2 },
                new OrderLine { Item = item3, Quantity = 1 },
            },
            Time = DateTime.Now.AddDays(-2)
        };

        var order2 = new Order
        {
            OrderLines = new()
            {
                new OrderLine { Item = item2, Quantity = 1 },
            },
            Time = DateTime.Now
        };

        orderBook.AddOrder(order1);
        orderBook.AddOrder(order2);

        Console.WriteLine("Seeded sample orders.\n");
    }
}
