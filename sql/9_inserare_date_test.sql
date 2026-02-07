-- USER: ART_GALLETY_OLTP

-- ============================================================================
-- 1. DIM_DATE - 10,000 de randuri
-- Incepem de la 2000-01-01 pentru a avea un istoric bogat
-- ============================================================================
DECLARE
  v_date DATE;
  v_date_key NUMBER(8);
BEGIN
  -- Generam 10,000 de zile consecutive (aprox 27 ani de date)
  FOR i IN 0..9999 LOOP
    v_date := DATE '2000-01-01' + i; 
    v_date_key := TO_NUMBER(TO_CHAR(v_date, 'YYYYMMDD'));
    
    BEGIN
      INSERT INTO DIM_DATE (
        DATE_KEY, CALENDAR_DATE, CALENDAR_YEAR, CALENDAR_MONTH,
        CALENDAR_DAY, MONTH_NAME, QUARTER, IS_WEEKEND
      ) VALUES (
        v_date_key, v_date,
        EXTRACT(YEAR FROM v_date),
        EXTRACT(MONTH FROM v_date),
        EXTRACT(DAY FROM v_date),
        TO_CHAR(v_date, 'MONTH'),
        TO_NUMBER(TO_CHAR(v_date, 'Q')),
        CASE WHEN TO_CHAR(v_date, 'DY', 'NLS_DATE_LANGUAGE=ENGLISH') IN ('SAT','SUN') THEN 'Y' ELSE 'N' END
      );
    EXCEPTION WHEN DUP_VAL_ON_INDEX THEN NULL;
    END;
  END LOOP;
  COMMIT;
END;
/

-- ============================================================================
-- 2. DIM_ARTIST - 10,000 de randuri
-- ============================================================================
DECLARE
  TYPE t_names IS VARRAY(50) OF VARCHAR2(64);
  v_first_names t_names := t_names('Pablo', 'Claude', 'Vincent', 'Leonardo', 'Michelangelo', 'Rembrandt', 'Salvador', 'Frida', 'Andy', 'Jackson', 'Georgia', 'Henri', 'Edvard', 'Gustav', 'Wassily', 'Marc', 'Paul', 'Edgar', 'Pierre', 'Amedeo', 'Rene', 'Joan', 'Alberto', 'Constantin', 'Francis', 'Lucian', 'David', 'Yayoi', 'Banksy', 'Marina', 'Ai', 'Damien', 'Jeff', 'Takashi', 'Gerhard', 'Cindy', 'Jean-Michel', 'Keith', 'Roy', 'Jasper', 'Robert', 'Mark', 'Willem', 'Alexander', 'Egon', 'Artemisia', 'Caravaggio', 'Jan', 'Diego', 'Francisco');
  v_last_names t_names := t_names('Picasso', 'Monet', 'van Gogh', 'da Vinci', 'Buonarroti', 'van Rijn', 'Dali', 'Kahlo', 'Warhol', 'Pollock', 'OKeeffe', 'Matisse', 'Munch', 'Klimt', 'Kandinsky', 'Chagall', 'Cezanne', 'Degas', 'Renoir', 'Modigliani', 'Magritte', 'Miro', 'Giacometti', 'Brancusi', 'Bacon', 'Freud', 'Hockney', 'Kusama', 'Unknown', 'Abramovic', 'Weiwei', 'Hirst', 'Koons', 'Murakami', 'Richter', 'Sherman', 'Basquiat', 'Haring', 'Lichtenstein', 'Johns', 'Rauschenberg', 'Rothko', 'de Kooning', 'Calder', 'Schiele', 'Gentileschi', 'Merisi', 'Vermeer', 'Velazquez', 'Goya');
BEGIN
  FOR i IN 1..10000 LOOP
    BEGIN
      INSERT INTO DIM_ARTIST (ARTIST_KEY, ARTIST_ID_OLTP, NAME, NATIONALITY, BIRTH_YEAR, DEATH_YEAR)
      VALUES (
        i, i,
        v_first_names(MOD(i, 50) + 1) || ' ' || v_last_names(MOD(i + 15, 50) + 1) || ' (ID:' || i || ')',
        'Nation ' || (MOD(i, 20) + 1),
        1850 + MOD(i, 150),
        CASE WHEN MOD(i, 5) = 0 THEN NULL ELSE 1950 + MOD(i, 70) END
      );
    EXCEPTION WHEN DUP_VAL_ON_INDEX THEN NULL;
    END;
    IF MOD(i, 1000) = 0 THEN COMMIT; END IF;
  END LOOP;
  COMMIT;
END;
/

-- ============================================================================
-- 3. DIM_EXHIBITION - 10,000 de randuri
-- ============================================================================
DECLARE
  v_start_d DATE;
  v_end_d DATE;
BEGIN
  FOR i IN 1..10000 LOOP
    v_start_d := DATE '2020-01-01' + MOD(i, 1500); -- Incepand cu 2020
    v_end_d   := v_start_d + 15 + MOD(i, 60);
    
    BEGIN
      INSERT INTO DIM_EXHIBITION (EXHIBITION_KEY, EXHIBITION_ID_OLTP, TITLE, START_DATE_KEY, END_DATE_KEY, DESCRIPTION)
      VALUES (
        i, i,
        'Exhibition #' || i,
        TO_NUMBER(TO_CHAR(v_start_d, 'YYYYMMDD')),
        TO_NUMBER(TO_CHAR(v_end_d, 'YYYYMMDD')),
        'Descriere detaliata pentru expozitia cu numarul ' || i
      );
    EXCEPTION WHEN DUP_VAL_ON_INDEX THEN NULL;
    END;
    IF MOD(i, 1000) = 0 THEN COMMIT; END IF;
  END LOOP;
  COMMIT;
END;
/

-- ============================================================================
-- 4. FACT_EXHIBITION_ACTIVITY - 10,000 de randuri
-- ============================================================================
DECLARE
  TYPE t_date_keys IS TABLE OF NUMBER(8) INDEX BY BINARY_INTEGER;
  v_date_keys   t_date_keys;
  v_current_dk  NUMBER(8);
  v_est_value   NUMBER;
  v_idx         NUMBER := 1;
BEGIN
  -- 1. Colect?m cheile de dat? existente
  FOR r IN (SELECT DATE_KEY FROM DIM_DATE WHERE ROWNUM <= 5000) LOOP
    v_date_keys(v_idx) := r.DATE_KEY;
    v_idx := v_idx + 1;
  END LOOP;

  IF v_date_keys.COUNT = 0 THEN
     DBMS_OUTPUT.PUT_LINE('Eroare: DIM_DATE este goal?!');
     RETURN;
  END IF;

  -- 2. Inserare 10.000 de rânduri
  FOR i IN 1..10000 LOOP
    v_current_dk := v_date_keys(MOD(i, v_date_keys.COUNT) + 1);
    v_est_value  := 5000 + MOD(i * 123, 1000000);

    BEGIN
      INSERT INTO FACT_EXHIBITION_ACTIVITY (
        FACT_KEY, 
        DATE_KEY, 
        EXHIBITION_KEY, 
        EXHIBITOR_KEY,   -- Ad?ugat pentru a evita ORA-01400
        ARTWORK_KEY, 
        ARTIST_KEY, 
        COLLECTION_KEY,  -- Ad?ugat (valoare dummy)
        LOCATION_KEY,    -- Ad?ugat (valoare dummy)
        POLICY_KEY,      -- Ad?ugat (valoare dummy)
        ESTIMATED_VALUE, 
        INSURED_AMOUNT, 
        LOAN_FLAG, 
        RESTORATION_COUNT, 
        REVIEW_COUNT, 
        AVG_RATING
      ) VALUES (
        i,
        v_current_dk,
        MOD(i, 10000) + 1,
        MOD(i, 50) + 1,      -- EXHIBITOR_KEY dummy
        i,                   -- ARTWORK_KEY unic
        MOD(i * 3, 10000) + 1,
        MOD(i, 30) + 1,      -- COLLECTION_KEY dummy
        MOD(i, 20) + 1,      -- LOCATION_KEY dummy
        MOD(i, 10) + 1,      -- POLICY_KEY dummy
        v_est_value,
        v_est_value * 1.15,
        MOD(i, 2),
        MOD(i, 4),
        MOD(i, 150),
        ROUND(1 + (MOD(i, 40)/10), 2)
      );
    EXCEPTION 
      WHEN DUP_VAL_ON_INDEX THEN NULL;
      WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('Eroare la randul ' || i || ': ' || SQLERRM);
        RAISE;
    END;

    IF MOD(i, 1000) = 0 THEN 
      COMMIT; 
    END IF;
  END LOOP;

  COMMIT;
  DBMS_OUTPUT.PUT_LINE('Succes: 10.000 de randuri populate în FACT_EXHIBITION_ACTIVITY.');
END;
/