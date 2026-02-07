-- USER: ART_GALLETY_DW

-- ============================================================================
-- Module 1 & 2, Requirement 10: Five Natural Language Analytical Queries
-- Implemented in DwAnalyticsService.cs and accessible via AnalyticsController
-- ============================================================================

-- 1. Top 10 Artisti dupa numar de opere expuse
-- Cerinta in limbaj natural:
-- "Afisati primii 10 artisti in functie de numarul de opere din colectie, 
--  impreuna cu valoarea totala si medie estimata a operelor lor."

SELECT * FROM (
    SELECT
        da.NAME as artist_name,
        COUNT(DISTINCT daw.ARTWORK_KEY) as artwork_count,
        SUM(NVL(f.ESTIMATED_VALUE, 0)) as total_value,
        AVG(NVL(f.ESTIMATED_VALUE, 0)) as average_value
    FROM ART_GALLERY_DW.FACT_EXHIBITION_ACTIVITY f
    JOIN ART_GALLERY_DW.DIM_ARTIST da ON f.ARTIST_KEY = da.ARTIST_KEY
    JOIN ART_GALLERY_DW.DIM_ARTWORK daw ON f.ARTWORK_KEY = daw.ARTWORK_KEY
    GROUP BY da.NAME
    ORDER BY artwork_count DESC
)
WHERE ROWNUM <= 10;


-- 2. Valoarea colectiei pe tip de mediu si colectie
-- Cerinta in limbaj natural:
-- "Prezentati o analiza a valorii totale a colectiei descompusa pe tipuri de mediu 
--  (pictura, sculptura, etc.) si pe colectii, incluzand numarul de opere si 
--  valoarea medie pe fiecare categorie."

SELECT
    NVL(daw.MEDIUM, 'Unknown') as medium_type,
    NVL(dc.NAME, 'Unknown') as collection_name,
    COUNT(DISTINCT f.ARTWORK_KEY) as artwork_count,
    SUM(NVL(f.ESTIMATED_VALUE, 0)) as total_value,
    AVG(NVL(f.ESTIMATED_VALUE, 0)) as average_value
FROM ART_GALLERY_DW.FACT_EXHIBITION_ACTIVITY f
JOIN ART_GALLERY_DW.DIM_ARTWORK daw ON f.ARTWORK_KEY = daw.ARTWORK_KEY
LEFT JOIN ART_GALLERY_DW.DIM_COLLECTION dc ON f.COLLECTION_KEY = dc.COLLECTION_KEY
GROUP BY daw.MEDIUM, dc.NAME
ORDER BY total_value DESC;


-- 3. Activitatea lunara a expozitiilor (ultimele 12 luni)
-- Cerinta in limbaj natural:
-- "Analizati performanta expozitiilor lunar pentru ultimul an: afisati pentru fiecare luna 
--  numarul de expozitii active, numarul de opere expuse, valoarea totala a operelor 
--  si evaluarea medie primita de vizitatori."

SELECT
    dd.MONTH_NAME as month_name,
    dd.CALENDAR_YEAR as year,
    COUNT(DISTINCT f.EXHIBITION_KEY) as exhibition_count,
    COUNT(DISTINCT f.ARTWORK_KEY) as artworks_exhibited,
    SUM(NVL(f.ESTIMATED_VALUE, 0)) as total_artwork_value,
    AVG(f.AVG_RATING) as average_rating
FROM ART_GALLERY_DW.FACT_EXHIBITION_ACTIVITY f
JOIN ART_GALLERY_DW.DIM_DATE dd ON f.DATE_KEY = dd.DATE_KEY
WHERE dd.CALENDAR_DATE >= ADD_MONTHS(SYSDATE, -12)
GROUP BY dd.CALENDAR_YEAR, dd.MONTH_NAME, dd.CALENDAR_MONTH
ORDER BY dd.CALENDAR_YEAR, dd.CALENDAR_MONTH;


-- 4. Distributia operelor pe locatii si galerii
-- Cerinta in limbaj natural:
-- "Prezentati distributia operelor de arta in diferitele locatii ale galeriei: 
--  pentru fiecare locatie si sala afisati numarul de opere, valoarea totala 
--  si procentul pe care il reprezinta din intreaga colectie expusa."


SELECT
    NVL(dl.NAME, 'Unknown') as location_name,
    NVL(dl.GALLERY_ROOM, 'N/A') as gallery_room,
    NVL(dl.TYPE, 'Unknown') as location_type,
    COUNT(DISTINCT f.ARTWORK_KEY) as artworks_count,
    SUM(NVL(f.ESTIMATED_VALUE, 0)) as total_value,
    ROUND(COUNT(DISTINCT f.ARTWORK_KEY) * 100.0 / 
        NULLIF(SUM(COUNT(DISTINCT f.ARTWORK_KEY)) OVER(), 0), 2) as percentage
FROM ART_GALLERY_DW.FACT_EXHIBITION_ACTIVITY f
JOIN ART_GALLERY_DW.DIM_LOCATION dl ON f.LOCATION_KEY = dl.LOCATION_KEY
GROUP BY dl.NAME, dl.GALLERY_ROOM, dl.TYPE
ORDER BY artworks_count DESC;


-- 5. Trendul anual al activitatii expozitionale (ultimii 5 ani)
-- Cerinta in limbaj natural:
-- "Afisati evolutia activitatii expozitionale pe ultimii 5 ani: pentru fiecare an 
--  prezentati numarul de expozitii organizate, numarul de opere expuse, valoarea 
--  totala si medie a operelor, precum si rata de crestere an-la-an (YoY) a valorii."
--
SELECT
    dd.CALENDAR_YEAR as year,
    COUNT(DISTINCT f.EXHIBITION_KEY) as exhibitions_count,
    COUNT(DISTINCT f.ARTWORK_KEY) as artworks_count,
    SUM(NVL(f.ESTIMATED_VALUE, 0)) as total_artwork_value,
    AVG(NVL(f.ESTIMATED_VALUE, 0)) as average_artwork_value,
    LAG(SUM(NVL(f.ESTIMATED_VALUE, 0))) OVER (ORDER BY dd.CALENDAR_YEAR) as previous_year_value,
    CASE 
        WHEN LAG(SUM(NVL(f.ESTIMATED_VALUE, 0))) OVER (ORDER BY dd.CALENDAR_YEAR) IS NOT NULL 
             AND LAG(SUM(NVL(f.ESTIMATED_VALUE, 0))) OVER (ORDER BY dd.CALENDAR_YEAR) > 0
        THEN ROUND(
            ((SUM(NVL(f.ESTIMATED_VALUE, 0)) - 
              LAG(SUM(NVL(f.ESTIMATED_VALUE, 0))) OVER (ORDER BY dd.CALENDAR_YEAR)) * 100.0 / 
              LAG(SUM(NVL(f.ESTIMATED_VALUE, 0))) OVER (ORDER BY dd.CALENDAR_YEAR)), 2)
        ELSE NULL
    END as yoy_growth_rate
FROM ART_GALLERY_DW.FACT_EXHIBITION_ACTIVITY f
JOIN ART_GALLERY_DW.DIM_DATE dd ON f.DATE_KEY = dd.DATE_KEY
WHERE dd.CALENDAR_YEAR >= EXTRACT(YEAR FROM SYSDATE) - 5
GROUP BY dd.CALENDAR_YEAR
ORDER BY dd.CALENDAR_YEAR;