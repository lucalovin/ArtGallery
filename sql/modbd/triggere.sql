-- validare vizitator pentru recenzii
-- Se execută pe ARTGALLERY_AM (similar și pe EU, schimbând numele tabelului)
CREATE OR REPLACE TRIGGER TRG_FK_REVIEW_VISITOR_GLOBAL
BEFORE INSERT OR UPDATE OF visitor_id ON GALLERY_REVIEW_AM
FOR EACH ROW
DECLARE
    v_exists NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_exists FROM VISITOR@link_global 
    WHERE visitor_id = :NEW.visitor_id;
    
    IF v_exists = 0 THEN
        RAISE_APPLICATION_ERROR(-20002, 'Integritate încălcată: Vizitatorul ' || :NEW.visitor_id || ' nu există pe serverul Global.');
    END IF;
END;
/

-- validare locatie pentru ARTWORK_DETAILS (AM)
-- Se execută pe ARTGALLERY_AM
CREATE OR REPLACE TRIGGER TRG_FK_ARTWORK_LOCATION_GLOBAL
BEFORE INSERT OR UPDATE OF location_id ON ARTWORK_DETAILS
FOR EACH ROW
DECLARE
    v_exists NUMBER;
BEGIN
    IF :NEW.location_id IS NOT NULL THEN
        SELECT COUNT(*) INTO v_exists FROM LOCATION@link_global 
        WHERE location_id = :NEW.location_id;
        
        IF v_exists = 0 THEN
            RAISE_APPLICATION_ERROR(-20001, 'Integritate încălcată: Locația ' || :NEW.location_id || ' nu există pe serverul Global.');
        END IF;
    END IF;
END;
/

--sincronizare fragmente Verticale (Artwork)
-- Exemplu pentru ștergere în cascadă (executat pe ARTGALLERY_EU)
CREATE OR REPLACE TRIGGER TRG_VFRAG_ARTWORK_DELETE
AFTER DELETE ON ARTWORK_CORE
FOR EACH ROW
BEGIN
    DELETE FROM ARTWORK_DETAILS@link_am WHERE artwork_id = :OLD.artwork_id;
END;
/