
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[ScheduleId](@day INT, @time datetime)
RETURNS INT
AS
BEGIN
	-- Declare the return variable here
	DECLARE @id INT
	
	-- Add the T-SQL statements to compute the return value here
	SELECT @id = (ISNULL(@day, 0) + 1) * 10000 + DATEPART(hour, @time) * 100 + DATEPART(mi, @time)

	-- Return the result of the function
	RETURN @id

END
GO
