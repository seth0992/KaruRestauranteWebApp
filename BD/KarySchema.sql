USE [dbWebAppRestauranteKaru]
GO
ALTER TABLE [dbo].[UserRoles] DROP CONSTRAINT [FK__UserRoles__UserI__5535A963]
GO
ALTER TABLE [dbo].[UserRoles] DROP CONSTRAINT [FK__UserRoles__RoleI__5629CD9C]
GO
ALTER TABLE [dbo].[RefreshTokens] DROP CONSTRAINT [FK__RefreshTo__UserI__59063A47]
GO
ALTER TABLE [dbo].[ProductInventory] DROP CONSTRAINT [FK__ProductIn__FastF__71D1E811]
GO
ALTER TABLE [dbo].[Payments] DROP CONSTRAINT [FK__Payments__Proces__3493CFA7]
GO
ALTER TABLE [dbo].[Payments] DROP CONSTRAINT [FK__Payments__OrderI__339FAB6E]
GO
ALTER TABLE [dbo].[Orders] DROP CONSTRAINT [FK__Orders__UserID__2645B050]
GO
ALTER TABLE [dbo].[Orders] DROP CONSTRAINT [FK__Orders__TableID__25518C17]
GO
ALTER TABLE [dbo].[Orders] DROP CONSTRAINT [FK__Orders__Customer__245D67DE]
GO
ALTER TABLE [dbo].[OrderItemCustomizations] DROP CONSTRAINT [FK__OrderItem__Order__2EDAF651]
GO
ALTER TABLE [dbo].[OrderItemCustomizations] DROP CONSTRAINT [FK__OrderItem__Ingre__2FCF1A8A]
GO
ALTER TABLE [dbo].[OrderDetails] DROP CONSTRAINT [FK__OrderDeta__Order__2A164134]
GO
ALTER TABLE [dbo].[ItemIngredients] DROP CONSTRAINT [FK__ItemIngre__Ingre__797309D9]
GO
ALTER TABLE [dbo].[ItemIngredients] DROP CONSTRAINT [FK__ItemIngre__FastF__787EE5A0]
GO
ALTER TABLE [dbo].[InventoryTransactions] DROP CONSTRAINT [FK__Inventory__UserI__0A9D95DB]
GO
ALTER TABLE [dbo].[InventoryTransactions] DROP CONSTRAINT [FK__Inventory__Ingre__09A971A2]
GO
ALTER TABLE [dbo].[FastFoodItems] DROP CONSTRAINT [FK__FastFoodI__Produ__6A30C649]
GO
ALTER TABLE [dbo].[FastFoodItems] DROP CONSTRAINT [FK__FastFoodI__Categ__693CA210]
GO
ALTER TABLE [dbo].[ElectronicInvoices] DROP CONSTRAINT [FK__Electroni__Order__3864608B]
GO
ALTER TABLE [dbo].[ElectronicInvoices] DROP CONSTRAINT [FK__Electroni__Custo__395884C4]
GO
ALTER TABLE [dbo].[ComboItems] DROP CONSTRAINT [FK__ComboItem__FastF__05D8E0BE]
GO
ALTER TABLE [dbo].[ComboItems] DROP CONSTRAINT [FK__ComboItem__Combo__04E4BC85]
GO
ALTER TABLE [dbo].[CashRegisterTransactions] DROP CONSTRAINT [FK__CashRegis__UserI__5D95E53A]
GO
ALTER TABLE [dbo].[CashRegisterTransactions] DROP CONSTRAINT [FK__CashRegis__Sessi__5CA1C101]
GO
ALTER TABLE [dbo].[CashRegisterTransactions] DROP CONSTRAINT [FK__CashRegis__Relat__5E8A0973]
GO
ALTER TABLE [dbo].[CashRegisterSessions] DROP CONSTRAINT [FK__CashRegis__Openi__55F4C372]
GO
ALTER TABLE [dbo].[CashRegisterSessions] DROP CONSTRAINT [FK__CashRegis__Closi__56E8E7AB]
GO
ALTER TABLE [dbo].[Users] DROP CONSTRAINT [DF__Users__CreatedAt__52593CB8]
GO
ALTER TABLE [dbo].[Users] DROP CONSTRAINT [DF__Users__IsActive__5165187F]
GO
ALTER TABLE [dbo].[Tables] DROP CONSTRAINT [DF__Tables__IsActive__1BC821DD]
GO
ALTER TABLE [dbo].[Tables] DROP CONSTRAINT [DF__Tables__Status__1AD3FDA4]
GO
ALTER TABLE [dbo].[ProductInventory] DROP CONSTRAINT [DF__ProductIn__Sugge__70DDC3D8]
GO
ALTER TABLE [dbo].[ProductInventory] DROP CONSTRAINT [DF__ProductIn__Purch__6FE99F9F]
GO
ALTER TABLE [dbo].[ProductInventory] DROP CONSTRAINT [DF__ProductIn__Minim__6EF57B66]
GO
ALTER TABLE [dbo].[ProductInventory] DROP CONSTRAINT [DF__ProductIn__Curre__6E01572D]
GO
ALTER TABLE [dbo].[Payments] DROP CONSTRAINT [DF__Payments__Paymen__32AB8735]
GO
ALTER TABLE [dbo].[Orders] DROP CONSTRAINT [DF__Orders__CreatedA__236943A5]
GO
ALTER TABLE [dbo].[Orders] DROP CONSTRAINT [DF__Orders__Discount__22751F6C]
GO
ALTER TABLE [dbo].[Orders] DROP CONSTRAINT [DF__Orders__TaxAmoun__2180FB33]
GO
ALTER TABLE [dbo].[Orders] DROP CONSTRAINT [DF__Orders__TotalAmo__208CD6FA]
GO
ALTER TABLE [dbo].[Orders] DROP CONSTRAINT [DF__Orders__PaymentS__1F98B2C1]
GO
ALTER TABLE [dbo].[OrderItemCustomizations] DROP CONSTRAINT [DF__OrderItem__Extra__2DE6D218]
GO
ALTER TABLE [dbo].[OrderItemCustomizations] DROP CONSTRAINT [DF__OrderItem__Quant__2CF2ADDF]
GO
ALTER TABLE [dbo].[OrderDetails] DROP CONSTRAINT [DF__OrderDeta__Statu__29221CFB]
GO
ALTER TABLE [dbo].[ItemIngredients] DROP CONSTRAINT [DF__ItemIngre__Extra__778AC167]
GO
ALTER TABLE [dbo].[ItemIngredients] DROP CONSTRAINT [DF__ItemIngre__CanBe__76969D2E]
GO
ALTER TABLE [dbo].[ItemIngredients] DROP CONSTRAINT [DF__ItemIngre__IsOpt__75A278F5]
GO
ALTER TABLE [dbo].[ItemIngredients] DROP CONSTRAINT [DF__ItemIngre__Quant__74AE54BC]
GO
ALTER TABLE [dbo].[InventoryTransactions] DROP CONSTRAINT [DF__Inventory__Trans__08B54D69]
GO
ALTER TABLE [dbo].[Ingredients] DROP CONSTRAINT [DF__Ingredien__Creat__5FB337D6]
GO
ALTER TABLE [dbo].[Ingredients] DROP CONSTRAINT [DF__Ingredien__IsAct__5EBF139D]
GO
ALTER TABLE [dbo].[Ingredients] DROP CONSTRAINT [DF__Ingredien__Purch__5DCAEF64]
GO
ALTER TABLE [dbo].[Ingredients] DROP CONSTRAINT [DF__Ingredien__Minim__5CD6CB2B]
GO
ALTER TABLE [dbo].[Ingredients] DROP CONSTRAINT [DF__Ingredien__Stock__5BE2A6F2]
GO
ALTER TABLE [dbo].[FastFoodItems] DROP CONSTRAINT [DF__FastFoodI__Creat__68487DD7]
GO
ALTER TABLE [dbo].[FastFoodItems] DROP CONSTRAINT [DF__FastFoodI__IsAva__6754599E]
GO
ALTER TABLE [dbo].[FastFoodItems] DROP CONSTRAINT [DF__FastFoodI__Estim__66603565]
GO
ALTER TABLE [dbo].[FastFoodItems] DROP CONSTRAINT [DF__FastFoodI__Selli__656C112C]
GO
ALTER TABLE [dbo].[ElectronicInvoices] DROP CONSTRAINT [DF__Electroni__Creat__37703C52]
GO
ALTER TABLE [dbo].[Customers] DROP CONSTRAINT [DF__Customers__Creat__17036CC0]
GO
ALTER TABLE [dbo].[Customers] DROP CONSTRAINT [DF__Customers__IsAct__160F4887]
GO
ALTER TABLE [dbo].[Combos] DROP CONSTRAINT [DF__Combos__CreatedA__00200768]
GO
ALTER TABLE [dbo].[Combos] DROP CONSTRAINT [DF__Combos__IsAvaila__7F2BE32F]
GO
ALTER TABLE [dbo].[Combos] DROP CONSTRAINT [DF__Combos__Discount__7E37BEF6]
GO
ALTER TABLE [dbo].[Combos] DROP CONSTRAINT [DF__Combos__SellingP__7D439ABD]
GO
ALTER TABLE [dbo].[Combos] DROP CONSTRAINT [DF__Combos__RegularP__7C4F7684]
GO
ALTER TABLE [dbo].[ComboItems] DROP CONSTRAINT [DF__ComboItem__Allow__03F0984C]
GO
ALTER TABLE [dbo].[ComboItems] DROP CONSTRAINT [DF__ComboItem__Quant__02FC7413]
GO
ALTER TABLE [dbo].[Categories] DROP CONSTRAINT [DF__Categorie__Creat__4AB81AF0]
GO
ALTER TABLE [dbo].[Categories] DROP CONSTRAINT [DF__Categorie__IsAct__49C3F6B7]
GO
ALTER TABLE [dbo].[CashRegisterTransactions] DROP CONSTRAINT [DF__CashRegis__Amoun__5BAD9CC8]
GO
ALTER TABLE [dbo].[CashRegisterTransactions] DROP CONSTRAINT [DF__CashRegis__Amoun__5AB9788F]
GO
ALTER TABLE [dbo].[CashRegisterTransactions] DROP CONSTRAINT [DF__CashRegis__Trans__59C55456]
GO
ALTER TABLE [dbo].[CashRegisterSessions] DROP CONSTRAINT [DF__CashRegis__Statu__55009F39]
GO
ALTER TABLE [dbo].[CashRegisterSessions] DROP CONSTRAINT [DF__CashRegis__Initi__540C7B00]
GO
ALTER TABLE [dbo].[CashRegisterSessions] DROP CONSTRAINT [DF__CashRegis__Initi__531856C7]
GO
ALTER TABLE [dbo].[CashRegisterSessions] DROP CONSTRAINT [DF__CashRegis__Initi__5224328E]
GO
ALTER TABLE [dbo].[CashRegisterSessions] DROP CONSTRAINT [DF__CashRegis__Initi__51300E55]
GO
ALTER TABLE [dbo].[CashRegisterSessions] DROP CONSTRAINT [DF__CashRegis__Initi__503BEA1C]
GO
ALTER TABLE [dbo].[CashRegisterSessions] DROP CONSTRAINT [DF__CashRegis__Initi__4F47C5E3]
GO
/****** Object:  Index [IX_UserRoles_UserID]    Script Date: 21/03/2025 09:15:05 a. m. ******/
DROP INDEX [IX_UserRoles_UserID] ON [dbo].[UserRoles]
GO
/****** Object:  Index [IX_UserRoles_RoleID]    Script Date: 21/03/2025 09:15:05 a. m. ******/
DROP INDEX [IX_UserRoles_RoleID] ON [dbo].[UserRoles]
GO
/****** Object:  Index [IX_Payments_OrderID]    Script Date: 21/03/2025 09:15:05 a. m. ******/
DROP INDEX [IX_Payments_OrderID] ON [dbo].[Payments]
GO
/****** Object:  Index [IX_Orders_UserID]    Script Date: 21/03/2025 09:15:05 a. m. ******/
DROP INDEX [IX_Orders_UserID] ON [dbo].[Orders]
GO
/****** Object:  Index [IX_Orders_TableID]    Script Date: 21/03/2025 09:15:05 a. m. ******/
DROP INDEX [IX_Orders_TableID] ON [dbo].[Orders]
GO
/****** Object:  Index [IX_Orders_OrderStatus]    Script Date: 21/03/2025 09:15:05 a. m. ******/
DROP INDEX [IX_Orders_OrderStatus] ON [dbo].[Orders]
GO
/****** Object:  Index [IX_Orders_CustomerID]    Script Date: 21/03/2025 09:15:05 a. m. ******/
DROP INDEX [IX_Orders_CustomerID] ON [dbo].[Orders]
GO
/****** Object:  Index [IX_OrderDetails_OrderID]    Script Date: 21/03/2025 09:15:05 a. m. ******/
DROP INDEX [IX_OrderDetails_OrderID] ON [dbo].[OrderDetails]
GO
/****** Object:  Index [IX_ItemIngredients_IngredientID]    Script Date: 21/03/2025 09:15:05 a. m. ******/
DROP INDEX [IX_ItemIngredients_IngredientID] ON [dbo].[ItemIngredients]
GO
/****** Object:  Index [IX_ItemIngredients_FastFoodItemID]    Script Date: 21/03/2025 09:15:05 a. m. ******/
DROP INDEX [IX_ItemIngredients_FastFoodItemID] ON [dbo].[ItemIngredients]
GO
/****** Object:  Index [IX_InventoryTransactions_UserID]    Script Date: 21/03/2025 09:15:05 a. m. ******/
DROP INDEX [IX_InventoryTransactions_UserID] ON [dbo].[InventoryTransactions]
GO
/****** Object:  Index [IX_InventoryTransactions_IngredientID]    Script Date: 21/03/2025 09:15:05 a. m. ******/
DROP INDEX [IX_InventoryTransactions_IngredientID] ON [dbo].[InventoryTransactions]
GO
/****** Object:  Index [IX_FastFoodItems_ProductTypeID]    Script Date: 21/03/2025 09:15:05 a. m. ******/
DROP INDEX [IX_FastFoodItems_ProductTypeID] ON [dbo].[FastFoodItems]
GO
/****** Object:  Index [IX_FastFoodItems_CategoryID]    Script Date: 21/03/2025 09:15:05 a. m. ******/
DROP INDEX [IX_FastFoodItems_CategoryID] ON [dbo].[FastFoodItems]
GO
/****** Object:  Index [IX_ElectronicInvoices_OrderID]    Script Date: 21/03/2025 09:15:05 a. m. ******/
DROP INDEX [IX_ElectronicInvoices_OrderID] ON [dbo].[ElectronicInvoices]
GO
/****** Object:  Index [IX_ElectronicInvoices_CustomerID]    Script Date: 21/03/2025 09:15:05 a. m. ******/
DROP INDEX [IX_ElectronicInvoices_CustomerID] ON [dbo].[ElectronicInvoices]
GO
/****** Object:  Index [IX_ComboItems_FastFoodItemID]    Script Date: 21/03/2025 09:15:05 a. m. ******/
DROP INDEX [IX_ComboItems_FastFoodItemID] ON [dbo].[ComboItems]
GO
/****** Object:  Index [IX_ComboItems_ComboID]    Script Date: 21/03/2025 09:15:05 a. m. ******/
DROP INDEX [IX_ComboItems_ComboID] ON [dbo].[ComboItems]
GO
/****** Object:  Index [IX_CashRegisterTransactions_TransactionType]    Script Date: 21/03/2025 09:15:05 a. m. ******/
DROP INDEX [IX_CashRegisterTransactions_TransactionType] ON [dbo].[CashRegisterTransactions]
GO
/****** Object:  Index [IX_CashRegisterTransactions_SessionID]    Script Date: 21/03/2025 09:15:05 a. m. ******/
DROP INDEX [IX_CashRegisterTransactions_SessionID] ON [dbo].[CashRegisterTransactions]
GO
/****** Object:  Index [IX_CashRegisterSessions_Status]    Script Date: 21/03/2025 09:15:05 a. m. ******/
DROP INDEX [IX_CashRegisterSessions_Status] ON [dbo].[CashRegisterSessions]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 21/03/2025 09:15:05 a. m. ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
DROP TABLE [dbo].[Users]
GO
/****** Object:  Table [dbo].[UserRoles]    Script Date: 21/03/2025 09:15:05 a. m. ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserRoles]') AND type in (N'U'))
DROP TABLE [dbo].[UserRoles]
GO
/****** Object:  Table [dbo].[Tables]    Script Date: 21/03/2025 09:15:05 a. m. ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tables]') AND type in (N'U'))
DROP TABLE [dbo].[Tables]
GO
/****** Object:  Table [dbo].[Roles]    Script Date: 21/03/2025 09:15:05 a. m. ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Roles]') AND type in (N'U'))
DROP TABLE [dbo].[Roles]
GO
/****** Object:  Table [dbo].[RefreshTokens]    Script Date: 21/03/2025 09:15:05 a. m. ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RefreshTokens]') AND type in (N'U'))
DROP TABLE [dbo].[RefreshTokens]
GO
/****** Object:  Table [dbo].[ProductTypes]    Script Date: 21/03/2025 09:15:05 a. m. ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProductTypes]') AND type in (N'U'))
DROP TABLE [dbo].[ProductTypes]
GO
/****** Object:  Table [dbo].[ProductInventory]    Script Date: 21/03/2025 09:15:05 a. m. ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProductInventory]') AND type in (N'U'))
DROP TABLE [dbo].[ProductInventory]
GO
/****** Object:  Table [dbo].[Payments]    Script Date: 21/03/2025 09:15:05 a. m. ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Payments]') AND type in (N'U'))
DROP TABLE [dbo].[Payments]
GO
/****** Object:  Table [dbo].[Orders]    Script Date: 21/03/2025 09:15:05 a. m. ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND type in (N'U'))
DROP TABLE [dbo].[Orders]
GO
/****** Object:  Table [dbo].[OrderItemCustomizations]    Script Date: 21/03/2025 09:15:05 a. m. ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrderItemCustomizations]') AND type in (N'U'))
DROP TABLE [dbo].[OrderItemCustomizations]
GO
/****** Object:  Table [dbo].[OrderDetails]    Script Date: 21/03/2025 09:15:05 a. m. ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrderDetails]') AND type in (N'U'))
DROP TABLE [dbo].[OrderDetails]
GO
/****** Object:  Table [dbo].[ItemIngredients]    Script Date: 21/03/2025 09:15:05 a. m. ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ItemIngredients]') AND type in (N'U'))
DROP TABLE [dbo].[ItemIngredients]
GO
/****** Object:  Table [dbo].[InventoryTransactions]    Script Date: 21/03/2025 09:15:05 a. m. ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InventoryTransactions]') AND type in (N'U'))
DROP TABLE [dbo].[InventoryTransactions]
GO
/****** Object:  Table [dbo].[Ingredients]    Script Date: 21/03/2025 09:15:05 a. m. ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Ingredients]') AND type in (N'U'))
DROP TABLE [dbo].[Ingredients]
GO
/****** Object:  Table [dbo].[FastFoodItems]    Script Date: 21/03/2025 09:15:05 a. m. ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FastFoodItems]') AND type in (N'U'))
DROP TABLE [dbo].[FastFoodItems]
GO
/****** Object:  Table [dbo].[ElectronicInvoices]    Script Date: 21/03/2025 09:15:05 a. m. ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ElectronicInvoices]') AND type in (N'U'))
DROP TABLE [dbo].[ElectronicInvoices]
GO
/****** Object:  Table [dbo].[Customers]    Script Date: 21/03/2025 09:15:05 a. m. ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Customers]') AND type in (N'U'))
DROP TABLE [dbo].[Customers]
GO
/****** Object:  Table [dbo].[Combos]    Script Date: 21/03/2025 09:15:05 a. m. ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Combos]') AND type in (N'U'))
DROP TABLE [dbo].[Combos]
GO
/****** Object:  Table [dbo].[ComboItems]    Script Date: 21/03/2025 09:15:05 a. m. ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ComboItems]') AND type in (N'U'))
DROP TABLE [dbo].[ComboItems]
GO
/****** Object:  Table [dbo].[Categories]    Script Date: 21/03/2025 09:15:05 a. m. ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Categories]') AND type in (N'U'))
DROP TABLE [dbo].[Categories]
GO
/****** Object:  Table [dbo].[CashRegisterTransactions]    Script Date: 21/03/2025 09:15:05 a. m. ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CashRegisterTransactions]') AND type in (N'U'))
DROP TABLE [dbo].[CashRegisterTransactions]
GO
/****** Object:  Table [dbo].[CashRegisterSessions]    Script Date: 21/03/2025 09:15:05 a. m. ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CashRegisterSessions]') AND type in (N'U'))
DROP TABLE [dbo].[CashRegisterSessions]
GO
USE [master]
GO
/****** Object:  Database [dbWebAppRestauranteKaru]    Script Date: 21/03/2025 09:15:05 a. m. ******/
DROP DATABASE [dbWebAppRestauranteKaru]
GO
/****** Object:  Database [dbWebAppRestauranteKaru]    Script Date: 21/03/2025 09:15:05 a. m. ******/
CREATE DATABASE [dbWebAppRestauranteKaru]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'dbWebAppRestauranteKaru', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\dbWebAppRestauranteKaru.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'dbWebAppRestauranteKaru_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\dbWebAppRestauranteKaru_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [dbWebAppRestauranteKaru] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [dbWebAppRestauranteKaru].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [dbWebAppRestauranteKaru] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [dbWebAppRestauranteKaru] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [dbWebAppRestauranteKaru] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [dbWebAppRestauranteKaru] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [dbWebAppRestauranteKaru] SET ARITHABORT OFF 
GO
ALTER DATABASE [dbWebAppRestauranteKaru] SET AUTO_CLOSE ON 
GO
ALTER DATABASE [dbWebAppRestauranteKaru] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [dbWebAppRestauranteKaru] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [dbWebAppRestauranteKaru] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [dbWebAppRestauranteKaru] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [dbWebAppRestauranteKaru] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [dbWebAppRestauranteKaru] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [dbWebAppRestauranteKaru] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [dbWebAppRestauranteKaru] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [dbWebAppRestauranteKaru] SET  ENABLE_BROKER 
GO
ALTER DATABASE [dbWebAppRestauranteKaru] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [dbWebAppRestauranteKaru] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [dbWebAppRestauranteKaru] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [dbWebAppRestauranteKaru] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [dbWebAppRestauranteKaru] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [dbWebAppRestauranteKaru] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [dbWebAppRestauranteKaru] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [dbWebAppRestauranteKaru] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [dbWebAppRestauranteKaru] SET  MULTI_USER 
GO
ALTER DATABASE [dbWebAppRestauranteKaru] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [dbWebAppRestauranteKaru] SET DB_CHAINING OFF 
GO
ALTER DATABASE [dbWebAppRestauranteKaru] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [dbWebAppRestauranteKaru] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [dbWebAppRestauranteKaru] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [dbWebAppRestauranteKaru] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [dbWebAppRestauranteKaru] SET QUERY_STORE = ON
GO
ALTER DATABASE [dbWebAppRestauranteKaru] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [dbWebAppRestauranteKaru]
GO
/****** Object:  Table [dbo].[CashRegisterSessions]    Script Date: 21/03/2025 09:15:05 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CashRegisterSessions](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[OpeningDate] [datetime] NOT NULL,
	[ClosingDate] [datetime] NULL,
	[OpeningUserID] [int] NOT NULL,
	[ClosingUserID] [int] NULL,
	[InitialAmountCRC] [decimal](10, 2) NOT NULL,
	[InitialAmountUSD] [decimal](10, 2) NOT NULL,
	[FinalAmountCRC] [decimal](10, 2) NULL,
	[FinalAmountUSD] [decimal](10, 2) NULL,
	[InitialBillsCRC] [decimal](10, 2) NOT NULL,
	[InitialCoinsCRC] [decimal](10, 2) NOT NULL,
	[InitialBillsUSD] [decimal](10, 2) NOT NULL,
	[InitialCoinsUSD] [decimal](10, 2) NOT NULL,
	[FinalBillsCRC] [decimal](10, 2) NULL,
	[FinalCoinsCRC] [decimal](10, 2) NULL,
	[FinalBillsUSD] [decimal](10, 2) NULL,
	[FinalCoinsUSD] [decimal](10, 2) NULL,
	[Status] [nvarchar](20) NOT NULL,
	[Notes] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CashRegisterTransactions]    Script Date: 21/03/2025 09:15:05 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CashRegisterTransactions](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SessionID] [int] NOT NULL,
	[UserID] [int] NOT NULL,
	[TransactionDate] [datetime] NOT NULL,
	[TransactionType] [nvarchar](20) NOT NULL,
	[Description] [nvarchar](200) NOT NULL,
	[AmountCRC] [decimal](10, 2) NOT NULL,
	[AmountUSD] [decimal](10, 2) NOT NULL,
	[PaymentMethod] [nvarchar](20) NOT NULL,
	[ReferenceNumber] [nvarchar](50) NULL,
	[RelatedOrderID] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Categories]    Script Date: 21/03/2025 09:15:05 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Categories](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](200) NULL,
	[IsActive] [bit] NULL,
	[CreatedAt] [datetime] NULL,
	[UpdatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ComboItems]    Script Date: 21/03/2025 09:15:05 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ComboItems](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ComboID] [int] NOT NULL,
	[FastFoodItemID] [int] NOT NULL,
	[Quantity] [int] NOT NULL,
	[AllowCustomization] [bit] NULL,
	[SpecialInstructions] [nvarchar](200) NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Combos]    Script Date: 21/03/2025 09:15:05 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Combos](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](200) NULL,
	[RegularPrice] [decimal](10, 2) NOT NULL,
	[SellingPrice] [decimal](10, 2) NOT NULL,
	[DiscountPercentage] [decimal](5, 2) NOT NULL,
	[IsAvailable] [bit] NULL,
	[ImageUrl] [nvarchar](500) NULL,
	[CreatedAt] [datetime] NULL,
	[UpdatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Customers]    Script Date: 21/03/2025 09:15:05 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Customers](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Email] [nvarchar](100) NULL,
	[PhoneNumber] [nvarchar](20) NULL,
	[IdentificationType] [nvarchar](20) NULL,
	[IdentificationNumber] [nvarchar](30) NULL,
	[Address] [nvarchar](200) NULL,
	[IsActive] [bit] NULL,
	[CreatedAt] [datetime] NULL,
	[UpdatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ElectronicInvoices]    Script Date: 21/03/2025 09:15:05 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ElectronicInvoices](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[OrderID] [int] NOT NULL,
	[InvoiceNumber] [nvarchar](50) NOT NULL,
	[CustomerID] [int] NOT NULL,
	[TotalAmount] [decimal](10, 2) NOT NULL,
	[TaxAmount] [decimal](10, 2) NOT NULL,
	[InvoiceXML] [nvarchar](max) NULL,
	[InvoiceStatus] [nvarchar](20) NOT NULL,
	[CreationDate] [datetime] NULL,
	[ResponseDate] [datetime] NULL,
	[ErrorDescription] [nvarchar](500) NULL,
	[HaciendaConfirmationNumber] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FastFoodItems]    Script Date: 21/03/2025 09:15:05 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FastFoodItems](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](200) NULL,
	[CategoryID] [int] NOT NULL,
	[SellingPrice] [decimal](10, 2) NOT NULL,
	[EstimatedCost] [decimal](10, 2) NOT NULL,
	[ProductTypeID] [int] NOT NULL,
	[IsAvailable] [bit] NULL,
	[ImageUrl] [nvarchar](500) NULL,
	[EstimatedPreparationTime] [int] NULL,
	[CreatedAt] [datetime] NULL,
	[UpdatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Ingredients]    Script Date: 21/03/2025 09:15:05 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Ingredients](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](200) NULL,
	[StockQuantity] [decimal](10, 2) NOT NULL,
	[UnitOfMeasure] [nvarchar](20) NOT NULL,
	[MinimumStock] [decimal](10, 2) NOT NULL,
	[PurchasePrice] [decimal](10, 2) NOT NULL,
	[LastRestockDate] [datetime] NULL,
	[IsActive] [bit] NULL,
	[CreatedAt] [datetime] NULL,
	[UpdatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[InventoryTransactions]    Script Date: 21/03/2025 09:15:05 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[InventoryTransactions](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[IngredientID] [int] NOT NULL,
	[UserID] [int] NOT NULL,
	[TransactionType] [nvarchar](20) NOT NULL,
	[Quantity] [decimal](10, 2) NOT NULL,
	[UnitPrice] [decimal](10, 2) NOT NULL,
	[Notes] [nvarchar](200) NULL,
	[TransactionDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ItemIngredients]    Script Date: 21/03/2025 09:15:05 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ItemIngredients](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FastFoodItemID] [int] NOT NULL,
	[IngredientID] [int] NOT NULL,
	[Quantity] [decimal](10, 2) NOT NULL,
	[IsOptional] [bit] NULL,
	[CanBeExtra] [bit] NULL,
	[ExtraPrice] [decimal](10, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderDetails]    Script Date: 21/03/2025 09:15:05 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderDetails](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[OrderID] [int] NOT NULL,
	[ItemType] [nvarchar](20) NOT NULL,
	[ItemID] [int] NOT NULL,
	[Quantity] [int] NOT NULL,
	[UnitPrice] [decimal](10, 2) NOT NULL,
	[SubTotal] [decimal](10, 2) NOT NULL,
	[Notes] [nvarchar](200) NULL,
	[Status] [nvarchar](20) NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderItemCustomizations]    Script Date: 21/03/2025 09:15:05 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderItemCustomizations](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[OrderDetailID] [int] NOT NULL,
	[IngredientID] [int] NOT NULL,
	[CustomizationType] [nvarchar](20) NOT NULL,
	[Quantity] [int] NOT NULL,
	[ExtraCharge] [decimal](10, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Orders]    Script Date: 21/03/2025 09:15:05 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Orders](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[OrderNumber] [nvarchar](20) NOT NULL,
	[CustomerID] [int] NULL,
	[TableID] [int] NULL,
	[UserID] [int] NOT NULL,
	[OrderType] [nvarchar](20) NOT NULL,
	[OrderStatus] [nvarchar](20) NOT NULL,
	[PaymentStatus] [nvarchar](20) NULL,
	[TotalAmount] [decimal](10, 2) NOT NULL,
	[TaxAmount] [decimal](10, 2) NOT NULL,
	[DiscountAmount] [decimal](10, 2) NOT NULL,
	[Notes] [nvarchar](500) NULL,
	[CreatedAt] [datetime] NULL,
	[UpdatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[OrderNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Payments]    Script Date: 21/03/2025 09:15:05 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Payments](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[OrderID] [int] NOT NULL,
	[PaymentMethod] [nvarchar](50) NOT NULL,
	[Amount] [decimal](10, 2) NOT NULL,
	[ReferenceNumber] [nvarchar](50) NULL,
	[PaymentDate] [datetime] NULL,
	[ProcessedBy] [int] NOT NULL,
	[Notes] [nvarchar](200) NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductInventory]    Script Date: 21/03/2025 09:15:05 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductInventory](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FastFoodItemID] [int] NOT NULL,
	[CurrentStock] [int] NOT NULL,
	[MinimumStock] [int] NOT NULL,
	[PurchasePrice] [decimal](10, 2) NOT NULL,
	[SuggestedMarkup] [decimal](5, 2) NOT NULL,
	[LastRestockDate] [datetime] NULL,
	[SKU] [nvarchar](50) NULL,
	[UnitOfMeasure] [nvarchar](20) NULL,
	[LocationCode] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[FastFoodItemID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductTypes]    Script Date: 21/03/2025 09:15:05 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductTypes](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RefreshTokens]    Script Date: 21/03/2025 09:15:05 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RefreshTokens](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[RefreshToken] [nvarchar](500) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Roles]    Script Date: 21/03/2025 09:15:05 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Roles](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[RoleName] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Tables]    Script Date: 21/03/2025 09:15:05 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tables](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TableNumber] [int] NOT NULL,
	[Capacity] [int] NOT NULL,
	[Status] [nvarchar](20) NULL,
	[Location] [nvarchar](50) NULL,
	[IsActive] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[TableNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserRoles]    Script Date: 21/03/2025 09:15:05 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserRoles](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[RoleID] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 21/03/2025 09:15:05 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Username] [nvarchar](50) NOT NULL,
	[PasswordHash] [nvarchar](max) NOT NULL,
	[FirstName] [nvarchar](100) NOT NULL,
	[LastName] [nvarchar](100) NOT NULL,
	[Email] [nvarchar](100) NOT NULL,
	[IsActive] [bit] NULL,
	[CreatedAt] [datetime] NULL,
	[LastLogin] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Username] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_CashRegisterSessions_Status]    Script Date: 21/03/2025 09:15:05 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_CashRegisterSessions_Status] ON [dbo].[CashRegisterSessions]
(
	[Status] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CashRegisterTransactions_SessionID]    Script Date: 21/03/2025 09:15:05 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_CashRegisterTransactions_SessionID] ON [dbo].[CashRegisterTransactions]
(
	[SessionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_CashRegisterTransactions_TransactionType]    Script Date: 21/03/2025 09:15:05 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_CashRegisterTransactions_TransactionType] ON [dbo].[CashRegisterTransactions]
(
	[TransactionType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ComboItems_ComboID]    Script Date: 21/03/2025 09:15:05 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_ComboItems_ComboID] ON [dbo].[ComboItems]
(
	[ComboID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ComboItems_FastFoodItemID]    Script Date: 21/03/2025 09:15:05 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_ComboItems_FastFoodItemID] ON [dbo].[ComboItems]
(
	[FastFoodItemID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ElectronicInvoices_CustomerID]    Script Date: 21/03/2025 09:15:05 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_ElectronicInvoices_CustomerID] ON [dbo].[ElectronicInvoices]
(
	[CustomerID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ElectronicInvoices_OrderID]    Script Date: 21/03/2025 09:15:05 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_ElectronicInvoices_OrderID] ON [dbo].[ElectronicInvoices]
(
	[OrderID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_FastFoodItems_CategoryID]    Script Date: 21/03/2025 09:15:05 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_FastFoodItems_CategoryID] ON [dbo].[FastFoodItems]
(
	[CategoryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_FastFoodItems_ProductTypeID]    Script Date: 21/03/2025 09:15:05 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_FastFoodItems_ProductTypeID] ON [dbo].[FastFoodItems]
(
	[ProductTypeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_InventoryTransactions_IngredientID]    Script Date: 21/03/2025 09:15:05 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_InventoryTransactions_IngredientID] ON [dbo].[InventoryTransactions]
(
	[IngredientID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_InventoryTransactions_UserID]    Script Date: 21/03/2025 09:15:05 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_InventoryTransactions_UserID] ON [dbo].[InventoryTransactions]
(
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ItemIngredients_FastFoodItemID]    Script Date: 21/03/2025 09:15:05 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_ItemIngredients_FastFoodItemID] ON [dbo].[ItemIngredients]
(
	[FastFoodItemID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ItemIngredients_IngredientID]    Script Date: 21/03/2025 09:15:05 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_ItemIngredients_IngredientID] ON [dbo].[ItemIngredients]
(
	[IngredientID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_OrderDetails_OrderID]    Script Date: 21/03/2025 09:15:05 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_OrderDetails_OrderID] ON [dbo].[OrderDetails]
(
	[OrderID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Orders_CustomerID]    Script Date: 21/03/2025 09:15:05 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_Orders_CustomerID] ON [dbo].[Orders]
(
	[CustomerID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Orders_OrderStatus]    Script Date: 21/03/2025 09:15:05 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_Orders_OrderStatus] ON [dbo].[Orders]
(
	[OrderStatus] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Orders_TableID]    Script Date: 21/03/2025 09:15:05 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_Orders_TableID] ON [dbo].[Orders]
(
	[TableID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Orders_UserID]    Script Date: 21/03/2025 09:15:05 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_Orders_UserID] ON [dbo].[Orders]
(
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Payments_OrderID]    Script Date: 21/03/2025 09:15:05 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_Payments_OrderID] ON [dbo].[Payments]
(
	[OrderID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_UserRoles_RoleID]    Script Date: 21/03/2025 09:15:05 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_UserRoles_RoleID] ON [dbo].[UserRoles]
(
	[RoleID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_UserRoles_UserID]    Script Date: 21/03/2025 09:15:05 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_UserRoles_UserID] ON [dbo].[UserRoles]
(
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[CashRegisterSessions] ADD  DEFAULT ((0)) FOR [InitialAmountCRC]
GO
ALTER TABLE [dbo].[CashRegisterSessions] ADD  DEFAULT ((0)) FOR [InitialAmountUSD]
GO
ALTER TABLE [dbo].[CashRegisterSessions] ADD  DEFAULT ((0)) FOR [InitialBillsCRC]
GO
ALTER TABLE [dbo].[CashRegisterSessions] ADD  DEFAULT ((0)) FOR [InitialCoinsCRC]
GO
ALTER TABLE [dbo].[CashRegisterSessions] ADD  DEFAULT ((0)) FOR [InitialBillsUSD]
GO
ALTER TABLE [dbo].[CashRegisterSessions] ADD  DEFAULT ((0)) FOR [InitialCoinsUSD]
GO
ALTER TABLE [dbo].[CashRegisterSessions] ADD  DEFAULT ('Open') FOR [Status]
GO
ALTER TABLE [dbo].[CashRegisterTransactions] ADD  DEFAULT (getdate()) FOR [TransactionDate]
GO
ALTER TABLE [dbo].[CashRegisterTransactions] ADD  DEFAULT ((0)) FOR [AmountCRC]
GO
ALTER TABLE [dbo].[CashRegisterTransactions] ADD  DEFAULT ((0)) FOR [AmountUSD]
GO
ALTER TABLE [dbo].[Categories] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Categories] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[ComboItems] ADD  DEFAULT ((1)) FOR [Quantity]
GO
ALTER TABLE [dbo].[ComboItems] ADD  DEFAULT ((0)) FOR [AllowCustomization]
GO
ALTER TABLE [dbo].[Combos] ADD  DEFAULT ((0)) FOR [RegularPrice]
GO
ALTER TABLE [dbo].[Combos] ADD  DEFAULT ((0)) FOR [SellingPrice]
GO
ALTER TABLE [dbo].[Combos] ADD  DEFAULT ((0)) FOR [DiscountPercentage]
GO
ALTER TABLE [dbo].[Combos] ADD  DEFAULT ((1)) FOR [IsAvailable]
GO
ALTER TABLE [dbo].[Combos] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Customers] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Customers] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[ElectronicInvoices] ADD  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[FastFoodItems] ADD  DEFAULT ((0)) FOR [SellingPrice]
GO
ALTER TABLE [dbo].[FastFoodItems] ADD  DEFAULT ((0)) FOR [EstimatedCost]
GO
ALTER TABLE [dbo].[FastFoodItems] ADD  DEFAULT ((1)) FOR [IsAvailable]
GO
ALTER TABLE [dbo].[FastFoodItems] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Ingredients] ADD  DEFAULT ((0)) FOR [StockQuantity]
GO
ALTER TABLE [dbo].[Ingredients] ADD  DEFAULT ((0)) FOR [MinimumStock]
GO
ALTER TABLE [dbo].[Ingredients] ADD  DEFAULT ((0)) FOR [PurchasePrice]
GO
ALTER TABLE [dbo].[Ingredients] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Ingredients] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[InventoryTransactions] ADD  DEFAULT (getdate()) FOR [TransactionDate]
GO
ALTER TABLE [dbo].[ItemIngredients] ADD  DEFAULT ((0)) FOR [Quantity]
GO
ALTER TABLE [dbo].[ItemIngredients] ADD  DEFAULT ((0)) FOR [IsOptional]
GO
ALTER TABLE [dbo].[ItemIngredients] ADD  DEFAULT ((0)) FOR [CanBeExtra]
GO
ALTER TABLE [dbo].[ItemIngredients] ADD  DEFAULT ((0)) FOR [ExtraPrice]
GO
ALTER TABLE [dbo].[OrderDetails] ADD  DEFAULT ('Pending') FOR [Status]
GO
ALTER TABLE [dbo].[OrderItemCustomizations] ADD  DEFAULT ((1)) FOR [Quantity]
GO
ALTER TABLE [dbo].[OrderItemCustomizations] ADD  DEFAULT ((0)) FOR [ExtraCharge]
GO
ALTER TABLE [dbo].[Orders] ADD  DEFAULT ('Pending') FOR [PaymentStatus]
GO
ALTER TABLE [dbo].[Orders] ADD  DEFAULT ((0)) FOR [TotalAmount]
GO
ALTER TABLE [dbo].[Orders] ADD  DEFAULT ((0)) FOR [TaxAmount]
GO
ALTER TABLE [dbo].[Orders] ADD  DEFAULT ((0)) FOR [DiscountAmount]
GO
ALTER TABLE [dbo].[Orders] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Payments] ADD  DEFAULT (getdate()) FOR [PaymentDate]
GO
ALTER TABLE [dbo].[ProductInventory] ADD  DEFAULT ((0)) FOR [CurrentStock]
GO
ALTER TABLE [dbo].[ProductInventory] ADD  DEFAULT ((0)) FOR [MinimumStock]
GO
ALTER TABLE [dbo].[ProductInventory] ADD  DEFAULT ((0)) FOR [PurchasePrice]
GO
ALTER TABLE [dbo].[ProductInventory] ADD  DEFAULT ((0)) FOR [SuggestedMarkup]
GO
ALTER TABLE [dbo].[Tables] ADD  DEFAULT ('Available') FOR [Status]
GO
ALTER TABLE [dbo].[Tables] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[CashRegisterSessions]  WITH CHECK ADD FOREIGN KEY([ClosingUserID])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[CashRegisterSessions]  WITH CHECK ADD FOREIGN KEY([OpeningUserID])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[CashRegisterTransactions]  WITH CHECK ADD FOREIGN KEY([RelatedOrderID])
REFERENCES [dbo].[Orders] ([ID])
GO
ALTER TABLE [dbo].[CashRegisterTransactions]  WITH CHECK ADD FOREIGN KEY([SessionID])
REFERENCES [dbo].[CashRegisterSessions] ([ID])
GO
ALTER TABLE [dbo].[CashRegisterTransactions]  WITH CHECK ADD FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[ComboItems]  WITH CHECK ADD FOREIGN KEY([ComboID])
REFERENCES [dbo].[Combos] ([ID])
GO
ALTER TABLE [dbo].[ComboItems]  WITH CHECK ADD FOREIGN KEY([FastFoodItemID])
REFERENCES [dbo].[FastFoodItems] ([ID])
GO
ALTER TABLE [dbo].[ElectronicInvoices]  WITH CHECK ADD FOREIGN KEY([CustomerID])
REFERENCES [dbo].[Customers] ([ID])
GO
ALTER TABLE [dbo].[ElectronicInvoices]  WITH CHECK ADD FOREIGN KEY([OrderID])
REFERENCES [dbo].[Orders] ([ID])
GO
ALTER TABLE [dbo].[FastFoodItems]  WITH CHECK ADD FOREIGN KEY([CategoryID])
REFERENCES [dbo].[Categories] ([ID])
GO
ALTER TABLE [dbo].[FastFoodItems]  WITH CHECK ADD FOREIGN KEY([ProductTypeID])
REFERENCES [dbo].[ProductTypes] ([ID])
GO
ALTER TABLE [dbo].[InventoryTransactions]  WITH CHECK ADD FOREIGN KEY([IngredientID])
REFERENCES [dbo].[Ingredients] ([ID])
GO
ALTER TABLE [dbo].[InventoryTransactions]  WITH CHECK ADD FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[ItemIngredients]  WITH CHECK ADD FOREIGN KEY([FastFoodItemID])
REFERENCES [dbo].[FastFoodItems] ([ID])
GO
ALTER TABLE [dbo].[ItemIngredients]  WITH CHECK ADD FOREIGN KEY([IngredientID])
REFERENCES [dbo].[Ingredients] ([ID])
GO
ALTER TABLE [dbo].[OrderDetails]  WITH CHECK ADD FOREIGN KEY([OrderID])
REFERENCES [dbo].[Orders] ([ID])
GO
ALTER TABLE [dbo].[OrderItemCustomizations]  WITH CHECK ADD FOREIGN KEY([IngredientID])
REFERENCES [dbo].[Ingredients] ([ID])
GO
ALTER TABLE [dbo].[OrderItemCustomizations]  WITH CHECK ADD FOREIGN KEY([OrderDetailID])
REFERENCES [dbo].[OrderDetails] ([ID])
GO
ALTER TABLE [dbo].[Orders]  WITH CHECK ADD FOREIGN KEY([CustomerID])
REFERENCES [dbo].[Customers] ([ID])
GO
ALTER TABLE [dbo].[Orders]  WITH CHECK ADD FOREIGN KEY([TableID])
REFERENCES [dbo].[Tables] ([ID])
GO
ALTER TABLE [dbo].[Orders]  WITH CHECK ADD FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[Payments]  WITH CHECK ADD FOREIGN KEY([OrderID])
REFERENCES [dbo].[Orders] ([ID])
GO
ALTER TABLE [dbo].[Payments]  WITH CHECK ADD FOREIGN KEY([ProcessedBy])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[ProductInventory]  WITH CHECK ADD FOREIGN KEY([FastFoodItemID])
REFERENCES [dbo].[FastFoodItems] ([ID])
GO
ALTER TABLE [dbo].[RefreshTokens]  WITH CHECK ADD FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[UserRoles]  WITH CHECK ADD FOREIGN KEY([RoleID])
REFERENCES [dbo].[Roles] ([ID])
GO
ALTER TABLE [dbo].[UserRoles]  WITH CHECK ADD FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([ID])
GO
USE [master]
GO
ALTER DATABASE [dbWebAppRestauranteKaru] SET  READ_WRITE 
GO
