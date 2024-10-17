using OrderManagementSystem.dao;
using OrderManagementSystem.entity;

using OrderManagementSystem.exception;
using System;
using System.Collections.Generic;

namespace OrderManagementSystem.Main
{
    public class MainModule
    {
        public static void Main(string[] args)
        {
            IOrderManagementRepository orderManager = new OrderProcessor();
            while (true)
            {
                Console.WriteLine("Select an operation: \n1. Create User \n2. Create Product \n3. Cancel Order \n4. Get All Products \n5. Get Order by User \n6. Create Order \n7. Exit");
                int choice = Convert.ToInt32(Console.ReadLine());

                switch (choice)
                {
                    case 1:
                        // Create user
                        Console.WriteLine("Enter User ID:");
                        int userId = Convert.ToInt32(Console.ReadLine());

                        Console.WriteLine("Enter Username:");
                        string username = Console.ReadLine();

                        Console.WriteLine("Enter Password:");
                        string password = Console.ReadLine();

                        Console.WriteLine("Enter Role (Admin/User):");
                        string role = Console.ReadLine();

                        User newUser = new User(userId,username, password, role);
                        try
                        {
                            orderManager.CreateUser(newUser);
                            Console.WriteLine("User created successfully!");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error creating user: " + ex.Message);
                        }
                        break;

                    case 2:
                        // Create product
                        Console.WriteLine("Enter User ID (Admin only):");
                        int adminId = Convert.ToInt32(Console.ReadLine());

                        Console.WriteLine("Enter Product ID:");
                        int productId = Convert.ToInt32(Console.ReadLine());

                        Console.WriteLine("Enter Product Name:");
                        string productName = Console.ReadLine();

                        Console.WriteLine("Enter Description:");
                        string description = Console.ReadLine();

                        Console.WriteLine("Enter Price:");
                        double price = Convert.ToDouble(Console.ReadLine());

                        Console.WriteLine("Enter Quantity in Stock:");
                        int quantityInStock = Convert.ToInt32(Console.ReadLine());

                        Console.WriteLine("Enter Product Type (Electronics/Clothing):");
                        string type = Console.ReadLine();

                        Product newProduct;
                        if (type == "Electronics")
                        {
                            Console.WriteLine("Enter Brand:");
                            string brand = Console.ReadLine();

                            Console.WriteLine("Enter Warranty Period (in months):");
                            int warrantyPeriod = Convert.ToInt32(Console.ReadLine());

                            newProduct = new Electronics(productId,productName, description, price, quantityInStock, type, brand, warrantyPeriod);
                        }
                        else if (type == "Clothing")
                        {
                            Console.WriteLine("Enter Size:");
                            string size = Console.ReadLine();

                            Console.WriteLine("Enter Color:");
                            string color = Console.ReadLine();

                            newProduct = new Clothing(productId,productName, description, price, quantityInStock, type, size, color);
                        }
                        else
                        {
                            Console.WriteLine("Invalid product type.");
                            break;
                        }

                        try
                        {
                            User adminUser = new User(adminId,"AdminUser", "password", "Admin");
                            orderManager.CreateProduct(adminUser, newProduct);
                            Console.WriteLine("Product created successfully!");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error creating product: " + ex.Message);
                        }
                        break;

                    case 3:
                        // Cancel order
                        Console.WriteLine("Enter User ID:");
                        int cancelUserId = Convert.ToInt32(Console.ReadLine());

                        Console.WriteLine("Enter Order ID:");
                        int cancelOrderId = Convert.ToInt32(Console.ReadLine());

                        try
                        {
                            orderManager.CancelOrder(cancelUserId, cancelOrderId);
                            Console.WriteLine("Order canceled successfully!");
                        }
                        catch (OrderNotFoundException onf)
                        {
                            Console.WriteLine("Error: " + onf.Message);
                        }
                        catch (UserNotFoundException unf)
                        {
                            Console.WriteLine("Error: " + unf.Message);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error canceling order: " + ex.Message);
                        }
                        break;

                    case 4:
                        // Get all products
                        try
                        {
                            List<Product> products = orderManager.GetAllProducts();
                            if (products.Count > 0)
                            {
                                Console.WriteLine("Product List:");
                                foreach (var product in products)
                                {
                                    Console.WriteLine($"Product ID: {product.ProductId}, Name: {product.ProductName}, Price: {product.Price}, Stock: {product.QuantityInStock}, Type: {product.Type}");
                                }
                            }
                            else
                            {
                                Console.WriteLine("No products found.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error fetching products: " + ex.Message);
                        }
                        break;

                    case 5:
                        // Get order by user
                        Console.WriteLine("Enter User ID:");
                        int orderUserId = Convert.ToInt32(Console.ReadLine());

                        try
                        {
                            User orderUser = new User(orderUserId,"User", "password", "User");
                            List<Product> orderedProducts = orderManager.GetOrderByUser(orderUser);

                            if (orderedProducts.Count > 0)
                            {
                                Console.WriteLine("Ordered Products:");
                                foreach (var product in orderedProducts)
                                {
                                    Console.WriteLine($"Product ID: {product.ProductId}, Name: {product.ProductName}, Price: {product.Price}, Stock: {product.QuantityInStock}, Type: {product.Type}");
                                }
                            }
                            else
                            {
                                Console.WriteLine("No orders found for this user.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error fetching orders: " + ex.Message);
                        }
                        break;
                    case 6:  // Create Order
    Console.WriteLine("Enter User ID to create an order:");
    int orderCreatorUserId = Convert.ToInt32(Console.ReadLine());
    Console.WriteLine("Enter number of products to order:");
    int numProducts = Convert.ToInt32(Console.ReadLine());
    List<(Product, int)> orderProductsWithQuantities = new List<(Product, int)>();

    for (int i = 0; i < numProducts; i++)
    {
        Console.WriteLine($"Enter Product ID for product {i + 1}:");
        int orderProductId = Convert.ToInt32(Console.ReadLine());

        // Assuming you have a method to fetch a product by ID
        Product product = orderManager.GetAllProducts().Find(p => p.ProductId == orderProductId);
        if (product != null)
        {
            Console.WriteLine($"Enter quantity for {product.ProductName}:");
            int quantity = Convert.ToInt32(Console.ReadLine());
            orderProductsWithQuantities.Add((product, quantity));
        }
        else
        {
            Console.WriteLine($"Product with ID {orderProductId} not found.");
        }
    }

    try
    {
                     orderManager.CreateOrder(new User(orderCreatorUserId, "username", "password", "role"), orderProductsWithQuantities);

                     Console.WriteLine("Order created successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error creating order: {ex.Message}");
    }
    break;
                    case 7:
                        Console.WriteLine("Exiting application...");
                        Environment.Exit(0);
                        break;

                    default:
                        Console.WriteLine("Invalid choice! Please select a valid option.");
                        break;
                }
            }
        }
    }
}
