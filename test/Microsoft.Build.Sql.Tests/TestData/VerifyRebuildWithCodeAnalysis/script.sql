CREATE PROCEDURE [dbo].[uspLogError]
@ErrorLogID int = 0 OUTPUT
AS
BEGIN
SET @ErrorLogID = @@IDENTITY;
END