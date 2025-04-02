USE [dbWebAppRestauranteKaru]
GO
SET IDENTITY_INSERT [dbo].[Roles] ON 
GO
INSERT [dbo].[Roles] ([ID], [RoleName]) VALUES (1, N'SuperAdmin')
GO
INSERT [dbo].[Roles] ([ID], [RoleName]) VALUES (2, N'Admin')
GO
INSERT [dbo].[Roles] ([ID], [RoleName]) VALUES (3, N'User')
GO
SET IDENTITY_INSERT [dbo].[Roles] OFF
GO
SET IDENTITY_INSERT [dbo].[Users] ON 
GO
INSERT [dbo].[Users] ([ID], [Username], [PasswordHash], [FirstName], [LastName], [Email], [IsActive], [CreatedAt], [LastLogin]) VALUES (1, N'superadmin', N'$2a$11$XEyJPaiE7dT2u3UnS4MGOOyXeH4.bosU3k/nJ9.TgJBWoCJh7w6ge', N'Super', N'Admin', N'admin@restaurant.com', 1, CAST(N'2025-02-21T23:43:56.233' AS DateTime), CAST(N'2025-03-19T19:02:09.177' AS DateTime))
GO
SET IDENTITY_INSERT [dbo].[Users] OFF
GO
SET IDENTITY_INSERT [dbo].[UserRoles] ON 
GO
INSERT [dbo].[UserRoles] ([ID], [UserID], [RoleID]) VALUES (2, 1, 1)
GO
SET IDENTITY_INSERT [dbo].[UserRoles] OFF
GO
