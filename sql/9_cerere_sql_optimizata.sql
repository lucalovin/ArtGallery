-- USER: ART_GALLETY_OLTP

-- ============================================================================
-- 9a. PLANUL DE EXECUTIE ALES DE OPTIMIZORUL BAZAT PE COST (CBO)
--    (varianta de baza, fara optimizari explicite)
-- ============================================================================


-- Cererea de baza:
SELECT
  ex.TITLE                          AS exhibition_title,
  d.CALENDAR_YEAR,
  a.NAME                            AS artist_name,
  SUM(f.ESTIMATED_VALUE)            AS total_estimated_value,
  SUM(f.INSURED_AMOUNT)             AS total_insured_amount,
  COUNT(DISTINCT f.ARTWORK_KEY)     AS artwork_count,
  AVG(f.AVG_RATING)                 AS avg_rating
FROM FACT_EXHIBITION_ACTIVITY f
JOIN DIM_EXHIBITION ex
  ON ex.EXHIBITION_KEY = f.EXHIBITION_KEY
JOIN DIM_ARTIST a
  ON a.ARTIST_KEY = f.ARTIST_KEY
JOIN DIM_DATE d
  ON d.DATE_KEY = f.DATE_KEY
WHERE d.CALENDAR_YEAR BETWEEN 2022 AND 2024
GROUP BY
  ex.TITLE,
  d.CALENDAR_YEAR,
  a.NAME
ORDER BY
  d.CALENDAR_YEAR,
  ex.TITLE,
  total_insured_amount DESC;

-- Pas 0: Colectarea statisticilor pentru tabelele implicate
ANALYZE TABLE FACT_EXHIBITION_ACTIVITY COMPUTE STATISTICS;
ANALYZE TABLE DIM_EXHIBITION COMPUTE STATISTICS;
ANALYZE TABLE DIM_ARTIST COMPUTE STATISTICS;
ANALYZE TABLE DIM_DATE COMPUTE STATISTICS;

-- Pas 1: Generarea planului de executie pentru cererea de baza
EXPLAIN PLAN
SET STATEMENT_ID = 'cerinta9_baza'
FOR
SELECT
  ex.TITLE                          AS exhibition_title,
  d.CALENDAR_YEAR,
  a.NAME                            AS artist_name,
  SUM(f.ESTIMATED_VALUE)            AS total_estimated_value,
  SUM(f.INSURED_AMOUNT)             AS total_insured_amount,
  COUNT(DISTINCT f.ARTWORK_KEY)     AS artwork_count,
  AVG(f.AVG_RATING)                 AS avg_rating
FROM FACT_EXHIBITION_ACTIVITY f
JOIN DIM_EXHIBITION ex
  ON ex.EXHIBITION_KEY = f.EXHIBITION_KEY
JOIN DIM_ARTIST a
  ON a.ARTIST_KEY = f.ARTIST_KEY
JOIN DIM_DATE d
  ON d.DATE_KEY = f.DATE_KEY
WHERE d.CALENDAR_YEAR BETWEEN 2022 AND 2024
GROUP BY
  ex.TITLE,
  d.CALENDAR_YEAR,
  a.NAME
ORDER BY
  d.CALENDAR_YEAR,
  ex.TITLE,
  total_insured_amount DESC;

-- Pas 2: Afisarea planului de executie
SELECT plan_table_output
FROM table(dbms_xplan.display('plan_table', 'cerinta9_baza', 'serial'));

-- ============================================================================
-- EXPLICAREA ETAPELOR PLANULUI DE EXECUTIE (estimat):
-- ============================================================================
--
-- Planul de executie generat de CBO va parcurge, in mod tipic, urmatoarele
-- etape (citite de la cea mai indentata operatie spre radacina):
--
-- 1. TABLE ACCESS FULL pe FACT_EXHIBITION_ACTIVITY
--    - Optimizorul scaneaza integral tabela de fapte (full table scan)
--    - Aceasta este cea mai costisitoare operatie, deoarece tabela de
--      fapte contine cel mai mare volum de date
--
-- 2. TABLE ACCESS FULL pe DIM_DATE
--    - Scaneaza tabela DIM_DATE pentru a putea aplica filtrul
--      CALENDAR_YEAR BETWEEN 2022 AND 2024
--
-- 3. HASH JOIN intre FACT_EXHIBITION_ACTIVITY si DIM_DATE
--    - Optimizorul construieste un hash table pe dimensiunea mai mica
--      (DIM_DATE filtrata) si face probe cu randurile din tabela de fapte
--    - Filtrul pe CALENDAR_YEAR este aplicat aici, reducand setul de date
--
-- 4. TABLE ACCESS FULL pe DIM_EXHIBITION
--    - Scaneaza tabela dimensiune DIM_EXHIBITION
--
-- 5. HASH JOIN cu rezultatul anterior si DIM_EXHIBITION
--    - Face legatura pe EXHIBITION_KEY
--
-- 6. TABLE ACCESS FULL pe DIM_ARTIST
--    - Scaneaza tabela dimensiune DIM_ARTIST
--
-- 7. HASH JOIN cu rezultatul anterior si DIM_ARTIST
--    - Face legatura pe ARTIST_KEY
--
-- 8. HASH GROUP BY
--    - Grupeaza rezultatele pe (TITLE, CALENDAR_YEAR, NAME) si
--      calculeaza SUM, COUNT DISTINCT, AVG
--
-- 9. SORT ORDER BY
--    - Ordoneaza rezultatul final dupa CALENDAR_YEAR, TITLE,
--      total_insured_amount DESC
--
-- Observatii:
-- - Toate accesele la tabele sunt de tip FULL TABLE SCAN deoarece
--   nu exista indecsi pe coloanele de JOIN ale tabelei de fapte
-- - Costul principal provine din scanarea integrala a tabelei de fapte
-- - Hash Join este preferat de CBO pentru volume mari de date (tipic DW)
-- ============================================================================


-- ============================================================================
-- 9b. SUGESTII DE OPTIMIZARE A CERERII
-- ============================================================================

-- ---------------------------------------------------------------------------
-- OPTIMIZARE 1: Index B*Tree pe coloana DATE_KEY din tabela de fapte
-- ---------------------------------------------------------------------------
-- Justificare: cererea filtreaza pe un interval de ani, ceea ce se traduce
-- intr-un interval de valori DATE_KEY (20220101 - 20241231)
-- Un index B*Tree permite range scan in loc de full table scan.

CREATE INDEX idx_fact_date_key
ON FACT_EXHIBITION_ACTIVITY(DATE_KEY);

ANALYZE INDEX idx_fact_date_key COMPUTE STATISTICS;

-- Generarea planului cu hint de utilizare a indexului
EXPLAIN PLAN
SET STATEMENT_ID = 'cerinta9_optim1'
FOR
SELECT
  ex.TITLE                          AS exhibition_title,
  d.CALENDAR_YEAR,
  a.NAME                            AS artist_name,
  SUM(f.ESTIMATED_VALUE)            AS total_estimated_value,
  SUM(f.INSURED_AMOUNT)             AS total_insured_amount,
  COUNT(DISTINCT f.ARTWORK_KEY)     AS artwork_count,
  AVG(f.AVG_RATING)                 AS avg_rating
FROM (
  SELECT /*+ INDEX(f idx_fact_date_key) */
         *
  FROM FACT_EXHIBITION_ACTIVITY f
  WHERE DATE_KEY BETWEEN 20220101 AND 20241231
) f
JOIN DIM_DATE d
  ON d.DATE_KEY = f.DATE_KEY
JOIN DIM_EXHIBITION ex
  ON ex.EXHIBITION_KEY = f.EXHIBITION_KEY
JOIN DIM_ARTIST a
  ON a.ARTIST_KEY = f.ARTIST_KEY
WHERE d.CALENDAR_YEAR BETWEEN 2022 AND 2024
GROUP BY
  ex.TITLE,
  d.CALENDAR_YEAR,
  a.NAME
ORDER BY
  d.CALENDAR_YEAR,
  ex.TITLE,
  total_insured_amount DESC;

SELECT plan_table_output
FROM table(dbms_xplan.display('plan_table', 'cerinta9_optim1', 'serial'));

-- Planul de executie asteptat:
-- - INDEX RANGE SCAN pe idx_fact_date_key (in loc de TABLE ACCESS FULL)
-- - TABLE ACCESS BY INDEX ROWID pe FACT_EXHIBITION_ACTIVITY
-- - Restul planului ramane similar (HASH JOIN-uri cu dimensiunile)
-- - Costul total scade deoarece se acceseaza doar randurile din intervalul
--   de date relevant, nu intreaga tabela de fapte


-- ---------------------------------------------------------------------------
-- OPTIMIZARE 2: Vizualizare materializata cu ENABLE QUERY REWRITE
-- ---------------------------------------------------------------------------
-- Justificare: pre-calculam agregatele (SUM, COUNT, AVG) la nivel de
-- (EXHIBITION_KEY, ARTIST_KEY, CALENDAR_YEAR), eliminand necesitatea
-- JOIN-urilor si GROUP BY la fiecare executie.

--ALTER SESSION SET QUERY_REWRITE_INTEGRITY = STALE_TOLERATED;

DROP MATERIALIZED VIEW mv_exhibition_artist_year;

CREATE MATERIALIZED VIEW mv_exhibition_artist_year
  BUILD IMMEDIATE
  REFRESH COMPLETE ON DEMAND
  ENABLE QUERY REWRITE
AS
SELECT
  ex.TITLE                          AS exhibition_title,
  d.CALENDAR_YEAR,
  a.NAME                            AS artist_name,
  SUM(f.ESTIMATED_VALUE)            AS total_estimated_value,
  SUM(f.INSURED_AMOUNT)             AS total_insured_amount,
  COUNT(f.ARTWORK_KEY)              AS artwork_count,
  SUM(f.AVG_RATING)                 AS sum_rating,
  COUNT(f.AVG_RATING)               AS cnt_rating
FROM FACT_EXHIBITION_ACTIVITY f
JOIN DIM_EXHIBITION ex ON ex.EXHIBITION_KEY = f.EXHIBITION_KEY
JOIN DIM_ARTIST a      ON a.ARTIST_KEY      = f.ARTIST_KEY
JOIN DIM_DATE d        ON d.DATE_KEY        = f.DATE_KEY
GROUP BY
  ex.TITLE,
  d.CALENDAR_YEAR,
  a.NAME;

-- Colectare statistici pentru vizualizarea materializata
ANALYZE TABLE mv_exhibition_artist_year COMPUTE STATISTICS;

-- Generarea planului de executie - cererea originala ar trebui rescrisa
-- automat de optimizor pentru a folosi vizualizarea materializata
EXPLAIN PLAN
SET STATEMENT_ID = 'cerinta9_optim2_rewrite'
FOR
SELECT
  ex.TITLE                          AS exhibition_title,
  d.CALENDAR_YEAR,
  a.NAME                            AS artist_name,
  SUM(f.ESTIMATED_VALUE)            AS total_estimated_value,
  SUM(f.INSURED_AMOUNT)             AS total_insured_amount,
  COUNT(DISTINCT f.ARTWORK_KEY)     AS artwork_count,
  AVG(f.AVG_RATING)                 AS avg_rating
FROM FACT_EXHIBITION_ACTIVITY f
JOIN DIM_EXHIBITION ex
  ON ex.EXHIBITION_KEY = f.EXHIBITION_KEY
JOIN DIM_ARTIST a
  ON a.ARTIST_KEY = f.ARTIST_KEY
JOIN DIM_DATE d
  ON d.DATE_KEY = f.DATE_KEY
WHERE d.CALENDAR_YEAR BETWEEN 2022 AND 2024
GROUP BY
  ex.TITLE,
  d.CALENDAR_YEAR,
  a.NAME
ORDER BY
  d.CALENDAR_YEAR,
  ex.TITLE,
  total_insured_amount DESC;

SELECT plan_table_output
FROM table(dbms_xplan.display('plan_table', 'cerinta9_optim2_rewrite', 'serial'));

-- Planul de executie asteptat (cu rescriere):
-- - TABLE ACCESS FULL pe MV_EXHIBITION_ARTIST_YEAR (vizualizarea materializata)
-- - FILTER pe CALENDAR_YEAR BETWEEN 2022 AND 2024
-- - SORT ORDER BY (pentru ordonarea finala)
--
-- Avantaj major: se elimina complet cele 3 JOIN-uri si operatia de GROUP BY,
-- deoarece datele sunt deja pre-agregate in vizualizarea materializata.
-- Costul scade semnificativ.

-- Verificare ca rescrierea a fost utilizata (fara NOREWRITE)
-- vs. planul cu hint NOREWRITE pentru comparatie:
EXPLAIN PLAN
SET STATEMENT_ID = 'cerinta9_optim2_norewrite'
FOR
SELECT /*+ NOREWRITE */
  ex.TITLE                          AS exhibition_title,
  d.CALENDAR_YEAR,
  a.NAME                            AS artist_name,
  SUM(f.ESTIMATED_VALUE)            AS total_estimated_value,
  SUM(f.INSURED_AMOUNT)             AS total_insured_amount,
  COUNT(DISTINCT f.ARTWORK_KEY)     AS artwork_count,
  AVG(f.AVG_RATING)                 AS avg_rating
FROM FACT_EXHIBITION_ACTIVITY f
JOIN DIM_EXHIBITION ex
  ON ex.EXHIBITION_KEY = f.EXHIBITION_KEY
JOIN DIM_ARTIST a
  ON a.ARTIST_KEY = f.ARTIST_KEY
JOIN DIM_DATE d
  ON d.DATE_KEY = f.DATE_KEY
WHERE d.CALENDAR_YEAR BETWEEN 2022 AND 2024
GROUP BY
  ex.TITLE,
  d.CALENDAR_YEAR,
  a.NAME
ORDER BY
  d.CALENDAR_YEAR,
  ex.TITLE,
  total_insured_amount DESC;

SELECT plan_table_output
FROM table(dbms_xplan.display('plan_table', 'cerinta9_optim2_norewrite', 'serial'));