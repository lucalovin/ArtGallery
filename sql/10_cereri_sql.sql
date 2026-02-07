-- USER: ART_GALLETY_DW

-- ============================================================================
-- Module 1 & 2, Requirement 10: Five Natural Language Analytical Queries
-- Implemented in DwAnalyticsService.cs and accessible via AnalyticsController
-- ============================================================================

-- 1. Top 10 Arti?ti dup? num?r de opere expuse
-- Cerin?? în limbaj natural:
-- "Afi?a?i primii 10 arti?ti în func?ie de num?rul de opere din colec?ie, 
--  împreun? cu valoarea total? ?i medie estimat? a operelor lor."

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


-- 2. Valoarea colec?iei pe tip de mediu ?i colec?ie
-- Cerin?? în limbaj natural:
-- "Prezenta?i o analiz? a valorii totale a colec?iei descompus? pe tipuri de mediu 
--  (pictur?, sculptur?, etc.) ?i pe colec?ii, incluzând num?rul de opere ?i 
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


-- 3. Activitatea lunar? a expozi?iilor (ultimele 12 luni)
-- Cerin?? în limbaj natural:
-- "Analiza?i performan?a expozi?iilor lunar pentru ultimul an: afi?a?i pentru fiecare lun? 
--  num?rul de expozi?ii active, num?rul de opere expuse, valoarea total? a operelor 
--  ?i evaluarea medie primit? de vizitatori."

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


-- 4. Distribu?ia operelor pe loca?ii ?i galerii
-- Cerin?? în limbaj natural:
-- "Prezenta?i distribu?ia operelor de art? în diferitele loca?ii ale galeriei: 
--  pentru fiecare loca?ie ?i sal? afi?a?i num?rul de opere, valoarea total? 
--  ?i procentul pe care îl reprezint? din întreaga colec?ie expus?."


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


-- 5. Trendul anual al activit??ii expozi?ionale (ultimii 5 ani)
-- Cerin?? în limbaj natural:
-- "Afi?a?i evolu?ia activit??ii expozi?ionale pe ultimii 5 ani: pentru fiecare an 
--  prezenta?i num?rul de expozi?ii organizate, num?rul de opere expuse, valoarea 
--  total? ?i medie a operelor, precum ?i rata de cre?tere an-la-an (YoY) a valorii."
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