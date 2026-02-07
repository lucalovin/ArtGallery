-- USER: ART_GALLETY_OLTP

-- ============================================================================
-- 9a. PLANUL DE EXECU?IE ALES DE OPTIMIZORUL BAZAT PE COST (CBO)
--    (varianta de baz?, f?r? optimiz?ri explicite)
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

-- Pas 1: Generarea planului de execu?ie pentru cererea de baz?
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

-- Pas 2: Afi?area planului de execu?ie
SELECT plan_table_output
FROM table(dbms_xplan.display('plan_table', 'cerinta9_baza', 'serial'));

-- ============================================================================
-- EXPLICAREA ETAPELOR PLANULUI DE EXECU?IE (estimat):
-- ============================================================================
--
-- Planul de execu?ie generat de CBO va parcurge, în mod tipic, urm?toarele
-- etape (citite de la cea mai indentat? opera?ie spre r?d?cin?):
--
-- 1. TABLE ACCESS FULL pe FACT_EXHIBITION_ACTIVITY
--    - Optimizorul scaneaz? integral tabela de fapte (full table scan)
--    - Aceasta este cea mai costisitoare opera?ie, deoarece tabela de
--      fapte con?ine cel mai mare volum de date
--
-- 2. TABLE ACCESS FULL pe DIM_DATE
--    - Scaneaz? tabela DIM_DATE pentru a putea aplica filtrul
--      CALENDAR_YEAR BETWEEN 2022 AND 2024
--
-- 3. HASH JOIN între FACT_EXHIBITION_ACTIVITY ?i DIM_DATE
--    - Optimizorul construie?te un hash table pe dimensiunea mai mic?
--      (DIM_DATE filtrat?) ?i face probe cu rândurile din tabela de fapte
--    - Filtrul pe CALENDAR_YEAR este aplicat aici, reducând setul de date
--
-- 4. TABLE ACCESS FULL pe DIM_EXHIBITION
--    - Scaneaz? tabela dimensiune DIM_EXHIBITION
--
-- 5. HASH JOIN cu rezultatul anterior ?i DIM_EXHIBITION
--    - Face leg?tura pe EXHIBITION_KEY
--
-- 6. TABLE ACCESS FULL pe DIM_ARTIST
--    - Scaneaz? tabela dimensiune DIM_ARTIST
--
-- 7. HASH JOIN cu rezultatul anterior ?i DIM_ARTIST
--    - Face leg?tura pe ARTIST_KEY
--
-- 8. HASH GROUP BY
--    - Grupeaz? rezultatele pe (TITLE, CALENDAR_YEAR, NAME) ?i
--      calculeaz? SUM, COUNT DISTINCT, AVG
--
-- 9. SORT ORDER BY
--    - Ordoneaz? rezultatul final dup? CALENDAR_YEAR, TITLE,
--      total_insured_amount DESC
--
-- Observa?ii:
-- - Toate accesele la tabele sunt de tip FULL TABLE SCAN deoarece
--   nu exist? indec?i pe coloanele de JOIN ale tabelei de fapte
-- - Costul principal provine din scanarea integral? a tabelei de fapte
-- - Hash Join este preferat de CBO pentru volume mari de date (tipic DW)
-- ============================================================================


-- ============================================================================
-- 9b. SUGESTII DE OPTIMIZARE A CERERII
-- ============================================================================

-- ---------------------------------------------------------------------------
-- OPTIMIZARE 1: Index B*Tree pe coloana DATE_KEY din tabela de fapte
-- ---------------------------------------------------------------------------
-- Justificare: cererea filtreaz? pe un interval de ani, ceea ce se traduce
-- într-un interval de valori DATE_KEY (20220101 - 20241231)
-- Un index B*Tree permite range scan în loc de full table scan.

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

-- Planul de execu?ie a?teptat:
-- - INDEX RANGE SCAN pe idx_fact_date_key (în loc de TABLE ACCESS FULL)
-- - TABLE ACCESS BY INDEX ROWID pe FACT_EXHIBITION_ACTIVITY
-- - Restul planului r?mâne similar (HASH JOIN-uri cu dimensiunile)
-- - Costul total scade deoarece se acceseaz? doar rândurile din intervalul
--   de date relevant, nu întreaga tabel? de fapte


-- ---------------------------------------------------------------------------
-- OPTIMIZARE 2: Vizualizare materializat? cu ENABLE QUERY REWRITE
-- ---------------------------------------------------------------------------
-- Justificare: pre-calcul?m agregatele (SUM, COUNT, AVG) la nivel de
-- (EXHIBITION_KEY, ARTIST_KEY, CALENDAR_YEAR), eliminând necesitatea
-- JOIN-urilor ?i GROUP BY la fiecare execu?ie.

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

-- Colectare statistici pentru vizualizarea materializat?
ANALYZE TABLE mv_exhibition_artist_year COMPUTE STATISTICS;

-- Generarea planului de execu?ie - cererea original? ar trebui rescris?
-- automat de optimizor pentru a folosi vizualizarea materializat?
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

-- Planul de execu?ie a?teptat (cu rescriere):
-- - TABLE ACCESS FULL pe MV_EXHIBITION_ARTIST_YEAR (vizualizarea materializat?)
-- - FILTER pe CALENDAR_YEAR BETWEEN 2022 AND 2024
-- - SORT ORDER BY (pentru ordonarea final?)
--
-- Avantaj major: se elimin? complet cele 3 JOIN-uri ?i opera?ia de GROUP BY,
-- deoarece datele sunt deja pre-agregate în vizualizarea materializat?.
-- Costul scade semnificativ.

-- Verificare c? rescrierea a fost utilizat? (f?r? NOREWRITE)
-- vs. planul cu hint NOREWRITE pentru compara?ie:
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