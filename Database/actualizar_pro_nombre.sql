-- Amplía pro_nombre para títulos largos (CSV Amazon, etc.)
USE dis_deber2_db;
GO

IF EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'tbl_producto'
      AND COLUMN_NAME = 'pro_nombre'
      AND CHARACTER_MAXIMUM_LENGTH < 500
)
BEGIN
    ALTER TABLE dbo.tbl_producto
        ALTER COLUMN pro_nombre VARCHAR(500) NOT NULL;
    PRINT 'Columna pro_nombre actualizada a VARCHAR(500).';
END
ELSE
    PRINT 'Columna pro_nombre ya tiene capacidad suficiente.';
GO
