CREATE TABLE [dbo].[CheckInActivity]
(
[Id] [int] NOT NULL,
[Activity] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
