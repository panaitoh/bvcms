CREATE NONCLUSTERED INDEX [STATE_LOOKUP_CODE_IX] ON [lookup].[StateLookup] ([StateCode])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
