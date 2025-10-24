-- En PostgreSQL, puedes usar row-level locking
-- Este script es solo un ejemplo de cómo manejar concurrencia

-- Opción 1: SELECT FOR UPDATE (en el repositorio)
SELECT * FROM transaction_days 
WHERE source_account_id = @accountId 
  AND transaction_date::date = @date::date 
FOR UPDATE;

-- Opción 2: Usar transacciones con isolation level
-- Esto se configuraría en el DbContext