using OrderManagementSystem.entity;
using OrderManagementSystem.util;
using OrderManagementSystem.exception;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace OrderManagementSystem.dao
{
    public class OrderProcessor : IOrderManagementRepository
    {
        // Create Order and store it in the Orders and OrderItems tables
        public void CreateOrder(User user, List<(Product product, int quantity)> productsWithQuantities)
        {
            using (SqlConnection conn = DBConnUtil.GetDBConn())
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Check if the user exists in the Users table
                        string checkUserQuery = "SELECT COUNT(*) FROM Users WHERE UserId = @UserId";
                        using (SqlCommand checkUserCmd = new SqlCommand(checkUserQuery, conn, transaction))
                        {
                            checkUserCmd.Parameters.AddWithValue("@UserId", user.UserId);
                            int userExists = (int)checkUserCmd.ExecuteScalar();
                            if (userExists == 0)
                            {
                                throw new UserNotFoundException($"User with ID {user.UserId} not found.");
                            }
                        }

                        // Generate a new OrderId
                        int newOrderId;
                        string getNextOrderIdQuery = "SELECT ISNULL(MAX(OrderId), 0) + 1 FROM Orders";
                        using (SqlCommand getOrderIdCmd = new SqlCommand(getNextOrderIdQuery, conn, transaction))
                        {
                            newOrderId = (int)getOrderIdCmd.ExecuteScalar();
                        }

                        // Insert the new order in the Orders table
                        string insertOrderQuery = "INSERT INTO Orders (OrderId, UserId, OrderDate) VALUES (@OrderId, @UserId, GETDATE())";
                        using (SqlCommand insertOrderCmd = new SqlCommand(insertOrderQuery, conn, transaction))
                        {
                            insertOrderCmd.Parameters.AddWithValue("@OrderId", newOrderId);
                            insertOrderCmd.Parameters.AddWithValue("@UserId", user.UserId);
                            insertOrderCmd.ExecuteNonQuery();
                        }

                        // Insert the order items in the OrderItems table
                        foreach (var (product, quantity) in productsWithQuantities)
                        {
                            // Ensure product exists
                            string checkProductQuery = "SELECT COUNT(*) FROM Products WHERE ProductId = @ProductId";
                            using (SqlCommand checkProductCmd = new SqlCommand(checkProductQuery, conn, transaction))
                            {
                                checkProductCmd.Parameters.AddWithValue("@ProductId", product.ProductId);
                                int productExists = (int)checkProductCmd.ExecuteScalar();
                                if (productExists == 0)
                                {
                                    throw new ProductNotFoundException($"Product with ID {product.ProductId} not found.");
                                }
                            }

                            // Generate a new OrderItemId
                            int newOrderItemId;
                            string getNextOrderItemIdQuery = "SELECT ISNULL(MAX(OrderItemId), 0) + 1 FROM OrderItems";
                            using (SqlCommand getOrderItemIdCmd = new SqlCommand(getNextOrderItemIdQuery, conn, transaction))
                            {
                                newOrderItemId = (int)getOrderItemIdCmd.ExecuteScalar();
                            }

                            // Insert into OrderItems table
                            string insertOrderItemQuery = @"
                                INSERT INTO OrderItems (OrderItemId, OrderId, ProductId, Quantity) 
                                VALUES (@OrderItemId, @OrderId, @ProductId, @Quantity)";
                            using (SqlCommand insertOrderItemCmd = new SqlCommand(insertOrderItemQuery, conn, transaction))
                            {
                                insertOrderItemCmd.Parameters.AddWithValue("@OrderItemId", newOrderItemId);
                                insertOrderItemCmd.Parameters.AddWithValue("@OrderId", newOrderId);
                                insertOrderItemCmd.Parameters.AddWithValue("@ProductId", product.ProductId);
                                insertOrderItemCmd.Parameters.AddWithValue("@Quantity", quantity);
                                insertOrderItemCmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        Console.WriteLine("Order created successfully.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Error creating order: " + ex.Message);
                    }
                }
            }
        }

        // Cancel the order and remove it from the Orders and OrderItems tables
        public void CancelOrder(int userId, int orderId)
        {
            using (SqlConnection conn = DBConnUtil.GetDBConn())
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Check if the order exists
                        string checkOrderQuery = "SELECT COUNT(*) FROM Orders WHERE OrderId = @OrderId AND UserId = @UserId";
                        using (SqlCommand checkOrderCmd = new SqlCommand(checkOrderQuery, conn, transaction))
                        {
                            checkOrderCmd.Parameters.AddWithValue("@OrderId", orderId);
                            checkOrderCmd.Parameters.AddWithValue("@UserId", userId);
                            int orderExists = (int)checkOrderCmd.ExecuteScalar();
                            if (orderExists == 0)
                            {
                                throw new OrderNotFoundException($"Order with ID {orderId} for User ID {userId} not found.");
                            }
                        }

                        // Delete the order items first
                        string deleteOrderItemsQuery = "DELETE FROM OrderItems WHERE OrderId = @OrderId";
                        using (SqlCommand deleteOrderItemsCmd = new SqlCommand(deleteOrderItemsQuery, conn, transaction))
                        {
                            deleteOrderItemsCmd.Parameters.AddWithValue("@OrderId", orderId);
                            deleteOrderItemsCmd.ExecuteNonQuery();
                        }

                        // Then delete the order
                        string deleteOrderQuery = "DELETE FROM Orders WHERE OrderId = @OrderId AND UserId = @UserId";
                        using (SqlCommand deleteOrderCmd = new SqlCommand(deleteOrderQuery, conn, transaction))
                        {
                            deleteOrderCmd.Parameters.AddWithValue("@OrderId", orderId);
                            deleteOrderCmd.Parameters.AddWithValue("@UserId", userId);
                            deleteOrderCmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        Console.WriteLine("Order cancelled successfully.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Error cancelling order: " + ex.Message);
                    }
                }
            }
        }

        // Create a new product
        public void CreateProduct(User user, Product product)
        {
            if (user.Role != "Admin")
                throw new UnauthorizedAccessException("Only admins can create products.");

            using (SqlConnection conn = DBConnUtil.GetDBConn())
            {
                conn.Open();
                string insertProductQuery = @"
                    INSERT INTO Products (ProductId, ProductName, Description, Price, QuantityInStock, Type)
                    VALUES (@ProductId, @ProductName, @Description, @Price, @QuantityInStock, @Type)";

                using (SqlCommand insertProductCmd = new SqlCommand(insertProductQuery, conn))
                {
                    insertProductCmd.Parameters.AddWithValue("@ProductId", product.ProductId);
                    insertProductCmd.Parameters.AddWithValue("@ProductName", product.ProductName);
                    insertProductCmd.Parameters.AddWithValue("@Description", product.Description);
                    insertProductCmd.Parameters.AddWithValue("@Price", product.Price);
                    insertProductCmd.Parameters.AddWithValue("@QuantityInStock", product.QuantityInStock);
                    insertProductCmd.Parameters.AddWithValue("@Type", product.Type);
                    insertProductCmd.ExecuteNonQuery();
                }

                conn.Close();
            }
        }

        // Create a new user
        public void CreateUser(User user)
        {
            using (SqlConnection conn = DBConnUtil.GetDBConn())
            {
                conn.Open();
                string insertUserQuery = @"
                    INSERT INTO Users (UserId, Username, Password, Role)
                    VALUES (@UserId, @Username, @Password, @Role)";

                using (SqlCommand insertUserCmd = new SqlCommand(insertUserQuery, conn))
                {
                    insertUserCmd.Parameters.AddWithValue("@UserId", user.UserId);
                    insertUserCmd.Parameters.AddWithValue("@Username", user.Username);
                    insertUserCmd.Parameters.AddWithValue("@Password", user.Password);
                    insertUserCmd.Parameters.AddWithValue("@Role", user.Role);
                    insertUserCmd.ExecuteNonQuery();
                }

                conn.Close();
            }
        }

        // Retrieve all products
        public List<Product> GetAllProducts()
        {
            List<Product> products = new List<Product>();

            using (SqlConnection conn = DBConnUtil.GetDBConn())
            {
                conn.Open();
                string getAllProductsQuery = "SELECT * FROM Products";

                using (SqlCommand getAllProductsCmd = new SqlCommand(getAllProductsQuery, conn))
                {
                    using (SqlDataReader reader = getAllProductsCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Product product = new Product(
                                reader.GetInt32(0), // ProductId
                                reader.GetString(1), // ProductName
                                reader.GetString(2), // Description
                                reader.GetDouble(3), // Price
                                reader.GetInt32(4),  // QuantityInStock
                                reader.GetString(5)  // Type
                            );
                            products.Add(product);
                        }
                    }
                }

                conn.Close();
            }

            return products;
        }

        // Retrieve all orders by user
        public List<Product> GetOrderByUser(User user)
        {
            List<Product> orderedProducts = new List<Product>();

            using (SqlConnection conn = DBConnUtil.GetDBConn())
            {
                conn.Open();
                string getOrderByUserQuery = @"
                    SELECT p.ProductId, p.ProductName, p.Description, p.Price, p.QuantityInStock, p.Type
                    FROM Orders o
                    JOIN OrderItems oi ON o.OrderId = oi.OrderId
                    JOIN Products p ON oi.ProductId = p.ProductId
                    WHERE o.UserId = @UserId";

                using (SqlCommand getOrderByUserCmd = new SqlCommand(getOrderByUserQuery, conn))
                {
                    getOrderByUserCmd.Parameters.AddWithValue("@UserId", user.UserId);

                    using (SqlDataReader reader = getOrderByUserCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Product product = new Product(
                                reader.GetInt32(0), // ProductId
                                reader.GetString(1), // ProductName
                                reader.GetString(2), // Description
                                reader.GetDouble(3), // Price
                                reader.GetInt32(4),  // QuantityInStock
                                reader.GetString(5)  // Type
                            );
                            orderedProducts.Add(product);
                        }
                    }
                }

                conn.Close();
            }

            return orderedProducts;
        }
    }
}
