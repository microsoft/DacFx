-- Table1 definition comes from dacpac reference
-- Dacpac is from different database which is why RefProj SQLCMD variable is needed
CREATE VIEW [dbo].[View1] AS
SELECT c1, c2 FROM [$(RefProj)].[dbo].[Table1]
