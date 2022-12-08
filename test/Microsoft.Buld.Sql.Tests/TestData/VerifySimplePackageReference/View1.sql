-- Table1 definition comes from the reference project that will be added as package reference
CREATE VIEW [dbo].[View1] AS
SELECT c1, c2 FROM [$(RefProj)].[dbo].[Table1]
