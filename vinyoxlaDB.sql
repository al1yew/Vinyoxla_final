USE [master]
GO
/****** Object:  Database [Vinyoxla]    Script Date: 2/12/2023 12:15:33 AM ******/
CREATE DATABASE [Vinyoxla]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'Vinyoxla', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\Vinyoxla.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'Vinyoxla_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\Vinyoxla_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT
GO
ALTER DATABASE [Vinyoxla] SET COMPATIBILITY_LEVEL = 150
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [Vinyoxla].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [Vinyoxla] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [Vinyoxla] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [Vinyoxla] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [Vinyoxla] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [Vinyoxla] SET ARITHABORT OFF 
GO
ALTER DATABASE [Vinyoxla] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [Vinyoxla] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [Vinyoxla] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [Vinyoxla] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [Vinyoxla] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [Vinyoxla] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [Vinyoxla] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [Vinyoxla] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [Vinyoxla] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [Vinyoxla] SET  ENABLE_BROKER 
GO
ALTER DATABASE [Vinyoxla] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [Vinyoxla] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [Vinyoxla] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [Vinyoxla] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [Vinyoxla] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [Vinyoxla] SET READ_COMMITTED_SNAPSHOT ON 
GO
ALTER DATABASE [Vinyoxla] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [Vinyoxla] SET RECOVERY FULL 
GO
ALTER DATABASE [Vinyoxla] SET  MULTI_USER 
GO
ALTER DATABASE [Vinyoxla] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [Vinyoxla] SET DB_CHAINING OFF 
GO
ALTER DATABASE [Vinyoxla] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [Vinyoxla] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [Vinyoxla] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [Vinyoxla] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
EXEC sys.sp_db_vardecimal_storage_format N'Vinyoxla', N'ON'
GO
ALTER DATABASE [Vinyoxla] SET QUERY_STORE = OFF
GO
USE [Vinyoxla]
GO
/****** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: 2/12/2023 12:15:34 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AppUserToVincodes]    Script Date: 2/12/2023 12:15:34 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AppUserToVincodes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AppUserId] [nvarchar](450) NULL,
	[VinCodeId] [int] NOT NULL,
	[CreatedAt] [datetime2](7) NULL,
 CONSTRAINT [PK_AppUserToVincodes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetRoleClaims]    Script Date: 2/12/2023 12:15:34 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoleClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RoleId] [nvarchar](450) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetRoles]    Script Date: 2/12/2023 12:15:34 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoles](
	[Id] [nvarchar](450) NOT NULL,
	[Name] [nvarchar](256) NULL,
	[NormalizedName] [nvarchar](256) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetRoles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserClaims]    Script Date: 2/12/2023 12:15:34 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](450) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserLogins]    Script Date: 2/12/2023 12:15:34 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserLogins](
	[LoginProvider] [nvarchar](450) NOT NULL,
	[ProviderKey] [nvarchar](450) NOT NULL,
	[ProviderDisplayName] [nvarchar](max) NULL,
	[UserId] [nvarchar](450) NOT NULL,
 CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY CLUSTERED 
(
	[LoginProvider] ASC,
	[ProviderKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserRoles]    Script Date: 2/12/2023 12:15:34 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserRoles](
	[UserId] [nvarchar](450) NOT NULL,
	[RoleId] [nvarchar](450) NOT NULL,
 CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUsers]    Script Date: 2/12/2023 12:15:34 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUsers](
	[Id] [nvarchar](450) NOT NULL,
	[UserName] [nvarchar](256) NULL,
	[NormalizedUserName] [nvarchar](256) NULL,
	[Email] [nvarchar](256) NULL,
	[NormalizedEmail] [nvarchar](256) NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PasswordHash] [nvarchar](max) NULL,
	[SecurityStamp] [nvarchar](max) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
	[PhoneNumber] [nvarchar](max) NOT NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[LockoutEnd] [datetimeoffset](7) NULL,
	[LockoutEnabled] [bit] NOT NULL,
	[AccessFailedCount] [int] NOT NULL,
	[Balance] [int] NOT NULL,
	[IsAdmin] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NULL,
 CONSTRAINT [PK_AspNetUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserTokens]    Script Date: 2/12/2023 12:15:34 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserTokens](
	[UserId] [nvarchar](450) NOT NULL,
	[LoginProvider] [nvarchar](450) NOT NULL,
	[Name] [nvarchar](450) NOT NULL,
	[Value] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[LoginProvider] ASC,
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EventMessages]    Script Date: 2/12/2023 12:15:34 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EventMessages](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CreatedAt] [datetime2](7) NULL,
	[Message] [nvarchar](max) NULL,
	[EventId] [int] NOT NULL,
 CONSTRAINT [PK_EventMessages] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Events]    Script Date: 2/12/2023 12:15:34 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Events](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CreatedAt] [datetime2](7) NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[DidRefundToBalance] [bit] NOT NULL,
	[IsApiError] [bit] NOT NULL,
	[FileExists] [bit] NOT NULL,
	[IsFromApi] [bit] NOT NULL,
	[IsRenewedDueToExpire] [bit] NOT NULL,
	[IsRenewedDueToAbsence] [bit] NOT NULL,
	[ErrorWhileRenew] [bit] NOT NULL,
	[ErrorWhileReplace] [bit] NOT NULL,
	[Vin] [nvarchar](max) NULL,
	[AppUserId] [nvarchar](450) NULL,
	[IsFromAdminArea] [bit] NOT NULL,
 CONSTRAINT [PK_Events] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Transactions]    Script Date: 2/12/2023 12:15:34 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Transactions](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[AppUserId] [nvarchar](450) NULL,
	[Amount] [int] NOT NULL,
	[IsFromBalance] [bit] NOT NULL,
	[IsTopUp] [bit] NOT NULL,
	[PaymentIsSuccessful] [bit] NOT NULL,
	[OrderId] [nvarchar](max) NOT NULL,
	[SessionId] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_Transactions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[VinCodes]    Script Date: 2/12/2023 12:15:34 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VinCodes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[Vin] [nvarchar](17) NOT NULL,
	[FileName] [nvarchar](max) NOT NULL,
	[PurchasedTimes] [int] NOT NULL,
 CONSTRAINT [PK_VinCodes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20221226230606_Initial', N'3.1.32')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20221226230702_AddedTables', N'3.1.32')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20221227090125_AddedTransactionTableAndManyToManyTable', N'3.1.32')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20221227090532_AddedTransactionTableAndManyToManyTableV2', N'3.1.32')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20221227091009_AddedTransactionTableAndManyToManyTableV3', N'3.1.32')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20221228222640_UpdatedVIncodeTable', N'3.1.32')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20230102145426_updateduser', N'3.1.32')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20230103184001_updatedRelationalTable', N'3.1.32')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20230115224959_AddedEventTableAndUpdatedSomeTables', N'3.1.32')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20230130141458_UpdatedTransactionAndEvent', N'3.1.32')
GO
SET IDENTITY_INSERT [dbo].[AppUserToVincodes] ON 

INSERT [dbo].[AppUserToVincodes] ([Id], [AppUserId], [VinCodeId], [CreatedAt]) VALUES (1437, N'd3c86ad0-0961-4dc2-81ba-9f2c08b91681', 2231, CAST(N'2023-02-11T21:53:48.4458438' AS DateTime2))
INSERT [dbo].[AppUserToVincodes] ([Id], [AppUserId], [VinCodeId], [CreatedAt]) VALUES (1438, N'd3c86ad0-0961-4dc2-81ba-9f2c08b91681', 2232, CAST(N'2023-02-11T21:54:31.0346682' AS DateTime2))
INSERT [dbo].[AppUserToVincodes] ([Id], [AppUserId], [VinCodeId], [CreatedAt]) VALUES (1443, N'd3c86ad0-0961-4dc2-81ba-9f2c08b91681', 2233, CAST(N'2023-02-11T23:07:39.6083430' AS DateTime2))
SET IDENTITY_INSERT [dbo].[AppUserToVincodes] OFF
GO
INSERT [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'4f497643-c30c-4287-ac2e-842d37ba7a89', N'Admin', N'ADMIN', N'46feac74-4fb6-4a84-b90d-c451564a1bac')
INSERT [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'db23fce7-b355-4e56-8ba6-ddc993c88347', N'Member', N'MEMBER', N'5b467cff-52cc-4f6b-938a-ded20c86a70f')
GO
INSERT [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'd3c86ad0-0961-4dc2-81ba-9f2c08b91681', N'4f497643-c30c-4287-ac2e-842d37ba7a89')
INSERT [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'7f8aad40-0d43-400a-a8d5-cd39afdbfcaf', N'db23fce7-b355-4e56-8ba6-ddc993c88347')
INSERT [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'b6b8fe4f-e592-48ae-8408-30bb36a2734d', N'db23fce7-b355-4e56-8ba6-ddc993c88347')
INSERT [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'c14c6104-9ffe-4e4b-ba8e-d91f281e6df6', N'db23fce7-b355-4e56-8ba6-ddc993c88347')
GO
INSERT [dbo].[AspNetUsers] ([Id], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount], [Balance], [IsAdmin], [CreatedAt]) VALUES (N'7f8aad40-0d43-400a-a8d5-cd39afdbfcaf', N'+994666666666', N'+994666666666', NULL, NULL, 0, NULL, N'LCHNMPL2KVDFRB6W4OVUFC2FYG4FWRRN', N'9dd5fa25-bc59-4ab8-907c-5f88daee7732', N'+994666666666', 0, 0, NULL, 0, 0, 0, 0, CAST(N'2023-02-11T21:55:09.7430095' AS DateTime2))
INSERT [dbo].[AspNetUsers] ([Id], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount], [Balance], [IsAdmin], [CreatedAt]) VALUES (N'b6b8fe4f-e592-48ae-8408-30bb36a2734d', N'+994444444444', N'+994444444444', NULL, NULL, 0, NULL, N'TXUWLGLA25DU2NOBRKILI3RDHCCCQMQY', N'7afad2f2-2dbb-4382-8710-613d8291d960', N'+994444444444', 0, 0, NULL, 0, 0, 0, 0, CAST(N'2023-02-11T21:53:47.9177556' AS DateTime2))
INSERT [dbo].[AspNetUsers] ([Id], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount], [Balance], [IsAdmin], [CreatedAt]) VALUES (N'c14c6104-9ffe-4e4b-ba8e-d91f281e6df6', N'+994555555555', N'+994555555555', NULL, NULL, 0, NULL, N'7AWSLH57A4MNQ46GTKDP6KZ7MKD2DN5N', N'fc34eba4-0096-4cae-bc3c-84755ce70167', N'+994555555555', 0, 0, NULL, 0, 0, 0, 0, CAST(N'2023-02-11T21:54:30.6399173' AS DateTime2))
INSERT [dbo].[AspNetUsers] ([Id], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount], [Balance], [IsAdmin], [CreatedAt]) VALUES (N'd3c86ad0-0961-4dc2-81ba-9f2c08b91681', N'+994505788901', N'+994505788901', NULL, NULL, 0, N'AQAAAAEAACcQAAAAEF+Tg/kqH5VHojhZ6hB8dG8/6+jR74S2lh8Fndlrj4SuRYB9VHxLZmmFeOIN3lrp5w==', N'4IGURIYGOEUTJGIYQKRMGXR2NGKJLMV3', N'74aeb746-3252-4086-bbe2-42f96648d4aa', N'+994505788901', 1, 0, NULL, 0, 0, 0, 1, CAST(N'2023-01-30T18:18:25.5827777' AS DateTime2))
GO
SET IDENTITY_INSERT [dbo].[VinCodes] ON 

INSERT [dbo].[VinCodes] ([Id], [CreatedAt], [Vin], [FileName], [PurchasedTimes]) VALUES (2231, CAST(N'2023-02-01T21:51:53.0170111' AS DateTime2), N'1G1PF5SC3C7133311', N'1G1PF5SC3C7133311_92f278b4-066b-4baa-948a-b432143d528e.html', 1)
INSERT [dbo].[VinCodes] ([Id], [CreatedAt], [Vin], [FileName], [PurchasedTimes]) VALUES (2232, CAST(N'2023-02-01T23:08:35.7875504' AS DateTime2), N'4T1BG22K9YU930834', N'4T1BG22K9YU930834_9b269381-ceef-4d64-9950-d109803f9b01.html', 1)
INSERT [dbo].[VinCodes] ([Id], [CreatedAt], [Vin], [FileName], [PurchasedTimes]) VALUES (2233, CAST(N'2023-02-11T21:54:31.0346636' AS DateTime2), N'WAUDG74F25N111998', N'WAUDG74F25N111998_b1e9dda4-0f30-4b58-ac58-d706bc212bde.html', 1)
SET IDENTITY_INSERT [dbo].[VinCodes] OFF
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AppUserToVincodes_AppUserId]    Script Date: 2/12/2023 12:15:34 AM ******/
CREATE NONCLUSTERED INDEX [IX_AppUserToVincodes_AppUserId] ON [dbo].[AppUserToVincodes]
(
	[AppUserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_AppUserToVincodes_VinCodeId]    Script Date: 2/12/2023 12:15:34 AM ******/
CREATE NONCLUSTERED INDEX [IX_AppUserToVincodes_VinCodeId] ON [dbo].[AppUserToVincodes]
(
	[VinCodeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AspNetRoleClaims_RoleId]    Script Date: 2/12/2023 12:15:34 AM ******/
CREATE NONCLUSTERED INDEX [IX_AspNetRoleClaims_RoleId] ON [dbo].[AspNetRoleClaims]
(
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [RoleNameIndex]    Script Date: 2/12/2023 12:15:34 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [RoleNameIndex] ON [dbo].[AspNetRoles]
(
	[NormalizedName] ASC
)
WHERE ([NormalizedName] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AspNetUserClaims_UserId]    Script Date: 2/12/2023 12:15:34 AM ******/
CREATE NONCLUSTERED INDEX [IX_AspNetUserClaims_UserId] ON [dbo].[AspNetUserClaims]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AspNetUserLogins_UserId]    Script Date: 2/12/2023 12:15:34 AM ******/
CREATE NONCLUSTERED INDEX [IX_AspNetUserLogins_UserId] ON [dbo].[AspNetUserLogins]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AspNetUserRoles_RoleId]    Script Date: 2/12/2023 12:15:34 AM ******/
CREATE NONCLUSTERED INDEX [IX_AspNetUserRoles_RoleId] ON [dbo].[AspNetUserRoles]
(
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [EmailIndex]    Script Date: 2/12/2023 12:15:34 AM ******/
CREATE NONCLUSTERED INDEX [EmailIndex] ON [dbo].[AspNetUsers]
(
	[NormalizedEmail] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UserNameIndex]    Script Date: 2/12/2023 12:15:34 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex] ON [dbo].[AspNetUsers]
(
	[NormalizedUserName] ASC
)
WHERE ([NormalizedUserName] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_EventMessages_EventId]    Script Date: 2/12/2023 12:15:34 AM ******/
CREATE NONCLUSTERED INDEX [IX_EventMessages_EventId] ON [dbo].[EventMessages]
(
	[EventId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Events_AppUserId]    Script Date: 2/12/2023 12:15:34 AM ******/
CREATE NONCLUSTERED INDEX [IX_Events_AppUserId] ON [dbo].[Events]
(
	[AppUserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Transactions_AppUserId]    Script Date: 2/12/2023 12:15:34 AM ******/
CREATE NONCLUSTERED INDEX [IX_Transactions_AppUserId] ON [dbo].[Transactions]
(
	[AppUserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[AspNetUsers] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsAdmin]
GO
ALTER TABLE [dbo].[Events] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsFromAdminArea]
GO
ALTER TABLE [dbo].[Transactions] ADD  DEFAULT ((0)) FOR [Amount]
GO
ALTER TABLE [dbo].[Transactions] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsFromBalance]
GO
ALTER TABLE [dbo].[Transactions] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsTopUp]
GO
ALTER TABLE [dbo].[Transactions] ADD  DEFAULT (CONVERT([bit],(0))) FOR [PaymentIsSuccessful]
GO
ALTER TABLE [dbo].[Transactions] ADD  DEFAULT (N'') FOR [OrderId]
GO
ALTER TABLE [dbo].[Transactions] ADD  DEFAULT (N'') FOR [SessionId]
GO
ALTER TABLE [dbo].[VinCodes] ADD  DEFAULT ((0)) FOR [PurchasedTimes]
GO
ALTER TABLE [dbo].[AppUserToVincodes]  WITH CHECK ADD  CONSTRAINT [FK_AppUserToVincodes_AspNetUsers_AppUserId] FOREIGN KEY([AppUserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
GO
ALTER TABLE [dbo].[AppUserToVincodes] CHECK CONSTRAINT [FK_AppUserToVincodes_AspNetUsers_AppUserId]
GO
ALTER TABLE [dbo].[AppUserToVincodes]  WITH CHECK ADD  CONSTRAINT [FK_AppUserToVincodes_VinCodes_VinCodeId] FOREIGN KEY([VinCodeId])
REFERENCES [dbo].[VinCodes] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AppUserToVincodes] CHECK CONSTRAINT [FK_AppUserToVincodes_VinCodes_VinCodeId]
GO
ALTER TABLE [dbo].[AspNetRoleClaims]  WITH CHECK ADD  CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetRoleClaims] CHECK CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserClaims]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserClaims] CHECK CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserLogins]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserLogins] CHECK CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserTokens]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserTokens] CHECK CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[EventMessages]  WITH CHECK ADD  CONSTRAINT [FK_EventMessages_Events_EventId] FOREIGN KEY([EventId])
REFERENCES [dbo].[Events] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[EventMessages] CHECK CONSTRAINT [FK_EventMessages_Events_EventId]
GO
ALTER TABLE [dbo].[Events]  WITH CHECK ADD  CONSTRAINT [FK_Events_AspNetUsers_AppUserId] FOREIGN KEY([AppUserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
GO
ALTER TABLE [dbo].[Events] CHECK CONSTRAINT [FK_Events_AspNetUsers_AppUserId]
GO
ALTER TABLE [dbo].[Transactions]  WITH CHECK ADD  CONSTRAINT [FK_Transactions_AspNetUsers_AppUserId] FOREIGN KEY([AppUserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
GO
ALTER TABLE [dbo].[Transactions] CHECK CONSTRAINT [FK_Transactions_AspNetUsers_AppUserId]
GO
USE [master]
GO
ALTER DATABASE [Vinyoxla] SET  READ_WRITE 
GO
