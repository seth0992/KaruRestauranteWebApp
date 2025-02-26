-- Crear base de datos
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'dbWebAppRestauranteKaru')
BEGIN
    CREATE DATABASE dbWebAppRestauranteKaru;
END
GO

USE dbWebAppRestauranteKaru;
GO

-- Crear tabla de Categorías
CREATE TABLE Categories (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,
    Description NVARCHAR(200),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL
);

-- Crear tabla de Roles
CREATE TABLE Roles (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL
);

-- Crear tabla de Usuarios
CREATE TABLE Users (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    LastLogin DATETIME NULL
);

-- Crear tabla de Roles de Usuario
CREATE TABLE UserRoles (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    RoleID INT NOT NULL,
    FOREIGN KEY (UserID) REFERENCES Users(ID),
    FOREIGN KEY (RoleID) REFERENCES Roles(ID)
);

-- Crear tabla de Refresh Tokens
CREATE TABLE RefreshTokens (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    RefreshToken NVARCHAR(500) NOT NULL,
    FOREIGN KEY (UserID) REFERENCES Users(ID)
);

-- Crear tabla de Ingredientes
CREATE TABLE Ingredients (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(200),
    StockQuantity DECIMAL(10,2) NOT NULL DEFAULT 0,
    UnitOfMeasure NVARCHAR(20) NOT NULL,
    MinimumStock DECIMAL(10,2) NOT NULL DEFAULT 0,
    PurchasePrice DECIMAL(10,2) NOT NULL DEFAULT 0,
    LastRestockDate DATETIME NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL
);

-- Crear enum ProductType como tabla
CREATE TABLE ProductTypes (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE
);

-- Insertar tipos de producto
INSERT INTO ProductTypes (Name) VALUES 
    ('Prepared'),    -- Productos que requieren preparación
    ('Inventory');   -- Productos directos de inventario

-- Crear tabla de Productos (FastFoodItems)
CREATE TABLE FastFoodItems (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(200),
    CategoryID INT NOT NULL,
    SellingPrice DECIMAL(10,2) NOT NULL DEFAULT 0,
    EstimatedCost DECIMAL(10,2) NOT NULL DEFAULT 0,
    ProductTypeID INT NOT NULL,
    IsAvailable BIT DEFAULT 1,
    ImageUrl NVARCHAR(500),
    EstimatedPreparationTime INT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    FOREIGN KEY (CategoryID) REFERENCES Categories(ID),
    FOREIGN KEY (ProductTypeID) REFERENCES ProductTypes(ID)
);

-- Crear tabla de Inventario de Productos
CREATE TABLE ProductInventory (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    FastFoodItemID INT NOT NULL UNIQUE,  -- Relación uno a uno
    CurrentStock INT NOT NULL DEFAULT 0,
    MinimumStock INT NOT NULL DEFAULT 0,
    PurchasePrice DECIMAL(10,2) NOT NULL DEFAULT 0,
    SuggestedMarkup DECIMAL(5,2) NOT NULL DEFAULT 0,
    LastRestockDate DATETIME NULL,
    SKU NVARCHAR(50),
    UnitOfMeasure NVARCHAR(20),
    LocationCode NVARCHAR(50),
    FOREIGN KEY (FastFoodItemID) REFERENCES FastFoodItems(ID)
);

-- Crear tabla de relación Productos-Ingredientes
CREATE TABLE ItemIngredients (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    FastFoodItemID INT NOT NULL,
    IngredientID INT NOT NULL,
    Quantity DECIMAL(10,2) NOT NULL DEFAULT 0,
    IsOptional BIT DEFAULT 0,
    CanBeExtra BIT DEFAULT 0,
    ExtraPrice DECIMAL(10,2) NOT NULL DEFAULT 0,
    FOREIGN KEY (FastFoodItemID) REFERENCES FastFoodItems(ID),
    FOREIGN KEY (IngredientID) REFERENCES Ingredients(ID)
);

-- Crear tabla de Combos
CREATE TABLE Combos (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(200),
    RegularPrice DECIMAL(10,2) NOT NULL DEFAULT 0,
    SellingPrice DECIMAL(10,2) NOT NULL DEFAULT 0,
    DiscountPercentage DECIMAL(5,2) NOT NULL DEFAULT 0,
    IsAvailable BIT DEFAULT 1,
    ImageUrl NVARCHAR(500),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL
);

-- Crear tabla de Items de Combo
CREATE TABLE ComboItems (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    ComboID INT NOT NULL,
    FastFoodItemID INT NOT NULL,
    Quantity INT NOT NULL DEFAULT 1,
    AllowCustomization BIT DEFAULT 0,
    SpecialInstructions NVARCHAR(200),
    FOREIGN KEY (ComboID) REFERENCES Combos(ID),
    FOREIGN KEY (FastFoodItemID) REFERENCES FastFoodItems(ID)
);

-- Crear tabla de Transacciones de Inventario
CREATE TABLE InventoryTransactions (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    IngredientID INT NOT NULL,
    UserID INT NOT NULL,
    TransactionType NVARCHAR(20) NOT NULL, -- Purchase, Consumption, Adjustment, Loss
    Quantity DECIMAL(10,2) NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL,
    Notes NVARCHAR(200),
    TransactionDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (IngredientID) REFERENCES Ingredients(ID),
    FOREIGN KEY (UserID) REFERENCES Users(ID)
);

-- Índices
CREATE INDEX IX_FastFoodItems_CategoryID ON FastFoodItems(CategoryID);
CREATE INDEX IX_FastFoodItems_ProductTypeID ON FastFoodItems(ProductTypeID);
CREATE INDEX IX_ItemIngredients_FastFoodItemID ON ItemIngredients(FastFoodItemID);
CREATE INDEX IX_ItemIngredients_IngredientID ON ItemIngredients(IngredientID);
CREATE INDEX IX_ComboItems_ComboID ON ComboItems(ComboID);
CREATE INDEX IX_ComboItems_FastFoodItemID ON ComboItems(FastFoodItemID);
CREATE INDEX IX_InventoryTransactions_IngredientID ON InventoryTransactions(IngredientID);
CREATE INDEX IX_InventoryTransactions_UserID ON InventoryTransactions(UserID);
CREATE INDEX IX_UserRoles_UserID ON UserRoles(UserID);
CREATE INDEX IX_UserRoles_RoleID ON UserRoles(RoleID);

-- Triggers para UpdatedAt
GO
CREATE TRIGGER TR_Categories_UpdatedAt ON Categories
AFTER UPDATE AS
BEGIN
    UPDATE Categories
    SET UpdatedAt = GETDATE()
    FROM Categories c
    INNER JOIN inserted i ON c.ID = i.ID;
END

GO
CREATE TRIGGER TR_FastFoodItems_UpdatedAt ON FastFoodItems
AFTER UPDATE AS
BEGIN
    UPDATE FastFoodItems
    SET UpdatedAt = GETDATE()
    FROM FastFoodItems f
    INNER JOIN inserted i ON f.ID = i.ID;
END

GO
CREATE TRIGGER TR_Ingredients_UpdatedAt ON Ingredients
AFTER UPDATE AS
BEGIN
    UPDATE Ingredients
    SET UpdatedAt = GETDATE()
    FROM Ingredients ing
    INNER JOIN inserted i ON ing.ID = i.ID;
END

GO
CREATE TRIGGER TR_Combos_UpdatedAt ON Combos
AFTER UPDATE AS
BEGIN
    UPDATE Combos
    SET UpdatedAt = GETDATE()
    FROM Combos c
    INNER JOIN inserted i ON c.ID = i.ID;
END

-- Datos iniciales
INSERT INTO Roles (RoleName) VALUES 
    ('SuperAdmin'),
    ('Admin'),
    ('User');

-- Insertar un usuario SuperAdmin inicial
-- Nota: Reemplazar el hash con uno generado apropiadamente
INSERT INTO Users (Username, PasswordHash, FirstName, LastName, Email, IsActive) 
VALUES ('admin', 'hash_de_password', 'Admin', 'System', 'admin@system.com', 1);
INSERT INTO [dbo].[Users] 
([Username], [PasswordHash], [FirstName], [LastName], [Email]) 
VALUES 
('superadmin', '$2a$11$XEyJPaiE7dT2u3UnS4MGOOyXeH4.bosU3k/nJ9.TgJBWoCJh7w6ge', 'Super', 'Admin', 'admin@restaurant.com');
--Admin123!


INSERT INTO UserRoles (UserID, RoleID)
VALUES (1, 1); -- Asignar rol SuperAdmin al usuario admin

/***********************************************/
/*											   */
/*			  Ordenes y facturación		       */
/*											   */
/***********************************************/
-- Tabla de Clientes
CREATE TABLE Customers (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100),
    PhoneNumber NVARCHAR(20),
    IdentificationType NVARCHAR(20), -- Cédula física, jurídica, DIMEX, etc.
    IdentificationNumber NVARCHAR(30),
    Address NVARCHAR(200),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL
);

-- Tabla para Mesas del Restaurante
CREATE TABLE Tables (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    TableNumber INT NOT NULL UNIQUE,
    Capacity INT NOT NULL,
    Status NVARCHAR(20) DEFAULT 'Available', -- Available, Occupied, Reserved, Maintenance
    Location NVARCHAR(50), -- Section or area of restaurant
    IsActive BIT DEFAULT 1
);

-- Tabla de Órdenes
CREATE TABLE Orders (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    OrderNumber NVARCHAR(20) NOT NULL UNIQUE,
    CustomerID INT NULL, -- Puede ser NULL para clientes no registrados
    TableID INT NULL, -- NULL para órdenes para llevar
    UserID INT NOT NULL, -- Empleado que registró la orden
    OrderType NVARCHAR(20) NOT NULL, -- DineIn, TakeOut, Delivery
    OrderStatus NVARCHAR(20) NOT NULL, -- Pending, InProgress, Ready, Delivered, Cancelled
    PaymentStatus NVARCHAR(20) DEFAULT 'Pending', -- Pending, Paid, Partially Paid, Cancelled
    TotalAmount DECIMAL(10,2) NOT NULL DEFAULT 0,
    TaxAmount DECIMAL(10,2) NOT NULL DEFAULT 0,
    DiscountAmount DECIMAL(10,2) NOT NULL DEFAULT 0,
    Notes NVARCHAR(500),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    FOREIGN KEY (CustomerID) REFERENCES Customers(ID),
    FOREIGN KEY (TableID) REFERENCES Tables(ID),
    FOREIGN KEY (UserID) REFERENCES Users(ID)
);

-- Tabla de Detalles de Orden
CREATE TABLE OrderDetails (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    OrderID INT NOT NULL,
    ItemType NVARCHAR(20) NOT NULL, -- Product, Combo
    ItemID INT NOT NULL, -- FastFoodItemID o ComboID
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL,
    SubTotal DECIMAL(10,2) NOT NULL,
    Notes NVARCHAR(200),
    Status NVARCHAR(20) DEFAULT 'Pending', -- Pending, InPreparation, Ready, Delivered, Cancelled
    FOREIGN KEY (OrderID) REFERENCES Orders(ID)
);

-- Tabla de Detalles de Personalización (para guardar personalizaciones de productos)
CREATE TABLE OrderItemCustomizations (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    OrderDetailID INT NOT NULL,
    IngredientID INT NOT NULL,
    CustomizationType NVARCHAR(20) NOT NULL, -- Add, Remove, Extra
    Quantity INT NOT NULL DEFAULT 1,
    ExtraCharge DECIMAL(10,2) NOT NULL DEFAULT 0,
    FOREIGN KEY (OrderDetailID) REFERENCES OrderDetails(ID),
    FOREIGN KEY (IngredientID) REFERENCES Ingredients(ID)
);

-- Tabla de Pagos
CREATE TABLE Payments (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    OrderID INT NOT NULL,
    PaymentMethod NVARCHAR(50) NOT NULL, -- Cash, CreditCard, DebitCard, Transfer, Other
    Amount DECIMAL(10,2) NOT NULL,
    ReferenceNumber NVARCHAR(50),
    PaymentDate DATETIME DEFAULT GETDATE(),
    ProcessedBy INT NOT NULL, -- UserID que procesa el pago
    Notes NVARCHAR(200),
    FOREIGN KEY (OrderID) REFERENCES Orders(ID),
    FOREIGN KEY (ProcessedBy) REFERENCES Users(ID)
);

-- Tabla de Facturas Electrónicas
CREATE TABLE ElectronicInvoices (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    OrderID INT NOT NULL,
    InvoiceNumber NVARCHAR(50) NOT NULL,
    CustomerID INT NOT NULL,
    TotalAmount DECIMAL(10,2) NOT NULL,
    TaxAmount DECIMAL(10,2) NOT NULL,
    InvoiceXML NVARCHAR(MAX), -- XML completo de la factura
    InvoiceStatus NVARCHAR(20) NOT NULL, -- Generated, Sent, Accepted, Rejected
    CreationDate DATETIME DEFAULT GETDATE(),
    ResponseDate DATETIME NULL,
    ErrorDescription NVARCHAR(500),
    HaciendaConfirmationNumber NVARCHAR(100),
    FOREIGN KEY (OrderID) REFERENCES Orders(ID),
    FOREIGN KEY (CustomerID) REFERENCES Customers(ID)
);

-- Índices para optimizar consultas
CREATE INDEX IX_Orders_CustomerID ON Orders(CustomerID);
CREATE INDEX IX_Orders_TableID ON Orders(TableID);
CREATE INDEX IX_Orders_UserID ON Orders(UserID);
CREATE INDEX IX_Orders_OrderStatus ON Orders(OrderStatus);
CREATE INDEX IX_OrderDetails_OrderID ON OrderDetails(OrderID);
CREATE INDEX IX_Payments_OrderID ON Payments(OrderID);
CREATE INDEX IX_ElectronicInvoices_OrderID ON ElectronicInvoices(OrderID);
CREATE INDEX IX_ElectronicInvoices_CustomerID ON ElectronicInvoices(CustomerID);

-- Triggers para UpdatedAt
GO
CREATE TRIGGER TR_Customers_UpdatedAt ON Customers
AFTER UPDATE AS
BEGIN
    UPDATE Customers
    SET UpdatedAt = GETDATE()
    FROM Customers c
    INNER JOIN inserted i ON c.ID = i.ID;
END

GO
CREATE TRIGGER TR_Orders_UpdatedAt ON Orders
AFTER UPDATE AS
BEGIN
    UPDATE Orders
    SET UpdatedAt = GETDATE()
    FROM Orders o
    INNER JOIN inserted i ON o.ID = i.ID;
END