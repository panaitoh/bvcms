CREATE NONCLUSTERED INDEX [POSTAL_LOOKUP_CODE_IX] ON [lookup].[PostalLookup] ([PostalCode])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
