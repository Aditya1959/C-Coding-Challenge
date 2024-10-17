CREATE DATABASE OrderManagementDB;

USE OrderManagementDB;

CREATE TABLE Users (
    UserId INT NOT NULL, 
    Username NVARCHAR(50) NOT NULL,
    Password NVARCHAR(50) NOT NULL,
    Role NVARCHAR(10) NOT NULL CHECK (Role IN ('Admin', 'User')),
    PRIMARY KEY (UserId) -- Setting UserId as Primary Key
);

CREATE TABLE Products (
    ProductId INT NOT NULL, 
    ProductName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255) NOT NULL,
    Price DECIMAL(18, 2) NOT NULL,
    QuantityInStock INT NOT NULL,
    Type NVARCHAR(50) NOT NULL CHECK (Type IN ('Electronics', 'Clothing')),
    PRIMARY KEY (ProductId) -- Setting ProductId as Primary Key
);

CREATE TABLE Orders (
    OrderId INT NOT NULL, 
    UserId INT NOT NULL,
    OrderDate DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId), -- Foreign key constraint
    PRIMARY KEY (OrderId) -- Setting OrderId as Primary Key
);

CREATE TABLE OrderItems (
    OrderItemId INT NOT NULL,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    FOREIGN KEY (OrderId) REFERENCES Orders(OrderId), -- Foreign key constraint
    FOREIGN KEY (ProductId) REFERENCES Products(ProductId), -- Foreign key constraint
    PRIMARY KEY (OrderItemId) -- Setting OrderItemId as Primary Key
);

ALTER TABLE Products
ALTER COLUMN Price FLOAT;

SELECT * 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Products';

Drop Table Orders;