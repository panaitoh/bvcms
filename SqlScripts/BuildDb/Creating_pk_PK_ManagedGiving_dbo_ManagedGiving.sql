ALTER TABLE [dbo].[ManagedGiving] ADD CONSTRAINT [PK_ManagedGiving] PRIMARY KEY CLUSTERED  ([PeopleId])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
