using OrderManagementSystem.entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderManagementSystem.util;
using OrderManagementSystem.exception;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace OrderManagementSystem.dao
{
    public class OrderProcessor : IOrderManagementRepository
    {
        // Create Order and store it in the Orders table, linking the user and product.
        
            public void CreateOrder(User user, List<Product> products)
            {
                using (SqlConnection conn = DBConnUtil.GetDBConn())
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Assuming there's an Orders table
                            string insertOrderQuery = "INSERT INTO Orders (UserId) OUTPUT INSERTED.OrderId VALUES (@UserId)";
                            SqlCommand command = new SqlCommand(insertOrderQuery, conn, transaction);
                            command.Parameters.AddWithValue("@UserId", user.UserId);

                            // Retrieve the new OrderId
                            int newOrderId = (int)command.ExecuteScalar();

                            // Insert products for the order
                            foreach (var product in products)
                            {
                                string insertOrderDetailsQuery = "INSERT INTO OrderDetails (OrderId, ProductId) VALUES (@OrderId, @ProductId)";
                                SqlCommand detailsCommand = new SqlCommand(insertOrderDetailsQuery, conn, transaction);
                                detailsCommand.Parameters.AddWithValue("@OrderId", newOrderId);
                                detailsCommand.Parameters.AddWithValue("@ProductId", product.ProductId); // Ensure this matches your column name
                                detailsCommand.ExecuteNonQuery();
                            }

                            transaction.Commit();
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw; // Rethrow to handle it in the calling method
                        }
                    }
                }
            }

            // Cancel the order and remove it from the Orders table
            public void CancelOrder(int userId, int orderId)
        {
            using (SqlConnection conn = DBConnUtil.GetDBConn())
            {
                conn.Open();

                // Check if the order exists in the Orders table
                string checkOrderQuery = "SELECT COUNT(*) FROM Orders WHERE OrderId = @OrderId AND UserId = @UserId";
                using (SqlCommand checkOrderCmd = new SqlCommand(checkOrderQuery, conn))
                {
                    checkOrderCmd.Parameters.AddWithValue("@OrderId", orderId);
                    checkOrderCmd.Parameters.AddWithValue("@UserId", userId);

                    int orderExists = (int)checkOrderCmd.ExecuteScalar();
                    if (orderExists == 0)
                    {
                        // If order does not exist, throw exception
                        throw new OrderNotFoundException($"Order with ID {orderId} for User ID {userId} not found.");
                    }
                }

                // Delete the order if it exists
                string deleteOrderQuery = "DELETE FROM Orders WHERE OrderId = @OrderId AND UserId = @UserId";
                using (SqlCommand deleteOrderCmd = new SqlCommand(deleteOrderQuery, conn))
                {
                    deleteOrderCmd.Parameters.AddWithValue("@OrderId", orderId);
                    deleteOrderCmd.Parameters.AddWithValue("@UserId", userId);
                    deleteOrderCmd.ExecuteNonQuery();
                }

                conn.Close();
            }
        }

        // Add a new product if the user is an admin
        public void CreateProduct(User user, Product product)
        {
            if (user.Role != "Admin")
                throw new UnauthorizedAccessException("Only admins can create products.");

            using (SqlConnection conn = DBConnUtil.GetDBConn())
            {
                conn.Open();

                // Insert product into the Products table
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

        // Add a new user to the Users table
        public void CreateUser(User user)
        {
            using (SqlConnection conn = DBConnUtil.GetDBConn())
            {
                conn.Open();

                // Insert user into the Users table
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

        // Retrieve all products from the Products table
        public List<Product> GetAllProducts()
        {
            List<Product> products = new List<Product>();

            using (SqlConnection conn = DBConnUtil.GetDBConn())
            {
                conn.Open();

                // Select all products from the database
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

        // Retrieve all orders placed by a specific user from the Orders table
        public List<Product> GetOrderByUser(User user)
        {
            List<Product> orderedProducts = new List<Product>();

            using (SqlConnection conn = DBConnUtil.GetDBConn())
            {
                conn.Open();

                // Fetch orders for the given user
                string getOrderByUserQuery = @"
                    SELECT p.ProductId, p.ProductName, p.Description, p.Price, p.QuantityInStock, p.Type
                    FROM Orders o
                    JOIN Products p ON o.ProductId = p.ProductId
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
